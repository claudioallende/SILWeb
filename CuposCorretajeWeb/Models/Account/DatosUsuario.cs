using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace CuposCorretajeWeb.Models.Account
{
    public class DatosUsuario
    {
        public string USUARIO { get; set; }
        public string PASSWORD { get; set; }
        public string NOMBRE { get; set; }
        public IList<Claim> Claims { get; set; }

        public bool IsAuthenticated()
        {
            return (!string.IsNullOrEmpty(USUARIO) && !string.IsNullOrEmpty(PASSWORD));
        }
    }
}