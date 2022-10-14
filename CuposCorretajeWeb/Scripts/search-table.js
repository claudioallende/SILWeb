var filasFiltradas = [];
var displayType = "";

function registryInputSearch(inputText, table, classSearch = "search", datatable = undefined) {
    filasFiltradas = Array.from(table.querySelectorAll("tbody tr"));
    inputText.addEventListener("keyup", function (event) {
        var value = event.target.value.toLowerCase();
        search(table, function (fila) {
            if (event.target.value === "")
                return true;
            else
                return $(fila).find(classSearch).text().toLowerCase().indexOf(value) > -1;
        });
        if (typeof datatable !== "undefined") datatable.draw();
    })
}

function registryDropdownSearch(dropdown, table, classSearch = "search", datatable = undefined) {
    filasFiltradas = Array.from(table.querySelectorAll("tbody tr"));
    dropdown.addEventListener("change", function (event) {
        var optionText = event.target.options[event.target.options.selectedIndex].innerText.toLowerCase();
        search(table, function (fila) {
            if (event.target.value === "")
                return true;
            else
                return $(fila).find(classSearch).text().toLowerCase().indexOf(optionText) > -1;
        });
        if (typeof datatable !== "undefined") datatable.draw();
    })
}

function search(table, fnCondicion) {
    Array.from(table.querySelectorAll("tbody tr")).forEach(function (el) {
        var index = filasFiltradas.indexOf(el);
        var result = fnCondicion(el);
        if (result) {
            if (index == -1) {
                filasFiltradas.push(el);
                $(el).toggle(result);
            }
        } else {
            if (index > -1) {
                filasFiltradas.splice(index, 1);
                $(el).toggle(result);
            }
        }
    });
}