using System.Collections.Generic;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  /// <summary>
  /// Respuesta del endpoint <c>POST /api/ShiftRequest/Reject</c>.
  /// Mapea <c>SILData.Model.SolicitudTurno.ShiftRequestRejectResult</c>.
  /// </summary>
  public class ShiftRequestRejectResultViewModel
  {
    public int TotalProcesados { get; set; }
    public int TotalRechazados { get; set; }
    public int TotalFallidos { get; set; }
    public List<long> Rechazados { get; set; }
    public List<RechazadoDetalleItem> RechazadosDetalle { get; set; }
    public List<FalloItem> Fallos { get; set; }

    public ShiftRequestRejectResultViewModel()
    {
      Rechazados = new List<long>();
      RechazadosDetalle = new List<RechazadoDetalleItem>();
      Fallos = new List<FalloItem>();
    }

    public bool TieneExitos => Rechazados != null && Rechazados.Count > 0;
    public bool TodosFallaron => TotalFallidos > 0 && TotalRechazados == 0;
  }

  public class RechazadoDetalleItem
  {
    public long SolicitudId { get; set; }
    /// <summary>"Directo" | "Parcial" | "Condicional" | null — clasificación al momento del rechazo.</summary>
    public string TipoMatchAlRechazar { get; set; }
    public string RazonIncompatibilidad { get; set; }
  }
}
