$(document).ready(function () {
  pathURL = window.location.pathname;
  arrayURL = pathURL.split("/")
  id = arrayURL[arrayURL.length - 1];
  arrayId = id.split("-");
  try {
    cupo.compcta = arrayId[0];
    cupo.vendcta = arrayId[1];
    cupo.puerto = arrayId[2];
    cupo.grano = arrayId[3];
  } catch (e) { }

  //Setear nombre de cuenta asociada al cuit ingresado
  $('.cuit-cuenta').blur(function (e) {
    name = $(this).attr("id");
    if (validaCuit($('#' + name).val()) && $('#' + name).val() !== "") {
      //var param = "{ 'cuit' : '" + ($('#' + name).val()).toString().replace(/[-_]/g, "") + "'}";
      var paramentro = ($('#' + name).val()).toString().replace(/[-_]/g, ''),
        _this = this;
      cuit = ($(this).val());
      cuit = agregoGuionesIECompatibilidad(cuit);
      $(this).val(cuit);
      $.ajax({
        type: "POST",
        url: window.modelData.actionGetCuit + "/" + paramentro,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
          SuccessGetCuit(_this.id, $(_this).closest('.row').find('.nombre-cuenta').attr('id'), data);
        },
        error: function (msg) {
          //alert(msg.responseText);
        }
      });
    }
  });

  handleBtnContrato();
});

//Click boton contrato
function handleBtnContrato() {
  $('.btn-contrato').click(function () {
    mostrarContratos(this);
  });
}

//Click boton detalle pendiente de aplicar
function handleBtnPendienteAplicar() {
  $(".btn-detalle_pend_aplicar").click(function () {
    if (!$(this).hasClass("disabled")) {
      GetDetallePendienteAplicar(this)
    }
  });
}

function mostrarContratos(el) {
  fila = $(el).closest('tr');
  $.ajax({
    type: "POST",
    url: window.modelData.actionContratos,
    contentType: "application/json; charset=utf-8",
    dataType: "json",
    data: JSON.stringify({
      compcta: cupo.compcta,
      vendcta: $(fila).data('vendedor'),
      ctadestino: $(fila).data('destino'),
      codcentro: $('#CentroSeleccionado').val(),
      grano: cupo.grano,
      fechaent: $('#EntregaHasta').val()
    }),
    success: function (data) {
      llenarTablaContratos("TablaContratos", data);
    },
    error: function (msg) {
      //alert('Error');
    }
  });
}


