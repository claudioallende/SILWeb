using CuposCorretajeWeb.Models.Error;
using CuposCorretajeWeb.Models.Identity;
using IdentityModel.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CuposCorretajeWeb.Models.Data
{
  public abstract class Util : IDisposable
  {
    // .NET Framework 4.6.1 no habilita TLS 1.2 por defecto en
    // ServicePointManager.SecurityProtocol. Sin esto, los endpoints HTTPS
    // modernos (Azure App Service, por ejemplo) rechazan la conexión con
    // "Could not create SSL/TLS secure channel". Localhost no lo nota porque
    // su cert negocia con versiones viejas, pero el sitio de test no.
    static Util()
    {
      ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
    }

    public abstract string GetWebSerive { get; internal set; }
    public abstract string GetWebServiceSILData { get; internal set; }

    public string GetPath(string Controller)
    {
      return GetWebSerive + Controller + "/";
    }

    //Obtengo la url para para conexion api SilData
    public string GetPathApiSilData(string Controller)
    {
      return GetWebServiceSILData + Controller + "/";
    }

    public async Task<string> RequestAsync(string Controller, string Action)
    {
      var token = GetTokenAsync();
      var client = new HttpClient();
      client.SetBearerToken(token);
      client.DefaultRequestHeaders.Add("Content-Type", "application/json");
      var json = await client.GetStringAsync(GetPath(Controller) + Action);
      return JArray.Parse(json).ToString();
    }

    /// <summary>
    /// Retorna un objeto de la clase T. Utiliza el WebService definido en web.config RSRC_SERVER.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="Action">Action al que se consulta</param>
    /// <returns></returns>
    public async Task<T> RequestGetAndDeserializeAsync<T>(string Controller, string Action)
    {
      var token = GetTokenAsync();
      WebClient client = new WebClient();

      // Add a user agent header in case the
      // requested URI contains a query.

      client.Headers.Add("Authorization", $"Bearer {token}");
      client.Headers.Add("Content-Type", "application/json");

      Stream data = client.OpenRead(GetPath(Controller) + Action);
      StreamReader reader = new StreamReader(data);
      string s = reader.ReadToEnd();
      return await DeserializeAsync<T>(s);
    }

    public async Task<T> RequestPostAndDeserializeAsync<T>(string Controller, string Action, object Data)
    {
      var token = GetTokenAsync();
      var client = new HttpClient();
      client.SetBearerToken(token);
      var jsonString = JsonConvert.SerializeObject(Data);
      HttpContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
      var json = await client.PostAsync(GetPath(Controller) + Action, content);
      if (json.StatusCode == System.Net.HttpStatusCode.NotFound) throw new Exception("No se encontro el action en el resource server.");
      if (json.StatusCode == System.Net.HttpStatusCode.InternalServerError || json.StatusCode == System.Net.HttpStatusCode.Conflict)
        throw new ApiException(await json.Content.ReadAsStringAsync());
      return await DeserializeAsync<T>(await json.Content.ReadAsStringAsync());
    }

    public async Task<T> RequestSILDataPostAndDeserializeAsync<T>(string Controller, string Action, object Data)
    {
      //var token = GetTokenAsync();
      var client = new HttpClient();
      //client.SetBearerToken(token);
      var jsonString = JsonConvert.SerializeObject(Data);
      HttpContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
      var response = await client.PostAsync(GetPathApiSilData(Controller) + Action, content);

      // 204 NoContent: el endpoint ejecutó OK pero no tiene cuerpo. Algunos
      // endpoints (ej. Matches cuando no hay resultados) lo declaran
      // explícitamente. Devolvemos default(T) para evitar JsonReaderException
      // al deserializar un body vacío.
      if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        return default(T);

      if (response.StatusCode == System.Net.HttpStatusCode.NotFound) throw new Exception("No se encontro el action en el resource server.");
      if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError || response.StatusCode == System.Net.HttpStatusCode.Conflict)
        throw new ApiException(await response.Content.ReadAsStringAsync());
      return await DeserializeAsync<T>(await response.Content.ReadAsStringAsync());
    }
    public async Task<T> DeserializeAsync<T>(string JsonResponse)
    {
      return await Task.FromResult(JsonConvert.DeserializeObject<T>(JsonResponse));
    }

    public T Deserialize<T>(string JsonResponse)
    {
      return JsonConvert.DeserializeObject<T>(JsonResponse);
    }

    private string GetTokenAsync()
    {
      return ClaimsUtil.GetClaim("access_token");
    }

    public void Dispose()
    {
      //Dispose(true);
      // This object will be cleaned up by the Dispose method.
      // Therefore, you should call GC.SupressFinalize to
      // take this object off the finalization queue
      // and prevent finalization code for this object
      // from executing a second time.
      GC.SuppressFinalize(this);
    }
  }
}