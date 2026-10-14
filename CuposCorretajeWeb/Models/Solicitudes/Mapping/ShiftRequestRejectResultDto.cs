using System.Collections.Generic;

namespace CuposCorretajeWeb.Models.Solicitudes.Mapping
{
  /// <summary>
  /// Mirror local de <c>SILData.Model.SolicitudTurno.ShiftRequestRejectResult</c>
  /// para que el MVC pueda consumir el response del endpoint Reject sin
  /// necesitar una referencia de proyecto a SILData.
  /// Mantener sincronizado con la versión SILData.
  /// </summary>
  public class ShiftRequestRejectResultDto
  {
    public int TotalProcesados { get; set; }
    public int TotalRechazados { get; set; }
    public int TotalFallidos { get; set; }
    public List<long> Rechazados { get; set; } = new List<long>();
  }
}
