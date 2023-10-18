using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models
{
  public class VistaCuposDistribuidos
  {
    public Int64 Compcta { get; set; }
    public string Cuitcomp { get; set; }
    public string Comprador { get; set; }
    public Int64 Vendcta { get; set; }
    public string Cuitvend { get; set; }
    public string Vendedor { get; set; }
    public int Codproducto { get; set; }
    public string Producto { get; set; }
    public string Cosecha { get; set; }
    public Int64 Ctadestino { get; set; }
    public string Destino { get; set; }
    public string Zonainfluencia { get; set; }
    public string Codcentro { get; set; }
    public string Centro { get; set; }
    public string Tipcta { get; set; }
    public int Pactado { get; set; }
    public int Pendentrega { get; set; }
    public int Fijado { get; set; }
    public int Liquidado { get; set; }
    public int Pendaplicar { get; set; }
    public int Cuposadist { get; set; }
    public int Cuposotorgados { get; set; }
    public int Cupostotalesadist { get; set; }
    public int Fechaent { get; set; }
    public int Fechavto { get; set; }
    public int Ayer { get; set; }
    public int Hoy { get; set; }
    public int Dia1 { get; set; }
    public int Dia2 { get; set; }
    public int Dia3 { get; set; }
    public int Dia4 { get; set; }
    public int Dia5 { get; set; }
    public int Dia6 { get; set; }
    public int Dia7 { get; set; }
    public int Dia8 { get; set; }
    public int Dia9 { get; set; }
    public int Dia10 { get; set; }
    public int Dia11 { get; set; }
    public int Dia12 { get; set; }
    public int Dia13 { get; set; }
    public int Dia14 { get; set; }
    public int Dia15 { get; set; }
    public int Dia16 { get; set; }
    public int Dia17 { get; set; }
    public int Dia18 { get; set; }
    public int Dia19 { get; set; }
    public int Dia20 { get; set; }
    public int Pedidosayer { get; set; }
    public int Pedidoshoy { get; set; }
    public int Pedidosdia1 { get; set; }
    public int Pedidosdia2 { get; set; }
    public int Pedidosdia3 { get; set; }
    public int Pedidosdia4 { get; set; }
    public int Pedidosdia5 { get; set; }
    public int Pedidosdia6 { get; set; }
    public int Pedidosdia7 { get; set; }
    public int Pedidosdia8 { get; set; }
    public int Pedidosdia9 { get; set; }
    public int Pedidosdia10 { get; set; }
    public int Pedidosdia11 { get; set; }
    public int Pedidosdia12 { get; set; }
    public int Pedidosdia13 { get; set; }
    public int Pedidosdia14 { get; set; }
    public int Pedidosdia15 { get; set; }
    public int Pedidosdia16 { get; set; }
    public int Pedidosdia17 { get; set; }
    public int Pedidosdia18 { get; set; }
    public int Pedidosdia19 { get; set; }
    public int Pedidosdia20 { get; set; }
    public string Cuitsolicitante { get; set; }
    public string Cuitintermediario { get; set; }
    public string Cuitrtecomercial { get; set; }
    public string Cuitcorrcomp { get; set; }
    public string Cuitmat { get; set; }
    public string Cuitcorrvta { get; set; }
    public string Cuitrteent { get; set; }
    public string Cuitdestinatario { get; set; }
    public string CuitRteComercialProductor { get; set; }
    public string CuitRteComercialVentaPrimaria { get; set; }
    public int Vendcyo { get; set; }
    public string Caratula { get; set; }
    public string ContactoComercial { get; set; }

    public Consignacion GetConsignacion()
    {
      return new Consignacion
      {
        Cuitsolicitante = this.Cuitsolicitante,
        Cuitintermediario = this.Cuitintermediario,
        Cuitrtecomercial = this.Cuitrtecomercial,
        Cuitcorrcomp = this.Cuitcorrcomp,
        Cuitmat = this.Cuitmat,
        Cuitcorrvta = this.Cuitcorrvta,
        Cuitrteent = this.Cuitrteent,
        Cuitdestinatario = this.Cuitdestinatario
      };
    }

    public void SetConsignacion(Consignacion consignacion)
    {
      this.Cuitsolicitante = consignacion.Cuitsolicitante;
      this.Cuitintermediario = consignacion.Cuitintermediario;
      this.Cuitrtecomercial = consignacion.Cuitrtecomercial;
      this.Cuitcorrcomp = consignacion.Cuitcorrcomp;
      this.Cuitmat = consignacion.Cuitmat;
      this.Cuitcorrvta = consignacion.Cuitcorrvta;
      this.Cuitdestinatario = consignacion.Cuitdestinatario;
    }

    public override bool Equals(object obj)
    {
      VistaCuposDistribuidos recievedObject = (VistaCuposDistribuidos)obj;

      if (ReferenceEquals(recievedObject, null)) return false;
      if (ReferenceEquals(recievedObject, this)) return true;

      if ((this.Compcta == recievedObject.Compcta)
          && (this.Codproducto == recievedObject.Codproducto)
          //&& (this.Cosecha == recievedObject.Cosecha)
          && (this.Vendcta == recievedObject.Vendcta)
          //&& (this.Ctadestino == recievedObject.Ctadestino)
          && (this.Codcentro == recievedObject.Codcentro)
          )
      {
        return (true);
      }
      return (false);
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    public int GetAcumuladoDias()
    {
      int acumulado = this.Hoy + this.Dia1 + this.Dia2 + this.Dia3 + this.Dia4 + this.Dia5 + this.Dia6 + this.Dia7 + this.Dia8 + this.Dia9;
      acumulado += +this.Dia10 + this.Dia11 + this.Dia12 + this.Dia13 + this.Dia14 + this.Dia15 + this.Dia16 + this.Dia17 + this.Dia18 + this.Dia19 + this.Dia20;
      return acumulado;
    }

    public bool SuperaCuposADist()
    {
      return this.Cuposadist >= GetAcumuladoDias();
    }
  }
}