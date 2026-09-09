var name,
  highlight,
  mousedownVendcyoCheck = false,
  valorCuit = "",
  MemoCuitCorredor = $('#Cuitcorrvta').val().replaceAll("-", "");

var ConsignacionInicial = {}

$('#Cuitsolicitante, #Cuitintermediario, #Cuitrtecomercial, #Cuitcorrcomp, #Cuitmat, #Cuitcorrvta, #Cuitrteent, #Cuitdestinatario, #CuitRteComercialProductor, #CuitRteComercialVentaPrimaria').blur(function (e) {
  name = $(this).attr("id");
  valorCuit = $('#' + name).val();
  if (valorCuit != undefined && valorCuit != "" && validaCuit(valorCuit)) {
    cuit = ($('#' + name).val());
    cuit = agregoGuionesIECompatibilidad(cuit);
    $("#" + name).val(cuit);
    var paramentro = ($('#' + name).val()).toString().replace(/[-_]/g, '');
    $.ajax({
      type: "POST",
      url: window.modelData.actionGetCuit + "/" + paramentro,
      contentType: "application/json; charset=utf-8",
      dataType: "json",
      success: function (data) {
        SuccessGetCuit(data);
        if (data && name == "Cuitcorrvta") {
          agregarContactoComercialSiCorredor(data[0].Cuit, data[0].Cuit + " - " + data[0].Nombre, document.getElementById("Vendcta").value, MemoCuitCorredor)
        }
      },
      error: function (msg) {
        alert(msg.responseText);
      }
    });
  } else {
  }
});

