using CuposCorretajeWeb.Models.Vista;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models
{
    public class TablaTurnosPendienteAutorizarViewModel
    {
        public IList<ColumnaTablaHTML> Columnas { get; set; }
        public IList<CuposAgrupados> Datos { get; set; }
        public readonly int CantidadDias = 20;
    }
}