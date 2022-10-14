using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models.Filtro
{
    public class FiltroPorDefecto
    {
        public static string GetFiltroPorDefectoCosechaDesde()
        {
            //return DateTime.Now.AddYears(-10).ToString("yy") + DateTime.Now.AddYears(-9).ToString("yy");
            return ConfigurationManager.AppSettings["cosechaDesdeFiltroDistribucion"];
        }

        public static string GetFiltroPorDefectoCosechaHasta()
        {
            //return DateTime.Now.ToString("yy") + DateTime.Now.AddYears(1).ToString("yy");
            return ConfigurationManager.AppSettings["cosechaHastaFiltroDistribucion"];
        }

        public static DateTime? GetFiltroPorDefectoFechaDesde()
        {
            //return null;
            return DateTime.Parse(ConfigurationManager.AppSettings["fechaDesdeFiltroDistribucion"]);
        }

        public static DateTime GetFiltroPorDefectoFechaHasta()
        {
            return DateTime.Parse(ConfigurationManager.AppSettings["fechaHastaFiltroDistribucion"]);
        }
    }
}