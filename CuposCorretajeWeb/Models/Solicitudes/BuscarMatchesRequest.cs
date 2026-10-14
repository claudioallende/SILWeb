using System.Collections.Generic;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  /// <summary>
  /// Payload que Pantalla 2 envía a <c>Solicitudes/BuscarMatches</c> cuando el
  /// operador tilda/des-tilda días de la grilla. El controller lo traduce al
  /// <c>MatchesFilterDto</c> de SILData (más los datos que ya conoce de la
  /// solicitud persistidos en TempData).
  /// </summary>
  public class BuscarMatchesRequest
  {
    /// <summary>Id de la solicitud activa en Pantalla 2.</summary>
    public long IdSolicitud { get; set; }

    /// <summary>
    /// Fechas seleccionadas por el operador, en formato <c>yyyy-MM-dd</c>. El
    /// controller toma el min y max para construir FechaDesde/FechaHasta del
    /// filtro.
    /// </summary>
    public List<string> Fechas { get; set; }
  }
}