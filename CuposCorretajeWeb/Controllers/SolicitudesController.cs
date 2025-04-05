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
    //[HttpGet]
    //public async Task<JsonResult> GetAllShiftRequests()
    //{
    //  try
    //  {
    //    IEnumerable<SolicitudTurnoGrupoView> result;
    //    var repo = new WebServiceSILRespository();
    //    result = (await repo.RequestGetAndDeserializeAsync<IEnumerable<SolicitudTurnoGrupoView>>("SolicitudesTurnos", "Grupo")) ?? new List<SolicitudTurnoGrupoView>();

    //    //var result = (await repo.RequestGetAndDeserializeAsync<IEnumerable<SolicitudTurnoGrupoView>>("SolicitudesTurnos", "Grupo"))
    //    //     ?.Select(s => new {
    //    //       s.NombreVendedor,
    //    //       s.NombreComprador,
    //    //       s.NombreDestino,
    //    //       CantidadFechas = s.CantidadFechas.ToDictionary(cf => DateTime.Parse(cf.Fecha).ToString("dd/MM"), cf => cf.Cantidad)
    //    //     }) ?? Enumerable.Empty<object>();

    //    return Json(new { data = result }, JsonRequestBehavior.AllowGet);
    //  }
    //  catch (Exception ex)
    //  {
    //    return Json(new { error = "Error al obtener los datos", details = ex.Message }, JsonRequestBehavior.AllowGet);
    //  }
    //}

    //[HttpGet]
    //public async Task<JsonResult> GetAllFutureShiftRequests()
    //{
    //  IEnumerable<SolicitudTurnoGrupoView> result;
    //  using (WebServiceSILRespository repo = new WebServiceSILRespository())
    //  {
    //    result = (await repo.RequestGetAndDeserializeAsync<IEnumerable<SolicitudTurnoGrupoView>>("SolicitudesTurnos", "Grupo?futuro=true")) ?? new List<SolicitudTurnoGrupoView>();
    //  }
    //  return Json(new { data = result }, JsonRequestBehavior.AllowGet);
    //}
  }
}