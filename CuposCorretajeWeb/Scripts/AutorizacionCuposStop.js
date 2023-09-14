document.getElementById("VendcyoBoolValue").addEventListener("change", function (event) {
  if (event.target.checked) {

  }
});

document.getElementById("formModalAlfas").addEventListener("submit", handleSubmitModalAlfas);

var btnsSeleccionAlfas = document.getElementsByClassName("btn-seleccionarAlfas")
for (var i = 0; i < btnsSeleccionAlfas.length; i++) {
  //Si tiene alfas abre el modal
  if (document.getElementById("codigos-dia-" + btnsSeleccionAlfas[i].dataset.nrodia).querySelectorAll(".list-group-item").length > 0) {
    btnsSeleccionAlfas[i].addEventListener("click", handleClickBtnSeleccionar);
  }
}

function handleCheckPorDia(check) {
  if (check.checked == true) {
    checkAllDia(check.dataset.lista.substr(check.dataset.lista.length - 1, 1));
  } else {
    uncheckAllDia(check.dataset.lista.substr(check.dataset.lista.length - 1, 1))
  }
}

function seleccionarLista(lista) {
  items = lista.querySelectorAll(".list-group-item");
  items.forEach(function (item) {
    cambiarEstado(item.id, anular);
  });
}

function deseleccionarLista(lista) {
  items = lista.querySelectorAll(".list-group-item");
  items.forEach(function (item) {
    cambiarEstado(item.id, habilitar);
  });
}

function cambiarEstado(id, callback) {
  var estabaVacio = false;
  if (idsEditar.trim() === "") estabaVacio = true;
  callback(id);
  if ((estabaVacio && idsEditar.trim() != "") || (!estabaVacio && idsEditar.trim() === "")) {
    cambiarEstadoBtnCancelar();
    cambiarEstadoBtnAceptar();
  }
}

function getNuevo() {
  var e = document.getElementById("Producto");
  var productoSeleccionado = e.options[e.selectedIndex].value;
  var nuevoCupo = {
    Producto: Number(productoSeleccionado),
    Compcta: getValue("Compcta"),
    Vendcta: getValue("Vendcta"),
    Puerto: getValue("Puerto"),
    Cuitsolicitante: getValue("Cuitsolicitante"),
    Nomsolicitante: getValue("CuitsolicitanteName"),
    Cuitintermediario: getValue("Cuitintermediario"),
    Nomintermediario: getValue("CuitintermediarioName"),
    Cuitrtecomercial: getValue("Cuitrtecomercial"),
    Nomrtecomercial: getValue("CuitrtecomercialName"),
    Cuitcorrcomp: getValue("Cuitcorrcomp"),
    Nomcorrcomp: getValue("CuitcorrcompName"),
    Cuitmat: getValue("Cuitmat"),
    Nommat: getValue("CuitmatName"),
    Cuitcorrvta: getValue("Cuitcorrvta"),
    Nomcorrvta: getValue("CuitcorrvtaName"),
    Cuitrteent: getValue("Cuitrteent"),
    Nomrteent: getValue("CuitrteentName"),
    Cuitdestinatario: getValue("Cuitdestinatario"),
    Nomdestinatario: getValue("CuitdestinatarioName"),
    CuitRteComercialProductor: getValue("CuitRteComercialProductor"),
    NomRteComercialProductor: getValue("CuitRteComercialProductorName"),
    CuitRteComercialVentaPrimaria: getValue("CuitRteComercialVentaPrimaria"),
    NomRteComercialVentaPrimaria: getValue("CuitRteComercialVentaPrimariaName"),
    CodigosDias: getCodigosDias(),
    Observaciones: getValue("Observaciones"),
    Centro: getValue("Centro"),
    CentroAnterior: cupoOriginal.Centro,
    Caratula: getValue("Caratula"),
    ContactoComercial: tfContactoComercial.tokenfield('getTokensList', ';')
  }

  return nuevoCupo;
}

