using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CuposCorretajeWeb.Models
{
  public class ConsignacionDpo
  {
    public Guid Id { get; set; }
    [Display(Name = "Titular de CCPP")]
    public string Cuitsolicitante { get; set; }
    [Display(Name = "Nombre")]
    public string Nomsolicitante { get; set; }
    [Display(Name = "Rte Comercial Venta Secundaria ")]
    public string Cuitintermediario { get; set; }
    [Display(Name = "Nombre")]
    public string Nomintermediario { get; set; }
    [Display(Name = "Rte Comercial Venta Secundaria 2")]
    public string Cuitrtecomercial { get; set; }
    [Display(Name = "Nombre")]
    public string Nomrtecomercial { get; set; }
    [Display(Name = "Corredor Venta Secundaria")]
    public string Cuitcorrcomp { get; set; }
    [Display(Name = "Nombre")]
    public string Nomcorrcomp { get; set; }
    [Display(Name = "Mercado a Termino")]
    public string Cuitmat { get; set; }
    [Display(Name = "Nombre")]
    public string Nommat { get; set; }
    [Display(Name = "Corredor Venta Primaria")]
    public string Cuitcorrvta { get; set; }
    [Display(Name = "Nombre")]
    public string Nomcorrvta { get; set; }
    [Display(Name = "Representante Entregador")]
    public string Cuitrteent { get; set; }
    [Display(Name = "Nombre")]
    public string Nomrteent { get; set; }
    [Display(Name = "Destinatario")]

    public string Cuitdestinatario { get; set; }
    [Display(Name = "Nombre")]

    public string Nomdestinatario { get; set; }
    [Display(Name = "Rte Comercial Productor")]
    public string CuitRteComercialProductor { get; set; }

    [Display(Name = "Nombre")]
    public string NomRteComercialProductor { get; set; }

    [Display(Name = "Rte Comercial Venta Primaria")]
    public string CuitRteComercialVentaPrimaria { get; set; }

    [Display(Name = "Nombre")]
    public string NomRteComercialVentaPrimaria { get; set; }

    [Display(Name = "Contacto Comercial")]
    public string ContactoComercial { get; set; }

    [Display(Name = "Nombres")]
    public IList<string> NomContactoComercial { get; set; }

    [Display(Name = "Carátula")]
    public string Caratula { get; set; }
    [Display(Name = "Condición Grano")]
    public string CondicionGrano { get; set; }
    public virtual IEnumerable<SelectListItem> CondicionGranoList { get; set; }

    public string Observacion { get; set; }
    public void SetObservacion(string Observacion)
    {
      this.Observacion = Observacion;
    }

    public string GetObservacion()
    {
      return this.Observacion;
    }
  }
}
