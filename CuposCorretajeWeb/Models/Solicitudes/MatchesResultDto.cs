using System;
using System.Collections.Generic;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  /// <summary>
  /// Respuesta del endpoint bulk <c>POST /api/ShiftRequest/Matches</c>.
  /// Espejo literal de <c>SILData.Model.SolicitudTurno.MatchesResultDto</c>.
  /// </summary>
  public class MatchesResultDto
  {
    public MatchesFiltrosAplicados FiltrosAplicados { get; set; } = new MatchesFiltrosAplicados();
    public MatchesResumen Resumen { get; set; } = new MatchesResumen();
    public List<MatchItemDto> Items { get; set; } = new List<MatchItemDto>();
  }

  public class MatchesFiltrosAplicados
  {
    public long? CuentaComprador { get; set; }
    public int CodigoGrano { get; set; }
    public long? ZonaGeograficaId { get; set; }
    public long? CuentaVendedor { get; set; }
    public DateTime FechaDesde { get; set; }
    public DateTime FechaHasta { get; set; }
    public bool IncluirIncompatibles { get; set; }
    public long? CuentaPuerto { get; set; }
    public List<long> ZonasResueltas { get; set; } = new List<long>();
  }

  public class MatchesResumen
  {
    public int TotalSolicitudesAnalizadas { get; set; }
    public int TotalCuposAnalizados { get; set; }
    public int MatchesDirectos { get; set; }
    public int MatchesParciales { get; set; }
    public int MatchesCondicionales { get; set; }
    public int Incompatibles { get; set; }
  }
}
