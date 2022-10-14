function MensajeAlerta(beforeElement) {
    this.beforeElement = beforeElement;
    this.id = "alert_" + Math.random().toString().substring(2);
}

MensajeAlerta.prototype.showMessage = function (message, tipoAlerta) {
    let element = this.getAlert();
    if (element) {
        element.parentNode.removeChild(element);
        this.createAlert(message, tipoAlerta)
    } else {
        this.createAlert(message, tipoAlerta)
    }
}

MensajeAlerta.prototype.createAlert = function (message, tipoAlerta) {
    var htmlAlert = '<div ' + 'id="' + this.id + '" class="alert ' + tipoAlerta + ' alert-dismissible fade show" role="alert" style="margin-top:5px;">';
    htmlAlert += message;
    htmlAlert += '<button type="button" class="close" data-dismiss="alert" aria-label="Close">';
    htmlAlert += '<span aria-hidden="true">&times;</span>';
    htmlAlert += '</button>';
    htmlAlert += '</div>';
    if (this.beforeElement == undefined) {
        throw "No se encuentra definido el valor de beforeElement";
    } else {
        this.removeAllAlerts();
        this.insertBefore(htmlAlert, this.beforeElement);
    }
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

MensajeAlerta.prototype.addAlert = function (message, tipoAlerta) {
    var htmlAlert = '<div ' + 'id="' + this.id + '" class="alert ' + tipoAlerta + ' alert-dismissible fade show" role="alert" style="margin-top:5px;">';
    htmlAlert += message;
    htmlAlert += '<button type="button" class="close" data-dismiss="alert" aria-label="Close">';
    htmlAlert += '<span aria-hidden="true">&times;</span>';
    htmlAlert += '</button>';
    htmlAlert += '</div>';
    if (this.beforeElement == undefined) {
        throw "No se encuentra definido el valor de beforeElement";
    } else {
        this.removeAllAlerts();
        this.insertBefore(htmlAlert, this.beforeElement);
    }
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

MensajeAlerta.prototype.appendAlert = function (message, tipoAlerta) {
    var htmlAlert = '<div class="alert ' + tipoAlerta + ' alert-dismissible fade show" role="alert" style="margin-top:5px;">';
    htmlAlert += message;
    htmlAlert += '<button type="button" class="close" data-dismiss="alert" aria-label="Close">';
    htmlAlert += '<span aria-hidden="true">&times;</span>';
    htmlAlert += '</button>';
    htmlAlert += '</div>';
    if (this.beforeElement == undefined) {
        throw "No se encuentra definido el valor de beforeElement";
    } else {
        this.insertBefore(htmlAlert, this.beforeElement);
    }
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

MensajeAlerta.prototype.removeAllAlerts = function () {
    document.querySelectorAll(".alert").forEach(function (el) {
        el.parentNode.removeChild(el);
    });
}

MensajeAlerta.prototype.insertBefore = function (strNode, beforeNode) {
    var div = document.createElement("div");
    div.innerHTML = strNode;
    beforeNode.parentNode.insertBefore(div.firstChild, beforeNode);
}

MensajeAlerta.prototype.getAlert = function () {
    return document.getElementById(this.id)
}

MensajeAlerta.prototype.remove = function () {
    const element = this.getAlert()
    if (element)
        element.parentNode.removeChild(element)
}