using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models.Filtro
{
    public class FiltroDistribucion
    {
        public virtual long CodigoGrano { get; set; }
        public virtual long CuentaPuerto { get; set; }
        public virtual long CuentaComprador { get; set; }
        public virtual long CuentaVendedor { get; set; }

        public virtual DateTime? EntregaDesde { get; set; }
        public virtual DateTime? EntregaHasta { get; set; }
        public virtual string CosechaDesde { get; set; }
        public virtual string CosechaHasta { get; set; }
        public virtual string Centro { get; set; }
        public virtual DateTime Fecha { get; set; }
        public virtual DateTime Hora { get; set; }
        public virtual long Uvdist { get; set; }

        public FiltroDistribucion() { }

        public FiltroDistribucion(long CodigoGrano, long CuentaPuerto, long CuentaComprador)
        {
            this.CodigoGrano = CodigoGrano;
            this.CuentaPuerto = CuentaPuerto;
            this.CuentaComprador = CuentaComprador;
            this.EntregaDesde = default(DateTime);
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
                FiltroDistribucion f = (FiltroDistribucion)obj;
                return CodigoGrano == f.CodigoGrano && 
                    CuentaPuerto == f.CuentaPuerto && 
                    CuentaComprador == f.CuentaComprador && 
                    CuentaVendedor == f.CuentaVendedor;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public virtual bool IsFiltroIncompleto()
        {
            return 
                string.IsNullOrEmpty(this.Centro) ||
                this.CodigoGrano == 0 ||
                (string.IsNullOrEmpty(this.CosechaDesde) || this.CosechaDesde == "0") ||
                (string.IsNullOrEmpty(this.CosechaHasta) || this.CosechaHasta == "0") ||
                this.CuentaComprador == 0 ||
                this.CuentaPuerto == 0 ||
                this.CuentaVendedor == 0 ||
                (this.EntregaDesde == null || this.EntregaDesde == default(DateTime)) ||
                (this.EntregaHasta == null || this.EntregaHasta == default(DateTime));
        }

        /// <summary>
        /// Si EntregaDesde de este objeto es igual a null o valor por defecto entonces se setea el parametro en la propiedad del objeto
        /// </summary>
        /// <param name="EntregaDesde"></param>
        public virtual void SetEntregaDesde(DateTime? EntregaDesde)
        {
            if (this.EntregaDesde == null || this.EntregaDesde == default(DateTime)) this.EntregaDesde = EntregaDesde;
        }
        /// <summary>
        /// Si EntregaHasta de este objeto es igual a null o valor por defecto entonces se setea el parametro en la propiedad del objeto
        /// </summary>
        /// <param name="EntregaHasta"></param>
        public virtual void SetEntregaHasta(DateTime? EntregaHasta)
        {
            if (this.EntregaHasta == null || this.EntregaHasta == default(DateTime)) this.EntregaHasta = EntregaHasta;
        }
        /// <summary>
        /// Si CosechaDesde de este objeto es igual a null o valor por defecto entonces se setea el parametro en la propiedad del objeto
        /// </summary>
        /// <param name="CosechaDesde"></param>
        public virtual void SetCosechaDesde(string CosechaDesde)
        {
            if (string.IsNullOrEmpty(this.CosechaDesde) || this.CosechaDesde == "0") this.CosechaDesde = CosechaDesde;
        }
        /// <summary>
        /// Si CosechaHasta de este objeto es igual a null o valor por defecto entonces se setea el parametro en la propiedad del objeto
        /// </summary>
        /// <param name="CosechaHasta"></param>
        public virtual void SetCosechaHasta(string CosechaHasta)
        {
            if (string.IsNullOrEmpty(this.CosechaHasta) || this.CosechaHasta == "0") this.CosechaHasta = CosechaHasta;
        }
    }
}