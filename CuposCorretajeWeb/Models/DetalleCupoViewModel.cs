using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models
{
    public class DetalleCupoViewModel
    {
        public int CodigoGrano { get; set; }
        public long CuentaComprador { get; set; }
        public long CuentaPuerto { get; set; }
        public string NombreComprador { get; set; }
        public string NombrePuerto { get; set; }
        public string CodigoCentro { get; set; }
        public string CodigoCentroDistribucion { get; set; }
        public string NombreCentro { get; set; }
        public string NombreCentroDistribucion { get; set; }
        public virtual IList<CuposAgrupadosDetalle> CuposAgrupados { get; set; }
        public int TotalDiarioDia0Cr { get; set; }
        public int TotalDiarioDia0Co { get; set; }
        public int TotalDiarioDia1Cr { get; set; }
        public int TotalDiarioDia1Co { get; set; }
        public int TotalDiarioDia2Cr { get; set; }
        public int TotalDiarioDia2Co { get; set; }
        public int TotalDiarioDia3Cr { get; set; }
        public int TotalDiarioDia3Co { get; set; }
        public int TotalDiarioDia4Cr { get; set; }
        public int TotalDiarioDia4Co { get; set; }
        public int TotalDiarioDia5Cr { get; set; }
        public int TotalDiarioDia5Co { get; set; }
        public readonly int CantidadDias = 20;
    }
}