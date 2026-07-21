using System.Collections.Generic;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  /// <summary>
  /// Respuesta del endpoint <c>POST /api/ShiftRequest/Accept</c>.
  /// Mapea <c>SILData.Model.SolicitudTurno.ShiftRequestAcceptResult</c>.
  ///
  /// Si <c>TodosFallaron == true</c>, SILData responde con 409 + mensaje
  /// "Ninguna solicitud pudo asignarse..."; el proxy del MVC debe traducirlo
  /// a un <c>JsonResult</c> con <c>success=false</c> para que el modal pueda
  /// mostrar el DLG de conflicto.
  /// </summary>
  public class ShiftRequestAcceptResultViewModel
  {
    public int TotalOperaciones { get; set; }
    public int TotalAsignadas { get; set; }
    public int TotalConflictos { get; set; }
    public List<AsignadoItem> Asignados { get; set; }
    public List<FalloItem> Fallos { get; set; }

    public ShiftRequestAcceptResultViewModel()
    {
      Asignados = new List<AsignadoItem>();
      Fallos = new List<FalloItem>();
    }

    public bool TieneExitos => Asignados != null && Asignados.Count > 0;
    public bool TodosFallaron => TotalConflictos > 0 && TotalAsignadas == 0;
  }

  public class AsignadoItem
  {
    public long SolicitudId { get; set; }
    public long CupoAsignadoId { get; set; }
    public List<long> CuposSplits { get; set; }

    /// <summary>"Directo" | "Parcial" | "Condicional" | null — clasificación del motor.</summary>
    public string TipoMatch { get; set; }

    public AsignadoItem()
    {
      CuposSplits = new List<long>();
    }
  }

  public class FalloItem
  {
    public long SolicitudId { get; set; }
    public string Motivo { get; set; }
  }
}
