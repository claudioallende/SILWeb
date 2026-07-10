using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  /// <summary>
  /// Fila de la grilla de solicitudes de turno (Pantalla 1).
  /// Agrupa todas las solicitudes del mismo (grano, vendedor) para la ventana
  /// de días solicitada. Comprador y destino pueden venir null (solicitudes
  /// sólo con solicitante); cuando vienen, se exponen en la fila pero no
  /// participan de la clave de agrupación.
  /// </summary>
  public class SolicitudTurnoGrupoView
  {
    /// <summary>Identificador de la solicitud (la primera del grupo, sirve para navegar al detalle).</summary>
    public long Id { get; set; }

    public int CodigoGrano { get; set; }
    public string NombreGrano { get; set; }
    public long CuentaVendedor { get; set; }
    public string NombreVendedor { get; set; }
    public long? CuentaComprador { get; set; }
    public string NombreComprador { get; set; }
    public long? CuentaDestino { get; set; }
    public string NombreDestino { get; set; }

    /// <summary>Código del centro al que pertenece la solicitud (ROS, BSAS, CBA).</summary>
    public string CodigoCentro { get; set; }

    /// <summary>Clase CSS del badge de estado ("pending" | "asig" | "rech").</summary>
    public string EstadoBadge { get; set; } = "pending";

    /// <summary>Etiqueta visible del estado ("Pendiente" | "Asignada" | "Rechazada").</summary>
    public string EstadoLabel { get; set; } = "Pendiente";

    /// <summary>Resumen de cupos compatibles para mostrar en la columna "Cupos compatibles".</summary>
    public CupoCompatibleResumenViewModel CuposCompatibles { get; set; } = new CupoCompatibleResumenViewModel();

    /// <summary>Observaciones del solicitante (se muestran en el banner ámbar de Pantalla 2).</summary>
    public string Observacion { get; set; }

    /// <summary>Detalle de cantidad de solicitudes por fecha (TR y TO), alineado con la grilla.</summary>
    public IEnumerable<SolicitudTurnoDetalleGrupoView> CantidadFechas { get; set; }

    /// <summary>
    /// Mapa fecha (yyyy-MM-dd) → id de la solicitud que corresponde a esa fecha.
    /// Pantalla 1 agrupa solicitudes (una por fecha) bajo una sola fila visual.
    /// Cuando el operador tilda una fecha en Pantalla 2, necesitamos el id
    /// ESPECÍFICO de la solicitud de esa fecha para pedirle al backend los
    /// matches correctos (no los de la primera solicitud del grupo).
    ///
    /// Si dos o m&aacute;s solicitudes del grupo caen en la misma fecha, nos
    /// quedamos con la primera que aparece en el resultado del backend (no
    /// se da en la pr&aacute;ctica porque SOLTURNOS tiene UNIQUE impl&iacute;cito
    /// por (vendedor, grano, fecha)).
    /// </summary>
    public Dictionary<string, long> SolicitudesPorFecha { get; set; }

    /// <summary>
    /// Mapa fecha (yyyy-MM-dd) → cantidad de cupos ACEPTADOS para esa fecha
    /// (≤ Cantidad pedida). Lo deposita <c>GroupBySolicitud</c> por cada
    /// item del grupo, en la celda de su propia <c>FechaSolicitado</c>.
    /// Pantalla 1 lo muestra en la columna TO, y Pantalla 2 lo muestra en
    /// la columna "Sol. TO" para que el operador sepa cu&aacute;ntos cupos
    /// ya fueron otorgados para esa fecha antes de aceptar m&aacute;s.
    /// </summary>
    public Dictionary<string, int> FechasAceptadas { get; set; }
  }

  /// <summary>
  /// Detalle por fecha de una fila de la grilla.
  /// Permite distinguir TR (Toneladas Requeridas) y TO (Toneladas Ofrecidas/cupo disponible).
  /// </summary>
  public class SolicitudTurnoDetalleGrupoView
  {
    /// <summary>Fecha en formato yyyy-MM-dd (alineado con la key usada por el JS de la vista).</summary>
    public string Fecha { get; set; }

    /// <summary>Fecha en formato dd/MM para mostrar en el header de la grilla.</summary>
    public string FechaDisplay { get; set; }

    /// <summary>Día de la semana (1=lunes ... 7=domingo) para mostrar en el header de la grilla.</summary>
    public int DiaSemana { get; set; }

    /// <summary>Cantidad de solicitudes contractuales (TR) para esta fecha.</summary>
    public int Cantidad { get; set; }

    /// <summary>Cantidad de solicitudes futuras (TO) para esta fecha. 0 si no hay TO para esta fecha.</summary>
    public int CantidadFuturo { get; set; }

    /// <summary>
    /// Indica si para esta fecha hay cupos disponibles sin asignar.
    /// Cuando es true y Cantidad es 0, la celda de TR/TO se pinta con chip rojo.
    /// </summary>
    public bool TieneCupoDisponible { get; set; }
  }
}
