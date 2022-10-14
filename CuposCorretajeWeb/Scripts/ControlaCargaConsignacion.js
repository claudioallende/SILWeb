//Construye el objeto para evaluar
function getDataValidate(data, arrInputs) {
    var obj = {};
    obj.data = data;
    obj.arrData = [];
    for (var i = 0; i < arrInputs.length; i++) {
        obj.arrData.push({ value: arrInputs[i].value, name: arrInputs[i].previousSibling.previousSibling.innerText });
    }
    return obj;
}

//Evalua que el dato este en al menos 1 campo del arrData.data
// objData puede ser un array con un unico valor o un valor
function validateDisjunctionData(objData, messageAlert) {
    var datos = objData.arrData;
    if (typeof messageAlert === "undefined") throw "Falta importar MessageAlert";
    var datoObligatorio = Array.isArray(objData.data) ? objData.data.find(function (el) { return el != "" }) : objData.data;
    if (datos != undefined) {
        for (var i = 0; i < datos.length; i++) {
            if (datoObligatorio == datos[i].value && datos[i].value != undefined && datos[i].value.trim() != "") {
                messageAlert.remove()
                return true;
            }
        }
    }
    var cuits = datos.map(function (el) { return el.name; }).reduce(function (acu, cur) { return acu + ", " + cur; });
    messageAlert.addAlert("El CUIT " + datoObligatorio + " debe estar en al menos un campo: " + cuits, "alert-danger");
    return false;
}

//Evalua que los datos esten en al menos 1 campo del arrData.data
// objData es un array de valores
function validateConjunctionData(objData, messageAlert) {
    var datos = objData.arrData;
    var datosObligatorios = objData.data; //array
    var index = 0;
    if (typeof messageAlert === "undefined") throw "Falta importar MessageAlert";
    if (datos != undefined) {
        for (var i = 0; i < datos.length; i++) {
            index = datosObligatorios.indexOf(datos[i].value);
            if (index > -1) datosObligatorios.splice(index, 1);
        }
    }
    if (datosObligatorios.length > 0) {
        var nombrecuits = datos.map(function (el) { return el.name; }).reduce(function (acu, cur) { return acu + ", " + cur; });
        var numeroscuits = datosObligatorios.reduce(function (acu, cur) { return acu + ", " + cur; });
        messageAlert.addAlert("Los CUIT " + numeroscuits + " deben estar en al menos un campo: " + nombrecuits, "alert-danger");
        return false;
    }
    return true;
}