using CuposCorretajeWeb.Models;
using CuposCorretajeWeb.Models.AtributosValidacion;
using CuposCorretajeWeb.Models.Configuracion;
using CuposCorretajeWeb.Models.Data;
using CuposCorretajeWeb.Models.Email;
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
  public class ConfiguracionController : Controller
  {
    private readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
    // GET: Configuracion
    [ClaimsAuthorize("AccesoConfiguracionCuposCorretaje", "True")]
    public async Task<ActionResult> Index()
    {
      PanelViewModel model = new PanelViewModel();
      using (WebServiceSILRespository repo = new WebServiceSILRespository())
      {
        model = await repo.RequestPostAndDeserializeAsync<PanelViewModel>("Configuracion", "Index", model);
      }
      return View(model);
    }

    [HttpPost]
    [ClaimsAuthorize("AccesoConfiguracionCuposCorretaje", "True")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Index(FormCollection collection, PanelViewModel model)
    {
      using (WebServiceSILRespository repo = new WebServiceSILRespository())
      {
        model = await repo.RequestPostAndDeserializeAsync<PanelViewModel>("Configuracion", "Guardar", model);
      }
      return View(model);
    }

    public ActionResult Email()
    {
      return View();
    }

    [HttpPost]
    [ClaimsAuthorize("AccesoConfiguracionCYOeEmailDeCuentasCuposCorretaje", "True")]
    public async Task<ActionResult> Email(EmailViewModel form)
    {
      EmailViewModel model = new EmailViewModel();
      using (WebServiceSILRespository repo = new WebServiceSILRespository())
      {
        model = await repo.RequestPostAndDeserializeAsync<EmailViewModel>("Configuracion", "GuardarEmail", form);
      }
      return View(model);
    }

    public async Task<JsonResult> GetEmailByCuit(string cuit)
    {
      using (WebServiceSILRespository repo = new WebServiceSILRespository())
      {
        return Json(await repo.RequestGetAndDeserializeAsync<string>("Configuracion", string.Format("GetEmailByCuit/{0}", cuit)));
      }
    }

    public async Task<JsonResult> GetEmailByCuenta(string Cuenta)
    {
      using (WebServiceSILRespository repo = new WebServiceSILRespository())
      {
        return Json(await repo.RequestGetAndDeserializeAsync<string>("Configuracion", string.Format("GetEmailByCuenta/{0}", Cuenta)));
      }
    }

    public ActionResult CuentaCYO()
    {
      return View();
    }

    [HttpPost]
    [ClaimsAuthorize("AccesoConfiguracionCYOeEmailDeCuentasCuposCorretaje", "True")]
    public async Task<ActionResult> CuentaCYO(FormCollection form, CuentaCYOViewModel model)
    {
      using (WebServiceSILRespository repo = new WebServiceSILRespository())
      {
        return View(await repo.RequestPostAndDeserializeAsync<CuentaCYOViewModel>("Configuracion", "GuardarCuentaCYO", model));
      }
    }

    public async Task<JsonResult> GetCuentaYOrdenByNumeroCuenta(string NumeroCuenta)
    {
      using (WebServiceSILRespository repo = new WebServiceSILRespository())
      {
        return Json(await repo.RequestGetAndDeserializeAsync<bool>("Configuracion", string.Format("GetCuentaYOrdenByNumeroCuenta/{0}", NumeroCuenta)));
      }
    }

    public async Task<JsonResult> GetCuentaYOrdenByNumeroCuentaAndCuit(string NumeroCuenta, string Cuit)
    {
      using (WebServiceSILRespository repo = new WebServiceSILRespository())
      {
        return Json(await repo.RequestGetAndDeserializeAsync<bool>("Configuracion", string.Format("GetCuentaYOrdenByNumeroCuentaAndCuit/{0}/{1}", Cuit, NumeroCuenta)));
      }
    }

    public async Task<ActionResult> CentroPuertoStop()
    {
      CentroPorPuertoViewModel model = new CentroPorPuertoViewModel();
      using (WebServiceSILRespository repo = new WebServiceSILRespository())
      {
        model.Nuevo = new NuevoCentroPorPuertoViewModel();
        model.ListaCentrosPorPuerto = await repo.RequestPostAndDeserializeAsync<IList<CentroPorPuertoDTO>>("CentroPorPuerto", "Relaciones", null);
        await model.Nuevo.SetCentros();
      }
      return View(model);
    }

    public async Task<JsonResult> NuevaRelacionCentroPuerto(CentroPorPuertoDTO Relacion)
    {
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          var result = await repo.RequestPostAndDeserializeAsync<CentroPorPuertoDTO>("CentroPorPuerto", "SaveRelacion", Relacion);
          return Json(new { data = result, error = false });
        }
      }
      catch (ApiException e)
      {
        return Json(new { data = e.Message.Replace("\"", string.Empty), error = true });
      }
    }

    public async Task<JsonResult> ModificarRelacionCentroPuerto(CentroPorPuertoDTO Relacion)
    {
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          var result = await repo.RequestPostAndDeserializeAsync<CentroPorPuertoDTO>("CentroPorPuerto", "UpdateRelacion", Relacion);
          if (result != null)
            return Json(new { result = result, data = Relacion, error = false });
          else
            return Json(new { result = result, data = new object(), error = false });
        }
      }
      catch (ApiException e)
      {
        return Json(new { data = e.Message, error = true });
      }
    }

    public async Task<JsonResult> EliminarRelacionCentroPuerto(CentroPorPuertoDTO Relacion)
    {
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          return Json(new { result = await repo.RequestPostAndDeserializeAsync<bool>("CentroPorPuerto", "DeleteRelacion", Relacion), data = Relacion, error = false });
        }
      }
      catch (ApiException e)
      {
        return Json(new { data = e.Message, error = true });
      }
    }

    //Granos Stop
    public async Task<ActionResult> GetGranoStop(string Id)
    {
      long Codigo = 0;
      long.TryParse(Id, out Codigo);
      string Nombre = Codigo == 0 ? Id : "";
      using (WebServiceSILRespository repo = new WebServiceSILRespository())
      {
        return Json(new { data = await repo.RequestPostAndDeserializeAsync<IList<GranoStop>>("GranosSTOP", "GetGrano", new GranoStop() { NroGrano = Codigo, NombreGrano = Nombre }) });
      }
    }

    public async Task<ActionResult> GranoStop()
    {
      GranoStopViewModel model = new GranoStopViewModel();
      ServicioGrano ServicioGrano = new ServicioGrano();
      using (WebServiceSILRespository repo = new WebServiceSILRespository())
      {
        model.ListaGranosStop = await repo.RequestPostAndDeserializeAsync<IList<GranoStop>>("GranosSTOP", "GetGrano", null);
        model.NuevoGranoStop = new GranoStop();
      }
      return View(model);
    }

    public async Task<ActionResult> NuevoGranoStop(GranoStop Grano)
    {
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          var result = await repo.RequestPostAndDeserializeAsync<GranoStop>("GranosSTOP", "AddGrano", Grano);
          return Json(new { data = result, error = false });
        }
      }
      catch (ApiException e)
      {
        return Json(new { data = e.Message.Replace("\"", string.Empty), error = true });
      }
    }

    public async Task<ActionResult> ModificarGranoStop(GranoStop Grano)
    {
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          var result = await repo.RequestPostAndDeserializeAsync<GranoStop>("GranosSTOP", "UpdateGrano", Grano);
          if (result != null)
            return Json(new { result = result, data = Grano, error = false });
          else
            return Json(new { result = result, data = new object(), error = false });
        }
      }
      catch (ApiException e)
      {
        return Json(new { data = e.Message, error = true });
      }
    }

    public async Task<ActionResult> EliminarGranoStop(GranoStop Grano)
    {
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          return Json(new { result = await repo.RequestPostAndDeserializeAsync<bool>("GranosSTOP", "DeleteGrano", Grano), data = Grano, error = false });
        }
      }
      catch (ApiException e)
      {
        return Json(new { data = e.Message, error = true });
      }
    }

    //Relaciones Grano Stop
    public async Task<ActionResult> RelacionGranoStop()
    {
      RelacionGranoStopViewModel model = new RelacionGranoStopViewModel();
      ServicioGrano ServicioGrano = new ServicioGrano();
      using (WebServiceSILRespository repo = new WebServiceSILRespository())
      {
        var ListaRelaciones = repo.RequestPostAndDeserializeAsync<IList<RelacionGranoStop>>("RelacionGranoSILGranoSTOP", "GetRelaciones", null);
        var GranosStop = ServicioGrano.GetGranosStop();
        var GranosSil = ServicioGrano.GetGranosSil();
        model.ListaRelacionesGranosStop = await ListaRelaciones;
        model.NuevaRelacion.GranosStop = (await GranosStop).Select(x => new SelectListItem() { Value = x.NroGrano.ToString(), Text = x.NombreGrano }).ToList();
        model.NuevaRelacion.GranosSil = (await GranosSil).Select(x => new SelectListItem() { Value = x.CodigoGrano.ToString(), Text = x.Nombre }).ToList();
      }
      return View(model);
    }

    public async Task<JsonResult> NuevaRelacionGranoStop(RelacionGranoStop Relacion)
    {
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          var result = await repo.RequestPostAndDeserializeAsync<RelacionGranoStop>("RelacionGranoSILGranoSTOP", "AddRelacion", Relacion);
          return Json(new { data = result, error = false });
        }
      }
      catch (ApiException e)
      {
        return Json(new { data = e.Message.Replace("\"", string.Empty), error = true });
      }
    }

    public async Task<JsonResult> ModificarRelacionGranoStop(RelacionGranoStop Relacion)
    {
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          var result = await repo.RequestPostAndDeserializeAsync<RelacionGranoStop>("RelacionGranoSILGranoSTOP", "UpdateRelacion", Relacion);
          if (result != null)
            return Json(new { result = result, data = Relacion, error = false });
          else
            return Json(new { result = result, data = new object(), error = false });
        }
      }
      catch (ApiException e)
      {
        return Json(new { data = e.Message, error = true });
      }
    }

    public async Task<JsonResult> EliminarRelacionGranoStop(RelacionGranoStop Relacion)
    {
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          return Json(new { result = await repo.RequestPostAndDeserializeAsync<bool>("RelacionGranoSILGranoSTOP", "DeleteRelacion", Relacion), data = Relacion, error = false });
        }
      }
      catch (ApiException e)
      {
        return Json(new { data = e.Message, error = true });
      }
    }

    //Puerto
    public async Task<JsonResult> GetPuertoStop(string Id)
    {
      long Codigo = 0;
      long.TryParse(Id, out Codigo);
      string Nombre = Codigo == 0 ? Id : "";
      using (WebServiceSILRespository repo = new WebServiceSILRespository())
      {
        return Json(new { data = await repo.RequestPostAndDeserializeAsync<IList<PuertoStop>>("PuertosSTOP", "GetPuerto", new PuertoStop() { NroPuerto = Codigo, NombrePuerto = Nombre }) });
      }
    }

    public async Task<ActionResult> PuertoStop()
    {
      PuertoStopViewModel model = new PuertoStopViewModel();
      ServicioPuerto ServicioPuerto = new ServicioPuerto();
      using (WebServiceSILRespository repo = new WebServiceSILRespository())
      {
        model.ListaPuertosStop = await repo.RequestPostAndDeserializeAsync<IList<PuertoStop>>("PuertosSTOP", "GetPuerto", null);
        model.NuevoPuertoStop = new PuertoStop();
      }
      return View(model);
    }

    public async Task<JsonResult> NuevoPuertoStop(PuertoStop Puerto)
    {
      log.Debug($"Nuevo Puerto Stop {Puerto.NombrePuerto}");
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          var result = await repo.RequestPostAndDeserializeAsync<PuertoStop>("PuertosSTOP", "AddPuerto", Puerto);
          return Json(new { data = result, error = false });
        }
      }
      catch (ApiException e)
      {
        return Json(new { data = e.Message.Replace("\"", string.Empty), error = true });
      }
      catch (Exception e)
      {
        log.Error(e);
        return Json(new { data = "Error", error = true });
      }
    }

    public async Task<JsonResult> ModificarPuertoStop(PuertoStop Puerto)
    {
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          var result = await repo.RequestPostAndDeserializeAsync<PuertoStop>("PuertosSTOP", "UpdatePuerto", Puerto);
          if (result != null)
            return Json(new { result = result, data = Puerto, error = false });
          else
            return Json(new { result = result, data = new object(), error = false });
        }
      }
      catch (ApiException e)
      {
        return Json(new { data = e.Message, error = true });
      }
    }

    public async Task<JsonResult> EliminarPuertoStop(PuertoStop Puerto)
    {
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          return Json(new { result = await repo.RequestPostAndDeserializeAsync<bool>("PuertosSTOP", "DeletePuerto", Puerto), data = Puerto, error = false });
        }
      }
      catch (ApiException e)
      {
        return Json(new { data = e.Message, error = true });
      }
    }

    //Relaciones Puerto Stop
    public async Task<ActionResult> RelacionPuertoStop()
    {
      RelacionPuertoStopViewModel model = new RelacionPuertoStopViewModel();
      ServicioPuerto ServicioPuerto = new ServicioPuerto();
      using (WebServiceSILRespository repo = new WebServiceSILRespository())
      {
        var ListaRelaciones = repo.RequestPostAndDeserializeAsync<IList<RelacionPuertoStop>>("RelacionPuertoSILPuertoSTOP", "GetRelaciones", null);
        //var PuertosStop = ServicioPuerto.GetPuertosStop();
        //var PuertosSil = ServicioPuerto.GetPuertosSil();
        model.ListaRelacionesPuertosStop = await ListaRelaciones;
        //var ListPuertoSil = await PuertosSil;
        //model.NuevaRelacion.PuertosSil = new List<SelectListItem>();
        //model.NuevaRelacion.PuertosStop = new List<SelectListItem>();
        //model.NuevaRelacion.PuertosSil = ListPuertoSil.Select(x => new SelectListItem() { Value = x.Cuenta.ToString(), Text = x.Nombre }).ToList();
      }
      return View(model);
    }

    public async Task<JsonResult> NuevaRelacionPuertoStop(RelacionPuertoStop Relacion)
    {
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          var result = await repo.RequestPostAndDeserializeAsync<RelacionPuertoStop>("RelacionPuertoSILPuertoSTOP", "AddRelacion", Relacion);
          return Json(new { data = result, error = false });
        }
      }
      catch (ApiException e)
      {
        return Json(new { data = e.Message.Replace("\"", string.Empty), error = true });
      }
    }

    public async Task<JsonResult> ModificarRelacionPuertoStop(RelacionPuertoStop Relacion)
    {
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          var result = await repo.RequestPostAndDeserializeAsync<RelacionPuertoStop>("RelacionPuertoSILPuertoSTOP", "UpdateRelacion", Relacion);
          if (result != null)
            return Json(new { result = result, data = Relacion, error = false });
          else
            return Json(new { result = result, data = new object(), error = false });
        }
      }
      catch (ApiException e)
      {
        return Json(new { data = e.Message, error = true });
      }
    }

    public async Task<JsonResult> EliminarRelacionPuertoStop(RelacionPuertoStop Relacion)
    {
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          return Json(new { result = await repo.RequestPostAndDeserializeAsync<bool>("RelacionPuertoSILPuertoSTOP", "DeleteRelacion", Relacion), data = Relacion, error = false });
        }
      }
      catch (ApiException e)
      {
        return Json(new { data = e.Message, error = true });
      }
    }
  }
}
