using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models.Cupo
{
    public class TransicionDeEstado
    {
        public string Nombre { get; set; }
        public string Valor { get; set; }
        public string VOld { get; set; }
        public string VNew { get; set; }
        public string Tipo { get; set; }
    }
}