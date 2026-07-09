namespace CuposCorretajeWeb.Models.Solicitudes
{
  /// <summary>
  /// Respuesta del POST a <c>SolicitudesController.ConfirmarAsignacionSeleccionada</c>.
  /// La UI la consume como JSON y muestra un toast acorde al resultado
  /// (éxito total → "Asignación confirmada", éxito parcial → "Asignaste N de M cupos").
  /// </summary>
  public class ConfirmarAsignacionResponse
  {
    /// <summary>True si la asignación se ejecutó OK.</summary>
    public bool Success { get; set; }

    /// <summary>Mensaje legible para mostrar en UI.</summary>
    public string Message { get; set; }

    /// <summary>Cupos pedidos originalmente (de la solicitud — antes del Accept).</summary>
    public int Solicitados { get; set; }

    /// <summary>Cupos efectivamente asignados en este Accept.</summary>
    public int Asignados { get; set; }

    /// <summary>Cupos que quedaron Pendientes para otra corrida (&gt; 0 indica parcial).</summary>
    public int Pendientes { get; set; }

    /// <summary>URL a la que la UI debe redirigir luego del éxito.</summary>
    public string RedirectUrl { get; set; }
  }
}
