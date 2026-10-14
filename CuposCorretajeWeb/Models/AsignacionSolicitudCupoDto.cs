namespace CuposCorretajeWeb.Models
{
  /// <summary>
  /// Asociación solicitud-cupo enviada a SILApi en modo SolicitudMatch.
  /// Espejo de SILApi/SILResourceServer/Models/DTO/AsignacionSolicitudCupo.cs.
  /// </summary>
  public class AsignacionSolicitudCupoDto
  {
    /// <summary>Id de la solicitud a la que se le asigna el cupo.</summary>
    public long SolicitudId { get; set; }

    /// <summary>
    /// SolicitudMatch: ID exacto del CUPOSCORRE que debe distribuirse y que se
    /// persiste como CUPO_ID en SOLTURNOS_DETALLE. Obligatorio.
    /// </summary>
    public long? CupoSeleccionadoId { get; set; }

    /// <summary>
    /// DistribucionManual: referencia del matching. En SolicitudMatch queda null.
    /// </summary>
    public long? CupoReferenciaId { get; set; }

    /// <summary>
    /// Cantidad a asignar. En SolicitudMatch se fija siempre en 1
    /// (cada entrada representa un cupo y no se permite subdivisión intra-cupo).
    /// </summary>
    public int Cantidad { get; set; }

    /// <summary>
    /// Tipo de match devuelto por el motor: "Directo" | "Parcial" | "Condicional".
    /// Sólo diagnóstico; el backend lo ignora al persistir.
    /// </summary>
    public string MatchType { get; set; }

    /// <summary>Clave de fila/día de Distribucion.cshtml.</summary>
    public long Compcta { get; set; }
    public long Vendcta { get; set; }
    public int Codproducto { get; set; }
    public long Ctadestino { get; set; }
    public string Cosecha { get; set; }
    public string Centro { get; set; }
    public int Fechaent { get; set; }
    /// <summary>Día 0..20 dentro de la fila. -1 si no se asocia a un día específico.</summary>
    public int Dia { get; set; }
  }
}
