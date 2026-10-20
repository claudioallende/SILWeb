using System;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  /// <summary>
  /// Alfanumérico asociado a un cupo aceptado de una solicitud histórica
  /// (RF-03). Se hidrata desde <c>cuposcorre.NroCupo</c> vía
  /// <c>SOLTURNOS_DETALLE (solicitud_id, cupo_id)</c>.
  /// </summary>
  public class HistoricoAlfanumericoViewModel
  {
    /// <summary>Alfanumérico del cupo (cuposcorre.NroCupo). Ej.: "AAAA1234".</summary>
    public string Alfanumerico { get; set; }

    /// <summary>PK de CUPOSCORRE — útil para trazabilidad.</summary>
    public long CupoId { get; set; }

    /// <summary>Fecha de asignación (= SOLTURNOS_DETALLE.fecha_creacion).</summary>
    public DateTime? FechaAsignacion { get; set; }

    /// <summary>Puerto/zona geográfica del cupo (cuposcorre.PUERTOCTA → MVCUPOSPUERTO.nombre).</summary>
    public string Puerto { get; set; }

    /// <summary>
    /// "Directo" / "Parcial". <b>No se persiste en SILData</b> (se computa
    /// on-demand por <c>ACA.Matching.Engine</c> al matchear). Queda en null
    /// por ahora; si el PO lo pide persistido, hay que extender
    /// SOLTURNOS_DETALLE con una columna nueva (fuera del alcance de RF-03).
    /// </summary>
    public string TipoMatch { get; set; }
  }
}
