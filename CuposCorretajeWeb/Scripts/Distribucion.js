var cupo = {
  compcta: "",
  vendcta: "",
  puerto: "",
  grano: "",
  cuitsolicitante: "",
  nomsolicitante: "",
  cuitintermediario: "",
  nomintermediario: "",
  cuitrtecomercial: "",
  nomrtecomercial: "",
  cuitcorrcomp: "",
  nomcorrcomp: "",
  cuitmat: "",
  nommat: "",
  cuitcorrvta: "",
  nomcorrvta: "",
  cuitrteent: "",
  nomrteent: "",
  cuitdestinatario: "",
  nomdestinatario: "",
  cuitrtecomercialproductor: "",
  nomrtecomercialproductor: "",
  cuitrtecomercialventaprimaria: "",
  nomrtecomercialventaprimaria: "",
  observa: "",
  caratula: "",
  contactocomercial: "",
  condiciongrano: "",
}

window.modelData.cupo = cupo;
window.modelData.dias = $('.dia').toArray();

var consignacionSeleccionada = {
  clave: ""
}

var modalConsignaciones = new ModalConsignaciones();

$(document).ready(function () {

  modalConsignaciones.filaClickEventListener(seleccionarConsignacion);

  document.getElementById("btnInformaCupos").onclick = function () { distribuir(btnInformaCupos, getMotivo(), true); };

  //Obsolete, los totales están incluidos en la tabla que responde _DistribucionContratosPartial
  function modificarCantidadCuposPorDia() {
    $.ajax({
      url: window.modelData.actionCantidadCuposConsignacionSeleccionada,
      contentType: 'application/json; charset=utf-8',
      type: "POST",
      dataType: "json",
      data: JSON.stringify({
        consignacion: cupo
      }),
      success: function (data) {
        $("#totaldisponibledia1 .total").text(data.TotalDia1);
        $("#totaldisponibledia2 .total").text(data.TotalDia2);
        $("#totaldisponibledia3 .total").text(data.TotalDia3);
        $("#totaldisponibledia4 .total").text(data.TotalDia4);
        $("#totaldisponibledia5 .total").text(data.TotalDia5);
      },
      error: function (data) {

      }
    });
  }
  //////////////////////////////

  //Llenar datos al inicio
  try {
    seleccionarConsignacion($('#idModalConsignaciones').find('tbody tr:first-child')[0]);
  } catch (e) { }
  //Datepicker
  $("#EntregaDesde, #EntregaHasta").datepicker({
    dateFormat: "dd/mm/yy",
    dayNames: ["Domingo", "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado"],
    dayNamesMin: ["Dom", "Lun", "Mar", "Mie", "Jue", "Vie", "Sab"],
    monthNames: ["Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"]
  });

  //Buscador

  $("#Vendedor").on("keyup", function () {
    var value = $(this).val().toLowerCase();
    filtroTablaContratos(function (fila) {
      return $(fila).find('.vendedor').text().toLowerCase().indexOf(value) > -1;
    });
  });

  //Control tabla distribucion

  //Agrega a una matriz los divs editables para recorrerlos con Tab
  function agregarNodesAlArray($nodes) {
    window.modelData.dias.push($nodes.toArray());
  }

  $(".dia").keydown(function (e) {
    // Allow: backspace, delete and escape
    _this = this;
    if ($.inArray(e.keyCode, [46, 8, 27]) !== -1 ||
      // Allow: home, end, left, right, down, up
      (e.keyCode >= 35 && e.keyCode <= 40)) {
      return;
    }
    //Allow: Enter and Tab
    if (e.keyCode === 13 || (e.keyCode === 9 && !e.shiftKey)) {
      var index = window.modelData.dias.findIndex(function (element) {
        return element == _this;
      });
      if (window.modelData.dias[index + 1] != undefined) {
        $(window.modelData.dias[index + 1]).focus();
      }
      tabulacion.posicionarEnPantalla(document.activeElement);
    }
    //Allow: Shift + Tab
    if (e.shiftKey && e.keyCode == 9) {
      var index = window.modelData.dias.findIndex(function (element) {
        return element == _this;
      });
      if (window.modelData.dias[index - 1] != undefined) {
        $(window.modelData.dias[index - 1]).focus();
      }
      tabulacion.posicionarEnPantalla(document.activeElement);
    }
    // Ensure that it is a number and stop the keypress
    if ((e.keyCode < 48 || e.keyCode > 57) && (e.keyCode < 96 || e.keyCode > 105)) {
      e.preventDefault();
    }
  });

  //---------------------------------------

  //Setear cosecha hasta con el valor de cosecha desde al perder el foco
  document.getElementById("CosechaDesde").onblur = function () {
    setCosechaHasta();
  }

  function setCosechaHasta() {
    var valorCosechaDesde = document.getElementById("CosechaDesde").value;
    document.getElementById("CosechaHasta").value = valorCosechaDesde;
  }

});

