using CuposCorretajeWeb.Models.AtributosValidacion;
using CuposCorretajeWeb.Models.Configuracion;
using CuposCorretajeWeb.Models.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CuposCorretajeWeb.Models
{
  public class NuevoCupoViewModel
  {
    public string Error { get; set; }
    public int ProductoSeleccionado { get; set; }
    public int PuertoSeleccionado { get; set; }
    public int CompctaSeleccionada { get; set; }
    public int VendctaSeleccionada { get; set; }
    public readonly int CantidadDias = 20;

    [Display(Name = "Producto")]
    public virtual IEnumerable<SelectListItem> Productos { get; set; }
    [Display(Name = "Comprador")]
    [Required]
    [Range(0, long.MaxValue, ErrorMessage = "Por favor ingrese un número")]
    public string Compcta { get; set; }
    [Required]
    [Range(0, long.MaxValue, ErrorMessage = "Por favor ingrese un número")]
    public string Puerto { get; set; }
    [Display(Name = "Centro")]
    [Required]
    public virtual string Centro { get; set; }
    [Display(Name = "Vendedor")]
    [Range(0, long.MaxValue, ErrorMessage = "Por favor ingrese un número")]
    [NotEqual("Compcta", "Vendedor", "Comprador")]
    public string Vendcta { get; set; }
    public virtual bool VendcyoBoolValue { get; set; }
    [Display(Name = "Titular de CCPP")]
    public virtual string Cuitsolicitante { get; set; }
    [Display(Name = "Nombre")]
    public virtual string Nomsolicitante { get; set; }
    [Display(Name = "Rte Comercial Venta Secundaria ")]
    public virtual string Cuitintermediario { get; set; }
    [Display(Name = "Nombre")]
    public virtual string Nomintermediario { get; set; }
    [Display(Name = "Rte Comercial Venta Secundaria 2")]
    public virtual string Cuitrtecomercial { get; set; }
    [Display(Name = "Nombre")]
    public virtual string Nomrtecomercial { get; set; }
    [Display(Name = "Corredor Venta Secundaria")]
    public virtual string Cuitcorrcomp { get; set; }
    [Display(Name = "Nombre")]
    public virtual string Nomcorrcomp { get; set; }
    [Display(Name = "Mercado a Termino")]
    public virtual string Cuitmat { get; set; }
    [Display(Name = "Nombre")]
    public virtual string Nommat { get; set; }
    [Display(Name = "Corredor Venta Primaria")]
    public virtual string Cuitcorrvta { get; set; }
    [Display(Name = "Nombre")]
    public virtual string Nomcorrvta { get; set; }
    [Display(Name = "Representante Entregador")]
    public virtual string Cuitrteent { get; set; }
    [Display(Name = "Nombre")]
    public virtual string Nomrteent { get; set; }
    [Display(Name = "Destinatario")]
    [Required]
    public virtual string Cuitdestinatario { get; set; }
    [Display(Name = "Nombre")]
    [Required]
    public virtual string Nomdestinatario { get; set; }
    [Display(Name = "Rte Comercial Productor")]
    public string CuitRteComercialProductor { get; set; }

    [Display(Name = "Nombre")]
    public string NomRteComercialProductor { get; set; }

    [Display(Name = "Rte Comercial Venta Primaria")]
    public string CuitRteComercialVentaPrimaria { get; set; }

    [Display(Name = "Nombre")]
    public string NomRteComercialVentaPrimaria { get; set; }

    //public Consignacion Consignacion { get; set; }
    public virtual IList<CodigosAlfanumericos> CodigosDias { get; set; }
    public virtual string Observaciones { get; set; }

    [Display(Name = "Contacto Comercial")]
    public virtual string ContactoComercial { get; set; }

    [Display(Name = "Carátula")]
    [RegularExpression(@"\d{6}", ErrorMessage = "El campo Carátula debe contener un número de 6 dígitos")]
    public virtual string Caratula { get; set; }

    public NuevoCupoViewModel()
    {
      Productos = new List<SelectListItem>();
    }
  }

  public abstract class NuevoCupoStopViewModel
  {
    protected const string DISABLED = "disabled";
    protected const string DISABLED_IF_NOT_EMPTY = "disabled_if_not_empty";

    public string Error { get; set; }
    public int Producto { get; set; }
    public readonly int CantidadDias = 20;

    [Display(Name = "Productos")]
    public IEnumerable<RelacionGranoStop> Productos { get; set; }
    public abstract string ProductoDisabled { get; set; }
    [Display(Name = "Comprador")]
    [Required]
    [Range(0, long.MaxValue, ErrorMessage = "Por favor ingrese un número")]
    public string Compcta { get; set; }
    public string CompradorNombre { get; set; }
    public abstract string CompctaDisabled { get; set; }
    public string Puerto { get; set; }
    [Display(Name = "Puertos")]
    public IEnumerable<RelacionPuertoStop> Puertos { get; set; }
    public abstract string PuertoDisabled { get; set; }
    [Display(Name = "Centro")]
    [Required]
    public string Centro { get; set; }
    public IList<SelectListItem> Centros { get; set; }
    [Display(Name = "Vendedor")]
    [Range(0, long.MaxValue, ErrorMessage = "Por favor ingrese un número")]
    [NotEqual("Compcta", "Vendedor", "Comprador")]
    public string Vendcta { get; set; }
    public string VendedorNombre { get; set; }
    public abstract string VendctaDisabled { get; set; }
    public bool VendcyoBoolValue { get; set; }
    [Display(Name = "Titular de CCPP")]
    public abstract string Cuitsolicitante { get; set; }
    public abstract string CuitsolicitanteDisabled { get; set; }
    [Display(Name = "Nombre")]
    public abstract string Nomsolicitante { get; set; }
    public abstract string NomsolicitanteDisabled { get; set; }
    [Display(Name = "Rte Comercial Venta Secundaria ")]
    public abstract string Cuitintermediario { get; set; }
    public abstract string CuitintermediarioDisabled { get; set; }
    [Display(Name = "Nombre")]
    public abstract string Nomintermediario { get; set; }
    public abstract string NomintermediarioDisabled { get; set; }
    [Display(Name = "Rte Comercial Venta Secundaria 2")]
    public abstract string Cuitrtecomercial { get; set; }
    public abstract string CuitrtecomercialDisabled { get; set; }
    [Display(Name = "Nombre")]
    public abstract string Nomrtecomercial { get; set; }
    public abstract string NomrtecomercialDisabled { get; set; }
    [Display(Name = "Corredor Venta Secundaria")]
    public abstract string Cuitcorrcomp { get; set; }
    public abstract string CuitcorrcompDisabled { get; set; }
    [Display(Name = "Nombre")]
    public abstract string Nomcorrcomp { get; set; }
    public abstract string NomcorrcompDisabled { get; set; }
    [Display(Name = "Mercado a Termino")]
    public abstract string Cuitmat { get; set; }
    public abstract string CuitmatDisabled { get; set; }
    [Display(Name = "Nombre")]
    public abstract string Nommat { get; set; }
    public abstract string NommatDisabled { get; set; }
    [Display(Name = "Corredor Venta Primaria")]
    public abstract string Cuitcorrvta { get; set; }
    public abstract string CuitcorrvtaDisabled { get; set; }
    [Display(Name = "Nombre")]
    public abstract string Nomcorrvta { get; set; }
    public abstract string NomcorrvtaDisabled { get; set; }
    [Display(Name = "Representante Entregador")]
    public abstract string Cuitrteent { get; set; }
    public abstract string CuitrteentDisabled { get; set; }
    [Display(Name = "Nombre")]
    public abstract string Nomrteent { get; set; }
    public abstract string NomrteentDisabled { get; set; }
    [Display(Name = "Destinatario")]
    [Required]
    public abstract string Cuitdestinatario { get; set; }
    public abstract string CuitdestinatarioDisabled { get; set; }
    [Display(Name = "Nombre")]
    [Required]
    public abstract string Nomdestinatario { get; set; }
    public abstract string NomdestinatarioDisabled { get; set; }

    [Display(Name = "Rte Comercial Productor")]
    public abstract string CuitRteComercialProductor { get; set; }
    public abstract string CuitRteComercialProductorDisabled { get; set; }

    [Display(Name = "Nombre")]
    public abstract string NomRteComercialProductor { get; set; }
    public abstract string NomRteComercialProductorDisabled { get; set; }

    [Display(Name = "Rte Comercial Venta Primaria")]
    public abstract string CuitRteComercialVentaPrimaria { get; set; }
    public abstract string CuitRteComercialVentaPrimariaDisabled { get; set; }

    [Display(Name = "Nombre")]
    public abstract string NomRteComercialVentaPrimaria { get; set; }
    public abstract string NomRteComercialVentaPrimariaDisabled { get; set; }

    [Display(Name = "Contacto Comercial")]
    public abstract string ContactoComercial { get; set; }

    [Display(Name = "Carátula")]
    public abstract string Caratula { get; set; }

    //public Consignacion Consignacion { get; set; }
    public IList<CuposPorDiaDTO> CodigosDias { get; set; }
    public string Observaciones { get; set; }

    //public IList<NuevoAlfanumerico> ListaAlfanumerico { get; set; }

    public NuevoCupoStopViewModel(long CuentaComprador, long CuentaVendedor, long CuentaPuerto, int CodigoGrano)
    {
      this.Compcta = CuentaComprador.ToString();
      this.Puerto = CuentaPuerto.ToString();
      this.Producto = CodigoGrano;
      this.Vendcta = CuentaVendedor.ToString();

      //this.ListaAlfanumerico = new List<NuevoAlfanumerico>();

      //for (var i = 0; i <= CantidadDias; i++)
      //{
      //    this.ListaAlfanumerico.Add(new NuevoAlfanumerico() { NumeroDia = i, Dia = DateTime.Now.AddDays(i).ToString("dd/MM/yyyy"), AlfaDia = new List<string>(), CuerposDia = new List<Cupos>() });
      //}
    }

    public IList<CuposPorDiaDTO> CompletarDias(IList<CuposPorDiaDTO> Codigos)
    {
      List<CuposPorDiaDTO> ListaAlfanumerico = new List<CuposPorDiaDTO>();
      IEnumerable<CuposPorDiaDTO> CuposDelDia = new List<CuposPorDiaDTO>();
      for (var i = 0; i <= CantidadDias; i++)
      {
        CuposDelDia = Codigos
            .Where(y => y.Fecha.Date == DateTime.Now.AddDays(i).Date);

        //ListaAlfanumerico.AddRange(
        //    CuposDelDia.Select(x => new NuevoAlfanumerico
        //    {
        //        Dia = x.Fecha.ToString("dd/MM/yyyy"),
        //        NumeroDia = i,
        //        CuerposDia = x.Turnos.Select(y => new Cupos() { Nrocupo = y }).ToList()
        //    }));

        if (CuposDelDia.Count() == 0)
          ListaAlfanumerico.Add(new CuposPorDiaDTO { Fecha = DateTime.Now.AddDays(i).Date, Turnos = Array.Empty<string>() });
        else
          ListaAlfanumerico.AddRange(CuposDelDia);
        //ListaAlfanumerico.Add(new NuevoAlfanumerico() { NumeroDia = i, Dia = DateTime.Now.AddDays(i).ToString("dd/MM/yyyy"), AlfaDia = new List<string>(), CuerposDia = new List<Cupos>() });
      }
      return ListaAlfanumerico;
    }
  }

  public class NuevoCupoNominadoStopViewModel : NuevoCupoStopViewModel
  {

    public NuevoCupoNominadoStopViewModel(long CuentaComprador, long CuentaVendedor, long CuentaPuerto, int CodigoGrano)
        : base(CuentaComprador, CuentaVendedor, CuentaPuerto, CodigoGrano)
    {

    }

    public override string ProductoDisabled
    {
      get { return DISABLED; }
      set { }
    }

    public override string CompctaDisabled
    {
      get;
      set;
    }

    public override string PuertoDisabled
    {
      get { return DISABLED; }
      set { }
    }

    public override string VendctaDisabled
    {
      get;
      set;
    }

    public override string Cuitsolicitante
    {
      get;
      set;
    }

    public override string CuitsolicitanteDisabled
    {
      get;
      set;
    }

    public override string Nomsolicitante
    {
      get;
      set;
    }

    public override string NomsolicitanteDisabled
    {
      get;
      set;
    }

    public override string Cuitintermediario
    {
      get;
      set;
    }

    public override string CuitintermediarioDisabled
    {
      get;
      set;
    }

    public override string Nomintermediario
    {
      get;
      set;
    }

    public override string NomintermediarioDisabled
    {
      get;
      set;
    }

    public override string Cuitrtecomercial
    {
      get;
      set;
    }

    public override string CuitrtecomercialDisabled
    {
      get;
      set;
    }

    public override string Nomrtecomercial
    {
      get;
      set;
    }

    public override string NomrtecomercialDisabled
    {
      get;
      set;
    }

    public override string Cuitcorrcomp
    {
      get;
      set;
    }

    public override string CuitcorrcompDisabled
    {
      get;
      set;
    }

    public override string Nomcorrcomp
    {
      get;
      set;
    }

    public override string NomcorrcompDisabled
    {
      get;
      set;
    }

    public override string Cuitmat
    {
      get;
      set;
    }

    public override string CuitmatDisabled
    {
      get;
      set;
    }

    public override string Nommat
    {
      get;
      set;
    }

    public override string NommatDisabled
    {
      get;
      set;
    }

    public override string Cuitcorrvta
    {
      get;
      set;
    }

    public override string CuitcorrvtaDisabled
    {
      get;
      set;
    }

    public override string Nomcorrvta
    {
      get;
      set;
    }

    public override string NomcorrvtaDisabled
    {
      get;
      set;
    }

    public override string Cuitrteent
    {
      get;
      set;
    }

    public override string CuitrteentDisabled
    {
      get;
      set;
    }

    public override string Nomrteent
    {
      get;
      set;
    }

    public override string NomrteentDisabled
    {
      get;
      set;
    }

    public override string Cuitdestinatario
    {
      get;
      set;
    }

    public override string CuitdestinatarioDisabled
    {
      get { return DISABLED; }
      set { }
    }

    public override string Nomdestinatario
    {
      get;
      set;
    }

    public override string NomdestinatarioDisabled
    {
      get { return DISABLED; }
      set { }
    }

    public override string CuitRteComercialProductor { get; set; }
    public override string CuitRteComercialProductorDisabled { get; set; }
    public override string NomRteComercialProductor { get; set; }
    public override string NomRteComercialProductorDisabled { get; set; }
    public override string CuitRteComercialVentaPrimaria { get; set; }
    public override string CuitRteComercialVentaPrimariaDisabled { get; set; }
    public override string NomRteComercialVentaPrimaria { get; set; }
    public override string NomRteComercialVentaPrimariaDisabled { get; set; }
    public override string ContactoComercial { get; set; }
    public override string Caratula { get; set; }
  }

  public class NuevoCupoNoNominadoStopViewModel : NuevoCupoStopViewModel
  {
    public NuevoCupoNoNominadoStopViewModel(long CuentaComprador, long CuentaVendedor, long CuentaPuerto, int CodigoGrano)
        : base(CuentaComprador, CuentaVendedor, CuentaPuerto, CodigoGrano)
    {

    }

    public override string ProductoDisabled
    {
      get { return DISABLED; }
      set { }
    }

    public override string CompctaDisabled
    {
      get;
      set;
    }

    public override string PuertoDisabled
    {
      get { return DISABLED; }
      set { }
    }

    public override string VendctaDisabled
    {
      get;
      set;
    }

    public override string Cuitsolicitante
    {
      get;
      set;
    }

    public override string CuitsolicitanteDisabled
    {
      get { return DISABLED_IF_NOT_EMPTY; }
      set { }
    }

    public override string Nomsolicitante
    {
      get;
      set;
    }

    public override string NomsolicitanteDisabled
    {
      get { return DISABLED_IF_NOT_EMPTY; }
      set { }
    }

    public override string Cuitintermediario
    {
      get;
      set;
    }

    public override string CuitintermediarioDisabled
    {
      get { return DISABLED_IF_NOT_EMPTY; }
      set { }
    }

    public override string Nomintermediario
    {
      get;
      set;
    }

    public override string NomintermediarioDisabled
    {
      get { return DISABLED_IF_NOT_EMPTY; }
      set { }
    }

    public override string Cuitrtecomercial
    {
      get;
      set;
    }

    public override string CuitrtecomercialDisabled
    {
      get { return DISABLED_IF_NOT_EMPTY; }
      set { }
    }

    public override string Nomrtecomercial
    {
      get;
      set;
    }

    public override string NomrtecomercialDisabled
    {
      get { return DISABLED_IF_NOT_EMPTY; }
      set { }
    }

    public override string Cuitcorrcomp
    {
      get;
      set;
    }

    public override string CuitcorrcompDisabled
    {
      get { return DISABLED_IF_NOT_EMPTY; }
      set { }
    }

    public override string Nomcorrcomp
    {
      get;
      set;
    }

    public override string NomcorrcompDisabled
    {
      get { return DISABLED_IF_NOT_EMPTY; }
      set { }
    }

    public override string Cuitmat
    {
      get;
      set;
    }

    public override string CuitmatDisabled
    {
      get { return DISABLED_IF_NOT_EMPTY; }
      set { }
    }

    public override string Nommat
    {
      get;
      set;
    }

    public override string NommatDisabled
    {
      get { return DISABLED_IF_NOT_EMPTY; }
      set { }
    }

    public override string Cuitcorrvta
    {
      get;
      set;
    }

    public override string CuitcorrvtaDisabled
    {
      get { return DISABLED_IF_NOT_EMPTY; }
      set { }
    }

    public override string Nomcorrvta
    {
      get;
      set;
    }

    public override string NomcorrvtaDisabled
    {
      get { return DISABLED_IF_NOT_EMPTY; }
      set { }
    }

    public override string Cuitrteent
    {
      get;
      set;
    }

    public override string CuitrteentDisabled
    {
      get { return DISABLED_IF_NOT_EMPTY; }
      set { }
    }

    public override string Nomrteent
    {
      get;
      set;
    }

    public override string NomrteentDisabled
    {
      get { return DISABLED_IF_NOT_EMPTY; }
      set { }
    }

    public override string Cuitdestinatario
    {
      get;
      set;
    }

    public override string CuitdestinatarioDisabled
    {
      get { return DISABLED; }
      set { }
    }

    public override string Nomdestinatario
    {
      get;
      set;
    }

    public override string NomdestinatarioDisabled
    {
      get { return DISABLED; }
      set { }
    }
    public override string CuitRteComercialProductor { get; set; }
    public override string CuitRteComercialProductorDisabled { get; set; }
    public override string NomRteComercialProductor { get; set; }
    public override string NomRteComercialProductorDisabled { get; set; }
    public override string CuitRteComercialVentaPrimaria { get; set; }
    public override string CuitRteComercialVentaPrimariaDisabled { get; set; }
    public override string NomRteComercialVentaPrimaria { get; set; }
    public override string NomRteComercialVentaPrimariaDisabled { get; set; }
    public override string ContactoComercial { get; set; }
    public override string Caratula { get; set; }
  }
}