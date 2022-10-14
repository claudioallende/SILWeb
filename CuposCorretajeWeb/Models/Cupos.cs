using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using CuposCorretajeWeb.Models.Cupo;

namespace CuposCorretajeWeb.Models
{
    public class Cupos : ICloneable
    {
        [Display(Name = "Id")]
        public virtual long Id { get; set; }
        [Display(Name = "Grano")]
        public virtual int Grano { get; set; }
        [Display(Name = "Comprador")]
        public virtual Int64 Compcta { get; set; }
        [Display(Name = "Puerto")]
        public virtual Int64 Puerto { get; set; }
        [Display(Name = "Vendedor")]
        public virtual Int64 Vendcta { get; set; }
        [Display(Name = "Vendedor CyO")]
        public virtual Int64 Vendcyo { get; set; }
        [Display(Name = "Solicitante")]
        public virtual string Cuitsolicitante { get; set; }
        [Display(Name = "Nombre")]
        public virtual string Nomsolicitante { get; set; }
        [Display(Name = "Intermediario")]
        public virtual string Cuitintermediario { get; set; }
        [Display(Name = "Nombre")]
        public virtual string Nomintermediario { get; set; }
        [Display(Name = "Comercial")]
        public virtual string Cuitrtecomercial { get; set; }
        [Display(Name = "Nombre")]
        public virtual string Nomrtecomercial { get; set; }
        [Display(Name = "Corredor Comprador")]
        public virtual string Cuitcorrcomp { get; set; }
        [Display(Name = "Nombre")]
        public virtual string Nomcorrcomp { get; set; }
        [Display(Name = "Mercado a Termino")]
        public virtual string Cuitmat { get; set; }
        [Display(Name = "Nombre")]
        public virtual string Nommat { get; set; }
        [Display(Name = "Corredor Vendedor")]
        public virtual string Cuitcorrvta { get; set; }
        [Display(Name = "Nombre")]
        public virtual string Nomcorrvta { get; set; }
        [Display(Name = "Entregador")]
        public virtual string Cuitrteent { get; set; }
        [Display(Name = "Nombre")]
        public virtual string Nomrteent { get; set; }
        [Display(Name = "Destinatario")]
        public virtual string Cuitdestinatario { get; set; }
        [Display(Name = "Nombre")]
        public virtual string Nomdestinatario { get; set; }
        [Display(Name = "Remitente Comercial Productor")]
        public virtual string CuitRteComercialProductor { get; set; }
        [Display(Name = "Nombre")]
        public virtual string NomRteComercialProductor { get; set; }
        [Display(Name = "Remitente Comercial Venta Primaria")]
        public virtual string CuitRteComercialVentaPrimaria { get; set; }
        [Display(Name = "Nombre")]
        public virtual string NomRteComercialVentaPrimaria { get; set; }
        public virtual string Alfadia0 { get; set; }
        public virtual string Alfadia1 { get; set; }
        public virtual string Alfadia2 { get; set; }
        public virtual string Alfadia3 { get; set; }
        public virtual string Alfadia4 { get; set; }
        public virtual string Alfadia5 { get; set; }
        [Display(Name = "Centro")]
        public virtual string Centro { get; set; }
        public virtual string Centrodist { get; set; }
        [Display(Name = "Contrato")]
        public virtual string Contrato { get; set; }
        [Display(Name = "Corredor")]
        public virtual long Corrcta { get; set; }
        [Display(Name = "Cupos Otorgados")]
        public virtual int Cuposotorgados { get; set; }
        [Display(Name = "Cupos Pedidos")]
        public virtual int Cupospedidos { get; set; }
        [Display(Name = "Cupos Recibidos")]
        public virtual int Cuposrecibidos { get; set; }
        public virtual DateTime Dia0 { get; set; }
        public virtual DateTime Dia1 { get; set; }
        public virtual DateTime Dia2 { get; set; }
        public virtual DateTime Dia3 { get; set; }
        public virtual DateTime Dia4 { get; set; }
        public virtual DateTime Dia5 { get; set; }
        public virtual DateTime Dia6 { get; set; }
        public virtual DateTime Dia7 { get; set; }
        public virtual DateTime Dia8 { get; set; }
        public virtual DateTime Dia9 { get; set; }
        public virtual DateTime Dia10 { get; set; }
        public virtual DateTime Dia11 { get; set; }
        public virtual DateTime Dia12 { get; set; }
        public virtual DateTime Dia13 { get; set; }
        public virtual DateTime Dia14 { get; set; }
        public virtual DateTime Dia15 { get; set; }
        public virtual DateTime Dia16 { get; set; }
        public virtual DateTime Dia17 { get; set; }
        public virtual DateTime Dia18 { get; set; }
        public virtual DateTime Dia19 { get; set; }
        public virtual DateTime Dia20 { get; set; }
        [Display(Name = "Fecha")]
        public virtual DateTime Fecha { get; set; }
        [Display(Name = "Cupo Nro.")]
        public virtual string Nrocupo { get; set; }
        [Display(Name = "Status")]
        public virtual int Status { get; set; }
        [Display(Name = "Tipo")]
        public virtual int Tipo { get; set; }
        [Display(Name = "Origen")]
        public virtual Int64 Idorigen { get; set; }
        public virtual long Uvcupodist { get; set; }
        public virtual string Motbaja { get; set; }
        public virtual string Observa { get; set; }
        public virtual int Pdf { get; set; }
        public virtual EstadoCupo Estado { get; set; }
        public virtual string Usuario { get; set; }

