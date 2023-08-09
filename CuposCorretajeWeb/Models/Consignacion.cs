using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models
{
  public class Consignacion
  {
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

    [Display(Name = "Carátula")]
    public string Caratula { get; set; }

    public string Observacion { get; set; }

    public void SetObservacion(string Observacion)
    {
      this.Observacion = Observacion;
    }

    public string GetObservacion()
    {
      return this.Observacion;
    }

    public override bool Equals(Object obj)
    {
      //Check for null and compare run-time types.
      if ((obj == null) || !this.GetType().Equals(obj.GetType()))
      {
        return false;
      }
      else
      {
        Consignacion c = (Consignacion)obj;
        return (Cuitsolicitante == c.Cuitsolicitante) && (Cuitintermediario == c.Cuitintermediario) &&
            (Cuitrtecomercial == c.Cuitrtecomercial) && (Cuitcorrcomp == c.Cuitcorrcomp) &&
            (Cuitmat == c.Cuitmat) && (Cuitcorrvta == c.Cuitcorrvta) &&
            (Cuitrteent == c.Cuitrteent) && (Cuitdestinatario == c.Cuitdestinatario);
      }
    }

    public Cupos GetConsignacion()
    {
      return new Cupos
      {
        Cuitsolicitante = this.Cuitsolicitante,
        Nomsolicitante = this.Nomsolicitante,
        Cuitintermediario = this.Cuitintermediario,
        Nomintermediario = this.Nomintermediario,
        Cuitrtecomercial = this.Cuitrtecomercial,
        Nomrtecomercial = this.Nomrtecomercial,
        Cuitcorrcomp = this.Cuitcorrcomp,
        Nomcorrcomp = this.Nomcorrcomp,
        Cuitmat = this.Cuitmat,
        Nommat = this.Nommat,
        Cuitcorrvta = this.Cuitcorrvta,
        Nomcorrvta = this.Nomcorrvta,
        Cuitrteent = this.Cuitrteent,
        Nomrteent = this.Nomrteent,
        Cuitdestinatario = this.Cuitdestinatario,
        Nomdestinatario = this.Nomdestinatario
      };
    }

    public IQueryable<Cupos> FiltroConsignacion(IQueryable<Cupos> cupos)
    {
      if (this.Cuitsolicitante == null || this.Cuitsolicitante.Trim() == "")
      {
        cupos = cupos.Where(x => x.Cuitsolicitante == null || x.Cuitsolicitante.Trim() == "");
      }
      else
      {
        cupos = cupos.Where(x => this.Cuitsolicitante == x.Cuitsolicitante);
      }
      if (this.Cuitintermediario == null || this.Cuitintermediario.Trim() == "")
      {
        cupos = cupos.Where(x => x.Cuitintermediario == null || x.Cuitintermediario.Trim() == "");
      }
      else
      {
        cupos = cupos.Where(x => this.Cuitintermediario == x.Cuitintermediario);
      }
      if (this.Cuitrtecomercial == null || this.Cuitrtecomercial.Trim() == "")
      {
        cupos = cupos.Where(x => x.Cuitrtecomercial == null || x.Cuitrtecomercial.Trim() == "");
      }
      else
      {
        cupos = cupos.Where(x => this.Cuitrtecomercial == x.Cuitrtecomercial);
      }
      if (this.Cuitcorrcomp == null || this.Cuitcorrcomp.Trim() == "")
      {
        cupos = cupos.Where(x => x.Cuitcorrcomp == null || x.Cuitcorrcomp.Trim() == "");
      }
      else
      {
        cupos = cupos.Where(x => this.Cuitcorrcomp == x.Cuitcorrcomp);
      }
      if (this.Cuitmat == null || this.Cuitmat.Trim() == "")
      {
        cupos = cupos.Where(x => x.Cuitmat == null || x.Cuitmat.Trim() == "");
      }
      else
      {
        cupos = cupos.Where(x => this.Cuitmat == x.Cuitmat);
      }
      if (this.Cuitcorrvta == null || this.Cuitcorrvta.Trim() == "")
      {
        cupos = cupos.Where(x => x.Cuitcorrvta == null || x.Cuitcorrvta.Trim() == "");
      }
      else
      {
        cupos = cupos.Where(x => this.Cuitcorrvta == x.Cuitcorrvta);
      }
      if (this.Cuitrteent == null || this.Cuitrteent.Trim() == "")
      {
        cupos = cupos.Where(x => x.Cuitrteent == null || x.Cuitrteent.Trim() == "");
      }
      else
      {
        cupos = cupos.Where(x => this.Cuitrteent == x.Cuitrteent);
      }
      if (this.Cuitdestinatario == null || this.Cuitdestinatario.Trim() == "")
      {
        cupos = cupos.Where(x => x.Cuitdestinatario == null || x.Cuitdestinatario.Trim() == "");
      }
      else
      {
        cupos = cupos.Where(x => this.Cuitdestinatario == x.Cuitdestinatario);
      }
      return cupos;
    }
  }
}