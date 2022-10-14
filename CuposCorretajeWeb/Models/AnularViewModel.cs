using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models
{
    public class AnularViewModel
    {

    }

    public class AnuladoViewModel
    {
        public long Id { get; set; }
        public string Alfanumerico { get; set; }
        public long CuentaVendedor { get; set; }
        public int Status { get; set; }
    }
}