using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models.Configuracion
{
    public class PanelViewModel
    {
        public Consignacion Consignacion { get; set; }
        [RegularExpression("([01]?[0-9]|2[0-3]):[0-5][0-9]", ErrorMessage="Hora no válida. El formato correcto es hh:mm.")]
        [Display(Name="Hora límite de distribución del día actual")]
        public string HoraLimiteDistribucion { get; set; }
        [Display(Name = "Fecha desde por defecto de última distribución")]
        [RegularExpression(@"(((0[1-9]|[12][0-9]|3[01])([/])(0[13578]|10|12)([/])(\d{4}))|(([0][1-9]|[12][0-9]|30)([/])(0[469]|11)([/])(\d{4}))|((0[1-9]|1[0-9]|2[0-8])([/])(02)([/])(\d{4}))|((29)(\.|-|\/)(02)([/])([02468][048]00))|((29)([/])(02)([/])([13579][26]00))|((29)([/])(02)([/])([0-9][0-9][0][48]))|((29)([/])(02)([/])([0-9][0-9][2468][048]))|((29)([/])(02)([/])([0-9][0-9][13579][26])))", ErrorMessage = "Fecha no válida. El formato correcto es dd/mm/yyyy.")]
        public string FechaDesdeDistribucion { get; set; }
        [Display(Name = "Fecha hasta por defecto de última distribución")]
        [RegularExpression(@"(((0[1-9]|[12][0-9]|3[01])([/])(0[13578]|10|12)([/])(\d{4}))|(([0][1-9]|[12][0-9]|30)([/])(0[469]|11)([/])(\d{4}))|((0[1-9]|1[0-9]|2[0-8])([/])(02)([/])(\d{4}))|((29)(\.|-|\/)(02)([/])([02468][048]00))|((29)([/])(02)([/])([13579][26]00))|((29)([/])(02)([/])([0-9][0-9][0][48]))|((29)([/])(02)([/])([0-9][0-9][2468][048]))|((29)([/])(02)([/])([0-9][0-9][13579][26])))", ErrorMessage = "Fecha no válida. El formato correcto es dd/mm/yyyy.")]
        public string FechaHastaDistribucion { get; set; }
        [Display(Name = "Cosecha desde por defecto de última distribución")]
        [Range(0, int.MaxValue, ErrorMessage = "Ingrese solo números")]
        public string CosechaDesdeDistribucion { get; set; }
        [Display(Name = "Cosecha hasta por defecto de última distribución")]
        [Range(0, int.MaxValue, ErrorMessage = "Ingrese solo números")]
        public string CosechaHastaDistribucion { get; set; }
        [RegularExpression("([01]?[0-9]|2[0-3]):[0-5][0-9]", ErrorMessage = "Hora no válida. El formato correcto es hh:mm.")]
        [Display(Name = "Hora de envío de email de cupos distribuidos informados en STOP")]
        public string HoraEnvioMailSTOP { get; set; }
        [RegularExpression("([01]?[0-9]|2[0-3]):[0-5][0-9]", ErrorMessage = "Hora no válida. El formato correcto es hh:mm.")]
        [Display(Name = "Hora de envío de email de cupos distribuidos informados en STOP no cumplidos")]
        public string HoraEnvioMailNoCumplimiento { get; set; }
    }
}