function getValue(idNode) {
  return document.getElementById(idNode) == null ? "" : document.getElementById(idNode).value;
}

function getText(idNode) {
  return document.getElementById(idNode) == null ? "" : document.getElementById(idNode).innerText;
}

function getCodigosDias() {
  var codigos = [];
  var codigoDia = {};
  for (var dia = 0; dia <= 20; dia++) {
    codigoDia = getCodigosDia(dia);
    if (codigoDia.Turnos != undefined && codigoDia.Turnos != null && codigoDia.Turnos.length > 0)
      codigos.push(codigoDia);
  }
  return codigos;
}

function getCodigosDia(ndia) {
  var listaHTML = document.getElementById("codigos-dia-" + ndia);
  var dia = listaHTML.querySelectorAll("#CodigosDias_" + ndia + "__Fecha")[0].value;
  var alfasDia = [];

  listaHTML.querySelectorAll(".list-group-item.active").forEach(function (codigo) {
    alfasDia.push(codigo.innerText);
  });

  return { Fecha: dia, Turnos: alfasDia };
}

function checkAllDia(ndia) {
  operarSobreTodosCodigosDia(ndia, function (codigo) {
    codigo.classList.add("active");
  });
}

function uncheckAllDia(ndia) {
  operarSobreTodosCodigosDia(ndia, function (codigo) {
    codigo.classList.remove("active");
  });
}

function operarSobreTodosCodigosDia(ndia, callback) {
  var listaHTML = document.getElementById("codigos-dia-" + ndia);

  listaHTML.querySelectorAll(".list-group-item").forEach(function (codigo) {
    callback(codigo);
  });
}

function autorizar(cuposAutorizar) {
  if (validaSeleccionAlfanumerico() && validaCuitsConsignaciones() && tipoCupos.isValid()) {
    $.ajax({
      type: "POST",
      url: getActionAutorizar(),//window.modelData.actionAutorizar,
      contentType: "application/json; charset=utf-8",
      dataType: "json",
      data: JSON.stringify(cuposAutorizar),
      success: function (data) {
        if (data.Status) {
          autorizado(data);
        } else {
          if (data.TypeError == "API") {
            if (data.ShowMessageRepeatedAlpha) {
              showMessageRepeatedAlpha(data.Message.substr(data.Message.indexOf("[") + 1, data.Message.lastIndexOf("]") - data.Message.indexOf("[") - 1))
            }
            mensajeAlerta.addAlert(data.Message, "alert-danger");
          } else {
            mensajeAlerta.addAlert("Ha ocurrido un error", "alert-danger");
          }
        }
      },
      error: function (msg) {
        if (msg.TypeError == "API")
          mensajeAlerta.addAlert(msg.Messsage, "alert-danger");
        else
          mensajeAlerta.addAlert("Ha ocurrido un error", "alert-danger");
      }
    });
  }
}

function showMessageRepeatedAlpha(alfanumericos) {
  if (confirm("Los alfanuméricos " + alfanumericos + " ya se encuentran registrados en SIL ¿Desea agregar el resto?")) {
    mensajeAlerta.removeAllAlerts();
    var cuponuevo = getNuevo();
    cuponuevo.EmparejoTurnos = true;
    autorizar(cuponuevo);
  }
}

function getActionAutorizar() {
  if (cambioCentro()) {
    return modelData.actionCambiarCentro;
  } else {
    return modelData.actionAutorizar;
  }
}

function autorizado(data) {
  if (data.Status) {
    document.querySelectorAll(".list-group-item.active").forEach(function (el) {
      el.parentNode.removeChild(el);
    });
    setEstado(cupoOriginal);
    checkearSiQuedanCuposPendientesAutorizar();
    mensajeAlerta.addAlert("Se agregaron correctamente", "alert-success");
  }
}

