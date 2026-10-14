using System;
using System.Collections.Generic;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  /// <summary>
  /// Respuesta del endpoint /Solicitudes/GetAllPendingShiftRequests.
  /// Contiene la lista de filas (agrupadas) para cada una de las dos tablas:
  /// Contractual y Futuro, más metadata para renderizar la grilla (header de fechas).
  /// </summary>
  public class SolicitudesIndexResponseViewModel
  {
    /// <summary>Centro sobre el que se hizo la consulta (ej. "ROS").</summary>
    public string Centro { get; set; }

    /// <summary>Fecha de referencia (primer día de la ventana).</summary>
    public DateTime FechaDesde { get; set; }

    /// <summary>Cantidad de días que se devuelven (default 7).</summary>
    public int CantidadDias { get; set; } = 7;

    /// <summary>Fechas que se usaron para armar el header de la grilla (formato dd/MM).</summary>
    public List<string> FechasHeader { get; set; } = new List<string>();

    /// <summary>Filas de la tabla CONTRACTUAL.</summary>
    public List<SolicitudTurnoGrupoView> Contractuales { get; set; } = new List<SolicitudTurnoGrupoView>();

    /// <summary>Filas de la tabla FUTURO.</summary>
    public List<SolicitudTurnoGrupoView> Futuros { get; set; } = new List<SolicitudTurnoGrupoView>();
  }
}
