using System;
using System.Collections.Generic;
using System.Configuration;

namespace CuposCorretajeWeb.Models.Auth
{
    // Arma el access_token que antes emitia IdentityServer3 (acabase.com.ar). Usa las mismas
    // claves de AppSettings (JWT_ISSUER/JWT_AUDIENCE/JWT_SIGNING_KEY/JWT_EXPIRES_MINUTES) que
    // SILResourceServer usa del lado API para validar el token (deben coincidir en ambos Web.config).
    public static class LocalTokenIssuer
    {
        public static string IssueAccessToken(UsuarioLocal usuario)
        {
            string issuer = ConfigurationManager.AppSettings["JWT_ISSUER"];
            string audience = ConfigurationManager.AppSettings["JWT_AUDIENCE"];
            string signingKey = ConfigurationManager.AppSettings["JWT_SIGNING_KEY"];
            int expiresMinutes = int.Parse(ConfigurationManager.AppSettings["JWT_EXPIRES_MINUTES"] ?? "1190");

            // NOTA: usuariosSIL.txt (fuente de los permisos) no trae Cuit/Cuenta, asi que esos
            // claims -que algunos endpoints de SILResourceServer leen via ClaimsUtil.GetCuit()/
            // GetCuenta()- no se emiten hoy. Si algun flujo los necesita, agregar aca cuando se
            // consiga ese dato.
            var claims = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("Usuario", usuario.Usuario),
                new KeyValuePair<string, object>("Centro", usuario.Centros ?? new List<string>()),
                new KeyValuePair<string, object>("CentroPorDefecto", usuario.CentroPorDefecto ?? string.Empty),
                new KeyValuePair<string, object>("AccesoAuditoriaResourceServer", usuario.AccesoAuditoria ? "True" : "False"),
                new KeyValuePair<string, object>("scope", "CuposCorrRSRCServ")
            };

            return SimpleJwtBuilder.Issue(issuer, audience, signingKey, TimeSpan.FromMinutes(expiresMinutes), claims);
        }
    }
}
