using CuposCorretajeWeb.Models.Vista;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models
{
    public class TablaIndexViewModel
    {
        public IList<CuposAgrupados> Datos { get; set; }
        public IList<ColumnaTablaHTML> Columnas { get; set; }
        public bool TieneFiltroGrano { get; set; }
        public bool TieneUnCentro { get; set; }
        public readonly int CantidadDias = 20;
    }
}