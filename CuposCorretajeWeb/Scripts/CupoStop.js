function CupoNominado(Cupo, MensajeAlerta) {
    this.CupoInicial = Cupo;
    if (typeof MensajeAlerta !== "object") {
        console.log("Alert no es una funcion, no se podran visualizar los mensajes de error en la pantalla.");
    } else {
        this.MensajeAlerta = MensajeAlerta;
    }
}

CupoNominado.prototype.isValid = function () {
    this.MensajeAlerta.removeAllAlerts();
    if (typeof getCupo === "function") {
        var CupoNuevo = getCupo();
    } else {
        throw ExceptionInformation("Funcion getCupo no definida");
    }
    return this.validateConsignacion(CupoNuevo);
}

CupoNominado.prototype.validateConsignacion = function (cupo) {
    return this.validateSolicitanteIntermediarioRemitente(cupo) && validateCorredor(cupo, this);
}

CupoNominado.prototype.validateSolicitanteIntermediarioRemitente = function (cupo) {
    var consignacionesEvaluar = [this.CupoInicial.Consignacion.Cuitsolicitante, this.CupoInicial.Consignacion.Cuitintermediario, this.CupoInicial.Consignacion.Cuitrtecomercial, this.CupoInicial.Consignacion.Cuitmat];

    return validateConjunctionData(
        getDataValidate(
            removeDuplicatedAndEmptyItemsArray(consignacionesEvaluar),
            [document.getElementById("Cuitsolicitante"), document.getElementById("Cuitintermediario"), document.getElementById("Cuitrtecomercial"), document.getElementById("Cuitmat")]
        ),
        this.MensajeAlerta
    );
}

CupoNominado.prototype.showErrorMessage = function (message) {
    try {
        if (message == "" || message == undefined)
            this.MensajeAlerta.addAlert("Ha ocurrido un error", "alert-danger");
        else
            this.MensajeAlerta.addAlert(message, "alert-danger");
    } catch (e) { console.log(e); }
}

function CupoSinNominar(Cupo, MensajeAlerta) {
    this.CupoInicial = Cupo;
    if (typeof MensajeAlerta !== "object") {
        console.log("Alert no es una funcion, no se podran visualizar los mensajes de error en la pantalla.");
    } else {
        this.MensajeAlerta = MensajeAlerta;
    }
}

CupoSinNominar.prototype.isValid = function () {
    this.MensajeAlerta.removeAllAlerts();
    if (typeof getCupo === "function") {
        var CupoNuevo = getCupo();
    } else {
        throw ExceptionInformation("Funcion getCupo no definida");
    }
    return validateCorredor(CupoNuevo, this);
}

CupoSinNominar.prototype.showErrorMessage = function (message) {
    try {
        if (message == "" || message == undefined)
            this.MensajeAlerta.addAlert("Ha ocurrido un error", "alert-danger");
        else
            this.MensajeAlerta.addAlert(message, "alert-danger");
    } catch (e) { console.log(e); }
}

function getTipoCupos(CupoOriginal, mensajeAlerta) {
    if (CupoOriginal.Consignacion.Cuitsolicitante != "" || CupoOriginal.Consignacion.Cuitsolicitante != "" || CupoOriginal.Consignacion.Cuitsolicitante != "")
        return new CupoNominado(CupoOriginal, mensajeAlerta);
    else
        return new CupoSinNominar(CupoOriginal, mensajeAlerta);
}

function isCorredorNotEmpty(cupoInicial) {
    return (cupoInicial.Consignacion.Cuitcorrvta != undefined && cupoInicial.Consignacion.Cuitcorrvta != "") || (cupoInicial.Consignacion.Cuitcorrcomp != undefined && cupoInicial.Consignacion.Cuitcorrcomp != "");
}

function validateCorredor(cupo, tipoCupo) {
    if (isCorredorNotEmpty(tipoCupo.CupoInicial)) {
        return validateDisjunctionData(getDataValidate(
            getCuitCorredorNotEmpty(tipoCupo.CupoInicial), [document.getElementById("Cuitcorrvta"), document.getElementById("Cuitcorrcomp")]),
            tipoCupo.MensajeAlerta
        );
    }
    return true;
}

function getCuitCorredorNotEmpty(cupoInicial) {
    if (cupoInicial.Consignacion.Cuitcorrvta != undefined && cupoInicial.Consignacion.Cuitcorrvta != "") {
        return cupoInicial.Consignacion.Cuitcorrvta;
    }
    if (cupoInicial.Consignacion.Cuitcorrcomp != undefined && cupoInicial.Consignacion.Cuitcorrcomp != "") {
        return cupoInicial.Consignacion.Cuitcorrcomp;
    }
    return null;
}

function removeDuplicatedAndEmptyItemsArray(arr) {
    return arr.filter(function(item, pos) {
        return arr.indexOf(item) == pos && item != "";
    })
}

function isUniqueInArray(arr) {
    return removeDuplicatedAndEmptyItemsArray(arr).length === 1;
}