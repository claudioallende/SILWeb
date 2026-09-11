var model = window.modelData;
var datos = {};

function construirDatosTablaContratos() {
  var cupoActual = model.cupo || {};

  return {
    datosContrato: {
      Compcta: model.cuentaComprador,
      Vendcta: model.cuentaVendedor,
      Codproducto: model.codigoProducto,
      Codcentro: $("#CentroSeleccionado").val(),
      Cuitsolicitante: cupoActual.cuitsolicitante,
      Cuitintermediario: cupoActual.cuitintermediario,
      Cuitrteent: cupoActual.cuitrteent,
      Cuitcorrcomp: cupoActual.cuitcorrcomp,
      Cuitmat: cupoActual.cuitmat,
      Cuitcorrvta: cupoActual.cuitcorrvta,
      Cuitrtecomercial: cupoActual.cuitrtecomercial,
      Cuitdestinatario: cupoActual.cuitdestinatario,
      CuitRteComercialProductor: cupoActual.cuitrtecomercialproductor,
      CuitRteComercialVentaPrimaria: cupoActual.cuitrtecomercialventaprimaria,
      Caratula: cupoActual.caratula,
      ContactoComercial: cupoActual.contactocomercial
    },
    CuentaPuerto: model.cuentaPuerto,
    fechaDesde: $("#EntregaDesde").val(),
    fechaHasta: $("#EntregaHasta").val(),
    cosechaDesde: $("#CosechaDesde").val(),
    cosechaHasta: $("#CosechaHasta").val(),
    ConsignacionSeleccionada: modalConsignaciones.getConsignacionSeleccionada(),
    Cyo: model.cyo !== "" ? "TRUE" : "FALSE"
  };
}

function mostrarEstadoTabla(mensaje, esError) {
  var $estado = $("#estadoCargaTabla");
  if ($estado.length === 0) {
    $estado = $('<div id="estadoCargaTabla" class="small mb-2" role="status"></div>');
    $(".container_table_distribucion").before($estado);
  }

  $estado
    .removeClass("text-muted text-danger")
    .addClass(esError ? "text-danger" : "text-muted")
    .text(mensaje)
    .removeClass("d-none");
}

function ocultarEstadoTabla() {
  $("#estadoCargaTabla").addClass("d-none");
}

function actualizarTablaContratos(opciones) {
  opciones = opciones || {};

  if (opciones.mostrarEstado) {
    mostrarEstadoTabla("Actualizando tabla de distribución...", false);
  }

  var request = $.ajax({
    type: "POST",
    url: window.modelData.actionGetTablaContratos,
    contentType: "application/json; charset=utf-8",
    dataType: "html",
    data: JSON.stringify(construirDatosTablaContratos())
  });

  request.done(function (data) {
    initTabla(data);
    handleDiaFocus();
  });

  return request;
}

$("#btnAceptarModalConsignaciones").click(function () {
  if ($('input:radio[name="Consignacion"]').is(':checked')) {
    datos = construirDatosTablaContratos();
    spinnerBtnAceptarConsignacion.mostrarSpinner();

    // Capturar el id de búsqueda pendiente ANTES del AJAX, porque el modal
    // puede haberse abierto disparado por el submit handler de Distribucion.cshtml
    // (gate por 2+ consignaciones) o por el flujo legacy (modal en cada carga).
    var pendingId = (typeof window._silMatchingPendingId !== 'undefined')
      ? window._silMatchingPendingId
      : null;

    actualizarTablaContratos()
      .done(function () {
        spinnerBtnAceptarConsignacion.ocultarSpinner();
        $("#idModalConsignaciones").modal("hide");
        if (cupo.contactocomercial) {
          getContactosComerciales();
        }

        // Si el submit handler de Distribucion.cshtml estaba esperando esta
        // selección para encadenar la búsqueda de matching, ejecutarla ahora.
        // (Seteado en Distribucion.cshtml líneas del gate por count > 1.)
        if (pendingId !== null &&
            typeof window.ejecutarBusquedaConMatching === 'function') {
          window._silMatchingPendingId = null;
          window.ejecutarBusquedaConMatching(pendingId);
        }
      })
      .fail(function () {
        spinnerBtnAceptarConsignacion.ocultarSpinner();
        // Si falló el AJAX, también limpiar el pending id para que no quede
        // un fantasma si el usuario vuelve a apretar Buscar.
        if (pendingId !== null) {
          window._silMatchingPendingId = null;
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
