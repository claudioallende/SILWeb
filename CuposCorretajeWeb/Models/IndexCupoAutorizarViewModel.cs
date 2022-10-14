using CuposCorretajeWeb.Models.Identity;
using CuposCorretajeWeb.Models.Vista;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CuposCorretajeWeb.Models
{
    public class IndexCupoAutorizarViewModel
    {
        public int ProductoSeleccionado { get; set; }
        public IList<CuposAgrupados> CuposAgrupados { get; set; }
        [Display(Name = "Producto")]
        public virtual IEnumerable<SelectListItem> Productos { get; set; }
        public bool TieneFiltroGrano { get; set; }

        public IndexCupoAutorizarViewModel()
        {
            this.Productos = new List<SelectListItem>();
            this.CuposAgrupados = new List<CuposAgrupados>();
        }

        public IList<string> OcultarColumnas()
        {
            IList<string> Columnas = new List<string>();
            if (ProductoSeleccionado != 0)
            {
                Columnas.Add("Grano");
                TieneFiltroGrano = true;
            }
            else
            {
                TieneFiltroGrano = false;
            }
            return Columnas;
        }
        public IList<ColumnaTablaHTML> ColumnasAMostrar { get { return FiltrarColumnas(); } }
        public IList<ColumnaTablaHTML> FiltrarColumnas()
        {
            var ColumnasAMostrar = new List<ColumnaTablaHTML>();
            if (!TieneFiltroGrano)
            {
                ColumnasAMostrar = new List<CuposCorretajeWeb.Models.Vista.ColumnaTablaHTML> { 
                    new CuposCorretajeWeb.Models.Vista.ColumnaTablaHTML { Nombre = "Grano", Tamano = 10 },
                    new CuposCorretajeWeb.Models.Vista.ColumnaTablaHTML { Nombre = "Comprador", Tamano = 25 },
                    new CuposCorretajeWeb.Models.Vista.ColumnaTablaHTML { Nombre = "Puerto", Tamano = 25 },
                    new CuposCorretajeWeb.Models.Vista.ColumnaTablaHTML { Nombre = "Vendedor", Tamano = 20 }
                };
            }
            if (TieneFiltroGrano)
            {
                ColumnasAMostrar = new List<CuposCorretajeWeb.Models.Vista.ColumnaTablaHTML> { 
                    new CuposCorretajeWeb.Models.Vista.ColumnaTablaHTML { Nombre = "Comprador", Tamano = 30 },
                    new CuposCorretajeWeb.Models.Vista.ColumnaTablaHTML { Nombre = "Puerto", Tamano = 30 },
                    new CuposCorretajeWeb.Models.Vista.ColumnaTablaHTML { Nombre = "Vendedor", Tamano = 20 }
                };
            }
            return ColumnasAMostrar;
        }
        public TablaIndexViewModel TablaCupos {
            get
            {
                return new TablaIndexViewModel 
                { 
                    Columnas = ColumnasAMostrar, 
                    Datos = CuposAgrupados,
                    TieneFiltroGrano = TieneFiltroGrano
                }; 
            } 
        }
    }
}