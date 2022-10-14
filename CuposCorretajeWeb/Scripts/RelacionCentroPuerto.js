var cuentas,
    cuentaSeleccionada,
    autocomplete;

var btnsUpd = document.querySelectorAll(".btn-upd");
var btnsDel = document.querySelectorAll(".btn-del");
var Alerta = new MensajeAlerta(document.getElementById("main"));

registryClickBtnAgregarRelacion();

btnsUpd.forEach(function (el) {
    registryClickEventBtnModificar(el);
});

btnsDel.forEach(function (el) {
    registryClickEventBtnEliminar(el);
});

function construirRelacionModal() {
    var selectCentro = document.getElementById("CodigoOrNombreCentro");
    
    return {
        Id: document.getElementById("IdRelacion").value,
        IdTerminal: document.getElementById("IdOrNombrePuerto").value,
        NombreTerminal: document.getElementById("NombrePuerto").innerHTML,
        CodigoCentro: document.getElementById("CodigoOrNombreCentro").value,
        NombreCentro: selectCentro.options[selectCentro.options.selectedIndex].innerText
    }
}

function construirRelacionFilaTabla(fila) {
    return {
        Id: fila.id.replace("relacion-", ""),
        IdTerminal: fila.children[0].innerText,
        NombreTerminal: fila.children[1].innerText,
        CodigoCentro: fila.children[2].innerText,
        NombreCentro: fila.children[3].innerText
    }
}

function registryClickBtnAceptarModal(fnSend) {
    document.getElementById("formRelacion").onsubmit = function (event) {
        event.preventDefault();
        if (!validaFormularioRelacion()) return;
        fnSend();
    }
}

function successAgregar(data) {
    $("#modal-nuevo").modal("hide");
    if (!data.error) {
        agregarFila(data);
        Alerta.addAlert("La relación se agregó correctamente", "alert-success")
    } else {
        Alerta.addAlert(data.data, "alert-danger")
    }
}

function registryClickBtnAgregarRelacion() {
    document.getElementById("btnAgregar").addEventListener("click", function (event) {
        limpiarModal();
        cambiarTitulo("Nuevo");
        registryClickBtnAceptarModal(function () {
            document.getElementById("formRelacion").removeEventListener("click", registryClickEventBtnModificar);
            send(model.actionNuevaRelacion, construirRelacionModal(), successAgregar);
        });
    });
}

function validaFormularioRelacion() {
    if (document.getElementById("IdOrNombrePuerto").value.trim() === "") {
        alert("Debe ingresar un valor en el campo Id Puerto");
        return false;
    }
    if (document.getElementById("CodigoOrNombreCentro").value.trim() === "") {
        alert("Debe ingresar un valor en el campo Código Centro");
        return false;
    }
    return true;
}

function successModificar(data) {
    $("#modal-nuevo").modal("hide");
    if (!data.error) {
        modificarFila(data);
        Alerta.addAlert("La relación se modificó correctamente", "alert-success")
    } else {
        Alerta.addAlert(data.data, "alert-danger")
    }
}

function registryClickEventBtnModificar(el) {
    el.addEventListener("click", function (event) {
        var fila = document.getElementById("relacion-" + event.target.dataset["id"]);
        cambiarTitulo("Modificar");
        setValoresModal(fila.id.replace("relacion-", ""), fila.children[0].innerText, fila.children[1].innerText, fila.children[2].innerText, fila.children[3].innerText);
        registryClickBtnAceptarModal(function () {
            send(model.actionModificarRelacion, construirRelacionModal(), successModificar);
        });
        $("#modal-nuevo").modal("show");
    });
}

function successEliminar(data) {
    if (!data.error) {
        borrarFila(data);
        Alerta.addAlert("La relación se eliminó correctamente", "alert-success")
    } else {
        Alerta.addAlert(data.data, "alert-danger")
    }
}

