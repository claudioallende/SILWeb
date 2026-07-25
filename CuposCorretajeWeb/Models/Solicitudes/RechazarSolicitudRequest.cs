namespace CuposCorretajeWeb.Models.Solicitudes
{
  /// <summary>
  /// Payload que Pantalla 2 envía a <c>Solicitudes/RechazarSolicitud</c> cuando
  /// el operador confirma el rechazo completo desde el Swal.
  ///
  /// Se necesita un DTO wrapper (no un primitivo <c>long</c>) porque el
  /// <c>JsonValueProviderFactory</c> de MVC 5 no bindea primitivos desde el
  /// body: sólo lee objetos JSON. Si el body es el número crudo
  /// (<c>1234</c>) el provider intenta deserializarlo como
  /// <c>Dictionary&lt;string,object&gt;</c> y revienta, por lo que la acción
  /// nunca llega a ejecutar (no se ve ninguna respuesta — la página queda
  /// colgada con el loader). El atributo <c>[System.Web.Http.FromBody]</c>
  /// del Web API no aplica en MVC.
  /// </summary>
  public class RechazarSolicitudRequest
  {
    /// <summary>Id de la solicitud a rechazar.</summary>
    public long IdSolicitud { get; set; }
  }
}
