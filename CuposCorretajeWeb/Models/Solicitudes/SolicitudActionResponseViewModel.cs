namespace CuposCorretajeWeb.Models.Solicitudes
{
  /// <summary>
  /// Respuesta genérica para endpoints de acción sobre una solicitud
  /// (Guardar, Confirmar, Rechazar). La UI la consume como JSON.
  /// </summary>
  public class SolicitudActionResponseViewModel
  {
    /// <summary>True si la acción se ejecutó OK.</summary>
    public bool Success { get; set; }

    /// <summary>Mensaje legible para mostrar en UI (típicamente en un Swal).</summary>
    public string Message { get; set; }

    /// <summary>URL a la que la UI debe redirigir luego del éxito (ej. /Solicitudes/Index).</summary>
    public string RedirectUrl { get; set; }
  }
}
