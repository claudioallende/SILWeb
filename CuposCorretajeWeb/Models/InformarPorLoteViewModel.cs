using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models
{
    public class InformarPorLoteViewModel
    {
        public long CuentaComprador { get; set; }
        public long CuentaVendedor { get; set; }
        public long CuentaPuerto { get; set; }
        public int CodigoGrano { get; set; }
        public string CodigoCentro { get; set; }
        public string CodigoCentroDistribucion { get; set; }
        public bool Cyo { get; set; }
    }

    public class InformadosViewModel
    {
        public IList<EmailInformado> result { get; set; }
        public IList<CuposAgrupadosDetalle> model { get; set; }
    }

    public class EmailInformado {
        public long CuentaVendedor { get; set; }
        public int Estado { get; set; }
        public string Mensaje { get; set; }
        /// <summary>
        /// Si es anulacion o asignacion de distribucion
        /// </summary>
        public string TipoEmail { get; set; }

        public EmailInformado()
        {

        }

        public EmailInformado(int Estado)
        {
            if (Estado == 0) Mensaje = "OK";
            if (Estado == 1) Mensaje = "CORREO NO CONFIGURADO";
            //if (Estado == 2)
        }
    }
}