function filtroTablaContratos(fnCondicion) {
  $("tr.grupo-contrato").filter(function () {
    $(this).toggle(fnCondicion(this));
  });
  $(".btn-group-toggle").addClass("button-checked");
  datatable.draw();
}

function borrarFiltroTablaContratos() {
  $("#TablaDistribuciones tbody tr").css('display', 'table-row');
  $(".btn-group-toggle").removeClass("button-checked");
  datatable.draw();
}

//Alert
function addAlert(message, tipoAlerta, beforeElement) {
  var htmlAlert = '<div class="alert ' + tipoAlerta + ' alert-dismissible fade show" role="alert" style="margin-top:5px;">';
  htmlAlert += message;
  htmlAlert += '<button type="button" class="close" data-dismiss="alert" aria-label="Close">';
  htmlAlert += '<span aria-hidden="true">&times;</span>';
  htmlAlert += '</button>';
  htmlAlert += '</div>';
  if (beforeElement == undefined) {
    $beforeElement = $("#FormFiltro");
  } else {
    $beforeElement = $(beforeElement);
  }
  $beforeElement.prev('.alert').remove();
  $beforeElement.before(htmlAlert);
}

//Comprobaciones
function camposFiltroNoVacios() {
  var result;
  result = comprador.trim() != "";
  result &= vendedor.trim() != "";
  result &= puerto.trim() != "";
  result &= grano.trim() != "";
  result &= $('#CosechaDesde').val().trim() != "" && $('#CosechaDesde').val().trim() != "0";
  result &= $('#CosechaHasta').val().trim() != "" && $('#CosechaHasta').val().trim() != "0";
  result &= $('#EntregaDesde').val().trim() != "";
  return result == 1 ? true : false;
}

function camposNoVacios() {
  var result = camposFiltroNoVacios();
  result &= $('#ConsignacionSeleccionada').val().trim() != "";
  return result == 1 ? true : false;
}

//Llenar texts datos consignacion
function llenarDatosConsignacion() {
  $('#Cuitsolicitante').val(cupo.cuitsolicitante?.trim());
  $('#CuitsolicitanteName').val(cupo.nomsolicitante?.trim());
  $('#Cuitintermediario').val(cupo.cuitintermediario?.trim());
  $('#CuitintermediarioName').val(cupo.nomintermediario?.trim());
  $('#Cuitrtecomercial').val(cupo.cuitrtecomercial?.trim());
  $('#CuitrtecomercialName').val(cupo.nomrtecomercial?.trim());
  $('#Cuitcorrcomp').val(cupo.cuitcorrcomp?.trim());
  $('#CuitcorrcompName').val(cupo.nomcorrcomp?.trim());
  $('#Cuitmat').val(cupo.cuitmat?.trim());
  $('#CuitmatName').val(cupo.nommat?.trim());
  $('#Cuitcorrvta').val(cupo.cuitcorrvta?.trim());
  $('#CuitcorrvtaName').val(cupo.nomcorrvta?.trim());
  $('#Cuitrteent').val(cupo.cuitrteent?.trim());
  $('#CuitrteentName').val(cupo.nomrteent?.trim());
  $('#Cuitdestinatario').val(cupo.cuitdestinatario?.trim());
  $('#CuitdestinatarioName').val(cupo.nomdestinatario?.trim());
  $('#CuitRteComercialProductor').val(cupo.cuitrtecomercialproductor?.trim());
  $('#CuitRteComercialProductorName').val(cupo.nomrtecomercialproductor?.trim());
  $('#CuitRteComercialVentaPrimaria').val(cupo.cuitrtecomercialventaprimaria?.trim());
  $('#CuitRteComercialVentaPrimariaName').val(cupo.nomrtecomercialventaprimaria?.trim());
  $('#ConsignacionSeleccionada_Caratula').val(cupo.caratula?.trim());
  $('#Observaciones').val(cupo.observa?.trim());
  if (cupo.contactocomercial) {
    contactosComerciales.tokenfield('setTokens', cupo.contactocomercial.split(";").map(function (cc) {
      return { value: cc, label: cc }
    }))
  }
  $('#ConsignacionSeleccionada_CondicionGrano').val(cupo.condiciongrano?.trim());
}

