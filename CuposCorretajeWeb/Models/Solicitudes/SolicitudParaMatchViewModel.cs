using System;
using System.Collections.Generic;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  /// <summary>
  /// ViewModel que consume el modal de Pantalla 3 (Variante A/B).
  /// Representa una solicitud pendiente con su clasificación de match
  /// y los días en los que se la puede asignar.
  ///
  /// Es la proyección que el frontend necesita para renderizar tanto las
  /// tarjetas por día (Variante A) como la tabla expandible por solicitante
  /// (Variante B), más el detalle de observaciones para el match Condicional.
  /// </summary>
  public class SolicitudParaMatchViewModel
  {
    public long Id { get; set; }
    /// <summary>Cuenta del vendedor de la solicitud (CUIT/cuenta, string para preservar ceros a la izquierda).</summary>
    public string Vendedor { get; set; }
    /// <summary>
    /// Nombre del vendedor (<see cref="Vendedor"/>). Preferido por la UI sobre
    /// <see cref="Vendedor"/> para mostrar en la columna "Solicitante" del
    /// modal de matching. Si llega vacío, la UI cae al valor de
    /// <see cref="Vendedor"/>.
    /// </summary>
    public string NombreVendedor { get; set; }
    public string Comprador { get; set; }
    public string Destino { get; set; }
    public string Zona { get; set; }
    public DateTime FechaSolicitado { get; set; }
    /// <summary>Cantidad total pedida por la solicitud (SOLTURNOS.CANTIDAD).</summary>
    public int Cantidad { get; set; }
    /// <summary>
    /// Cantidad disponible para asignar en este Accept
    /// (Cantidad - CantidadAceptada - CantidadRechazada, según backend).
    /// La UI muestra este valor como "disponibles" y valida contra este límite.
    /// </summary>
    public int CantidadDisponible { get; set; }
    /// <summary>Cantidad rechazada acumulada (SOLTURNOS.CANTIDAD_RECHAZADA).</summary>
    public int CantidadRechazada { get; set; }
    public int CantidadFuturo { get; set; }
    public string Observacion { get; set; }
    /// <summary>Días enteros desde el ingreso de la solicitud hasta hoy (heurística simple).</summary>
    public int AntiguedadDias { get; set; }

    /// <summary>Tipo de match calculado por el backend: "Directo" | "Parcial" | "Condicional".</summary>
    public string MatchType { get; set; }
    /// <summary>Razón de incompatibilidad si !Compatible; null en caso contrario.</summary>
    public string MatchRazon { get; set; }

    /// <summary>
    /// Cupos físicos disponibles que pueden satisfacer esta solicitud.
    /// Cada item tiene <see cref="CupoDisponibleParaSolicitud.Id"/>,
    /// <see cref="CupoDisponibleParaSolicitud.Fecha"/> y
    /// <see cref="CupoDisponibleParaSolicitud.Cantidad"/> (cuántos cupos hay
    /// en esa fecha). Una solicitud puede matchear con varios cupos en
    /// fechas distintas — la suma de <c>Cantidad</c> es lo que el operador
    /// puede asignar. Esta lista reemplaza al antiguo lookup por
    /// <c>cupo.Id</c> en el frontend (el "cupo contenedor" sintético con
    /// Id=0 ya no lleva los cupos hijos).
    /// </summary>
    public List<CupoDisponibleParaSolicitud> Cupos { get; set; }

    /// <summary>
    /// Detalle por día (una entrada por fecha solicitada), para habilitar
    /// el ajuste +/− de Variante B. Si la API devuelve una solicitud
    /// consolidada por (grano+vendedor+comprador+destino), se desglosa
    /// aquí día por día.
    /// </summary>
    public List<MatchDiaItem> Dias { get; set; }

    public SolicitudParaMatchViewModel()
    {
      Dias = new List<MatchDiaItem>();
      Cupos = new List<CupoDisponibleParaSolicitud>();
    }
  }

  /// <summary>
  /// Cupo físico que el motor de matching reportó como compatible con una
  /// solicitud. Proyectado dentro de
  /// <see cref="SolicitudParaMatchViewModel.Cupos"/> para que el frontend
  /// pueda renderizar la lista de cupos disponibles por solicitud y, en el
  /// confirm, emitir el listado
  /// <c>(SolicitudId, CupoSeleccionadoId, Cantidad=1)</c> que espera
  /// <c>POST /api/Cupos/ActualizarDistribucion</c> en modo SolicitudMatch.
  /// </summary>
  public class CupoDisponibleParaSolicitud
  {
    public long Id { get; set; }
    public DateTime Fecha { get; set; }
    public int Cantidad { get; set; }
    public string MatchType { get; set; }
    public string MatchRazon { get; set; }
  }

  /// <summary>
  /// Item de desglose por día para Variante B.
  /// </summary>
  public class MatchDiaItem
  {
    public DateTime Fecha { get; set; }
    public int Cantidad { get; set; }
  }
}
