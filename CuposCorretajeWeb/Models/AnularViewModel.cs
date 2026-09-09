using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models
{
    public class AnularViewModel
    {

    }

    public class AnuladoViewModel
    {
        public long Id { get; set; }
        public string Alfanumerico { get; set; }
        public long CuentaVendedor { get; set; }
        public int Status { get; set; }
    }

    /// <summary>
    /// Body del action MVC <c>CuposController.AnularDistribucion</c>.
    /// Se usa en lugar de <c>[FromBody] long[]</c> porque en ASP.NET MVC 4.6.1
    /// <c>[FromBody]</c> no funciona (es de Web API). En su lugar, el
    /// <c>JsonValueProviderFactory</c> builtin de MVC deserializa el body
    /// JSON cuando el <c>contentType</c> del request es
    /// <c>application/json</c> y el parámetro del action es un tipo
    /// complejo (no <c>long[]</c>, que no se bindea por JSON).
    ///
    /// Contrato esperado por el cliente:
    /// <code>{ "cupoIds": [123, 456, 789] }</code>
    /// </summary>
    public class AnularDistribucionRequestDto
    {
        /// <summary>PKs de los cupos cuyas distribuciones se quieren revertir.</summary>
        public long[] CupoIds { get; set; }
    }

    /// <summary>
    /// DTO para deserializar la respuesta del endpoint
    /// <c>POST /api/ShiftRequest/AnularDistribucion</c> de SILData.
    /// Se usa desde <c>CuposController.AnularDistribucion</c> (proxy MVC).
    /// </summary>
    public class AnularDistribucionResponseDto
    {
        /// <summary>True si al menos un item se procesó como Exitoso.</summary>
        public bool AlMenosUnoExitoso { get; set; }

        /// <summary>Cantidad de cupos revertidos con éxito.</summary>
        public int CantidadExitosos { get; set; }

        /// <summary>Cantidad de cupos sin distribución activa (skipped).</summary>
        public int CantidadSkipped { get; set; }

        /// <summary>Cantidad de cupos con fallo al revertir.</summary>
        public int CantidadFallos { get; set; }

        /// <summary>Detalle por cupo.</summary>
        public List<AnularDistribucionItemResponseDto> Items { get; set; }
    }

    public class AnularDistribucionItemResponseDto
    {
        public long CupoId { get; set; }

        /// <summary>0 = Exitoso, 1 = Skipped, 2 = Fallo.</summary>
        public int Estado { get; set; }

        public long SolicitudId { get; set; }

        public string MotivoFalla { get; set; }
    }
}