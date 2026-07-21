using System.Collections.Generic;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  /// <summary>
  /// Espejo de <c>SILData.Model.SolicitudTurno.ZonaGeograficaView</c>.
  /// Se usa en el endpoint <c>POST /api/GeographicalArea/ResolveByDestino</c>
  /// para devolver las zonas a las que pertenece un puerto.
  /// </summary>
  public class ZonaGeograficaView
  {
    public long ZonaGeoId { get; set; }
    public string Nombre { get; set; }
    public string Codigo { get; set; }
    public string Descripcion { get; set; }
    public string CentroId { get; set; }
    public List<DestinoView> Destinos { get; set; }
    public int? Disponible { get; set; }
    public bool ShowDetails { get; set; }

    public ZonaGeograficaView()
    {
      Destinos = new List<DestinoView>();
    }
  }

  public class DestinoView
  {
    public long Id { get; set; }
    public int Cuenta { get; set; }
    public string Cuit { get; set; }
    public string Nombre { get; set; }
    public string Domicilio { get; set; }
    public string TipodDeCuenta { get; set; }
    public string Localidad { get; set; }
    public string Provincia { get; set; }
    public int CPostal { get; set; }
  }

  /// <summary>
  /// Request del endpoint <c>POST /api/GeographicalArea/ResolveByDestino</c>.
  /// </summary>
  public class ResolveByDestinoRequest
  {
    public long CuentaPuerto { get; set; }
  }
}
