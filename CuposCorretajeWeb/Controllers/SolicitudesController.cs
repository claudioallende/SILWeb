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
          Centros = new List<string> {"ROS", "BSAS"},
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

    [HttpPost]
    public ActionResult AltaSolicitud([System.Web.Http.FromBody] SolicitudViewModel solicitud)
    {
      try
      {
        return RedirectToAction("AltaSolicitud");
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }
  }
}