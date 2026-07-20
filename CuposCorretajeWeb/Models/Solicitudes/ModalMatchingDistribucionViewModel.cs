using System.Collections.Generic;

namespace CuposCorretajeWeb.Models.Solicitudes
{
  /// <summary>
  /// ViewModel que la vista parcial del modal recibe. Si bien el modal
  /// se hidrata mayoritariamente por JS vía AJAX, este VM fuertemente
  /// tipado permite que la vista renderice la estructura inicial con
  /// @Html.Partial / @model y que el JS localice secciones con selectores
  /// estables.
  ///
  /// No contiene la data real de matches: esa llega vía endpoints
  /// <c>CuposMatchingController.BuscarCuposConMatch</c> y
  /// <c>CuposMatchingController.ObtenerMatchPorCupo</c>.
  /// </summary>
  public class ModalMatchingDistribucionViewModel
  {
    /// <summary>
    /// Lista de cupos candidatos a mostrar (opcional: si llega vacía, el modal
    /// muestra "Aún no se buscó ningún cupo" y queda a la espera de una
    /// llamada al endpoint de búsqueda).
    /// </summary>
    public List<CupoParaMatchViewModel> Cupos { get; set; }

    /// <summary>
    /// "A" si el modal debe arrancar en Variante A (cupo con vendedor),
    /// "B" si en Variante B (sin vendedor), null si aún no se decidió
    /// (lo decidirá el JS al cargar el cupo).
    /// </summary>
    public string ModoVariant { get; set; }

    public ModalMatchingDistribucionViewModel()
    {
      Cupos = new List<CupoParaMatchViewModel>();
    }
  }
}
