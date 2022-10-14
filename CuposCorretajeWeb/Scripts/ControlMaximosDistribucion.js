function EstadoGrilla(datatable) {
    this.CantidadDias = 20;
    this.init(datatable);
}

EstadoGrilla.prototype.init = function (datatable) {
    var _this = this;
    _this.estadoInicial = this.getEstado();
    _this.totalPorDiaEstadoInicial = _this.getSumaColumnasMatriz(_this.estadoInicial);
    _this.modificacionesDelEstadoInicial = [];
    _this.modificacionesDelEstadoInicialPorVendedor = [];
    _this.inicializarModificacionesDelEstado();
    _this.handleEvents(datatable);
    _this.getDiferenciasYValidar({
        mensajeErrorCuposContrato: function () {
            _this.informarVendedorNoValidado();
        }
    });
}

EstadoGrilla.prototype.inicializarModificacionesDelEstado = function () {
    this.modificacionesDelEstadoInicial.totalPorDia = new Array(this.CantidadDias + 1).fill(0);//[0, 0, 0, 0, 0];
}

EstadoGrilla.prototype.setEstadoInicial = function (estado) {
    this.estadoInicial = estado;
    this.totalPorDiaEstadoInicial = this.getSumaColumnasMatriz(this.estadoInicial);
}

EstadoGrilla.prototype.handleEvents = function (datatable) {
    var _this = this;
    $(".dia").keyup(function (e) {
        _this.getDiferenciasYValidar()
        _this.setTotalPorDia();
    });
    $("#btnAceptar").click(function (e) {
        if (!_this.getDiferenciasYValidar() || !control_destinatario()) {
            e.preventDefault();
            e.stopImmediatePropagation();
        }
    });
}

EstadoGrilla.prototype.setTotalPorDia = function (functionAtEnd) {
    for (var i = 0; i <= this.CantidadDias; i++) {
        //Despues de utilizar DataTables se clona el footer y oculta el original. El que se muestra está dentro de un div de id = dataTables_scrollFoot
        $(".dataTables_scrollFoot .total-dia" + i).html(this.totalPorDiaEstadoInicial[i] + this.modificacionesDelEstadoInicial.totalPorDia[i]); 
    }
    if (typeof functionAtEnd == "function") functionAtEnd();
}

EstadoGrilla.prototype.getDiferencias = function () {
    var estadoActual = this.getEstado();
    var matrizDiferencias = [];
    var arrayDiferencias = [];
    var arraySumatoriaColumna = new Array(this.CantidadDias + 1).fill(0);//[0, 0, 0, 0, 0, 0];
    var arraySumatoriaFila = [];
    var sumatoria;
    var total = {};
    var _this = this;
    estadoActual.forEach(function (array, nroFila) {
        sumatoria = 0;
        arrayDiferencias = [];
        array.forEach(function (val, nroColumna) {
            if (val == "" || val == null || val == NaN) val = 0;
            arrayDiferencias.push(val - _this.estadoInicial[nroFila][nroColumna]);
            sumatoria += val;// - _this.estadoInicial[nroFila][nroColumna];
            arraySumatoriaColumna[nroColumna] += val - _this.estadoInicial[nroFila][nroColumna];
        });
        arraySumatoriaFila.push(sumatoria);
        matrizDiferencias.push(arrayDiferencias);
    });
    total.matrizDiferencias = matrizDiferencias;
    total.totalPorDia = arraySumatoriaColumna;
    total.totalPorContrato = arraySumatoriaFila;
    return total;
}

EstadoGrilla.prototype.clearVendedoresNoValidados = function() {
    this.modificacionesDelEstadoInicialPorVendedor.forEach(function (fila) {
        var filasTabla = document.querySelectorAll("tr[data-vendedor='" + fila.dataset.vendedor + "']");//.classList.add("table-danger");
        [].forEach.call(filasTabla, function (filaTabla) {
            filaTabla.classList.remove("table-danger");
        });
        //fila.classList.remove("table-danger");
    });
    this.modificacionesDelEstadoInicialPorVendedor = [];
}

EstadoGrilla.prototype.informarVendedorNoValidado = function() {
    if (document.getElementById("EntregaHasta").value.trim() === "") {
        this.marcarVendedoresNoValidados();
        addAlert("Los cupos distribuidos superan los Totales a Distribuir.", "alert-danger", document.getElementsByClassName("container_table_distribucion")[0]);
    } else {
        this.sugerirQuitarFiltro();
    }
}

EstadoGrilla.prototype.sugerirQuitarFiltro = function() {
    addAlert("Los cupos distribuidos superan los Totales a Distribuir. Por favor pruebe quitando los filtros Entrega Desde y Entrega Hasta", "alert-danger", document.getElementsByClassName("container_table_distribucion")[0]);
}

EstadoGrilla.prototype.setVendedoresNoValidadosEInformar = function(fila) {
    this.setVendedoresNoValidados(fila);
    this.informarVendedorNoValidado();
    //this.marcarVendedoresNoValidados();
}

