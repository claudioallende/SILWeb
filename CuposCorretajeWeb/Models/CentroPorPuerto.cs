using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models
{
    public class CentroPorPuerto
    {
        public virtual long Id { get; set; }
        public virtual long Idterminal { get; set; }
        public virtual string CodigoCentro { get; set; }
    }

    public class CentroPorPuertoDTO
    {
        [Display(Name = "Código de Puerto")]
        public long Id { get; set; }
        [Display(Name = "Código de Terminal")]
        public long IdTerminal { get; set; }
        [Display(Name = "Cuenta de Terminal")]
        public long CuentaTerminal { get; set; }
        [Display(Name = "Nombre de Terminal")]
        public string NombreTerminal { get; set; }
        [Display(Name = "Código de Centro")]
        public string CodigoCentro { get; set; }
        [Display(Name = "Nombre de Centro")]
        public string NombreCentro { get; set; }
    }
}