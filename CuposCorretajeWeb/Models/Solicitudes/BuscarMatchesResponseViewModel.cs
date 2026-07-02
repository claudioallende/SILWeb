using System;
using System.Collections.Generic;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  /// <summary>
  /// Respuesta que el controller <c>Solicitudes/BuscarMatches</c> serializa a
  /// JSON y consume Pantalla 2. Es un espejo recortado de
  /// <c>SILData.Model.SolicitudTurno.MatchesResultDto</c>: sólo trae los
  /// campos que la UI necesita para pintar las cards y agrupar por tipo de
  /// match. Se mantienen los nombres de propiedades idénticos a los DTOs de
  /// SILData para minimizar el mapeo en el controller.
  /// </summary>
  public class BuscarMatchesResponseViewModel
  {
    /// <summary>Items (pares solicitud-cupo) clasificados por el motor.</summary>
    public List<MatchItemViewModel> Items { get; set; } = new List<MatchItemViewModel>();
  }

  /// <summary>Item individual de MatchesResultDto.Items.</summary>
  public class MatchItemViewModel
  {
    public long SolicitudId { get; set; }
    public long CupoId { get; set; }
    /// <summary>"Directo" | "Parcial" | "Condicional" | null (Incompatible).</summary>
    public string MatchType { get; set; }
    /// <summary>Razón textual para Parcial / Condicional / Incompatible.</summary>
    public string Razon { get; set; }
    public MatchSolicitudResumenViewModel Solicitud { get; set; } = new MatchSolicitudResumenViewModel();
    public MatchCupoResumenViewModel Cupo { get; set; } = new MatchCupoResumenViewModel();
  }

  /// <summary>Resumen de la solicitud involucrada en el match.</summary>
  public class MatchSolicitudResumenViewModel
  {
    public long Id { get; set; }
    public long CuentaVendedor { get; set; }
    public long? CuentaComprador { get; set; }
    public int CodigoGrano { get; set; }
    public long? CuentaDestino { get; set; }
    public string TipoDestino { get; set; }
    public int Cantidad { get; set; }
    public DateTime FechaSolicitado { get; set; }
    public string Observacion { get; set; }
  }

  /// <summary>
  /// Resumen del cupo involucrado en el match. Hoy SILData sólo expone IDs
  /// (CodVendSIL, CodCompSIL, CodDestino) — sin nombres. Las cards de Pantalla
  /// 2 muestran los IDs con fallback a em-dash para nulls.
  /// </summary>
  public class MatchCupoResumenViewModel
  {
    public long Id { get; set; }
    public string CodGrano { get; set; }
    public string CodVendSIL { get; set; }
    public string CodCompSIL { get; set; }
    public string CodDestino { get; set; }
    public DateTime? Fecha { get; set; }
  }
}