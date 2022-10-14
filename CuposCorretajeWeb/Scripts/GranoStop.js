var btnsUpd = document.querySelectorAll(".btn-upd");
var btnsDel = document.querySelectorAll(".btn-del");
var Alerta = new MensajeAlerta(document.getElementById("main"));

registryClickBtnAgregarGrano();

btnsUpd.forEach(function (el) {
    registryClickEventBtnModificar(el);
});

btnsDel.forEach(function (el) {
    registryClickEventBtnEliminar(el);
});

function construirGranoModal() {
    return {
        NroGrano: document.getElementById("NroGrano").value,
        NombreGrano: document.getElementById("NombreGrano").value
    }
}

function construirGranoFilaTabla(fila) {
    return {
        NroGrano: fila.id.replace("grano-", ""),
        NombreGrano: fila.children[1].innerText
    }
}

function registryClickBtnAceptarModal(fnSend) {
    document.getElementById("formGrano").onsubmit = function (event) {
        event.preventDefault();
        if (!validaFormularioGrano()) return;
        fnSend();
    }
}

function successAgregar(data) {
    $("#modal-nuevo").modal("hide");
    if (!data.error) {
        agregarFila(data);
        Alerta.addAlert("El grano se agregó correctamente", "alert-success")
    } else {
        Alerta.addAlert(data.data, "alert-danger")
    }
}

function registryClickBtnAgregarGrano() {
    document.getElementById("btnAgregar").addEventListener("click", function (event) {
        cambiarTitulo("Nuevo");
        limpiarModal();
        desbloquearInputsModal();
        registryClickBtnAceptarModal(function () {
            document.getElementById("formGrano").removeEventListener("click", registryClickEventBtnModificar);
            send(model.actionNuevoGrano, construirGranoModal(), successAgregar);
        });
    });
}

function validaFormularioGrano() {
    if (document.getElementById("NroGrano").value.trim() === "") {
        alert("Debe ingresar un valor en el campo Nro. Grano STOP");
        return false;
    }
    if (document.getElementById("NombreGrano").value.trim() === "") {
        alert("Debe ingresar un valor en el campo Nombre Grano");
        return false;
    }
    return true;
}

function successModificar(data) {
    $("#modal-nuevo").modal("hide");
    if (!data.error) {
        modificarFila(data);
        Alerta.addAlert("El grano se modificó correctamente", "alert-success")
    } else {
        Alerta.addAlert(data.data, "alert-danger")
    }
}

function registryClickEventBtnModificar(el) {
    el.addEventListener("click", function (event) {
        cambiarTitulo("Modificar");
        var fila = document.getElementById("grano-" + event.target.dataset["id"]);
        bloquearInputsModal();
        setValoresModal(fila.id.replace("grano-", ""), fila.children[1].innerText);
        registryClickBtnAceptarModal(function () {
            send(model.actionModificarGrano, construirGranoModal(), successModificar);
        });
        $("#modal-nuevo").modal("show");
    });
}

function successEliminar(data) {
    if (!data.error) {
        borrarFila(data);
        Alerta.addAlert("El grano se eliminó correctamente", "alert-success")
    } else {
        Alerta.addAlert(data.data, "alert-danger")
    }
}

function registryClickEventBtnEliminar(el) {
    el.addEventListener("click", function (event) {
        if (confirm("¿Está seguro que desea eliminar esta relación?")) {
            var fila = document.getElementById("grano-" + event.target.dataset["id"]);
            send(model.actionEliminarGrano, construirGranoFilaTabla(fila), successEliminar);
        }
    });
}

function limpiarModal() {
    setValoresModal("", "");
}

function setValoresModal(NroGrano, NombreGrano) {
    document.getElementById("NroGrano").value = NroGrano;
    document.getElementById("NombreGrano").value = NombreGrano.trim();
}

function send(url, data, fnSuccess) {
    $.ajax({
        type: "POST",
        url: url,
        content: "application/json",
        dataType: "json",
        data: data,
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
var tabla = document.getElementById("tablaGranos");

function getIdFila(data) {
    return "grano-" + data.data.NroGrano;
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
    var fila = '<tr id="grano-' + data.NroGrano + '">' +
            '<td>' + data.NroGrano + '</td>' +
            '<td style="text-align:left">' + data.NombreGrano + '</td>' +
            '<td>' +
                '<div style="display: inline-block">' +
                    '<button class="btn btn-warning px-3 btn-upd text-white" data-id="' + data.NroGrano + '">Modificar</button>' +
                    '<button class="btn btn-danger btn-del" data-id="' + data.NroGrano + '">Eliminar</button>' +
                '</div>' +
            '</td>' +
        '</tr>';
    return fila;
}

function modificarFila(data) {
    if (data.result) {
        var idFila = getIdFila(data);
        var fila = tabla.querySelector("#" + idFila);
        fila.cells[0].innerText = data.data.NroGrano;
        fila.cells[1].innerText = data.data.NombreGrano;
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
    document.getElementById("NroGrano").disabled = true;
}

function desbloquearInputsModal() {
    document.getElementById("NroGrano").disabled = false;
}

//Cambiar titulo modal
function cambiarTitulo(title) {
    document.getElementsByClassName("modal-title")[0].innerText = title;
}