function getContactosComerciales() {
  $.ajax({
    url: window.modelData.actionGetCuentas,
    contentType: 'application/json; charset=utf-8',
    type: "POST",
    dataType: "json",
    data: JSON.stringify(cupo.contactocomercial.split(";")),
    success: function (data) {
      contactosComerciales.tokenfield('setTokens', data.data.map(function (cc) {
        return { value: cc.Cuenta, label: cc.Cuenta + " - " + cc.Nombre }
      }))
    },
    error: function (data) {

    }
  });
}

function cambioEnObservaciones() {
  return document.getElementById("Observaciones").value !== cupo.observa;
}

function seleccionarConsignacion(el) {
  arrayNombresConsignacion = [].slice.call(el.cells).slice(1, el.cells.length);
  arrayCuitConsignacion = $(el).find('input:radio').val().split('/');
  consignacionSeleccionada.clave = $(el).find('input:radio').val();
  var consignacion = modalConsignaciones.getConsignacionSeleccionada()
  cupo.cuitsolicitante = consignacion.Cuitsolicitante;
  cupo.nomsolicitante = consignacion.Nomsolicitante;
  cupo.cuitintermediario = consignacion.Cuitintermediario;
  cupo.nomintermediario = consignacion.Nomintermediario;
  cupo.cuitrtecomercial = consignacion.Cuitrtecomercial;
  cupo.nomrtecomercial = consignacion.Nomrtecomercial;
  cupo.cuitcorrcomp = consignacion.Cuitcorrcomp;
  cupo.nomcorrcomp = consignacion.Nomcorrcomp;
  cupo.cuitmat = consignacion.Cuitmat;
  cupo.nommat = consignacion.Nommat;
  cupo.cuitcorrvta = consignacion.Cuitcorrvta;
  cupo.nomcorrvta = consignacion.Nomcorrvta;
  cupo.cuitrteent = consignacion.Cuitrteent;
  cupo.nomrteent = consignacion.Nomrteent;
  cupo.cuitdestinatario = consignacion.Cuitdestinatario;
  cupo.nomdestinatario = consignacion.Nomdestinatario;
  cupo.cuitrtecomercialproductor = consignacion.CuitRteComercialProductor;
  cupo.nomrtecomercialproductor = consignacion.NomRteComercialProductor;
  cupo.cuitrtecomercialventaprimaria = consignacion.CuitRteComercialVentaPrimaria;
  cupo.nomrtecomercialventaprimaria = consignacion.NomRteComercialVentaPrimaria;
  cupo.caratula = consignacion.Caratula;
  cupo.contactocomercial = consignacion.ContactoComercial;
  cupo.observa = $(el).data("observacion");
  cupo.condiciongrano = consignacion.CondicionGrano;
  llenarDatosConsignacion();
}

