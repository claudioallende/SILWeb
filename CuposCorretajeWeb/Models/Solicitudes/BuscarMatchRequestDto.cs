using System;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  /// <summary>
  /// Request del endpoint <c>POST /CuposMatching/BuscarCuposConMatch</c>.
  /// El operador de distribución pasa el destino del cupo (no la zona) y
  /// el backend se ocupa de resolver a zonas. Reuso de <see cref="FilterCuposDisponible"/>
  /// como filtro base + <c>CuentaPuerto</c> como destino.
  /// </summary>
  public class BuscarMatchRequestDto
  {
    public long CuentaVendedor { get; set; }
    public long CuentaComprador { get; set; }
    public int CodigoGrano { get; set; }
    public DateTime Fecha { get; set; }
    public long CuentaPuerto { get; set; }
  }
}
