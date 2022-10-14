using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models
{
    public class CuposDist
    {
        public virtual Int64 Uvalue { get; set; }
        public virtual Int64 Ctacomp { get; set; }
        public virtual Int64 CtaVend { get; set; }
        public virtual int Grano { get; set; }
        public virtual Int64 Destino { get; set; }
        public virtual string Centro { get; set; }
        public virtual DateTime Fecha { get; set; }
        public virtual int Cupos { get; set; }
        public virtual int Cupospedidos { get; set; }
        public virtual Int64 Contrato { get; set; }
        public virtual string Cuitsolicitante { get; set; }
        public virtual string Cuitintermediario { get; set; }
        public virtual string Cuitrtecomercial { get; set; }
        public virtual string Cuitcorrcomp { get; set; }
        public virtual string Cuitmat { get; set; }
        public virtual string Cuitcorrvta { get; set; }
        public virtual string Cuitrteent { get; set; }
        public virtual string Cuitdestinatario { get; set; }
        public virtual string Usuario { get; set; }

        public CuposDist() { }

        public CuposDist(long CuentaComprador, long CuentaVendedor, int CodigoGrano, long CuentaDestino, string Centro, DateTime Fecha, Consignacion Consignacion) {
            this.Ctacomp = CuentaComprador;
            this.CtaVend = CuentaVendedor;
            this.Grano = CodigoGrano;
            this.Destino = CuentaDestino;
            this.Centro = Centro;
            this.Fecha = Fecha;
            this.Cuitsolicitante = Consignacion.Cuitsolicitante;
            this.Cuitintermediario = Consignacion.Cuitintermediario;
            this.Cuitrtecomercial = Consignacion.Cuitrtecomercial;
            this.Cuitcorrcomp = Consignacion.Cuitcorrcomp;
            this.Cuitmat = Consignacion.Cuitmat;
            this.Cuitcorrvta = Consignacion.Cuitcorrvta;
            this.Cuitrteent = Consignacion.Cuitrteent;
            this.Cuitdestinatario = Consignacion.Cuitdestinatario;
        }

        public virtual Consignacion GetConsignacion()
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

        public virtual void SetConsignacion(Consignacion consignacion)
        {
            this.Cuitsolicitante = consignacion.Cuitsolicitante;
            this.Cuitintermediario = consignacion.Cuitintermediario;
            this.Cuitrtecomercial = consignacion.Cuitrtecomercial;
            this.Cuitcorrcomp = consignacion.Cuitcorrcomp;
            this.Cuitmat = consignacion.Cuitmat;
            this.Cuitcorrvta = consignacion.Cuitcorrvta;
            this.Cuitdestinatario = consignacion.Cuitdestinatario;
        }
    }
}