using System;
using System.Collections.Generic;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  public class IndexModel
  {
    /// <summary>Cantidad de días a mostrar en la grilla (default 7).</summary>
    public int CantidadDias { get; set; } = 7;

    /// <summary>Fecha de referencia desde la que se arma la ventana (default hoy).</summary>
    public DateTime FechaReferencia { get; set; } = DateTime.Today;

    /// <summary>Centro operativo sobre el que se filtran las solicitudes (default "ROS").</summary>
    public string Centro { get; set; } = "ROS";

    /// <summary>Subtítulo dinámico de la pantalla (ej. "Solicitudes pendientes de asignación — Centro Rosario · 08/05/2026").</summary>
    public string Subtitulo { get; set; }
  }
}