//Nominado o no
var cupoOriginal = getCupo();
var cupoDesactivado = setCupo(true);
var cupoOriginalDesactivado = getCupo("disabled");
var memoEstado = cupoOriginal;
var mensajeAlerta = new MensajeAlerta(document.getElementById("FormNuevo"));
var tipoCupos = getTipoCupos(cupoOriginal, mensajeAlerta);

function getCupo(property = "value") {
  return {
    Comprador: document.getElementById("Compcta")[property],
    CompradorNombre: document.getElementById("CompradorNombre").innerText,
    Vendedor: document.getElementById("Vendcta")[property],
    VendedorNombre: document.getElementById("VendedorNombre").innerText,
    Puerto: document.getElementById("Puerto")[property],
    Consignacion: {
      Cuitsolicitante: agregarGuionesText(document.getElementById("Cuitsolicitante")[property]),
      Nomsolicitante: document.getElementById("CuitsolicitanteName")[property],
      Cuitintermediario: agregarGuionesText(document.getElementById("Cuitintermediario")[property]),
      Nomintermediario: document.getElementById("CuitintermediarioName")[property],
      Cuitrtecomercial: agregarGuionesText(document.getElementById("Cuitrtecomercial")[property]),
      Nomrtecomercial: document.getElementById("CuitrtecomercialName")[property],
      Cuitcorrcomp: agregarGuionesText(document.getElementById("Cuitcorrcomp")[property]),
      Nomcorrcomp: document.getElementById("CuitcorrcompName")[property],
      Cuitmat: agregarGuionesText(document.getElementById("Cuitmat")[property]),
      Nommat: document.getElementById("CuitmatName")[property],
      Cuitcorrvta: agregarGuionesText(document.getElementById("Cuitcorrvta")[property]),
      Nomcorrvta: document.getElementById("CuitcorrvtaName")[property],
      Cuitrteent: agregarGuionesText(document.getElementById("Cuitrteent")[property]),
      Nomrteent: document.getElementById("CuitrteentName")[property],
      Cuitdestinatario: agregarGuionesText(document.getElementById("Cuitdestinatario")[property]),
      Nomdestinatario: document.getElementById("CuitdestinatarioName")[property],
      CuitRteComercialProductor: agregarGuionesText(document.getElementById("CuitRteComercialProductor")[property]),
      NomRteComercialProductor: document.getElementById("CuitRteComercialProductorName")[property],
      CuitRteComercialVentaPrimaria: agregarGuionesText(document.getElementById("CuitRteComercialVentaPrimaria")[property]),
      NomRteComercialVentaPrimaria: document.getElementById("CuitRteComercialVentaPrimariaName")[property],
    },
    Observaciones: document.getElementById("Observaciones")[property],
    Centro: document.getElementById("Centro")[property],
    Producto: document.getElementById("Producto")[property],
    Vendcyo: property === "value" ? document.getElementById("VendcyoBoolValue")["checked"] : document.getElementById("VendcyoBoolValue")[property]
  };
}

function setCupo(defaultValue) {
  return {
    Comprador: defaultValue,
    Vendedor: defaultValue,
    Puerto: defaultValue,
    Consignacion: {
      Cuitsolicitante: defaultValue,
      Nomsolicitante: defaultValue,
      Cuitintermediario: defaultValue,
      Nomintermediario: defaultValue,
      Cuitrtecomercial: defaultValue,
      Nomrtecomercial: defaultValue,
      Cuitcorrcomp: defaultValue,
      Nomcorrcomp: defaultValue,
      Cuitmat: defaultValue,
      Nommat: defaultValue,
      Cuitcorrvta: defaultValue,
      Nomcorrvta: defaultValue,
      Cuitrteent: defaultValue,
      Nomrteent: defaultValue,
      Cuitdestinatario: defaultValue,
      Nomdestinatario: defaultValue,
      CuitRteComercialProductor: defaultValue,
      NomRteComercialProductor: defaultValue,
      CuitRteComercialVentaPrimaria: defaultValue,
      NomRteComercialVentaPrimaria: defaultValue,
    },
    Observacion: defaultValue,
    Centro: defaultValue,
    Producto: defaultValue,
    Vendcyo: defaultValue
  };
}

