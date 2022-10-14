using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models
{
    public class TurnoViewModel
    {
        public IList<string> ListaCodigosAlfanumericosBuscador { get; set; }
        public IList<Cupos> ListaCupos { get; set; }
    }
}