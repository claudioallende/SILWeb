using CuposCorretajeWeb.Models;
using CuposCorretajeWeb.Models.Data;
using CuposCorretajeWeb.Models.Error;
using CuposCorretajeWeb.Models.Identity;
using CuposCorretajeWeb.Models.Solicitudes;
using CuposCorretajeWeb.Models.Solicitudes.Mapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CuposCorretajeWeb.Controllers
{
  [Authorize]
  public class SolicitudesController : Controller
  {
    // Centros por defecto sobre los que se consultan las solicitudes cuando el
    // operador no tiene centros configurados en sus claims (no debería ocurrir
    // porque el login lo bloquea, pero queda como fallback defensivo).
    private static readonly List<string> CentrosDefault = new List<string> { "ROS", "BSAS", "CBA" };

    // Centro por defecto del operador (claim "CentroPorDefecto"). Si no existe
    // el claim o está vacío, cae a "ROS" para preservar el comportamiento previo.
    private string ResolverCentroPorDefecto()
    {
      string centro = ClaimsUtil.GetClaimValue(User, "CentroPorDefecto");
      return string.IsNullOrEmpty(centro) ? "ROS" : centro;
    }

    // GET: Solicitudes
    public ActionResult Index()
    {
      string centroDefecto = ResolverCentroPorDefecto();
      IndexModel model = new IndexModel
      {
        CantidadDias = 7,
        Centro = centroDefecto,
        FechaReferencia = DateTime.Today,
        Subtitulo = $"Solicitudes pendientes de asignación — Centro {NombreCentro(centroDefecto)} · {DateTime.Today:dd/MM/yyyy}"
      };
      return View(model);
    }

    [HttpGet]
    public async Task<ActionResult> GetAllPendingShiftRequests()
    {
      try
      {
        // Tomamos los centros configurados para el operador desde los claims.
        // Si el operador no tiene ningún "Centro" (caso anómalo bloqueado en el
        // login por DatosUsuario.IsAuthorized) caemos a CentrosDefault y
        // dejamos rastro en el log.
        IList<string> centrosUsuario = ClaimsUtil.GetListClaims("Centro");
        List<string> centrosParaFiltro = centrosUsuario.Count > 0
          ? centrosUsuario.ToList()
          : CentrosDefault;
        if (centrosParaFiltro == CentrosDefault)
        {
          Trace.TraceWarning(
            "[Solicitudes] El operador no tiene claims 'Centro' configurados. " +
            "Se utiliza CentrosDefault como fallback.");
        }

        SILSolicitudDeTurnosFilterViewModel filterSolicitud = new SILSolicitudDeTurnosFilterViewModel
        {
          Centros = centrosParaFiltro,
          Dias = 7
        };

        WebServiceSILRespository repo = new WebServiceSILRespository();
        IEnumerable<ShiftRequestPendingViewModel> rawList =
          await repo.RequestSILDataPostAndDeserializeAsync<IEnumerable<ShiftRequestPendingViewModel>>(
            "ShiftRequest", "GetAllPendingShiftRequestAsync", filterSolicitud) ?? new List<ShiftRequestPendingViewModel>();

        int totalRecibidos = rawList.Count();
        int sinComprador = rawList.Count(x => !x.CuentaComprador.HasValue || x.CuentaComprador.Value == 0);
        int sinDestino = rawList.Count(x => !x.CuentaDestino.HasValue || x.CuentaDestino.Value == 0);
        int sinCompradorYDestino = rawList.Count(x =>
          (!x.CuentaComprador.HasValue || x.CuentaComprador.Value == 0) &&
          (!x.CuentaDestino.HasValue || x.CuentaDestino.Value == 0));
        Trace.TraceInformation(
          $"[Solicitudes] API devolvio {totalRecibidos} solicitudes. " +
          $"Sin Comprador: {sinComprador}. Sin Destino: {sinDestino}. " +
          $"Sin Comprador y Sin Destino: {sinCompradorYDestino}.");

        // Diagnóstico: logueamos los acumuladores de TODAS las solicitudes
        // que tuvieron alguna acción (Aceptada > 0 ó Rechazada > 0). Si el
        // back no está poblando CantidadRechazada en este endpoint, lo vamos
        // a ver acá — sin este log el filtro EsPendiente puede fallar
        // silenciosamente (devuelve true aunque la solicitud ya esté
        // completamente rechazada, porque Aceptada + 0 < Cantidad).
        foreach (var it in rawList.Where(x => x.CantidadAceptada > 0 || x.CantidadRechazada > 0
                                            || x.CantidadFuturoAceptada > 0 || x.CantidadFuturoRechazada > 0))
        {
          Trace.TraceInformation(
            $"[Solicitudes] diag id={it.Id} fut={it.EsFuturo} " +
            $"Cantidad={it.Cantidad} Aceptada={it.CantidadAceptada} Rechazada={it.CantidadRechazada} " +
            $"CantidadFuturo={it.CantidadFuturo} FutAceptada={it.CantidadFuturoAceptada} FutRechazada={it.CantidadFuturoRechazada} " +
            $"EsPendiente={it.EsPendiente}");
        }

        DateTime fechaDesde = DateTime.Today;
        List<SolicitudTurnoDetalleGrupoView> fechasVentana = EnumerateFechas(fechaDesde, filterSolicitud.Dias);

        var rawPendientes = rawList.Where(x => x.EsPendiente).ToList();
        Trace.TraceInformation(
          $"[Solicitudes] rawList={rawList.Count()}, pendientes={rawPendientes.Count}, " +
          $"resueltos={rawList.Count() - rawPendientes.Count}.");

        var contractuales = GroupBySolicitud(rawPendientes.Where(x => !x.EsFuturo), fechasVentana, campoCantidadTR: "Cantidad");
        var futuros = GroupBySolicitud(rawPendientes.Where(x => x.EsFuturo), fechasVentana, campoCantidadTR: "CantidadFuturo");

        var cuposAceptadosPorSolicitud = await GetCuposAceptadosPorSolicitudesAsync(
          repo,
          rawList.Select(x => x.Id).Where(id => id > 0).Distinct().ToList());

        var observacionPorSolicitud = rawPendientes
          .Where(x => !string.IsNullOrWhiteSpace(x.Observacion))
          .GroupBy(x => x.Id)
          .ToDictionary(g => g.Key, g => g.First().Observacion);

        var cuposCompatiblesPorGrupo = await EnriquecerResumenesMatchingAsync(
          repo,
          contractuales.Concat(futuros).ToList(),
          fechaDesde,
          filterSolicitud.Dias,
          cuposAceptadosPorSolicitud,
          observacionPorSolicitud);
        foreach (var row in contractuales.Concat(futuros))
        {
          row.CuposCompatibles = cuposCompatiblesPorGrupo.TryGetValue(row, out var c) && c != null
            ? c
            : BuildResumenMatching(row);
        }

        // 4) Armar la respuesta final.
        SolicitudesIndexResponseViewModel response = new SolicitudesIndexResponseViewModel
        {
          Centro = ResolverCentroPorDefecto(),
          FechaDesde = fechaDesde,
          CantidadDias = filterSolicitud.Dias,
          FechasHeader = fechasVentana.Select(f => f.FechaDisplay).ToList(),
          Contractuales = contractuales,
          Futuros = futuros
        };

        return Content(JsonConvert.SerializeObject(response), "application/json");
      }
      catch (Exception ex)
      {
        // En log de error: devolvemos estructura vacía para no romper la grilla.
        // En una iteración posterior esto debería ser un Json con success=false.
        return Content(JsonConvert.SerializeObject(new SolicitudesIndexResponseViewModel
        {
          FechasHeader = EnumerateFechas(DateTime.Today, 7).Select(f => f.FechaDisplay).ToList()
        }), "application/json");
      }
    }

    [HttpGet]
    public async Task<ActionResult> AltaSolicitud()
    {
      SolicitudViewModel solicitud = TempData["Solicitud"] as SolicitudViewModel;
      if (solicitud == null)
      {
        return RedirectToAction("Index");
      }
      // Keep para que sobreviva a las llamadas AJAX que Pantalla 2 hace a
      // BuscarMatches (TempData se borra al final del request si no se Keep).
      TempData.Keep("Solicitud");

      // ViewBag de modo + estado (la vista los usa para banner y readonly).
      ViewBag.Modo = solicitud.EsEditable ? "editable" : "readonly";
      ViewBag.EstadoBadge = solicitud.EstadoBadge ?? "pending";
      ViewBag.EstadoLabel = solicitud.EstadoLabel ?? "Pendiente";
      ViewBag.ErrorMessage = null;

      // En modo read-only no se necesitan los dropdowns: se salta la llamada
      // a SILData y se devuelven listas vacías.
      if (!solicitud.EsEditable)
      {
        ViewBag.Compradores = new List<SelectListItem>();
        ViewBag.CompradorMap = "{}";
        ViewBag.ZonaPortuaria = new List<SelectListItem>();
        return View(solicitud);
      }

      try
      {
        FilterCuposDisponible filterCuposDisponible = new FilterCuposDisponible
        {
          CuentaVendedor = (long)solicitud.CuentaVendedor,
          Fecha = DateTime.Now,
          CodigoGrano = solicitud.CodigoGrano,
          CuentaComprador = 0,
          ZonaGeografica = 0
        };
        ICollection<CuposDisponibleViewModel> cuposDisponible = new List<CuposDisponibleViewModel>();
        WebServiceSILRespository repo = new WebServiceSILRespository();
        cuposDisponible = await repo.RequestSILDataPostAndDeserializeAsync<ICollection<CuposDisponibleViewModel>>("CuposDisponibles", "Disponibles", filterCuposDisponible);

        List<SelectListItem> compradoresDistinct = cuposDisponible
            .Where(x => x.CuentaComprador != 0)
            .GroupBy(x => x.CuentaComprador)
            .Select(g =>
            {
              // Tomar el primer NombreComprador no vacío del grupo como nombre "canónico".
              string nombre = g.Select(x => x.NombreComprador).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n))
                            ?? g.First().NombreComprador
                            ?? string.Empty;
              return new SelectListItem
              {
                Value = g.Key.ToString(),
                Text = $"{g.Key} - {nombre}"
              };
            })
            .OrderBy(o => o.Value)
            .ToList();

        var compradorMap = compradoresDistinct.ToDictionary(o => o.Value, o =>
        {
          string nombre = o.Text;
          int idx = nombre.IndexOf(" - ", StringComparison.Ordinal);
          return idx >= 0 ? nombre.Substring(idx + 3) : nombre;
        });

        List<SelectListItem> zonaDistinct = cuposDisponible
            .Where(x => x.ZonaGeograficaId != 0 && !string.IsNullOrWhiteSpace(x.ZonaGeografica))
            .GroupBy(x => x.ZonaGeograficaId)
            .Select(g => new SelectListItem
            {
              Value = g.Key.ToString(),
              // Tomar el primer nombre no vacío del grupo.
              Text = g.Select(x => x.ZonaGeografica).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)) ?? g.First().ZonaGeografica
            })
            .OrderBy(o => o.Text)
            .ToList();

        ViewBag.Compradores = compradoresDistinct;
        ViewBag.CompradorMap = JsonConvert.SerializeObject(compradorMap);
        ViewBag.ZonaPortuaria = zonaDistinct;

        return View(solicitud);
      }
      catch (Exception ex)
      {
        // No rompemos la página: devolvemos la vista con dropdowns vacíos y
        // un mensaje genérico para mostrar en el banner. El detalle técnico
        // queda en el log para no exponerlo al usuario final.
        Trace.TraceError("AltaSolicitud GET error: " + ex);
        ViewBag.ErrorMessage = "No se pudieron cargar los compradores/zonas disponibles. Reintentá o contactá al administrador.";
        ViewBag.Compradores = new List<SelectListItem>();
        ViewBag.CompradorMap = "{}";
        ViewBag.ZonaPortuaria = new List<SelectListItem>();

        return View(solicitud);
      }
    }

    [HttpPost]
    public ActionResult AltaSolicitud([System.Web.Http.FromBody] SolicitudViewModel solicitud)
    {
      try
      {
        // Defensa: si el caller no mand&oacute; SolicitudesPorFecha (caso
        // t&iacute;pico porque el ViewModel lo completa el controller en el
        // flujo GET), lo inicializamos vac&iacute;o para no romper la vista.
        if (solicitud.SolicitudesPorFecha == null)
          solicitud.SolicitudesPorFecha = new Dictionary<string, long>();

        // .Keep(): necesitamos que la VM sobreviva a las llamadas AJAX que
        // Pantalla 2 hace a BuscarMatches (TempData se borra después del
        // primer Read en el mismo request pipeline).
        TempData["Solicitud"] = solicitud;
        TempData.Keep("Solicitud");
        return Json(new { success = true, redirectUrl = Url.Action("AltaSolicitud") });
      }
      catch (Exception ex)
      {
        return Json(new { success = false, message = ex.Message });
      }
    }

    [HttpPost]
    public async Task<JsonResult> BuscarMatches([System.Web.Http.FromBody] BuscarMatchesRequest req)
    {
      if (req == null || req.IdSolicitud <= 0)
        return Json(new { success = false, message = "Id de solicitud inválido." });

      try
      {
        var solicitud = TempData["Solicitud"] as SolicitudViewModel;
        if (solicitud == null || solicitud.IdSolicitud != req.IdSolicitud)
        {
          return Json(new
          {
            success = false,
            message = "No se encontró la solicitud. Volvé a la grilla e ingresá de nuevo."
          });
        }
        // Mantener para próximas llamadas (ver AltaSolicitud POST).
        TempData.Keep("Solicitud");

        object filter;
        if (req.Fechas != null && req.Fechas.Count > 0)
        {
          var fechasParsed = req.Fechas
            .Select(f => DateTime.ParseExact(f, "yyyy-MM-dd", CultureInfo.InvariantCulture))
            .OrderBy(d => d)
            .ToList();
          filter = new MatchesFilterDto
          {
            CodigoGrano = solicitud.CodigoGrano,
            CuentaVendedor = solicitud.CuentaVendedor,
            CuentaComprador = solicitud.CuentaComprador,
            ZonaGeograficaId = solicitud.CuentaDestino ?? 0,
            FechaDesde = fechasParsed.First(),
            FechaHasta = fechasParsed.Last(),
            IncluirIncompatibles = false
            // agruparPor se omite: default = Solicitud (= 0) en el DTO.
          };
        }
        else
        {
          // Sin rango: el backend resuelve fechaDesde/fechaHasta a
          // hoy / hoy+7 (ver MatchesFilterDto defaults).
          filter = new MatchesFilterDto
          {
            CodigoGrano = solicitud.CodigoGrano,
            CuentaVendedor = solicitud.CuentaVendedor,
            CuentaComprador = solicitud.CuentaComprador,
            ZonaGeograficaId = solicitud.CuentaDestino ?? 0,
            IncluirIncompatibles = false
          };
        }

        var repo = new WebServiceSILRespository();
        var result = await repo.RequestSILDataPostAndDeserializeAsync<BuscarMatchesResponseViewModel>(
          "ShiftRequest", "Matches", filter);

        if (result == null)
          result = new BuscarMatchesResponseViewModel();

        var fechasLog = req.Fechas != null ? string.Join(",", req.Fechas) : "(default)";
        Trace.TraceInformation(
          $"[Solicitudes] BuscarMatches id={req.IdSolicitud} fechas=[{fechasLog}] items={result.Items.Count}");

        return Json(new { success = true, data = result });
      }
      catch (ApiException ex)
      {
        Trace.TraceError($"BuscarMatches API error id={req.IdSolicitud}: " + ex.Message);
        return Json(new { success = false, message = "La API de matching rechazó la consulta. Reintentá o contactá al administrador." });
      }
      catch (Exception ex)
      {
        Trace.TraceError($"BuscarMatches error id={req.IdSolicitud}: " + ex);
        return Json(new { success = false, message = "Error inesperado al buscar matches. Reintentá." });
      }
    }

    /// <summary>
    /// Confirma una solicitud (cambia su estado a Asignada).
    /// Pantalla 2 lo llama desde el botón "Confirmar".
    /// Legacy: acepta Cantidad=1 contra el cupo pre-asignado en la solicitud
    /// (de SOLTURNOS.CUPO_ID). Funciona como wrapper de
    /// <see cref="ConfirmarAsignacionSeleccionada"/> con un único cupo.
    /// </summary>
    [HttpPost]
    public async Task<JsonResult> ConfirmarSolicitud([System.Web.Http.FromBody] long idSolicitud)
    {
      try
      {
        if (idSolicitud <= 0)
          return Json(new SolicitudActionResponseViewModel
          {
            Success = false,
            Message = "Id de solicitud inválido."
          });

        var repo = new WebServiceSILRespository();

        // Traer la solicitud para conocer su cupo pre-asignado.
        var solicitud = await repo.RequestSILDataGetAndDeserializeAsync<SolicitudTurnoDto>(
          "ShiftRequest", idSolicitud.ToString());

        if (solicitud == null)
          return Json(new SolicitudActionResponseViewModel
          {
            Success = false,
            Message = "No se encontró la solicitud."
          });

        if (!solicitud.CupoId.HasValue || solicitud.CupoId.Value <= 0)
          return Json(new SolicitudActionResponseViewModel
          {
            Success = false,
            Message = "La solicitud no tiene un cupo compatible pre-asignado. Use el flujo de selección múltiple (ConfirmarAsignacionSeleccionada)."
          });

        // Wrapper de Cantidad=1 sobre el cupo pre-asignado.
        var req = new ConfirmarAsignacionRequest
        {
          IdSolicitud = idSolicitud,
          CupoCompatibleId = solicitud.CupoId,
          CupoIds = new List<long> { solicitud.CupoId.Value },
          Fechas = new Dictionary<string, int>(),
          CantidadPorCupo = new Dictionary<long, int> { { solicitud.CupoId.Value, 1 } }
        };

        return await ConfirmarAsignacionSeleccionada(req);
      }
      catch (ApiException ex)
      {
        Trace.TraceError("ConfirmarSolicitud error: " + ex);
        return Json(new SolicitudActionResponseViewModel
        {
          Success = false,
          Message = ExtractApiMessage(ex)
        });
      }
      catch (Exception ex)
      {
        Trace.TraceError("ConfirmarSolicitud error: " + ex);
        return Json(new SolicitudActionResponseViewModel { Success = false, Message = ex.Message });
      }
    }

    /// <summary>
    /// Rechaza una solicitud completa (cambia su estado a Rechazada).
    /// Pantalla 2 lo llama desde el botón "Rechazar".
    /// Si la solicitud tenía cupos previamente asignados (Accept parcial),
    /// el backend libera esos cupos automáticamente dentro de la misma
    /// transacción (ver <c>SolicitudTurnoStore.RejectRequestsAsync</c>).
    /// </summary>
    [HttpPost]
    public async Task<JsonResult> RechazarSolicitud(RechazarSolicitudRequest req)
    {
      long idSolicitud = req != null ? req.IdSolicitud : 0;
      // Si el caller manda la lista completa del grupo (uno por fecha), la usamos;
      // si no, caemos al id único legacy para no romper integraciones externas.
      var solicitudIds = (req != null && req.IdsSolicitudes != null && req.IdsSolicitudes.Count > 0)
        ? req.IdsSolicitudes.Where(x => x > 0).Distinct().ToList()
        : new List<long> { idSolicitud };

      try
      {
        if (idSolicitud <= 0 && solicitudIds.Count == 0)
          return Json(new SolicitudActionResponseViewModel
          {
            Success = false,
            Message = "Id de solicitud inválido."
          });

        var payload = new ShiftRequestRejectDataViewModel
        {
          SolicitudIds = solicitudIds,
          Motivo = "Rechazo manual desde Pantalla 2.",
          Automatico = false
        };

        var repo = new WebServiceSILRespository();
        var resultado = await repo.RequestSILDataPostAndDeserializeAsync<ShiftRequestRejectResultDto>(
          "ShiftRequest", "Reject", payload);

        bool exito = resultado != null && resultado.TotalRechazados > 0;
        string message = exito
          ? "Solicitud rechazada correctamente."
          : "La solicitud ya no está pendiente (fue asignada o rechazada por otro operador).";

        if (exito)
        {
          RefreshTempDataSolicitud(asignadosDelta: 0, rechazada: true);
        }

        return Json(new SolicitudActionResponseViewModel
        {
          Success = exito,
          Message = message,
          RedirectUrl = Url.Action("Index")
        });
      }
      catch (ApiException ex)
      {
        Trace.TraceError("RechazarSolicitud error: " + ex);
        return Json(new SolicitudActionResponseViewModel
        {
          Success = false,
          Message = ExtractApiMessage(ex)
        });
      }
      catch (Exception ex)
      {
        Trace.TraceError("RechazarSolicitud error: " + ex);
        return Json(new SolicitudActionResponseViewModel { Success = false, Message = ex.Message });
      }
    }

    [HttpPost]
    public async Task<JsonResult> ConfirmarAsignacionSeleccionada(ConfirmarAsignacionRequest req)
    {
      try
      {
        if (req == null)
          return Json(new ConfirmarAsignacionResponse
          {
            Success = false,
            Message = "Request nulo."
          });

        // Multi-solicitud: si vienen AsignacionesPorSolicitud, vamos a SILApi
        // (SolicitudMatch). Reemplaza al loop legacy de Accepts individuales.
        if (req.AsignacionesPorSolicitud != null && req.AsignacionesPorSolicitud.Count > 0)
        {
          return await ConfirmarAsignacionesPorSolicitudMatch(req);
        }

        if (req.IdSolicitud <= 0)
          return Json(new ConfirmarAsignacionResponse
          {
            Success = false,
            Message = "Id de solicitud inválido."
          });
        if (req.Fechas == null || req.Fechas.Count == 0)
          return Json(new ConfirmarAsignacionResponse
          {
            Success = false,
            Message = "Seleccione al menos un día."
          });
        if (req.CupoIds == null || req.CupoIds.Count == 0)
          return Json(new ConfirmarAsignacionResponse
          {
            Success = false,
            Message = "Seleccione al menos un cupo compatible."
          });

        // CupoCompatibleId legacy: si el caller lo manda null pero trae la lista,
        // completamos con el primero para no romper consumidores que lo esperan.
        if (!req.CupoCompatibleId.HasValue && req.CupoIds.Count > 0)
          req.CupoCompatibleId = req.CupoIds[0];

        var repo = new WebServiceSILRespository();

        // 1) Construir el payload SILData (trae solicitud + cupos, popula
        //    Cantidad desde CantidadPorCupo si existe).
        var payload = await AcceptPayloadBuilder.BuildAsync(repo, req);

        // 2) POST a /api/ShiftRequest/Accept. La respuesta trae el desglose
        //    de asignados vs solicitados vs pendientes (modelo SOLTURNOS_DETALLE).
        var resultado = await repo.RequestSILDataPostAndDeserializeAsync<ShiftRequestAcceptResultDto>(
          "ShiftRequest", "Accept", payload);

        if (resultado == null)
          return Json(new ConfirmarAsignacionResponse
          {
            Success = false,
            Message = "El backend no devolvió resultado."
          });

        // 3) Mapear el resultado al response VM.
        int asignados = resultado.CantidadAsignadaEnEsteAccept;
        int solicitados = resultado.CantidadSolicitadaTotal > 0
          ? resultado.CantidadSolicitadaTotal
          : req.CupoIds.Count;
        int pendientes = resultado.CantidadPendienteRestante > 0
          ? resultado.CantidadPendienteRestante
          : Math.Max(0, solicitados - asignados);

        string message = pendientes > 0
          ? $"Asignaste {asignados} de {solicitados} cupos. {pendientes} quedaron pendientes."
          : $"Asignación confirmada: {asignados} cupos.";

        // Delta por fecha: en el flujo legacy un cupo = 1 turno aceptado
        // (no hay subdivisión intra-cupo), así que cada fecha del request
        // recibe +1. Lo pasamos al RefreshTempDataSolicitud para que el
        // FechasAceptadas se actualice en TempData antes del reload, y la
        // columna "Sol. TO" muestre el valor nuevo sin tener que volver al Index.
        var deltaPorFecha = new Dictionary<string, int>();
        if (req.Fechas != null)
        {
          foreach (var f in req.Fechas.Keys)
          {
            if (string.IsNullOrEmpty(f)) continue;
            deltaPorFecha[f] = 1;
          }
        }
        RefreshTempDataSolicitud(asignados, rechazada: false, deltaPorFecha: deltaPorFecha);

        return Json(new ConfirmarAsignacionResponse
        {
          Success = true,
          Message = message,
          Solicitados = solicitados,
          Asignados = asignados,
          Pendientes = pendientes,
          RedirectUrl = Url.Action("Index")
        });
      }
      catch (ApiException ex)
      {
        Trace.TraceError("ConfirmarAsignacionSeleccionada error: " + ex);
        return Json(new ConfirmarAsignacionResponse
        {
          Success = false,
          Message = ExtractApiMessage(ex)
        });
      }
      catch (Exception ex)
      {
        Trace.TraceError("ConfirmarAsignacionSeleccionada error: " + ex);
        return Json(new ConfirmarAsignacionResponse { Success = false, Message = ex.Message });
      }
    }


    private async Task<JsonResult> ConfirmarAsignacionesPorSolicitudMatch(ConfirmarAsignacionRequest req)
    {
      if (req.CantidadPorCupo != null)
      {
        foreach (var kv in req.CantidadPorCupo)
        {
          if (kv.Value > 1)
          {
            return Json(new ConfirmarAsignacionResponse
            {
              Success = false,
              Message = $"Asignación múltiple por cupo no soportada en este flujo (cupo {kv.Key}: {kv.Value})."
            });
          }
        }
      }

      var asignaciones = new List<AsignacionSolicitudCupoDto>();
      foreach (var a in req.AsignacionesPorSolicitud)
      {
        if (a == null || a.IdSolicitud <= 0 || a.CupoIds == null || a.CupoIds.Count == 0)
        {
          return Json(new ConfirmarAsignacionResponse
          {
            Success = false,
            Message = $"Asignación inválida (solicitud={a?.IdSolicitud}, cupos vacíos)."
          });
        }

        foreach (var cupoId in a.CupoIds)
        {
          if (cupoId <= 0) continue;
          asignaciones.Add(new AsignacionSolicitudCupoDto
          {
            SolicitudId = a.IdSolicitud,
            CupoSeleccionadoId = cupoId,
            Cantidad = 1,
            MatchType = "Directo"
          });
        }
      }

      if (asignaciones.Count == 0)
      {
        return Json(new ConfirmarAsignacionResponse
        {
          Success = false,
          Message = "No hay asignaciones válidas para procesar."
        });
      }

      var payload = new RegistroDistribucionViewModel
      {
        Modo = ModoActualizacionDistribucion.SolicitudMatch,
        AsignacionesSolicitudCupo = asignaciones,
        Confirmacion = true
      };

      var repo = new WebServiceSILRespository();
      ActualizarDistribucionResult resultado;
      try
      {
        resultado = await repo.RequestApiPostAndDeserializeAsync<ActualizarDistribucionResult>(
          "Cupos", "ActualizarDistribucion", payload);
      }
      catch (ApiException ex)
      {
        Trace.TraceError("ConfirmarAsignacionesPorSolicitudMatch SILApi error: " + ex);
        return Json(new ConfirmarAsignacionResponse
        {
          Success = false,
          Message = ExtractApiMessage(ex)
        });
      }

      if (resultado == null)
      {
        return Json(new ConfirmarAsignacionResponse
        {
          Success = false,
          Message = "SILApi no devolvió resultado."
        });
      }

      bool ok = resultado.Success && resultado.Codigo == 1;
      string message;
      if (ok && resultado.Pendientes > 0)
      {
        message = $"Asignaste {resultado.Asignados} de {resultado.Solicitados} cupos. {resultado.Pendientes} quedaron pendientes.";
      }
      else if (ok)
      {
        message = $"Asignación confirmada: {resultado.Asignados} cupos en {asignaciones.Count} asociación(es).";
      }
      else
      {
        message = resultado.Message ?? "SILApi rechazó la asignación.";
      }

      if (ok)
      {
        // Construimos el delta por fecha desde AsignacionesPorSolicitud
        // (cada cupo = 1 turno aceptado en su fecha). Esto actualiza
        // FechasAceptadas en TempData para que al recargar la pantalla
        // (window.location.reload() desde Pantalla 2) la columna "Sol. TO"
        // muestre los valores nuevos sin tener que volver al Index.
        var deltaPorFechaMatch = new Dictionary<string, int>();
        foreach (var a in req.AsignacionesPorSolicitud)
        {
          if (a == null || string.IsNullOrEmpty(a.Fecha)) continue;
          int cuposEnFecha = a.CupoIds != null ? a.CupoIds.Count(c => c > 0) : 0;
          if (cuposEnFecha <= 0) continue;
          if (!deltaPorFechaMatch.ContainsKey(a.Fecha)) deltaPorFechaMatch[a.Fecha] = 0;
          deltaPorFechaMatch[a.Fecha] += cuposEnFecha;
        }
        RefreshTempDataSolicitud(resultado.Asignados, rechazada: false, deltaPorFecha: deltaPorFechaMatch);
      }

      return Json(new ConfirmarAsignacionResponse
      {
        Success = ok,
        Message = message,
        Solicitados = resultado.Solicitados > 0 ? resultado.Solicitados : asignaciones.Count,
        Asignados = resultado.Asignados,
        Pendientes = resultado.Pendientes,
        RedirectUrl = ok ? Url.Action("Index") : null
      });
    }

    /// <summary>
    /// Helper privado LEGACY: ejecuta la corrida multi-solicitud via SILData
    /// (un Accept por cada <see cref="AsignacionPorSolicitud"/>). Se conserva
    /// por compatibilidad con callers externos al flujo SolicitudMatch pero
    /// ya NO se llama desde <c>ConfirmarAsignacionSeleccionada</c>: el flujo
    /// standard de Pantalla 2 ahora va a SILApi vía
    /// <see cref="ConfirmarAsignacionesPorSolicitudMatch"/>.
    /// </summary>
    [Obsolete("Reemplazado por ConfirmarAsignacionesPorSolicitudMatch (SILApi). Conservado por compatibilidad.")]
    private async Task<JsonResult> ConfirmarAsignacionesMultiples(ConfirmarAsignacionRequest req)
    {
      var repo = new WebServiceSILRespository();
      int totalAsignados = 0;
      int totalSolicitados = 0;
      int totalPendientes = 0;
      var errores = new List<string>();
      var exitos = new List<string>();

      foreach (var a in req.AsignacionesPorSolicitud)
      {
        if (a == null || a.IdSolicitud <= 0 || a.CupoIds == null || a.CupoIds.Count == 0)
        {
          errores.Add($"Asignación inválida (solicitud={a?.IdSolicitud}, cupos vacíos).");
          continue;
        }

        var subRequest = new ConfirmarAsignacionRequest
        {
          IdSolicitud = a.IdSolicitud,
          CupoIds = a.CupoIds,
          CupoCompatibleId = a.CupoIds[0],
          Fechas = new Dictionary<string, int> { { a.Fecha ?? string.Empty, a.Tr } },
          CantidadPorCupo = a.CantidadPorCupo ?? new Dictionary<long, int>()
        };

        try
        {
          var payload = await AcceptPayloadBuilder.BuildAsync(repo, subRequest);
          var resultado = await repo.RequestSILDataPostAndDeserializeAsync<ShiftRequestAcceptResultDto>(
            "ShiftRequest", "Accept", payload);

          if (resultado == null)
          {
            errores.Add($"Solicitud {a.IdSolicitud}: el backend no devolvió resultado.");
            continue;
          }

          int asig = resultado.CantidadAsignadaEnEsteAccept;
          int soli = resultado.CantidadSolicitadaTotal > 0
            ? resultado.CantidadSolicitadaTotal
            : a.CupoIds.Count;
          int pend = resultado.CantidadPendienteRestante > 0
            ? resultado.CantidadPendienteRestante
            : Math.Max(0, soli - asig);

          totalAsignados += asig;
          totalSolicitados += soli;
          totalPendientes += pend;
          exitos.Add($"Sol. {a.IdSolicitud}: {asig}/{soli}");
        }
        catch (ApiException ex)
        {
          errores.Add($"Solicitud {a.IdSolicitud}: {ExtractApiMessage(ex)}");
        }
        catch (Exception ex)
        {
          errores.Add($"Solicitud {a.IdSolicitud}: {ex.Message}");
        }
      }

      bool allOk = errores.Count == 0;
      string message;
      if (totalPendientes > 0)
        message = $"Asignaste {totalAsignados} de {totalSolicitados} cupos. {totalPendientes} quedaron pendientes.";
      else
        message = $"Asignación confirmada: {totalAsignados} cupos en {exitos.Count} solicitud(es).";

      if (errores.Count > 0)
        message += " Errores: " + string.Join(" | ", errores);

      return Json(new ConfirmarAsignacionResponse
      {
        Success = allOk,
        Message = message,
        Solicitados = totalSolicitados,
        Asignados = totalAsignados,
        Pendientes = totalPendientes,
        RedirectUrl = allOk ? Url.Action("Index") : null
      });
    }

    // =====================================================================
    // Helpers privados
    // =====================================================================

    /// <summary>
    /// Extrae el mensaje de error de un <see cref="ApiException"/> lanzado por
    /// la capa <c>Util.RequestSILData*</c>. Esos métodos envuelven el body del
    /// <c>ProblemDetails</c> devuelto por SILData en caso de 4xx/5xx.
    /// </summary>
    /// <remarks>
    /// SILData devuelve <c>ProblemDetails</c> (RFC 7807) con campos
    /// <c>title</c>, <c>detail</c>, <c>status</c>. Preferimos <c>detail</c>
    /// (mensaje específico) sobre <c>title</c> (genérico).
    /// </remarks>
    private static string ExtractApiMessage(ApiException ex)
    {
      if (ex == null) return "Error desconocido.";
      try
      {
        var problem = JObject.Parse(ex.Message);
        return problem["detail"]?.ToString()
            ?? problem["title"]?.ToString()
            ?? ex.Message;
      }
      catch
      {
        // El body no es JSON — devolvemos el mensaje crudo.
        return ex.Message;
      }
    }

    private void RefreshTempDataSolicitud(int asignadosDelta, bool rechazada, IDictionary<string, int> deltaPorFecha = null)
    {
      var vm = TempData["Solicitud"] as SolicitudViewModel;
      if (vm == null) return;

      if (rechazada)
      {
        vm.EstadoBadge = "rech";
        vm.EstadoLabel = "Rechazada";
      }
      else
      {
        vm.CantidadAceptada += Math.Max(0, asignadosDelta);
        // Sumar al acumulador por fecha (FechasAceptadas) para que al hacer
        // window.location.reload() desde Pantalla 2 la columna "Sol. TO" muestre
        // el valor actualizado por fecha. Sin esto, el operador ve el TO viejo
        // después del reload y tiene que volver al Index para refrescar.
        if (deltaPorFecha != null && deltaPorFecha.Count > 0)
        {
          if (vm.FechasAceptadas == null) vm.FechasAceptadas = new Dictionary<string, int>();
          foreach (var kv in deltaPorFecha)
          {
            if (string.IsNullOrEmpty(kv.Key)) continue;
            int delta = Math.Max(0, kv.Value);
            if (delta == 0) continue;
            if (!vm.FechasAceptadas.ContainsKey(kv.Key)) vm.FechasAceptadas[kv.Key] = 0;
            vm.FechasAceptadas[kv.Key] += delta;
          }
        }
        if (vm.CantidadOriginal > 0 && vm.CantidadAceptada >= vm.CantidadOriginal)
        {
          vm.EstadoBadge = "asig";
          vm.EstadoLabel = "Asignada";
        }
      }

      TempData["Solicitud"] = vm;
      TempData.Keep("Solicitud");
    }

    private static List<SolicitudTurnoDetalleGrupoView> EnumerateFechas(DateTime desde, int dias)
    {
      List<SolicitudTurnoDetalleGrupoView> list = new List<SolicitudTurnoDetalleGrupoView>();
      for (int i = 0; i < dias; i++)
      {
        DateTime f = desde.AddDays(i);
        list.Add(new SolicitudTurnoDetalleGrupoView
        {
          Fecha = f.ToString("yyyy-MM-dd"),
          FechaDisplay = f.ToString("dd/MM"),
          DiaSemana = (int)f.DayOfWeek,
          Cantidad = 0,
          CantidadAceptada = 0,
          TieneCupoDisponible = false
        });
      }
      return list;
    }

    private static List<SolicitudTurnoGrupoView> GroupBySolicitud(
      IEnumerable<ShiftRequestPendingViewModel> items,
      List<SolicitudTurnoDetalleGrupoView> ventana,
      string campoCantidadTR)
    {
      if (campoCantidadTR != "Cantidad" && campoCantidadTR != "CantidadFuturo")
        throw new ArgumentException("campoCantidadTR debe ser 'Cantidad' o 'CantidadFuturo'.");

      var grupos = items
        .GroupBy(x => new
        {
          x.CodigoGrano,
          x.NombreGrano,
          x.CuentaVendedor,
          x.NombreVendedor,
          x.CuentaComprador,
          x.NombreComprador,
          x.CuentaDestino,
          x.NombreDestino
        });

      List<SolicitudTurnoGrupoView> result = new List<SolicitudTurnoGrupoView>();

      foreach (var g in grupos)
      {
        // Inicializo la fila con todas las fechas de la ventana en cero.
        var detalles = ventana
          .Select(v => new SolicitudTurnoDetalleGrupoView
          {
            Fecha = v.Fecha,
            FechaDisplay = v.FechaDisplay,
            DiaSemana = v.DiaSemana,
            Cantidad = 0,
            CantidadAceptada = 0,
            CantidadRechazada = 0,
            TieneCupoDisponible = v.TieneCupoDisponible
          })
          .ToDictionary(k => k.Fecha, k => k);

        foreach (var item in g)
        {
          string fechaKey = item.FechaSolicitado.ToString("yyyy-MM-dd");
          if (!detalles.ContainsKey(fechaKey)) continue; // fuera de la ventana

          // TS (Solicitados = TR). En tabla CONTRACTUAL se popula con item.Cantidad;
          // en tabla FUTURO con item.CantidadFuturo (la cantidad FUTURA pedida, no la
          // cantidad de cupos futuros).
          int valor = campoCantidadTR == "Cantidad" ? item.Cantidad : item.CantidadFuturo;
          if (valor > 0) detalles[fechaKey].Cantidad += valor;

          // TO (Aceptados/Otorgados). Mapeamos item.CantidadAceptada o
          // item.CantidadFuturoAceptada según la tabla. CantidadAceptada NO es
          // la cantidad futura pedida — es la cantidad ya aceptada para esta fecha.
          int aceptados = campoCantidadTR == "Cantidad"
            ? item.CantidadAceptada
            : item.CantidadFuturoAceptada;
          if (aceptados > 0) detalles[fechaKey].CantidadAceptada = aceptados;

          // Rechazados (R del modelo acumulativo). Mapeamos
          // item.CantidadRechazada o item.CantidadFuturoRechazada según la tabla.
          // Vale 0 mientras la solicitud siga pendiente; pasa a tener valor
          // cuando se rechaza (parcial o totalmente) la solicitud.
          int rechazados = campoCantidadTR == "Cantidad"
            ? item.CantidadRechazada
            : item.CantidadFuturoRechazada;
          if (rechazados > 0) detalles[fechaKey].CantidadRechazada = rechazados;
        }

        // TP (Pendientes) por fecha = Cantidad - CantidadAceptada -
        // CantidadRechazada, clampeado a 0. La columna TP de Pantalla 1 lo
        // muestra al operador: lo que aún resta aceptar o rechazar para esa
        // fecha. Mismo cálculo que la columna "Sol. TP" de Pantalla 2 y que el
        // filtro SQL C - A - R > 0 que decide si la solicitud aparece.
        //
        // Si no se restara CantidadRechazada, una solicitud con
        // (C=5, A=3, R=2) mostraría TP=2 aunque el pendiente real sea 0
        // (solicitud cerrada), y tras una anulación de un aceptado
        // (C=5, A=2, R=2) mostraría TP=3 cuando el pendiente real es 1.
        foreach (var kv in detalles)
        {
          int pendiente = kv.Value.Cantidad - kv.Value.CantidadAceptada - kv.Value.CantidadRechazada;
          kv.Value.CantidadPendiente = pendiente > 0 ? pendiente : 0;
        }

        var first = g.First();
        SolicitudTurnoGrupoView row = new SolicitudTurnoGrupoView
        {
          Id = first.Id,
          CodigoGrano = g.Key.CodigoGrano,
          NombreGrano = g.Key.NombreGrano,
          CuentaVendedor = g.Key.CuentaVendedor,
          NombreVendedor = g.Key.NombreVendedor,
          CuentaComprador = g.Key.CuentaComprador,
          NombreComprador = g.Key.NombreComprador,
          CuentaDestino = g.Key.CuentaDestino,
          NombreDestino = g.Key.NombreDestino,
          CodigoCentro = first.CodigoCentro,
          EstadoBadge = first.GetEstadoBadgeClass(),
          EstadoLabel = first.GetEstadoBadgeLabel(),
          Observacion = first.Observacion,
          CantidadFechas = detalles.Values
            .OrderBy(d => d.Fecha)
            .ToList(),
          SolicitudesPorFecha = g
            .GroupBy(x => x.FechaSolicitado.Date)
            .ToDictionary(
              gg => gg.Key.ToString("yyyy-MM-dd"),
              gg => gg.First().Id),
          FechasAceptadas = g
            .Where(x => ventana.Any(v => v.Fecha == x.FechaSolicitado.ToString("yyyy-MM-dd")))
            .GroupBy(x => x.FechaSolicitado.Date)
            .ToDictionary(
              gg => gg.Key.ToString("yyyy-MM-dd"),
              gg => campoCantidadTR == "Cantidad"
                ? gg.Sum(x => x.CantidadAceptada)
                : gg.Sum(x => x.CantidadFuturoAceptada)),
          // Suma de rechazos por grupo. Si el back no expone
          // CantidadRechazada en el response, este campo queda en 0
          // y se usa como señal diagnóstica en consola del browser.
          CantidadRechazada = campoCantidadTR == "Cantidad"
            ? g.Sum(x => x.CantidadRechazada)
            : g.Sum(x => x.CantidadFuturoRechazada)
        };

        result.Add(row);
      }

      return result;
    }

    /// <summary>
    /// Placeholder: arma el resumen de cupos compatibles para la columna "Cupos compatibles".
    /// Devuelve el resumen propio de las filas en estado Asignada/Rechazada y
    /// "Sin coincidencia" para las pendientes (cuando el enriquecimiento bulk
    /// se saltea o falla).
    /// </summary>
    private static CupoCompatibleResumenViewModel BuildResumenMatching(SolicitudTurnoGrupoView row)
    {
      var resumen = new CupoCompatibleResumenViewModel();

      if (row.EstadoBadge == "asig")
      {
        resumen.CupoAsignadoId = row.Id;
        resumen.TextoResumen = "Cupo asignado";
        return resumen;
      }

      if (row.EstadoBadge == "rech")
      {
        resumen.TextoResumen = "Rechazo manual";
        return resumen;
      }

      resumen.TextoResumen = "Sin coincidencia";
      return resumen;
    }

    private static async Task<Dictionary<long, HashSet<long>>> GetCuposAceptadosPorSolicitudesAsync(
      WebServiceSILRespository repo,
      List<long> solicitudIds)
    {
      var resultado = new Dictionary<long, HashSet<long>>();
      if (solicitudIds == null || solicitudIds.Count == 0) return resultado;

      try
      {
        var resp = await repo.RequestSILDataPostAndDeserializeAsync<List<CupoAceptadoPorSolicitudDto>>(
          "ShiftRequest", "Cupos/Aceptados/PorSolicitudes", solicitudIds);

        if (resp == null) return resultado;
        foreach (var item in resp)
        {
          if (item == null || item.SolicitudId <= 0) continue;
          if (!resultado.TryGetValue(item.SolicitudId, out var hash))
          {
            hash = new HashSet<long>();
            resultado[item.SolicitudId] = hash;
          }
          if (item.CupoIds != null)
          {
            foreach (var cupoId in item.CupoIds)
              if (cupoId > 0) hash.Add(cupoId);
          }
        }
        return resultado;
      }
      catch (Exception ex)
      {
        Trace.TraceWarning(
          "GetCuposAceptadosPorSolicitudesAsync falló: " + ex.Message);
        return resultado;
      }
    }

    /// <summary>
    /// Calcula el resumen de cupos compatibles ("Cupos compatibles") de cada
    /// fila de la grilla.
    ///
    /// Antes esto disparaba un <c>POST ShiftRequest/Matches</c> POR FILA, en
    /// paralelo: con R filas eran R requests HTTP contra SILData y ~4R queries
    /// a Oracle, repitiendo el mismo scan de <c>cuposcorre</c> de la ventana
    /// una y otra vez, y trayendo por red la solicitud y el cupo hidratados
    /// (nombres de vendedor, comprador, destino y zona) para terminar usando
    /// sólo cuatro campos.
    ///
    /// Ahora es UNA llamada a <c>POST ShiftRequest/MatchesVentana</c>, que
    /// devuelve los pares de toda la ventana con los campos justos. La
    /// atribución de cada par a su fila se hace acá, replicando exactamente
    /// el filtro que antes iba en el body de cada request (ver
    /// <see cref="ItemPerteneceAFila"/>).
    ///
    /// Las reglas de negocio NO se movieron: exclusión de cupos ya aceptados,
    /// filtro por fecha con solicitud, dedup por (cupo, tipo) y override a
    /// Condicional por observación siguen ejecutándose acá, igual que antes.
    /// </summary>
    private static async Task<Dictionary<SolicitudTurnoGrupoView, CupoCompatibleResumenViewModel>>
      EnriquecerResumenesMatchingAsync(WebServiceSILRespository repo, List<SolicitudTurnoGrupoView> rows, DateTime fechaDesde, int cantidadDias, Dictionary<long, HashSet<long>> cuposAceptadosPorSolicitud, Dictionary<long, string> observacionPorSolicitud)
    {
      var resultado = new Dictionary<SolicitudTurnoGrupoView, CupoCompatibleResumenViewModel>();

      // 1) Saltamos las filas no pendientes: ya tienen resumen propio.
      var pendientes = rows
        .Where(r => r.EstadoBadge == "pending")
        .ToList();

      if (pendientes.Count == 0)
        return resultado;

      // 2) Una sola llamada al backend para toda la ventana.
      List<MatchVentanaItemDto> items;
      try
      {
        var ventana = await repo.RequestSILDataPostAndDeserializeAsync<MatchesVentanaResponseDto>(
          "ShiftRequest",
          "MatchesVentana",
          // Kind=Unspecified a propósito: DateTime.Today es Kind=Local y
          // Newtonsoft lo serializaría con offset ("-03:00"). Si SILData
          // corre en otra zona horaria (p. ej. UTC en Azure), el backend
          // recibiría el instante convertido y podría caer en otro día.
          // Sin offset viaja la fecha calendario, que es lo que significa.
          new MatchesVentanaFilterDto
          {
            FechaDesde = DateTime.SpecifyKind(fechaDesde.Date, DateTimeKind.Unspecified),
            Dias = cantidadDias
          });

        items = ventana?.Items ?? new List<MatchVentanaItemDto>();

        Trace.TraceInformation(
          $"[Solicitudes] MatchesVentana devolvio {items.Count} items para {pendientes.Count} filas pendientes " +
          $"(desde={fechaDesde:yyyy-MM-dd} dias={cantidadDias}).");
      }
      catch (Exception ex)
      {
        // Mismo criterio de degradación que antes, pero ahora aplica a todas
        // las filas: si el backend no responde, cada fila cae a su resumen
        // derivado del estado en vez de romper la grilla.
        Trace.TraceWarning("EnriquecerResumenesMatchingAsync: fallo MatchesVentana: " + ex.Message);
        foreach (var row in pendientes)
          resultado[row] = BuildResumenMatching(row);
        return resultado;
      }

      // 3) Pre-agrupamos por (grano, vendedor), que son las dos claves de
      // coincidencia exacta. Evita recorrer la lista completa de items una
      // vez por fila cuando la ventana trae varios miles de pares.
      var itemsPorGranoYVendedor = new Dictionary<string, List<MatchVentanaItemDto>>(StringComparer.Ordinal);
      foreach (var it in items)
      {
        if (it == null) continue;
        var clave = it.CodigoGrano + "|" + it.CuentaVendedor;
        if (!itemsPorGranoYVendedor.TryGetValue(clave, out var lista))
        {
          lista = new List<MatchVentanaItemDto>();
          itemsPorGranoYVendedor[clave] = lista;
        }
        lista.Add(it);
      }

      // 4) Un resumen por fila, con las mismas reglas de siempre.
      foreach (var row in pendientes)
      {
        try
        {
          var fechasConSolicitud = new HashSet<string>(
            (row.CantidadFechas ?? Enumerable.Empty<SolicitudTurnoDetalleGrupoView>())
              .Where(d => (d.Cantidad + d.CantidadAceptada) > 0)
              .Select(d => d.Fecha),
            StringComparer.Ordinal);

          var resumen = new CupoCompatibleResumenViewModel();
          var cuposContadosPorTipo = new HashSet<string>(StringComparer.Ordinal);

          List<MatchVentanaItemDto> candidatos;
          if (!itemsPorGranoYVendedor.TryGetValue(row.CodigoGrano + "|" + row.CuentaVendedor, out candidatos))
            candidatos = new List<MatchVentanaItemDto>();

          int itemsBack = candidatos.Count;
          int itemsRechazadosPorFila = 0;
          int itemsRechazadosPorAceptado = 0;
          int itemsRechazadosPorFecha = 0;
          int itemsRechazadosPorDedup = 0;
          int itemsContados = 0;
          int itemsForzadosAObservacion = 0;

          foreach (var it in candidatos)
          {
            // Filtro que antes viajaba en el body del request de esta fila.
            if (!ItemPerteneceAFila(it, row))
            {
              itemsRechazadosPorFila++;
              continue;
            }

            if (cuposAceptadosPorSolicitud != null
                && cuposAceptadosPorSolicitud.TryGetValue(it.SolicitudId, out var aceptados)
                && aceptados != null
                && aceptados.Contains(it.CupoId))
            {
              itemsRechazadosPorAceptado++;
              continue;
            }

            string cupoFechaKey = it.CupoFecha.HasValue
              ? it.CupoFecha.Value.ToString("yyyy-MM-dd")
              : null;
            if (cupoFechaKey == null || !fechasConSolicitud.Contains(cupoFechaKey))
            {
              itemsRechazadosPorFecha++;
              continue;
            }

            var dedupKey = it.CupoId + "|" + (it.MatchType ?? string.Empty);
            if (!cuposContadosPorTipo.Add(dedupKey))
            {
              itemsRechazadosPorDedup++;
              continue;
            }

            // Clasificación por solicitud: si ESA solicitud tiene observación,
            // override a Condicional. Si no, respetamos lo del motor.
            bool solConObs = observacionPorSolicitud != null
              && observacionPorSolicitud.TryGetValue(it.SolicitudId, out var obsTxt)
              && !string.IsNullOrWhiteSpace(obsTxt);
            string tipoEfectivo = solConObs ? "Condicional" : it.MatchType;
            if (solConObs && it.MatchType != "Condicional") itemsForzadosAObservacion++;

            switch (tipoEfectivo)
            {
              case "Directo": resumen.Directos++; break;
              case "Parcial": resumen.Parciales++; break;
              case "Condicional": resumen.Observaciones++; break;
            }
            itemsContados++;
          }

          Trace.TraceInformation(
            $"[Solicitudes] EnriquecerResumenesMatching row id={row.Id} grano={row.CodigoGrano} vendedor={row.CuentaVendedor} " +
            $"itemsBack={itemsBack} rechazadosFila={itemsRechazadosPorFila} rechazadosAceptado={itemsRechazadosPorAceptado} " +
            $"rechazadosFecha={itemsRechazadosPorFecha} rechazadosDedup={itemsRechazadosPorDedup} contados={itemsContados} " +
            $"forzadosObs={itemsForzadosAObservacion} " +
            $"-> directos={resumen.Directos} parciales={resumen.Parciales} observaciones={resumen.Observaciones}");

          resumen.TextoResumen = (resumen.Directos + resumen.Parciales + resumen.Observaciones) > 0
            ? null
            : "Sin coincidencia";

          resultado[row] = resumen;
        }
        catch (Exception ex)
        {
          Trace.TraceWarning(
            $"EnriquecerResumenesMatchingAsync fila id={row.Id} grano={row.CodigoGrano} vendedor={row.CuentaVendedor}: {ex.Message}");
          resultado[row] = BuildResumenMatching(row);
        }
      }

      return resultado;
    }

    /// <summary>
    /// Replica en memoria el filtro que antes viajaba en el body del
    /// <c>POST ShiftRequest/Matches</c> de cada fila
    /// (<c>MatchesFilterDto</c> → <c>GetByMatchesFilterAsync</c>):
    /// <list type="bullet">
    ///   <item>Grano y vendedor: coincidencia exacta
    ///     (<c>s.grano = :grano</c>, <c>s.ctavend = :vendedor</c>).</item>
    ///   <item>Comprador y destino: sólo filtran cuando la fila los tiene.
    ///     En el SQL el patrón era <c>(col = :p OR 0 = :p)</c>, y el
    ///     parámetro llegaba como <c>valor ?? 0</c>: un 0 desactivaba el
    ///     filtro. Por eso una fila sin comprador (o sin destino) sigue
    ///     contando los matches de solicitudes con cualquier comprador
    ///     (o cualquier destino), igual que antes.</item>
    /// </list>
    /// </summary>
    private static bool ItemPerteneceAFila(MatchVentanaItemDto item, SolicitudTurnoGrupoView row)
    {
      if (item.CodigoGrano != row.CodigoGrano) return false;
      if (item.CuentaVendedor != row.CuentaVendedor) return false;

      long compradorFila = row.CuentaComprador ?? 0;
      if (compradorFila != 0 && (item.CuentaComprador ?? 0) != compradorFila) return false;

      long destinoFila = row.CuentaDestino ?? 0;
      if (destinoFila != 0 && (item.CuentaDestino ?? 0) != destinoFila) return false;

      return true;
    }

    private static string NombreCentro(string codigo)
    {
      switch (codigo)
      {
        case "ROS": return "Rosario";
        case "BSAS": return "Buenos Aires";
        case "CBA": return "Córdoba";
        default: return codigo;
      }
    }
  }
}