EstadoGrilla.prototype.setVendedoresNoValidados = function(fila) {
    this.modificacionesDelEstadoInicialPorVendedor.push(fila);
}

EstadoGrilla.prototype.getVendedoresNoValidados = function () {
    return this.modificacionesDelEstadoInicialPorVendedor;
}

EstadoGrilla.prototype.marcarVendedoresNoValidados = function() {
    this.getVendedoresNoValidados().forEach(function (fila) {
        var filasTabla = document.querySelectorAll("tr[data-vendedor='" + fila.dataset.vendedor + "']");//.classList.add("table-danger");
        [].forEach.call(filasTabla, function (filaTabla) {
            filaTabla.classList.add("table-danger")
        });
        //fila.classList.add("table-danger");
    });
}

EstadoGrilla.prototype.getSumaColumnasMatriz = function (matriz) {
    result = matriz.reduce(function (r, a) {
        a.forEach(function (b, i) {
            r[i] = (r[i] || 0) + b;
        });
        return r;
    }, []);
    return result;
}

EstadoGrilla.prototype.getEstado = function () {
    var $filas = $("#TablaDistribuciones .cupos-disponibles").closest("tr");
    var matriz = [];
    $filas.each(function (i, fila) {
        matriz.push(
            $(fila).find(".dia").map(function () {
                if (this.innerText.trim() == "") {
                    result = 0;
                } else {
                    result = this.innerText;
                }
                return parseInt(result);
            })
            .toArray()
        );
    });
    return matriz;
}

EstadoGrilla.prototype.getDiferenciasYValidar = function (mensajeError) {
    var _this = this;
    _this.modificacionesDelEstadoInicial = _this.getDiferencias();
    return _this.validar(mensajeError);
}

EstadoGrilla.prototype.validar = function (mensajeNoValida) {
    if (!this.validarCuposDisponiblesDia()) {
        if (mensajeNoValida !== undefined && typeof mensajeNoValida.mensajeErrorCuposDia === "function") {
            mensajeNoValida.mensajeErrorCuposDia();
        } else {
            addAlert('Cantidad de cupos excedidos', "alert-danger", document.getElementsByClassName("container_table_distribucion")[0]);
        }
        return false;
    }
    if (!this.validarCuposDisponiblesContrato()) {
        if (mensajeNoValida !== undefined && typeof mensajeNoValida.mensajeErrorCuposContrato === "function") {
            mensajeNoValida.mensajeErrorCuposContrato();
        } else {
            addAlert('Cantidad de cupos excedidos para la consignación seleccionada', "alert-danger", document.getElementsByClassName("container_table_distribucion")[0]);
        }
        return false;
    }
    $(".container_table_distribucion").prev(".alert").remove();
    return true;
}

EstadoGrilla.prototype.validarCuposDisponiblesDia = function () {
    var $columnas = $("#TablaDistribuciones .col-dia");
    var result = true;
    var _this = this;
    $columnas.each(function (i, columna) {
        if (parseInt(columna.innerText) < _this.modificacionesDelEstadoInicial.totalPorDia[i]) {
            result = false;
            return result;
        }
    });
    return result;
}

EstadoGrilla.prototype.validarCuposDisponiblesContrato = function() {
    var $cuposADistr = $("#TablaDistribuciones .cupos-disponibles");
    var cantidadCuposADistr = 0;
    var $filas = $cuposADistr.closest("tr");
    var result = true;
    var _this = this;
    _this.clearVendedoresNoValidados();
    $filas.each(function (i, fila) {
        cantidadCuposADistr = parseInt($(fila).find(".cupos-disponibles").text());
        if (cantidadCuposADistr < _this.modificacionesDelEstadoInicial.totalPorContrato[i]) {
            result = false;
            _this.setVendedoresNoValidadosEInformar(fila);
            //return result;
        }
    });
    return result;
}

EstadoGrilla.prototype.getObjetoDistribucion = function () {
    var objeto;
    var listaDeObjetos = [];
    var _this = this;
    this.modificacionesDelEstadoInicial.matrizDiferencias.forEach(function (fila, index) {
        tr = document.getElementById("TablaDistribuciones").rows[index + 2]; //index + 2 porque empieza por el titulo
        objeto = {};
        objeto.Compcta = cupo.compcta,
        objeto.Vendcta = $(tr).data('vendedor'),
        objeto.Codproducto = cupo.grano,
        objeto.Cosecha = $(tr).data('cosecha'),
        objeto.Ctadestino = $(tr).data('destino'),
        objeto.Centro = $('#CentroSeleccionado').val(),
        objeto.Fechaent = $(tr).data('fecha')
        for (var i = 0; i <= _this.CantidadDias; i++) {
            objeto["Dia" + i] = fila[i];
        }
        //objeto.Dia0 = fila[0];
        //objeto.Dia1 = fila[1];
        //objeto.Dia2 = fila[2];
        //objeto.Dia3 = fila[3];
        //objeto.Dia4 = fila[4];
        //objeto.Dia5 = fila[5];
        listaDeObjetos.push(objeto);
    });
    return listaDeObjetos;
}