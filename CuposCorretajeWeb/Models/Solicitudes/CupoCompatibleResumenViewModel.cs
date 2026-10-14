using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models.Solicitudes
{
    /// <summary>
    /// Resumen de cupos compatibles calculado por el motor de matching
    /// para una solicitud de turno. Se muestra en la columna
    /// "Cupos compatibles" de la grilla (Pantalla 1).
    /// </summary>
    public class CupoCompatibleResumenViewModel
    {
        /// <summary>Cantidad de cupos con match directo (comprador, vendedor, grano y fecha exactos).</summary>
        public int Directos { get; set; }

        /// <summary>Cantidad de cupos con match parcial (alguno de los campos no coincide pero es compatible).</summary>
        public int Parciales { get; set; }

        /// <summary>Cantidad de cupos con observaciones que requieren confirmación explícita.</summary>
        public int Observaciones { get; set; }

        /// <summary>Texto resumen a mostrar en la grilla (ej. "Cupo #3872 asignado", "Sin coincidencia", "Rechazo manual 17:42").</summary>
        public string TextoResumen { get; set; }

        /// <summary>Identificador del cupo ya asignado, si la solicitud pasó a estado Asignada.</summary>
        public long? CupoAsignadoId { get; set; }
    }
}
