//en model.Lote se guardan los vendedores a informar.
//La vista se actualiza de acuerdo a lo que esté en este array, por lo que si se quiere no actualizar alguno se debe quitar de la lista
//y luego llamar a actualizarVista
//model.Lote = [];
//model.ChecksAvailable = [];

model.Estados = new ObservableArray(model.estado);

//Deshabilita el check si no Informa
function ChecksObserver(data) {
    var checks = document.getElementsByClassName("check_" + data.CuentaVendedor);
    for (var i = 0; i < checks.length; i++) {
        if (data.Informa) {
            for (var i = 0; i < checks.length; i++) {
                checks[i].checked = data.ParaInformar;
            }
        } else {
            for (var i = 0; i < checks.length; i++) {
                checks[i].disabled = true;
                checks[i].checked = false;
            }
        }
    }
}

//Tilda el check SeleccionaTodo en caso de que todos los checks que informan esten tildados.
function CheckSeleccionarTodoObserver(data) {
    var allParaInformar = model.Estados.array.filter(function (el) { return el.Informa; });
    var inputsChecked = model.Estados.array.filter(function (el) { return el.Informa && el.ParaInformar; });
    var checks = document.getElementsByName("seleccionaTodo");
    if (allParaInformar != undefined && allParaInformar.length != 0 && allParaInformar.length == inputsChecked.length) {
        for (var i = 0; i < checks.length; i++) {
            checks[i].checked = true;
        }
    } else {
        for (var i = 0; i < checks.length; i++) {
            checks[i].checked = false;
        }
    }
}

function ButtonsObserver(data) {
    var btns = document.getElementsByClassName("btn_" + data.CuentaVendedor);
    if (!data.Informa) {
        for (var i = 0; i < btns.length; i++) {
            btns[i].disabled = true;
        }
    }
}

function ButtonEnviarObserver(data) {
    var btn = document.getElementById("btnEnviar");
    var paraInformar = model.Estados.array.filter(function (el) { return el.Informa && el.ParaInformar; });
    if (paraInformar.length) {
        btn.disabled = false;
    } else {
        btn.disabled = true;
    }
}

model.Estados.subscribe(ChecksObserver);
model.Estados.subscribe(CheckSeleccionarTodoObserver);
model.Estados.subscribe(ButtonsObserver);
model.Estados.subscribe(ButtonEnviarObserver);

function handleEventBtnInformar(btn) {
    enviarLoteParaInformar([newLote(btn.dataset.vendedor)]);
}

function handleEventCheckInformar(check) {
    model.Estados.notify(changeParaInformar(check.value))
}

function handleEventBtnEnviar() {
    var lote = buildAllLote();
    enviarLoteParaInformar(lote);
}

function handleEventCheckSeleccionarTodo() {
    if (document.getElementById("seleccionaTodo").checked) {
        checkAll();
    } else {
        uncheckAll();
    }
}

function getEstado(CuentaVendedor) {
    return model.Estados.array.find(function (el) { return el.CuentaVendedor == CuentaVendedor });
}

function changeParaInformar(CuentaVendedor) {
    var el = getEstado(CuentaVendedor);
    if (el.Informa) el.ParaInformar = !el.ParaInformar;
    return el;
}

function changeInforma(CuentaVendedor) {
    var el = getEstado(CuentaVendedor);
    el.Informa = !el.Informa;
    return el;
}

function checkAll() {
    var paraInformar = model.Estados.array.filter(function (el) { return el.Informa && !el.ParaInformar; });
    var checksNoSeleccionados = paraInformar.map(function (el) {
        return document.getElementById("check_" + el.CuentaVendedor);
    });
    checksNoSeleccionados.forEach(function (el) {
        el.checked = true;
        handleEventCheckInformar(el);
    })
}

function uncheckAll() {
    var paraInformar = model.Estados.array.filter(function (el) { return el.Informa && el.ParaInformar; });
    var checksNoSeleccionados = paraInformar.map(function (el) {
        return document.getElementById("check_" + el.CuentaVendedor);
    });
    checksNoSeleccionados.forEach(function (el) {
        el.checked = false;
        handleEventCheckInformar(el);
    })
}

function buildAllLote() {
    var lotes = model.Estados.array.filter(function (estado) {
        return estado.ParaInformar;
    }).map(function (estado) {
        return newLote(estado.CuentaVendedor);
    });
    return lotes;
}

function newLote(vendedor) {
    return {
        CodigoGrano: model.Grano,
        CuentaComprador: model.Comprador,
        CuentaPuerto: model.Puerto,
        CuentaVendedor: vendedor,
        CodigoCentro: model.CentroOrigen,
        CodigoCentroDistribucion: model.CentroDistribucion,
        Cyo: model.Cyo
    };
}

function enviarLoteParaInformar(lote) {
    $.ajax({
        type: "POST",
        url: window.model.actionInformaPorLote,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify({ Lote: lote }),
        success: function (data) {
            mostrarMensajeYActualizarVista(data);
        },
        error: function (msg) {
            mensajeError();
        }
    });
}

function mostrarMensajeYActualizarVista(cuposResult) {
    if (cuposResult.result.find(function (el) { return el.Estado == 2 }) != undefined) {
        mensajeError();
        return;
    }
    if (cuposResult.result.find(function (el) { return el.Estado == 1 }) != undefined) {
        mensajeFaltaCorreo();
        return;
    }
    updateModel(cuposResult.result);
    updateTableAnulacion(cuposResult.model);
    mensajeSeEnvioCorrectamente();
}