function validate() {
  if (tipoCupos.isValid()) {

  } else {
    tipoCupos.showErrorMessage();
  }
}

//Autocompletado
$(".input-cuenta").autocomplete({
  source: function (request, response) {
    $.ajax({
      url: this.element.data("cuenta"),
      type: "POST",
      contentType: "application/json",
      dataType: "json",
      data: JSON.stringify({ Texto: request.term }),
      success: function (data) {
        response($.map(data, function (item) {
          return { label: item.Cuenta + " - " + item.Nombre, value: item.Cuenta, customNombre: item.Nombre, customCuit: item.Cuit };
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
    cambiarNombreCompradorOrVendedorOrPuerto(this, ui);
  },
  //change: function (event, ui) {
  //    cambiarNombreCompradorOrVendedorOrPuerto(this, ui);
  //}
});

function cambiarNombreCompradorOrVendedorOrPuerto(input, ui) {
  switch (input.dataset.tipo) {
    case "Comprador":
      ObservableComprador.notify(ui.item);
      break;
    case "Vendedor":
      ObservableVendedor.notify(ui.item);
      break;
    case "Puerto":
      break;
  }
}

//Nombre Cuenta
var ObservableComprador = new Observable();
var ObservableVendedor = new Observable();

//Si se necesita modificar el nombre de algun campo mas subscribir otra funcion que lo haga
ObservableComprador.subscribe(cambiaNombreComprador);
ObservableComprador.subscribe(setearRteComercial);
ObservableVendedor.subscribe(cambiaNombreVendedor);
ObservableVendedor.subscribe(consultarEsCyoSeleccionar);

//data = cuenta - nombre

function setearRteComercial(data) {
  if (document.getElementById("Cuitrtecomercial").value.trim() === "" || confirm("¿Desea también modificar el Rte Comercial?")) {
    mensajeAlerta.removeAllAlerts();
    var cuitrtecomercial = document.getElementById("Cuitrtecomercial").value.trim();
    var nombrertecomercial = document.getElementById("CuitrtecomercialName").value.trim();
    document.getElementById("Cuitrtecomercial").value = agregarGuionesText(data.customCuit);
    document.getElementById("CuitrtecomercialName").value = data.customNombre;
    mensajeAlerta.appendAlert(`Se modificó el valor de Rte Comercial Venta Secundaria 2 ${cuitrtecomercial} por ${agregarGuionesText(data.customCuit)}`, "alert-info");
    if (cuitrtecomercial != "") {
      var cuitintermediario = document.getElementById("Cuitintermediario").value.trim();
      var nombreintermediario = document.getElementById("CuitintermediarioName").value.trim();
      document.getElementById("Cuitintermediario").value = cuitrtecomercial;
      document.getElementById("CuitintermediarioName").value = nombrertecomercial;
      mensajeAlerta.appendAlert(`Se modificó el valor de Comercial Venta Secundaria ${cuitintermediario} por ${cuitrtecomercial}`, "alert-info");
      if (cuitintermediario != "") {
        var cuitsolicitante = document.getElementById("Cuitsolicitante").value.trim();
        var nombresolicitante = document.getElementById("CuitsolicitanteName").value.trim();
        document.getElementById("Cuitsolicitante").value = cuitintermediario;
        document.getElementById("CuitsolicitanteName").value = nombreintermediario;
        mensajeAlerta.appendAlert(`Se modificó el valor de Titular de CCPP ${cuitsolicitante} por ${cuitintermediario}`, "alert-info");
        if (cuitsolicitante != "") {
          var cuitmat = document.getElementById("Cuitmat").value.trim();
          var nombremat = document.getElementById("CuitmatName").value.trim();
          document.getElementById("Cuitmat").value = cuitsolicitante;
          document.getElementById("CuitmatName").value = nombresolicitante;
          mensajeAlerta.appendAlert(`Se modificó el valor de Mercado a Término ${cuitmat} por ${cuitsolicitante}`, "alert-info");
        }
      }
    }
  }
}

function cambiaNombreComprador(data) {
  document.getElementById("CompradorNombre").innerText = data != null && data != undefined ? data.customNombre : "";
}

function cambiaNombreVendedor(data) {
  document.getElementById("VendedorNombre").innerText = data != null && data != undefined ? data.customNombre : "";
}

//Autocompletar CUITs y Nombres de Consignacion
$(".cuit-cuenta").autocomplete({
  source: function (request, response) {
    $.ajax({
      url: modelData.actionGetCuit,
      type: "POST",
      dataType: "json",
      contentType: "application/json",
      data: JSON.stringify({ Id: request.term }),
      success: function (data) {
        response($.map(data, function (item) {
          return { label: item.Cuenta + " - " + item.Nombre, value: agregarGuionesText(item.Cuenta.toString()), customNombre: item.Nombre };
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
    cambiarNombreConsignacion(this, ui);
  },
  change: function (event, ui) {
    cambiarNombreConsignacion(this, ui);
  }
});

function cambiarNombreConsignacion(input, ui) {
  document.getElementById(input.dataset.nombretarget).value = ui.item != undefined ? ui.item.customNombre : "";
}

$(".nombre-cuenta").autocomplete({
  source: function (request, response) {
    $.ajax({
      url: modelData.actionGetCuposCuitFromNroCuentaOrNombre,
      type: "POST",
      dataType: "json",
      contentType: "application/json",
      data: JSON.stringify({ Texto: request.term }),
      success: function (data) {
        response($.map(data, function (item) {
          return { label: item.Cuenta + " - " + item.Nombre, value: item.Nombre, customNombre: item.Nombre, customCuit: item.Cuit };
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
    cambiarCuitConsignacion(this, ui);
  },
  change: function (event, ui) {
    cambiarCuitConsignacion(this, ui);
  }
});

function cambiarCuitConsignacion(input, ui) {
  if (ui.item != null && ui.item != undefined)
    document.getElementById(input.dataset.cuittarget).value = agregarGuionesText(ui.item.customCuit);
}

function agegarGuionesInput(input) {
  input.value = input.value.slice(0, 2) + "-" + input.value.slice(2, -1) + "-" + input.value.slice(-1)
}

function agregarGuionesText(text) {
  if (typeof text === 'string') {
    text = text.replace(/-/g, "");
    if (text != undefined && text != "" && text != null)
      return text.slice(0, 2) + "-" + text.slice(2, -1) + "-" + text.slice(-1)
    else
      return ""
  }
}

//Eventos cuando los inputs de cuit de consignacion pierden el foco, agregar guiones
var inputsCuitsConsignaciones = document.getElementsByClassName("cuit-cuenta");
for (var i = 0; i < inputsCuitsConsignaciones.length; i++) {
  inputsCuitsConsignaciones[i].addEventListener("blur", function (event) {
    event.target.value = agregarGuionesText(event.target.value);
  });
}

function validaCuitsConsignaciones() {
  for (var i = 0; i < inputsCuitsConsignaciones.length; i++) {
    if (inputsCuitsConsignaciones[i].value != undefined && inputsCuitsConsignaciones[i].value != "" && !validaCuit(inputsCuitsConsignaciones[i].value)) {
      mensajeAlerta.addAlert("Cuit " + inputsCuitsConsignaciones[i].id.replace("Cuit", "") + " no válido", "alert-danger");
      return false;
    }
  }
  return true;
}

//Colocar en primera fila el alfanumerico seleccionado
var lsitasAlfanumericos = document.getElementsByClassName("codigos-alfanumericos");
for (var i = 0; i < lsitasAlfanumericos.length; i++) {
  ordenarListaSeleccionados(lsitasAlfanumericos[i]);
}
function ordenarListaSeleccionados(lista) {
  lista.addEventListener("click", function (ev) {
    var elem = ev.target;
    cambiarSeleccionAlfa(elem);
  });
}

function cambiarSeleccionAlfa(elem) {
  if (!elem.classList.contains("active")) {
    seleccionarAlfa(elem);
  } else {
    quitarSeleccionAlfa(elem);
  }
}

//Volver a la página de Pendientes de Autorizar
function volverPendientesAutorizar() {
  window.location.replace(modelData.actionPendientesAutorizar);
}

//Valida si quedan cupos por autorizar, en caso de que no queden pendientes volver a Pendientes de Autorizar
function checkearSiQuedanCuposPendientesAutorizar() {
  var cantidadAlfasDisponibles = document.getElementsByClassName("list-group-item").length;
  if (cantidadAlfasDisponibles === 0) {
    volverPendientesAutorizar();
  }
}

//Valida si seleccionó alfanuméricos
function validaSeleccionAlfanumerico() {
  var valida = getCodigosDias().length > 0;
  if (!valida) {
    mensajeAlerta.addAlert("Debe seleccionar al menos un alfanumérico", "alert-danger");
  }
  return valida;
}

//Tilda check de Vendedor Cyo
function seleccionarVendedorCyo() {
  var check = document.getElementById("VendcyoBoolValue");
  check.checked = true;
  check.disabled = true;
}

//Destilda check de Vendedor Cyo
function deseleccionarVendedorCyo() {
  document.getElementById("VendcyoBoolValue").checked = false;
}

//Intercambia check de Vendedor Cyo
function cambiarSeleccionVendedorCyo() {
  if (document.getElementById("VendcyoBoolValue").checked) {
    deseleccionarVendedorCyo();
  } else {
    seleccionarVendedorCyo();
  }
}

//Consulta si es una cuenta vendedora cyo y en caso de que lo sea tildar el check
function consultarEsCyoSeleccionar(dataObserver) {
  $.ajax({
    url: modelData.actionEsCYO,
    type: "POST",
    dataType: "json",
    data: { NumeroCuenta: dataObserver.value }, //dataObserver.value es el numero de cuenta del objeto seleccionado en el autocompletado
    success: function (data) {
      if (data) {
        seleccionarVendedorCyo();
        setearRteComercial(dataObserver)
      } else {
        deseleccionarVendedorCyo();
      }
    },
    error: function (msg) {
      console.log(msg);
    }
  });
}

//Evento que se ejecuta cuando el input del comprador pierde el foco
document.getElementById("Compcta").addEventListener("blur", function (event) {
  if (event.target.value == "0" || event.target.value == "") {
    document.getElementById("CompradorNombre").innerText = "";
  }
});

//    //Evento que se ejecuta cuando el input del vendedor pierde el foco
//document.getElementById("Vendcta").addEventListener("blur", function (event) {
//    if (event.target.value == "0" || event.target.value == "") {
//        document.getElementById("VendedorNombre").innerText = "";
//    }
//    consultarEsCyoSeleccionar();
//});

//Evento que se ejecuta cuando el input del vendedor cambia el texto
document.getElementById("Vendcta").addEventListener("keydown", function (event) {
  document.getElementById("VendcyoBoolValue").disabled = false;
});

if (document.getElementById("VendcyoBoolValue").checked)
  document.getElementById("VendcyoBoolValue").disabled = true;

/*Centros*/
function onChangeCentro(ev) {

  //if (!deepEqual(cupo, cupoOriginal, ["Centro"]))
  //    memoEstado = cupo;

  if (cambioCentro()) {
    //memoEstado = getCupo();
    setEstado(cupoOriginal);
    setEstado(cupoDesactivado, "disabled");
  } else {
    //setEstado(memoEstado);
    setEstado(cupoOriginalDesactivado, "disabled");
  }
}

function cambioCentro() {
  return cupoOriginal.Centro !== document.getElementById("Centro").value;
}

//Modifica los valores de los inputs segun el parametro cupo
function setEstado(cupo, property = "value") {
  document.getElementById("Producto")[property] = cupo.Producto
  document.getElementById("Compcta")[property] = cupo.Comprador
  document.getElementById("Vendcta")[property] = cupo.Vendedor
  document.getElementById("VendcyoBoolValue")[property === "value" ? "checked" : property] = cupo.Vendcyo
  document.getElementById("Cuitsolicitante")[property] = cupo.Consignacion.Cuitsolicitante
  document.getElementById("CuitsolicitanteName")[property] = cupo.Consignacion.Nomsolicitante
  document.getElementById("Cuitintermediario")[property] = cupo.Consignacion.Cuitintermediario
  document.getElementById("CuitintermediarioName")[property] = cupo.Consignacion.Nomintermediario
  document.getElementById("Cuitrtecomercial")[property] = cupo.Consignacion.Cuitrtecomercial
  document.getElementById("CuitrtecomercialName")[property] = cupo.Consignacion.Nomrtecomercial
  document.getElementById("Cuitcorrcomp")[property] = cupo.Consignacion.Cuitcorrcomp
  document.getElementById("CuitcorrcompName")[property] = cupo.Consignacion.Nomcorrcomp
  document.getElementById("Cuitmat")[property] = cupo.Consignacion.Cuitmat
  document.getElementById("CuitmatName")[property] = cupo.Consignacion.Nommat
  document.getElementById("Cuitcorrvta")[property] = cupo.Consignacion.Cuitcorrvta
  document.getElementById("CuitcorrvtaName")[property] = cupo.Consignacion.Nomcorrvta
  document.getElementById("Cuitrteent")[property] = cupo.Consignacion.Cuitrteent
  document.getElementById("CuitrteentName")[property] = cupo.Consignacion.Nomrteent
  document.getElementById("Cuitdestinatario")[property] = cupo.Consignacion.Cuitdestinatario
  document.getElementById("CuitdestinatarioName")[property] = cupo.Consignacion.Nomdestinatario
  document.getElementById("CuitRteComercialProductor")[property] = cupo.Consignacion.CuitRteComercialProductor
  document.getElementById("CuitRteComercialProductorName")[property] = cupo.Consignacion.NomRteComercialProductor
  document.getElementById("CuitRteComercialVentaPrimaria")[property] = cupo.Consignacion.CuitRteComercialVentaPrimaria
  document.getElementById("CuitRteComercialVentaPrimariaName")[property] = cupo.Consignacion.NomRteComercialVentaPrimaria

  if (property == "value") {
    document.getElementById("CompradorNombre").innerText = cupo.CompradorNombre
    document.getElementById("VendedorNombre").innerText = cupo.VendedorNombre
  }
}

function deepEqual(object1, object2, arrIgnore) {
  var keys1 = Object.keys(object1);
  var keys2 = Object.keys(object2);

  if (Array.isArray(arrIgnore)) {
    var index1 = -1;
    var index2 = -1;
    for (var i = 0; i < arrIgnore.length; i++) {
      index1 = keys1.indexOf(arrIgnore[i]);
      index2 = keys2.indexOf(arrIgnore[i]);
      if (index1 > -1 && index2 > -1) {
        keys1.splice(index1, 1);
        keys2.splice(index2, 1);
      }
    }
  }

  if (keys1.length !== keys2.length)
    return false;

  for (const key of keys1) {
    const val1 = object1[key];
    const val2 = object2[key];
    const areObjects = isObject(val1) && isObject(val2);
    if (areObjects && !deepEqual(val1, val2) || !areObjects && val1 !== val2)
      return false;
  }

  return true;
}

function isObject(object) {
  return object != null && typeof object === 'object';
}

var alfasSeleccionadosToken = [];

function borrarAlfasSeleccionadosToken() {
  alfasSeleccionadosToken = [];
}

tokenfieldAlfas.on('tokenfield:createtoken', tokenAgregar);
tokenfieldAlfas.on('tokenfield:removedtoken', tokenQuitar);

function tokenAgregar(event) {
  var alfa = buscarAlfaDia(event.attrs.value, document.getElementById("modalAlfas").dataset.nrodia);
  if (typeof alfa !== "undefined") {
    alfasSeleccionadosToken.push({ alfa: event.attrs.value, dia: document.getElementById("modalAlfas").dataset.nrodia });
    seleccionarAlfa(alfa);
  }
}

function tokenQuitar(event) {
  var alfa = buscarAlfaDia(event.attrs.value, document.getElementById("modalAlfas").dataset.nrodia);
  if (typeof alfa !== "undefined") quitarSeleccionAlfa(alfa);
}

function handleClickBtnSeleccionar(event) {
  tokenfieldAlfas.tokenfield('setTokens', []);
  document.getElementById("fechaModalAlfas").innerText = event.target.dataset.fecha;
  document.getElementById("modalAlfas").dataset.nrodia = event.target.dataset.nrodia;
  $("#modalAlfas").modal("show");
}

function handleSubmitModalAlfas(event) {
  $('#modalAlfas').modal('hide')
  borrarAlfasSeleccionadosToken();
  event.preventDefault();
}

document.getElementById("btnCerrarModal").addEventListener("click", handleCloseModalAlfas)

function handleCloseModalAlfas(event) {
  $('#modalAlfas').modal('hide')
  for (var i = 0; i < alfasSeleccionadosToken.length; i++) {
    quitarSeleccionAlfa(buscarAlfaDia(alfasSeleccionadosToken[i].alfa, alfasSeleccionadosToken[i].dia));
  }
  borrarAlfasSeleccionadosToken();
}

function buscarAlfaDia(alfa, dia) {
  var listaAlfasDia = Array.from(document.getElementById("codigos-dia-" + dia).querySelectorAll(".list-group-item"));
  return listaAlfasDia.find(function (el) { return el.innerText.toUpperCase() === alfa.toUpperCase() })
}

function seleccionarAlfa(elem) {
  var parent = elem.parentNode;
  elem.classList.add("active");
  var copiaelem = elem.cloneNode(true);
  parent.removeChild(elem);
  parent.prepend(copiaelem);
}

function quitarSeleccionAlfa(elem) {
  if (elem.classList.contains("active")) {
    var parent = elem.parentNode;
    var allActives = parent.querySelectorAll(".active");
    if (allActives.length > 0) {
      var copiaelem = elem.cloneNode(true);
      copiaelem.classList.remove("active");
      parent.insertBefore(copiaelem, allActives[allActives.length - 1].nextSibling);
      parent.removeChild(elem);
    }
  }
}

function visibleCaratula(value) {
  //MATBA ROFEX
  if (value.trim() == "30525698412" || value.trim() == '30-52569841-2') {
    document.getElementById("col-caratula").style.display = "block";
  } else {
    document.getElementById("col-caratula").style.display = "none";
  }
}

function limpiarAlfasSeleccionados() {

}
(function () {
  Array.from(document.getElementsByClassName("cuit-cuenta")).forEach(el => {
    el.value = agregarGuionesText(el.value);
  })
})();

$(document).ready(function () {
  visibleCaratula(document.getElementById("Compcta").value);
  if ($("#Compcta").val().trim() != "30525698412" && $("#Compcta").val().trim() != '30-52569841-2') {
    visibleCaratula(document.getElementById("Vendcta").value);
  }

  $("#Vendcta").change(function () {
    if ($("#Compcta").val().trim() != "30525698412" && $("#Compcta").val().trim() != '30-52569841-2') {
      visibleCaratula(this.value.trim());
    }
  });

  $("#Compcta").change(function () {
    if ($("#Vendcta").val().trim() != "30525698412" && $("#Vendcta").val().trim() != '30-52569841-2') {
      visibleCaratula(this.value.trim());
    }
  });
})

var tfContactoComercial = $('#ContactoComercial').tokenfield({
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
          },
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