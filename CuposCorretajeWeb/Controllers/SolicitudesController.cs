using CuposCorretajeWeb.Models;
using CuposCorretajeWeb.Models.Data;
using CuposCorretajeWeb.Models.Error;
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
    // Centros por defecto sobre los que se consultan las solicitudes.
    // En una iteración posterior esto debería salir de ClaimsUtil (centro del operador).
    private static readonly List<string> CentrosDefault = new List<string> { "ROS", "BSAS", "CBA" };

    // GET: Solicitudes
    public ActionResult Index()
    {
      IndexModel model = new IndexModel
      {
        CantidadDias = 7,
        Centro = "ROS",
        FechaReferencia = DateTime.Today,
        Subtitulo = $"Solicitudes pendientes de asignación — Centro {NombreCentro("ROS")} · {DateTime.Today:dd/MM/yyyy}"
      };
      return View(model);
    }

    [HttpGet]
    public async Task<ActionResult> GetAllPendingShiftRequests()
    {
      try
      {
        SILSolicitudDeTurnosFilterViewModel filterSolicitud = new SILSolicitudDeTurnosFilterViewModel
        {
          Centros = CentrosDefault,
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

        // 1) Construir la ventana de fechas (7 días a partir de hoy).
        DateTime fechaDesde = DateTime.Today;
        List<SolicitudTurnoDetalleGrupoView> fechasVentana = EnumerateFechas(fechaDesde, filterSolicitud.Dias);

        // 2) Agrupar el response crudo por (grano, vendedor), separando EsFuturo.
        //    CONTRACTUAL: la columna TR muestra Cantidad. FUTURO: la columna TR
        //    muestra CantidadFuturo. La columna TO queda en 0 en ambos casos.
        var contractuales = GroupBySolicitud(rawList.Where(x => !x.EsFuturo), fechasVentana, campoCantidadTR: "Cantidad");
        var futuros = GroupBySolicitud(rawList.Where(x => x.EsFuturo), fechasVentana, campoCantidadTR: "CantidadFuturo");

        // 3) Enriquecer cada fila con el resumen de matching. Las filas pendientes
        //    piden al motor bulk /ShiftRequest/Matches un conteo por tipo
        //    (Directo / Parcial / Condicional) usando la misma ventana que la
        //    grilla. Las ya Asignadas/Rechazadas conservan su resumen propio
        //    (CupoAsignadoId / "Rechazo manual"). Las llamadas se hacen en
        //    paralelo para no serializar N requests HTTP.
        //
        //    Para no contar como "match disponible" los cupos que la
        //    solicitud ya tiene aceptados (el operador no los puede volver
        //    a asignar) traenos todos los cupoIds ya otorgados en una sola
        //    query batched antes del fan-out.
        var cuposAceptadosPorSolicitud = await GetCuposAceptadosPorSolicitudesAsync(
          repo,
          rawList.Select(x => x.Id).Where(id => id > 0).Distinct().ToList());

        var cuposCompatiblesPorGrupo = await EnriquecerResumenesMatchingAsync(
          repo,
          contractuales.Concat(futuros).ToList(),
          fechaDesde,
          filterSolicitud.Dias,
          cuposAceptadosPorSolicitud);
        foreach (var row in contractuales.Concat(futuros))
        {
          row.CuposCompatibles = cuposCompatiblesPorGrupo.TryGetValue(row, out var c) && c != null
            ? c
            : BuildResumenMatching(row);
        }

        // 4) Armar la respuesta final.
        SolicitudesIndexResponseViewModel response = new SolicitudesIndexResponseViewModel
        {
          Centro = "ROS",
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
        // Reconstruimos los datos de la solicitud desde TempData (mismo
        // patrón que AltaSolicitud GET). Si no hay TempData o no coincide el
        // id, devolvemos error para que el operador vuelva a Pantalla 1
        // en vez de rehidratar contra la API (no queremos un side-effect
        // silencioso).
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
          filter = new
          {
            codigoGrano = solicitud.CodigoGrano,
            cuentaVendedor = solicitud.CuentaVendedor,
            cuentaComprador = solicitud.CuentaComprador,
            zonaGeograficaId = solicitud.CuentaDestino ?? 0,
            fechaDesde = fechasParsed.First(),
            fechaHasta = fechasParsed.Last(),
            incluirIncompatibles = false
            // agruparPor se omite: default = Solicitud (= 0) en el DTO.
          };
        }
        else
        {
          // Sin rango: el backend resuelve fechaDesde/fechaHasta a
          // hoy / hoy+7 (ver MatchesFilterDto defaults).
          filter = new
          {
            codigoGrano = solicitud.CodigoGrano,
            cuentaVendedor = solicitud.CuentaVendedor,
            cuentaComprador = solicitud.CuentaComprador,
            zonaGeograficaId = solicitud.CuentaDestino ?? 0,
            incluirIncompatibles = false
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
        // 400/409/500 de SILData: el body ya viene como ProblemDetails. El
        // caller (JS) extrae responseJSON.detail/title con mostrarErrorAjax.
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
      try
      {
        if (idSolicitud <= 0)
          return Json(new SolicitudActionResponseViewModel
          {
            Success = false,
            Message = "Id de solicitud inválido."
          });

        var payload = new ShiftRequestRejectDataViewModel
        {
          SolicitudIds = new List<long> { idSolicitud },
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

    /// <summary>
    /// Confirma la asignación de los cupos seleccionados para una solicitud (Pantalla 2,
    /// botón "Confirmar asignación seleccionada").
    ///
    /// Recibe idSolicitud + lista de CupoIds (long, reales del motor de matching) +
    /// mapa de fechas seleccionadas + mapa CantidadPorCupo (default 1 por cupo).
    /// CupoCompatibleId se conserva por compatibilidad legacy y se popula con el
    /// primero de CupoIds si está null.
    ///
    /// Si vienen <see cref="ConfirmarAsignacionRequest.AsignacionesPorSolicitud"/>
    /// (camino multi-solicitud de Pantalla 2), arma un payload
    /// <see cref="RegistroDistribucionViewModel"/> en modo
    /// <see cref="ModoActualizacionDistribucion.SolicitudMatch"/> y POSTea a
    /// <c>SILApi /api/Cupos/ActualizarDistribucion</c>. Esta ruta reemplaza al
    /// flujo legacy de N calls a SILData <c>/api/ShiftRequest/Accept</c>: SILApi
    /// ahora persiste CUPOSDIST + CUPOSCORRE + SOLTURNOS + SOLTURNOS_DETALLE en
    /// una sola transacción NHibernate.
    ///
    /// Si NO vienen AsignacionesPorSolicitud, mantiene el flujo legacy
    /// (ConfirmarSolicitud / Pantallas 1) que sigue yendo a SILData por el
    /// endpoint Accept legacy.
    /// </summary>
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

    /// <summary>
    /// Helper privado: ejecuta el flujo SolicitudMatch contra SILApi.
    /// Toma las <see cref="ConfirmarAsignacionRequest.AsignacionesPorSolicitud"/>
    /// de Pantalla 2 (multi-solicitud), las traduce a una lista de
    /// <see cref="AsignacionSolicitudCupoDto"/> con <c>CupoSeleccionadoId</c>
    /// fijo (1 cupo = 1 asignación) y arma un único payload
    /// <see cref="RegistroDistribucionViewModel"/> en modo
    /// <see cref="ModoActualizacionDistribucion.SolicitudMatch"/>.
    /// SILApi persiste todo en una transacción NHibernate atómica
    /// (CUPOSCORRE + CUPOSDIST + SOLTURNOS + SOLTURNOS_DETALLE).
    ///
    /// Si el request tiene <see cref="ConfirmarAsignacionRequest.CantidadPorCupo"/>
    /// con algún valor > 1, abortamos: el flujo SolicitudMatch sólo soporta
    /// 1 asignación por card. La subdivisión intra-cupo queda como hook futuro
    /// (ver §8 del plan-integracion-solicitudes-distribucion.md).
    /// </summary>
    private async Task<JsonResult> ConfirmarAsignacionesPorSolicitudMatch(ConfirmarAsignacionRequest req)
    {
      // Defensa: la subdivisión intra-cupo (>1 por card) no está habilitada
      // en SolicitudMatch v1. Bloqueamos cualquier intento de asignar >1 por
      // cupo para no terminar con un payload inconsistente (la sumatoria de
      // cantidades excedería lo que el backend sabe distribuir).
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
        // 4xx/5xx de SILApi: el wrapper ya tira ApiException con el body
        // (mensaje de ExceptionHandlingAttribute o ProblemDetails). Lo
        // propagamos como mensaje al operador.
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

      // Mapear ActualizarDistribucionResult → ConfirmarAsignacionResponse.
      // Success se evalúa contra Codigo == 1 (semántica legacy) Y
      // Success == true. Pendientes > 0 indica asignación parcial.
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
          CantidadFuturo = 0,
          TieneCupoDisponible = false
        });
      }
      return list;
    }

    /// <summary>
    /// Agrupa el response crudo por (grano, vendedor, comprador, destino) y arma
    /// la lista de CantidadFechas dentro de la ventana.
    ///
    /// La separación entre las dos tablas (CONTRACTUAL y FUTURO) se hace a nivel
    /// del llamador filtrando por EsFuturo, así que acá se agrupa por la
    /// combinación (grano, vendedor, comprador, destino) ignorando el tipo de
    /// solicitud. Comprador y destino pueden venir null (solicitudes sólo con
    /// solicitante): en ese caso el null participa de la key y las filas con
    /// destino null forman su propio grupo, no se mezclan con las que sí
    /// tienen destino.
    ///
    /// Parámetro <paramref name="campoCantidadTR"/>: campo del item que se
    /// acumula en la columna TR (Cantidad o CantidadFuturo según la tabla).
    ///
    /// Columna TO (<see cref="SolicitudTurnoDetalleGrupoView.CantidadFuturo"/>):
    /// acumula la cantidad ACEPTADA para esa fecha. Como el response del
    /// backend no desglosa los aceptados por fecha (vienen totales en
    /// CantidadAceptada / CantidadFuturoAceptada), ponemos el total del grupo
    /// en la celda de la fecha solicitada por el primer item del grupo y 0 en
    /// el resto. Es una simplificación: si en una iteración posterior se
    /// quiere el desglose por fecha, hay que agregar un endpoint que joinee
    /// SOLTURNOS_DETALLE con CUPOSCORRE.fecha.
    /// </summary>
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
            CantidadFuturo = 0,
            TieneCupoDisponible = v.TieneCupoDisponible
          })
          .ToDictionary(k => k.Fecha, k => k);

        foreach (var item in g)
        {
          string fechaKey = item.FechaSolicitado.ToString("yyyy-MM-dd");
          if (!detalles.ContainsKey(fechaKey)) continue; // fuera de la ventana

          // TR (Cantidad/CantidadFuturo segun corresponda a la tabla)
          int valor = campoCantidadTR == "Cantidad" ? item.Cantidad : item.CantidadFuturo;
          if (valor > 0) detalles[fechaKey].Cantidad += valor;

          // TO: CantidadAceptada (o CantidadFuturoAceptada para la tabla FUTURO)
          // por FECHA, no la suma del grupo depositada en la celda del primer
          // item. Cada item tiene su propia FechaSolicitado y su propio
          // CantidadAceptada; depositar la SUYA en la celda de SU fecha es lo
          // que el operador espera ver — aceptar cupos para el d&iacute;a 10
          // debe impactar la celda del 10, no la del 9.
          int aceptados = campoCantidadTR == "Cantidad"
            ? item.CantidadAceptada
            : item.CantidadFuturoAceptada;
          if (aceptados > 0) detalles[fechaKey].CantidadFuturo = aceptados;
        }

        // El estado de la fila lo define la primera solicitud del grupo.
        // (Si todas coinciden perfecto, si no, se puede refinar en una iteración posterior.)
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
          // Observaciones: las del primer item del grupo. Si en una iteración
          // posterior hace falta consolidar observaciones de varios items, se
          // cambia acá. Por ahora alcanza con una sola para el banner.
          Observacion = first.Observacion,
          CantidadFechas = detalles.Values
            .OrderBy(d => d.Fecha)
            .ToList(),
          // Mapa fecha → solicitudId. Pantalla 1 agrupa solicitudes por
          // (grano, vendedor, comprador, destino); cada item del grupo tiene
          // su propio Id y FechaSolicitado. Cuando el operador tilda una
          // fecha en Pantalla 2, ese mapa nos permite saber la solicitudId
          // ESPECÍFICA del d&iacute;a (no s&oacute;lo la del primer item).
          // Si dos items del grupo caen en la misma fecha (no deber&iacute;a
          // pasar en la pr&aacute;ctica por la l&oacute;gica de creaci&oacute;n
          // de SOLTURNOS), gana el primero que aparece.
          SolicitudesPorFecha = g
            .GroupBy(x => x.FechaSolicitado.Date)
            .ToDictionary(
              gg => gg.Key.ToString("yyyy-MM-dd"),
              gg => gg.First().Id),
          // Mapa fecha → CantidadAceptada (o CantidadFuturoAceptada para
          // FUTURO) por FECHA, no la suma del grupo. Cada item deposita su
          // aceptados en la celda de su propia fecha. Si en una iteraci&oacute;n
          // posterior se necesita el desglose por cupo aceptado, se cambia
          // el SELECT en SolicitudTurnoStore.
          FechasAceptadas = g
            .Where(x => ventana.Any(v => v.Fecha == x.FechaSolicitado.ToString("yyyy-MM-dd")))
            .GroupBy(x => x.FechaSolicitado.Date)
            .ToDictionary(
              gg => gg.Key.ToString("yyyy-MM-dd"),
              gg => campoCantidadTR == "Cantidad"
                ? gg.Sum(x => x.CantidadAceptada)
                : gg.Sum(x => x.CantidadFuturoAceptada))
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

    /// <summary>
    /// Trae el conjunto de cupos ya ACEPTADOS para cada solicitud en una sola
    /// query batched (un POST por página de grilla, no por fila). Se usa
    /// después para descontar del conteo de matches del motor bulk los cupos
    /// que la solicitud ya tiene otorgados — sin este descuento la columna
    /// "Cupos compatibles" muestra un conteo inflado (incluiría cupos ya
    /// asignados en fechas anteriores que la solicitud ya completó).
    ///
    /// Si la llamada falla (timeout, 5xx, etc.), devolvemos un mapa vacío
    /// para no romper la grilla: el operador vería los counts originales
    /// (un poco inflados) hasta el pr&oacute;ximo refresh.
    /// </summary>
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
    /// Enriquece cada fila pendiente con el conteo de matches del motor
    /// (Directos / Parciales / Observaciones[=Condicional]). Reutiliza el
    /// endpoint bulk <c>POST /api/ShiftRequest/Matches</c> con los mismos
    /// filtros que aplicar&iacute;a Pantalla 2 (codigoGrano + cuentaVendedor
    /// + cuentaComprador + zonaGeograficaId + ventana de fechas). Para cada
    /// combinaci&oacute;n (grano, vendedor, comprador, destino) hace 1 llamada
    /// y cuenta <c>Items[].MatchType</c> en buckets.
    ///
    /// Antes de contar, descuenta los items cuyo <c>CupoId</c> ya figura
    /// como aceptado para esa solicitud (sale de
    /// <paramref name="cuposAceptadosPorSolicitud"/>). Sin este descuento el
    /// motor devolvería matches para TODAS las fechas de la ventana sin
    /// importar que la solicitud ya haya aceptado cupos en alguna, y la
    /// columna "Cupos compatibles" mostraría un conteo inflado.
    ///
    /// Las llamadas se ejecutan en paralelo (<see cref="Task.WhenAll(Task[])"/>)
    /// para no serializar N round-trips HTTP al backend en grillas grandes.
    /// Si la llamada de una fila falla (timeout, 5xx, conflict), esa fila
    /// recibe el placeholder por defecto — el resto no se ve afectada.
    /// </summary>
    /// <param name="cuposAceptadosPorSolicitud">
    /// Mapa <c>solicitudId → HashSet&lt;cupoId&gt;</c> con los cupos que ya
    /// fueron aceptados (vienen de <c>SOLTURNOS_DETALLE</c>). Se trae antes
    /// en una sola query batched desde
    /// <c>POST /api/ShiftRequest/Cupos/Aceptados/PorSolicitudes</c>. Si el
    /// fetch falló, este mapa puede ser null/vacío — en ese caso no se
    /// descuenta nada (no rompemos la grilla).
    /// </param>
    private static async Task<Dictionary<SolicitudTurnoGrupoView, CupoCompatibleResumenViewModel>>
      EnriquecerResumenesMatchingAsync(
        WebServiceSILRespository repo,
        List<SolicitudTurnoGrupoView> rows,
        DateTime fechaDesde,
        int cantidadDias,
        Dictionary<long, HashSet<long>> cuposAceptadosPorSolicitud)
    {
      var resultado = new Dictionary<SolicitudTurnoGrupoView, CupoCompatibleResumenViewModel>();

      // 1) Saltamos las filas no pendientes: ya tienen resumen propio.
      var pendientes = rows
        .Where(r => r.EstadoBadge == "pending")
        .ToList();

      if (pendientes.Count == 0)
        return resultado;

      var tareas = pendientes.Select(async row =>
      {
        try
        {
          var filter = new
          {
            codigoGrano = row.CodigoGrano,
            cuentaVendedor = row.CuentaVendedor,
            cuentaComprador = row.CuentaComprador,
            zonaGeograficaId = row.CuentaDestino,
            fechaDesde = fechaDesde,
            fechaHasta = fechaDesde.AddDays(cantidadDias - 1),
            incluirIncompatibles = false
            // agruparPor se omite: default = Solicitud (= 0) en el DTO.
          };

          var resp = await repo.RequestSILDataPostAndDeserializeAsync<GrillaMatchResumenDto>(
            "ShiftRequest", "Matches", filter);

          var fechasConSolicitud = new HashSet<string>(
            (row.CantidadFechas ?? Enumerable.Empty<SolicitudTurnoDetalleGrupoView>())
              .Where(d => (d.Cantidad + d.CantidadFuturo) > 0)
              .Select(d => d.Fecha),
            StringComparer.Ordinal);

          var resumen = new CupoCompatibleResumenViewModel();
          if (resp != null && resp.Items != null)
          {
            var cuposContadosPorTipo = new HashSet<string>(StringComparer.Ordinal);

            foreach (var it in resp.Items)
            {
              if (it == null) continue;

              if (cuposAceptadosPorSolicitud != null
                  && cuposAceptadosPorSolicitud.TryGetValue(it.SolicitudId, out var aceptados)
                  && aceptados != null
                  && aceptados.Contains(it.CupoId))
              {
                continue;
              }

              string cupoFechaKey = (it.Cupo != null && it.Cupo.Fecha.HasValue)
                ? it.Cupo.Fecha.Value.ToString("yyyy-MM-dd")
                : null;
              if (cupoFechaKey == null || !fechasConSolicitud.Contains(cupoFechaKey))
              {
                continue;
              }

              var dedupKey = it.CupoId + "|" + (it.MatchType ?? string.Empty);
              if (!cuposContadosPorTipo.Add(dedupKey))
              {
                continue;
              }

              switch (it.MatchType)
              {
                case "Directo": resumen.Directos++; break;
                case "Parcial": resumen.Parciales++; break;
                case "Condicional": resumen.Observaciones++; break;
              }
            }
          }

          resumen.TextoResumen = (resumen.Directos + resumen.Parciales + resumen.Observaciones) > 0
            ? null
            : "Sin coincidencia";

          return new KeyValuePair<SolicitudTurnoGrupoView, CupoCompatibleResumenViewModel>(row, resumen);
        }
        catch (Exception ex)
        {
          Trace.TraceWarning(
            $"EnriquecerResumenesMatchingAsync fila id={row.Id} grano={row.CodigoGrano} vendedor={row.CuentaVendedor}: {ex.Message}");
          return new KeyValuePair<SolicitudTurnoGrupoView, CupoCompatibleResumenViewModel>(
            row, BuildResumenMatching(row));
        }
      }).ToList();

      var pares = await Task.WhenAll(tareas);
      foreach (var kvp in pares)
        resultado[kvp.Key] = kvp.Value;

      return resultado;
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

    /// <summary>
    /// Enriquece cada <see cref="MatchItemViewModel"/> de la lista con los
    /// nombres hidratados del catálogo de SILData (<c>SolicitudTurnoView</c>).
    ///
    /// Estrategia: 1 sola llamada GET a <c>/api/ShiftRequest/GetByVendedorAsync/{cuentaVendedor}</c>
    /// que devuelve TODAS las solicitudes del vendedor con nombres resueltos.
    /// Después indexamos por <c>Id</c> en un diccionario y copiamos los nombres
    /// a los items de match que correspondan.
    ///
    /// Notas:
    /// - La pantalla 2 siempre trabaja con un único vendedor activo
    ///   (el de la solicitud en TempData), as&iacute; que con 1 llamada alcanza.
    /// - Los nombres son de la SOLICITUD. Para campos del CUPO (Comprador /
    ///   Destino) el nombre puede NO coincidir con el id del cupo cuando el
    ///   match es Parcial/Condicional. La UI muestra el nombre cuando est&aacute;
    ///   disponible y el id del cupo como contexto.
    /// - Si la llamada falla (timeout, 5xx, etc.) NO rompemos el flujo:
    ///   dejamos los nombres null y la card cae al fallback de IDs.
    /// </summary>
    [Obsolete("Reemplazado por hidrataci&oacute;n directa en MatchCupoResumen (SILData).")]
    private static async Task HidratarNombres(List<MatchItemViewModel> items, long cuentaVendedor)
    {
      // Stub inofensivo tras la hidrata directa en SILData
      // (MatchCupoResumen.NombreVendedor / NombreComprador / NombreDestino).
      // Se conserva la firma porque MatchItemViewModel ya no tiene las
      // propiedades planas NombreVendedor/Comprador/Destino/Grano (viven
      // adentro de Cupo). Si en alg&uacute;n momento queremos volver a
      // hidratar desde GetByVendedorAsync, este stub ser&aacute; el lugar.
      await Task.CompletedTask;
    }
  }
}
