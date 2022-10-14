using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models.Cupo
{
    public abstract class EstadoCupo
    {
        public string Nombre { get; internal set; }
        public CodigoEstado Codigo { get; internal set; }
        public int Status { get; internal set; }
        public bool Informado { get; internal set; }
        public bool EstaDistribuido { get; internal set; }
        public string Mensaje { get; set; }
        protected Cupos Cupo { get; set; }
        protected Cupos CupoHijo { get; set; }
        protected Cupos CupoPadre { get; set; }

        public EstadoCupo(Cupos Cupo)
        {
            this.Status = Cupo.Status;
            this.Informado = (Cupo.Pdf == 1);
            this.EstaDistribuido = (Cupo.Uvcupodist != 0);
            this.Mensaje = "";
            this.Cupo = Cupo;

            this.Nombre = "";
            this.Codigo = 0;
        }

        protected bool EsCuentaYOrden()
        {
            return this.Cupo.Vendcyo != 0;
        }

        protected InformacionModificacionEstado GetInformacion()
        {
            return new InformacionModificacionEstado { CupoPadreCyOModificado = this.CupoPadre, CupoHijoCyOModificado = this.CupoHijo };
        }
    }

    public enum CodigoEstado : int
    {
        Creado = 0,
        DistribuidoPendienteInformar = 1,
        DistribuidoInformado = 2,
        DistribucionAnuladaPendienteInformar = 3,
        DistribucionAnuladaInformada = 4,
        AnuladoPendienteInformar = 5,
        Anulado = 6,
        CuentaYOrdenCreado = 100,
        CuentaYOrdenDistribuidoPendienteDistribuirHijo = 101,
        CuentaYOrdenDistribuidoConHijoDistribuido = 102,
        CuentaYOrdenAnulado = 103
    }
}