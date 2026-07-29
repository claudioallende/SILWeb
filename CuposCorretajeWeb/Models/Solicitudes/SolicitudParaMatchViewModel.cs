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
    public string Vendedor { get; set; }
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
    /// Detalle por día (una entrada por fecha solicitada), para habilitar
    /// el ajuste +/− de Variante B. Si la API devuelve una solicitud
    /// consolidada por (grano+vendedor+comprador+destino), se desglosa
    /// aquí día por día.
    /// </summary>
    public List<MatchDiaItem> Dias { get; set; }

    public SolicitudParaMatchViewModel()
    {
      Dias = new List<MatchDiaItem>();
    }
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
