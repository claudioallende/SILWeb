using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models
{
    public class FiltroCentro
    {
        public string CentroOrigen { get; set; }
        public string CentroDistribucion { get; set; }

        public bool IsOneEmpty()
        {
            return (string.IsNullOrEmpty(this.CentroDistribucion) || string.IsNullOrEmpty(this.CentroOrigen));
        }
    }
}