function SuccessGetCuit(idCuit, idNombre, response) {
  /*el contolador nombre se llama como el del cuil + "name" al final*/
  var cuit;
  if (response != "") {
    cuit = ($('#' + idCuit).val());
    if (cuit != null && cuit != "") {
      if ($("#" + idNombre).val() == "") {
        $("#" + idNombre).val((response).toUpperCase());
      } else if ($("#" + idNombre).val().toUpperCase() != (response).toUpperCase()) {
        $("#" + idNombre).val((response).toUpperCase());
      }
    } else {
      $("#" + idNombre).val("");
    }
  } else {
  }
}

function agregoGuionesIECompatibilidad(cuit) {
  if (cuit != null && cuit != "") {
    if (cuit.charAt(2) != "-" && cuit.charAt(10) != "-") /*no tiene los guiones*/ {
      cuit = cuit.substring(0, 2) + "-" + cuit.substring(2, 10) + "-" + cuit.substring(10, cuit.length);
    } else {
      if (cuit.charAt(2) != "-") {
        cuit = cuit.substring(0, 2) + "-" + cuit.substring(2, 10) + cuit.substring(10, cuit.length);
      }
      if (cuit.charAt(11) != "-") {
        cuit = cuit.substring(0, 2) + cuit.substring(2, 11) + "-" + cuit.substring(11, cuit.length);
      }
    }
  }
  return cuit;
}

//Llenar tabla contratos
function llenarTablaContratos(idTabla, datos) {
  $tabla = $('#' + idTabla);
  $body = $tabla.find('tbody');
  filas = "";
  if ($body == undefined) $body = $tabla;
  datos.forEach(function (el) {
    filas += obtenerFila(el);
  });
  $body.html(filas);
}

function obtenerFila(fila) {
  sFila = "<tr>";
  sFila += "<td>" + fila.contrato + "</td>";
  sFila += "<td>" + fila.neg + "</td>";
  sFila += "<td>" + fila.oper + "</td>";
  sFila += "<td>" + fila.prod + "</td>";
  sFila += "<td>" + fila.cos + "</td>";
  sFila += "<td>" + fila.entrega + "</td>";
  sFila += "<td>" + fila.vtoEnt + "</td>";
  sFila += "<td class='text-right'>" + fila.pactadas + "</td>";
  sFila += "<td class='text-right'>" + fila.apli + "</td>";
  sFila += "<td class='text-right'>" + fila.pendEnt + "</td>";
  sFila += "<td class='text-right'>" + fila.liq + "</td>";
  sFila += "<td class='text-right'>" + fila.precio + "</td>";
  sFila += "<td>" + fila.destino + "</td>";
  sFila += "</tr>";
  return sFila;
}

//Form submit
$("#FormFiltro").submit(function () {
  spinnerBtnBuscar.mostrarSpinner();
});

$(".dia").focus(function () {
  selectElementContents(this);
});

function handleDiaFocus() {
  $(".dia").focus(function () {
    selectElementContents(this);
  });
}

function selectElementContents(el) {
  var range = document.createRange();
  range.selectNodeContents(el);
  var sel = window.getSelection();
  sel.removeAllRanges();
  sel.addRange(range);
}

function handleChangeFiltroCantidadCupoSuperaDisponible(check) {
  var _this = check;
  if (_this.checked) {
    var vendedoresNoValidados = controlEstados.getVendedoresNoValidados();
    filtroTablaContratos(function (fila) {
      return vendedoresNoValidados.includes(fila);
    });
  } else {
    borrarFiltroTablaContratos();
  }
}

var contactosComerciales = $('#ConsignacionSeleccionada_ContactoComercial').tokenfield({
  delimiter: [";"],
  autocomplete: {
    source: function (request, response) {
      if (request.term !== undefined && request.term !== "") {
        $.ajax({
          url: modelData.actionGetContactoComercial,// + "?q=" + request.term,
          type: "POST",
          dataType: "json",
          data: { Texto: request.term },
          success: function (data) {
            response($.map(data, function (item) {
              return {
                label: item.Cuenta + " - " + item.Nombre,
                value: item.Cuenta
              };
            }))
          },
          select: function (event, ui) {
            ui.item.classList.add("success");
          }
        })
      }
    },
    delay: 300
  },
  showAutocompleteOnFocus: false,
  minWidth: 200
});