function registryClickEventBtnEliminar(el) {
    el.addEventListener("click", function (event) {
        if (confirm("¿Está seguro que desea eliminar esta relación?")) {
            var fila = document.getElementById("relacion-" + event.target.dataset["id"]);
            send(model.actionEliminarRelacion, construirRelacionFilaTabla(fila), successEliminar);
        }
    });
}

createAutocomplete(
    "IdOrNombrePuerto",
    model.actionGetPuerto,
    function (data, response) {
        cuentas = data.data;
        response(cuentas.map(function (item) {
            return { label: item.IdTerminal + " - " + item.Nombre, value: item.IdTerminal.trim() };
        }))
    },
    function () { return { Id: document.getElementById("IdOrNombrePuerto").value } },
    function (event, ui) {
        cuentaSeleccionada = cuentas.find(function (el) {
            return el.IdTerminal.trim() == ui.item.value.trim();
        });
        setValoresModal(cuentaSeleccionada.Id, cuentaSeleccionada.IdTerminal.trim(), cuentaSeleccionada.Nombre.trim(), "", "");
    });

function createAutocomplete(id, action, callback, fnValue, fnSelect) {
    $("#" + id).autocomplete({
        source: function (request, response) {
            $.ajax({
                url: action,
                type: "POST",
                dataType: "json",
                data: fnValue(),
                success: function (data) {
                    callback(data, response);
                }
            })
        },
        autoFocus: true,
        messages: {
            noResults: "",
            results: function (resultsCount) { }
        },
        select: fnSelect,
        search: function (event, ui) {
        }
    });
}

function selectAutocomplete() {
    setValoresModal(cuentaSeleccionada[0].Id, cuentaSeleccionada[0].IdTerminal, cuentaSeleccionada[0].NombreTerminal, cuentaSeleccionada[0].CodigoCentro, cuentaSeleccionada[0].NombreCentro);
}

function limpiarModal() {
    setValoresModal("", "", "", "", "", "");
}

function setValoresModal(Id, IdTerminal, NombreTerminal, CodigoCentro, NombreCentro) {
    document.getElementById("IdRelacion").value = Id;
    document.getElementById("IdOrNombrePuerto").value = IdTerminal.trim();
    document.getElementById("NombrePuerto").innerHTML = NombreTerminal.trim();
    document.getElementById("CodigoOrNombreCentro").value = CodigoCentro.trim();
    //document.getElementById("NombreCentro").innerHTML = NombreCentro.trim();
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
            console.log(xhr.responseText);
        }
    })
}

/*Actualizar tabla relaciones centro terminal*/
var tabla = document.getElementById("tablaRelaciones");

function getIdFila(data) {
    return "relacion-" + data.data.Id;
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
    var fila = '<tr id="relacion-' + data.Id + '">' +
            '<td>' + data.IdTerminal + '</td>' +
            '<td>' + data.NombreTerminal + '</td>' +
            '<td>' + data.CodigoCentro + '</td>' +
            '<td>' + data.NombreCentro + '</td>' +
            '<td>' +
                '<div style="display: inline-block">' +
                    '<button class="btn btn-warning px-3 btn-upd text-white" data-id="' + data.Id + '">Modificar</button>' +
                    '<button class="btn btn-danger btn-del" data-id="' + data.Id + '">Eliminar</button>' +
                '</div>' +
            '</td>' +
        '</tr>';
    return fila;
}

function modificarFila(data) {
    if (data.result) {
        var idFila = getIdFila(data);
        var fila = tabla.querySelector("#" + idFila);
        fila.cells[2].innerText = data.data.CodigoCentro;
        fila.cells[3].innerText = data.data.NombreCentro;
    }
}

function borrarFila(data) {
    if (data.result) {
        var idFila = getIdFila(data);
        var fila = tabla.querySelector("#" + idFila);
        tabla.tBodies[0].removeChild(fila)
    }
}

//Cambiar titulo modal
function cambiarTitulo(title) {
    document.getElementsByClassName("modal-title")[0].innerText = title;
}