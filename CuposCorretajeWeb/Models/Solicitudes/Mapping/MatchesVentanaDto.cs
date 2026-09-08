using System;
using System.Collections.Generic;

namespace CuposCorretajeWeb.Models.Solicitudes.Mapping
{
  /// <summary>
  /// Request de <c>POST /api/ShiftRequest/MatchesVentana</c> (SILData).
  /// Describe únicamente la ventana temporal: el backend resuelve en una sola
  /// llamada los matches de TODAS las solicitudes pendientes del rango.
  ///
  /// Nombres en PascalCase a propósito: el binding de SILData es
  /// case-sensitive (ver <c>Util.RequestSILDataPostAndDeserializeAsync</c>).
  /// </summary>
  public class MatchesVentanaFilterDto
  {
    public DateTime FechaDesde { get; set; }
    public int Dias { get; set; }
  }

  /// <summary>
  /// Mirror de <c>SILData.Model.SolicitudTurno.MatchesVentanaResultDto</c>.
  /// Reemplaza al fan-out de un <see cref="GrillaMatchResumenDto"/> por fila
  /// de grilla: una sola respuesta trae los pares de toda la ventana.
  /// </summary>
  public class MatchesVentanaResponseDto
  {
    public List<MatchVentanaItemDto> Items { get; set; } = new List<MatchVentanaItemDto>();

    public MatchesVentanaResumenDto Resumen { get; set; } = new MatchesVentanaResumenDto();
  }

  /// <summary>
  /// Par (solicitud, cupo) compatible. Trae sólo lo que la grilla consume,
  /// más las cuatro claves de la solicitud, que son las que permiten
  /// atribuir el item a la fila que le corresponde.
  /// </summary>
  public class MatchVentanaItemDto
  {
    public long SolicitudId { get; set; }
    public long CupoId { get; set; }

    /// <summary>"Directo" | "Parcial" | "Condicional".</summary>
    public string MatchType { get; set; }

    /// <summary>Fecha del cupo (coincide con la fecha de la solicitud).</summary>
    public DateTime? CupoFecha { get; set; }

    public int CodigoGrano { get; set; }
    public long CuentaVendedor { get; set; }
    public long? CuentaComprador { get; set; }
    public long? CuentaDestino { get; set; }
  }

  public class MatchesVentanaResumenDto
  {
    public int TotalSolicitudesAnalizadas { get; set; }
    public int TotalCuposAnalizados { get; set; }
    public int TotalGranos { get; set; }
    public int MatchesDirectos { get; set; }
    public int MatchesParciales { get; set; }
    public int MatchesCondicionales { get; set; }
    public int Incompatibles { get; set; }
  }
}
