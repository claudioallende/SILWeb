function getMercaderiaSinDestino(cuentaVendedor) {
    $.ajax({
        type: "POST",
        url: window.modelData.actionGetMercaderiaSinDestino,
        contentType: "application/json; charset=utf-8",
        dataType: "html",
        data: JSON.stringify({
            Producto: window.modelData.codigoProducto,
            Compcta: window.modelData.cuentaComprador,
            Centro: document.getElementById("CentroSeleccionado").value,
            Vendcta: cuentaVendedor
        }),
        success: llenarTablaMercaderiaSinDestino,
        error: function (msg) {
            //alert('Error');
        }
    });
}

function llenarTablaMercaderiaSinDestino(datos) {
    $('#modalMercaderiaSinDestino .modal-body').html(datos);
    $('#modalMercaderiaSinDestino').modal('show');
}