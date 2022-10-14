using CuposCorretajeWeb.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CuposCorretajeWeb.Models
{
    public class ServicioCentro
    {
        public async Task<IList<Centro>> GetCentros()
        {
            using (WebServiceSILRespository repo = new WebServiceSILRespository())
            {
                var Centros = await repo.RequestGetAndDeserializeAsync<IList<Centro>>("CentroPorPuerto", "Centros");
                return Centros;
            }
        }
    }
}