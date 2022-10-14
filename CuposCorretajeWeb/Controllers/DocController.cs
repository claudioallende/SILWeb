using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CuposCorretajeWeb.Controllers
{
    [Authorize]
    public class DocController : Controller
    {
        public ActionResult GetPdf(string filename)
        {
            return File(this.Server.MapPath("~/Manuales/") + filename, "application/pdf", filename);
        }
    }
}