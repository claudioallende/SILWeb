// Devuelve la fecha en formato yyyy-mm-dd ajustada a la zona horaria local
function FormatearFechaLocal(fecha) {
    if (!(fecha instanceof Date)) {
        fecha = new Date(fecha);
    }

    const offsetMs = fecha.getTimezoneOffset() * 60000;
    const localDate = new Date(fecha.getTime() - offsetMs);

    return localDate.toISOString().split('T')[0];
}

// Devuelve la fecha en formato dd/mm/yyyy ajustada a la zona horaria local
function toLocalDate_ddmmyyyy(fecha) {
    if (!(fecha instanceof Date)) {
        fecha = new Date(fecha);
    }

    const offsetMs = fecha.getTimezoneOffset() * 60000;
    const localDate = new Date(fecha.getTime() - offsetMs);

    const day = String(localDate.getDate()).padStart(2, '0');
    const month = String(localDate.getMonth() + 1).padStart(2, '0');
    const year = localDate.getFullYear();

    return `${day}/${month}/${year}`;
}