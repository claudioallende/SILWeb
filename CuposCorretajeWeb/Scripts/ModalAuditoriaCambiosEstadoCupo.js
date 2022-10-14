//Auditoria
function handleBtnAuditoria(idCupo) {
    buscarAuditoria(idCupo);
}

function buscarAuditoria(idCupo) {
    $.ajax({
        type: "POST",
        url: model.actionGetAuditoriaCupo,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify({
            IdCupo: idCupo
        }),
        success: successAjaxAuditoria,
        error: function (msg) {
        }
    });
}

function successAjaxAuditoria(auditorias) {
    $('#modalAuditoria').modal('show');
    agregarFilasTablaAuditoria(auditorias);
}

function agregarFilasTablaAuditoria(auditoriasCupos) {
    var bodyTablaAuditoria = model.tablaModalAuditoria;
    var filas = "";
    auditoriasCupos.forEach(function (auditoria) {
        filas += crearFilaAuditoria(auditoria);
    });
    bodyTablaAuditoria.innerHTML = filas;
}

function crearFilaAuditoria(auditoriaCupo) {
    var fila = '';
    fila += '<tr>';
    fila += '<td>' + auditoriaCupo.Usuario + '</td>';
    fila += '<td>' + auditoriaCupo.Operacion + '</td>';
    fila += '<td class="text-center">' + auditoriaCupo.Fecha + '</td>';
    fila += '<td class="text-center">' + auditoriaCupo.Hora + '</td>';
    fila += '</tr>';
    return fila;
}
//!Auditoria