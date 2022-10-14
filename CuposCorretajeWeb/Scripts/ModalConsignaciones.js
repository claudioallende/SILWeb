function ModalConsignaciones() {
  this.$showModales = $("#idModalConsignaciones.modal[data-open='True']");
  this.$modales = $("#idModalConsignaciones.modal");
  this.$filas = $("#idModalConsignaciones.modal tbody tr");
  this.$btnAceptar = $("#btnAceptarModalConsignaciones");
  this.filaSeleccionada = null;
  this.consignacionSeleccionada = {};
  this.main();
}

ModalConsignaciones.prototype.getConsignacionSeleccionada = function () {
  return this.consignacionSeleccionada;
}

ModalConsignaciones.prototype.setConsignacionSeleccionada = function (fila) {
  arrayNombresConsignacion = [].slice.call(fila.cells).slice(1, fila.cells.length);
  arrayCuitConsignacion = $(fila).find('input:radio').val().split('/');
  this.consignacionSeleccionada.Cuitsolicitante = arrayCuitConsignacion[0];
  this.consignacionSeleccionada.Nomsolicitante = arrayCuitConsignacion[1];
  this.consignacionSeleccionada.Cuitintermediario = arrayCuitConsignacion[2];
  this.consignacionSeleccionada.Nomintermediario = arrayCuitConsignacion[3];
  this.consignacionSeleccionada.Cuitrtecomercial = arrayCuitConsignacion[4];
  this.consignacionSeleccionada.Nomrtecomercial = arrayCuitConsignacion[5];
  this.consignacionSeleccionada.Cuitcorrcomp = arrayCuitConsignacion[6];
  this.consignacionSeleccionada.Nomcorrcomp = arrayCuitConsignacion[7];
  this.consignacionSeleccionada.Cuitmat = arrayCuitConsignacion[8];
  this.consignacionSeleccionada.Nommat = arrayCuitConsignacion[9];
  this.consignacionSeleccionada.Cuitcorrvta = arrayCuitConsignacion[10];
  this.consignacionSeleccionada.Nomcorrvta = arrayCuitConsignacion[11];
  this.consignacionSeleccionada.Cuitrteent = arrayCuitConsignacion[12];
  this.consignacionSeleccionada.Nomrteent = arrayCuitConsignacion[13];
  this.consignacionSeleccionada.Cuitdestinatario = arrayCuitConsignacion[14];
  this.consignacionSeleccionada.Nomdestinatario = arrayCuitConsignacion[15];
  this.consignacionSeleccionada.CuitRteComercialProductor = arrayCuitConsignacion[16];
  this.consignacionSeleccionada.NomRteComercialProductor = arrayCuitConsignacion[17];
  this.consignacionSeleccionada.CuitRteComercialVentaPrimaria = arrayCuitConsignacion[18];
  this.consignacionSeleccionada.NomRteComercialVentaPrimaria = arrayCuitConsignacion[19];
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
  if (this.$filas.length == 1) {
    var $radio = this.$filas.find('input:radio');
    $radio.prop("checked", true);
  }
}