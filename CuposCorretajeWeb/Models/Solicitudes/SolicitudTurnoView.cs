using System;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  /// <summary>
  /// Mirror local de <c>SILData.Model.SolicitudTurno.SolicitudTurnoView</c>.
  /// Sólo se usan los campos que SILData deserializa al armar el payload.
  /// Mantener sincronizado con la versión SILData.
  ///
  /// A partir del refactor que elimina <c>SOLTURNOS.STATUS</c> ya no se
  /// recibe ni se persiste <c>CodigoEstado</c>/<c>NombreEstado</c>: el estado
  /// se deriva siempre de los acumuladores
  /// (<see cref="CantidadAceptada"/>, <see cref="CantidadRechazada"/>,
  /// <see cref="Cantidad"/>, <see cref="CantidadFuturo"/>).
  /// Use las propiedades calculadas (<see cref="EsPendiente"/>,
  /// <see cref="EsRechazada"/>, <see cref="EsCubierta"/>) en lugar de switchear
  /// contra códigos mágicos.
  /// </summary>
  public class SolicitudTurnoView
  {
    public long Id { get; set; }
    public long? CuentaComprador { get; set; }
    public string NombreComprador { get; set; }
    public long CuentaVendedor { get; set; }
    public string NombreVendedor { get; set; }
    public long? CuentaDestino { get; set; }
    public string NombreDestino { get; set; }
    public int CodigoGrano { get; set; }
    public string NombreGrano { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaSolicitado { get; set; }
    public bool EsFuturo { get; set; }
    public string CodigoCentro { get; set; }
    public string NombreCentro { get; set; }
    public string Observacion { get; set; }
    public short? EstadoCupo { get; set; }
    public short? CtgCupo { get; set; }

    /// <summary>Acumulador de cupos aceptados (≤ Cantidad).</summary>
    public int CantidadAceptada { get; set; }

    /// <summary>Acumulador de cupos futuros aceptados (≤ CantidadFuturo).</summary>
    public int CantidadFuturoAceptada { get; set; }

    /// <summary>
    /// True si la solicitud está pendiente de resolución (a&uacute;n no
    /// cubierta ni rechazada). Calculada desde los acumuladores; reemplaza al
    /// antiguo <c>CodigoEstado == 1</c>.
    /// </summary>
    public bool EsPendiente => CantidadAceptada + CantidadRechazada < Cantidad;

    /// <summary>
    /// True si la solicitud fue rechazada (cupo del remanente cerrado por
    /// rechazo manual). Calculada desde los acumuladores; reemplaza al
    /// antiguo <c>CodigoEstado == 3</c>.
    /// </summary>
    public bool EsRechazada => CantidadRechazada > 0;

    /// <summary>
    /// True si la solicitud recibió la cantidad total de cupos pedidos
    /// (CantidadAceptada == Cantidad). Aplica a contractuales y futuros.
    /// </summary>
    public bool EsCubierta => CantidadAceptada >= Cantidad;
  }
}
