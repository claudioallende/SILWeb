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
    /// <summary>Id de la solicitud (en grilla, el del primer item del grupo). Legacy.</summary>
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

    /// <summary>
    /// Multi-solicitud: Pantalla 1 agrupa N solicitudes (una por fecha) bajo
    /// la misma fila visual. Cuando el operador tilda 2+ d&iacute;as con
    /// solicitudId distintos y elige cupos, se arman N asignaciones y se
    /// mandan como una sola corrida. El controller hace un Accept por cada
    /// entrada y agrega los resultados.
    ///
    /// Si viene null/empty, el controller cae al flujo legacy (un solo
    /// Accept contra <see cref="IdSolicitud"/>).
    /// </summary>
    public List<AsignacionPorSolicitud> AsignacionesPorSolicitud { get; set; }
  }

  /// <summary>
  /// Una unidad de asignaci&oacute;n dentro de <see cref="ConfirmarAsignacionRequest.AsignacionesPorSolicitud"/>.
  /// Identifica la solicitud a la que se le asignan cupos, el d&iacute;a
  /// (del lote de d&iacute;as tildados) al que aplica, y la lista concreta
  /// de cupos con su cantidad individual.
  /// </summary>
  public class AsignacionPorSolicitud
  {
    /// <summary>Id de la solicitud a la que se le asignan estos cupos.</summary>
    public long IdSolicitud { get; set; }

    /// <summary>Fecha del d&iacute;a tildado (yyyy-MM-dd) al que aplica esta asignaci&oacute;n.</summary>
    public string Fecha { get; set; }

    /// <summary>Cantidad TR pedida originalmente por esta solicitud para ese d&iacute;a (validaci&oacute;n client + server).</summary>
    public int Tr { get; set; }

    /// <summary>IDs reales (long) de los cupos a asignar a esta solicitud.</summary>
    public List<long> CupoIds { get; set; }

    /// <summary>Mapa cupoId → cantidad a asignar. Default 1.</summary>
    public Dictionary<long, int> CantidadPorCupo { get; set; } = new Dictionary<long, int>();
  }
}
