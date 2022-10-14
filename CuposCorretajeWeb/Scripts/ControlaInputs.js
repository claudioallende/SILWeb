function ControlaInputs(elementos) {
    this.elements = elementos;
}

//primer parametro input independiente
//segundo parametro input dependiente del primero
ControlaInputs.prototype.habilitarInputsSiIngresoDatos = function (callbackEmptyValue) {
    var _this = this,
        $element = $(_this.elements[0]);
    $(_this.elements[1]).prop('disabled', true);
    $element.keyup(function () {
        if (this.value === "") {
            $(_this.elements[1]).prop('disabled', true);
            callbackEmptyValue();
        } else {
            $(_this.elements[1]).prop('disabled', false);
        }
    });
}

ControlaInputs.prototype.deshabilitarInputsSiIngresoDatos = function () {
    var _this = this,
        $elements = $(_this.elements);
    $elements.keyup(function () {
        if (this.value === "") {
            $elements.prop('disabled', false);
        } else {
            $elements.not(this).prop('disabled', true);
        }
    });
}

ControlaInputs.prototype.soloNumeros = function () {
    var _this = this,
        $elements = $(_this.elements);
    $elements.keydown(function (e) {
        // Allow: backspace, delete, tab, escape, enter and .
        if ($.inArray(e.keyCode, [46, 8, 9, 27, 13, 110, 190]) !== -1 ||
            // Allow: Ctrl/cmd+A
            (e.keyCode == 65 && (e.ctrlKey === true || e.metaKey === true)) ||
            // Allow: Ctrl/cmd+C
            (e.keyCode == 67 && (e.ctrlKey === true || e.metaKey === true)) ||
            // Allow: Ctrl/cmd+v
            (e.keyCode == 86 && (e.ctrlKey === true || e.metaKey === true)) ||
            // Allow: Ctrl/cmd+X
            (e.keyCode == 88 && (e.ctrlKey === true || e.metaKey === true)) ||
            // Allow: home, end, left, right
            (e.keyCode >= 35 && e.keyCode <= 39)) {
            // let it happen, don't do anything
            return;
        }
        // Ensure that it is a number and stop the keypress
        if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
            e.preventDefault();
        }
    });
}