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
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;

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
    public async Task<ActionResult> Nuevo(int id = 0, string returnUrl = null)
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
      // Guardamos el returnUrl en ViewBag para que Nuevo.cshtml lo renderee
      // como hidden y sobreviva al POST. Validamos que sea URL relativa
      // (comienza con "/") para evitar open-redirect a sitios externos.
      ViewBag.ReturnUrl = EsReturnUrlSeguro(returnUrl) ? returnUrl : null;
      return View(model);
    }

    // POST: Cupos/Create
    [HttpPost]
    public async Task<ActionResult> Nuevo(FormCollection collection, NuevoCupoViewModel model, string returnUrl = null)
    {
      // Re-publicamos el returnUrl para que sobreviva un re-render del form
      // (cuando ModelState no es válido o la API devuelve ApiException).
      ViewBag.ReturnUrl = EsReturnUrlSeguro(returnUrl) ? returnUrl : null;

      if (!ModelState.IsValid)
      {
        model.Productos = (IList<SelectListItem>)HttpContext.Cache["Granos"];
        return View(model);
      }
      try
      {
        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          model.ContactoComercial = string.IsNullOrEmpty(model.ContactoComercial) ? string.Empty : string.Join(";", Regex.Replace(model.ContactoComercial, @"\s+", "").Split(';').OrderBy(x => x));
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

      // El caller pasó returnUrl por query string/hidden → volvemos a esa
      // pantalla (ej. Solicitudes/Index cuando se creó el cupo desde el
      // listado de solicitudes). Si no vino (flujo legacy u otro caller),
      // mantenemos el redirect original a Cupos/Index.
      if (!string.IsNullOrEmpty(ViewBag.ReturnUrl as string))
      {
        return Redirect(ViewBag.ReturnUrl as string);
      }
      return RedirectToAction("Index");
    }

    /// <summary>
    /// Valida que un <c>returnUrl</c> sea seguro de usar como destino de
    /// redirect: sólo aceptamos URLs relativas (que empiecen con "/") y
    /// que NO sean protocol-relative ("//example.com" sería open-redirect).
    /// </summary>
    private static bool EsReturnUrlSeguro(string returnUrl)
    {
      if (string.IsNullOrWhiteSpace(returnUrl)) return false;
      if (!returnUrl.StartsWith("/")) return false;
      if (returnUrl.StartsWith("//")) return false;
      return true;
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

    /// <summary>
    /// Proxy al endpoint <c>POST /api/ShiftRequest/AnularDistribucion</c> de
    /// SILData. Se invoca desde <c>Scripts/Editar.js</c> cuando el operador
    /// anula una distribución, después de que la acción legacy
    /// <see cref="Anular"/> ya marcó los cupos en CUPOSCORRE como anulados.
    ///
    /// Objetivo: revertir la asociación cupo ↔ solicitud en
    /// <c>SOLTURNOS</c> + <c>SOLTURNOS_DETALLE</c> para que las solicitudes
    /// vuelvan a figurar como Pendientes (y por lo tanto vuelvan a aparecer
    /// en Pantalla 1 con un cupo menos aceptado cada una).
    ///
    /// El backend procesa cada cupo de forma independiente. Los que NO
    /// tengan solicitud asociada quedan como Skipped (el flujo legacy ya
    /// los cubrió en CUPOSCORRE).
    /// </summary>
    /// <param name="req">DTO con la lista de cupos. Se bindea vía
    /// <c>JsonValueProviderFactory</c> (builtin de MVC 4.6.1) — NO se usa
    /// <c>[FromBody]</c> porque es un atributo de Web API que MVC ignora.
    /// El JS debe mandar <c>contentType: application/json</c> y
    /// <c>data: JSON.stringify({ cupoIds: [...] })</c>.</param>
    [HttpPost]
    public async Task<ActionResult> AnularDistribucion(AnularDistribucionRequestDto req)
    {
      // Defensa: req puede venir null si el body no se bindeó (p.ej.
      // content-type incorrecto o body vacío). En ese caso devolvemos
      // un resultado neutro para que el JS no rompa.
      var cupoIds = req?.CupoIds ?? Array.Empty<long>();

      try
      {
        if (cupoIds.Length == 0)
        {
          return Json(new AnularDistribucionResponseDto
          {
            AlMenosUnoExitoso = false,
            CantidadExitosos = 0,
            CantidadSkipped = 0,
            CantidadFallos = 0,
            Items = new List<AnularDistribucionItemResponseDto>()
          });
        }

        // Filtramos ids no positivos antes de mandar al backend (defensa).
        var idsValidos = cupoIds.Where(id => id > 0).Distinct().ToList();
        if (idsValidos.Count == 0)
        {
          return Json(new AnularDistribucionResponseDto
          {
            AlMenosUnoExitoso = false,
            CantidadExitosos = 0,
            CantidadSkipped = 0,
            CantidadFallos = 0,
            Items = new List<AnularDistribucionItemResponseDto>()
          });
        }

        using (WebServiceSILRespository repo = new WebServiceSILRespository())
        {
          // El endpoint de SILData vive bajo ShiftRequestController.
          // Una sola llamada HTTP con todos los cupos en una lista
          // (RequestSILDataPostAndDeserializeAsync serializa el objeto a
          // JSON, que coincide con el contrato del backend).
          var resultado = await repo.RequestSILDataPostAndDeserializeAsync<AnularDistribucionResponseDto>(
            "ShiftRequest",
            "AnularDistribucion",
            new { cupoIds = idsValidos });

          return Json(resultado ?? new AnularDistribucionResponseDto());
        }
      }
      catch (ApiException e)
      {
        // ApiException viene del helper cuando SILData responde con un
        // status no-2xx (400 lista vacía, 409 conflicto de estado, etc.).
        // Devolvemos un JSON con todos los cupos como Fallo para que el JS
        // muestre un Swal.fire informativo en vez de propagar la excepción.
        return Json(new AnularDistribucionResponseDto
        {
          AlMenosUnoExitoso = false,
          CantidadExitosos = 0,
          CantidadSkipped = 0,
          CantidadFallos = cupoIds.Length,
          Items = cupoIds
            .Where(id => id > 0)
            .Select(id => new AnularDistribucionItemResponseDto
            {
              CupoId = id,
              Estado = 2, // Fallo
              SolicitudId = 0,
              MotivoFalla = e.Message
            }).ToList()
        });
      }
      catch (Exception e)
      {
        return Json(new AnularDistribucionResponseDto
        {
          AlMenosUnoExitoso = false,
          CantidadExitosos = 0,
          CantidadSkipped = 0,
          CantidadFallos = cupoIds.Length,
          Items = cupoIds
            .Where(id => id > 0)
            .Select(id => new AnularDistribucionItemResponseDto
            {
              CupoId = id,
              Estado = 2, // Fallo
              SolicitudId = 0,
              MotivoFalla = e.Message
            }).ToList()
        });
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
            // Si llega un nuevo con datos de contacto, normalizar antes de enviar.
            if (model.nuevo != null)
            {
              model.nuevo.ContactoComercial = string.IsNullOrEmpty(model.nuevo.ContactoComercial)
                ? string.Empty
                : string.Join(";", Regex.Replace(model.nuevo.ContactoComercial, @"\s+", "").Split(';').OrderBy(x => x));
            }
            model.Confirmacion = Confirmacion;

            // Si el cliente no envía Modo, default a DistribucionManual para
            // mantener compatibilidad con callers que no informan el modo.
            if (!model.Modo.HasValue)
            {
              model.Modo = ModoActualizacionDistribucion.DistribucionManual;
            }

            // En modo SolicitudMatch SILApi devuelve ActualizarDistribucionResult;
            // en DistribucionManual sigue devolviendo un int legacy. Detectamos
            // el modo para deserializar correctamente.
            if (model.Modo == ModoActualizacionDistribucion.SolicitudMatch)
            {
              ActualizarDistribucionResult resultado =
                await repo.RequestPostAndDeserializeAsync<ActualizarDistribucionResult>(
                  "Cupos", "ActualizarDistribucion", model);

              if (resultado == null)
              {
                return Json(new ActualizarDistribucionResult
                {
                  Codigo = 0,
                  Success = false,
                  Message = "Respuesta vacía del backend."
                }, JsonRequestBehavior.AllowGet);
              }

              return Json(resultado, JsonRequestBehavior.AllowGet);
            }

            int legacyResultado = await repo.RequestPostAndDeserializeAsync<int>(
              "Cupos", "ActualizarDistribucion", model);
            return Json(legacyResultado, JsonRequestBehavior.AllowGet);
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
          dto.ContactoComercial = string.IsNullOrEmpty(dto.ContactoComercial) ? string.Empty : string.Join(";", Regex.Replace(dto.ContactoComercial, @"\s+", "").Split(';').OrderBy(x => x));
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