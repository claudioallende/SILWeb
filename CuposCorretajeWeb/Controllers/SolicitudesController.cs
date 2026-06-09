using CuposCorretajeWeb.Models.Data;
using CuposCorretajeWeb.Models.Solicitudes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        // 2) Agrupar el response crudo por (grano, vendedor, comprador, destino),
        //    separando EsFuturo. Dentro de cada grupo acumular Cantidad y CantidadFuturo por fecha.
        var contractuales = GroupBySolicitud(rawList.Where(x => !x.EsFuturo), fechasVentana, usarCantidadFuturo: false);
        var futuros = GroupBySolicitud(rawList.Where(x => x.EsFuturo), fechasVentana, usarCantidadFuturo: true);

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
      try
      {
        SolicitudViewModel solicitud = TempData["Solicitud"] as SolicitudViewModel;
        if (solicitud == null)
        {
          return RedirectToAction("Index");
        }

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
            .GroupBy(x => new { x.CuentaComprador, x.NombreComprador })
            .Select(g => new SelectListItem
            {
              Value = g.Key.CuentaComprador.ToString(),
              Text = $"{g.Key.CuentaComprador} - {g.Key.NombreComprador}"
            })
            .OrderBy(o => o.Value)
            .ToList();

        var compradorMap = cuposDisponible
            .GroupBy(x => new { x.CuentaComprador, x.NombreComprador })
            .ToDictionary(
                g => g.Key.CuentaComprador.ToString(),
                g => g.Key.NombreComprador.ToString()
            );

        List<SelectListItem> zonaDistinct = cuposDisponible
            .Where(x => x.ZonaGeografica != null && x.ZonaGeograficaId != 0)
            .GroupBy(x => new { x.ZonaGeografica, x.ZonaGeograficaId })
            .Select(g => new SelectListItem
            {
              Value = g.Key.ZonaGeograficaId.ToString(),
              Text = g.Key.ZonaGeografica
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
        throw;
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
    /// la lista de CantidadFechas (TR y/o TO) dentro de la ventana.
    /// Si usarCantidadFuturo=true, se usa el campo CantidadFuturo (tabla FUTURO);
    /// en caso contrario, se usa Cantidad (tabla CONTRACTUAL).
    /// </summary>
    private static List<SolicitudTurnoGrupoView> GroupBySolicitud(
      IEnumerable<ShiftRequestPendingViewModel> items,
      List<SolicitudTurnoDetalleGrupoView> ventana,
      bool usarCantidadFuturo)
    {
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

          if (usarCantidadFuturo)
          {
            detalles[fechaKey].CantidadFuturo += item.CantidadFuturo > 0 ? item.CantidadFuturo : 1;
          }
          else
          {
            detalles[fechaKey].Cantidad += item.Cantidad;
          }
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
          EstadoBadge = first.GetEstadoBadgeClass(),
          EstadoLabel = first.GetEstadoBadgeLabel(),
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
