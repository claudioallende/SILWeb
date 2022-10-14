using CuposCorretajeWeb.Models;
using CuposCorretajeWeb.Models.Data;
using CuposCorretajeWeb.Models.Error;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CuposCorretajeWeb.Controllers
{
    [Authorize]
    public class CuentaController : Controller
    {
        public async Task<JsonResult> GetCuentaFromNumeroCuentaComprador(string id)
        {
            if (!string.IsNullOrEmpty(id)) { 
                using (WebServiceSILRespository repo = new WebServiceSILRespository())
                {
                    return Json(await repo.RequestGetAndDeserializeAsync<IList<CuentaViewModel>>("Cuenta", string.Format("GetCuentaFromNumeroCuentaComprador/{0}", id)));
                }
            }
            return Json("Parámetro vacío");
        }

        public async Task<JsonResult> GetCuentaFromNumeroCuentaVendedor(string Cuenta)
        {
            if (!string.IsNullOrEmpty(Cuenta))
            {
                using (WebServiceSILRespository repo = new WebServiceSILRespository())
                {
                    return Json(await repo.RequestGetAndDeserializeAsync<IList<CuentaViewModel>>("Cuenta", string.Format("GetCuentaFromNumeroCuentaVendedor/{0}", Cuenta)));
                }
            }
            return Json("Parámetro vacío");
        }

        public async Task<JsonResult> GetCuentaFromNombreCuentaVendedor(string Nombre)
        {
            if (!string.IsNullOrEmpty(Nombre))
            {
                using (WebServiceSILRespository repo = new WebServiceSILRespository())
                {
                    return Json(await repo.RequestGetAndDeserializeAsync<IList<CuentaViewModel>>("Cuenta", string.Format("GetCuentaFromNombreCuentaVendedor/{0}", Nombre)));
                }
            }
            return Json("Parámetro vacío");
        }

        public async Task<JsonResult> GetCuentaFromCuitVendedor(string Cuit)
        {
            if (!string.IsNullOrEmpty(Cuit))
            {
                using (WebServiceSILRespository repo = new WebServiceSILRespository())
                {
                    return Json(await repo.RequestGetAndDeserializeAsync<IList<CuentaViewModel>>("Cuenta", string.Format("GetCuentaFromCuitVendedor/{0}", Cuit)));
                }
            }
            return Json("Parámetro vacío");
        }

        public async Task<JsonResult> GetCuentaFromCuit(string Id)
        {
            if (!string.IsNullOrEmpty(Id))
            {
                using (WebServiceSILRespository repo = new WebServiceSILRespository())
                {
                    return Json(await repo.RequestGetAndDeserializeAsync<IList<CuentaViewModel>>("Cuenta", string.Format("GetCuentaFromCuit/{0}", Id)));
                }
            }
            return Json("Parámetro vacío");
        }

        public async Task<JsonResult> GetCuentaFromNombre(string Nombre)
        {
            if (!string.IsNullOrEmpty(Nombre))
            {
                using (WebServiceSILRespository repo = new WebServiceSILRespository())
                {
                    return Json(await repo.RequestGetAndDeserializeAsync<IList<CuentaViewModel>>("Cuenta", string.Format("GetCuentaFromNombre/{0}", Nombre)));
                }
            }
            return Json("Parámetro vacío");
        }
        
        public async Task<JsonResult> GetCuentaFromNombreCompradorOrNombreCuit(string Nombre)
        {
            return Json("");
        }

        public async Task<JsonResult> GetCuitFromNombre(string Nombre)
        {
            if (!string.IsNullOrEmpty(Nombre))
            {
                using (WebServiceSILRespository repo = new WebServiceSILRespository())
                {
                    return Json(await repo.RequestGetAndDeserializeAsync<IList<CuentaViewModel>>("Cuenta", string.Format("GetCuitFromNombre/{0}", Nombre)));
                }
            }
            return Json("Parámetro vacío");
        }

        public async Task<JsonResult> GetCuposCuitFromNroCuentaOrNombre(string Texto)
        {
            if (!string.IsNullOrEmpty(Texto))
            {
                using (WebServiceSILRespository repo = new WebServiceSILRespository())
                {
                    return Json(await repo.RequestPostAndDeserializeAsync<IList<CuentaViewModel>>("Cuenta", "GetCuposCuitFromNroCuentaOrNombre", Texto));
                }
            }
            return Json("Parámetro vacío");
        }

        public async Task<JsonResult> GetVendedorFromNroCuentaOrNombre(string Texto)
        {
            if (!string.IsNullOrEmpty(Texto))
            {
                using (WebServiceSILRespository repo = new WebServiceSILRespository())
                {
                    return Json(await repo.RequestGetAndDeserializeAsync<IList<CuentaViewModel>>("Cuenta", string.Format("GetVendedorFromNroCuentaOrNombre/{0}", Texto)));
                }
            }
            return Json("Parámetro vacío");
        }

        public async Task<JsonResult> GetCompradorFromNroCuentaOrNombre(string Texto)
        {
            if (!string.IsNullOrEmpty(Texto))
            {
                using (WebServiceSILRespository repo = new WebServiceSILRespository())
                {
                    var result = await repo.RequestGetAndDeserializeAsync<IList<CuentaViewModel>>("Cuenta", string.Format("GetCompradorFromNroCuentaOrNombre/{0}", Texto));
                    return Json(result);
                }
            }
            return Json("Parámetro vacío");
        }

        public async Task<JsonResult> GetPuertoFromNroCuentaOrNombre(string Texto)
        {
            if (!string.IsNullOrEmpty(Texto))
            {
                using (WebServiceSILRespository repo = new WebServiceSILRespository())
                {
                    return Json(await repo.RequestPostAndDeserializeAsync<IList<CuentaViewModel>>("Cuenta", "GetPuertoFromNroCuentaOrNombre", Texto));
                }
            }
            return Json("Parámetro vacío");
        }

        public async Task<JsonResult> GetPuertoStopFromNroCuentaOrNombre(string Texto)
        {
            if (!string.IsNullOrEmpty(Texto))
            {
                using (WebServiceSILRespository repo = new WebServiceSILRespository())
                {
                    return Json(await repo.RequestPostAndDeserializeAsync<IList<CuentaViewModel>>("Cuenta", "GetPuertoStopFromNroCuentaOrNombre", Texto));
                }
            }
            return Json("Parámetro vacío");
        }

        /// <summary>
        /// Busca la relación existente para el param que le paso
        /// </summary>
        /// <param name="Terminal">Si es numérico buscar por cuenta, sino por nombre</param>
        /// <returns></returns>
        public async Task<JsonResult> GetRelacionCentroPuerto(string Terminal)
        {
            if (!string.IsNullOrEmpty(Terminal))
            {
                using (WebServiceSILRespository repo = new WebServiceSILRespository())
                {
                    long IdTerminal = 0;
                    long.TryParse(Terminal, out IdTerminal);
                    if (IdTerminal != 0)
                        return Json(await repo.RequestPostAndDeserializeAsync<IList<CentroPorPuertoDTO>>("CentroPorPuerto", "Relaciones", new CentroPorPuertoDTO { IdTerminal = IdTerminal }));
                    else
                        return Json(await repo.RequestPostAndDeserializeAsync<IList<CentroPorPuertoDTO>>("CentroPorPuerto", "Relaciones", new CentroPorPuertoDTO { NombreTerminal = Terminal }));
                }
            }
            return Json("Parámetro vacío");
        }

        public async Task<JsonResult> Puerto(string Id = "")
        {
            if (Id.Trim() != "")
            {
                using (WebServiceSILRespository repo = new WebServiceSILRespository())
                {
                    var Puertos = await repo.RequestGetAndDeserializeAsync<IList<Puerto>>("CentroPorPuerto", string.Format("Puertos/{0}", Id));
                    //La ZZZZZ es para que lo mande al final en caso de que este vacio
                    return Json(new { data = Puertos.OrderBy(x => x.IdTerminal.Trim() == string.Empty ? "ZZZZZZZ" : x.IdTerminal) }); 
                }
            }
            return Json(new { data = new List<Puerto>() });
        }

        public async Task<JsonResult> Centro(string Id = "")
        {
            if (Id.Trim() != "")
            {
                using (WebServiceSILRespository repo = new WebServiceSILRespository())
                {
                    return Json( new { data = await repo.RequestGetAndDeserializeAsync<IList<Centro>>("CentroPorPuerto", string.Format("Centros/{0}", Id)) });
                }
            }
            return Json(new { data = new List<Centro>() });
        }

        public async Task<JsonResult> GetPuertos()
        {
            using (WebServiceSILRespository repo = new WebServiceSILRespository())
            {
                return Json(new { data = await repo.RequestGetAndDeserializeAsync<IList<Puerto>>("Cuenta", "GetPuertos") });
            }
        }
    }
}
