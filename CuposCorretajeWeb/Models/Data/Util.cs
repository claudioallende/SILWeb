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

    public async Task<T> RequestApiPostAndDeserializeAsync<T>(string Controller, string Action, object Data)
    {
      var token = GetTokenAsync();
      var client = new HttpClient();
      client.SetBearerToken(token);
      var jsonString = JsonConvert.SerializeObject(Data);
      HttpContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
      var response = await client.PostAsync(GetPath(Controller) + Action, content);

      if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        return default(T);

      if (response.StatusCode == System.Net.HttpStatusCode.NotFound) throw new Exception("No se encontro el action en el resource server.");
      if (response.StatusCode == System.Net.HttpStatusCode.BadRequest ||
          response.StatusCode == System.Net.HttpStatusCode.InternalServerError ||
          response.StatusCode == System.Net.HttpStatusCode.Conflict)
        throw new ApiException(await response.Content.ReadAsStringAsync());
      return await DeserializeAsync<T>(await response.Content.ReadAsStringAsync());
    }

    /// <summary>
    /// Wrapper POST contra SILData (sin bearer). El contrato con el backend es
    /// PascalCase en el body; el binding default de ASP.NET Core es case-sensitive
    /// en deserialización, por lo que los payloads deben construirse con nombres
    /// de propiedad PascalCase (ej. SolicitudIds, no solicitudIds).
    /// </summary>
    public async Task<T> RequestSILDataPostAndDeserializeAsync<T>(string Controller, string Action, object Data)
    {
      //var token = GetTokenAsync();
      var client = new HttpClient();
      //client.SetBearerToken(token);
      var jsonString = JsonConvert.SerializeObject(Data);
      HttpContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");
      var response = await client.PostAsync(GetPathApiSilData(Controller) + Action, content);

      if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        return default(T);

      if (response.StatusCode == System.Net.HttpStatusCode.NotFound) throw new Exception("No se encontro el action en el resource server.");
      if (response.StatusCode == System.Net.HttpStatusCode.BadRequest ||
          response.StatusCode == System.Net.HttpStatusCode.InternalServerError ||
          response.StatusCode == System.Net.HttpStatusCode.Conflict)
        throw new ApiException(await response.Content.ReadAsStringAsync());
      return await DeserializeAsync<T>(await response.Content.ReadAsStringAsync());
    }

    /// <summary>
    /// Wrapper GET contra SILData (sin body). Los endpoints públicos de SILData
    /// están sin [Authorize], así que no se manda bearer.
    ///
    /// Hoy no existe un wrapper GET→SILData: <see cref="RequestGetAndDeserializeAsync{T}"/>
    /// apunta al otro base URL (WebServiceCuposCorretaje legacy, con bearer).
    /// Este wrapper sirve para los endpoints GET de SILData que ya tenemos
    /// publicados (ej. <c>GET /api/ShiftRequest/GetByVendedorAsync/{cuenta}</c>).
    /// </summary>
    public async Task<T> RequestSILDataGetAndDeserializeAsync<T>(string Controller, string Action)
    {
      var client = new HttpClient();
      var response = await client.GetAsync(GetPathApiSilData(Controller) + Action);

      // 204 NoContent: mismo tratamiento que el wrapper POST.
      if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        return default(T);

      if (response.StatusCode == System.Net.HttpStatusCode.NotFound) throw new Exception("No se encontro el action en el resource server.");
      // Mismo tratamiento que el wrapper POST: capturar 400/422/409/500 para
      // que el detalle del ProblemDetails llegue al operador en vez de tirar 500.
      if (response.StatusCode == System.Net.HttpStatusCode.BadRequest ||
          response.StatusCode == System.Net.HttpStatusCode.InternalServerError ||
          response.StatusCode == System.Net.HttpStatusCode.Conflict)
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