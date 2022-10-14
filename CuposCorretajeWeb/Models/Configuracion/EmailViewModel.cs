using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models.Email
{
    public class EmailViewModel
    {
        [Display(Name = "Cuenta")]
        public string NumeroCuenta { get; set; }
        [Required]
        [Display(Name = "Nombre")]
        public string NombreCuenta { get; set; }
        [Required]
        public string Cuit { get; set; }
        [RegularExpression(@"^(([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)(\s*;\s*|\s*$))*$", ErrorMessage = "Dirección de correo electrónico no válido.")]
        public string EMail { get; set; }
    }
}