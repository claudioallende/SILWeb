using CuposCorretajeWeb.Models.Data;
using CuposCorretajeWeb.Models.Solicitudes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        // 1) Construir la ventana de fechas (7 días a partir de hoy).
        DateTime fechaDesde = DateTime.Today;
        List<SolicitudTurnoDetalleGrupoView> fechasVentana = EnumerateFechas(fechaDesde, filterSolicitud.Dias);

        // 2) Agrupar el response crudo por (grano, vendedor), separando EsFuturo.
        //    CONTRACTUAL: la columna TR muestra Cantidad. FUTURO: la columna TR
        //    muestra CantidadFuturo. La columna TO queda en 0 en ambos casos.
        var contractuales = GroupBySolicitud(rawList.Where(x => !x.EsFuturo), fechasVentana, campoCantidadTR: "Cantidad");
        var futuros = GroupBySolicitud(rawList.Where(x => x.EsFuturo), fechasVentana, campoCantidadTR: "CantidadFuturo");

        // 3) Enriquecer cada fila con el resumen de matching (placeholder por ahora).
        contractuales.ForEach(r => r.CuposCompatibles = BuildResumenMatching(r));
        futuros.ForEach(r => r.CuposCompatibles = BuildResumenMatching(r));

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

      // Forzar UTF-8 en la respuesta para que tildes y caracteres especiales
      // (Selección, días, Próximamente, etc.) se pinten correctamente.
      Response.ContentEncoding = System.Text.Encoding.UTF8;
      Response.Charset = "utf-8";
      Response.ContentType = "text/html; charset=utf-8";

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
          return idx >= 0 ? nombre[(idx + 3)..] : nombre;
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

        // Forzar UTF-8 en la respuesta para que tildes y caracteres especiales
        // (Selección, días, Próximamente, etc.) se pinten correctamente.
        Response.ContentEncoding = System.Text.Encoding.UTF8;
        Response.Charset = "utf-8";
        Response.ContentType = "text/html; charset=utf-8";
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

        Response.ContentEncoding = System.Text.Encoding.UTF8;
        Response.Charset = "utf-8";
        Response.ContentType = "text/html; charset=utf-8";
        return View(solicitud);
      }
    }

    [HttpPost]
    public ActionResult AltaSolicitud([System.Web.Http.FromBody] SolicitudViewModel solicitud)
    {
      try
      {
        TempData["Solicitud"] = solicitud;
        return Json(new { success = true, redirectUrl = Url.Action("AltaSolicitud") });
      }
      catch (Exception ex)
      {
        return Json(new { success = false, message = ex.Message });
      }
    }

    /// <summary>
    /// Confirma una solicitud (cambia su estado a Asignada).
    /// Pantalla 2 lo llama desde el botón "Confirmar".
    ///
    /// TODO: reemplazar el stub por la llamada real a SILData cuando se
    /// defina el endpoint correspondiente (controller/acción a confirmar
    /// con backend). Por ahora sólo devuelve success para validar el flujo
    /// de UI.
    /// </summary>
    [HttpPost]
    public JsonResult ConfirmarSolicitud([System.Web.Http.FromBody] long idSolicitud)
    {
      try
      {
        if (idSolicitud <= 0)
          return Json(new SolicitudActionResponseViewModel { Success = false, Message = "Id de solicitud inválido." });

        // TODO: llamada a SILData (ShiftRequest/ConfirmShiftRequestAsync?).
        Trace.TraceInformation($"[stub] ConfirmarSolicitud id={idSolicitud}");

        return Json(new SolicitudActionResponseViewModel
        {
          Success = true,
          Message = "Solicitud confirmada (stub).",
          RedirectUrl = Url.Action("Index")
        });
      }
      catch (Exception ex)
      {
        Trace.TraceError("ConfirmarSolicitud error: " + ex);
        return Json(new SolicitudActionResponseViewModel { Success = false, Message = ex.Message });
      }
    }

    /// <summary>
    /// Rechaza una solicitud (cambia su estado a Rechazada).
    /// Pantalla 2 lo llama desde el botón "Rechazar".
    ///
    /// TODO: reemplazar el stub por la llamada real a SILData cuando se
    /// defina el endpoint correspondiente.
    /// </summary>
    [HttpPost]
    public JsonResult RechazarSolicitud([System.Web.Http.FromBody] long idSolicitud)
    {
      try
      {
        if (idSolicitud <= 0)
          return Json(new SolicitudActionResponseViewModel { Success = false, Message = "Id de solicitud inválido." });

        // TODO: llamada a SILData (ShiftRequest/RejectShiftRequestAsync?).
        Trace.TraceInformation($"[stub] RechazarSolicitud id={idSolicitud}");

        return Json(new SolicitudActionResponseViewModel
        {
          Success = true,
          Message = "Solicitud rechazada (stub).",
          RedirectUrl = Url.Action("Index")
        });
      }
      catch (Exception ex)
      {
        Trace.TraceError("RechazarSolicitud error: " + ex);
        return Json(new SolicitudActionResponseViewModel { Success = false, Message = ex.Message });
      }
    }

    /// <summary>
    /// Confirma la asignación de los días seleccionados para una solicitud (Pantalla 2,
    /// botón "Confirmar asignación seleccionada").
    ///
    /// Recibe idSolicitud + cupoCompatibleId (null por ahora) + mapa de fechas seleccionadas.
    /// El cupo compatible es null hoy porque el panel derecho "Cupos Compatibles" es
    /// placeholder; cuando se enchufe el motor de matching, este campo viajará poblado
    /// y la llamada a SILData pasará los tres datos.
    ///
    /// TODO: reemplazar el stub por la llamada real a SILData (ShiftRequest/ConfirmShiftRequestAsync?).
    /// Por ahora sólo valida inputs y devuelve success para confirmar el flujo de UI.
    /// </summary>
    [HttpPost]
    public JsonResult ConfirmarAsignacionSeleccionada(ConfirmarAsignacionRequest req)
    {
      try
      {
        if (req == null || req.IdSolicitud <= 0)
          return Json(new SolicitudActionResponseViewModel { Success = false, Message = "Id de solicitud inválido." });
        if (req.Fechas == null || req.Fechas.Count == 0)
          return Json(new SolicitudActionResponseViewModel { Success = false, Message = "Seleccione al menos un día." });

        // TODO: llamar a SILData con req.IdSolicitud + req.CupoCompatibleId + req.Fechas.
        // Por ahora el cupo compatible es null; cuando se enchufe el motor de matching
        // este endpoint recibirá el id del cupo elegido del panel derecho.
        Trace.TraceInformation($"[stub] ConfirmarAsignacionSeleccionada id={req.IdSolicitud} cupo={req.CupoCompatibleId} dias=[{string.Join(",", req.Fechas.Keys)}]");

        return Json(new SolicitudActionResponseViewModel
        {
          Success = true,
          Message = $"Asignación confirmada (stub) para {req.Fechas.Count} día(s).",
          RedirectUrl = Url.Action("Index")
        });
      }
      catch (Exception ex)
      {
        Trace.TraceError("ConfirmarAsignacionSeleccionada error: " + ex);
        return Json(new SolicitudActionResponseViewModel { Success = false, Message = ex.Message });
      }
    }

    // =====================================================================
    // Helpers privados
    // =====================================================================

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
    /// El parámetro campoCantidadTR indica qué campo del item se acumula en la
    /// columna TR de la celda resultante. La columna TO se mantiene en 0 por
    /// ahora; se completará en una iteración posterior.
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

          // Tabla CONTRACTUAL: campoCantidadTR = "Cantidad"  → TR recibe Cantidad.
          // Tabla FUTURO:      campoCantidadTR = "CantidadFuturo" → TR recibe CantidadFuturo.
          // La columna TO queda en 0 (se completará después).
          int valor = campoCantidadTR == "Cantidad" ? item.Cantidad : item.CantidadFuturo;
          if (valor > 0) detalles[fechaKey].Cantidad += valor;
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
          // Comprador y destino participan de la key, así que se leen de g.Key.
          // (Si vienen null en la API, vienen null acá también.)
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
            .ToList()
        };

        result.Add(row);
      }

      return result;
    }

    /// <summary>
    /// Placeholder: arma el resumen de cupos compatibles para la columna "Cupos compatibles".
    /// En una iteración posterior esto se reemplaza por la consulta al motor de matching real.
    /// Por ahora devuelve "Sin coincidencia" salvo cuando la fila está Asignada o Rechazada.
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
