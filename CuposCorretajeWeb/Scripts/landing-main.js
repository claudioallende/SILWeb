let botones = document.querySelectorAll('.boton_proximamente');

// Abre la modal que solo dice proximamente
for (let i = 0; i < botones.length; i++) {
	botones[i].onclick = function () {
		modal.style.display = "block";
	}
}

let span = document.getElementsByClassName("cerrar");
let modal = document.getElementById("myModal");


let boton = document.querySelector('#nav-icon4');
let barra = document.querySelector('#barra');

// Abre el menu hamburguesa
boton.onclick = function() {
  this.classList.toggle("open");
  barra.style.left = 20 + '%';
  if (boton.classList.value === 'close') {
  	barra.style.left = -100 + '%';
  }
}

let navBotones = document.querySelectorAll('#barra li');
let modal1 = document.getElementById("myModal1");
let modal2 = document.getElementById("myModal2");

// Abre las modales del menu
for (let i = 0; i < navBotones.length; i++) {
	navBotones[i].onclick = function () {
		console.log(this.getAttribute('data-id'));
		boton.classList.toggle("open");
		barra.style.left = -100 + '%';

		if (this.getAttribute('data-id') == 1) {
			modal1.style.display = "block";
		}

		if (this.getAttribute('data-id') == 2) {
			modal2.style.display = "block";
		}

		if (this.getAttribute('data-id') == 3) {
			modal.style.display = "block";
		}

	}
}

let modal3 = document.getElementById("myModal3");
let manuales = document.querySelector('.manuales');

// Abre la modal del boton manuales
manuales.onclick = function() {
  modal3.style.display = "block";
}

let aModal = [modal,modal1,modal2,modal3];

// Permite cerrar la modal haciendo click fuera de la misma
window.onclick = function(event) {
	for (let i = 0; i < aModal.length; i++) {
		if (event.target == aModal[i]) {
		    aModal[i].style.display = "none";
		    console.log('qqqq');
		}
	}
}

// Cruz que cierra la modal
for (let i = 0; i < span.length; i++) {
	span[i].onclick = function() {
		aModal[i].style.display = "none";
	}
}

