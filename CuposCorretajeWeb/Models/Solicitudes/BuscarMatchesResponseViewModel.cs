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
    /// <summary>
    /// Flag copiado del backend. true = el campo coincide entre solicitud y cupo.
    /// Se usa en Pantalla 2 para mostrar u ocultar la fila correspondiente en la card.
    /// </summary>
    public bool VendedorCoincide { get; set; }
    /// <summary>
    /// Flag copiado del backend. true = el campo coincide entre solicitud y cupo
    /// (o ambos no exigen comprador). Se usa para mostrar/ocultar la fila Comprador.
    /// </summary>
    public bool CompradorCoincide { get; set; }
    /// <summary>
    /// Flag copiado del backend. true = el campo coincide entre solicitud y cupo
    /// (o ambos no exigen destino). Se usa para mostrar/ocultar la fila Destino.
    /// </summary>
    public bool DestinoCoincide { get; set; }
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
  /// Resumen del cupo involucrado en el match. SILData hidrata
  /// <c>NombreVendedor</c>, <c>NombreComprador</c>, <c>NombreDestino</c> y
  /// <c>NombreGrano</c> vía AccountService + GeographicalAereaService. Cuando
  /// el catálogo no tiene el código, los nombres quedan null y la card
  /// muestra el ID como fallback. No hace falta que el controller MVC haga
  /// una llamada extra a GetByVendedorAsync para traer estos datos: ya
  /// vienen en el payload de Matches.
  /// </summary>
  public class MatchCupoResumenViewModel
  {
    public long Id { get; set; }
    public string CodGrano { get; set; }
    public string CodVendSIL { get; set; }
    public string CodCompSIL { get; set; }
    public string CodDestino { get; set; }
    public DateTime? Fecha { get; set; }
    public string NombreVendedor { get; set; }
    public string NombreComprador { get; set; }
    public string NombreDestino { get; set; }
    public string NombreGrano { get; set; }
  }
}