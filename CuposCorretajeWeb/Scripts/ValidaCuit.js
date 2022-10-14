function validaCuit(cuit) {
    if (typeof (cuit) == 'undefined')
        return true;
    cuit = cuit.toString().replace(/[-_]/g, "");
    if (cuit == '')
        return true; //No estamos validando si el campo esta vacio, eso queda para el "required"
    if (cuit.length != 11)
        return false;
    else {
        var mult = [5, 4, 3, 2, 7, 6, 5, 4, 3, 2];
        var total = 0;
        for (var i = 0; i < mult.length; i++) {
            //total += parseInt(cuit[i]) * mult[i]; //no funciona en IE
            total += parseInt(cuit.charAt(i)) * mult[i];
        }
        var mod = total % 11;
        var digito = mod == 0 ? 0 : mod == 1 ? 9 : 11 - mod;
    }
    //return digito == parseInt(cuit[10]);  //no funciona en IE
    return digito == parseInt(cuit.charAt(10));
}