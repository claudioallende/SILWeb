using CuposCorretajeWeb.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CuposCorretajeWeb.Models
{
    public class ServicioGrano
    {
        public async Task<IList<RelacionGranoStop>> GetEquivalenciasSilDeGranoStop(int CodigoStop)
        {
            using (WebServiceSILRespository repo = new WebServiceSILRespository())
            {
                var Granos = await repo.RequestPostAndDeserializeAsync<IList<RelacionGranoStop>>("RelacionGranoSILGranoSTOP", "GetRelaciones", new
                {
                    NroGranoSTOP = CodigoStop
                });
                return Granos;
            }
        }

        public async Task<IList<GranoStop>> GetGranosStop()
        {
            using (WebServiceSILRespository repo = new WebServiceSILRespository())
            {
                var Granos = await repo.RequestPostAndDeserializeAsync<IList<GranoStop>>("GranosSTOP", "GetGrano", null);
                return Granos;
            }
        }

        public async Task<IList<Grano>> GetGranosSil()
        {
            using (WebServiceSILRespository repo = new WebServiceSILRespository())
            {
                var Granos = await repo.RequestGetAndDeserializeAsync<IList<Grano>>("Cliente", "GetGranos");
                return Granos;
            }
        }
    }
}