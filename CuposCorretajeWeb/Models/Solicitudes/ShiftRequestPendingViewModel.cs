using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static CuposCorretajeWeb.Models.Enums.Enum;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  /// <summary>
  /// ViewModel para la grilla de Pantalla 1 (Solicitudes). Se hidrata desde
  /// <c>POST /api/ShiftRequest/GetAllPendingShiftRequestAsync</c> y agrupa por
  /// (grano, vendedor, comprador, destino) para armar el response de la grilla.
  ///
  /// Tras el refactor que elimina <c>SOLTURNOS.STATUS</c>, este VM deriva su
  /// estado de los acumuladores (<see cref="CantidadAceptada"/>,
  /// <see cref="CantidadRechazada"/>, <see cref="Cantidad"/>). El VM usa
  /// <see cref="GetEstadoBadgeClass"/> para traducir esa derivación al mismo
  /// set de claves CSS que la UI ya consume ("pending" | "asig" | "rech").
  /// </summary>
  public class ShiftRequestPendingViewModel
  {
    public long Id { get; set; }
    public long? CuentaComprador { get; set; }
    public string NombreComprador { get; set; }
    public long CuentaVendedor { get; set; }
    public string NombreVendedor { get; set; }
    public long? CuentaDestino { get; set; }
    public string NombreDestino { get; set; }
    public TipoDestino? TipoDestino { get; set; }
    public int CodigoGrano { get; set; }
    public string NombreGrano { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaSolicitado { get; set; }

    /// <summary>Cantidad de solicitudes contractuales para la fecha solicitada. Usado en la tabla CONTRACTUAL.</summary>
    public int Cantidad { get; set; } = 1;

    /// <summary>Cantidad de solicitudes futuras para la fecha solicitada. Usado en la tabla FUTURO.</summary>
    public int CantidadFuturo { get; set; }

    public bool EsFuturo { get; set; }
    public string CodigoCentro { get; set; }
    public string NombreCentro { get; set; }
    public string Observacion { get; set; }
    public short? EstadoCupo { get; set; }
    public short? CtgCupo { get; set; }

    /// <summary>
    /// Acumulador de cupos aceptados (≤ Cantidad). Se setea en Accept.
    /// </summary>
    public int CantidadAceptada { get; set; }

    /// <summary>
    /// Acumulador de cupos futuros aceptados (≤ CantidadFuturo). Subset
    /// de <see cref="CantidadAceptada"/> cuando la solicitud es EsFuturo=true.
    /// </summary>
    public int CantidadFuturoAceptada { get; set; }

    /// <summary>
    /// Acumulador de cupos rechazados al cierre del remanente (rechazo
    /// manual). Vale 0 mientras la solicitud sigue pendiente. Subset del
    /// remanente entre aceptados y pedidos originales.
    /// </summary>
    public int CantidadRechazada { get; set; }

    /// <summary>
    /// Acumulador de cupos futuros rechazados (≤ CantidadFuturo).
    /// Subset de <see cref="CantidadRechazada"/>.
    /// </summary>
    public int CantidadFuturoRechazada { get; set; }

    /// <summary>
    /// True si la solicitud está pendiente (a&uacute;n no cubierta ni rechazada).
    /// </summary>
    public bool EsPendiente => CantidadAceptada + CantidadRechazada < Cantidad;

    /// <summary>True si la solicitud fue rechazada (rechazo manual).</summary>
    public bool EsRechazada => CantidadRechazada > 0;

    /// <summary>True si la solicitud recibió todos los cupos pedidos.</summary>
    public bool EsCubierta => CantidadAceptada >= Cantidad;

    /// <summary>
    /// Devuelve la clave CSS del badge de estado para la grilla de Pantalla 1.
    /// Valores posibles: "pending" | "asig" | "rech".
    ///
    /// Regla derivada de los acumuladores (reemplaza al switch sobre
    /// <c>CodigoEstado</c>):
    /// <list type="bullet">
    ///   <item>Si <see cref="EstadoCupo"/> indica asignación (=2), es "asig".</item>
    ///   <item>Si <see cref="EsCubierta"/>, es "asig".</item>
    ///   <item>Si <see cref="EsRechazada"/>, es "rech".</item>
    ///   <item>Si <see cref="EsPendiente"/>, es "pending".</item>
    /// </list>
    /// </summary>
    public string GetEstadoBadgeClass()
    {
      if (EstadoCupo.HasValue && EstadoCupo.Value == 2) return "asig";
      if (EsCubierta) return "asig";
      if (EsRechazada) return "rech";
      return "pending";
    }

    /// <summary>
    /// Devuelve la etiqueta visible del estado de la solicitud.
    /// </summary>
    public string GetEstadoBadgeLabel()
    {
      switch (GetEstadoBadgeClass())
      {
        case "asig": return "Asignada";
        case "rech": return "Rechazada";
        default: return "Pendiente";
      }
    }
  }
}