function updateModel(result) {
    //Obtiene los estados que deben ser actualizados segun lo que responde el servidor
    var estadosActualizar = model.Estados.array.filter(function (estado) {
        return result.find(function (itemResult) {
            return estado.CuentaVendedor == itemResult.CuentaVendedor;
        });
    });
    //Cambia el valor Informa de los estados y notifica a los observadores para que deshabiliten
    estadosActualizar.map(function (element) {
        model.Estados.notify(changeInforma(element.CuentaVendedor));
    });
}

function updateTableAnulacion(cambios) {
    //Crea fila nueva con Vendedor = 0 si es que no existe en la tabla
    agregarVendedorVacioSiNoExiste(cambios);
    //Actualiza los valores de los dias de la tabla
    actualizarValoresTabla(cambios);
    //Elimina fila en caso de que quede con 0 cupos
    eliminarFilaVacia(cambios);
    datatables.draw();
}

function agregarVendedorVacioSiNoExiste(cambios) {
    //Crea fila nueva con Vendedor = 0 si es que no existe en la tabla
    if (!document.querySelectorAll("tr[data-vendedor='0']").length) {
        var vendedor0 = cambios.find(function (fila) { return fila.VendCta == 0; });
        if (vendedor0 != undefined) {
            datatables.row.add(crearFilaSinVendedor(vendedor0));
        }
    }
}

function actualizarValoresTabla(cambios) {
    //Actualiza los valores de los dias de la tabla
    var fila;
    var tabla = document.getElementById("tabla_detalle");
    cambios.map(function (cambio) {
        fila = tabla.querySelectorAll("tr[data-vendedor='" + cambio.VendCta + "']")[0];
        if (fila != undefined) {
            repetirCantidadDias(function (dia) {
                fila.querySelectorAll("td[data-dia='CR" + dia + "']")[0].innerText = cambio["D" + dia + "Cr"];
                fila.querySelectorAll("td[data-dia='CO" + dia + "']")[0].innerText = cambio["D" + dia + "Co"];
            });
        }
    });
}

function eliminarFilaVacia(cambios) {
    var filas = [].slice.call(document.querySelectorAll("#tabla_detalle tbody tr"));
    filas.forEach(function (fila) {
        if (!cambios.find(function (el) { return fila.dataset.vendedor == el.VendCta; })) {
            datatables.row(fila).remove();
        }
    });
}

function mensajeError() {
    addAlert("No se pudo enviar el PDF correctamente", "alert-danger", document.getElementsByClassName("row")[0]);
}

function mensajeFaltaCorreo() {
    addAlert("No se enviaron algunos de los PDF debido a que faltan configurar los destinatarios del correo electrónico", "alert-primary", document.getElementsByClassName("row")[0]);
}

function mensajeSeEnvioCorrectamente() {
    addAlert("Se envió el PDF correctamente", "alert-success", document.getElementsByClassName("row")[0]);
}

function crearFilaSinVendedor(row) {
    var fila = "<tr>";
    fila += "<td>" + "</td>";
    fila += crearCeldasDias(row);
    fila += '<td class="text-center border-left">';
    fila += '<a href="' + model.actionEditar + '/' + formarId(row) + '" class="btn btn-primary btn-sm" title="Editar">';
    fila += '<span class="oi oi-list"></span>';
    fila += '</a>';
    fila += '<a href="' + model.actionDistribucion + '/' + formarId(row) + '" class="btn btn-warning btn-sm" title="Distribución">';
    fila += '<span class="oi oi-transfer" style="color:white"></span>';
    fila += '</a>';
    fila += '<button id="btnInforma_0" data-href="" class="btn btn-indigo btn-sm btn--informar disabled" title="Informar" data-check="check_0">';
    fila += '<span class="oi oi-envelope-closed"></span>';
    fila += '</button>';
    fila += "</td>";
    fila += '<td>';
    fila += '<div class="align-self-center">';
    fila += '<div class="custom-control custom-checkbox">';
    fila += '<input type="checkbox" name="check-informa" id="check_0" class="custom-control-input" value="0" disabled />';
    fila += '<label class="custom-control-label" for="check_0"></label>';
    fila += '</div>';
    fila += '</div>';
    fila += '</td>';
    fila += '</tr>';
    return createElementFromHTML(fila);
}

function createElementFromHTML(htmlString) {
    var table = document.createElement('table');
    table.innerHTML = htmlString.trim();

    return table.querySelectorAll("tbody tr")[0];
}

function crearCeldasDias(filas) {
    return repetirCantidadDias(function (dia) {
        celdas = '<td class="text-center border-left cupos_recibidos">' + filas["D" + dia + "Cr"] + "</td>";
        celdas += '<td class="text-center cupos_otorgados">' + filas["D" + dia + "Co"] + "</td>";
        return celdas;
    });
}

function formarId(fila) {
    var url = model.Comprador + "-" + "0" + "-" + model.Puerto + "-" + model.Grano + "?";
    if (model.Cyo)
        url += "cyo=" + model.Cyo + "&";
    url += "centroorigen=" + model.CentroOrigen + "&centrodistribucion=" + model.CentroDistribucion;
    return url;
}

function repetirCantidadDias(fnRepetir) {
    var result = "";
    for (var dia = 0; dia <= 20; dia++) {
        result += fnRepetir(dia);
    }
    return result;
}