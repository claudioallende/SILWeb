using CuposCorretajeWeb.Models.AtributosValidacion;
using CuposCorretajeWeb.Models.Auditoria;
using CuposCorretajeWeb.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CuposCorretajeWeb.Controllers
{
    [ClaimsAuthorize("AccesoAuditoriaCuposCorretaje", "True")]
    public class AuditoriaController : Controller
    {
        // GET: Auditoria
        public ActionResult Index()
        {
            return View("Auditoria", new AuditoriaCuposCorreViewModel());
        }

        public async Task<JsonResult> ObtenerAuditoriaCuposCorre(AuditoriaCuposCorreViewModel filtro)
        {
            using (WebServiceSILRespository repo = new WebServiceSILRespository())
            {
                return Json(await repo.RequestPostAndDeserializeAsync<IList<AuditoriaCuposCorre>>("Auditoria", "GetAuditoriaCuposCorre", filtro));
            }
        }

        public async Task<JsonResult> GetAuditoriaCupo(long IdCupo)
        {
            using (WebServiceSILRespository repo = new WebServiceSILRespository())
            {
                return Json(await repo.RequestGetAndDeserializeAsync<IList<AuditoriaCuposCorre>>("Auditoria", string.Format("GetAuditoriaCupo/{0}", IdCupo)));
            }
        }
    }
}