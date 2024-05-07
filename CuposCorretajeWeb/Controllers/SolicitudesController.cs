using CuposCorretajeWeb.Models.Data;
using CuposCorretajeWeb.Models.Solicitudes;
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
    public async Task<JsonResult> GetAllShiftRequests()
    {
      IEnumerable<SolicitudTurnoGrupoView> result;
      using (WebServiceSILRespository repo = new WebServiceSILRespository())
      {
        result = (await repo.RequestGetAndDeserializeAsync<IEnumerable<SolicitudTurnoGrupoView>>("SolicitudesTurnos", "Grupo")) ?? new List<SolicitudTurnoGrupoView>();
      }
      return Json(new { data = result }, JsonRequestBehavior.AllowGet);
    }

    [HttpGet]
    public async Task<JsonResult> GetAllFutureShiftRequests()
    {
      IEnumerable<SolicitudTurnoGrupoView> result;
      using (WebServiceSILRespository repo = new WebServiceSILRespository())
      {
        result = (await repo.RequestGetAndDeserializeAsync<IEnumerable<SolicitudTurnoGrupoView>>("SolicitudesTurnos", "Grupo?futuro=true")) ?? new List<SolicitudTurnoGrupoView>();
      }
      return Json(new { data = result }, JsonRequestBehavior.AllowGet);
    }
  }
}