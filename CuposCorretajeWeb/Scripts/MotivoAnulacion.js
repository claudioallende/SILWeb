var btnsInfoMotivo = document.querySelectorAll(".btn-info--anulado"),
    dropdownmotivo = document.getElementById("MotivoAnulacionSeleccionado"),
    tipoAnulacion = {
        dropDownTipoAnulacion: document.getElementById("TipoAnulacionSeleccionado"),
        group: document.getElementById("TipoAnulacion"),
        tieneDropDownTipoAnulacion: (document.getElementById("TipoAnulacionSeleccionado") != null),
        pideTipoAnulacion: false,
        fnMuestraTipoAnulacion: undefined,
        cuposPermiteAnularDistribucion: undefined
    },
    descripcionMotivo = document.getElementById("descripcionMotivo"),
    textMotivo = "";

for (var i = 0; i < btnsInfoMotivo.length; i++){
    agregarEventoGetMotivo(btnsInfoMotivo[i]);
}

//Evento del boton Aceptar del modal Motivo
//funcionEjecutar: funcion a ejecutarse despues de validar que se selecciono motivo; parametro: textMotivo, el motivo que se selecciono
function handleClickBtnAceptarMotivo(funcionEjecutar) {
    document.getElementById("btnAceptarMotivo").onclick = function () {

        //Seleccion tipo de anulacion
        if (tipoAnulacion.pideTipoAnulacion && tipoAnulacion.dropDownTipoAnulacion.value == "") {
            alert("Debe seleccionar un tipo de anulación");
            return;
        }

        textMotivo = getMotivo();

        if (funcionEjecutar !== undefined) {
            funcionEjecutar(textMotivo, tipoAnulacion.fnMuestraTipoAnulacion);
        }
    }
}

function getMotivo() {
    //Si selecciono motivo Otro o no selecciono motivo
    if (dropdownmotivo.value == 0) {
        //Si la descripcion del motivo esta vacio avisar y salir de la funcion
        if (descripcionMotivo.value.trim() == "" || dropdownmotivo.value === undefined) {
            alert("Debe ingresar una descripción del motivo");
            return;
        }
        textMotivo = descripcionMotivo.value.trim();
    } else {
        textMotivo = dropdownmotivo.options[dropdownmotivo.selectedIndex].innerText.trim();
    }
    return textMotivo;
}

function agregarEventoGetMotivo(el) {
    el.addEventListener("click", function () {
        mostrarMotivoAnulado(this);
    });
}

//condicion: funcion de condicion si muestra o no el modal; false = no muestra, true = muestra
function handleClickBtnShowModal(btnShowModal, condicion, evento) {
    if (evento === undefined) evento = "click";
    $(btnShowModal).on(evento, function () {
        if (condicion !== undefined) {
            if (condicion()) {
                $('#modalMotivo').modal('show');
            }
        } else {
            $('#modalMotivo').modal('show');
        }
    });
}

dropdownmotivo.onchange = function(){
    if (Number.parseInt(this.value) === 0){
        descripcionMotivo.disabled = false;
    } else {
        descripcionMotivo.disabled = true;
        descripcionMotivo.value = "";
    }
}

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

function tipoAnulacionDistribucion(cupos, fnAceptarAnulacion) {
    if (cupos instanceof HTMLCollection) cupos = Array.from(cupos);
    tipoAnulacion.cuposPermiteAnularDistribucion = cupos;
    tipoAnulacion.fnMuestraTipoAnulacion = fnAceptarAnulacion;
    if (tipoAnulacion.cuposPermiteAnularDistribucion.length === 0 && tipoAnulacion.tieneDropDownTipoAnulacion) {
        ocultarTipoAnulacion();
    } else {
        tipoAnulacion.pideTipoAnulacion = true;
    }
}

function getCuposPermiteAnularDistribucion() {
    return tipoAnulacion.cuposPermiteAnularDistribucion;
}

function setCuposPermiteAnularDistribucion(cupos) {
    if (cupos instanceof HTMLCollection) cupos = Array.from(cupos);
    tipoAnulacion.cuposPermiteAnularDistribucion = cupos;
}

function mostrarTipoAnulacion() {
    tipoAnulacion.group.style.display = "block";
    tipoAnulacion.pideTipoAnulacion = true;
}

function ocultarTipoAnulacion() {
    tipoAnulacion.group.style.display = "none";
    tipoAnulacion.pideTipoAnulacion = false;
}

function tieneTipoAnulacion() {
    return tipoAnulacion.pideTipoAnulacion;
}