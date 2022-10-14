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
    var selectPuertoSil = document.getElementById("CodigoPuertoSil");
    var selectPuertoStop = document.getElementById("CodigoPuertoStop");

    return {
        Id: document.getElementById("Id").value,
        NroPuertoSTOP: document.getElementById("CodigoPuertoStop").value,
        nombrePuertoSTOP: $(selectPuertoStop).next().find('.nombre-cuenta').html(),
        NroPuertoSIL: document.getElementById("CodigoPuertoSil").value,
        nombrePuertoSil: $(selectPuertoSil).next().find('.nombre-cuenta').html(),
        ValorPorDefecto: document.getElementById("ValorPorDefecto").checked ? 1 : 0
    }
}

function construirRelacionFilaTabla(fila) {
    return {
        Id: fila.id.replace("relacion-", ""),
        NroPuertoSTOP: fila.children[0].innerText,
        nombrePuertoSTOP: fila.children[1].innerText,
        NroPuertoSIL: fila.children[2].innerText,
        nombrePuertoSil: fila.children[3].innerText,
        ValorPorDefecto: fila.dataset.valorDefecto
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
        setValorPorDefecto(false);
        registryClickBtnAceptarModal(function () {
            document.getElementById("formRelacion").removeEventListener("click", registryClickEventBtnModificar);
            send(model.actionNuevaRelacion, construirRelacionModal(), successAgregar);
        });
    });
}

function validaFormularioRelacion() {
    if (document.getElementById("CodigoPuertoStop").value.trim() === "") {
        alert("Debe ingresar un valor en el campo Código STOP");
        return false;
    }
    if (document.getElementById("CodigoPuertoSil").value.trim() === "") {
        alert("Debe ingresar un valor en el campo Código SIL");
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
        setValoresModal(fila.id.replace("relacion-", ""), fila.children[0].innerText, fila.children[1].innerText, fila.children[2].innerText, fila.children[3].innerText);
        cambiarTitulo("Modificar");
        evalValorPorDefecto(fila);
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
            if (!esValorPorDefecto(fila)) {
                send(model.actionEliminarRelacion, construirRelacionFilaTabla(fila), successEliminar);
            } else {
                Alerta.addAlert("Esta relación está asignada como valor por defecto. Por favor seleccione a otra relación como valor por defecto antes de eliminar esta.", "alert-danger");
            }
        }
    });
}

function limpiarModal() {
    setValoresModal("", "", "", "", "");
}

function setValoresModal(Id, CodigoPuertoStop, NombrePuertoStop, CodigoPuertoSil, NombrePuertoSil) {
    document.getElementById("Id").value = Id;
    document.getElementById("CodigoPuertoStop").value = CodigoPuertoStop.trim();
    document.getElementById("NombrePuertoStopcta").innerText = NombrePuertoStop.trim();
    document.getElementById("CodigoPuertoSil").value = CodigoPuertoSil.trim();
    document.getElementById("NombrePuertocta").innerText = NombrePuertoSil.trim();
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
    var fila = '<tr id="relacion-' + data.Id + '" data-valor-defecto="' + data.ValorDefecto + '">' +
        '<td>' + data.NroPuertoSTOP + '</td>' +
        '<td>' + data.nombrePuertoSTOP + '</td>' +
        '<td>' + data.NroPuertoSIL + '</td>' +
        '<td>' + data.nombrePuertoSil + '</td>' +
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
        fila.cells[0].innerText = data.data.NroPuertoSTOP;
        fila.cells[1].innerText = data.data.nombrePuertoSTOP;
        fila.cells[2].innerText = data.data.NroPuertoSIL;
        fila.cells[3].innerText = data.data.nombrePuertoSil;
        fila.dataset.valorDefecto = data.data.ValorPorDefecto;
    }
}

function borrarFila(data) {
    if (data.result) {
        var idFila = getIdFila(data);
        var fila = tabla.querySelector("#" + idFila);
        tabla.tBodies[0].removeChild(fila)
    }
}

//Valor por defecto
function evalValorPorDefecto(fila) {
    setValorPorDefecto(esValorPorDefecto(fila));
}

function setValorPorDefecto(valor) {
    document.getElementById("ValorPorDefecto").checked = valor;
    document.getElementById("ValorPorDefecto").disabled = valor;
}

function esValorPorDefecto(fila) {
    return fila.dataset.valorDefecto == "1";
}

//Cambiar titulo modal
function cambiarTitulo(title) {
    document.getElementsByClassName("modal-title")[0].innerText = title;
}

$("#CodigoPuertoSil").autocomplete({
    source: function (request, response) {
        $.ajax({
            url: window.model.actionGetPuertoSil,
            type: "POST",
            dataType: "json",
            contentType: "application/json",
            data: JSON.stringify({ Texto: request.term }),
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

$("#CodigoPuertoStop").autocomplete({
    source: function (request, response) {
        $.ajax({
            url: window.model.actionGetPuertoStop,
            type: "POST",
            dataType: "json",
            contentType: "application/json",
            data: JSON.stringify({ Texto: request.term }),
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
        borrarNombre(el);
    }
}

function handleChangeAutocompleteCuenta(inputText, item) {
    if (item != null) {
        setearNombre(item.label, inputText);
    }
}

function setearNombre(label, inputText) {
    nombre = label?.substring(label.indexOf('-') + 2, label.length);
    $(inputText).next().find('.nombre-cuenta').html(nombre);
}

function borrarNombre(el) {
    $(el).next().find('.nombre-cuenta').html('');
}