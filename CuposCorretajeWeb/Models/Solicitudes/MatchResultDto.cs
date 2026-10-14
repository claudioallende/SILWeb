namespace CuposCorretajeWeb.Models.Solicitudes
{
  /// <summary>
  /// DTO espejo de <c>SILData.Model.SolicitudTurno.MatchResultDto</c>.
  /// Representa el resultado de evaluar la compatibilidad entre un cupo y una solicitud.
  ///
  /// Valores posibles de <see cref="Tipo"/>:
  /// <list type="bullet">
  ///   <item><c>"Directo"</c> — todos los criterios coinciden (grano, vendedor, comprador, zona).</item>
  ///   <item><c>"Parcial"</c> — obligatorios OK pero algún opcional falta o no coincide.</item>
  ///   <item><c>"Condicional"</c> — la solicitud tiene observaciones y exige confirmación explícita.</item>
  ///   <item><c>null</c> cuando <see cref="Compatible"/> es <c>false</c>; en ese caso
  ///       <see cref="Razon"/> contiene el motivo de la incompatibilidad.</item>
  /// </list>
  /// </summary>
  public class MatchResultDto
  {
    public long CupoId { get; set; }
    public long SolicitudId { get; set; }
    public bool Compatible { get; set; }
    /// <summary>"Directo" | "Parcial" | "Condicional" | null</summary>
    public string Tipo { get; set; }
    /// <summary>null si Compatible. Si !Compatible, razón de la incompatibilidad.</summary>
    public string Razon { get; set; }
  }
}
