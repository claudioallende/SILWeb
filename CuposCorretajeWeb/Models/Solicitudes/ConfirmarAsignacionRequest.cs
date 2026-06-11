using System.Collections.Generic;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  /// <summary>
  /// Payload que envía Pantalla 2 al confirmar una asignación sobre los días seleccionados.
  /// Por ahora <see cref="CupoCompatibleId"/> es null (el panel de Cupos Compatibles es placeholder);
  /// cuando se enchufe el motor de matching, este campo viajará poblado.
  /// </summary>
  public class ConfirmarAsignacionRequest
  {
    /// <summary>Id de la solicitud (en grilla, el del primer item del grupo).</summary>
    public long IdSolicitud { get; set; }

    /// <summary>Id del cupo compatible elegido en Pantalla 2 (null por ahora).</summary>
    public long? CupoCompatibleId { get; set; }

    /// <summary>Dict con los días seleccionados (yyyy-MM-dd) y la cantidad TR asociada.</summary>
    public Dictionary<string, int> Fechas { get; set; }
  }
}
