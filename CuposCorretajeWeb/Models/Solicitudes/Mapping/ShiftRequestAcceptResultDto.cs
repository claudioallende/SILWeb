using System.Collections.Generic;

namespace CuposCorretajeWeb.Models.Solicitudes.Mapping
{
  /// <summary>
  /// Mirror local de <c>SILData.Model.SolicitudTurno.ShiftRequestAcceptResult</c>
  /// para que el MVC pueda consumir el response del endpoint Accept sin
  /// necesitar una referencia de proyecto a SILData.
  /// Mantener sincronizado con la versión SILData.
  /// </summary>
  public class ShiftRequestAcceptResultDto
  {
    public int TotalOperaciones { get; set; }
    public int TotalAsignadas { get; set; }
    public int TotalConflictos { get; set; }

    /// <summary>Cantidad total de cupos pedidos originalmente (de la solicitud).</summary>
    public int CantidadSolicitadaTotal { get; set; }

    /// <summary>Cantidad efectivamente asignada en este Accept.</summary>
    public int CantidadAsignadaEnEsteAccept { get; set; }

    /// <summary>Cantidad pendiente restante (SolicitadaTotal - AsignadaEnEsteAccept).</summary>
    public int CantidadPendienteRestante { get; set; }

    public List<ShiftRequestAssignedItemDto> Asignados { get; set; } = new();
    public List<ShiftRequestAcceptFailureDto> Fallos { get; set; } = new();
  }

  public class ShiftRequestAssignedItemDto
  {
    public long SolicitudId { get; set; }

    /// <summary>Primer cupo aceptado en esta operación (compatibilidad histórica).</summary>
    public long CupoAsignadoId { get; set; }

    /// <summary>
    /// IDs de todos los cupos efectivamente aceptados en esta operación.
    /// Cada uno se persiste como una fila en <c>SOLTURNOS_DETALLE</c>
    /// y se cuenta en <c>CantidadAceptada</c> (o <c>CantidadFuturoAceptada</c>
    /// si la solicitud es <c>EsFuturo=true</c>).
    /// </summary>
    public List<long> CuposAsignados { get; set; } = new();

    public string TipoMatch { get; set; }
  }

  public class ShiftRequestAcceptFailureDto
  {
    public long SolicitudId { get; set; }
    public string Motivo { get; set; }
  }
}
