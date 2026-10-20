using System;
using System.Collections.Generic;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  /// <summary>
  /// Filtro enviado como body POST a <c>ShiftRequest/GetHistoricoAsync</c>
  /// (RF-03 — Histórico de solicitudes). El verbo en CuposCorretajeWeb es
  /// GET (la pantalla llama por AJAX con jQuery), pero internamente se
  /// serializa como POST a SILData siguiendo el patrón existente de
  /// <c>GetAllPendingShiftRequestAsync</c> (filtros complejos como body).
  /// </summary>
  public class SILHistoricoSolicitudesFilterViewModel
  {
    /// <summary>Centros del operador (vía claims "Centro"). SILData los
    /// usa para acotar la query a la zona geográfica del operador.</summary>
    public List<string> Centros { get; set; } = new List<string>();

    /// <summary>"Todos" (default) / "Cumplida" / "Vencida".</summary>
    public string Estado { get; set; } = "Todos";

    /// <summary>Rango de fechas aplicado sobre FechaSolicitado.</summary>
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }

    public int? CodigoGrano { get; set; }
    public long? CuentaComprador { get; set; }
    public long? CuentaVendedor { get; set; }

    /// <summary>Búsqueda libre por CUIT, nombre comprador/vendedor, etc.</summary>
    public string BusquedaRapida { get; set; }

    /// <summary>1-based.</summary>
    public int Pagina { get; set; } = 1;

    /// <summary>Cap implícito en el controller: 1..100.</summary>
    public int TamanoPagina { get; set; } = 20;
  }
}