function distribuir(spinner, textMotivo, confimacionDistribucion) {
  var arrayobj = [];

  spinner.mostrarSpinner();

  controlEstados.getObjetoDistribucion().filter(function (el) {
    //var tieneNuevaAsigancion = false;
    for (var i = 0; i <= modelData.cantidaddias; i++) {
      if (el["Dia" + i] != 0) return true;
    }
    return false;//(el.Dia0 != 0 || el.Dia1 != 0 || el.Dia2 != 0 || el.Dia3 != 0 || el.Dia4 != 0 || el.Dia5 != 0)
  }).forEach(function (element, index) {
    obj = {
      Compcta: cupo.compcta,
      Vendcta: element.Vendcta,
      Codproducto: element.Codproducto,
      Hoy: element.Dia0,
      Dia1: element.Dia1,
      Dia2: element.Dia2,
      Dia3: element.Dia3,
      Dia4: element.Dia4,
      Dia5: element.Dia5,
      Dia6: element.Dia6,
      Dia7: element.Dia7,
      Dia8: element.Dia8,
      Dia9: element.Dia9,
      Dia10: element.Dia10,
      Dia11: element.Dia11,
      Dia12: element.Dia12,
      Dia13: element.Dia13,
      Dia14: element.Dia14,
      Dia15: element.Dia15,
      Dia16: element.Dia16,
      Dia17: element.Dia17,
      Dia18: element.Dia18,
      Dia19: element.Dia19,
      Dia20: element.Dia20,
      Cosecha: element.Cosecha,
      Ctadestino: element.Ctadestino,
      Centro: modelData.centrocontratos,
      Fechaent: element.Fechaent
    }
    arrayobj.push(obj);
  });

  valido = validaCuit($('#Cuitsolicitante').val()) && validaCuit($('#Cuitintermediario').val()) && validaCuit($('#Cuitrtecomercial').val());
  valido &= validaCuit($('#Cuitcorrcomp').val()) && validaCuit($('#Cuitmat').val()) && validaCuit($('#Cuitcorrvta').val());
  valido &= validaCuit($('#Cuitrteent').val()) && validaCuit($('#Cuitdestinatario').val()) && validaCuit($('#CuitRteComercialProductor').val()) && validaCuit($('#CuitRteComercialVentaPrimaria').val());
  if (valido) {
    $.ajax({
      url: window.modelData.actionActualizarDistribucion,// + "/" + window.modelData.ModelId,
      contentType: 'application/json; charset=utf-8',
      type: "POST",
      dataType: "json",
      data: JSON.stringify({
        model: {
          'cupos': arrayobj,
          anterior: cupo,
          nuevo: {
            Cuitsolicitante: $('#Cuitsolicitante').val(),
            Nomsolicitante: $('#CuitsolicitanteName').val(),
            Cuitintermediario: $('#Cuitintermediario').val(),
            Nomintermediario: $('#CuitintermediarioName').val(),
            Cuitrtecomercial: $('#Cuitrtecomercial').val(),
            Nomrtecomercial: $('#CuitrtecomercialName').val(),
            Cuitcorrcomp: $('#Cuitcorrcomp').val(),
            Nomcorrcomp: $('#CuitcorrcompName').val(),
            Cuitmat: $('#Cuitmat').val(),
            Nommat: $('#CuitmatName').val(),
            Cuitcorrvta: $('#Cuitcorrvta').val(),
            Nomcorrvta: $('#CuitcorrvtaName').val(),
            Cuitrteent: $('#Cuitrteent').val(),
            Nomrteent: $('#CuitrteentName').val(),
            Cuitdestinatario: $('#Cuitdestinatario').val(),
            Nomdestinatario: $('#CuitdestinatarioName').val(),
            CuitRteComercialProductor: $('#CuitRteComercialProductor').val(),
            NomRteComercialProductor: $('#CuitRteComercialProductorName').val(),
            CuitRteComercialVentaPrimaria: $('#CuitRteComercialVentaPrimaria').val(),
            NomRteComercialVentaPrimaria: $('#CuitRteComercialVentaPrimariaName').val(),
            Cuitdestinatario: $('#Cuitdestinatario').val(),
            Nomdestinatario: $('#CuitdestinatarioName').val(),
            Motbaja: textMotivo,
            Observa: document.getElementById("Observaciones").value
          },
          puerto: cupo.puerto,
          cosechaDesde: $('#CosechaDesde').val(),
          cosechaHasta: $('#CosechaHasta').val(),
          fechaDesde: $('#EntregaDesde').val() == "" ? null : $('#EntregaDesde').val(),
          fecha: $('#EntregaHasta').val(),
          ConsignacionSeleccionada: $('input[name="Consignacion"]:checked').val(),
          tieneVendedor: cupo.vendcta != 0,
          CentroSeleccionado: modelData.centrocontratos
          //DistribucionCupos: controlEstados.getObjetoDistribucion()
        },
        Confirmacion: (confimacionDistribucion == null || confimacionDistribucion == undefined ? false : confimacionDistribucion)
      }),
      traditional: true,
      success: function (data) {
        successDistribucion(data, spinner);
      },
      error: function (jqXHR, textStatus, errorThrown) {
        spinner.ocultarSpinner();
        if (jqXHR.responseJSON != undefined && jqXHR.responseJSON != "") {
          jqXHR.responseJSON.forEach(function (el) {
            addErrorValidationMessage(el.key, el.errors[0]);
            onAlert();
          });
        } else {
          addAlert('Se produjo un error durante el proceso', "alert-danger");
          onAlert();
        }
      }
    });
  } else {
    addAlert("CUIT con formato inválido", "alert-danger");
    onAlert();
  }
  arrayobj = [];
}

