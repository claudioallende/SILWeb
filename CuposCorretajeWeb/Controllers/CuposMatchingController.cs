using CuposCorretajeWeb.Models.Data;
using CuposCorretajeWeb.Models.Solicitudes;
using CuposCorretajeWeb.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CuposCorretajeWeb.Controllers
{
  /// <summary>
  /// Controller dedicado a la integración del matching (motor ACA_Matching)
  /// con el flujo de distribución de cupos del MVC legacy.
  ///
  /// No modifica ninguna acción del <see cref="CuposController"/> legacy: este
  /// controller es **opt-in** y se dispara desde el botón "Buscar" de
  /// <c>Views\Cupos\Distribucion.cshtml</c> cuando el operador quiere ver
  /// las solicitudes compatibles con los cupos que está distribuyendo.
  ///
  /// Decisión de implementación (iteración 1):
  /// <list type="number">
  ///   <item>Para los cupos disponibles por filtros se usa el endpoint ya conocido
  ///         <c>POST /api/CuposDisponibles/Disponibles</c> (mismo que consume
  ///         <c>SolicitudesController.AltaSolicitud</c>). Devuelve agregaciones
  ///         por (vendedor+comprador+grano+zona) sin Id de cupo individual.</item>
  ///   <item>Para las solicitudes pendientes se usa
  ///         <c>POST /api/ShiftRequest/GetAllPendingShiftRequestAsync</c> y se
  ///         hace una clasificación heurística del lado del MVC
  ///         (no del motor real) ya que la agregación de cupos no expone
  ///         un Id de cupo para invocar <c>MatchesPorCupo/{id}</c>.</item>
  ///   <item>En una iteración posterior, cuando el backend exponga un endpoint
  ///         bulk "Matches con filtros" (señalado en MotorMatch.md §endpoint
  ///         bulk), la clasificación del MVC será reemplazada por el motor.</item>
  /// </list>
  /// </summary>
  //[Authorize]
  public class CuposMatchingController : Controller
  {
    private static readonly List<string> CentrosDefault = new List<string> { "ROS", "BSAS", "CBA" };

    /// <summary>
    /// POST /CuposMatching/BuscarCuposConMatch
    ///
    /// Recibe los filtros del form de distribución (Producto, Comprador, Puerto, Fechas)
    /// y devuelve los cupos disponibles con sus solicitudes compatibles ya clasificadas
    /// por el motor real (Directo / Parcial / Condicional).
    ///
    /// Internamente delega al endpoint bulk <c>POST /api/ShiftRequest/Matches</c>
    /// que ejecuta el <c>IMatchingEngine</c> del backend (chain of responsibility:
    /// Grano → Vendedor → Comprador → Destino → Clasificador). La respuesta bulk
    /// es una lista plana de pares (solicitudId, cupoId, MatchType); acá la
    /// agrupamos por cupoId para que la UI pueda renderizar Variante A/B.
    ///
    /// Si la combinación no devuelve pares, devuelve <c>cupos = []</c> (200 con array vacío).
    /// </summary>
    [HttpPost]
    [ValidateAjax]
    public async Task<JsonResult> BuscarCuposConMatch([System.Web.Http.FromBody] BuscarMatchRequestDto filter)
    {
      try
      {
        if (filter == null)
        {
          return Json(new { success = false, cupos = new List<object>(), message = "Filtro nulo." });
        }

        // El endpoint bulk exige CodigoGrano (es lo único obligatorio).
        if (filter.CodigoGrano <= 0)
        {
          return Json(new
          {
            success = false,
            cupos = new List<object>(),

            message = "El filtro requiere CodigoGrano (producto) para consultar el motor de matching."
          });
        }

        WebServiceSILRespository repo = new WebServiceSILRespository();
        Stopwatch sw = Stopwatch.StartNew();

        // Resolver la fecha exacta del cupo. El matching compara el día
        // del cupo con la FechaSolicitado de cada solicitud: si no
        // coinciden, no se muestra nada. Si el cliente no envía CupoId
        // (caso legacy), caemos al comportamiento anterior con la ventana
        // de hoy a hoy+14.
        DateTime? cupoFecha = null;
        if (filter.CupoId > 0)
        {
          try
          {
            var cupos = await repo.RequestPostAndDeserializeAsync<IList<Models.Cupos>>(
              "CuposData", "GetCupos", new { ids = new List<long> { filter.CupoId } });
            if (cupos != null && cupos.Count > 0 && cupos[0].Fecha.HasValue)
            {
              cupoFecha = cupos[0].Fecha.Value.Date;
            }
          }
          catch (Exception ex)
          {
            Trace.TraceWarning("BuscarCuposConMatch: no se pudo obtener la fecha del cupo " +
                                filter.CupoId + " — " + ex.Message);
          }
        }

        // 1) Construir el filtro que entiende el motor.
        //    Pasamos el destino del cupo (CuentaPuerto) y dejamos que el
        //    backend resuelva las zonas a las que pertenece (vía
        //    PUERTOPORZONA + ZONASGEOGRAFICAS). El frontend ya no necesita
        //    conocer la zona explícita.
        DateTime? fechaDesde = cupoFecha ?? (filter.Fecha == default ? (DateTime?)null : filter.Fecha);
        DateTime? fechaHasta = cupoFecha ?? (filter.Fecha == default ? (DateTime?)null : filter.Fecha.AddDays(14));

        MatchesFilterDto bulkFilter = new MatchesFilterDto
        {
          CodigoGrano = filter.CodigoGrano,
          // Opcionales: 0 / null = no discrimina, el motor lo trata como wildcard.
          CuentaVendedor = filter.CuentaVendedor > 0 ? (long?)filter.CuentaVendedor : null,
          CuentaComprador = filter.CuentaComprador > 0 ? (long?)filter.CuentaComprador : null,
          CuentaPuerto = filter.CuentaPuerto > 0 ? (long?)filter.CuentaPuerto : null,
          FechaDesde = fechaDesde,
          FechaHasta = fechaHasta,
          IncluirIncompatibles = false,
          AgruparPor = MatchesAgrupacion.Ninguno
        };

        // 2) Llamar al endpoint bulk específico de Distribución. Soporta
        // correctamente los casos parciales donde la solicitud no tiene
        // comprador o destino: el motor los marca como Parcial en vez de
        // ser descartados por el WHERE. Pantalla 2 (Solicitudes) sigue
        // usando el endpoint /Matches legacy, sin cambios.
        MatchesResultDto result;
        try
        {
          result = await repo.RequestSILDataPostAndDeserializeAsync<MatchesResultDto>(
            "ShiftRequest", "MatchesDistribucion", bulkFilter);
        }
        catch (Models.Error.ApiException apiEx)
        {
          Trace.TraceWarning("BuscarCuposConMatch 4xx: " + apiEx.Message);
          return Json(new
          {
            success = false,
            cupos = new List<object>(),
            status = apiEx.Message,
            message = "El motor de matching rechazó la consulta: " + apiEx.Message
          });
        }

        sw.Stop();
        Trace.TraceInformation(
          $"[Matching] BuscarCuposConMatch bulk filter=grano={filter.CodigoGrano} " +
          $"items={result?.Items?.Count ?? 0} directos={result?.Resumen?.MatchesDirectos ?? 0} " +
          $"parciales={result?.Resumen?.MatchesParciales ?? 0} cond={result?.Resumen?.MatchesCondicionales ?? 0} " +
          $"tiempo={sw.ElapsedMilliseconds}ms");

        // 3) Agrupar por cupoId (cada item es (solicitud, cupo)).
        //    Si el motor devolvió 0 items, devolvemos array vacío.
        List<CupoParaMatchViewModel> cuposViewModel = new List<CupoParaMatchViewModel>();

        if (result != null && result.Items != null && result.Items.Count > 0)
        {
          cuposViewModel = AgruparPorCupo(result.Items);
        }

        return Json(new
        {
          success = true,
          cupos = cuposViewModel,
          totalCupos = cuposViewModel.Count,
          totalMatches = cuposViewModel.Sum(c => c.Matches.Count),
          resumen = result?.Resumen,
          message = cuposViewModel.Count == 0
            ? "El motor no encontró matches compatibles con esos filtros."
            : null
        });
      }
      catch (Exception ex)
      {
        Trace.TraceError("BuscarCuposConMatch error: " + ex);
        return Json(new
        {
          success = false,
          cupos = new List<object>(),
          message = "Error al consultar el motor de matching: " + ex.Message
        });
      }
    }

    /// <summary>
    /// Agrupa los items (solicitud, cupo) que devuelve el motor por <c>cupoId</c>,
    /// para producir un <see cref="CupoParaMatchViewModel"/> por cada cupo único.
    /// La metadata del cupo se hidrata desde <c>MatchItemDto.Cupo</c>.
    /// </summary>
    private static List<CupoParaMatchViewModel> AgruparPorCupo(List<MatchItemDto> items)
    {
      var grupos = items
        .GroupBy(i => i.CupoId)
        .ToList();

      List<CupoParaMatchViewModel> resultado = new List<CupoParaMatchViewModel>();

      foreach (var g in grupos)
      {
        MatchItemDto first = g.First();
        MatchCupoResumenDto cupoBackend = first.Cupo ?? new MatchCupoResumenDto();

        // Sumamos cantidades para reportar cupos totales del cupo (no viene
        // directo en el response; usamos Cantidad de la primera solicitud
        // como heurística — el cupo real podría tener varios asociados).
        int totalCupos = g.Sum(i => i.Solicitud != null ? Math.Max(1, i.Solicitud.Cantidad) : 1);

        CupoParaMatchViewModel view = new CupoParaMatchViewModel
        {
          Id = g.Key,
          CodGrano = cupoBackend.CodGrano,
          NomGrano = cupoBackend.NombreGrano ?? cupoBackend.CodGrano,
          CodVendSIL = cupoBackend.CodVendSIL ?? string.Empty,
          NomVendSIL = cupoBackend.NombreVendedor ?? string.Empty,
          CodCompSIL = cupoBackend.CodCompSIL ?? string.Empty,
          NomCompSIL = cupoBackend.NombreComprador ?? string.Empty,
          CodDestino = cupoBackend.CodDestino ?? string.Empty,
          NomDestino = cupoBackend.NombreDestino ?? string.Empty,
          Fecha = cupoBackend.Fecha,
          CuposTotales = totalCupos
        };

        foreach (MatchItemDto item in g)
        {
          if (item.MatchType == null) continue; // Incompatible: el motor no lo incluye si IncluirIncompatibles=false, pero por las dudas.

          MatchSolicitudCompletaDto s = item.Solicitud ?? new MatchSolicitudCompletaDto();
          int antiguedad = s.FechaSolicitado == default
            ? 0
            : Math.Max(0, (int)(DateTime.Today - s.FechaSolicitado.Date).TotalDays);

          view.Matches.Add(new SolicitudParaMatchViewModel
          {
            Id = item.SolicitudId,
            Vendedor = s.CuentaVendedor.ToString(),
            Comprador = s.CuentaComprador.HasValue ? s.CuentaComprador.Value.ToString() : null,
            Destino = s.CuentaDestino.HasValue ? s.CuentaDestino.Value.ToString() : null,
            Zona = cupoBackend.NombreDestino,
            FechaSolicitado = s.FechaSolicitado,
            Cantidad = s.Cantidad,
            CantidadDisponible = s.CantidadDisponible,
            CantidadRechazada = s.CantidadRechazada,
            CantidadFuturo = 0,
            Observacion = s.Observacion,
            AntiguedadDias = antiguedad,
            MatchType = item.MatchType,
            MatchRazon = item.Razon,
            Dias = new List<MatchDiaItem>
            {
              new MatchDiaItem
              {
                Fecha = s.FechaSolicitado,
                Cantidad = s.CantidadDisponible
              }
            }
          });
        }

        resultado.Add(view);
      }

      return resultado;
    }

    /// <summary>
    /// GET /CuposMatching/ObtenerMatchPorCupo?cupoId=123
    ///
    /// Atajo: si el cliente ya conoce el cupoId, llama directo al endpoint
    /// <c>GET /api/ShiftRequest/MatchesPorCupo/{cupoId}</c> y devuelve la lista
    /// cruda de <see cref="MatchResultDto"/>.
    /// </summary>
    [HttpGet]
    public async Task<JsonResult> ObtenerMatchPorCupo(long cupoId)
    {
      try
      {
        if (cupoId <= 0)
        {
          return Json(new { success = false, matches = new List<MatchResultDto>(), message = "cupoId inválido." });
        }

        WebServiceSILRespository repo = new WebServiceSILRespository();
        IList<MatchResultDto> matches = await repo.RequestSILDataGetAndDeserializeAsync<IList<MatchResultDto>>(
          "ShiftRequest",
          $"MatchesPorCupo/{cupoId}")
          ?? new List<MatchResultDto>();

        return Json(new
        {
          success = true,
          matches = matches,
          totalMatches = matches.Count,
          message = matches.Count == 0 ? "Sin solicitudes compatibles para este cupo." : null
        });
      }
      catch (Exception ex)
      {
        Trace.TraceError("ObtenerMatchPorCupo({cupoId}) error: " + ex);
        return Json(new
        {
          success = false,
          matches = new List<MatchResultDto>(),
          message = "Error al obtener matches: " + ex.Message
        });
      }
    }

    /// <summary>
    /// POST /CuposMatching/AceptarMatch
    ///
    /// Proxy del MVC al endpoint <c>POST /api/ShiftRequest/Accept</c>.
    /// Si el backend devuelve 409 (todos fallaron por conflicto de concurrencia),
    /// traduce a <c>success=false</c> con mensaje claro para que el modal muestre
    /// el diálogo de conflicto.
    /// </summary>
    [HttpPost]
    [ValidateAjax]
    public async Task<JsonResult> AceptarMatch([System.Web.Http.FromBody] ShiftRequestAcceptDataViewModel data)
    {
      try
      {
        if (data == null || data.ShiftRequest == null || data.ShiftRequest.Count == 0
            || data.CuposToBeDistributed == null || data.CuposToBeDistributed.Count == 0)
        {
          return Json(new
          {
            success = false,
            message = "Datos de Accept inválidos (ShiftRequest y CuposToBeDistributed requeridos)."
          });
        }

        int totalSolic = data.ShiftRequest.Sum(r => r.Cantidad > 0 ? r.Cantidad : 1);
        if (totalSolic != data.CuposToBeDistributed.Count)
        {
          return Json(new
          {
            success = false,
            message = $"Cantidad solicitada ({totalSolic}) no coincide con la cantidad de cupos ({data.CuposToBeDistributed.Count})."
          });
        }

        WebServiceSILRespository repo = new WebServiceSILRespository();
        ShiftRequestAcceptResultViewModel result;
        try
        {
          result = await repo.RequestSILDataPostAndDeserializeAsync<ShiftRequestAcceptResultViewModel>(
            "ShiftRequest", "Accept", data);
        }
        catch (Models.Error.ApiException apiEx)
        {
          // 409 desde el backend → todos fallaron (otro operador ya asignó).
          Trace.TraceWarning("AceptarMatch 409: " + apiEx.Message);
          return Json(new
          {
            success = false,
            status = 409,
            message = "Conflicto: la solicitud ya fue procesada por otro operador.",
            detalle = apiEx.Message
          });
        }

        if (result == null)
        {
          return Json(new { success = false, message = "El backend devolvió una respuesta vacía." });
        }

        // Si el backend devolvió 2xx pero TODOS fallaron por algún motivo
        // (caso raro: 200 con Fallos.Count == ShiftRequest.Count),我们也 devolvemos 409.
        if (result.TodosFallaron)
        {
          return Json(new
          {
            success = false,
            status = 409,
            message = "Ninguna solicitud pudo asignarse: todas fueron procesadas previamente por otro operador.",
            asignados = result.Asignados,
            fallos = result.Fallos
          });
        }

        return Json(new
        {
          success = true,
          message = $"Asignación realizada: {result.TotalAsignadas} OK, {result.TotalConflictos} con conflicto.",
          totalAsignadas = result.TotalAsignadas,
          totalConflictos = result.TotalConflictos,
          asignados = result.Asignados,
          fallos = result.Fallos
        });
      }
      catch (Exception ex)
      {
        Trace.TraceError("AceptarMatch error: " + ex);
        return Json(new { success = false, message = "Error al aceptar matches: " + ex.Message });
      }
    }

    /// <summary>
    /// POST /CuposMatching/RechazarMatch
    ///
    /// Proxy del MVC al endpoint <c>POST /api/ShiftRequest/Reject</c>.
    /// </summary>
    [HttpPost]
    [ValidateAjax]
    public async Task<JsonResult> RechazarMatch([System.Web.Http.FromBody] ShiftRequestRejectDataViewModel data)
    {
      try
      {
        if (data == null || data.SolicitudIds == null || data.SolicitudIds.Count == 0)
        {
          return Json(new { success = false, message = "Sin solicitudes para rechazar." });
        }

        WebServiceSILRespository repo = new WebServiceSILRespository();
        ShiftRequestRejectResultViewModel result;
        try
        {
          result = await repo.RequestSILDataPostAndDeserializeAsync<ShiftRequestRejectResultViewModel>(
            "ShiftRequest", "Reject", data);
        }
        catch (Models.Error.ApiException apiEx)
        {
          Trace.TraceWarning("RechazarMatch 409: " + apiEx.Message);
          return Json(new
          {
            success = false,
            status = 409,
            message = "Conflicto al rechazar: la solicitud ya fue procesada.",
            detalle = apiEx.Message
          });
        }

        if (result == null)
        {
          return Json(new { success = false, message = "El backend devolvió una respuesta vacía." });
        }

        return Json(new
        {
          success = result.TieneExitos,
          message = result.TieneExitos
            ? $"Rechazo aplicado a {result.TotalRechazados} solicitud(es)."
            : "No se pudo rechazar ninguna solicitud.",
          totalRechazados = result.TotalRechazados,
          totalFallidos = result.TotalFallidos,
          rechazados = result.Rechazados,
          rechazosDetalle = result.RechazadosDetalle
        });
      }
      catch (Exception ex)
      {
        Trace.TraceError("RechazarMatch error: " + ex);
        return Json(new { success = false, message = "Error al rechazar matches: " + ex.Message });
      }
    }

    // =====================================================================
    // Helpers privados
    // =====================================================================

    /// <summary>
    /// Normaliza el filtro del form para el endpoint de ACA_SILData:
    /// si el operador pasó 0 en CuentaComprador o ZonaGeografica, mantenemos
    /// 0 (que ACA_SILData interpreta como "no discriminar"). Si pasó string
    /// vacío en algún lado, lo dejamos en 0 también.
    /// </summary>
    private static FilterCuposDisponible NormalizarFiltro(FilterCuposDisponible f)
    {
      return new FilterCuposDisponible
      {
        CuentaVendedor = f.CuentaVendedor,
        CuentaComprador = f.CuentaComprador,
        CodigoGrano = f.CodigoGrano,
        ZonaGeografica = f.ZonaGeografica,
        Fecha = f.Fecha == default ? DateTime.Now.Date : f.Fecha
      };
    }

    /// <summary>
    /// Clasificación heurística client-side. DEPRECADO: ahora usamos el motor
    /// real del backend vía <c>POST /api/ShiftRequest/Matches</c>.
    /// Dejado comentado por si se necesita un fallback offline.
    /// </summary>
    /*
    private static IEnumerable<SolicitudParaMatchViewModel> ClasificarSolicitudes(
      CuposDisponibleViewModel cupo,
      IEnumerable<ShiftRequestPendingViewModel> solicitudes)
    {
      ... (código viejo, reemplazado por el motor real)
    }
    */
  }
}
