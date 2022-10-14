function Spinner(btn, opciones){
    this.boton = btn;

    defaults = {
        ubicarSpinner: this.agregarSpinnerIzquierda,
        boton: this.boton
    }

    this.opciones = $.extend({}, defaults, opciones);

    this.init();
}

Spinner.prototype.init = function () {
    this.opciones.ubicarSpinner();
}

Spinner.prototype.agregarSpinnerIzquierda = function(){
    $(this.boton).addClass("ld-ext-left");
    $(this.boton).prepend('<span class="ld ld-ring ld-spin"></span>');
}

Spinner.prototype.mostrarSpinner = function () {
    $(this.boton).addClass("running");
}

Spinner.prototype.ocultarSpinner = function () {
    $(this.boton).removeClass("running");
}

Spinner.prototype.mostrarSpinnerEnEventoBoton = function (nombreEvento) {
    var _this = this;
    this.opciones.boton.addEventListener(nombreEvento, function () {
        _this.mostrarSpinner();
    });
}

//BasicSpinner es para cualquier elemento y no tiene efectos.
function BasicSpinner(el, opciones) {
    this.elemento = el;

    if (this.elemento == undefined || this.elemento == null) throw "La instancia de BasicSpinner requiere un elemento como parámetro";

    defaults = {
        ubicarSpinner: this.ubicarSpinner,
        ubicacion: 'center'
    }

    this.opciones = $.extend({}, defaults, opciones);

    this.init();
}

BasicSpinner.prototype.init = function () {
    this.opciones.ubicarSpinner();
}

BasicSpinner.prototype.ubicarSpinner = function () {
    switch (this.opciones.ubicacion) {
        case 'center':
            this.agregarSpinnerCentro();
            break;
    }
}

BasicSpinner.prototype.agregarSpinnerCentro = function () {
    $(this.elemento).prepend('<div class="d-flex justify-content-center">');
    $(this.elemento).prepend('<div class="spinner-border" role="status">');
    $(this.elemento).prepend('<span class="sr-only">Loading...</span>');
    $(this.elemento).prepend('</div>');
    $(this.elemento).prepend('</div>');
}

BasicSpinner.prototype.agregarSpinnerIzquierda = function () {
}

BasicSpinner.prototype.mostrarSpinner = function () {
    $(this.boton).addClass("running");
}

BasicSpinner.prototype.ocultarSpinner = function () {
    $(this.boton).removeClass("running");
}