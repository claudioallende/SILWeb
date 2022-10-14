function DataTablesTabulacion(fixed_table_left, fixed_table_right, div_scroll) {
    this.fixed_table_left = fixed_table_left;
    this.fixed_table_right = fixed_table_right;
    this.div_scroll = div_scroll;
    this.init();
}
//var fixed_table_left = document.getElementsByClassName("DTFC_LeftWrapper")[0];
//var fixed_table_right = document.getElementsByClassName("DTFC_RightWrapper")[0];
DataTablesTabulacion.prototype.init = function () {
    this.limiteIzquierdo = this.obtenerPosicionDerecha(this.fixed_table_left);
    this.limiteDerecho = this.obtenerPosicionIzquierda(this.fixed_table_right);
}
DataTablesTabulacion.prototype.obtenerPosicionElemento = function (elemento) {
    return elemento.getBoundingClientRect();
}
DataTablesTabulacion.prototype.obtenerPosicionIzquierda = function (elemento) {
    return this.obtenerPosicionElemento(elemento).left;
}
DataTablesTabulacion.prototype.obtenerPosicionDerecha = function (elemento) {
    return this.obtenerPosicionElemento(elemento).right;
}
DataTablesTabulacion.prototype.obtenerLimiteIzquierdo = function () {
    return this.limiteIzquierdo;
}
DataTablesTabulacion.prototype.obtenerLimiteDerecho = function () {
    return this.limiteDerecho;
}
DataTablesTabulacion.prototype.estaFueraLimiteIzquierdo = function (elemento) {
    return (this.obtenerPosicionIzquierda(elemento) < this.obtenerLimiteIzquierdo());
}
DataTablesTabulacion.prototype.estaFueraLimiteDerecho = function (elemento) {
    return (this.obtenerPosicionDerecha(elemento) > this.obtenerLimiteDerecho());
}
DataTablesTabulacion.prototype.estaFueraLimites = function (elemento) {

}
//Da positivo para la cantidad de pixeles fuera de la vista del lado izquierdo
DataTablesTabulacion.prototype.diferenciaFueraLimiteIzquierdo = function (elemento) {
    return (this.obtenerPosicionIzquierda(elemento) - this.obtenerLimiteIzquierdo());
}
//Da positivo para la cantidad de pixeles fuera de la vista del lado derecho
DataTablesTabulacion.prototype.diferenciaFueraLimiteDerecho = function (elemento) {
    return (this.obtenerPosicionDerecha(elemento) - this.obtenerLimiteDerecho());
}
DataTablesTabulacion.prototype.moverScrollDerecha = function (numero) {
    this.div_scroll.scrollLeft += numero;
}
DataTablesTabulacion.prototype.moverScrollIzquierda = function (numero) {
    this.div_scroll.scrollLeft -= numero;
}
DataTablesTabulacion.prototype.moverScroll = function (numero) {
    this.div_scroll.scrollLeft += numero;
}

DataTablesTabulacion.prototype.posicionarEnPantalla = function (elemento) {
    if (this.estaFueraLimiteIzquierdo(elemento)) this.moverScroll(this.diferenciaFueraLimiteIzquierdo(elemento));
    if (this.estaFueraLimiteDerecho(elemento)) this.moverScroll(this.diferenciaFueraLimiteDerecho(elemento));
}
