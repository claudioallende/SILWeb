/* =========================================================
   SIL — Matching en Distribución de Cupos (Pantalla 3)
   Lógica de los modales de matching invocados desde
   Views/Cupos/Distribucion.cshtml cuando el operador
   presiona el botón "Buscar" existente (sin reemplazar
   el flujo legacy).

   Stack: jQuery 1.10.2 + Bootstrap 3 + Swal.fire (CDN).
   No utiliza el loader global: la tabla y la grilla permanecen disponibles
   mientras se consulta el matching.

   Invariante de pendientes (UC1-UC5):
     pending = Cantidad - CantidadAceptada - CantidadRechazada
   CantidadAceptada es un acumulado por asociación (SUM(SOLTURNOS_DETALLE.Cantidad))
   y NO depende del STATUS. La columna STATUS fue retirada en
   ALTER_SOLTURNOS_DROP_REQUEST_STATUS.sql; el motor de matching de SILData ya
   filtra por CantidadDisponible > 0 antes de evaluar pares.
   ========================================================= */

(function ($) {
  'use strict';

  // ── Estado interno ────────────────────────────────────────
  var estado = {
    cupos: [],                        // Array<CupoParaMatchViewModel> devuelto por BuscarCuposConMatch
    cupoActual: null,                 // CupoParaMatchViewModel que se está matcheando
    variant: 'A',                     // 'A' (con vendedor) | 'B' (sin vendedor)
    seleccionados: {},                // { solicitudId: { tipo, cupoId, matchType, cantidad, vendedora } }
    pendientesParaAceptar: [],        // Array<{ cupoId, solicitudId, matchType }> armado para Accept
    rechazarDespues: null             // { solicitudId, motivo } si el operador rechaza
  };

  // ── Helpers de DOM ───────────────────────────────────────
  function $o(sel) { return $(sel); }
  function showOverlay(id) {
    $('#' + id).addClass('is-open').attr('aria-hidden', 'false');
  }
  function hideOverlay(id) {
    $('#' + id).removeClass('is-open').attr('aria-hidden', 'true');
  }
  function hideAllOverlays() {
    $('.sil-modal-overlay').removeClass('is-open').attr('aria-hidden', 'true');
  }
  function escapeHtml(s) {
    if (s === null || s === undefined) return '';
    return String(s)
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;');
  }
  function formatFechaCorta(d) {
    if (!d) return '';
    try {
      // Si llega un string (la API ACA responde a veces con /Date(...)/ de
      // WCF o con ISO 8601), lo pasamos por parsearFechaJSON para que
      // ambos formatos se manejen de la misma forma que el resto del modal.
      var dt = (typeof d === 'string') ? parsearFechaJSON(d) : d;
      if (!dt || isNaN(dt.getTime())) return '';
      return dt.toLocaleDateString('es-AR', { day: '2-digit', month: '2-digit', year: 'numeric' });
    } catch (e) {
      return '';
    }
  }

  // ── Overlay bloqueante (mientras se ejecuta AceptarMatch) ─────
  function showBlockingOverlay(subtitulo) {
    var $overlay = $('#sil-blocking-overlay');
    if ($overlay.length === 0) return;
    $('#sil-blocking-overlay-sub').text(subtitulo || 'Aceptando match y actualizando tabla.');
    $overlay.addClass('is-open');
  }

  function hideBlockingOverlay() {
    var $overlay = $('#sil-blocking-overlay');
    if ($overlay.length === 0) return;
    $overlay.removeClass('is-open');
  }

  // Notificación previa a la distribución. Devuelve una Promise que se
  // resuelve true si el operador confirma, false si cancela.
  // Usa el modal "Confirmación con observaciones" del mock (sección
  // "Otros diálogos operativos") en lugar de Swal.
  function confirmarDistribucion(totalCupos, totalSolicitudes) {
    var dfd = $.Deferred();
    mostrarDialogo({
      tipo: 'confirm-obs',
      header: 'Confirmar distribución',
      titulo: 'Aceptar este match distribuirá los cupos',
      observaciones: null,
      copy: 'Se asignarán <b>' + (totalCupos || 0) + '</b> cupo(s) a ' +
            (totalSolicitudes || 0) + ' solicitud(es). Esta acción no se puede deshacer.',
      meta: 'Esta distribución impacta la tabla de cupos.',
      confirmarTexto: 'Aceptar y distribuir',
      onConfirm: function () { dfd.resolve(true); },
      onCancel: function () { dfd.resolve(false); }
    });
    return dfd.promise();
  }

  // ── API expuesta ─────────────────────────────────────────
  // Exposición del estado actual para que la vista pueda incluir el id
  // del cupo en requests de matching.
  window.SILMatching = {
    /**
     * Abre el modal de matching para el primer cupo en `cuposViewModel`.
     * Decide Variante A o B según si el cupo trae CodVendSIL poblado.
     */
    abrir: function (cuposViewModel) {
      if (!cuposViewModel || cuposViewModel.length === 0) {
        // Modal del mock: "Rechazo automático" (informativo, sin redirección).
        mostrarDialogo({
          tipo: 'rechazo-auto',
          header: 'Sin cupos disponibles',
          titulo: 'No hay cupos para distribuir',
          resumen: '0 cupos',
          detalle: 'No se encontraron cupos en ACA_SILData con esos filtros.',
          lista: [],
          metaFooter: 'Ajustá los filtros y volvé a buscar para obtener resultados.',
          botonTexto: 'Aceptar'
        });
        return;
      }
      estado.cupos = cuposViewModel;
      estado.cupoActual = cuposViewModel[0];
      estado.seleccionados = {};
      estado.pendientesParaAceptar = [];
      decidirVariante(estado.cupoActual);
    },

    /**
     * Buscar cupos con matching desde el backend ACA_SILData (proxy MVC).
     * `filtroForm` es el FilterCuposDisponible armado desde Distribucion.
     */
    buscar: function (filtroForm) {
      return $.ajax({
        url: window.modelData.actionBuscarCuposConMatch,
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(filtroForm)
      });
    },

    cerrar: hideAllOverlays,
    mostrarRechazoAutomatico: mostrarRechazoAutomatico,
    mostrarConflicto: mostrarConflicto,
    /**
     * Helper unificado para los tres modales del mock en la sección
     * "Otros diálogos operativos". Reemplaza al Swal.fire del flujo.
     *   tipo: 'confirm-obs' | 'conflict' | 'rechazo-auto'
     * Ver MatchingDistribucionPartial.cshtml para los IDs y la
     * MatchingDistribucionPartial.cshtml#sil-modal-* para cada
     * diálogo.
     */
    mostrarDialogo: mostrarDialogo,

    /**
     * Devuelve la lista de AsignacionesSolicitudCupo que SILApi espera en
     * modo SolicitudMatch. Cada asociación es un objeto:
     *   { SolicitudId, CupoSeleccionadoId, Cantidad, MatchType,
     *     Compcta, Vendcta, Codproducto, Ctadestino, Cosecha, Centro, Fechaent, Dia }
     * `Cantidad` refleja la cantidad aceptada del match, NO la suma acumulada
     * de la celda. Si la celda tenía 1 y el match suma 3, la celda pasa a 4
     * pero esta asociación dice Cantidad=3 (sólo el incremento del match).
     */
    getAsignacionesSolicitudCupo: function () {
      var mapa = window.SILMatching && window.SILMatching.estadoAsignaciones;
      if (!mapa) return [];

      var out = [];
      Object.keys(mapa).forEach(function (cupoId) {
        Object.keys(mapa[cupoId]).forEach(function (solId) {
          var a = mapa[cupoId][solId];
          if (!a || !a.cellUpdated) return;
          out.push({
            SolicitudId: parseInt(solId, 10),
            CupoSeleccionadoId: parseInt(cupoId, 10),
            Cantidad: a.cantidad || 1,
            MatchType: a.matchType || 'Parcial',
            Compcta: 0,
            Vendcta: 0,
            Codproducto: 0,
            Ctadestino: 0,
            Cosecha: '',
            Centro: '',
            Fechaent: 0,
            Dia: -1
          });
        });
      });
      return out;
    },

    /**
     * Vacía la tabla de asignaciones persistidas. Se llama después de un
     * ActualizarDistribucion exitoso para no reenviar las mismas asociaciones.
     */
    clearAsignaciones: function () {
      window.SILMatching.estadoAsignaciones = {};
    },

    /**
     * Construye un lookup { CuentaVendedor: disponible } a partir de la
     * tabla HTML de Distribución. El disponible por vendedor se calcula
     * en frontend como:
     *
     *   disponible = sum(Cupostotalesadist del vendedor en todas sus filas)
     *              - sum(cupos ya distribuidos en celdas .dia0..dia20)
     *
     * La primera resta es "el total absoluto a distribuir" (lo que muestra
     * el backend en la celda `.cupos-disponibles`, igual para todas las
     * filas del mismo CUIT). La segunda resta descuenta lo que el
     * operador ya tipeó en las celdas de días de la grilla manual.
     * Resultado = cupos pendientes reales para asignar via matching.
     *
     * Filas sin vendedor válido o con valores no numéricos se tratan como
     * 0. Se recorren TODAS las filas del mismo vendor (no se salta la
     * segunda en adelante) porque ahora la métrica es agregada.
     */
    obtenerDisponibilidadVendedores: function () {
      var lookup = {};
      try {
        var cupostotalesadistPorVendor = {};
        var distribuidosPorVendor = {};

        $('#TablaDistribuciones tbody tr.grupo-contrato').each(function () {
          var $row = $(this);
          var rawVendedor = $row.attr('data-vendedor');
          var vendedor = parseInt(rawVendedor, 10);
          if (!vendedor || vendedor <= 0) return;

          // Sumar el Cupostotalesadist del vendedor en esta fila
          var rawCupos = $row.find('.cupos-disponibles').first().text();
          if (rawCupos === null || rawCupos === undefined) rawCupos = '0';
          var limpio = String(rawCupos).replace(/[^0-9\-]/g, '');
          var cupos = parseInt(limpio, 10);
          if (isNaN(cupos)) cupos = 0;
          cupostotalesadistPorVendor[vendedor] =
            (cupostotalesadistPorVendor[vendedor] || 0) + cupos;

          // Sumar los cupos ya distribuidos en las celdas de días
          // (clase .dia0 a .dia20). El backend no actualiza Cuposotorgados
          // cuando el operador tipea en la grilla manual, así que esta es
          // la única forma de saber "cuánto ya se distribuyó para este
          // vendedor" en el frontend.
          var distribuidos = 0;
          for (var d = 0; d <= 20; d++) {
            var $celda = $row.find('.dia' + d).first();
            if ($celda.length === 0) continue;
            var raw = $celda.text();
            if (raw === null || raw === undefined) raw = '0';
            var limpioCelda = String(raw).replace(/[^0-9\-]/g, '');
            var v = parseInt(limpioCelda, 10);
            if (!isNaN(v)) distribuidos += v;
          }
          distribuidosPorVendor[vendedor] =
            (distribuidosPorVendor[vendedor] || 0) + distribuidos;
        });

        // Calcular el disponible por vendor y armar el lookup
        Object.keys(cupostotalesadistPorVendor).forEach(function (v) {
          var key = parseInt(v, 10);
          var total = cupostotalesadistPorVendor[v] || 0;
          var dist = distribuidosPorVendor[v] || 0;
          // Math.max(0, ...) por si el operador tipeó de más en la grilla
          // y la resta da negativo.
          lookup[key] = Math.max(0, total - dist);
        });
      } catch (ex) {
        console.warn('[Matching] obtenerDisponibilidadVendedores error:', ex);
      }
      return lookup;
    },

    /**
     * Decide si la respuesta del motor (`CuposMatching/BuscarCuposConMatch`)
     * debe abrir el modal de matching. La disponibilidad por vendedor ya no
     * la trae el backend: la calcula la UI desde la tabla HTML vigente
     * (#TablaDistribuciones, atributo data-vendedor + celda .cupos-disponibles)
     * y se recibe en `disponibilidad`.
     *
     * Reglas:
     *  - Si no hay respuesta o el array de cupos viene vacío → false.
     *  - Para cada cupo se filtra cada match por el vendedor de su solicitud.
     *    Si `disponibilidad[CuentaVendedor]` <= 0 el match se descarta.
     *  - Un cupo sin matches sobrevivientes se descarta entero.
     *  - Los matches supervivientes quedan como los únicos que `abrir()`
     *    debe mostrar; cada match hereda `Cupostotalesadist` para uso de la UI.
     */
    procesarRespuestaSearch: function (resp, disponibilidad) {
      try {
        if (!resp || !resp.success || !resp.cupos || resp.cupos.length === 0) {
          return false;
        }
        var lookup = disponibilidad || {};
        var cuposFiltrados = [];
        (resp.cupos || []).forEach(function (cupo) {
          var matchesFiltrados = (cupo.Matches || []).filter(function (m) {
            var vendedor = (m && m.Solicitud && m.Solicitud.CuentaVendedor !== undefined && m.Solicitud.CuentaVendedor !== null)
              ? m.Solicitud.CuentaVendedor
              : (m && m.Vendedor ? parseInt(m.Vendedor, 10) : NaN);
            if (!vendedor || vendedor <= 0) return false;
            var disponibles = lookup.hasOwnProperty(vendedor) ? lookup[vendedor] : 0;
            return Number(disponibles) > 0;
          });
          if (matchesFiltrados.length === 0) return;

          var cupoClonado = $.extend({}, cupo, { Matches: matchesFiltrados });
          var primerMatch = matchesFiltrados[0];
          // Resolver el vendedor del primer match con el mismo fallback
          // que usa el filter de arriba. Sin este fallback, si el match
          // trae "Vendedor" como string en lugar de "Solicitud.CuentaVendedor",
          // vendedor queda undefined y Cupostotalesadist cae a 0 (en vez
          // de tomar lookup[vendedor]), haciendo que la validación per-
          // vendor al confirmar use CuposTotales como fallback y nunca
          // dispare cuando la suma excede Cupostotalesadist.
          var vendedor = (primerMatch && primerMatch.Solicitud && primerMatch.Solicitud.CuentaVendedor !== undefined && primerMatch.Solicitud.CuentaVendedor !== null)
            ? primerMatch.Solicitud.CuentaVendedor
            : (primerMatch && primerMatch.Vendedor ? parseInt(primerMatch.Vendedor, 10) : NaN);
          cupoClonado.Cupostotalesadist = (vendedor && lookup[vendedor]) ? lookup[vendedor] : 0;
          cuposFiltrados.push(cupoClonado);
        });
        return cuposFiltrados.length > 0 ? cuposFiltrados : false;
      } catch (ex) {
        console.warn('[Matching] procesarRespuestaSearch error:', ex);
        return false;
      }
    },

    /**
     * Alias simple: ¿hay al menos un cupo para mostrar? Usado por tests
     * manuales y como atajo.
     */
    debeMostrarModal: function (resp, disponibilidad) {
      var resultado = this.procesarRespuestaSearch(resp, disponibilidad);
      return Array.isArray(resultado) && resultado.length > 0;
    },
    mostrarConfirmacionObs: function (solicitante, observacion) {
      // Disparador explícito (útil para QA y demo). Si no se pasan args,
      // muestra un placeholder.
      $('#confirm-obs-texto').html('<b>Observaciones:</b><br />&laquo;' +
        escapeHtml(observacion || 'Soja con humedad máxima 14%. Requiere camión seco.') + '&raquo;');
      $('#confirm-obs-solicitante').html(solicitante
        ? 'Solicitante: <strong>' + escapeHtml(solicitante) + '</strong>'
        : '');
      showOverlay('sil-modal-confirm-obs');
    }
  };

  // ── Decidir variante ─────────────────────────────────────
  function decidirVariante(cupo) {
    var vendedorCupo = normalizarVendedor(cupo.CodVendSIL);
    var conVendedor = !!vendedorCupo;
    estado.variant = conVendedor ? 'A' : 'B';

    if (conVendedor) {
      renderVarianteA(cupo);
      showOverlay('sil-modal-variant-a');
    } else {
      renderVarianteB(cupo);
      showOverlay('sil-modal-variant-b');
    }
  }

  // ============================================================
  // VARIANTE A — Cupo con vendedor (3 tabs)
  // ============================================================

  // Agrupa los matches por SolicitudId. Devuelve una lista de solicitudes
  // únicas con los cupos disponibles para matchear.
  //
  // Tras el refactor de CuposMatchingController.AgruparPorSolicitud, cada
  // match ya viene agregado por solicitud (1 entrada por solicitudId, con
  // su lista de cupos en m.Cupos[]). Acá seguimos agrupando por
  // solicitudId como defensa por si el backend alguna vez devuelve dos
  // entradas con el mismo id (no debería, pero es barato protegerse).
  // cupoIds se llena desde m.Cupos[].Id, no desde m.CupoId — esa propiedad
  // ya no existe en SolicitudParaMatchViewModel.
  function agruparMatchesPorSolicitud(matches) {
    var grupos = {};
    (matches || []).forEach(function (m) {
      var key = String(m.Id);
      if (!grupos[key]) {
        grupos[key] = {
          solicitud: m,
          cupoIds: [],
          matchTypes: []
        };
      }
      var cuposDeMatch = m.Cupos || [];
      cuposDeMatch.forEach(function (cupo) {
        if (cupo && cupo.Id && grupos[key].cupoIds.indexOf(cupo.Id) === -1) {
          grupos[key].cupoIds.push(cupo.Id);
        }
      });
      if (m.MatchType) grupos[key].matchTypes.push(m.MatchType);
    });
    return Object.values(grupos);
  }

  // Agrupa los grupos (de agruparMatchesPorSolicitud) por solicitante,
  // MatchType y Observacion. La clave incluye esos tres campos para que
  // matches del mismo vendedor pero con distinta observación (o distinto
  // tipo) aparezcan como filas separadas en Variante B, ya que el
  // operador puede querer tratarlos como decisiones independientes.
  //
  // Devuelve:
  //   { solicitante, records[], conObs, matchType, observacion,
  //     totalSols, fechaMasAntigua }
  // records[] está ordenado por fecha asc (más antigua primero).
  function agruparPorSolicitante(gruposPorSolicitud) {
    var grupos = {};
    (gruposPorSolicitud || []).forEach(function (g) {
      var sol = g.solicitud;
      var solicitante = (sol.NombreVendedor || sol.Vendedor || '—').toString().trim() || '—';
      var tipo = sol.MatchType || 'Parcial';
      var obs = sol.Observacion || '';

      // Clave compuesta: si dos matches tienen distinta observación (o
      // uno es Condicional y otro Parcial), no se mezclan aunque sean
      // del mismo vendedor.
      var key = solicitante + '||' + tipo + '||' + obs;

      if (!grupos[key]) {
        grupos[key] = {
          solicitante: solicitante,
          matchType: tipo,
          observacion: obs,
          records: [],
          conObs: false,
          totalSols: 0,
          fechaMasAntigua: null
        };
      }
      grupos[key].records.push(g);
      // totalSols = suma de las unidades disponibles para asignar en este
      // grupo (suma de s.CantidadDisponible). Es lo que el operador va a
      // poder mover como máximo para este solicitante respetando el tope
      // que el backend expone en cada solicitud.
      grupos[key].totalSols += Math.max(0, sol.CantidadDisponible || 0);

      if (obs || tipo === 'Condicional') {
        grupos[key].conObs = true;
      }

      // Fecha más antigua del grupo.
      var f = parsearFechaJSON(sol.FechaSolicitado);
      if (f && (!grupos[key].fechaMasAntigua || f < grupos[key].fechaMasAntigua)) {
        grupos[key].fechaMasAntigua = f;
      }
    });

    // Ordenar records de cada grupo por fecha asc (antigüedad).
    Object.keys(grupos).forEach(function (k) {
      grupos[k].records.sort(function (a, b) {
        var fa = parsearFechaJSON(a.solicitud.FechaSolicitado);
        var fb = parsearFechaJSON(b.solicitud.FechaSolicitado);
        if (!fa) return 1;
        if (!fb) return -1;
        return fa - fb;
      });
    });

    // Ordenar los grupos por fechaMasAntigua asc — la solicitud más
    // vieja arriba. Coincide con el chip "Ordenado: Antigüedad" del
    // header del modal y con el comportamiento del mock v5.
    var lista = Object.values(grupos);
    lista.sort(function (a, b) {
      var fa = a.fechaMasAntigua;
      var fb = b.fechaMasAntigua;
      if (!fa && !fb) return 0;
      if (!fa) return 1;
      if (!fb) return -1;
      return fa - fb;
    });
    return lista;
  }

  // ============================================================
  // Validación de conflicto de cupos entre solicitudes (Variante B)
  // ============================================================
  // Itera el estado (indexado por solicitudId) y simula lo que doAccept
  // haría: para cada solicitud con grupoChecked y cantidad > 0 toma los
  // primeros `cantidad` cupoIds del pool compatible. Si el mismo cupoId
  // queda asignado a dos o más solicitudes, hay conflicto.
  //
  // Devuelve:
  //   - hayConflictos: bool
  //   - conflictosPorSolicitud: { solicitudId: [cupoId, ...] } cupos por
  //       los que esta solicitud está peleando con otras
  //   - cuposEnConflicto: [cupoId, ...] ids únicos en conflicto
  //   - nombresPorSolicitud: { solicitudId: stringHuman } para los mensajes
  function detectarConflictosCupos() {
    var nombresPorSolicitud = {};
    var asignacionesPorCupo = {};
    var conflictosPorSolicitud = {};

    Object.keys(estado.seleccionados).forEach(function (solKey) {
      var sel = estado.seleccionados[solKey];
      if (!sel || !sel.grupoChecked || !sel.cantidad || sel.cantidad <= 0) return;

      var solId = String(sel.solicitudId);
      var sol = sel.solicitud;
      if (sol) {
        nombresPorSolicitud[solId] = sol.NombreVendedor || sol.Vendedor || ('Solicitud #' + sol.Id);
      } else {
        nombresPorSolicitud[solId] = 'Solicitud #' + solId;
      }

      // Tomar los primeros N cupos del pool (misma lógica que doAccept).
      var pool = sel.cupoIds || [];
      var n = Math.min(sel.cantidad, pool.length);
      for (var i = 0; i < n; i++) {
        var cupoId = pool[i];
        if (!asignacionesPorCupo[cupoId]) asignacionesPorCupo[cupoId] = [];
        asignacionesPorCupo[cupoId].push(solId);
      }
    });

    // Detectar cupos presentes en 2+ solicitudes.
    var cuposEnConflicto = [];
    Object.keys(asignacionesPorCupo).forEach(function (cupoId) {
      var sols = asignacionesPorCupo[cupoId];
      if (sols.length > 1) {
        cuposEnConflicto.push(parseInt(cupoId, 10));
        sols.forEach(function (solId) {
          if (!conflictosPorSolicitud[solId]) conflictosPorSolicitud[solId] = [];
          if (conflictosPorSolicitud[solId].indexOf(parseInt(cupoId, 10)) === -1) {
            conflictosPorSolicitud[solId].push(parseInt(cupoId, 10));
          }
        });
      }
    });

    return {
      hayConflictos: cuposEnConflicto.length > 0,
      conflictosPorSolicitud: conflictosPorSolicitud,
      cuposEnConflicto: cuposEnConflicto,
      nombresPorSolicitud: nombresPorSolicitud
    };
  }

  // Marca visualmente las filas-detalle en conflicto y muestra un
  // banner informativo debajo de la tabla. Se llama desde los handlers
  // de input, los ±, el checkbox de grupo y al confirmar.
  function actualizarConflictoVisual() {
    var $wrap = $('#vb-table-wrap');
    if ($wrap.length === 0) return;

    var res = detectarConflictosCupos();

    // Marcar / desmarcar las filas-detalle por fecha.
    $('.sil-modal-detail-data-row').each(function () {
      var $row = $(this);
      var solId = String($row.data('solicitud'));
      var enConflicto = !!res.conflictosPorSolicitud[solId];
      $row.toggleClass('is-conflict', enConflicto);
    });

    // Banner de conflicto debajo de la tabla.
    var $banner = $('#vb-conflict-banner');
    if (res.hayConflictos) {
      var lineas = [];
      Object.keys(res.conflictosPorSolicitud).forEach(function (solId) {
        var nombre = res.nombresPorSolicitud[solId] || ('Solicitud #' + solId);
        var cupos = res.conflictosPorSolicitud[solId].join(', ');
        lineas.push('<b>' + escapeHtml(nombre) + '</b>: cupos ' + escapeHtml(cupos));
      });
      var html = '<div class="sil-modal-callout sil-modal-callout-red">' +
                 '<span class="sil-modal-callout-icon" aria-hidden="true">!</span>' +
                 '<div><strong>Conflicto de cupos:</strong> los siguientes cupos están asignados a más de una solicitud. ' +
                 'Ajustá las cantidades y elegí en cuál solicitud los querés dejar.<br>' +
                 lineas.join('<br>') + '</div></div>';
      if ($banner.length === 0) {
        $banner = $('<div id="vb-conflict-banner" class="sil-modal-conflict-banner"></div>');
        $wrap.after($banner);
      }
      $banner.html(html).show();
    } else if ($banner.length) {
      $banner.empty().hide();
    }
  }

  function renderVarianteA(cupo) {
    // Subtítulo del cupo.
    $('#va-title-cupo').text('Match detectado');
    var subtitleParts = [];
    if (cupo.CuposTotales) subtitleParts.push(cupo.CuposTotales + ' cupos disponibles');
    if (cupo.NomGrano) subtitleParts.push(cupo.NomGrano);
    if (cupo.NomCompSIL) subtitleParts.push(cupo.NomCompSIL);
    if (cupo.NomVendSIL) subtitleParts.push('Vendedor: ' + cupo.NomVendSIL);
    $('#va-subtitle-cupo').text(subtitleParts.join(' · ') || 'Cupo distribuido');

    var grupos = agruparMatchesPorSolicitud(cupo.Matches || []);
    var dirs = grupos.filter(function (g) {
      return g.matchTypes.every(function (t) { return t === 'Directo'; });
    });
    var parciales = grupos.filter(function (g) {
      return !g.matchTypes.every(function (t) { return t === 'Directo'; });
    });

    // STEP 1 — solicitudes únicas con match (mayormente directos).
    var headline = '';
    if (grupos.length > 0) {
      var totalSolicitudes = grupos.length;
      var totalCuposMatch = grupos.reduce(function (a, g) { return a + g.cupoIds.length; }, 0);
      headline = '<b>' + totalSolicitudes + '</b> solicitud(es) detectada(s) para ' + totalCuposMatch + ' cupo(s).';
      $('#va-step1-headline').html(headline);

      var list = grupos.map(function (g) {
        return renderFilaSolicitud(g, 'va-step1');
      }).join('');
      $('#va-step1-list').html(list);
    } else {
      $('#va-step1-headline').html('<b>0</b> solicitudes detectadas');
      $('#va-step1-list').html('<em class="sil-empty-state-inline">No se detectaron solicitudes para los cupos disponibles.</em>');
    }

    // STEP 2 — confirmación.
    $('#va-step2-warning-headline').text('Los ' + (cupo.CuposTotales || 0) + ' cupos disponibles pueden asignarse a ' + grupos.length + ' solicitud(es).');

    // STEP 3 — matches parciales (incluye Condicionales como "con obs").
    var partialHtml = '';
    if (parciales.length > 0) {
      partialHtml += '<table class="sil-modal-table sil-modal-table-primary">';
      partialHtml += '<thead><tr><th class="tl">Solicitud</th><th>Grano</th><th>Fecha</th><th>Tipo match</th><th>Disponibles</th><th>Cupos a asignar</th></tr></thead><tbody>';
      parciales.forEach(function (g) {
        partialHtml += renderFilaTablaSolicitud(g, 'va-step3');
      });
      partialHtml += '</tbody></table>';
    } else {
      partialHtml = '<em class="sil-empty-state-inline">No hay matches parciales disponibles.</em>';
    }
    $('#va-step3-table-wrap').html(partialHtml);

    // Reset tab.
    irATab('va-step1');
  }

  // Renderiza una fila de solicitud para los listados en cards (Variante A step 1).
  function renderFilaSolicitud(g, prefix) {
    var m = g.solicitud;
    // m.Cantidad = total pedido. m.CantidadDisponible = lo que se puede aceptar
    // (Cantidad - CantidadAceptada - CantidadRechazada), según backend.
    var disponibles = Math.max(0, m.CantidadDisponible || 0);
    var total = Math.max(0, m.Cantidad || 0);
    var cupoIdsCount = g.cupoIds.length;
    var tipoMatch = m.MatchType || g.matchTypes[0] || 'Parcial';
    var cssTipo = tipoMatch === 'Directo' ? 'is-direct'
      : (tipoMatch === 'Condicional' ? 'is-cond' : 'is-partial');
    var badgeClass = tipoMatch === 'Directo' ? 'sil-badge-dir'
      : (tipoMatch === 'Condicional' ? 'sil-badge-obs' : 'sil-badge-par');
    var badgeLabel = tipoMatch === 'Directo' ? 'Match directo'
      : (tipoMatch === 'Condicional' ? 'Con obs.' : 'Parcial');
    var initialQty = Math.min(disponibles, cupoIdsCount);

    return '<div class="sil-match-row ' + cssTipo + '">' +
      '  <div class="sil-match-row-info">' +
      '    <b>Solicitud #' + m.Id + '</b>' +
      '    <span>Vendedor: ' + escapeHtml(m.Vendedor || '—') +
                ' &middot; ' + escapeHtml(m.NomGrano || ('Grano ' + (m.CodigoGrano || ''))) +
                ' &middot; ' + formatFechaCorta(m.FechaSolicitado) + '</span>' +
      '    <span class="sil-modal-cell-meta">' +
                '<b>' + disponibles + '</b> disponibles de ' + total + ' pedido(s) &middot; ' +
                (m.CantidadRechazada || 0) + ' rechazado(s) &middot; ' +
                cupoIdsCount + ' cupo(s) matchean</span>' +
      '  </div>' +
      '  <div class="sil-modal-qty">' +
      '    <button type="button" data-' + prefix + '-decr data-solicitud="' + m.Id + '" aria-label="Disminuir cupos de la solicitud ' + m.Id + '">&minus;</button>' +
      '    <input type="number" min="0" max="' + disponibles + '" value="' + initialQty +
                  '" data-' + prefix + '-input data-solicitud="' + m.Id +
                  '" data-cupo-count="' + cupoIdsCount + '" aria-label="Cupos a asignar a la solicitud ' + m.Id + '" />' +
      '    <button type="button" data-' + prefix + '-incr data-solicitud="' + m.Id + '" aria-label="Aumentar cupos de la solicitud ' + m.Id + '">&plus;</button>' +
      '  </div>' +
      '  <span class="sil-badge ' + badgeClass + '">' + badgeLabel + '</span>' +
      '</div>';
  }

  // Renderiza una fila de solicitud para la tabla de Variante A step 3 (parciales).
  function renderFilaTablaSolicitud(g, prefix) {
    var m = g.solicitud;
    var disponibles = Math.max(0, m.CantidadDisponible || 0);
    var total = Math.max(0, m.Cantidad || 0);
    var cupoIdsCount = g.cupoIds.length;
    var tipoMatch = m.MatchType || g.matchTypes[0] || 'Parcial';
    var badgeClass = tipoMatch === 'Directo' ? 'sil-badge-dir'
      : (tipoMatch === 'Condicional' ? 'sil-badge-obs' : 'sil-badge-par');
    var badgeLabel = tipoMatch === 'Directo' ? 'Match directo'
      : (tipoMatch === 'Condicional' ? 'Con obs.' : 'Parcial');
    var initialQty = Math.min(disponibles, cupoIdsCount);

    var html = '';
    html += '<tr class="sil-modal-data-row">';
    html += '  <td class="tl"><div class="sil-modal-cell-title">#' + m.Id + '</div>';
    html += '    <div class="sil-modal-cell-meta">' + escapeHtml(m.Vendedor || '—') + '</div></td>';
    html += '  <td>' + escapeHtml(m.NomGrano || '') + '</td>';
    html += '  <td>' + (formatFechaCorta(parsearFechaJSON(m.FechaSolicitado)) || '&mdash;') + '</td>';
    html += '  <td><span class="sil-badge ' + badgeClass + '">' + badgeLabel + '</span></td>';
    html += '  <td>' + disponibles + ' <span class="sil-modal-cell-subtle">/ ' + total + '</span></td>';
    html += '  <td>';
    html += '    <div class="sil-modal-qty">';
    html += '      <button type="button" data-' + prefix + '-decr data-solicitud="' + m.Id + '" aria-label="Disminuir cupos de la solicitud ' + m.Id + '">&minus;</button>';
    html += '      <input type="number" min="0" max="' + disponibles + '" value="' + initialQty +
                  '" data-' + prefix + '-input data-solicitud="' + m.Id +
                  '" data-cupo-count="' + cupoIdsCount + '" aria-label="Cupos a asignar a la solicitud ' + m.Id + '" />';
    html += '      <button type="button" data-' + prefix + '-incr data-solicitud="' + m.Id + '" aria-label="Aumentar cupos de la solicitud ' + m.Id + '">&plus;</button>';
    html += '    </div>';
    html += '    <div class="sil-modal-qty-limit">m&aacute;x. ' + disponibles + '</div>';
    html += '  </td>';
    html += '</tr>';
    return html;
  }

  function irATab(tabId) {
    var estadosVisuales = {
      'va-step1': {
        clase: 'is-step-direct',
        titulo: 'Match directo detectado',
        icono: '&#10003;'
      },
      'va-step2': {
        clase: 'is-step-warning',
        titulo: 'Desestimar match directo — confirmación requerida',
        icono: '&#9888;'
      },
      'va-step3': {
        clase: 'is-step-partial',
        titulo: 'Match directo desestimado — matches parciales disponibles',
        icono: '&#8776;'
      }
    };
    var estadoVisual = estadosVisuales[tabId] || estadosVisuales['va-step1'];
    var $modal = $('#sil-modal-variant-a');

    $modal.find('[data-tab-group="variant-a"]')
      .removeClass('is-active')
      .attr('aria-selected', 'false');
    $modal.find('[data-tab="' + tabId + '"]')
      .addClass('is-active')
      .attr('aria-selected', 'true');

    $modal.find('.sil-modal-tab-step')
      .removeClass('is-active')
      .attr('hidden', 'hidden');
    $('#' + tabId)
      .addClass('is-active')
      .removeAttr('hidden');

    $('#va-modal-card')
      .removeClass('is-step-direct is-step-warning is-step-partial')
      .addClass(estadoVisual.clase);
    $('#va-title-cupo').text(estadoVisual.titulo);
    $('#va-header-icon').html(estadoVisual.icono);
  }

  // ============================================================
  // VARIANTE B — Cupo sin vendedor (input numérico por solicitud)
  // ============================================================
  function renderVarianteB(cupo) {
    $('#vb-subtitle-cupo').text(
      // Mostramos el límite real (Cupostotalesadist) y no CuposTotales,
      // porque es lo que efectivamente puede asignar el modal.
      (cupo.Cupostotalesadist || cupo.CuposTotales || 0) + ' cupos · ' +
      (cupo.NomGrano || '') + ' · ' +
      (cupo.NomCompSIL || 'Sin comprador') + ' · Sin vendedor');

    // Agrupar primero por solicitudId (dedup) y luego consolidar por
    // solicitante. La tabla principal muestra una fila por solicitante;
    // al expandir se ve una subtabla con una fila por fecha (solicitudId).
    var gruposPorSolicitud = agruparMatchesPorSolicitud(cupo.Matches || []);
    var gruposPorSolicitante = agruparPorSolicitante(gruposPorSolicitud);
    // Tope real: Cupostotalesadist (los cupos pendientes en la tabla de
    // distribución para este cupo) y NO CuposTotales (que cuenta todos
    // los cupos físicos que matchearon los filtros, incluyendo los que
    // ya están agotados). Si vienen 0, caemos a CuposTotales como
    // defensa por si el backend aún no setea Cupostotalesadist.
    var totalCuposAsignar = cupo.Cupostotalesadist || cupo.CuposTotales || 0;
    var cuposFisicosMatcheados = cupo.CuposTotales || 0;

    // Total solicitudes = suma de las unidades disponibles para asignar
    // (CantidadDisponible) en todas las solicitudes del grupo. Es el
    // tope real que el operador puede mover respetando lo que el
    // backend expone por solicitud. La diferencia entre este valor y
    // totalCuposAsignar (los cupos pendientes en la tabla de distribu-
    // ción) son los "cupos sin asignar" del mock.
    var totalSolicitudes = gruposPorSolicitante.reduce(function (a, g) {
      return a + g.totalSols;
    }, 0);

    // Solicitantes = CUITs únicos (campo Vendedor), no cantidad de
    // grupos. Un mismo CUIT puede aparecer en varios grupos si tiene
    // matches con distinta observación o tipo.
    var cuitsUnicos = {};
    gruposPorSolicitante.forEach(function (g) {
      g.records.forEach(function (rec) {
        var cuit = rec.solicitud && rec.solicitud.Vendedor;
        if (cuit) cuitsUnicos[String(cuit)] = true;
      });
    });
    var solicitantesUnicos = Object.keys(cuitsUnicos).length;

    $('#vb-counter-total').text(totalCuposAsignar);
    $('#vb-counter-solicitantes').text(solicitantesUnicos);
    $('#vb-counter-total-sols').text(totalSolicitudes);
    // "Sin asignar" = cupos pedidos por más de una solicitud (overlap).
    // Es la diferencia entre el total de cupos que piden las solicitudes
    // y los cupos físicos disponibles para distribuir.
    $('#vb-counter-sin-asignar').text(Math.max(0, totalSolicitudes - totalCuposAsignar));
    $('#vb-counter-asignados-max').text(totalCuposAsignar);

    var tableHtml = '<table class="sil-modal-table sil-modal-table-main"><thead><tr>';
    tableHtml += '<th class="tl" style="width:32px;"></th>';
    tableHtml += '<th class="tl">Solicitante</th>';
    tableHtml += '<th>Sols.</th>';
    tableHtml += '<th>M&aacute;s antigua</th>';
    tableHtml += '<th>Match</th>';
    tableHtml += '<th>Total a asignar</th>';
    tableHtml += '<th>Detalle</th>';
    tableHtml += '</tr></thead><tbody>';

    estado.seleccionados = {};

    gruposPorSolicitante.forEach(function (grupo, grupoIdx) {
      var tipoMatch = grupo.matchType;
      var badgeClass = tipoMatch === 'Directo' ? 'sil-badge-dir'
        : (tipoMatch === 'Condicional' ? 'sil-badge-obs' : 'sil-badge-par');
      var badgeLabel = tipoMatch === 'Directo' ? 'Match directo'
        : (tipoMatch === 'Condicional' ? 'Con obs.' : 'Parcial');
      var tipoCss = tipoMatch === 'Directo' ? 'is-direct'
        : (tipoMatch === 'Condicional' ? 'is-cond' : 'is-partial');

      var fechaAntiguaDisplay = grupo.fechaMasAntigua ? formatFechaCorta(grupo.fechaMasAntigua) : '&mdash;';
      var antiguedadDisplay = '';
      if (grupo.fechaMasAntigua) {
        var dias = Math.max(0, Math.floor((Date.now() - grupo.fechaMasAntigua.getTime()) / 86400000));
        antiguedadDisplay = 'hace ' + dias + ' d&iacute;a' + (dias !== 1 ? 's' : '');
      }

      // Fila padre (sin input). El "Total" se actualiza dinámicamente
      // desde la suma de los inputs de la subtabla (ver actualizarBarraVB).
      // El checkbox arranca desmarcado: el operador debe optar
      // explícitamente por incluir el grupo (así se evita sobre-asignar
      // cuando hay más solicitudes que cupos disponibles).
      tableHtml += '<tr class="sil-modal-data-row ' + tipoCss + '" data-grupo="' + grupoIdx + '">';
      tableHtml += '  <td><input type="checkbox" data-vb-chk-grupo data-grupo="' + grupoIdx + '" aria-label="Incluir solicitudes de ' + escapeHtml(grupo.solicitante) + '"></td>';
      tableHtml += '  <td class="tl">';
      tableHtml += '    <div class="sil-modal-cell-title">' + escapeHtml(grupo.solicitante) + '</div>';
      tableHtml += '    <div class="sil-modal-cell-meta">Solicitudes agrupadas por vendedor</div>';
      tableHtml += '  </td>';
      tableHtml += '  <td>' + grupo.totalSols + '</td>';
      tableHtml += '  <td>' + fechaAntiguaDisplay + '<div class="sil-modal-cell-meta">' + antiguedadDisplay + '</div></td>';
      tableHtml += '  <td><span class="sil-badge ' + badgeClass + '">' + badgeLabel + '</span></td>';
      tableHtml += '  <td>';
      tableHtml += '    <strong class="sil-modal-grupo-total" data-vb-total-grupo="' + grupoIdx + '">0</strong>';
      tableHtml += '    <div class="sil-modal-cell-meta">suma de d&iacute;as</div>';
      tableHtml += '  </td>';
      tableHtml += '  <td>';
      tableHtml += '    <button type="button" class="sil-modal-btn-detail" data-vb-toggle data-grupo="' + grupoIdx + '" aria-expanded="false" aria-label="Ver detalle de ' + escapeHtml(grupo.solicitante) + '">&#9662; ver</button>';
      tableHtml += '  </td>';
      tableHtml += '</tr>';

      // Fila detalle (subtabla por fecha), inicialmente oculta.
      tableHtml += '<tr class="sil-modal-detail-row" data-grupo-detail="' + grupoIdx + '" style="display:none;">';
      tableHtml += '  <td colspan="7" class="sil-modal-detail-cell">';
      tableHtml += '    <div class="sil-modal-detail-body">';
      if (grupo.conObs) {
        // Con la nueva clave de agrupación todos los records de este grupo
        // comparten la misma observación, así que se usa directo.
        var obsTxt = grupo.observacion || 'La solicitud tiene condiciones registradas. Verificar antes de asignar.';
        tableHtml += '      <div class="sil-modal-callout sil-modal-callout-amber sil-modal-obs-callout">';
        tableHtml += '        <span class="sil-modal-callout-icon" aria-hidden="true">!</span>';
        tableHtml += '        <div><strong>Observaciones de la solicitud:</strong> ' + escapeHtml(obsTxt) + '</div>';
        tableHtml += '      </div>';
      }
      tableHtml += '      <div class="sil-modal-detail-title">Detalle por d&iacute;a &mdash; ' + escapeHtml(grupo.solicitante) + '</div>';
      tableHtml += '      <table class="sil-modal-table sil-modal-table-detail">';
      tableHtml += '        <thead><tr>';
      tableHtml += '          <th class="tl">Fecha</th>';
      tableHtml += '          <th>Ingresada</th>';
      tableHtml += '          <th>Cupos a asignar</th>';
      tableHtml += '        </tr></thead>';
      tableHtml += '        <tbody>';

      grupo.records.forEach(function (record, recordIdx) {
        var sol = record.solicitud;
        // Tope del input = unidades disponibles para asignar en esta
        // solicitud (sol.CantidadDisponible). Es lo que el backend va a
        // aceptar como máximo para esta fila; alineamos el input con el
        // límite real en lugar de con los cupos físicos matcheados.
        var disponibles = Math.max(0, sol.CantidadDisponible || 0);
        var initialQty = disponibles; // arranca al máximo para que el operador reduzca si quiere
        var fechaRecord = parsearFechaJSON(sol.FechaSolicitado);
        var fechaDisplay = fechaRecord ? formatFechaCorta(fechaRecord) : '&mdash;';
        var antiguedadRecord = '';
        if (fechaRecord) {
          var diasR = Math.max(0, Math.floor((Date.now() - fechaRecord.getTime()) / 86400000));
          antiguedadRecord = 'hace ' + diasR + ' d&iacute;a' + (diasR !== 1 ? 's' : '');
        }
        var solId = sol.Id;

        tableHtml += '          <tr class="sil-modal-detail-data-row" data-grupo="' + grupoIdx + '" data-record="' + recordIdx + '" data-solicitud="' + solId + '">';
        tableHtml += '            <td class="tl">' + fechaDisplay + '</td>';
        tableHtml += '            <td>' + antiguedadRecord + '</td>';
        tableHtml += '            <td>';
        tableHtml += '              <div class="sil-modal-qty">';
        tableHtml += '                <button type="button" data-vb-decr-day data-grupo="' + grupoIdx + '" data-record="' + recordIdx + '" disabled aria-label="Disminuir">&minus;</button>';
        tableHtml += '                <input type="number" min="0" max="' + disponibles + '" value="' + initialQty + '" disabled data-vb-input-day data-grupo="' + grupoIdx + '" data-record="' + recordIdx + '" data-solicitud="' + solId + '" aria-label="Cupos para solicitud ' + solId + '" />';
        tableHtml += '                <button type="button" data-vb-incr-day data-grupo="' + grupoIdx + '" data-record="' + recordIdx + '" disabled aria-label="Aumentar">&plus;</button>';
        tableHtml += '              </div>';
        tableHtml += '              <div class="sil-modal-qty-limit">m&aacute;x. ' + disponibles + '</div>';
        tableHtml += '            </td>';
        tableHtml += '          </tr>';

        // Estado por solicitudId. La cantidad se mantiene en sync con el
        // input de la subtabla. doAccept y detectarConflictosCupos iteran
        // sobre este mapa (clave = solicitudId).
        estado.seleccionados[String(solId)] = {
          checked: false,           // compat legacy
          grupoIdx: grupoIdx,
          grupoChecked: false,      // arranca desmarcado, se setea true al tildar el checkbox del padre
          grupoConObs: grupo.conObs,
          solicitudId: solId,
          cupoIds: record.cupoIds.slice(),
          cupoIdsPorFecha: (sol.Cupos || []).reduce(function (acc, c) {
            // La API puede devolver la fecha como Date, como número (ms),
            // como ISO 8601 o como string WCF /Date(1234567890123)/. Pasamos
            // siempre por parsearFechaJSON para no romper con el formato WCF.
            var parsed = c && c.Fecha ? parsearFechaJSON(c.Fecha) : null;
            if (!parsed) return acc;
            var f = parsed.toISOString().slice(0, 10);
            (acc[f] = acc[f] || []).push(c.Id);
            return acc;
          }, {}),
          disponibles: disponibles,
          cantidad: initialQty,
          matchType: sol.MatchType || 'Parcial',
          solicitud: sol
        };
      });

      tableHtml += '        </tbody>';
      tableHtml += '      </table>';
      tableHtml += '    </div>';
      tableHtml += '  </td>';
      tableHtml += '</tr>';
    });

    tableHtml += '</tbody></table>';
    $('#vb-table-wrap').html(tableHtml);

    actualizarBarraVB();
    actualizarConflictoVisual();
  }

  function actualizarBarraVB() {
    // La barra refleja la suma de cantidades tipeadas en los inputs de
    // la subtabla por fecha. Cada fila-detalle aporta su valor al grupo
    // y al total del vendedor (CUIT). Sólo cuentan las filas cuyo grupo
    // padre está incluido (grupoChecked).
    var asignado = 0;
    var totalesPorGrupo = {};
    var lookupVendedor = lookupCupostotalesadistPorVendedor();

    $('[data-vb-input-day]').each(function () {
      var $i = $(this);
      var grupoIdx = $i.data('grupo');
      var solId = String($i.data('solicitud'));
      var cant = parseInt($i.val(), 10) || 0;

      var s = estado.seleccionados[solId];
      if (!s) return;
      s.cantidad = cant;
      if (s.grupoChecked) {
        asignado += cant;
        totalesPorGrupo[grupoIdx] = (totalesPorGrupo[grupoIdx] || 0) + cant;
      } else {
        // Aunque no cuente para el total global, guardamos el 0 para
        // que el badge del padre quede en 0 cuando el grupo está apagado.
        totalesPorGrupo[grupoIdx] = totalesPorGrupo[grupoIdx] || 0;
      }
    });

    // Reflejar el total por grupo en la fila padre.
    Object.keys(totalesPorGrupo).forEach(function (grupoIdx) {
      var total = totalesPorGrupo[grupoIdx];
      var $el = $('[data-vb-total-grupo="' + grupoIdx + '"]');
      if ($el.length) {
        $el.text(total);
        $el.toggleClass('is-active', total > 0);
      }
    });

    // El tope se calcula como el máximo Cupostotalesadist entre los
    // vendedores presentes. Cuando todos los matches son del mismo
    // vendedor (caso típico), equivale a su Cupostotalesadist.
    var max = estado.cupoActual
      ? topeMaximoVendedor(lookupVendedor)
      : 0;
    var pct = max > 0 ? Math.min(100, Math.round(asignado / max * 100)) : 0;
    var excedido = max > 0 && asignado > max;
    $('#vb-progress-fill')
      .css('width', pct + '%')
      .toggleClass('is-complete', asignado >= max && max > 0)
      .toggleClass('is-overflow', excedido);
    $('#vb-progress-fill').parent().attr('aria-valuenow', pct);
    $('#vb-progress-label').text(asignado + ' / ' + max);
    $('#vb-counter-asignados').text(asignado);
    $('#vb-progress-warn').toggleClass('is-visible', asignado < max);

    // Banner inline de excedente. Aparece apenas la suma de los inputs
    // supera los cupos disponibles, para que el operador vea el problema
    // antes de llegar al Swal de confirmación.
    actualizarBannerExcedente(excedido, asignado, max);
  }

  // Construye un lookup { CUIT: Cupostotalesadist } a partir de los
  // matches vigentes. En el flujo actual todos los matches del modal
  // suelen ser del mismo vendedor, pero la estructura soporta varios.
  function lookupCupostotalesadistPorVendedor() {
    var lookup = {};
    var cupo = estado.cupoActual;
    if (!cupo) return lookup;
    var limite = cupo.Cupostotalesadist || cupo.CuposTotales || 0;
    (cupo.Matches || []).forEach(function (m) {
      var v = m && m.Vendedor != null ? String(m.Vendedor) : null;
      if (v && !lookup.hasOwnProperty(v)) lookup[v] = limite;
    });
    // Si por alguna razón no quedó ningún vendedor mapeado, caemos al
    // límite del cupo como único "vendor".
    if (Object.keys(lookup).length === 0) {
      lookup['__cupo__'] = limite;
    }
    return lookup;
  }

  // Suma las cantidades tipeadas por cada vendedor (CUIT) y devuelve
  // los totales para validar contra el lookup por vendedor.
  function totalesPorVendedor() {
    var lookupVendedor = lookupCupostotalesadistPorVendedor();
    var totales = {};
    Object.keys(lookupVendedor).forEach(function (v) { totales[v] = 0; });

    $('[data-vb-input-day]').each(function () {
      var $i = $(this);
      var solId = String($i.data('solicitud'));
      var cant = parseInt($i.val(), 10) || 0;
      var s = estado.seleccionados[solId];
      if (!s || !s.grupoChecked || cant <= 0) return;
      var v = s.solicitud && s.solicitud.Vendedor != null ? String(s.solicitud.Vendedor) : '__cupo__';
      totales[v] = (totales[v] || 0) + cant;
    });
    return { totales: totales, lookup: lookupVendedor };
  }

  function topeMaximoVendedor(lookup) {
    var keys = Object.keys(lookup || {});
    if (!keys.length) return 0;
    return keys.reduce(function (a, k) { return a + (lookup[k] || 0); }, 0);
  }

  function actualizarBannerExcedente(excedido, asignado, max) {
    var $banner = $('#vb-excedente-banner');
    if (!excedido) {
      if ($banner.length) $banner.empty().hide();
      return;
    }
    var diff = asignado - max;
    // Mensaje per-vendedor: listamos qué CUIT se pasó y por cuánto.
    var vends = totalesPorVendedor();
    var lineas = [];
    Object.keys(vends.lookup).forEach(function (v) {
      var limite = vends.lookup[v] || 0;
      var total = vends.totales[v] || 0;
      if (limite > 0 && total > limite) {
        lineas.push('CUIT <b>' + escapeHtml(v) + '</b>: ' + total +
                    ' pedidos / ' + limite + ' disponibles (' + (total - limite) + ' de m&aacute;s)');
      }
    });
    var detalle = lineas.length
      ? lineas.join('<br>')
      : 'est&aacute;s pidiendo <b>' + asignado + '</b> cupos pero s&oacute;lo hay <b>' + max + '</b> disponibles.';
    var html = '<div class="sil-modal-callout sil-modal-callout-red">' +
               '<span class="sil-modal-callout-icon" aria-hidden="true">!</span>' +
               '<div><strong>Excediste los cupos disponibles por vendedor:</strong> ' +
               detalle + ' Reduc&iacute; las cantidades o desactiv&aacute; un grupo antes de confirmar.</div>' +
               '</div>';
    if ($banner.length === 0) {
      $banner = $('<div id="vb-excedente-banner" class="sil-modal-excedente-banner"></div>');
      var $progress = $('#vb-progress');
      if ($progress.length) {
        $progress.after($banner);
      } else {
        $('#vb-table-wrap').before($banner);
      }
    }
    $banner.html(html).show();
  }

  // ============================================================
  // Confirmar / Accept / Reject
  // ============================================================
  function prepararYMostrarConfirmacionObs(onContinue) {
    // Busca un match Condicional entre los grupos seleccionados. Estado
    // nuevo: cada entrada está indexada por solicitudId y trae la
    // solicitud completa en `solicitud`. El flag de inclusión del grupo
    // es `grupoChecked` (no `checked`, que ya no se usa).
    var grupoCond = null;
    Object.keys(estado.seleccionados).forEach(function (k) {
      if (grupoCond) return;
      var s = estado.seleccionados[k];
      if (!s.grupoChecked) return;
      if (s.matchType !== 'Condicional') return;
      grupoCond = s;
    });
    if (!grupoCond) { onContinue(); return; }

    var solicitud = grupoCond.solicitud || (grupoCond.g && grupoCond.g.solicitud) || null;
    var obsTxt = (solicitud && solicitud.Observacion) || '(sin texto)';
    var solicitante = (solicitud && (solicitud.NombreVendedor || solicitud.Vendedor)) || '';
    var fechaTxt = (solicitud && solicitud.FechaSolicitado)
      ? formatFechaCorta(solicitud.FechaSolicitado)
      : '';
    var meta;
    if (solicitante && fechaTxt) {
      meta = 'Solicitante: ' + solicitante + ' · ' + fechaTxt;
    } else if (solicitante) {
      meta = 'Solicitante: ' + solicitante;
    } else {
      meta = fechaTxt;
    }

    mostrarDialogo({
      tipo: 'confirm-obs',
      header: 'Confirmación con observaciones',
      titulo: 'La solicitud tiene condiciones especiales',
      observaciones: obsTxt,
      copy: 'Verificá que el/los cupo(s) seleccionado(s) cumple(n) estas condiciones antes de confirmar.',
      meta: meta,
      confirmarTexto: 'Sí, confirmar',
      onConfirm: onContinue,
      onCancel: function () { /* no-op: el operador canceló */ }
    });
  }

  function doAccept(solicitudesAsignadas) {
    // Envía las asociaciones al backend (POST /api/Cupos/ActualizarDistribucion
    // en modo SolicitudMatch = 2). El endpoint valida que cada asociación tenga
    // Cantidad=1 y un CupoSeleccionadoId explícito, descuenta los cupos
    // disponibles, persiste SOLTURNOS_DETALLE y actualiza SOLTURNOS.Aceptada.
    //
    // solicitudesAsignadas: Array<{ solicitudId, matchType, cantidad? }>
    //
    // Cada entrada representa una solicitud aceptada por el operador con una
    // cantidad N = número de cupos físicos a asociar. Como ActualizarDistribucion
    // exige Cantidad=1 por asociación (1 fila de CUPOSCORRE = 1 cupo), expandimos
    // cada solicitud en N asociaciones (solicitudId, cupoId_i, cantidad=1)
    // usando los CupoId que el matching devolvió para esa solicitud.
    //
    // Antes del request se muestra el Swal de confirmación definido en
    // confirmarDistribucion() — el operador debe aceptar antes de ejecutar.

    var cupo = estado.cupoActual;
    if (!cupo) return;

    if (!Array.isArray(solicitudesAsignadas) || solicitudesAsignadas.length === 0) return;

    // Indexar todos los cupos disponibles por solicitud matcheada. El backend
    // (CuposMatchingController.AgruparPorSolicitud) ahora devuelve una vista
    // sintética (cupo.Id=0) con TODAS las solicitudes en Matches y, dentro de
    // cada solicitud, su lista de cupos disponibles en m.Cupos. Antes se
    // agrupaba por cupo y se leía c.Id como el cupo físico; ahora el cupo
    // "contenedor" sintético no tiene Id real — los cupos físicos viven en
    // m.Cupos[].Id.
    var cuposPorSolicitud = {};
    (estado.cupos || []).forEach(function (c) {
      (c.Matches || []).forEach(function (m) {
        var key = String(m.Id);
        cuposPorSolicitud[key] = (m.Cupos || []).map(function (cupo) {
          return cupo.Id;
        });
      });
    });

    // Construir AsignacionesSolicitudCupo: una asociación por cupo físico DISTINTO.
    var asociaciones = [];
    var totalCupos = 0;

    solicitudesAsignadas.forEach(function (pair) {
      var solicitudId = pair.solicitudId;
      var cuposDisponibles = cuposPorSolicitud[String(solicitudId)] || [];
      if (cuposDisponibles.length === 0) {
        console.warn('[Matching] doAccept: ningún cupo disponible para solicitud', solicitudId);
        return;
      }

      // MatchType: usar el del modal actual si está disponible, sino 'Parcial'.
      var matchLocal = (cupo.Matches || []).find(function (m) { return m.Id === solicitudId; });
      var matchType = pair.matchType || (matchLocal && matchLocal.MatchType) || 'Parcial';

      var cantidad = (typeof pair.cantidad === 'number' && pair.cantidad > 0) ? pair.cantidad : 1;
      var aAsociar = Math.min(cantidad, cuposDisponibles.length);
      if (aAsociar < cantidad) {
        console.warn('[Matching] doAccept: solicitud ' + solicitudId +
          ' pidió ' + cantidad + ' cupos pero sólo ' + cuposDisponibles.length +
          ' matchean. Se distribuyen ' + aAsociar + '.');
      }

      for (var i = 0; i < aAsociar; i++) {
        asociaciones.push({
          SolicitudId: parseInt(solicitudId, 10),
          CupoSeleccionadoId: cuposDisponibles[i],
          Cantidad: 1,
          MatchType: matchType,
          Compcta: 0,
          Vendcta: 0,
          Codproducto: 0,
          Ctadestino: 0,
          Cosecha: '',
          Centro: '',
          Fechaent: 0,
          Dia: -1
        });
        totalCupos++;
      }
    });

    if (asociaciones.length === 0) return;

    // Bloquear pantalla hasta que responda la API.
    showBlockingOverlay('Distribuyendo ' + totalCupos + ' cupo(s)...');

    return $.ajax({
      url: window.modelData.actionActualizarDistribucion,
      method: 'POST',
      contentType: 'application/json; charset=utf-8',
      dataType: 'json',
      data: JSON.stringify({
        model: {
          Modo: 2,  // ModoActualizacionDistribucion.SolicitudMatch
          AsignacionesSolicitudCupo: asociaciones
        },
        Confirmacion: false
      })
    }).done(function (data) {
      hideBlockingOverlay();

      // ActualizarDistribucion puede responder con un int legacy (Codigo)
      // o con ActualizarDistribucionResult { Codigo, Success, Message, ... }.
      var codigo = (typeof data === 'number') ? data : (data && data.Codigo);

      if (codigo === 1) {
        if (window.SILMatching && typeof window.SILMatching.clearAsignaciones === 'function') {
          window.SILMatching.clearAsignaciones();
        }
        hideAllOverlays();

        // Refrescar tabla de distribución para reflejar el estado real del backend.
        if (typeof actualizarTablaContratos === 'function') {
          actualizarTablaContratos({ mostrarEstado: true });
        }

        // Notificación informativa (mock: "Rechazo automático" → reutilizado
        // como notificación de cierre/ejecución exitosa).
        mostrarDialogo({
          tipo: 'rechazo-auto',
          header: 'Distribución realizada',
          titulo: 'Proceso ejecutado correctamente',
          resumen: totalCupos + ' cupo(s)',
          detalle: 'distribuidos correctamente.',
          lista: [],
          metaFooter: (data && data.Message) ? data.Message : 'La tabla de distribución se actualizó.',
          botonTexto: 'Aceptar'
        });
      } else if (codigo === 100) {
        if (typeof addAlert === 'function') { addAlert('Cantidad de cupos excedidos', 'alert-danger'); if (typeof onAlert === 'function') onAlert(); }
      } else if (codigo === 200) {
        if (typeof addAlert === 'function') { addAlert('Cantidad de cupos excedidos para la consignación seleccionada', 'alert-danger'); if (typeof onAlert === 'function') onAlert(); }
      } else if (codigo === 300) {
        if (typeof addAlert === 'function') { addAlert('No hubo cambios', 'alert-info'); if (typeof onAlert === 'function') onAlert(); }
      } else {
        // Error de negocio (mock: "Conflicto de concurrencia" → reutilizado
        // como modal de error genérico: header rojo, detalle del backend).
        mostrarDialogo({
          tipo: 'conflict',
          header: 'No se pudo distribuir',
          titulo: 'El backend rechazó la distribución',
          intro: '',
          help: (data && data.Message) ? data.Message : 'Revisá los datos y volvé a intentar.',
          detalle: null
        });
      }
    }).fail(function (xhr) {
      hideBlockingOverlay();
      console.warn('[Matching] ActualizarDistribucion error:', xhr && xhr.statusText);

      // 409 = conflicto de concurrencia explícito.
      if (xhr && xhr.status === 409) {
        var msg409 = (xhr.responseJSON && (xhr.responseJSON.Message || xhr.responseJSON.message))
          ? (xhr.responseJSON.Message || xhr.responseJSON.message)
          : 'La solicitud ya fue procesada por otro operador.';
        mostrarDialogo({
          tipo: 'conflict',
          header: 'Conflicto de concurrencia',
          titulo: 'La solicitud ya fue procesada',
          intro: 'La solicitud fue <strong>asignada o rechazada</strong> por otro operador mientras revisabas la vista.',
          help: 'La tabla se actualizó. Seleccioná otra solicitud pendiente o volvé a la grilla de cupos.',
          detalle: '<strong>' + escapeHtml(msg409) + '</strong>'
        });
        return;
      }

      // Otros errores: modal "Conflicto de concurrencia" usado como
      // canal genérico de error (header rojo) según el mock.
      var msgErr = (xhr && xhr.responseJSON && (xhr.responseJSON.Message || xhr.responseJSON.message))
        ? (xhr.responseJSON.Message || xhr.responseJSON.message)
        : 'No se pudo comunicar con el servidor.';
      mostrarDialogo({
        tipo: 'conflict',
        header: 'Error al distribuir',
        titulo: 'Falló la comunicación con el servidor',
        intro: '',
        help: 'Verificá tu conexión y volvé a intentar.',
        detalle: '<strong>' + escapeHtml(msgErr) + '</strong>'
      });
    }).always(function () {
      // Garantiza que el overlay bloqueante se oculte en cualquier camino,
      // incluso si el backend responde 2xx sin `Codigo` o si la promesa se
      // resuelve con un body vacío. Cubre UC1/UC2/UC3 post-Acept.
      hideBlockingOverlay();
    });
  }

  /**
   * Intenta reflejar la asignación en la celda de la tabla legacy
   * `#TablaDistribuciones`.
   *
   * Reglas:
   *  - Encuentra la fila cuyo `data-vendedor` coincida con el del cupo
   *    (CodVendSIL) o, si el cupo no tiene vendedor, con el de la
   *    solicitud (match.Vendedor). Sin importar destino ni cosecha.
   *  - Determina la columna de día a partir de `FechaSolicitado` de la
   *    solicitud: `diasDiff = (FechaSolicitado - baseDate).Days`, donde
   *    baseDate es hoy en horario local. Col 0 = `.dia0`, col 1 = `.dia1`, etc.
   *  - Si está dentro del rango (0..20), incrementa el contenido del
   *    `<div class="dia{N}">` por la cantidad recibida.
   *  - Dispara `change` para que el JS legacy (`AjaxDistribucion.js`)
   *    actualice su matriz `modificacionesDelEstadoInicial`.
   *
   * Devuelve `true` si encontró la celda, `false` en caso contrario.
   */
  function actualizarCeldaDistribucion(cupo, match, cantidad) {
    try {
      // Tomamos FechaSolicitado: puede llegar como ISO 8601 ("2026-07-30T00:00:00"),
      // como Date, como número (ms) o como el formato WCF /Date(1234567890123)/ que
      // devuelve SILData cuando no se configura un JsonConverter ISO 8601.
      var fechaSol = parsearFechaJSON(match.FechaSolicitado);
      if (!fechaSol || isNaN(fechaSol.getTime())) return false;

      var today = new Date();
      today.setHours(0, 0, 0, 0);
      var sol = new Date(fechaSol);
      sol.setHours(0, 0, 0, 0);

      var diasDiff = Math.round((sol - today) / (1000 * 60 * 60 * 24));
      if (diasDiff < 0 || diasDiff > 20) return false;

      // Con scroll + fixedColumns DataTables puede crear tablas clonadas. La
      // tabla original es la que vive dentro de dataTables_scrollBody y es la
      // única que contiene todas las celdas diaN editables.
      var $tabla = $('.container_table_distribucion .dataTables_scrollBody table').first();
      if ($tabla.length === 0) {
        $tabla = $('.container_table_distribucion table#TablaDistribuciones').first();
      }
      if ($tabla.length === 0) return false;

      // Para un cupo sin vendedor, la fila legacy se identifica por el vendedor
      // de la solicitud (match.Vendedor), no por cupo.CodVendSIL que es vacío/0.
      var vendedorCupo = normalizarVendedor(cupo.CodVendSIL);
      var vendedorSolicitud = normalizarVendedor(match.Vendedor);
      var vendedoresBuscables = [];
      if (vendedorCupo) vendedoresBuscables.push(vendedorCupo);
      if (vendedorSolicitud && vendedoresBuscables.indexOf(vendedorSolicitud) === -1) {
        vendedoresBuscables.push(vendedorSolicitud);
      }
      // Fallback para filas legacy de cupos sin vendedor.
      if (vendedoresBuscables.length === 0) vendedoresBuscables.push('');

      // Filtramos sólo por vendedor. No comparamos destino ni cosecha: si hay
      // varias filas para el mismo vendedor, tomamos la primera (mejor esfuerzo).
      var $fila = $tabla.find('tbody tr').filter(function () {
        var vendedorFila = normalizarVendedor($(this).attr('data-vendedor'));
        return vendedoresBuscables.indexOf(vendedorFila) !== -1;
      }).first();

      if ($fila.length === 0) {
        console.warn('[Matching] No se encontró fila para actualizar.', {
          vendedoresBuscables: vendedoresBuscables,
          filas: $tabla.find('tbody tr').length
        });
        return false;
      }

      // Selector de celda: el div editable dentro del td con clase .diaN.
      var $cellDiv = $fila.find('td .dia' + diasDiff).first();
      if ($cellDiv.length === 0) {
        console.warn('[Matching] Se encontró la fila pero no la celda de día.', {
          diasDiff: diasDiff,
          vendedor: vendedoresBuscables
        });
        return false;
      }

      var currentVal = parseInt($cellDiv.text(), 10) || 0;
      var newVal = currentVal + cantidad;
      $cellDiv.text(String(newVal));

      // Disparar keyup (mismo evento que escucha EstadoGrilla.handleEvents)
      // para que recalcule diferencias, valide cupos y refresque el total por
      // día del footer de la tabla. Disparar 'change' sobre un contentEditable
      // no reproduce el flujo de los inputs nativos.
      $cellDiv.trigger('keyup');

      // Refrescar explícitamente el total del footer y validar cupos disponibles.
      // DataTables clona el footer y en algunos navegadores el .trigger('keyup')
      // no propaga al handler legacy cuando se dispara sobre el original.
      // Devolvemos { updated, cellValue, diasDiff, rowKey } para que el caller
      // pueda saber cuánto quedó en la celda, qué día es y a qué fila
      // corresponde (necesario para mapear ediciones manuales posteriores).
      var rowKey = $fila.attr('data-vendedor') || '';

      if (window.controlEstados && typeof controlEstados.setTotalPorDia === 'function') {
        try {
          var validacionOK = controlEstados.getDiferenciasYValidar();
          controlEstados.setTotalPorDia();
          return {
            updated: validacionOK ? true : 'excede',
            cellValue: newVal,
            diasDiff: diasDiff,
            rowKey: rowKey
          };
        } catch (ex) {
          console.warn('[Matching] No se pudo refrescar el total del footer:', ex);
        }
      }

      return { updated: true, cellValue: newVal, diasDiff: diasDiff, rowKey: rowKey };
    }
    catch (ex) {
      console.warn('actualizarCeldaDistribucion error:', ex);
      return false;
    }
  }

  function normalizarValor(value) {
    return String(value == null ? '' : value).trim();
  }

  function normalizarVendedor(value) {
    var vendedor = normalizarValor(value);
    return vendedor === '0' ? '' : vendedor;
  }

  function parsearFechaJSON(value) {
    if (value == null) return null;
    if (value instanceof Date) return value;
    if (typeof value === 'number') return new Date(value);
    if (typeof value === 'string') {
      var s = value.trim();
      if (s === '') return null;
      // Formato WCF/Microsoft: /Date(1234567890123)/ o /Date(1234567890123+0200)/
      var m = /^\/Date\((-?\d+)([+\-]\d+)?\)\/$/.exec(s);
      if (m) {
        var ms = parseInt(m[1], 10);
        if (!isNaN(ms)) return new Date(ms);
      }
      // ISO 8601 u otros formatos estándar. Si no se puede parsear, devolver null
      // y dejar que el llamador trate la fecha como faltante.
      var d = new Date(s);
      return isNaN(d.getTime()) ? null : d;
    }
    return null;
  }

  // Serializa un valor de fecha a ISO 8601 que System.Text.Json del
  // backend acepta. Acepta Date, número en ms, ISO 8601 o el formato
  // WCF /Date(...)/. Devuelve null si no se puede parsear.
  function serializarFechaISO(value) {
    var d = parsearFechaJSON(value);
    if (!d || isNaN(d.getTime())) return null;
    // toISOString devuelve "YYYY-MM-DDTHH:mm:ss.sssZ" (UTC).
    return d.toISOString();
  }

  // Helper: CSS.escape para selectores jQuery seguros.
  function cssEscape(value) {
    return String(value == null ? '' : value).replace(/["\\]/g, '\\$&');
  }

  // ============================================================
  // Diálogos extras (mock "Otros diálogos operativos")
  // ============================================================

  // Helper unificado para mostrar uno de los tres modales del mock.
  // Reemplaza completamente al Swal.fire dentro del flujo de matching.
  //   tipo: 'confirm-obs' | 'conflict' | 'rechazo-auto'
  // Para cada tipo hay un set distinto de campos relevantes; ver
  // MatchingDistribucionPartial.cshtml para los IDs.
  // Flags comunes:
  //   noCerrarMatch (sólo conflict): si true, el botón de cierre solo
  //     oculta el modal de conflicto sin tocar la tabla de distribución
  //     ni el modal de match. Útil para validaciones en donde el
  //     operador debe ajustar cantidades y reintentar.
  function mostrarDialogo(opts) {
    if (!opts || !opts.tipo) return;
    var tipo = opts.tipo;

    if (tipo === 'confirm-obs') {
      // Header: configurable (default = "Confirmación con observaciones").
      if (opts.header) $('#confirm-obs-title').text(opts.header);
      // Título del diálogo (debajo del header).
      if (opts.titulo) $('#confirm-obs-message').text(opts.titulo);
      // Bloque de observaciones (amber). Si llega vacío/null, se oculta.
      var $texto = $('#confirm-obs-texto');
      if (opts.observaciones) {
        $texto.html('<b>Observaciones:</b><br />&laquo;' + escapeHtml(opts.observaciones) + '&raquo;').show();
      } else {
        $texto.hide();
      }
      // Copy debajo del callout.
      if (opts.copy) $('#confirm-obs-copy').html(opts.copy);
      // Meta footer (Solicitante: X · fecha).
      $('#confirm-obs-solicitante').html(opts.meta ? escapeHtml(opts.meta) : '');
      // Etiquetas de los botones.
      var $btnCancel = $('[data-action="confirm-obs-cancelar"]');
      var $btnConfirm = $('[data-action="confirm-obs-confirmar"]');
      if (opts.cancelarTexto) $btnCancel.text(opts.cancelarTexto);
      if (opts.confirmarTexto) $btnConfirm.text(opts.confirmarTexto);

      // Bind handlers (limpia los anteriores para evitar fugas).
      $(document).off('click.silDialogo', '[data-action="confirm-obs-confirmar"]');
      $(document).off('click.silDialogo', '[data-action="confirm-obs-cancelar"]');
      $(document).on('click.silDialogo', '[data-action="confirm-obs-confirmar"]', function () {
        hideOverlay('sil-modal-confirm-obs');
        if (typeof opts.onConfirm === 'function') opts.onConfirm();
      });
      $(document).on('click.silDialogo', '[data-action="confirm-obs-cancelar"]', function () {
        hideOverlay('sil-modal-confirm-obs');
        if (typeof opts.onCancel === 'function') opts.onCancel();
      });

      showOverlay('sil-modal-confirm-obs');
      return;
    }

    if (tipo === 'conflict') {
      if (opts.header) $('#conflict-title').text(opts.header);
      if (opts.titulo) $('#conflict-message').text(opts.titulo);
      if (opts.intro) $('#conflict-intro').html(opts.intro);
      if (opts.help) $('#conflict-help').html(opts.help);

      // Detalle (callout rojo). Si llega vacío/null, se oculta.
      var $det = $('#conflict-detail');
      if (opts.detalle) {
        $det.html(opts.detalle).show();
      } else {
        $det.hide();
      }

      // Botón: el flujo estándar es recargar la grilla. Si el llamador
      // quiere otra acción, se puede capturar via onClose y el handler
      // cancelar-modal-volver-grilla.
      var $btnClose = $('[data-action="conflict-cerrar-y-recargar"]');
      if (opts.botonTexto) $btnClose.text(opts.botonTexto);

      $(document).off('click.silDialogo', '[data-action="conflict-cerrar-y-recargar"]');
      $(document).on('click.silDialogo', '[data-action="conflict-cerrar-y-recargar"]', function () {
        // Modo "no cerrar match": sólo ocultamos el modal de conflicto
        // para que el operador pueda ajustar las cantidades en la
        // grilla del modal de matching y reintentar. NO recargamos la
        // página y NO tocamos la tabla de distribución.
        if (opts.noCerrarMatch) {
          hideOverlay('sil-modal-conflict');
          if (typeof opts.onClose === 'function') opts.onClose();
          return;
        }
        hideAllOverlays();
        if (typeof opts.onClose === 'function') {
          opts.onClose();
        } else {
          location.reload();
        }
      });

      showOverlay('sil-modal-conflict');
      return;
    }

    if (tipo === 'rechazo-auto') {
      if (opts.header) $('#rechazo-auto-title').text(opts.header);
      if (opts.titulo) $('#rechazo-auto-message').text(opts.titulo);
      if (typeof opts.resumen === 'string') $('#rechazo-auto-resumen').text(opts.resumen);
      if (opts.detalle) $('#rechazo-auto-detalle').text(opts.detalle);

      // Lista opcional.
      var $lista = $('#rechazo-auto-lista');
      if (opts.lista && opts.lista.length > 0) {
        var html = opts.lista.map(function (s) {
          return '&mdash; ' + escapeHtml(s);
        }).join('<br />');
        $lista.html(html).show();
      } else {
        $lista.empty().hide();
      }

      if (opts.metaFooter) $('#rechazo-auto-footer').text(opts.metaFooter);

      var $btnAceptar = $('[data-action="rechazo-auto-aceptar"]');
      if (opts.botonTexto) $btnAceptar.text(opts.botonTexto);

      $(document).off('click.silDialogo', '[data-action="rechazo-auto-aceptar"]');
      $(document).on('click.silDialogo', '[data-action="rechazo-auto-aceptar"]', function () {
        hideOverlay('sil-modal-rechazo-auto');
        if (typeof opts.onAccept === 'function') opts.onAccept();
      });

      showOverlay('sil-modal-rechazo-auto');
      return;
    }
  }

  // Backwards-compat: los callers existentes (prepararYMostrarConfirmacionObs,
  // doAccept, etc.) siguen llamando a mostrarConflicto / mostrarRechazoAutomatico.
  // Redirigen a mostrarDialogo con el shape del mock.
  function mostrarConflicto(mensaje, fallos) {
    var html = '<strong>' + escapeHtml(mensaje) + '</strong>';
    if (fallos && fallos.length > 0) {
      html += '<ul>';
      fallos.forEach(function (f) {
        html += '<li>Solicitud #' + escapeHtml(String(f.SolicitudId)) + ': ' + escapeHtml(f.Motivo) + '</li>';
      });
      html += '</ul>';
    }
    mostrarDialogo({
      tipo: 'conflict',
      header: 'Conflicto de concurrencia',
      titulo: 'La solicitud ya fue procesada',
      detalle: html
    });
  }

  function mostrarRechazoAutomatico(resumen, lista) {
    var total = (typeof resumen === 'number') ? resumen : (parseInt(resumen, 10) || 2);
    mostrarDialogo({
      tipo: 'rechazo-auto',
      header: 'Rechazo automático — cierre 18:00 hs',
      titulo: 'Proceso de cierre ejecutado',
      resumen: total + ' solicitudes',
      detalle: 'rechazadas automáticamente por vencimiento del día operativo.',
      lista: lista || [],
      metaFooter: 'Los solicitantes fueron notificados automáticamente.',
      botonTexto: 'Aceptar'
    });
  }

  // ============================================================
  // Handlers globales (delegados)
  // ============================================================
  $(document).ready(function () {

    // Cerrar modal con la X.
    $(document).on('click', '[data-action="cerrar-modal"]', function () {
      hideOverlay($(this).closest('.sil-modal-overlay').attr('id'));
    });

    // Clic fuera del card también cierra (overlay).
    $(document).on('click', '.sil-modal-overlay', function (e) {
      if (e.target === this) {
        hideOverlay(this.id);
      }
    });

    // Esc cierra.
    $(document).on('keydown', function (e) {
      if (e.key === 'Escape') hideAllOverlays();
    });

    // ── VARIANTE A: tabs ──
    $(document).on('click', '[data-tab-group="variant-a"]', function () {
      irATab($(this).data('tab'));
    });
    $(document).on('click', '[data-action="va-ir-a-paso-1"]', function () { irATab('va-step1'); });
    $(document).on('click', '[data-action="va-ir-a-paso-2"]', function () { irATab('va-step2'); });
    $(document).on('click', '[data-action="va-ir-a-paso-3"]', function () { irATab('va-step3'); });

    // ± en step1 (directo) y step3 (parcial).
    $(document).on('click', '[data-va-step1-decr], [data-va-step1-incr]', function () {
      var $b = $(this);
      var solId = $b.data('solicitud');
      var $inp = $('[data-va-step1-input][data-solicitud="' + solId + '"]');
      var max = parseInt($inp.attr('max'), 10) || 0;
      var cur = parseInt($inp.val(), 10) || 0;
      var inc = $b.data('vaStep1Incr') !== undefined ? +1 : -1;
      cur = Math.max(0, Math.min(max, cur + inc));
      $inp.val(cur);
    });

    // ± en step3.
    $(document).on('click', '[data-va-step3-decr], [data-va-step3-incr]', function () {
      var $b = $(this);
      var solId = $b.data('solicitud');
      var $inp = $('[data-va-step3-input][data-solicitud="' + solId + '"]');
      var max = parseInt($inp.attr('max'), 10) || 0;
      var cur = parseInt($inp.val(), 10) || 0;
      var inc = $b.data('vaStep3Incr') !== undefined ? +1 : -1;
      cur = Math.max(0, Math.min(max, cur + inc));
      $inp.val(cur);
    });

    // Confirmar directo / parcial en Variante A.
    $(document).on('click', '[data-action="va-confirmar-directo"]', function () {
      var cupo = estado.cupoActual;
      if (!cupo || !cupo.Matches) return;
      var solicitudes = [];
      $('[data-va-step1-input]').each(function () {
        var $i = $(this);
        var solId = parseInt($i.data('solicitud'), 10);
        var cant = parseInt($i.val(), 10) || 0;
        if (cant > 0) {
          var m = (cupo.Matches || []).find(function (mm) { return mm.Id === solId; });
          if (m) solicitudes.push({ cupoId: cupo.Id || 0, solicitudId: solId, matchType: 'Directo', cantidad: cant });
        }
      });
      if (solicitudes.length === 0) {
        mostrarDialogo({
          tipo: 'rechazo-auto',
          header: 'Nada seleccionado',
          titulo: 'Ajustá las cantidades antes de confirmar',
          resumen: '0 cupos',
          detalle: 'pendientes de asignación en el paso 1.',
          lista: [],
          metaFooter: 'Ingresá al menos una cantidad mayor a 0 en las solicitudes del paso 1.',
          botonTexto: 'Aceptar'
        });
        return;
      }
      var totalCupos = solicitudes.reduce(function (a, s) { return a + (s.cantidad || 1); }, 0);
      confirmarDistribucion(totalCupos, solicitudes.length).then(function (ok) {
        if (!ok) return;
        prepararYMostrarConfirmacionObs(function () {
          doAccept(solicitudes);
        });
      });
    });

    $(document).on('click', '[data-action="va-confirmar-parcial"]', function () {
      var cupo = estado.cupoActual;
      var solicitudes = [];
      $('[data-va-step3-input]').each(function () {
        var $i = $(this);
        var solId = parseInt($i.data('solicitud'), 10);
        var cant = parseInt($i.val(), 10) || 0;
        if (cant > 0) {
          var m = (cupo.Matches || []).find(function (mm) { return mm.Id === solId; });
          if (m) solicitudes.push({ cupoId: cupo.Id || 0, solicitudId: solId, matchType: m.MatchType, cantidad: cant });
        }
      });
      if (solicitudes.length === 0) {
        mostrarDialogo({
          tipo: 'rechazo-auto',
          header: 'Nada seleccionado',
          titulo: 'Ajustá las cantidades antes de confirmar',
          resumen: '0 cupos',
          detalle: 'pendientes de asignación en el paso 3.',
          lista: [],
          metaFooter: 'Ingresá al menos una cantidad mayor a 0 en las solicitudes del paso 3.',
          botonTexto: 'Aceptar'
        });
        return;
      }
      var totalCupos = solicitudes.reduce(function (a, s) { return a + (s.cantidad || 1); }, 0);
      confirmarDistribucion(totalCupos, solicitudes.length).then(function (ok) {
        if (!ok) return;
        prepararYMostrarConfirmacionObs(function () {
          doAccept(solicitudes);
        });
      });
    });

    // ── VARIANTE B: cancelar / confirmar ──
    $(document).on('click', '[data-action="vb-cancelar"]', function () {
      hideOverlay('sil-modal-variant-b');
    });

    // ± en inputs numéricos de la subtabla por fecha (data-vb-input-day).
    // El cap del input es m.Cupos[].length (los cupos que matchearon esta
    // solicitud). El tope per-vendedor (Cupostotalesadist) NO se clamp
    // acá — se valida al confirmar, para que el operador vea los números
    // que tipeó y entienda el motivo del rechazo en el Swal.
    $(document).on('click', '[data-vb-decr-day], [data-vb-incr-day]', function () {
      var $b = $(this);
      var grupoIdx = $b.data('grupo');
      var recordIdx = $b.data('record');
      var $inp = $('[data-vb-input-day][data-grupo="' + grupoIdx + '"][data-record="' + recordIdx + '"]');
      if ($inp.length === 0 || $inp.prop('disabled')) return;
      var max = parseInt($inp.attr('max'), 10) || 0;
      var cur = parseInt($inp.val(), 10) || 0;
      var inc = $b.is('[data-vb-incr-day]') ? +1 : -1;
      cur = Math.max(0, Math.min(max, cur + inc));
      $inp.val(cur);
      var solId = String($inp.data('solicitud'));
      if (estado.seleccionados[solId]) estado.seleccionados[solId].cantidad = cur;
      actualizarBarraVB();
      actualizarConflictoVisual();
    });

    // Cambio manual en el input: clamp sólo al max estático (m.Cupos.length),
    // NO al cap per-vendor. La validación per-vendor se hace al confirmar.
    $(document).on('input change', '[data-vb-input-day]', function () {
      var $i = $(this);
      if ($i.prop('disabled')) return;
      var max = parseInt($i.attr('max'), 10) || 0;
      var raw = parseInt($i.val(), 10);
      if (isNaN(raw) || raw < 0) raw = 0;
      if (raw > max) raw = max;
      $i.val(raw);
      var solId = String($i.data('solicitud'));
      if (estado.seleccionados[solId]) estado.seleccionados[solId].cantidad = raw;
      actualizarBarraVB();
      actualizarConflictoVisual();
    });

    // Expandir / colapsar la subtabla por fecha de un solicitante.
    $(document).on('click', '[data-vb-toggle]', function () {
      var $btn = $(this);
      var grupoIdx = $btn.data('grupo');
      var $det = $('[data-grupo-detail="' + grupoIdx + '"]');
      if ($det.length === 0) return;
      var open = $det.is(':visible');
      $det.toggle(!open);
      $btn.attr('aria-expanded', open ? 'false' : 'true')
          .html(open ? '&#9662; ver' : '&#9652; cerrar');
    });

    // Checkbox del grupo: habilita / deshabilita los inputs de la subtabla.
    // Para grupos Condicionales sirve como opt-in; para los demás, mantiene
    // el comportamiento anterior (incluir / excluir el grupo del total).
    $(document).on('change', '[data-vb-chk-grupo]', function () {
      var $chk = $(this);
      var grupoIdx = $chk.data('grupo');
      var enabled = $chk.is(':checked');
      $('[data-vb-input-day][data-grupo="' + grupoIdx + '"]').each(function () {
        var $inp = $(this);
        var solId = String($inp.data('solicitud'));
        var s = estado.seleccionados[solId];
        $inp.prop('disabled', !enabled);
        if (s) s.grupoChecked = enabled;
      });
      $('[data-vb-decr-day][data-grupo="' + grupoIdx + '"], [data-vb-incr-day][data-grupo="' + grupoIdx + '"]')
        .prop('disabled', !enabled);
      actualizarBarraVB();
      actualizarConflictoVisual();
    });

    $(document).on('click', '[data-action="vb-confirmar"]', function () {
      var solicitudes = [];
      // Leemos directo del DOM para tomar el valor vigente del input de la
      // subtabla por fecha, aunque s.cantidad ya esté sincronizado por los
      // handlers ± / change de arriba. Sólo se incluyen los inputs cuyo
      // grupo padre está marcado (grupoChecked).
      $('[data-vb-input-day]').each(function () {
        var $i = $(this);
        var solId = String($i.data('solicitud'));
        var s = estado.seleccionados[solId];
        if (!s || !s.grupoChecked) return;
        var cant = parseInt($i.val(), 10) || 0;
        if (cant <= 0) return;
        solicitudes.push({
          cupoId: s.cupoIds && s.cupoIds[0] ? s.cupoIds[0] : 0,  // sentinela, doAccept ignora esto y lee de m.Cupos
          solicitudId: s.solicitudId,
          matchType: s.matchType,
          cantidad: cant
        });
      });
      if (solicitudes.length === 0) {
        mostrarDialogo({
          tipo: 'rechazo-auto',
          header: 'Nada seleccionado',
          titulo: 'Ingresá al menos una cantidad mayor a 0',
          resumen: '0 cupos',
          detalle: 'pendientes de asignación.',
          lista: [],
          metaFooter: 'Ajustá las cantidades en las filas de la tabla y volvé a confirmar.',
          botonTexto: 'Aceptar'
        });
        return;
      }

      // Bloqueo per-vendedor: si la suma de cupos tipeados en todas las
      // filas del mismo CUIT supera Cupostotalesadist, no dejamos
      // confirmar. La validación es NO clamp: el operador ve los
      // números que tipeó en cada fila (aunque sumen más que el límite)
      // y acá le explicamos exactamente cuál CUIT se pasó, por cuánto,
      // y el nombre del solicitante para que ubique la fila.
      var vends = totalesPorVendedor();
      if (typeof console !== 'undefined' && console.debug) {
        console.debug('[Matching] validar excedente per-vendor:', {
          cupoId: estado.cupoActual && estado.cupoActual.Id,
          cupostotalesadist: estado.cupoActual && estado.cupoActual.Cupostotalesadist,
          cupostotales: estado.cupoActual && estado.cupoActual.CuposTotales,
          lookupVendedor: vends.lookup,
          totalesPorVendedor: vends.totales
        });
      }
      var vendedoresExcedidos = [];
      // Si el lookup por vendor no tiene entradas (caso edge: matches
      // vacíos o keys no coincidentes), caemos a comparar contra el
      // Cupostotalesadist del cupo como defensa. Así, aunque el lookup
      // falle, la validación sigue disparándose con el límite correcto.
      var keys = Object.keys(vends.lookup);
      if (keys.length === 0 && estado.cupoActual) {
        var cupostotalesadistCopo = estado.cupoActual.Cupostotalesadist
          || estado.cupoActual.CuposTotales || 0;
        var totalGlobal = Object.keys(vends.totales)
          .reduce(function (a, k) { return a + (vends.totales[k] || 0); }, 0);
        if (cupostotalesadistCopo > 0 && totalGlobal > cupostotalesadistCopo) {
          var nombreFallback = '';
          Object.keys(estado.seleccionados).some(function (k) {
            var ss = estado.seleccionados[k];
            if (ss && ss.solicitud && ss.solicitud.NombreVendedor) {
              nombreFallback = ss.solicitud.NombreVendedor;
              return true;
            }
            return false;
          });
          vendedoresExcedidos.push({
            cuit: '(lookup no poblado)',
            nombre: nombreFallback,
            solicitado: totalGlobal,
            limite: cupostotalesadistCopo,
            excedente: totalGlobal - cupostotalesadistCopo
          });
        }
      } else {
        keys.forEach(function (v) {
          var limite = vends.lookup[v] || 0;
          var total = vends.totales[v] || 0;
          if (limite > 0 && total > limite) {
            // Buscar un nombre humano del vendor en cualquier solicitud del
            // estado para mostrarlo junto al CUIT en el mensaje.
            var nombreVendor = '';
            Object.keys(estado.seleccionados).some(function (k) {
              var ss = estado.seleccionados[k];
              if (ss && ss.solicitud && String(ss.solicitud.Vendedor) === v && ss.solicitud.NombreVendedor) {
                nombreVendor = ss.solicitud.NombreVendedor;
                return true;
              }
              return false;
            });
            vendedoresExcedidos.push({
              cuit: v,
              nombre: nombreVendor,
              solicitado: total,
              limite: limite,
              excedente: total - limite
            });
          }
        });
      }
      if (vendedoresExcedidos.length > 0) {
        var lineasVend = vendedoresExcedidos.map(function (ve) {
          var header = ve.nombre
            ? '<b>' + escapeHtml(ve.nombre) + '</b> (CUIT ' + escapeHtml(ve.cuit) + ')'
            : '<b>CUIT ' + escapeHtml(ve.cuit) + '</b>';
          return header + ': querés asignar <b>' + ve.solicitado +
                 '</b> cupos pero su Cupos Totales a Distribuir en la tabla de distribución es <b>' +
                 ve.limite + '</b> (' + ve.excedente + ' de más). Reducí las cantidades de este CUIT antes de confirmar.';
        });
        mostrarDialogo({
          tipo: 'conflict',
          header: 'Cupos Totales a Distribuir excedido por vendedor',
          titulo: 'La suma de cupos por CUIT supera el disponible',
          intro: 'La suma de cupos a distribuir por cada CUIT no puede superar su Cupos Totales a Distribuir (los cupos pendientes en la tabla de distribución):',
          help: '<br>' + lineasVend.join('<br><br>'),
          detalle: null,
          botonTexto: 'Entendido, voy a ajustar',
          // No recargar ni cerrar el modal de match: el operador debe
          // ajustar cantidades y volver a intentar la confirmación.
          noCerrarMatch: true
        });
        return;
      }

      // Validar que un mismo cupo no quede asignado a dos solicitudes.
      // detectarConflictosCupos ahora lee directo de estado.seleccionados.
      var conflicto = detectarConflictosCupos();
      if (conflicto.hayConflictos) {
        actualizarConflictoVisual();
        var lineas = [];
        Object.keys(conflicto.conflictosPorSolicitud).forEach(function (solId) {
          var nombre = conflicto.nombresPorSolicitud[solId] || ('Solicitud #' + solId);
          var cupos = conflicto.conflictosPorSolicitud[solId].join(', ');
          lineas.push('<b>' + escapeHtml(nombre) + '</b>: cupos ' + escapeHtml(cupos));
        });
        mostrarDialogo({
          tipo: 'conflict',
          header: 'Conflicto de cupos entre solicitudes',
          titulo: 'Hay cupos asignados a más de una solicitud',
          intro: 'Los siguientes cupos están asignados a más de una solicitud. Reducí las cantidades y elegí en cuál solicitud los querés dejar.',
          help: '<br>' + lineas.join('<br>'),
          detalle: null,
          botonTexto: 'Entendido',
          // No recargar ni cerrar el modal de match: el operador debe
          // ajustar cantidades y volver a intentar la confirmación.
          noCerrarMatch: true
        });
        return;
      }

      var totalCupos = solicitudes.reduce(function (a, s) { return a + (s.cantidad || 1); }, 0);
      confirmarDistribucion(totalCupos, solicitudes.length).then(function (ok) {
        if (!ok) return;
        prepararYMostrarConfirmacionObs(function () {
          doAccept(solicitudes);
        });
      });
    });

    // ── Conflicto ──
    $(document).on('click', '[data-action="conflict-cerrar-y-recargar"]', function () {
      hideAllOverlays();
      location.reload();
    });

    // ── Rechazo automático ──
    $(document).on('click', '[data-action="rechazo-auto-aceptar"]', function () {
      hideOverlay('sil-modal-rechazo-auto');
    });
  });

})(jQuery);
