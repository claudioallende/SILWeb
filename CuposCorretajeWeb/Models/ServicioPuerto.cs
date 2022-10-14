using CuposCorretajeWeb.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CuposCorretajeWeb.Models
{
    public class ServicioPuerto
    {
        public async Task<IList<RelacionPuertoStop>> GetEquivalenciasSilDePuertoStop(string CodigoStop)
        {
            using (WebServiceSILRespository repo = new WebServiceSILRespository())
            {
                var Puertos = await repo.RequestPostAndDeserializeAsync<IList<RelacionPuertoStop>>("RelacionPuertoSILPuertoSTOP", "GetRelaciones", new
                {
                    NroPuertoSTOP = CodigoStop
                });
                return Puertos;
            }
        }

        public async Task<IList<PuertoStop>> GetPuertosStop()
        {
            using (WebServiceSILRespository repo = new WebServiceSILRespository())
            {
                var Puertos = await repo.RequestPostAndDeserializeAsync<IList<PuertoStop>>("PuertosSTOP", "GetPuerto", null);
                return Puertos;
            }
        }

        public async Task<IList<Puerto>> GetPuertosSil()
        {
            using (WebServiceSILRespository repo = new WebServiceSILRespository())
            {
                var Puertos = await repo.RequestGetAndDeserializeAsync<IList<Puerto>>("Cuenta", "GetPuertos");
                return Puertos;
            }
        }
    }
}