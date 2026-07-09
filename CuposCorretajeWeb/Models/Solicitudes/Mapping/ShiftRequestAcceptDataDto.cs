using System.Collections.Generic;

namespace CuposCorretajeWeb.Models.Solicitudes.Mapping
{
  /// <summary>
  /// Mirror local de <c>SILData.Model.SolicitudTurno.ShiftRequestAcceptData</c>.
  /// El frontend no referencia SILData como assembly: cuando el MVC necesita
  /// serializar un payload para <c>POST /api/ShiftRequest/Accept</c>, lo arma
  /// con este DTO local (Newtonsoft.Json, case-insensitive).
  /// </summary>
  /// <remarks>
  /// Mantener el set de campos sincronizado con la versión SILData (el backend
  /// sólo deserializa los que necesita vía System.Text.Json con camelCase).
  /// </remarks>
  public class ShiftRequestAcceptDataDto
  {
    public List<SolicitudTurnoDto> ShiftRequest { get; set; }
    public List<CupoDto> CuposToBeDistributed { get; set; }
    /// <summary>
    /// Mapa cupoId → cantidad. Cada cupo hoy es entero (1), pero se mantiene
    /// como hook para subdivisión intra-cupo.
    /// </summary>
    public Dictionary<long, int> CantidadPorCupo { get; set; }
  }
}
