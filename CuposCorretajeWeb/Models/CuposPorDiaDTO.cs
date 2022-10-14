using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models
{
    public class CuposPorDiaDTO
    {
        [Display(Name = "Fecha")]
        [Required]
        public virtual DateTime Fecha { get; set; }
        [Display(Name = "Turnos")]
        [Required]
        public virtual IList<String> Turnos { get; set; }
    }
}