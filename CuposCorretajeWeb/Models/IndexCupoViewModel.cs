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
    public class IndexCupoViewModel
    {
        public int ProductoSeleccionado { get; set; }
        public IList<CuposAgrupados> CuposAgrupados { get; set; }
        public IList<CuposAgrupados> CuposAgrupadosCyO { get; set; }
        [Display(Name = "Producto")]
        public virtual IEnumerable<SelectListItem> Productos { get; set; }
        [Display(Name = "Centro")]
        public virtual IEnumerable<SelectListItem> Centros { get; set; }
        public IList<string> CentroSeleccionado { get; set; }
        public bool TieneFiltroGrano { get; set; }
        public string Comprador { get; set; }
        public string Vendedor { get; set; }

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
            IList<string> Columnas = OcultarColumnas();
            var ColumnasAMostrar = new List<ColumnaTablaHTML>();
            bool TieneUnCentro = (ClaimsUtil.GetListClaims("Centro").Count == 1);
            if (!TieneFiltroGrano)
            {
                if (TieneUnCentro)
                {
                    ColumnasAMostrar = new List<CuposCorretajeWeb.Models.Vista.ColumnaTablaHTML> { 
                        new CuposCorretajeWeb.Models.Vista.ColumnaTablaHTML { Nombre = "Grano", Tamano = 10 },
                        new CuposCorretajeWeb.Models.Vista.ColumnaTablaHTML { Nombre = "Comprador", Tamano = 25 },
                        new CuposCorretajeWeb.Models.Vista.ColumnaTablaHTML { Nombre = "Puerto", Tamano = 25 },
                    };
                }
                else
                {
                    ColumnasAMostrar = new List<CuposCorretajeWeb.Models.Vista.ColumnaTablaHTML> { 
                        new CuposCorretajeWeb.Models.Vista.ColumnaTablaHTML { Nombre = "Grano", Tamano = 10 },
                        new CuposCorretajeWeb.Models.Vista.ColumnaTablaHTML { Nombre = "Comprador", Tamano = 17 },
                        new CuposCorretajeWeb.Models.Vista.ColumnaTablaHTML { Nombre = "Puerto", Tamano = 18 },
                        new CuposCorretajeWeb.Models.Vista.ColumnaTablaHTML { Nombre = "Centro", Tamano = 7 },
                        new CuposCorretajeWeb.Models.Vista.ColumnaTablaHTML { Nombre = "Distribución", Tamano = 8 }
                    };
                }
            }
            if (TieneFiltroGrano)
            {
                if (TieneUnCentro)
                {
                    ColumnasAMostrar = new List<CuposCorretajeWeb.Models.Vista.ColumnaTablaHTML> { 
                        new CuposCorretajeWeb.Models.Vista.ColumnaTablaHTML { Nombre = "Comprador", Tamano = 30 },
                        new CuposCorretajeWeb.Models.Vista.ColumnaTablaHTML { Nombre = "Puerto", Tamano = 30 },
                    };
                }
                else
                {
                    ColumnasAMostrar = new List<CuposCorretajeWeb.Models.Vista.ColumnaTablaHTML> { 
                        new CuposCorretajeWeb.Models.Vista.ColumnaTablaHTML { Nombre = "Comprador", Tamano = 23 },
                        new CuposCorretajeWeb.Models.Vista.ColumnaTablaHTML { Nombre = "Puerto", Tamano = 23 },
                        new CuposCorretajeWeb.Models.Vista.ColumnaTablaHTML { Nombre = "Centro", Tamano = 7 },
                        new CuposCorretajeWeb.Models.Vista.ColumnaTablaHTML { Nombre = "Distribución", Tamano = 7 }
                    };
                }
            }
            return ColumnasAMostrar;
        }
        public TablaIndexViewModel TablaCuposNormales {
            get
            {
                return new TablaIndexViewModel 
                { 
                    Columnas = ColumnasAMostrar, 
                    Datos = CuposAgrupados,
                    TieneFiltroGrano = TieneFiltroGrano,
                    TieneUnCentro = (ClaimsUtil.GetListClaims("Centro").Count == 1)
                }; 
            } 
        }
        public TablaIndexViewModel TablaCuposCyO
        {
            get
            {
                return new TablaIndexViewModel
                {
                    Columnas = ColumnasAMostrar,
                    Datos = CuposAgrupadosCyO,
                    TieneFiltroGrano = TieneFiltroGrano,
                    TieneUnCentro = (ClaimsUtil.GetListClaims("Centro").Count == 1)
                };
            }
        }
    }
}