using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models
{
    public class ClaveCupo
    {
        public long CuentaComprador { get; set; }
        public long CuentaVendedor { get; set; }
        public long CuentaPuerto { get; set; }
        public int CodigoGrano { get; set; }
    }
}