function successDistribucion(data, spinner) {
  spinner.ocultarSpinner();
  if (data) {
    if (data == 1) {
      window.location = window.modelData.actionDetalle + "/" + window.modelData.cuentaComprador + "-" + window.modelData.cuentaPuerto + "-" + window.modelData.codigoProducto + window.modelData.filtro;
    } else if (data == 100) {
      addAlert('Cantidad de cupos excedidos', "alert-danger");
      onAlert();
    } else if (data == 200) {
      addAlert('Cantidad de cupos excedidos para la consignación seleccionada', "alert-danger");
      onAlert();
    } else if (data == 300) {
      addAlert('No hubo cambios', "alert-info");
      onAlert();
    } else if (data.Status == 'Error') {
      $('#modalMotivo').modal('hide');
      addAlert(data.Mensaje, "alert-danger");
      onAlert();
    } else if (data.Status == 'ErrorCupos') {
      $('#modalMotivo').modal('hide');
      informarCuposNoAnulables(data.Mensaje);
      $('#modal-informar-cupos').modal('show');
    }
  }
}

function informarCuposNoAnulables(strCupos) {
  var lista = document.getElementById("lista-cupos-informar");
  var mensajeCupos = 'Cupos no anulables: ';
  var mensajeCantidad = 'Cantidad ingresada: ';
  var alfanumericos = strCupos.substring(strCupos.indexOf(mensajeCupos) + mensajeCupos.length, strCupos.indexOf(';')).split(',');
  var cantidadCuposIngresada = parseInt(strCupos.substring(strCupos.indexOf(mensajeCantidad) + mensajeCantidad.length));
  var listaAlfanumericosHtml = "";
  for (var i = 0; i < alfanumericos.length; i++) {
    listaAlfanumericosHtml += '<li class="list-group-item">' + alfanumericos[i] + '</li>';
  }
  document.getElementById
  lista.innerHTML = listaAlfanumericosHtml;
  evaluarYMostrarMensajeAnularCuposNoBloqueados(cantidadCuposIngresada, alfanumericos.length)
}

function evaluarYMostrarMensajeAnularCuposNoBloqueados(cantidadCuposIngresada, cantidadCuposBloqueados) {
  if (cantidadCuposIngresada == cantidadCuposBloqueados) {
    document.getElementById("mensajeAceptarInformaCupos").style.display = "none";
    document.getElementById("btnInformaCupos").disabled = true;
  } else {
    document.getElementById("mensajeAceptarInformaCupos").style.display = "visible";
    document.getElementById("btnInformaCupos").disabled = false;
    document.getElementById("idCantidadCuposNoBloqueados").innerHTML = cantidadCuposIngresada - cantidadCuposBloqueados;
  }
}

function onAlert() {
  $(document).scrollTop(0);
}

$(".nombre-cuenta").autocomplete({
  source: function (request, response) {
    $.ajax({
      url: window.modelData.actionGetCuposCuitFromNroCuentaOrNombre,
      type: "POST",
      dataType: "json",
      data: { Texto: request.term },
      success: function (data) {
        modelData.cuentas = data;
        response($.map(data, function (item) {
          return { label: item.Cuenta + " - " + item.Nombre, value: item.Nombre };
        }))
      }
    })
  },
  autoFocus: true,
  messages: {
    noResults: "",
    results: function (resultsCount) { }
  },
  select: function (event, ui) {
    var nrocuenta = ui.item.label.substring(0, ui.item.label.indexOf(' -'));
    var item_selected = modelData.cuentas.find(function (el) {
      if (el.Cuenta == nrocuenta) return el;
    });
    $(this).closest(".form-row").find(".cuit-cuenta").val(agregoGuionesIECompatibilidad(item_selected.Cuit));
    ///////////////////////////
    //var nrocuit = ui.item.label.substring(0, ui.item.label.indexOf(' -'));
    //$(this).closest(".row").find(".numero-cuit").val(agregoGuionesIECompatibilidad(nrocuit));
  },
  search: function (event, ui) {
  }
});

handleBtnPendienteAplicar();

//Detalle Contrato pendiente de aplicar
function GetDetallePendienteAplicar(boton) {
  var $fila = $(boton).closest("tr");
  $.ajax({
    url: window.modelData.actionGetDetallePendienteAplicar,
    type: "POST",
    dataType: "html",
    data: {
      Compcta: cupo.compcta,
      Vendcta: $fila.data("vendedor"),
      Producto: cupo.grano,
      Ctadestino: $fila.data("destino"),
      Cosecha: $fila.data("cosecha"),
      Codcentro: $("#CentroSeleccionado").val()
    },
    success: function (data) {
      $('#modalDetallePendienteAplicar .modal-body').html(data);
      $("#modalDetallePendienteAplicar").modal("show");
    },
    error: function (data) {
      //alert(data);
    }
  })
}