var idsEditar = "";
var model = {
    vendedor: window.location.href.substr(window.location.href.lastIndexOf('/') + 1).split("-")[1]
};

//Modal consignaciones
var modalConsignaciones = new ModalConsignaciones();
modalConsignaciones.btnAceptarClickEventListener(function () {
    document.getElementById("FormEditar").submit();
});

function handleBtnClose(id) {
    cambiarEstado(id, toggle);
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

//Funcion para cambiar el color de fondo de <li> a anular (si esta rojo pasa a blanco, si esta blanco pasa a rojo)
function toggle(id){
    var regex = new RegExp('^' + id + '$|^(' + id + '-)|(-' + id + '(?=-))|(-' + id + ')$', 'g');
    if (regex.test(idsEditar)){
        habilitar(id, regex);
    } else {
        anular(id);
    }
}

function anular(id) {
    var lista = document.getElementById(id);
    if (idsEditar != "") {
        idsEditar += "-";
    }
    idsEditar += id;
    lista.classList.add("anulado");
    lista.style.backgroundColor = "#dc3545";
    lista.style.color = "white";
}

function habilitar(id, regex) {
    var lista = document.getElementById(id);
    if (idsEditar != "") {
        if (regex == undefined) regex = new RegExp('^' + id + '$|^(' + id + '-)|(-' + id + '(?=-))|(-' + id + ')$', 'g');
        idsEditar = idsEditar.replace(regex, "");
    }
    lista.classList.remove("anulado");
    lista.style.backgroundColor = "";
    lista.style.color = "";
}

handleClickBtnShowModal(document.getElementById("Editar"), function () {
    if (idsEditar == "") {
        alert("Debe seleccionar al menos un cupo");
        return false;
    }
    return true;
});

//Evento del boton Aceptar del modal Motivo
handleClickBtnAceptarMotivo(function (textMotivo, callbackSuccess) {
    //Si confirma el aviso envia los ids al controlador para anular
    if (confirm("¿Está seguro que desea anular los cupos seleccionados?")) {
        $.ajax({
            type: "POST",
            url: window.modelData.actionAnular,
            content: "application/json",
            dataType: "json",
            data: {
                idCupos: idsEditar,
                motivo: textMotivo,
                tipo: (tieneTipoAnulacion() ? document.getElementById("TipoAnulacionSeleccionado").value : "Cupo"),
                cyo: modelData.cyo
            },
            success: function (data) {
                handleResponse(data);
                if (typeof callbackSuccess === "function") callbackSuccess(data);
            },
            error: function (xhr, textStatus, errorThrown) {
                handleResponseError("Error al enviar");
            }
        });
    }
});

//Tipo de Anulacion en modal Motivo
tipoAnulacionDistribucion(document.getElementsByClassName("distribuido-borde"), function (data) {
    var cuposDistribuidos = Array.from(document.getElementsByClassName("distribuido-borde"));
    var diferencia = getCuposPermiteAnularDistribucion().filter(function (i) { return cuposDistribuidos.indexOf(i) < 0; });
    setCuposPermiteAnularDistribucion(document.getElementsByClassName("distribuido-borde"));
});

function handleResponse(data) {
    if (data.Respuesta == "ANULADOS") {
        addAlert("Se anularon los cupos seleccionados con éxito", "alert-success");
        cambiarEstadoBtnAceptar();
        cambiarEstadoBtnCancelar();
        $('#modalMotivo').modal('hide');
        if (data.Tipo == "Cupo") {
            handleCupoAnulado();
        } else if (data.Tipo == "Distribucion") {
            handleDistribucionAnulada();
        }
        actualizarLista(data.Cupos);
        idsEditar = "";
    } else if (data.Respuesta == "ERROR") {
        handleResponseError("Se produjo un error durante el proceso");
    } else {
        handleResponseError(data.Respuesta);
    }
}

function actualizarLista(cupos) {
    cupos.forEach(function (cupo) {
        eliminarCupoDistintoVendedor(cupo.Id, cupo.CuentaVendedor);
    });
}

function eliminarCupoDistintoVendedor(idCupo, vendedor) {
    if (vendedor != model.vendedor) {
        document.getElementById(idCupo).remove();
    }
}

function handleCupoAnulado() {
    cambiarEstadoCupo(deshabilitarCupoAnulado);
}

function handleResponseError(mensaje) {
    addAlert(mensaje, "alert-danger");
    $('#modalMotivo').modal('hide');
}

function handleDistribucionAnulada() {
    cambiarEstadoCupo();
}

function cambiarEstadoCupo(callback) {
    var element;
    if (idsEditar != "") {
        var ids = idsEditar.split("-");
        ids.forEach(function (v) {
            element = document.getElementById(v);
            toggle(v);
            if (typeof callback === 'function') callback(element);
            quitarIndicadorDistribuido(element);
        });
    }
}

function deshabilitarCupoAnulado(element){
    $(element).find('.close').remove();
    $(element).addClass("list-group-item-danger");
    agregarBotonInfo(element);
    agregarEventoGetMotivo(element.querySelectorAll(".icon-li")[0]);
}

function addAlert(message, tipoAlerta){
    var htmlAlert = '<div class="alert ' + tipoAlerta + ' alert-dismissible fade show" role="alert">';
    htmlAlert += message;
    htmlAlert += '<button type="button" class="close" data-dismiss="alert" aria-label="Close">';
    htmlAlert += '<span aria-hidden="true">&times;</span>';
    htmlAlert += '</button>';
    htmlAlert += '</div>';
    $('.alert').remove();
    $("#FormEditar").before(htmlAlert);
}

document.getElementById("Editar").addEventListener("click", function(){
    if (idsEditar == "") {
        alert("Debe seleccionar al menos un cupo");
        return;
    }
    if (arrayCuposSeleccionadosPuedenAnularDistribucion().length > 0) {
        mostrarTipoAnulacion();
    } else {
        ocultarTipoAnulacion();
    }
    $('#modalMotivo').modal('show');
});

dropdownmotivo.onchange = function(){
    if (Number.parseInt(this.value) === 0){
        descripcionMotivo.disabled = false;
    } else {
        descripcionMotivo.disabled = true;
        descripcionMotivo.value = "";
    }
}

function agregarBotonInfo(el){
    var boton = '<button type="button" class="icon-li" data-toggle="tooltip" data-placement="right" title="">';
    boton += '<span class="oi oi-info"></span>';
    boton += '</button>';
    el.innerHTML += boton;
}

var btnsInfoMotivo = document.querySelectorAll(".icon-li");
for (var i = 0; i < btnsInfoMotivo.length; i++){
    agregarEventoGetMotivo(btnsInfoMotivo[i]);
}

function agregarEventoGetMotivo(el) {
    el.addEventListener("click", function () {
        mostrarMotivoAnulado(this);
    });
}

//$('.icon-li').tooltip({ boundary: 'window' });
function mostrarMotivoAnulado(el){
    $.ajax({
        type: "POST",
        url: window.modelData.actionGetMotivo + "/" + el.parentNode.id,
        content: "application/json; charset=utf-8",
        dataType: "json",
        data: {},
        success: function (result) {
            el.title = result.Motivo;
            $(el).tooltip('show');
        },
        error: function (xhr, textStatus, errorThrown) {
        }
    });
}

function cambiarEstadoBtnCancelar(){
    var btnCancelar = document.getElementById("btnCancelar");
    if (window.modelData.estadoBtnCancelar) {
        btnCancelar.innerText = "Volver";
        btnCancelar.className = "btn btn-secondary";
        window.modelData.estadoBtnCancelar = false;
    } else {
        btnCancelar.innerText = "Cancelar";
        btnCancelar.className = "btn btn-danger";
        window.modelData.estadoBtnCancelar = true;
    }
}

function cambiarEstadoBtnAceptar(){
    var btnAceptar = document.getElementById("Editar");
    if (window.modelData.estadoBtnAceptar) {
        btnAceptar.disabled = true;
        window.modelData.estadoBtnAceptar = false;
    } else {
        btnAceptar.disabled = false;
        window.modelData.estadoBtnAceptar = true;
    }
}

function handleCheckPorDia(check) {
    if (check.checked == true) {
        seleccionarLista(document.getElementById(check.dataset.lista));
    } else {
        deseleccionarLista(document.getElementById(check.dataset.lista));
    }
}

function seleccionarLista(lista) {
    items = lista.querySelectorAll(".list-group-item:not(.list-group-item-danger):not(.anulado)");
    items.forEach(function (item) {
        cambiarEstado(item.id, anular);
    });
}

function deseleccionarLista(lista) {
    items = lista.querySelectorAll(".list-group-item:not(.list-group-item-danger)");
    items.forEach(function (item) {
        cambiarEstado(item.id, habilitar);
    });
}

function quitarIndicadorDistribuido(cupo) {
    cupo.classList.remove("distribuido-borde");
}

function intersectionArrays(arr1, arr2) {
    return arr1.filter(function (value) { return arr2.indexOf(value) !== -1 });
}

function arrayCuposSeleccionadosPuedenAnularDistribucion() {
    var items = idsEditar.split("-").map(function (id) {
        return document.getElementById(id);
    });
    return intersectionArrays(getCuposPermiteAnularDistribucion(), items);
}