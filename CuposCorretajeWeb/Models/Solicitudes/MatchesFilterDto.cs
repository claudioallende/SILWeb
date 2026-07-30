using System;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  /// <summary>
  /// Filtros para el endpoint bulk <c>POST /api/ShiftRequest/Matches</c>.
  /// Espejo literal de <c>SILData.Model.SolicitudTurno.MatchesFilterDto</c>.
  /// El único filtro obligatorio es <see cref="CodigoGrano"/>.
  /// </summary>
  public class MatchesFilterDto
  {
    public long? CuentaComprador { get; set; }
    /// <summary>Obligatorio.</summary>
    public int CodigoGrano { get; set; }
    /// <summary>Cuenta del puerto/destino (nuevo). El backend resuelve
    /// internamente las zonas a las que pertenece ese puerto y filtra
    /// solicitudes y cupos por esa pertenencia.</summary>
    public long? CuentaPuerto { get; set; }
    public long? ZonaGeograficaId { get; set; }
    public long? CuentaVendedor { get; set; }
    /// <summary>Código del centro que está distribuyendo el operador. El backend
    /// lo usa para filtrar <c>cuposcorre</c> por el centro seleccionado. La
    /// disponibilidad por vendedor la calcula la UI desde la tabla HTML de
    /// Distribución (#TablaDistribuciones).</summary>
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
