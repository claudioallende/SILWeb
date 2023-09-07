using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CuposCorretajeWeb.Models;
using CuposCorretajeWeb.Models.Email;
using CuposCorretajeWeb.Models.Filtro;
using System.Security.Claims;
using CuposCorretajeWeb.Models.Auditoria;
using CuposCorretajeWeb.Models.Data;
using System.Threading.Tasks;
using CuposCorretajeWeb.Models.Error;

namespace CuposCorretajeWeb.Controllers
{
  [Authorize]
  public class CuposController : Controller
  {
    // GET: Cupos
    public async Task<ActionResult> Index(IndexCupoViewModel Model)
    {
      ViewBag.ColumnasOcultar = Model.OcultarColumnas();
      using (WebServiceSILRespository repo = new WebServiceSILRespository())
      {
        Model = await repo.RequestPostAndDeserializeAsync<IndexCupoViewModel>("Cupos", "Index", Model);
      }
      return View(Model);
    }

    // GET: Cupos/Details/5
    public async Task<ActionResult> Detalle(string id, string centroorigen, string centrodistribucion, bool cyo = false)
    {
      using (WebServiceSILRespository repo = new WebServiceSILRespository())
      {
        DetalleCupoViewModel model = await repo.RequestGetAndDeserializeAsync<DetalleCupoViewModel>("Cupos", string.Format("Detalle/{0}?centroorigen={1}&centrodistribucion={2}&cyo={3}", id, centroorigen, centrodistribucion, cyo));
        return View(model);
      }
    }

