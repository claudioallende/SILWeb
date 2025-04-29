using CuposCorretajeWeb.Models.Data;
using CuposCorretajeWeb.Models.Solicitudes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
//using System.Web.Http;
using System.Web.Mvc;

namespace CuposCorretajeWeb.Controllers
{
  [Authorize]
  public class SolicitudesController : Controller
  {
    // GET: Solicitudes
    public ActionResult Index()
    {
      return View(new IndexModel());
    }

    [HttpGet]
    public async Task<ActionResult> GetAllPendingShiftRequests()
    {
      try
      {
        SILSolicitudDeTurnosFilterViewModel filterSolicitud = new SILSolicitudDeTurnosFilterViewModel
        {
          Centros = new List<string> {"ROS", "BSAS", "CBA"},
          Dias = 7
        };
        IEnumerable<ShiftRequestPendingViewModel> shiftRequestPendingViewModel = new List<ShiftRequestPendingViewModel>();
        WebServiceSILRespository repo = new WebServiceSILRespository();
        shiftRequestPendingViewModel = await repo.RequestSILDataPostAndDeserializeAsync<IEnumerable<ShiftRequestPendingViewModel>>("ShiftRequest", "GetAllPendingShiftRequestAsync", filterSolicitud);
        string jsonResult = JsonConvert.SerializeObject(shiftRequestPendingViewModel);
        return Content(jsonResult, "application/json");
      }
      catch (Exception ex)
      {
        throw;
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
        cuposDisponible = await repo.RequestSILDataPostAndDeserializeAsync<ICollection<CuposDisponibleViewModel>>("CuposDisponibles","Disponibles",filterCuposDisponible);

        List<SelectListItem> compradoresDistinct = cuposDisponible
            .GroupBy(x => new { x.CuentaComprador, x.NombreComprador }) // Agrupás por ambos campos
            .Select(g => new SelectListItem
            {
              Value = g.Key.CuentaComprador.ToString(),
              Text = $"{g.Key.CuentaComprador} - {g.Key.NombreComprador}"
            })
            .OrderBy(o => o.Value)
            .ToList();

        // Crea un diccionario para el mapeo
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
        return View(solicitud); // Podés pasarlo al modelo
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }
    [HttpPost]
    public ActionResult AltaSolicitud([System.Web.Http.FromBody] SolicitudViewModel solicitud)
    {
      try
      {
        // Guardás en TempData si necesitás pasar el objeto a la próxima vista
        TempData["Solicitud"] = solicitud;

        // Retornás la URL a la vista que querés mostrar
        return Json(new { success = true, redirectUrl = Url.Action("AltaSolicitud") });
      }
      catch (Exception ex)
      {
        return Json(new { success = false, message = ex.Message });
      }
    }
  }
}