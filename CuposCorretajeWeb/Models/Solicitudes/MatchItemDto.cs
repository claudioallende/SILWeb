using System;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  /// <summary>
  /// Item individual de la respuesta del endpoint bulk.
  /// Cada item es un par (solicitud, cupo) con su clasificación y desglose.
  /// Espejo literal de <c>SILData.Model.SolicitudTurno.MatchItemDto</c>.
  /// </summary>
  public class MatchItemDto
  {
    public long SolicitudId { get; set; }
    public long CupoId { get; set; }
    /// <summary>"Directo" | "Parcial" | "Condicional" | null (si Incompatible).</summary>
    public string MatchType { get; set; }
    /// <summary>Razón textual para Parcial/Condicional/Incompatible.</summary>
    public string Razon { get; set; }

    public bool VendedorCoincide { get; set; }
    public bool CompradorCoincide { get; set; }
    public bool DestinoCoincide { get; set; }

    public MatchSolicitudCompletaDto Solicitud { get; set; } = new MatchSolicitudCompletaDto();
    public MatchCupoResumenDto Cupo { get; set; } = new MatchCupoResumenDto();
  }

  public class MatchSolicitudCompletaDto
  {
    public long Id { get; set; }
    public long CuentaVendedor { get; set; }
    public long? CuentaComprador { get; set; }
    public int CodigoGrano { get; set; }
    public long? CuentaDestino { get; set; }
    public string TipoDestino { get; set; }
    /// <summary>Cantidad total pedida por la solicitud (SOLTURNOS.CANTIDAD).</summary>
    public int Cantidad { get; set; }
    /// <summary>
    /// Cantidad disponible de la solicitud para asignar en este Accept
    /// (Cantidad - CantidadAceptada - CantidadRechazada).
    /// La UI la muestra como "disponibles" y valida contra este límite.
    /// </summary>
    public int CantidadDisponible { get; set; }
    /// <summary>Cantidad rechazada acumulada (SOLTURNOS.CANTIDAD_RECHAZADA).</summary>
    public int CantidadRechazada { get; set; }
    public DateTime FechaSolicitado { get; set; }
    public string Observacion { get; set; }
    public CuposAsociadosDesgloseDto CuposAsociados { get; set; } = new CuposAsociadosDesgloseDto();
  }

  public class MatchCupoResumenDto
  {
    public long Id { get; set; }
    public string CodGrano { get; set; }
    public string CodVendSIL { get; set; }
    public string CodCompSIL { get; set; }
    public string CodDestino { get; set; }
    public DateTime? Fecha { get; set; }
    /// <summary>
    /// Cupos totales a distribuir del cupo (de <c>VISTA_CUPOSDISTRIBUIDOV4</c>).
    /// Espejo literal de <c>SILData.Model.SolicitudTurno.MatchCupoResumen.Cupostotalesadist</c>.
    /// </summary>
    public int Cupostotalesadist { get; set; }
    public string NombreVendedor { get; set; }
    public string NombreComprador { get; set; }
    public string NombreDestino { get; set; }
    public string NombreGrano { get; set; }
  }

  public class CuposAsociadosDesgloseDto
  {
    public int Total { get; set; }
    public int Otorgados { get; set; }
    public int Pendientes { get; set; }
    public int Rechazados { get; set; }
  }
}