    // GET: Cupos/Create
    public async Task<ActionResult> Nuevo(int id = 0)
    {
      var model = new NuevoCupoViewModel();
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          model = await repo.RequestGetAndDeserializeAsync<NuevoCupoViewModel>("Cupos", string.Format("Nuevo/{0}", id));
          if (model.Productos != null && model.Productos.Count() > 0) HttpContext.Cache["Granos"] = model.Productos;
        }
      }
      catch (ApiException e)
      {
        model.Error = e.Message;
      }
      catch (Exception e)
      {
        throw e;
      }
      return View(model);
    }

    // POST: Cupos/Create
    [HttpPost]
    public async Task<ActionResult> Nuevo(FormCollection collection, NuevoCupoViewModel model)
    {
      if (!ModelState.IsValid)
      {
        model.Productos = (IList<SelectListItem>)HttpContext.Cache["Granos"];
        return View(model);
      }
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          await repo.RequestPostAndDeserializeAsync<NuevoCupoViewModel>("Cupos", "Nuevo", model);
        }
      }
      catch (ApiException e)
      {
        model.Error = e.Message;
        model.Productos = (IList<SelectListItem>)HttpContext.Cache["Granos"];
        return View(model);
      }
      catch (Exception e)
      {
        throw e;
      }
      return RedirectToAction("Index");
    }

    // GET: Cupos/Editar/5
    public async Task<ActionResult> Editar(string id, string centroorigen, string centrodistribucion, bool cyo = false)
    {
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          EditarCupoViewModel model = await repo.RequestGetAndDeserializeAsync<EditarCupoViewModel>("Cupos", string.Format("Editar/{0}?centroorigen={1}&centrodistribucion={2}&cyo={3}", id, centroorigen, centrodistribucion, cyo));
          return View(model);
        }
      }
      catch (ApiException e)
      {
        throw e;
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    [HttpPost]
    public async Task<ActionResult> Editar(string id, EditarCupoViewModel model, string centroorigen, string centrodistribucion, bool cyo = false)
    {
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          model = await repo.RequestPostAndDeserializeAsync<EditarCupoViewModel>("Cupos", "Editar", new
          {
            Id = new { Id = id, CentroOrigen = centroorigen, CentroDistribucion = centrodistribucion, Cyo = cyo },
            Modelo = model
          });
          return View(model);
        }
      }
      catch (ApiException e)
      {
        throw e;
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    public async Task<ActionResult> Anular(string idCupos, string tipo, string motivo, bool cyo)
    {
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          var RespuestaAnularViewModel = await repo.RequestPostAndDeserializeAsync<RespuestaAnularViewModel>("Cupos", "Anular", new { IdCupos = idCupos, Tipo = tipo, Motivo = motivo, Cyo = cyo });
          return Json(RespuestaAnularViewModel);
        }
      }
      catch (ApiException e)
      {
        throw e;
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    public async Task<ActionResult> Distribucion(string id, string centroorigen, string centrodistribucion, string cyo)
    {
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          DistribuirCupoViewModel model = await repo.RequestGetAndDeserializeAsync<DistribuirCupoViewModel>("Cupos", string.Format("Distribucion/{0}?centroorigen={1}&centrodistribucion={2}&cyo={3}", id, centroorigen, centrodistribucion, cyo));
          return View(model);
        }
      }
      catch (ApiException e)
      {
        throw e;
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    [HttpPost]
    public async Task<ActionResult> Distribucion(string id, DistribuirCupoViewModel model, string centroorigen, string centrodistribucion, string cyo)
    {
      DistribuirCupoViewModel response = new DistribuirCupoViewModel();
      if (ModelState.IsValid)
      {
        try
        {
          using (WebServiceSILRespository repo = new WebServiceSILRespository())
          {
            response = await repo.RequestPostAndDeserializeAsync<DistribuirCupoViewModel>("Cupos", "Distribucion", new
            {
              Id = new { Id = id, CentroOrigen = centroorigen, CentroDistribucion = centrodistribucion, Cyo = cyo },
              Modelo = model
            });
          }
          if (response.Consignaciones == null || response.Consignaciones.Count == 0)
          {
            ModelState.AddModelError("Distribucion", "No se encontró una consignación");
          }
          else if (response.Consignaciones.Count > 1)
          {
            ViewBag.OpenModal = true;
            foreach (ConsignacionDpo consignacion in response.Consignaciones)
            {
              consignacion.Id = Guid.NewGuid();
            }
          }
          else
          {
            //response.ConsignacionSeleccionada = response.Consignaciones.ElementAt(0);
            ViewBag.OpenModal = false;
          }
        }
        catch (ApiException ex)
        {
          throw ex;
        }
        catch (Exception e)
        {
          throw e;
        }
        return View(response);
      }
      return View(response);
    }

    [HttpPost]
    [ValidateAjax]
    public async Task<JsonResult> ActualizarDistribucion(RegistroDistribucionViewModel model, bool Confirmacion = false)
    {
      try
      {
        if (ModelState.IsValid)
        {
          using (WebServiceSILRespository repo = new WebServiceSILRespository())
          {
            model.Confirmacion = Confirmacion;
            return Json(await repo.RequestPostAndDeserializeAsync<int>("Cupos", "ActualizarDistribucion", model));
          }
        }
        else
        {
          return Json(model, JsonRequestBehavior.AllowGet);
        }
      }
      catch (ApiException ex)
      {
        if (ex.Message.Contains("Cupos no anulables:"))
        {
          string mensaje = ex.Message.Replace("Cupos no anulables:: ", string.Empty);
          return Json(new { Status = "ErrorCupo", Mensaje = mensaje }, JsonRequestBehavior.AllowGet);
        }
        else
        {
          return Json(new { Status = "Error", Mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
        }
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    [HttpPost]
    public async Task<JsonResult> Contratos(long compcta, long vendcta, long ctadestino, string codcentro, int grano, DateTime? fechaent)
    {
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          var response = await repo.RequestPostAndDeserializeAsync<IList<RespuestaBusquedaContratos>>("Cupos", "Contratos", new { compcta = compcta, vendcta = vendcta, ctadestino = ctadestino, codcentro = codcentro, grano = grano, fechaent = fechaent });
          return Json(response);
        }
      }
      catch (ApiException e)
      {
        throw e;
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    public async Task<JsonResult> GetMotivo(long id)
    {
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          return Json(new { Motivo = await repo.RequestGetAndDeserializeAsync<string>("Cupos", string.Format("GetMotivo/{0}", id)) });
        }
      }
      catch (ApiException e)
      {
        throw e;
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    public async Task<JsonResult> InformarCuposPorLote(IList<InformarPorLoteViewModel> Lote)
    {
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          return Json(await repo.RequestPostAndDeserializeAsync<InformadosViewModel>("Cupos", "InformarCuposPorLote", Lote));
        }
      }
      catch (ApiException e)
      {
        throw e;
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    /// <summary>
    /// Busca y muestra los contratos agrupados por vendedor y destino.
    /// </summary>
    /// <param name="datosContrato"></param>
    /// <param name="CuentaPuerto"></param>
    /// <param name="fechaDesde"></param>
    /// <param name="fechaHasta"></param>
    /// <param name="cosechaDesde"></param>
    /// <param name="cosechaHasta"></param>
    /// <param name="Cyo"></param>
    /// <returns>Retorna un PartialView de una tabla con los totales de cupos pendiente de distribuir por dia incluido.</returns>
    public async Task<ActionResult> GetTablaContratos(VistaCuposDistribuidos datosContrato, long CuentaPuerto,
        DateTime? fechaDesde, DateTime? fechaHasta, string cosechaDesde, string cosechaHasta, Consignacion ConsignacionSeleccionada, string Cyo = "FALSE")
    {
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          var Distribucion = await repo.RequestPostAndDeserializeAsync<DistribucionDisponible>("Cupos", "GetContratos", new
          {
            datosContrato = datosContrato,
            CuentaPuerto = CuentaPuerto,
            fechaDesde = fechaDesde,
            fechaHasta = fechaHasta,
            cosechaDesde = cosechaDesde,
            cosechaHasta = cosechaHasta,
            ConsignacionSeleccionada = ConsignacionSeleccionada,
            Cyo = Cyo
          });
          return PartialView("~/Views/Contratos/_DistribucionContratosPartial.cshtml", Distribucion);
        }
      }
      catch (ApiException e)
      {
        throw e;
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    public ActionResult Turno()
    {
      return View();
    }

    public async Task<JsonResult> GetCuposPorCodigoAlfanumerico(IList<string> CodigosAlfanumericos)
    {
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          var response = await repo.RequestPostAndDeserializeAsync<IList<RespuestaEstadoAlfanumericoViewModel>>("Cupos", "GetCuposPorCodigoAlfanumerico", CodigosAlfanumericos);
          return Json(new { Cupos = response });
        }
      }
      catch (ApiException e)
      {
        throw e;
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    public async Task<ActionResult> AutorizacionesPendientes()
    {
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          var Pendientes = await repo.RequestPostAndDeserializeAsync<IndexCupoAutorizarViewModel>("CupoSTOPtoSIL", "CuposPendientes", null);
          return View("~/Views/Cupos/AutorizacionesPendientes.cshtml", Pendientes);
        }
      }
      catch (ApiException e)
      {
        throw e;
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    public async Task<ActionResult> AutorizacionCuposStop(string id, string centro, string cuitcorrcomp, string cuitcorrvend)
    {
      var Cuentas = id.Split('-');
      long.TryParse(Cuentas[0], out long CuentaComprador);
      long.TryParse(Cuentas[1], out long CuentaVendedor);
      long.TryParse(Cuentas[2], out long CuentaPuerto);
      int.TryParse(Cuentas[3], out int CodigoGrano);
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          var Alfanumericos = await repo.RequestPostAndDeserializeAsync<NuevoCupoViewModelDTO>("CupoSTOPtoSIL", "CuposPendientes", new
          {
            Producto = CodigoGrano,
            Compcta = CuentaComprador,
            Puerto = CuentaPuerto,
            Vendcta = CuentaVendedor,
            Centro = centro,
            Cuitcorrcomp = cuitcorrcomp,
            Cuitcorrvta = cuitcorrvend
          });

          NuevoCupoStopViewModel model = await Alfanumericos.DTOtoViewModel();
          model.CodigosDias = model.CompletarDias(Alfanumericos.CodigosDias);

          return View("~/Views/Cupos/AutorizacionCupoSTOP.cshtml", model);
        }
      }
      catch (ApiException e)
      {
        throw e;
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    public async Task<JsonResult> AutorizarStop(NuevoCupoViewModelDTO dto)
    {
      var Autorizado = false;
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          Autorizado = await repo.RequestPostAndDeserializeAsync<bool>("CupoSTOPtoSIL", "AgregarCupos", dto);
        }
      }
      catch (ApiException e)
      {
        bool showMessageRepeatedAlpha = false;
        if (e.Message.IndexOf("ya existen") > -1)
          showMessageRepeatedAlpha = true;

        return Json(new { Status = Autorizado, TypeError = "API", Message = e.Message.Replace("|", ", "), ShowMessageRepeatedAlpha = showMessageRepeatedAlpha });
      }
      catch (Exception e)
      {
        throw e;
      }
      return Json(new { Status = Autorizado });
    }

    public async Task<JsonResult> CambiarCentroCupoPendienteStop(NuevoCupoViewModelDTO dto)
    {
      var Autorizado = false;
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          Autorizado = await repo.RequestPostAndDeserializeAsync<bool>("CupoSTOPtoSIL", "CambiarCentro", dto);
        }
      }
      catch (ApiException e)
      {
        bool showMessageRepeatedAlpha = false;
        if (e.Message.IndexOf("ya existen") > -1)
          showMessageRepeatedAlpha = true;

        return Json(new { Status = Autorizado, TypeError = "API", Message = e.Message.Replace("|", ", "), ShowMessageRepeatedAlpha = showMessageRepeatedAlpha });
      }
      catch (Exception e)
      {
        throw e;
      }
      return Json(new { Status = Autorizado });
    }

    public async Task<JsonResult> ExistenCuposPendientes()
    {
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          bool Existen = await repo.RequestGetAndDeserializeAsync<bool>("CupoSTOPtoSIL", "ExistenCuposPendientes");
          return Json(Existen);
        }
      }
      catch (ApiException e)
      {
        throw e;
      }
      catch (Exception e)
      {
        throw e;
      }
    }
  }
}