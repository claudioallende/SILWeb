using CuposCorretajeWeb.Models;
using CuposCorretajeWeb.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CuposCorretajeWeb.Controllers
{
    public class GranoController : Controller
    {
        public JsonResult GranosStop()
        {
            try
            {
                using (WebServiceSILRespository repo = new WebServiceSILRespository())
                {
                    //repo.RequestPostAndDeserializeAsync<Grano>("", "", new { });
                    return Json(new List<Grano>() { new Grano() { CodigoGrano = 10, Nombre = "Trigo", Id = "10" } });
                }
            } catch {
                return null;
            }
        }
    }
}