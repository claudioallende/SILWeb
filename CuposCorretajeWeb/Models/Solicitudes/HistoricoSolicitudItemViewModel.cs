using System;
using System.Collections.Generic;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  /// <summary>
  /// Fila de la grilla de Histórico (RF-03). Embebe los alfanuméricos
  /// asociados a los cupos aceptados para que el toggle del acordeón
  /// NO requiera un segundo request (CA-09 del spec).
  /// </summary>
  public class HistoricoSolicitudItemViewModel
  {
    public long Id { get; set; }

    /// <summary>Fecha del turno solicitado (la fecha del turno — NO la
    /// cantidad). Es la columna "Fecha sol." de la grilla.</summary>
    public DateTime FechaSolicitado { get; set; }

    public int CodigoGrano { get; set; }
    public string NombreGrano { get; set; }

    public long CuentaVendedor { get; set; }
    public string NombreVendedor { get; set; }

    public long? CuentaComprador { get; set; }
    public string NombreComprador { get; set; }

    public string NombreDestino { get; set; }

    /// <summary>T. sol. — cantidad de turnos solicitados por la cooperativa.</summary>
    public int Cantidad { get; set; }

    /// <summary>T. asig. — cantidad de turnos efectivamente asignados.</summary>
    public int CantidadAceptada { get; set; }

    /// <summary>"Cumplida" / "Vencida". Lo calcula el backend dentro del universo
    /// histórico (fecha pasada O ya cubierta):
    /// Cumplida ⇒ CantidadAceptada ≥ Cantidad; Vencida ⇒ lo demás (va parcial
    /// o sin nada). Ojo: "Vencida" NO es "CantidadAceptada == 0" — una solicitud
    /// de 5 turnos con 2 asignados y fecha pasada es vencida, y con la regla
    /// anterior se caía de la pantalla.</summary>
    public string Estado { get; set; }

    /// <summary>Alfanuméricos asociados a los cupos aceptados
    /// (hidratados vía SOLTURNOS_DETALLE → CUPOSCORRE.NroCupo).
    /// Lista vacía si la solicitud está Vencida.</summary>
    public List<HistoricoAlfanumericoViewModel> Alfanumericos { get; set; } = new List<HistoricoAlfanumericoViewModel>();
  }
}
