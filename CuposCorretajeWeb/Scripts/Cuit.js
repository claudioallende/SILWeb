function agregoGuionesIECompatibilidad(cuit) {
    if (cuit != null && cuit != "") {
        if (cuit.charAt(2) != "-" && cuit.charAt(10) != "-") /*no tiene los guiones*/ {
            cuit = cuit.substring(0, 2) + "-" + cuit.substring(2, 10) + "-" + cuit.substring(10, cuit.length);
        } else {
            if (cuit.charAt(2) != "-") {
                cuit = cuit.substring(0, 2) + "-" + cuit.substring(2, 10) + cuit.substring(10, cuit.length);
            }
            if (cuit.charAt(11) != "-") {
                cuit = cuit.substring(0, 2) + cuit.substring(2, 11) + "-" + cuit.substring(11, cuit.length);
            }
        }
    }
    return cuit;
}