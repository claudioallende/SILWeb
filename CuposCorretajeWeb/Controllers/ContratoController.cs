using CuposCorretajeWeb.Models.Contratos;
using CuposCorretajeWeb.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CuposCorretajeWeb.Controllers
{
    public class ContratoController : Controller
    {
        public async Task<PartialViewResult> GetDetallePendienteAplicar(long Compcta, long Vendcta, int Producto, long Ctadestino,
            string Cosecha, string Codcentro)
        {
            using (WebServiceSILRespository repo = new WebServiceSILRespository())
            {
                IList<Vista_CuposMpeCorreV3> model = 
                    await repo.RequestGetAndDeserializeAsync<IList<Vista_CuposMpeCorreV3>>(
                        "Contrato", 
                        string.Format("GetDetallePendienteAplicar/?Compcta={0}&Vendcta={1}&Producto={2}&Ctadestino={3}&Cosecha={4}&Codcentro={5}", Compcta, Vendcta, Producto, Ctadestino,
                            Cosecha, Codcentro)
                    );
                return PartialView("~/Views/Contratos/_DetallePendienteAplicarPartial.cshtml", model);
            }
        }

        //[Obsolete("Lo sacamos de la vista")]
        //public PartialViewResult GetMercaderiaSinDestino(int Producto, long Compcta, string Centro, long Vendcta)
        //{
        //    return PartialView("~/Views/Contratos/_MercaderiaSinDestinoPartial.cshtml", servicio.ObtenerMercaderiaSinDestino(Producto, Compcta, Centro, Vendcta));
        //}
    }
}
