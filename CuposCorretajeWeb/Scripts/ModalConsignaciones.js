function ModalConsignaciones() {
  this.$showModales = $("#idModalConsignaciones.modal[data-open='True']");
  this.$modales = $("#idModalConsignaciones.modal");
  this.$filas = $("#idModalConsignaciones.modal tbody tr");
  this.$btnAceptar = $("#btnAceptarModalConsignaciones");
  this.filaSeleccionada = null;
  this.consignacionSeleccionada = {};
  this.consignacion = {}
  this.main();
}

ModalConsignaciones.prototype.getConsignacionSeleccionada = function () {
  return this.consignacionSeleccionada;
}

ModalConsignaciones.prototype.setConsignacionSeleccionada = function (fila) {
  this.consignacionSeleccionada = modelData.consignaciones.find(function (consignacion) { return consignacion.Id == fila.dataset.consignacion })
}

ModalConsignaciones.prototype.main = function () {
  this.mostrarModalSiCumpleCondicion();
  this.seleccionarSiEsUnicaFila();
  this.checkRadioAlSeleccionarFila();
  this.toggleClassFilaAlSeleccionarFila();
}

ModalConsignaciones.prototype.mostrarModalSiCumpleCondicion = function () {
  this.$showModales.modal("show");
}

//Click fila consignaciones
ModalConsignaciones.prototype.filaClickEventListener = function (seleccionarConsignacion) {
  var _this = this;
  this.$modales.find("tbody tr").click(function () {
    _this.setConsignacionSeleccionada(this);
    seleccionarConsignacion(this);
  });
}

ModalConsignaciones.prototype.checkRadioAlSeleccionarFila = function () {
  this.$filas.click(function () {
    var $radio = $(this).find('input:radio');
    $radio.prop("checked", true);
  });
}

ModalConsignaciones.prototype.toggleClassFilaAlSeleccionarFila = function () {
  this.$modales.find('tr input:radio').change(function () {
    $(this).closest('tr').toggleClass('fila-seleccionada');
  });
}

ModalConsignaciones.prototype.btnAceptarClickEventListener = function (aceptarBtnModal, hideModalAtClick) {
  this.$btnAceptar.click(function () {
    if ($("#idModalConsignaciones").find("input:radio").is(":checked")) {
      if (hideModalAtClick == undefined || hideModalAtClick) $("#idModalConsignaciones").modal("hide");
      if (aceptarBtnModal != undefined) aceptarBtnModal();
    }
  });
}

ModalConsignaciones.prototype.seleccionarSiEsUnicaFila = function () {
  if (modelData.consignaciones.length == 1) {
    //var $radio = this.$filas.find('input:radio');
    //$radio.prop("checked", true);
    this.consignacionSeleccionada = modelData.consignaciones[0];
  }
}