using System;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  /// <summary>
  /// Subset de <c>SILData.Model.SolicitudTurno.SolicitudTurno</c> que el
  /// endpoint <c>POST /api/ShiftRequest/Accept</c> necesita dentro del
  /// array <c>ShiftRequest</c>.
  ///
  /// El backend hace matching con estos campos y luego ejecuta la asignación
  /// con la cantidad de cupos declarados en <see cref="Cantidad"/> (cuántos
  /// cupos consume esta solicitud del array <c>CuposToBeDistributed</c>).
  ///
  /// Para esta primera integración, sólo usamos Cantidad=1 (un cupo por
  /// solicitud); la multiselección con Cantidad>1 queda como evolución futura.
  /// </summary>
  public class SolicitudInputDto
  {
    /// <summary>0 si es nueva sin persistir; id real si ya existe.</summary>
    public long Id { get; set; }
    public long? CuentaComprador { get; set; }
    public long CuentaVendedor { get; set; }
    public long? CuentaDestino { get; set; }
    public int? TipoDestino { get; set; }
    public int CodigoEstado { get; set; }
    public int CodigoGrano { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaSolicitado { get; set; }
    public int Cantidad { get; set; } = 1;
    public int CantidadFuturo { get; set; }
    public bool EsFuturo { get; set; }
    public string CodigoCentro { get; set; }
    public string Observacion { get; set; }
    public long? CupoId { get; set; }
  }
}
