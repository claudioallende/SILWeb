using System;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  /// <summary>
  /// Request del endpoint <c>POST /CuposMatching/BuscarCuposConMatch</c>.
  /// El operador de distribución pasa el destino del cupo (no la zona) y
  /// el backend se ocupa de resolver a zonas. Reuso de <see cref="FilterCuposDisponible"/>
  /// como filtro base + <c>CupoId</c> para resolver la fecha exacta del cupo
  /// y comparar día a día contra la FechaSolicitado de cada solicitud.
  /// </summary>
  public class BuscarMatchRequestDto
  {
    public long CuentaVendedor { get; set; }
    public long CuentaComprador { get; set; }
    public int CodigoGrano { get; set; }
    public DateTime Fecha { get; set; }
    public long CuentaPuerto { get; set; }
    /// <summary>Id del cupo (CUPOSCORRE) que se está distribuyendo. Se usa para
    /// obtener su fecha exacta y comparar contra FechaSolicitado de las solicitudes.</summary>
    public long CupoId { get; set; }
    /// <summary>Código del centro seleccionado por el operador (de
    /// <c>Model.CentroSeleccionado</c>). El backend lo usa para filtrar
    /// <c>cuposcorre</c> por el centro que se está distribuyendo. La
    /// disponibilidad por vendedor la calcula la UI desde la tabla HTML
    /// de Distribución (#TablaDistribuciones).</summary>
    public string Codcentro { get; set; }
    /// <summary>Código del centro de distribución del cupo, recibido por URL
    /// como <c>centrodistribucion</c>. Junto con <see cref="Codcentro"/>
    /// forma la clave natural del cupo en <c>cuposcorre</c>
    /// (<c>Centro</c> + <c>CentroDist</c>) y se envía al backend V2 como
    /// filtro <c>AND</c>.</summary>
    public string Codcentrodist { get; set; }
  }
}
