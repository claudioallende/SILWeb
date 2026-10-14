using System.Collections.Generic;

namespace CuposCorretajeWeb.Models
{
  /// <summary>
  /// Relación solicitud-cupo efectivamente persistida por SILApi en SOLTURNOS_DETALLE.
  /// Espejo de SILApi/SILResourceServer/Models/DTO/AsignacionSolicitudCupo.cs.
  /// </summary>
  public class AsignacionRealizadaDto
  {
    public long SolicitudId { get; set; }
    public long CupoId { get; set; }
  }

  /// <summary>
  /// Respuesta estructurada de POST /api/Cupos/ActualizarDistribucion.
  /// Espejo de SILApi/SILResourceServer/Models/DTO/AsignacionSolicitudCupo.cs.
  /// <see cref="Codigo"/> conserva la semántica legacy (1 = OK con cambios,
  /// 100/200 = errores, 300 = sin cambios) para compatibilidad con
  /// Distribucion.cshtml.
  /// </summary>
  public class ActualizarDistribucionResult
  {
    public int Codigo { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; }

    public int Solicitados { get; set; }
    public int Asignados { get; set; }
    public int Pendientes { get; set; }

    public IList<AsignacionRealizadaDto> Relaciones { get; set; }

    public ActualizarDistribucionResult()
    {
      Relaciones = new List<AsignacionRealizadaDto>();
    }
  }
}
