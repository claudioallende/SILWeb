var btnsUpd = document.querySelectorAll(".btn-upd");
var btnsDel = document.querySelectorAll(".btn-del");
var Alerta = new MensajeAlerta(document.getElementById("main"));

registryClickBtnAgregarPuerto();

btnsUpd.forEach(function (el) {
    registryClickEventBtnModificar(el);
});

btnsDel.forEach(function (el) {
    registryClickEventBtnEliminar(el);
});

function construirPuertoModal() {
    return {
        NroPuerto: document.getElementById("NroPuerto").value,
        NombrePuerto: document.getElementById("NombrePuerto").value
    }
}

function construirPuertoFilaTabla(fila) {
    return {
        NroPuerto: fila.id.replace("puerto-", ""),
        NombrePuerto: fila.children[1].innerText
    }
}

function registryClickBtnAceptarModal(fnSend) {
    document.getElementById("formPuerto").onsubmit = function (event) {
        event.preventDefault();
        if (!validaFormularioPuerto()) return;
        fnSend();
    }
}

function successAgregar(data) {
    $("#modal-nuevo").modal("hide");
    if (!data.error) {
        agregarFila(data);
        Alerta.addAlert("El puerto se agregó correctamente", "alert-success")
    } else {
        Alerta.addAlert(data.data, "alert-danger")
    }
}

function registryClickBtnAgregarPuerto() {
    document.getElementById("btnAgregar").addEventListener("click", function (event) {
        cambiarTitulo("Nuevo");
        limpiarModal();
        desbloquearInputsModal();
        registryClickBtnAceptarModal(function () {
            document.getElementById("formPuerto").removeEventListener("click", registryClickEventBtnModificar);
            send(model.actionNuevoPuerto, construirPuertoModal(), successAgregar);
        });
    });
}

function validaFormularioPuerto() {
    if (document.getElementById("NroPuerto").value.trim() === "") {
        alert("Debe ingresar un valor en el campo Nro. Puerto STOP");
        return false;
    }
    if (document.getElementById("NombrePuerto").value.trim() === "") {
        alert("Debe ingresar un valor en el campo Nombre Puerto");
        return false;
    }
    return true;
}

function successModificar(data) {
    $("#modal-nuevo").modal("hide");
    if (!data.error) {
        modificarFila(data);
        Alerta.addAlert("El puerto se modificó correctamente", "alert-success")
    } else {
        Alerta.addAlert(data.data, "alert-danger")
    }
}

function registryClickEventBtnModificar(el) {
    el.addEventListener("click", function (event) {
        cambiarTitulo("Modificar");
        var fila = document.getElementById("puerto-" + event.target.dataset["id"]);
        bloquearInputsModal();
        setValoresModal(fila.id.replace("puerto-", ""), fila.children[1].innerText);
        registryClickBtnAceptarModal(function () {
            send(model.actionModificarPuerto, construirPuertoModal(), successModificar);
        });
        $("#modal-nuevo").modal("show");
    });
}

function successEliminar(data) {
    if (!data.error) {
        borrarFila(data);
        Alerta.addAlert("El puerto se eliminó correctamente", "alert-success")
    } else {
        Alerta.addAlert(data.data, "alert-danger")
    }
}

function registryClickEventBtnEliminar(el) {
    el.addEventListener("click", function (event) {
        if (confirm("¿Está seguro que desea eliminar esta relación?")) {
            var fila = document.getElementById("puerto-" + event.target.dataset["id"]);
            send(model.actionEliminarPuerto, construirPuertoFilaTabla(fila), successEliminar);
        }
    });
}

function limpiarModal() {
    setValoresModal("", "");
}

function setValoresModal(NroPuerto, NombrePuerto) {
    document.getElementById("NroPuerto").value = NroPuerto;
    document.getElementById("NombrePuerto").value = NombrePuerto.trim();
}

function send(url, data, fnSuccess) {
    $.ajax({
        type: "POST",
        url: url,
        contentType: "application/json",
        dataType: "json",
        data: JSON.stringify(data),
        success: function (data) {
            //console.log("Se agregó correcto");
            if (typeof fnSuccess === "function")
                fnSuccess(data);
        },
        error: function (xhr, textStatus, errorThrown) {
            $("#modal-nuevo").modal("hide");
            Alerta.addAlert("Error", "alert-danger");
            console.log(xhr.responseText);
        }
    })
}

/*Actualizar tabla relaciones centro terminal*/
var tabla = document.getElementById("tablaPuertos");

function getIdFila(data) {
    return "puerto-" + data.data.NroPuerto;
}

function agregarFila(data) {
    var idFila = getIdFila(data);
    var table = document.createElement("table");
    table.innerHTML = nuevaFila(data.data);
    var filaCreada = table.tBodies[0].firstChild;
    var btnModificar = filaCreada.querySelector(".btn-upd");
    var btnEliminar = filaCreada.querySelector(".btn-del");
    registryClickEventBtnModificar(btnModificar);
    registryClickEventBtnEliminar(btnEliminar);
    tabla.tBodies[0].appendChild(filaCreada);
    $("#modal-nuevo").modal("hide");
}

function nuevaFila(data) {
    var fila = '<tr id="puerto-' + data.NroPuerto + '">' +
            '<td>' + data.NroPuerto + '</td>' +
            '<td style="text-align:left">' + data.NombrePuerto + '</td>' +
            '<td>' +
                '<div style="display: inline-block">' +
                    '<button class="btn btn-warning px-3 btn-upd text-white" data-id="' + data.NroPuerto + '">Modificar</button>' +
                    '<button class="btn btn-danger btn-del" data-id="' + data.NroPuerto + '">Eliminar</button>' +
                '</div>' +
            '</td>' +
        '</tr>';
    return fila;
}

function modificarFila(data) {
    if (data.result) {
        var idFila = getIdFila(data);
        var fila = tabla.querySelector("#" + idFila);
        fila.cells[0].innerText = data.data.NroPuerto;
        fila.cells[1].innerText = data.data.NombrePuerto;
    }
}

function borrarFila(data) {
    if (data.result) {
        var idFila = getIdFila(data);
        var fila = tabla.querySelector("#" + idFila);
        tabla.tBodies[0].removeChild(fila)
    }
}

//Bloquear input
function bloquearInputsModal() {
    document.getElementById("NroPuerto").disabled = true;
}

function desbloquearInputsModal() {
    document.getElementById("NroPuerto").disabled = false;
}

//Cambiar titulo modal
function cambiarTitulo(title) {
    document.getElementsByClassName("modal-title")[0].innerText = title;
}