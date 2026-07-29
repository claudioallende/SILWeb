using System;

namespace CuposCorretajeWeb.Models.Solicitudes.Mapping
{
  /// <summary>
  /// Mirror local de <c>SILData.Model.SolicitudTurno.SolicitudTurno</c>.
  /// Sólo se usan los campos que SILData deserializa al armar el payload de
  /// Accept. Mantener sincronizado con la versión SILData.
  ///
  /// Tras eliminar <c>SOLTURNOS.STATUS</c> en el back, este DTO ya no
  /// expone <c>CodigoEstado</c>: el back end nunca lo manda y serializarlo
  /// era ruido. Si una vista necesita saber si la solicitud es pendiente/
  /// rechazada/cubierta, debe usar las propiedades calculadas
  /// (<see cref="EsPendiente"/>, <see cref="EsRechazada"/>,
  /// <see cref="EsCubierta"/>).
  /// </summary>
  public class SolicitudTurnoDto
  {
    public long Id { get; set; }
    public long? CuentaComprador { get; set; }
    public long CuentaVendedor { get; set; }
    public long? CuentaDestino { get; set; }
    /// <summary>0 = ZonaPortuaria (default si null en SILData).</summary>
    public int? TipoDestino { get; set; }
    public int CodigoGrano { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaSolicitado { get; set; }

    /// <summary>Cupos/turnos solicitados. FROZEN: no se actualiza en Accept.</summary>
    public int Cantidad { get; set; } = 1;

    /// <summary>Subconjunto de <see cref="Cantidad"/> futuro. FROZEN.</summary>
    public int CantidadFuturo { get; set; }
    public bool EsFuturo { get; set; }
    public string CodigoCentro { get; set; }
    public string Observacion { get; set; }
    public long? CupoId { get; set; }

    /// <summary>
    /// Acumulador de cupos aceptados en una o varias corridas de Accept
    /// (≤ <see cref="Cantidad"/>). La solicitud se considera cubierta cuando
    /// alcanza <see cref="Cantidad"/>.
    /// </summary>
    public int CantidadAceptada { get; set; }

    /// <summary>
    /// Acumulador de cupos aceptados con <see cref="EsFuturo"/>=true
    /// (≤ <see cref="CantidadFuturo"/>). Subconjunto de <see cref="CantidadAceptada"/>.
    /// </summary>
    public int CantidadFuturoAceptada { get; set; }

    /// <summary>
    /// Acumulador de cupos rechazados al cierre de la solicitud (mediante
    /// rechazo completo). Vale 0 mientras la solicitud sigue pendiente o
    /// cubierta. Invariante: Aceptada + Rechazada ≤ Cantidad.
    /// </summary>
    public int CantidadRechazada { get; set; }

    /// <summary>
    /// Acumulador de cupos rechazados con <see cref="EsFuturo"/>=true
    /// (≤ <see cref="CantidadFuturo"/>). Subconjunto de <see cref="CantidadRechazada"/>.
    /// </summary>
    public int CantidadFuturoRechazada { get; set; }

    /// <summary>
    /// True mientras la solicitud tenga cupos pendientes de asignar (no
    /// cubiertos ni rechazados).
    /// </summary>
    public bool EsPendiente => CantidadAceptada + CantidadRechazada < Cantidad;

    /// <summary>True si la solicitud fue rechazada (CantidadRechazada > 0).</summary>
    public bool EsRechazada => CantidadRechazada > 0;

    /// <summary>True si la solicitud recibió todos los cupos pedidos.</summary>
    public bool EsCubierta => CantidadAceptada >= Cantidad;
  }
}
