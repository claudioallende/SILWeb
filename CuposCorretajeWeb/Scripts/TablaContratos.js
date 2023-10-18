var model = window.modelData;
var datos = {};

$("#btnAceptarModalConsignaciones").click(function () {
  if ($('input:radio[name="Consignacion"]').is(':checked')) {
    datos = {
      datosContrato: {
        Compcta: model.cuentaComprador,
        Vendcta: model.cuentaVendedor,
        //Ctadestino: "",
        Codproducto: model.codigoProducto,
        Codcentro: $("#CentroSeleccionado").val(),
        Cuitsolicitante: model.cupo.cuitsolicitante,
        Cuitintermediario: model.cupo.cuitintermediario,
        Cuitrteent: model.cupo.cuitrteent,
        Cuitcorrcomp: model.cupo.cuitcorrcomp,
        Cuitmat: model.cupo.cuitmat,
        Cuitcorrvta: model.cupo.cuitcorrvta,
        Cuitrtecomercial: model.cupo.cuitrtecomercial,
        Cuitdestinatario: model.cupo.cuitdestinatario,
        CuitRteComercialProductor: model.cupo.cuitrtecomercialproductor,
        CuitRteComercialVentaPrimaria: model.cupo.cuitrtecomercialventaprimaria,
        Caratula: model.cupo.caratula,
        ContactoComercial: model.cupo.contactocomercial,
      },
      CuentaPuerto: window.modelData.cuentaPuerto,
      fechaDesde: $("#EntregaDesde").val(),
      fechaHasta: $("#EntregaHasta").val(),
      cosechaDesde: $("#CosechaDesde").val(),
      cosechaHasta: $("#CosechaHasta").val(),
      ConsignacionSeleccionada: modalConsignaciones.getConsignacionSeleccionada(),
      Cyo: window.modelData.cyo !== "" ? "TRUE" : "FALSE"
    };

    spinnerBtnAceptarConsignacion.mostrarSpinner();

    $.ajax({
      type: "POST",
      url: window.modelData.actionGetTablaContratos,
      contentType: "application/json; charset=utf-8",
      dataType: "html",
      data: JSON.stringify(datos),
      success: function (data) {
        initTabla(data);
        handleDiaFocus();
        spinnerBtnAceptarConsignacion.ocultarSpinner();
        $("#idModalConsignaciones").modal("hide");
        if (cupo.contactocomercial) {
          getContactosComerciales()
        }
      },
      error: function (msg) {
      }
    });
  }
});

function initTabla(data) {
  llenarTablaDistribucion(data);
  controlEstados.handleEvents();
  controlEstados.setEstadoInicial(controlEstados.getEstado());
  handleBtnContrato();
  handleBtnPendienteAplicar();
}

function llenarTablaDistribucion(data) {
  $(".container_table_distribucion").html(data);
  ConvertirDataTable();
}

function resetTablaContratos() {
  $(".container_table_distribucion").html('<table id="' + model.IdTablaDistribucion + '" class="' + model.ClassTablaDistribucion + '" style="' + model.StylesTablaDistribucion + '"></table>');
}