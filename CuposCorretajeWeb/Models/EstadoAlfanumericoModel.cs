using CuposCorretajeWeb.Models.Cupo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models
{
    public class EstadoAlfanumericoModel
    {
        public virtual string Alfanumerico { get; set; }
        public virtual string Comprador { get; set; }
        public virtual string Vendedor { get; set; }
        public virtual long CuentaVendedor { get; set; }
        public virtual string Puerto { get; set; }
        public virtual string Grano { get; set; }
        public virtual DateTime Fecha { get; set; }
        public virtual int Status { get; set; }
        public virtual int Pdf { get; set; }
        public virtual long Uvalue { get; set; }
        public virtual long Vendcyo { get; set; }
        public virtual EstadoCupo Estado { get; set; }
        public virtual string CodCentroOrigen { get; set; }
        public virtual string CodCentroDistribucion { get; set; }
        public virtual string DetalleCupoCNRT { get; set; }
        public virtual long IdCupo { get; set; }

        //public virtual EstadoCupo GetEstado()
        //{
        //    if (this.Estado == null)
        //    {
        //        Cupos Cupo = new Cupos
        //        {
        //            Status = this.Status,
        //            Pdf = this.Pdf,
        //            Uvcupodist = this.Uvalue,
        //            Vendcyo = this.Vendcyo,
        //            Vendcta = CuentaVendedor
        //        };
        //        this.Estado = Cupo.GetEstado();
        //    }
        //    return this.Estado;
        //}

        public override bool Equals(object obj)
        {
            EstadoAlfanumericoModel recievedObject = (EstadoAlfanumericoModel)obj;

            if (ReferenceEquals(recievedObject, null)) return false;
            if (ReferenceEquals(recievedObject, this)) return true;

            if ((this.Alfanumerico == recievedObject.Alfanumerico) && (this.Comprador == recievedObject.Comprador))
            {
                return (true);
            }
            return (false);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class RespuestaEstadoAlfanumericoViewModel
    {
        public string Alfanumerico { get; set; }
        public int Estado { get; set; }
        public string NombreEstado { get; set; }
        public string Fecha { get; set; }
        public string Comprador { get; set; }
        public string Vendedor { get; set; }
        public string Puerto { get; set; }
        public string Grano { get; set; }
        public string Centro { get; set; }
        public string CentroDistribucion { get; set; }
        public string DetalleCupoCNRT { get; set; }
        public long IdCupo { get; set; }
    }
}