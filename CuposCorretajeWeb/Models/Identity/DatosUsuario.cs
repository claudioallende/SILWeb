using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Claims;

namespace CuposCorretajeWeb.Models.Identity
{
    public class DatosUsuario
    {
        public string USUARIO { get; set; }
        public string PASSWORD { get; set; }
        public string NOMBRE { get; set; }
        public IList<Claim> Claims { get; set; }

        public bool IsAuthorized()
        {
            IList<string> Centros = ClaimsUtil.GetListClaims(this.Claims, "Centro");
            return (ClaimsUtil.GetClaim(this.Claims, "AccesoCuposCorretaje") == "True"
                && Centros != null && Centros.Count > 0
                && !string.IsNullOrEmpty(ClaimsUtil.GetClaim(this.Claims, "CentroPorDefecto"))
            );
        }
    }
}