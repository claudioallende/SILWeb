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
  }
}