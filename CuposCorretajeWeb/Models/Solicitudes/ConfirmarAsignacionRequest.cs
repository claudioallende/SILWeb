using System.Collections.Generic;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  /// <summary>
  /// Payload que envía Pantalla 2 al confirmar una asignación sobre los días seleccionados.
  /// <see cref="CupoIds"/> lleva los IDs reales (long) que devolvió el motor de matching;
  /// <see cref="CupoCompatibleId"/> se conserva para compatibilidad legacy (toma el primero
  /// de la lista o null).
  /// </summary>
  public class ConfirmarAsignacionRequest
  {
    /// <summary>Id de la solicitud (en grilla, el del primer item del grupo).</summary>
    public long IdSolicitud { get; set; }

    /// <summary>Id del cupo compatible elegido en Pantalla 2 (legacy: el primero de <see cref="CupoIds"/>).</summary>
    public long? CupoCompatibleId { get; set; }

    /// <summary>Lista de IDs reales (long) de los cupos compatibles seleccionados, devueltos por el motor.</summary>
    public List<long> CupoIds { get; set; }

    /// <summary>Dict con los días seleccionados (yyyy-MM-dd) y la cantidad TR asociada.</summary>
    public Dictionary<string, int> Fechas { get; set; }

    /// <summary>
    /// Mapa cupoId → cantidad a asignar (1 por defecto). Permite que el operador
    /// asigne parcialmente los cupos disponibles para la solicitud.
    /// Hoy todos los cupos son enteros (sin subdivisión), así que este campo
    /// se valida en frontend para que el operador no pida más de lo disponible
    /// por cupo. Queda como hook para cuando se agregue subdivisión intra-cupo.
    /// </summary>
    public Dictionary<long, int> CantidadPorCupo { get; set; } = new Dictionary<long, int>();
  }
}
