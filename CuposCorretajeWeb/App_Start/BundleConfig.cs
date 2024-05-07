using System.Web;
using System.Web.Optimization;

namespace CuposCorretajeWeb
{
  public class BundleConfig
  {
    // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
    public static void RegisterBundles(BundleCollection bundles)
    {
      bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                  "~/Scripts/jquery-{version}.js"));

      bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                  "~/Scripts/jquery.validate*"));

      // Use the development version of Modernizr to develop with and learn from. Then, when you're
      // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
      bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                  "~/Scripts/modernizr-*"));

      bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/Popper.min.js",
                "~/Scripts/bootstrap.js",
                "~/Scripts/respond.js"));

      bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap.min.css",
                "~/Content/site.css"));

      bundles.Add(new StyleBundle("~/Content/bootstrap").Include(
                "~/Content/bootstrap.min.css"));

      bundles.Add(new StyleBundle("~/Content/autocompletecss").Include(
                "~/Content/jquery-ui-autocomplete.min.css"));

      bundles.Add(new ScriptBundle("~/bundles/autocompletejs").Include(
                "~/Scripts/jquery-ui-autocomplete.min.js"));

      bundles.Add(new StyleBundle("~/Content/datepickercss").Include(
                "~/Content/jquery-ui-datepicker.min.css"));

      bundles.Add(new StyleBundle("~/Content/spinner").Include(
                "~/Content/loading.css",
                "~/Content/loading-btn.css"));

      bundles.Add(new ScriptBundle("~/bundles/spinner").Include(
                "~/Scripts/Spinner.js"));

      bundles.Add(new ScriptBundle("~/bundles/datepickerjs").Include(
                "~/Scripts/jquery-ui-datepicker.min.js"));

      bundles.Add(new ScriptBundle("~/bundles/ValidaCuit").Include(
                "~/Scripts/ValidaCuit.js"));

      bundles.Add(new ScriptBundle("~/bundles/Cuit").Include(
                "~/Scripts/Cuit.js"));

      bundles.Add(new ScriptBundle("~/bundles/Distribucion").Include(
                "~/Scripts/Distribucion.js",
                "~/Scripts/ControlMaximosDistribucion.js",
                "~/Scripts/AjaxDistribucion.js",
                "~/Scripts/TablaContratos.js",
                "~/Scripts/datatables.tabulacion-input.js"));

      bundles.Add(new ScriptBundle("~/bundles/ControlaInputs").Include(
                "~/Scripts/ControlaInputs.js"));

      bundles.Add(new ScriptBundle("~/bundles/Nuevo").Include(
                "~/Scripts/Alert.js",
                "~/Scripts/ControlaCargaConsignacion.js",
                "~/Scripts/Nuevo.js"));

      bundles.Add(new StyleBundle("~/Content/datatables").Include(
                "~/Content/datatables.min.css"));

      bundles.Add(new StyleBundle("~/Content/datatablesv2").Include(
                "~/Content/datatables-v2_0_2.min.css"));

      bundles.Add(new ScriptBundle("~/bundles/datatables").Include(
                "~/Scripts/datatables.min.js"));

      bundles.Add(new ScriptBundle("~/bundles/datatablesv2").Include(
                "~/Scripts/datatables-v2_0_2.min.js"));

      bundles.Add(new ScriptBundle("~/bundles/Detalle").Include(
                "~/Scripts/Observable.js",
                "~/Scripts/InformarPorLote.js"));

      bundles.Add(new StyleBundle("~/Content/multiselect").Include(
                "~/Content/bootstrap-multiselect.css"));

      bundles.Add(new ScriptBundle("~/bundles/multiselect").Include(
                "~/Scripts/Popper.min.js",
                "~/Scripts/bootstrap-multiselect.js"));

      bundles.Add(new StyleBundle("~/Content/skeleton").Include(
                "~/Content/skeleton.css"));

      bundles.Add(new StyleBundle("~/Content/WordTag").Include(
                "~/Content/tokenfield-typeahead.min.css",
                "~/Content/bootstrap-tokenfield.min.css"));

      bundles.Add(new ScriptBundle("~/bundles/skeleton").Include(
                "~/Scripts/skeleton.js"));

      bundles.Add(new ScriptBundle("~/bundles/Consignaciones").Include(
                "~/Scripts/ModalConsignaciones.js"));

      bundles.Add(new ScriptBundle("~/bundles/popper").Include(
                "~/Scripts/Popper.min.js"));

      bundles.Add(new ScriptBundle("~/bundles/Editar").Include(
                "~/Scripts/Editar.js"));

      bundles.Add(new ScriptBundle("~/bundles/RelacionCentroPuerto").Include(
                "~/Scripts/RelacionCentroPuerto.js"));

      bundles.Add(new ScriptBundle("~/bundles/RelacionGranoStop").Include(
                "~/Scripts/RelacionGranoStop.js"));

      bundles.Add(new ScriptBundle("~/bundles/RelacionPuertoStop").Include(
                "~/Scripts/RelacionPuertoStop.js"));

      bundles.Add(new ScriptBundle("~/bundles/PuertoStop").Include(
                "~/Scripts/PuertoStop.js"));

      bundles.Add(new ScriptBundle("~/bundles/GranoStop").Include(
                "~/Scripts/GranoStop.js"));

      bundles.Add(new ScriptBundle("~/bundles/SearchTable").Include(
                "~/Scripts/search-table.js"));

      bundles.Add(new ScriptBundle("~/bundles/AutorizacionCuposStop").Include(
                "~/Scripts/ControlaCargaConsignacion.js",
                "~/Scripts/CupoStop.js",
                "~/Scripts/AutorizacionCuposStop.js"));

      bundles.Add(new ScriptBundle("~/bundles/Alert").Include(
                "~/Scripts/Alert.js"));
      bundles.Add(new ScriptBundle("~/bundles/NotificacionCuposStopPendientes").Include(
                "~/Scripts/NotificacionCuposStopPendientes.js"));

      bundles.Add(new ScriptBundle("~/bundles/WordTag").Include(
                "~/Scripts/bootstrap-tokenfield.min.js"));

      bundles.Add(new StyleBundle("~/Content/landing").Include(
                "~/Content/landing-estilos.css"));

      bundles.Add(new ScriptBundle("~/bundles/landing").Include(
                "~/Scripts/landing-main.js"));

      bundles.Add(new ScriptBundle("~/bundles/motivoanulacion").Include(
          "~/Scripts/MotivoAnulacion.js"));

      bundles.Add(new StyleBundle("~/Content/highlightTextArea").Include(
                "~/Content/highlightTextArea.css"));

      bundles.Add(new ScriptBundle("~/bundles/highlightTextArea").Include(
                "~/Scripts/highlightTextArea.js"));

      bundles.Add(new ScriptBundle("~/bundles/Observable").Include(
                "~/Scripts/Observable.js"));
      bundles.Add(new ScriptBundle("~/bundles/ModalAuditoriaCambiosEstadoCupo").Include(
                "~/Scripts/ModalAuditoriaCambiosEstadoCupo.js"));
    }
  }
}
