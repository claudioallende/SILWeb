using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace CuposCorretajeWeb.Models.Auth
{
    // Emite un JWT HS256 "a mano" (sin agregar paquetes nuevos al proyecto Web) que
    // SILResourceServer valida del lado API con Microsoft.Owin.Security.Jwt (paquete ya
    // referenciado ahi) usando la misma clave simetrica (JWT_SIGNING_KEY). Reemplaza al
    // access_token que antes emitia IdentityServer3 via OAuth2 ROPC contra acabase.com.ar.
    public static class SimpleJwtBuilder
    {
        public static string Issue(string issuer, string audience, string signingKey, TimeSpan lifetime, IEnumerable<KeyValuePair<string, object>> claims)
        {
            var now = DateTimeOffset.UtcNow;

            var header = new Dictionary<string, object>
            {
                ["alg"] = "HS256",
                ["typ"] = "JWT"
            };

            var payload = new Dictionary<string, object>
            {
                ["iss"] = issuer,
                ["aud"] = audience,
                ["iat"] = now.ToUnixTimeSeconds(),
                ["nbf"] = now.ToUnixTimeSeconds(),
                ["exp"] = now.Add(lifetime).ToUnixTimeSeconds()
            };

            foreach (var claim in claims)
            {
                payload[claim.Key] = claim.Value;
            }

            string headerB64 = Base64UrlEncode(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(header)));
            string payloadB64 = Base64UrlEncode(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload)));
            string unsigned = headerB64 + "." + payloadB64;

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(signingKey)))
            {
                byte[] signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(unsigned));
                return unsigned + "." + Base64UrlEncode(signature);
            }
        }

        private static string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}
