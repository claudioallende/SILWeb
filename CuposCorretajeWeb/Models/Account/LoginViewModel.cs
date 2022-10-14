using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models.Account
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Usuario")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }
    }

    public class LoginAcabaseViewModel
    {
        [Required]
        [Display(Name = "Usuario")]
        public string Usuario { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        public string EsCyo { get; set; }

        public bool IsAuthenticated()
        {
            if (!string.IsNullOrEmpty(this.Usuario) && !string.IsNullOrEmpty(this.Password)) return true;
            else return false;
        }
    }
}