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
        var repo = new WebServiceSILRespository();
        shiftRequestPendingViewModel = await repo.RequestSILDataPostAndDeserializeAsync<IEnumerable<ShiftRequestPendingViewModel>>("ShiftRequest", "GetAllPendingShiftRequestAsync", filterSolicitud);
        var jsonResult = JsonConvert.SerializeObject(shiftRequestPendingViewModel);
        return Content(jsonResult, "application/json");
      }
      catch (Exception ex)
      {
        throw;
      }
    }
    [HttpGet]
    public ActionResult AltaSolicitud()
    {
      try
      {
        var solicitud = TempData["Solicitud"] as SolicitudViewModel;
        if (solicitud == null)
        {
          return RedirectToAction("Index");
        }
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