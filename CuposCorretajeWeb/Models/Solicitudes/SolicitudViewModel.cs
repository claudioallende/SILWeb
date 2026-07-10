using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  public class SolicitudViewModel
  {
    /// <summary>Id de la solicitud (en grilla, el del primer item del grupo).</summary>
    public long IdSolicitud { get; set; }

    public string NombreGrano { get; set; }
    public int CodigoGrano { get; set; }
    public long? CuentaVendedor { get; set; }
    public string Solicitante { get; set; }
    public long? CuentaComprador { get; set; }
    public string Comprador { get; set; }
    public long? CuentaDestino { get; set; }
    public string Zona { get; set; }

    /// <summary>Nombre del destino/puerto (para la cabecera resumen de Pantalla 2).</summary>
    public string NombreDestino { get; set; }

    /// <summary>Código del centro (ROS, BSAS, CBA) al que pertenece la solicitud.</summary>
    public string CodigoCentro { get; set; }

    /// <summary>Observaciones del solicitante (mostradas en el banner ámbar de Pantalla 2).</summary>
    public string Observacion { get; set; }

    /// <summary>Clave CSS del badge de estado ("pending" | "asig" | "rech").</summary>
    public string EstadoBadge { get; set; } = "pending";

    /// <summary>Etiqueta visible del estado ("Pendiente" | "Asignada" | "Rechazada").</summary>
    public string EstadoLabel { get; set; } = "Pendiente";

    /// <summary>True si la solicitud es editable (sólo Pendientes).</summary>
    public bool EsEditable => EstadoBadge == "pending";

    public Dictionary<string, int> Fechas { get; set; }

    /// <summary>
    /// Acumulador total de cupos aceptados al momento de abrir la pantalla
    /// (≤ Cantidad original de la solicitud). Sirve para mostrar en el
    /// banner "X de N ya aceptados" y deshabilitar acciones redundantes.
    /// </summary>
    public int CantidadAceptada { get; set; }

    /// <summary>
    /// Acumulador total de cupos futuros aceptados (≤ CantidadFuturo).
    /// Subset de <see cref="CantidadAceptada"/> cuando la solicitud es
    /// futura. En solicitudes contractuales siempre vale 0.
    /// </summary>
    public int CantidadFuturoAceptada { get; set; }

    /// <summary>Cantidad original pedida (suma de TR en el mapa Fechas).</summary>
    public int CantidadOriginal { get; set; }

    /// <summary>
    /// Mapa fecha (yyyy-MM-dd) → id de la solicitud para esa fecha. Copiado
    /// desde <see cref="SolicitudTurnoGrupoView.SolicitudesPorFecha"/> al
    /// armar el VM de Pantalla 2. Pantalla 2 lo usa para resolver la
    /// solicitudId del d&iacute;a tildado y as&iacute; consultar matches
    /// contra la solicitud correcta (no la primera del grupo).
    /// </summary>
    public Dictionary<string, long> SolicitudesPorFecha { get; set; }
  }
}