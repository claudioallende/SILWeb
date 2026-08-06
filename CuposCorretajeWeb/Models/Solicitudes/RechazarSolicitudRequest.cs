using System.Collections.Generic;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  public class RechazarSolicitudRequest
  {
    /// <summary>Id de la solicitud a rechazar (legacy: el primero del grupo, dejado por compatibilidad).</summary>
    public long IdSolicitud { get; set; }

    /// <summary>
    /// Lista completa de solicitudIds del grupo (uno por fecha). Si viene,
    /// el backend rechaza TODAS las solicitudes del grupo en una sola llamada,
    /// evitando que queden turnos pendientes para las fechas no incluidas en
    /// el listado. Si viene vacía o null, se usa <see cref="IdSolicitud"/>.
    /// </summary>
    public List<long> IdsSolicitudes { get; set; }
  }
}