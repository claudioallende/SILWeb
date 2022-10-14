using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models.Contratos
{
    public class Vista_CuposMpeCorreV3
    {
        public virtual int Empresa { get; set; }
        public virtual string Numcorrel	{ get; set; }
        public virtual long Carporte { get; set; }
        public virtual long Compcta	{ get; set; }
        public virtual string Cuitcomp { get; set; }
        public virtual string Comprador { get; set; }
        public virtual long Vendcta { get; set; }
        public virtual string Cuitvend { get; set; }
        public virtual string Vendedor { get; set; }
        public virtual int Producto { get; set; }
        public virtual string Nomgrano { get; set; }
        public virtual string Cosecha { get; set; }
        public virtual long Ctadestino { get; set; }
        public virtual string Destino { get; set; }
        public virtual string Zonainfluencia { get; set; }
        public virtual string Codcentro { get; set; }
        public virtual string Centro { get; set; }
        public virtual string Tipcta { get; set; }
        public virtual string Opera { get; set; }
        public virtual long Pactado { get; set; }
        public virtual long Pendentrega	{ get; set; }
        public virtual long Fijado { get; set; }
        public virtual long Liquidado { get; set; }
        public virtual double Pendaplicar { get; set; }
        public virtual long Transito { get; set; }
        public virtual long Cupotorgado { get; set; }
        public virtual DateTime Fecha { get; set; }

        public override bool Equals(object obj)
        {
            Vista_CuposMpeCorreV3 recievedObject = (Vista_CuposMpeCorreV3)obj;

            if (ReferenceEquals(recievedObject, null)) return false;
            if (ReferenceEquals(recievedObject, this)) return true;

            if ((this.Compcta == recievedObject.Compcta)
                && (this.Producto == recievedObject.Producto)
                && (this.Cosecha == recievedObject.Cosecha)
                && (this.Vendcta == recievedObject.Vendcta)
                && (this.Ctadestino == recievedObject.Ctadestino)
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
    }
}