using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CuposCorretajeWeb.Models.Filtro;

namespace CuposCorretajeWeb.Models
{
  public class DistribuirCupoViewModel
  {
    public string Id { get; set; }
    public string Producto { get; set; }
    public int CodigoProducto { get; set; }
    public string Comprador { get; set; }
    public long CuentaComprador { get; set; }
    public long CuentaVendedor { get; set; }
    public string Puerto { get; set; }
    public long CuentaPuerto { get; set; }
    [Display(Name = "Entrega Hasta")]
    public DateTime? EntregaHasta { get; set; }
    [Display(Name = "Entrega Desde")]
    public DateTime? EntregaDesde { get; set; }
    [Display(Name = "Cosecha Desde")]
    public string CosechaDesde { get; set; }
    [Display(Name = "Cosecha Hasta")]
    public string CosechaHasta { get; set; }
    public IEnumerable<SelectListItem> Centro { get; set; }
    [Required(AllowEmptyStrings = false)]
    [Display(Name = "Centro")]
    public string CentroSeleccionado { get; set; }
    public IList<ConsignacionDpo> Consignaciones;
    public IList<Counter<Cupos>> ConsignacionesAgrupadosPorFecha;
    public string Buscador { get; set; }
    public DistribucionDisponible DistribucionesDisponibles { get; set; }
    public string Destino { get; set; }
    public IEnumerable<DTabla> MotivosDecrementoCupos { get; set; }
    private string Cyo { get; set; }
    private FiltroDistribucion filtro { get; set; }
    public IList<CuposAgrupadosPdf> CuposAgrupados { get; set; }
    public Consignacion ConsignacionSeleccionada { get; set; }
  }

  public class RespuestaBusquedaContratos
  {
    public string contrato { get; set; }
    public string neg { get; set; }
    public string oper { get; set; }
    public string prod { get; set; }
    public string cos { get; set; }
    public string entrega { get; set; }
    public string vtoEnt { get; set; }
    public string pactadas { get; set; }
    public string apli { get; set; }
    public string pendEnt { get; set; }
    public string liq { get; set; }
    public string destino { get; set; }
    public string precio { get; set; }
  }

  //public class IdViewModel
  //{
  //    private string _cyo = "false";
  //    public string Id { get; set; }
  //    public string CentroOrigen { get; set; }
  //    public string CentroDistribucion { get; set; }
  //    public string Cyo { get { return _cyo; } set { _cyo = value; } }
  //}

  //public class BusquedaViewModel
  //{
  //    public IdViewModel Id { get; set; }
  //    public DistribuirCupoViewModel Modelo { get; set; }
  //}

  //public class BusquedaMultiplesConsignacionesViewModel
  //{
  //    public long compcta { get; set; }
  //    public long vendcta { get; set; }
  //    public long ctadestino { get; set; }
  //    public string codcentro { get; set; }
  //    public int grano { get; set; }
  //    public DateTime? fechaent { get; set; }
  //}

  //public class BusquedaContratosViewModel
  //{
  //    private string _cyo = "FALSE";
  //    public VistaCuposDistribuidos datosContrato { get; set; }
  //    public long CuentaPuerto { get; set; }
  //    public DateTime? fechaDesde { get; set; }
  //    public DateTime? fechaHasta { get; set; }
  //    public string cosechaDesde { get; set; }
  //    public string cosechaHasta { get; set; }
  //    public string Cyo { get { return _cyo; } set { _cyo = value; } }
  //}
}