function SuccessGetCuit(response, nombre) {
  /*el contolador nombre se llama como el del cuil + "name" al final*/
  var cuit;
  if (nombre != undefined) name = nombre;
  var controlName = name + "Name"
  if (response != "") {
    cuit = ($('#' + name).val());
    if (cuit != null && cuit != "") {
      if ($("#" + controlName).val() == "") {
        $("#" + controlName).val((response[0].Nombre).toUpperCase());
      } else if ($("#" + controlName).val().toUpperCase() != (response[0].Nombre).toUpperCase()) {
        $("#" + controlName).val((response[0].Nombre).toUpperCase());
      }
    } else {
      $("#" + controlName).val("");
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

$(document).ready(function () {
  var model = { cuentas: undefined };

  $(".input-cuenta").autocomplete({
    source: function (request, response) {
      $.ajax({
        url: this.element.data("cuenta"),
        type: "POST",
        dataType: "json",
        data: { Texto: request.term },
        success: function (data) {
          response($.map(data, function (item) {
            return { label: item.Cuenta + " - " + item.Nombre, value: item.Cuenta };
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
      this.selected = true;
      handleChangeAutocompleteCuenta(this, ui.item);
    },
    change: function (event, ui) {
      if (!this.selected) {
        this.selected = true;
        handleChangeAutocompleteCuenta(this, this.value);
      }
    },
    search: function (event, ui) {
      this.selected = false;
    },
    close: function (event, ui) {
      changeWithoutSelect(this);
    }
  });

  function changeWithoutSelect(el) {
    if (!el.selected) {
      desbloquearCheckVendcyo();
      borrarNombre(el);
    }
  }

  function handleChangeAutocompleteCuenta(inputText, item) {
    desbloquearCheckVendcyo();
    //borrarNombre(inputText);
    if (item != null) {
      setearNombre(item.label, inputText);
      if (inputText.id == "Vendcta") {
        sugerirCuentaYOrden(item.value);
      }
    }
  }

  function getColumnaGrilla(el) {
    var classElementoSeleccionado = "";
    while (classElementoSeleccionado.indexOf("col-") == -1) {
      el = el.parentNode;
      classElementoSeleccionado = el.className;
    }
    return el;
  }

  function setearNombre(label, inputText) {
    nombre = label.substring(label.indexOf('-') + 2, label.length);
    $(inputText).next().next().find('.nombre-cuenta').html(nombre);
  }

  //Consignacion
  $(".nombre-cuenta").autocomplete({
    source: function (request, response) {
      $.ajax({
        url: window.modelData.actionGetCuposCuitFromNroCuentaOrNombre,
        type: "POST",
        dataType: "json",
        data: { Texto: request.term },
        success: function (data) {
          model.cuentas = data;
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
      var item_selected = model.cuentas.find(function (el) {
        if (el.Cuenta == nrocuenta) return el;
      });
      if (event.target.id == "CuitcorrvtaName") {
        agregarContactoComercialSiCorredor(item_selected.Cuit, ui.item.label, document.getElementById("Vendcta").value, $(this).closest(".form-row").find(".cuit-cuenta").val().replaceAll("-", ""))
      }
      $(this).closest(".form-row").find(".cuit-cuenta").val(agregoGuionesIECompatibilidad(item_selected.Cuit));
    },
    search: function (event, ui) {
    }
  });

  var spinnerBtnCrearCupo = new Spinner(document.getElementById("btnCrear"));

  $('#FormNuevo').submit(function () {
    if ($('#VendcyoBoolValue').prop("checked") && ($('#Vendcta').val().trim() == 0 || $('#Vendcta').val().trim() == "")) {
      addAlert("Si seleccionó Cuenta y Orden es necesario ingresar un vendedor", "alert-danger");
      return false;
    }
    if (!RteComercialIsValid()) {
      return false;
    }
    spinnerBtnCrearCupo.mostrarSpinner();
  })

  $("#Compcta").blur(function () {
    if (this.value.trim() == "") {
      borrarNombre(this);
    } else {
      ajaxNombreYCuenta(this.value, function (data) {
        if (data != undefined && data[0] != undefined) {
          $('#Cuitdestinatario').val(agregoGuionesIECompatibilidad(data[0].Cuit));
          SuccessGetCuit(data, "Cuitdestinatario");
        }
      });
    }
  });

  $('input:text').keypress(function (e) {
    focusNextInput(e, this, "input:text");
  });

  function focusNextInput(event, el, selector) {
    if (event.which == 13) { //Enter key
      event.preventDefault();
      var n = $(selector).length;
      var nextIndex = $(selector).index(el) + 1;
      if (nextIndex < n) {
        $(selector)[nextIndex].focus();
      }
    }
  }

  highlight = new HighLight();
  highlight.onInput = compararCodigosAlfanumericos;
  //highlight.bindEvents();
})

function borrarNombre(el) {
  $(el).next().next().find('.nombre-cuenta').html('');
}

//Alert
function addAlert(message, tipoAlerta) {
  var htmlAlert = '<div class="alert ' + tipoAlerta + ' alert-dismissible fade show" role="alert">';
  htmlAlert += message;
  htmlAlert += '<button type="button" class="close" data-dismiss="alert" aria-label="Close">';
  htmlAlert += '<span aria-hidden="true">&times;</span>';
  htmlAlert += '</button>';
  htmlAlert += '</div>';
  $('.alert').remove();
  $("#FormNuevo").before(htmlAlert);
}

function compararCodigosAlfanumericos(txtArea) {
  var regex = /\r\n|\n|\r/g,
    arrayCodigos = txtArea.value.split(regex);;
  arrayElementosRepetidos = arrayCodigos.filter(function (item, pos, self) {
    return self.indexOf(item) != pos;
  });
  result = regexCodigosAlfanumericosRepetidos(arrayElementosRepetidos);
  if (result !== false) {
    mostrarMensajeCodigosRepetidos(txtArea);
  } else {
    borrarMensajeCodigosRepetidos(txtArea);
  }
  return result;
}

function regexCodigosAlfanumericosRepetidos(codigos) {
  var regex = new RegExp("^(" + codigos.join("|") + ")$", 'gim');
  if (codigos.length == 0) {
    return false;
  } else {
    return regex;
  }
}

function mostrarMensajeCodigosRepetidos(textArea) {
  $(textArea).closest(".form-group").find(".aviso-repetido").css("display", "block");
}

function borrarMensajeCodigosRepetidos(textArea) {
  $(textArea).closest(".form-group").find(".aviso-repetido").css("display", "none");
}

function ajaxNombreYCuenta(nroCuenta, successFunction) {
  $.ajax({
    type: "POST",
    url: window.modelData.actionGetNombreAndCuit + "/" + nroCuenta,
    contentType: "application/json; charset=utf-8",
    dataType: "json",
    success: function (data) {
      successFunction(data);
    }
  });
}

function sugerirCuentaYOrden(vendedor) {
  //if (!mousedownVendcyoCheck) {
  $.ajax({
    type: "POST",
    url: window.modelData.actionGetCuentaYOrdenByNumeroCuenta,
    contentType: "application/json; charset=utf-8",
    dataType: "json",
    data: JSON.stringify({ NumeroCuenta: vendedor }),
    success: function (data) {
      if (data) {
        $("#VendcyoBoolValue").prop("checked", true);
        document.getElementById("VendcyoBoolValue").blocked = true;
        document.getElementById("VendcyoBoolValue").onclick = function () {
          return false;
        }
        //$("#VendcyoBoolValue").prop("disabled", true);
        agregarNombreRemitenteComercial(document.getElementById("Vendcta").value);
        //} else {
        //    $("#VendcyoBoolValue").prop("checked", false);
      }
    }
  });
  //}
}

//remitente comercial y cyo
$("#Vendcta").blur(function () {
  agregarNombreRemitenteComercial(this.value);
  var cuitCorredor = document.getElementById("Cuitcorrvta").value.replaceAll("-", "")
  var nombreCorredor = document.getElementById("CuitcorrvtaName").value
  agregarContactoComercialSiCorredor(cuitCorredor, cuitCorredor + " - " + nombreCorredor, this.value, cuitCorredor)
});

$("#VendcyoBoolValue").click(function () {
  if (!document.getElementById("VendcyoBoolValue").blocked) {
    if ($("#VendcyoBoolValue").is(':checked')) {
      agregarNombreRemitenteComercial(document.getElementById("Vendcta").value);
    } else {
      document.getElementById("Cuitrtecomercial").value = "";
      document.getElementById("CuitrtecomercialName").value = "";
    }
  }
});

function agregarNombreRemitenteComercial(nroCuenta) {
  if ($("#VendcyoBoolValue").val().trim() !== "" && $("#VendcyoBoolValue").is(':checked')) {
    ajaxNombreYCuenta(nroCuenta, function (data) {
      ConsignacionInicial.Cuitrtecomercial = agregoGuionesIECompatibilidad(data[0].Cuit);
      document.getElementById("Cuitrtecomercial").value = agregoGuionesIECompatibilidad(data[0].Cuit);
      SuccessGetCuit(data, "Cuitrtecomercial");
    });
  }
}

function agregarContactoComercialSiCorredor(cuitCorredor, nombreCorredor, cuentaVendedor, cuitCorredorAnterior) {
  if (cuitCorredor != "30500120882" && cuentaVendedor == "30525698412") {
    borrarContactoComercial(cuitCorredorAnterior)
    agregarContactoComercial({ label: nombreCorredor, value: cuitCorredor })
    MemoCuitCorredor = cuitCorredor
  }
}

function borrarContactoComercial(value) {
  var tokens = $('#ContactoComercial').tokenfield("getTokens")
  if (tokens) {
    var filtered = tokens.filter(function (item) {
      return item.value != value
    })
    $('#ContactoComercial').tokenfield('setTokens', filtered);
  }
}

function agregarContactoComercial(item) {
  var tokens = $('#ContactoComercial').tokenfield("getTokens");
  tokens.push(item)
  $('#ContactoComercial').tokenfield('setTokens', tokens);
}

$('#VendcyoGroup').mousedown(function () {
  mousedownVendcyoCheck = true;
});

$('#VendcyoGroup').mouseout(function () {
  mousedownVendcyoCheck = false;
});

function handleKeyUpAlfanumericos(elTextArea) {
  elTextArea.value = replaceSpaceWithBreakLine(elTextArea.value)
}

function handlePasteAlfanumericos(elTextArea) {
  elTextArea.value = replaceSpaceWithBreakLine(elTextArea.value)
}

function replaceSpaceWithBreakLine(text) {
  return text.replace(/\s/g, "\n");
}

//Saca el bloqueo a cyo si cambia
function desbloquearCheckVendcyo() {
  //if ($("#VendcyoBoolValue").text().trim() !== "") {
  //document.getElementById("VendcyoBoolValue").readonly = false;
  document.getElementById("VendcyoBoolValue").onclick = function () {
    //return false;
  }
  document.getElementById("VendcyoBoolValue").blocked = false;
  //$("#VendcyoBoolValue").prop("disabled", false);
  //}
}
////////////////////////////

//Controles
var habilitaCheckVendcyo = new ControlaInputs($("#Vendcta, #VendcyoBoolValue"));
habilitaCheckVendcyo.habilitarInputsSiIngresoDatos(function () {
  $("#VendcyoBoolValue").prop('checked', false);
});
var arrayDeshabilitaInputsDia = [];
for (var Dia = 0; Dia <= 20; Dia++) {
  var deshabilitaInputsDia = new ControlaInputs($("#CodigosDias_" + Dia + "__Alfanumerico, #CodigosDias_" + Dia + "__CantidadCupos"));
  deshabilitaInputsDia.deshabilitarInputsSiIngresoDatos();
  arrayDeshabilitaInputsDia.push(deshabilitaInputsDia);
}
var soloNumeros = new ControlaInputs($(".solo-numeros"));
soloNumeros.soloNumeros();
/////////////////////////////////

const mensagejeAlerta = new MensajeAlerta(document.getElementById("FormNuevo"))

function RteComercialIsValid() {
  if (document.getElementById("VendcyoBoolValue").checked) {
    return validateDisjunctionData(
      getDataValidate(
        ConsignacionInicial.Cuitrtecomercial,
        [
          document.getElementById("CuitRteComercialProductor"),
          document.getElementById("CuitRteComercialVentaPrimaria"),
          document.getElementById("Cuitintermediario"),
          document.getElementById("Cuitrtecomercial"),
          document.getElementById("Cuitmat")
        ]
      ),
      mensagejeAlerta
    );
  }
  return true;
}

$('#ContactoComercial').tokenfield({
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
  showAutocompleteOnFocus: false
}).on('tokenfield:createtoken', function (event) {
  var existingTokens = $(this).tokenfield('getTokens');
  $.each(existingTokens, function (index, token) {
    if (token.value === event.attrs.value) {
      event.preventDefault();
    }
  });
});

document.addEventListener("DOMContentLoaded", function () {
  debugger;
  const caratulaInput = document.getElementById("Caratula");
  const condicionGranoSelect = document.getElementById("CondicionGranoSeleccionado");
  const observacionesInput = document.getElementById("Observaciones");

  function limpiarLeyendas(texto) {
    if (!texto) return "";

    // Quita leyenda de carátula
    texto = texto.replace(/\n?Carátula matba rofex:.*$/gm, "");

    // Quita leyenda de condición grano
    texto = texto.replace(/\n?Condición Grano:.*$/gm, "");

    return texto.trim();
  }

  function actualizarObservaciones() {

    let textoBase = limpiarLeyendas(observacionesInput.value);

    let partes = [];

    if (caratulaInput.value && caratulaInput.value.length === 6) {
      partes.push("Carátula matba rofex: " + caratulaInput.value);
    }

    if (condicionGranoSelect.value) {
      let textoGrano = condicionGranoSelect.options[condicionGranoSelect.selectedIndex].text;
      partes.push("Condición Grano: " + textoGrano);
    }

    let textoFinal = textoBase;

    if (partes.length > 0) {
      if (textoBase)
        textoFinal += "\n" + partes.join("\n");
      else
        textoFinal = partes.join("\n");
    }

    observacionesInput.value = textoFinal;
  }

  // Eventos
  caratulaInput.addEventListener("change", actualizarObservaciones);
  condicionGranoSelect.addEventListener("change", actualizarObservaciones);

});

//document.getElementById("Caratula").addEventListener("change", function (ev) {
//  var observacion = document.getElementById("Observaciones").value;
//  if (document.getElementById("Caratula").value.length == 6) {
//    if (observacion) {
//      document.getElementById("Observaciones").value = observacion.concat(". Carátula matba rofex: ", ev.currentTarget.value)
//    } else {
//      document.getElementById("Observaciones").value = observacion.concat("Carátula matba rofex: ", ev.currentTarget.value)
//    }
//  }
//})

//document.getElementById("CondicionGranoSeleccionado").addEventListener("change", function (cg) {

//})