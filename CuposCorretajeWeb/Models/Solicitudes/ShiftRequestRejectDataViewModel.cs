using System.Collections.Generic;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  /// <summary>
  /// Cuerpo del request <c>POST /api/ShiftRequest/Reject</c>.
  /// Mapea <c>SILData.Model.SolicitudTurno.ShiftRequestRejectData</c>.
  ///
  /// El campo <see cref="Automatico"/> distingue entre:
  /// <list type="bullet">
  ///   <item><c>true</c> cuando el rechazo lo dispara el job programado de las 20:00 hs.</item>
  ///   <item><c>false</c> cuando es el operador quien rechaza manualmente.</item>
  /// </list>
  /// </summary>
  public class ShiftRequestRejectDataViewModel
  {
    public List<long> SolicitudIds { get; set; }
    public string Motivo { get; set; }
    public bool Automatico { get; set; }

    public ShiftRequestRejectDataViewModel()
    {
      SolicitudIds = new List<long>();
    }
  }
}