        public virtual bool VendcyoBoolValue
        {
            get { return Vendcyo != 0; }
            set
            {
                if (value)
                    Vendcyo = Vendcta;
                else
                    Vendcyo = 0;
            }
        }

        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }

        public virtual Consignacion GetConsignacion()
        {
            return new Consignacion
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

        public virtual Consignacion GetConsignacionTrim()
        {
            return new Consignacion
            {
                Cuitsolicitante = string.IsNullOrEmpty(this.Cuitsolicitante) ? null : this.Cuitsolicitante.Trim(),
                Nomsolicitante = string.IsNullOrEmpty(this.Nomsolicitante) ? null : this.Nomsolicitante.Trim(),
                Cuitintermediario = string.IsNullOrEmpty(this.Cuitintermediario) ? null : this.Cuitintermediario.Trim(),
                Nomintermediario = string.IsNullOrEmpty(this.Nomintermediario) ? null : this.Nomintermediario.Trim(),
                Cuitrtecomercial = string.IsNullOrEmpty(this.Cuitrtecomercial) ? null : this.Cuitrtecomercial.Trim(),
                Nomrtecomercial = string.IsNullOrEmpty(this.Nomrtecomercial) ? null : this.Nomrtecomercial.Trim(),
                Cuitcorrcomp = string.IsNullOrEmpty(this.Cuitcorrcomp) ? null : this.Cuitcorrcomp.Trim(),
                Nomcorrcomp = string.IsNullOrEmpty(this.Nomcorrcomp) ? null : this.Nomcorrcomp.Trim(),
                Cuitmat = string.IsNullOrEmpty(this.Cuitmat) ? null : this.Cuitmat.Trim(),
                Nommat = string.IsNullOrEmpty(this.Nommat) ? null : this.Nommat.Trim(),
                Cuitcorrvta = string.IsNullOrEmpty(this.Cuitcorrvta) ? null : this.Cuitcorrvta.Trim(),
                Nomcorrvta = string.IsNullOrEmpty(this.Nomcorrvta) ? null : this.Nomcorrvta.Trim(),
                Cuitrteent = string.IsNullOrEmpty(this.Cuitrteent) ? null : this.Cuitrteent.Trim(),
                Nomrteent = string.IsNullOrEmpty(this.Nomrteent) ? null : this.Nomrteent.Trim(),
                Cuitdestinatario = string.IsNullOrEmpty(this.Cuitdestinatario) ? null : this.Cuitdestinatario.Trim(),
                Nomdestinatario = string.IsNullOrEmpty(this.Nomdestinatario) ? null : this.Nomdestinatario.Trim()
            };
        }

        public virtual bool EsCuentaYOrden()
        {
            return this.Vendcyo != 0;
        }

        public virtual bool EsCuentaYOrdenPadre()
        {
            return (this.Vendcyo != 0 && this.Vendcyo == this.Vendcta);
        }

    }
}