using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static CuposCorretajeWeb.Models.Enums.Enum;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  public class ShiftRequestPendingViewModel
  {
    public long Id { get; set; }
    public long? CuentaComprador { get; set; }
    public string NombreComprador { get; set; }
    public long CuentaVendedor { get; set; }
    public string NombreVendedor { get; set; }
    public long? CuentaDestino { get; set; }
    public string NombreDestino { get; set; }
    public TipoDestino? TipoDestino { get; set; }
    public int CodigoEstado { get; set; }
    public int NombreEstado { get; set; }
    public int CodigoGrano { get; set; }
    public string NombreGrano { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaSolicitado { get; set; }

    /// <summary>Cantidad de solicitudes contractuales para la fecha solicitada. Usado en la tabla CONTRACTUAL.</summary>
    public int Cantidad { get; set; } = 1;

    /// <summary>Cantidad de solicitudes futuras para la fecha solicitada. Usado en la tabla FUTURO.</summary>
    public int CantidadFuturo { get; set; }

    public bool EsFuturo { get; set; }
    public string CodigoCentro { get; set; }
    public string NombreCentro { get; set; }
    public string Observacion { get; set; }
    public short? EstadoCupo { get; set; }
    public short? CtgCupo { get; set; }

    /// <summary>
    /// Devuelve la clave CSS del badge de estado para la grilla de Pantalla 1.
    /// Valores posibles: "pending" | "asig" | "rech".
    /// </summary>
    public string GetEstadoBadgeClass()
    {
      // 1 = Pendiente, 2 = Asignada, 3 = Rechazada.
      // Si el EstadoCupo indica asignación, gana sobre CodigoEstado.
      if (EstadoCupo.HasValue && EstadoCupo.Value == 2) return "asig";
      switch (CodigoEstado)
      {
        case 2: return "asig";
        case 3: return "rech";
        default: return "pending";
      }
    }

    /// <summary>
    /// Devuelve la etiqueta visible del estado de la solicitud.
    /// </summary>
    public string GetEstadoBadgeLabel()
    {
      switch (GetEstadoBadgeClass())
      {
        case "asig": return "Asignada";
        case "rech": return "Rechazada";
        default: return "Pendiente";
      }
    }
  }
}
