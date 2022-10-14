using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace CuposCorretajeWeb.Models.Identity
{
    public class ClaimsUtil
    {
        public static string GetClaim(string key)
        {
            var claimsIdentity = HttpContext.Current.User.Identity as ClaimsIdentity;
            string claim = claimsIdentity.Claims.Where(x => x.Type == key).Select(x => x.Value).FirstOrDefault();
            if (string.IsNullOrEmpty(claim)) throw new Exception("Token vacío o nulo");
            return claim;
        }

        public static bool GetBooleanClaim(string key)
        {
            var claimsIdentity = HttpContext.Current.User.Identity as ClaimsIdentity;
            bool result = false;
            var claim = claimsIdentity.Claims.Where(x => x.Type == key).Select(x => x.Value).FirstOrDefault();
            if (claim != null)
                Boolean.TryParse(claim, out result);
            return result;
        }

        public static IList<string> GetListClaims(string key)
        {
            var claimsIdentity = HttpContext.Current.User.Identity as ClaimsIdentity;
            IList<string> claims = claimsIdentity.Claims.Where(x => x.Type == key).Select(x => x.Value).ToList();
            return claims;
        }

        public static string GetClaim(IEnumerable<Claim> Claims, string key)
        {
            string claim = Claims.Where(x => x.Type == key).Select(x => x.Value).FirstOrDefault();
            return claim;
        }

        public static IList<string> GetListClaims(IEnumerable<Claim> Claims, string key)
        {
            IList<string> claims = Claims.Where(x => x.Type == key).Select(x => x.Value).ToList();
            return claims;
        }

        public static string GetClaimValue(IPrincipal currentPrincipal, string key)
        {
            var identity = currentPrincipal.Identity as ClaimsIdentity;
            if (identity == null)
                return null;

            var claim = identity.Claims.FirstOrDefault(c => c.Type == key);
            return claim == null ? "" : claim.Value;
        }

        public static bool HasSomeClaim(IList<string> ClaimsName)
        {
            var claimsIdentity = HttpContext.Current.User.Identity as ClaimsIdentity;
            string claim = claimsIdentity.Claims.Where(x => ClaimsName.Contains(x.Type)).Select(x => x.Value).FirstOrDefault();
            if (string.IsNullOrEmpty(claim)) return false;
            return true;
        }

        public static bool HasClaim(string ClaimName)
        {
            var claimsIdentity = HttpContext.Current.User.Identity as ClaimsIdentity;
            string claim = claimsIdentity.Claims.Where(x => ClaimName == x.Type).Select(x => x.Value).FirstOrDefault();
            if (string.IsNullOrEmpty(claim)) return false;
            return true;
        }
    }

    public class ClaimsConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(System.Security.Claims.Claim));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            string type = (string)jo["m_type"];
            string value = (string)jo["m_value"];
            string valueType = (string)jo["m_valueType"];
            string issuer = (string)jo["m_issuer"];
            string originalIssuer = (string)jo["m_originalIssuer"];
            return new Claim(type, value, valueType, issuer, originalIssuer);
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }   
}