using Microsoft.Owin.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace CuposCorretajeWeb.Models.Claims
{
    public static class ClaimsUtil
    {
        public static string GetAccessToken()
        {
            var claimsIdentity = HttpContext.Current.User.Identity as ClaimsIdentity;
            string access_token = claimsIdentity.Claims.Where(x => x.Type == "access_token").Select(x => x.Value).FirstOrDefault();
            if (string.IsNullOrEmpty(access_token)) throw new Exception("Token vacío o nulo");
            return access_token;
            //return HttpContext.Current.Session["access_token"].ToString();
        }

        public static long GetCuenta()
        {
            var claimsIdentity = HttpContext.Current.User.Identity as ClaimsIdentity;
            string access_token = claimsIdentity.Claims.Where(x => x.Type == "Cuit").Select(x => x.Value).FirstOrDefault();
            if (string.IsNullOrEmpty(access_token)) throw new Exception("Token vacío o nulo");
            return 100395;
        }

        public static string GetClaimValue(this IPrincipal currentPrincipal, string key)
        {
            var identity = currentPrincipal.Identity as ClaimsIdentity;
            if (identity == null)
                return null;

            var claim = identity.Claims.FirstOrDefault(c => c.Type == key);
            return claim.Value;
        }

        public static string GetClaimValue(IEnumerable<Claim> claims, string key) {
            var claim = claims.FirstOrDefault(c => c.Type == key);
            return claim.Value;
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