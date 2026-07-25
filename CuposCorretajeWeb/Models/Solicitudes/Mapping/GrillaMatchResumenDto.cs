using System;
using System.Collections.Generic;

namespace CuposCorretajeWeb.Models.Solicitudes.Mapping
{
  /// <summary>
  /// Mirror recortado de <c>SILData.Model.SolicitudTurno.MatchesResultDto</c>
  /// que el MVC usa para calcular el resumen de matching por fila de la grilla
  /// (columna "Cupos compatibles"). Sólo trae el agregado que necesitamos:
  /// los items clasificados por <c>MatchType</c> y el resumen total del motor.
  /// </summary>
  public class GrillaMatchResumenDto
  {
    /// <summary>Items (pares solicitud-cupo) clasificados por el motor.</summary>
    public List<GrillaMatchItemDto> Items { get; set; } = new List<GrillaMatchItemDto>();

    /// <summary>Resumen total devuelto por el motor (no se usa para la grilla).</summary>
    public GrillaMatchResumenResumenDto Resumen { get; set; } = new GrillaMatchResumenResumenDto();
  }

  public class GrillaMatchItemDto
  {
    public long SolicitudId { get; set; }
    public long CupoId { get; set; }
    /// <summary>"Directo" | "Parcial" | "Condicional" | null (Incompatible).</summary>
    public string MatchType { get; set; }
    public GrillaMatchCupoFechaDto Cupo { get; set; } = new GrillaMatchCupoFechaDto();
  }

  public class GrillaMatchCupoFechaDto
  {
    public DateTime? Fecha { get; set; }
  }

  public class GrillaMatchResumenResumenDto
  {
    public int MatchesDirectos { get; set; }
    public int MatchesParciales { get; set; }
    public int MatchesCondicionales { get; set; }
    public int Incompatibles { get; set; }
  }
}
