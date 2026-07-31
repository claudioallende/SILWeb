using System;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  public class MatchesFilterDto
  {
    public long? CuentaComprador { get; set; }
    public int CodigoGrano { get; set; }
    public long? CuentaPuerto { get; set; }
    public long? ZonaGeograficaId { get; set; }
    public long? CuentaVendedor { get; set; }
    public string Codcentro { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public bool IncluirIncompatibles { get; set; }
    public MatchesAgrupacion AgruparPor { get; set; } = MatchesAgrupacion.Solicitud;
  }

  public enum MatchesAgrupacion
  {
    Solicitud = 0,
    Cupo = 1,
    Ninguno = 2
  }
}
