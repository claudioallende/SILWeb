consultarCuposPendientesAutorizarStop();

setInterval(function () { consultarCuposPendientesAutorizarStop() }, 600000);

function consultarCuposPendientesAutorizarStop() {
    $.ajax({
        url: model_layout.actionExistenPendietes,
        type: "POST",
        dataType: "json",
        success: function (data) {
            mostrarNotificacionCuposPendientesAutorizarStop(data)
        },
        error: function (msg) {
            console.log(msg);
        }
    });
}

function mostrarNotificacionCuposPendientesAutorizarStop(mostrar) {
    var notificaciones = document.getElementsByClassName("notification_counter");
    if (mostrar) {
        for (var i = 0; i < notificaciones.length; i++) {
            notificaciones[i].style.display = "block";
        }
    } else {
        for (var i = 0; i < notificaciones.length; i++) {
            notificaciones[i].style.display = "none";
        }
    }
    //function disponible para que demas paginas puedan saber que hubo una notificacion de pendietes de autorizar
    if (typeof notificarPendientesAutorizar === "function") notificarPendientesAutorizar(mostrar);
}