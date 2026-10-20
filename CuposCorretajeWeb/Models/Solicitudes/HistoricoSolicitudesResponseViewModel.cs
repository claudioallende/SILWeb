using System.Collections.Generic;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  /// <summary>
  /// Respuesta del endpoint <c>/Solicitudes/GetHistoricoSolicitudes</c> (RF-03).
  /// Refleja 1:1 el envelope <c>SolicitudHistoricoResponse</c> de SILData:
  /// items + resumen + paginación. El mapeo es por nombre de propiedad, así
  /// que cada bloque tiene que conservar los nombres exactos que emite el
  /// backend (Json.NET noLowerCase por defecto de <c>Content(...)</c>).
  /// </summary>
  public class HistoricoSolicitudesResponseViewModel
  {
    /// <summary>Filas de la página actual.</summary>
    public List<HistoricoSolicitudItemViewModel> Items { get; set; } = new List<HistoricoSolicitudItemViewModel>();

    /// <summary>Totales para las 3 tarjetas (Cumplidas / Vencidas / Total histórico),
    /// calculados sobre todo el conjunto filtrado — no sobre la página — y sin
    /// aplicar el filtro de estado, para que sirvan de contexto.</summary>
    public HistoricoSolicitudesResumenViewModel Resumen { get; set; } = new HistoricoSolicitudesResumenViewModel();

    /// <summary>Metadata de paginación. Va anidada (y no en la raíz) para
    /// seguir el mismo criterio que el backend: los DTOs de datos no llevan
    /// atributos de paginación encima.</summary>
    public PaginacionViewModel Paginacion { get; set; } = new PaginacionViewModel();
  }

  /// <summary>
  /// Equivalente en el front de <c>PaginacionResultadoDto</c> de SILData.
  /// </summary>
  public class PaginacionViewModel
  {
    public int TotalRegistros { get; set; }
    public int TotalPaginas { get; set; }

    /// <summary>1-based.</summary>
    public int Pagina { get; set; } = 1;

    public int TamanoPagina { get; set; } = 20;
  }

  /// <summary>
  /// Totales por estado final, calculado por el backend sobre el conjunto
  /// filtrado (no sobre la página actual).
  /// </summary>
  public class HistoricoSolicitudesResumenViewModel
  {
    public int Cumplidas { get; set; }
    public int Vencidas { get; set; }
    public int Total { get; set; }
  }
}