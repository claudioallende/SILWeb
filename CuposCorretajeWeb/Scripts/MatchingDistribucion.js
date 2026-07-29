/* =========================================================
   SIL — Matching en Distribución de Cupos (Pantalla 3)
   Lógica de los modales de matching invocados desde
   Views/Cupos/Distribucion.cshtml cuando el operador
   presiona el botón "Buscar" existente (sin reemplazar
   el flujo legacy).

   Stack: jQuery 1.10.2 + Bootstrap 3 + Swal.fire (CDN).
   No utiliza el loader global: la tabla y la grilla permanecen disponibles
   mientras se consulta el matching.
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
    $('#' + id).addClass('is-open');
  }
  function hideOverlay(id) {
    $('#' + id).removeClass('is-open');
  }
  function hideAllOverlays() {
    $('.sil-modal-overlay').removeClass('is-open');
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
      var dt = (typeof d === 'string') ? new Date(d) : d;
      if (isNaN(dt.getTime())) return '';
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
  function confirmarDistribucion(totalCupos, totalSolicitudes) {
    if (typeof Swal === 'undefined') return $.Deferred().resolve(true).promise();

    return Swal.fire({
      icon: 'warning',
      title: 'Aceptar este match distribuirá los cupos',
      html: 'Se asignarán <b>' + (totalCupos || 0) + '</b> cupo(s) a ',
      showCancelButton: true,
      confirmButtonText: 'Aceptar y distribuir',
      cancelButtonText: 'Cancelar',
      reverseButtons: true,
      allowOutsideClick: false,
      allowEscapeKey: false
    }).then(function (r) { return !!(r && r.isConfirmed); });
  }

  // ── API expuesta ─────────────────────────────────────────
  window.SILMatching = {
    /**
     * Abre el modal de matching para el primer cupo en `cuposViewModel`.
     * Decide Variante A o B según si el cupo trae CodVendSIL poblado.
     */
    abrir: function (cuposViewModel) {
      if (!cuposViewModel || cuposViewModel.length === 0) {
        if (typeof Swal !== 'undefined') {
          Swal.fire({ icon: 'info', title: 'Sin cupos', text: 'No se encontraron cupos en ACA_SILData con esos filtros.' });
        }
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
        url: '/CuposMatching/BuscarCuposConMatch',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(filtroForm)
      });
    },

    cerrar: hideAllOverlays,
    mostrarRechazoAutomatico: mostrarRechazoAutomatico,
    mostrarConflicto: mostrarConflicto,

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
  function renderVarianteA(cupo) {
    // Subtítulo del cupo.
    $('#va-title-cupo').text('Match directo detectado');
    var subtitleParts = [];
    if (cupo.CuposTotales) subtitleParts.push(cupo.CuposTotales + ' cupos');
    if (cupo.NomGrano) subtitleParts.push(cupo.NomGrano);
    if (cupo.NomCompSIL) subtitleParts.push(cupo.NomCompSIL);
    if (cupo.NomVendSIL) subtitleParts.push('Vendedor: ' + cupo.NomVendSIL);
    $('#va-subtitle-cupo').text(subtitleParts.join(' · ') || 'Cupo distribuido');

    // Clasificar matches.
    var dirs = [];
    var pars = [];
    var conds = [];
    (cupo.Matches || []).forEach(function (m) {
      var tipo = m.MatchType || 'Parcial';
      if (tipo === 'Directo') dirs.push(m);
      else if (tipo === 'Condicional') conds.push(m);
      else pars.push(m);
    });

    // STEP 1 — match directo detectado.
    var headline = '';
    if (dirs.length > 0) {
      headline = '<b>' + dirs.length + '</b> solicitud(es) con match directo de ' + cupo.CuposTotales + ' cupos disponibles.';
      $('#va-step1-headline').html(headline);
      var list = dirs.map(function (m) {
        var maxDirect = m.Cantidad || 1;
        return '<div class="sil-match-row is-direct">' +
          '  <div class="sil-match-row-info">' +
          '    <b>' + escapeHtml(m.Vendedor) + '</b>' +
          '    <span>' + formatFechaCorta(m.FechaSolicitado) + ' &middot; ' + maxDirect + ' cupo(s) solicitado(s)</span>' +
          '  </div>' +
          '  <div class="sil-modal-qty">' +
          '    <button type="button" data-va-step1-decr data-solicitud="' + m.Id + '">&minus;</button>' +
          '    <input type="number" min="0" max="' + maxDirect + '" value="' + maxDirect + '" data-va-step1-input data-solicitud="' + m.Id + '" />' +
          '    <button type="button" data-va-step1-incr data-solicitud="' + m.Id + '">&plus;</button>' +
          '  </div>' +
          '  <span class="sil-badge sil-badge-dir">Match directo</span>' +
          '</div>';
      }).join('');
      $('#va-step1-list').html(list);
    } else {
      $('#va-step1-headline').html('<b>0</b> solicitudes con match directo');
      $('#va-step1-list').html('<em style="color: var(--sil-fg-muted);">No se detectaron solicitudes con match directo.</em>');
    }

    // STEP 2 — confirmación.
    $('#va-step2-warning-headline').text('Los ' + dirs.length + ' cupos disponibles comparten vendedor y grano con solicitudes detectadas.');

    // STEP 3 — matches parciales (incluye Condicionales como "con obs").
    var parciales = pars.concat(conds);
    var partialHtml = '';
    if (parciales.length > 0) {
      partialHtml += '<table class="sil-modal-table">';
      partialHtml += '<thead><tr><th class="tl">Solicitante</th><th>Sols.</th><th>Match</th><th>Comprador en solicitud</th><th>M&aacute;s antigua</th><th>Cupos a asignar</th></tr></thead><tbody>';
      parciales.forEach(function (m) {
        var compradorTxt = m.Comprador ? m.Comprador : '<i style="color:var(--sil-amber);">&mdash; vac&iacute;o</i>';
        var matchBadge = m.MatchType === 'Condicional'
          ? '<span class="sil-badge sil-badge-obs">Con obs.</span>'
          : '<span class="sil-badge sil-badge-par">Parcial</span>';
        partialHtml += '<tr style="background:#fff;">';
        partialHtml += '  <td class="tl"><div style="font-weight:600;">' + escapeHtml(m.Vendedor) + '</div></td>';
        partialHtml += '  <td>' + (m.Cantidad || 1) + '</td>';
        partialHtml += '  <td>' + matchBadge + '</td>';
        partialHtml += '  <td>' + compradorTxt + '</td>';
        partialHtml += '  <td>' + formatFechaCorta(m.FechaSolicitado) + '</td>';
        partialHtml += '  <td>';
        partialHtml += '    <div class="sil-modal-qty">';
        partialHtml += '      <button type="button" data-va-step3-decr data-solicitud="' + m.Id + '">&minus;</button>';
        partialHtml += '      <input type="number" min="0" max="' + (m.Cantidad || 1) + '" value="' + (m.Cantidad || 1) + '" data-va-step3-input data-solicitud="' + m.Id + '" />';
        partialHtml += '      <button type="button" data-va-step3-incr data-solicitud="' + m.Id + '">&plus;</button>';
        partialHtml += '    </div>';
        partialHtml += '    <div style="font-size: 10px; color: var(--sil-fg-muted);">m&aacute;x. ' + (m.Cantidad || 1) + '</div>';
        partialHtml += '  </td>';
        partialHtml += '</tr>';
      });
      partialHtml += '</tbody></table>';
    } else {
      partialHtml = '<em style="color: var(--sil-fg-muted);">No hay matches parciales disponibles.</em>';
    }
    $('#va-step3-table-wrap').html(partialHtml);

    // Reset tab.
    irATab('va-step1');
  }

  function irATab(tabId) {
    $('[data-tab-group="variant-a"]').removeClass('is-active');
    $('[data-tab="' + tabId + '"]').addClass('is-active');
    $('.sil-modal-tab-step').removeClass('is-active');
    $('#' + tabId).addClass('is-active');
  }

  // ============================================================
  // VARIANTE B — Cupo sin vendedor (tabla expandible)
  // ============================================================
  function renderVarianteB(cupo) {
    $('#vb-subtitle-cupo').text(
      (cupo.CuposTotales || 0) + ' cupos · ' + (cupo.NomGrano || '') +
      ' · ' + (cupo.NomCompSIL || 'Sin comprador') + ' · Sin vendedor');

    // Agrupar matches por vendedor (el solicitante).
    var groups = {};   // { vendedor: { solicitante, sols, oldestFecha, matches, matchTypeAggregated } }
    (cupo.Matches || []).forEach(function (m) {
      var key = m.Vendedor || '(sin solicitante)';
      if (!groups[key]) {
        groups[key] = {
          solicitante: key,
          sols: [],
          oldestFecha: m.FechaSolicitado,
          matches: [],
          hayCondicional: false,
          hayDirecto: false
        };
      }
      groups[key].sols.push(m);
      groups[key].matches.push(m);
      if ((m.FechaSolicitado || new Date(2099, 0, 1)) < groups[key].oldestFecha) {
        groups[key].oldestFecha = m.FechaSolicitado;
      }
      if (m.MatchType === 'Condicional') groups[key].hayCondicional = true;
      if (m.MatchType === 'Directo') groups[key].hayDirecto = true;
    });

    var groupList = Object.values(groups);
    var totalSols = groupList.reduce(function (acc, g) { return acc + g.sols.length; }, 0);
    var totalCuposAsignar = cupo.CuposTotales || 0;

    $('#vb-counter-total').text(totalCuposAsignar);
    $('#vb-counter-solicitantes').text(groupList.length);
    $('#vb-counter-total-sols').text(totalSols);
    $('#vb-counter-asignados-max').text(totalCuposAsignar);

    // Tabla principal.
    var tableHtml = '<table class="sil-modal-table"><thead><tr>';
    tableHtml += '<th class="tl"></th><th class="tl">Solicitante (posible vendedor)</th>';
    tableHtml += '<th>Sols.</th><th>M&aacute;s antigua</th><th>Match</th>';
    tableHtml += '<th>Total a asignar</th><th></th>';
    tableHtml += '</tr></thead><tbody>';

    estado.seleccionados = {};

    groupList.forEach(function (g, idx) {
      var tipoAg = g.hayCondicional ? 'Condicional' : (g.hayDirecto ? 'Directo' : 'Parcial');
      var badge = tipoAg === 'Directo'
        ? '<span class="sil-badge sil-badge-dir">Match directo</span>'
        : (tipoAg === 'Condicional'
          ? '<span class="sil-badge sil-badge-obs">Con obs.</span>'
          : '<span class="sil-badge sil-badge-par">Parcial</span>');

      var detalleId = 'vb-detail-' + idx;
      var chkId = 'vb-chk-' + idx;
      var inputId = 'vb-qty-' + idx;
      var totalId = 'vb-total-' + idx;

      var sumSolsCantidad = g.sols.reduce(function (a, s) { return a + (s.Cantidad || 1); }, 0);
      var maxAsignar = Math.min(sumSolsCantidad, totalCuposAsignar);

      tableHtml += '<tr style="background:#fff;">';
      tableHtml += '  <td><input type="checkbox" checked id="' + chkId + '" data-vb-grp="' + idx + '" /></td>';
      tableHtml += '  <td class="tl"><div style="font-weight:600;">' + escapeHtml(g.solicitante) + '</div>';
      tableHtml += '    <div style="font-size:10.5px; color:var(--sil-fg-muted);">Zona: ' + escapeHtml(cupo.NomDestino || '') + '</div></td>';
      tableHtml += '  <td>' + g.sols.length + '</td>';
      tableHtml += '  <td>' + formatFechaCorta(g.oldestFecha) + '</td>';
      tableHtml += '  <td>' + badge + '</td>';
      tableHtml += '  <td>';
      tableHtml += '    <strong id="' + totalId + '" style="font-size:14px; font-family: var(--sil-mono); color: var(--sil-navy);">' + maxAsignar + '</strong>';
      tableHtml += '    <div style="font-size:10px; color:var(--sil-fg-muted);">m&aacute;x. ' + maxAsignar + '</div>';
      tableHtml += '  </td>';
      tableHtml += '  <td><button type="button" class="sil-btn" data-vb-toggle data-idx="' + idx + '" style="background:none; border:1px solid var(--sil-border); padding:3px 8px; font-size:11px; color:var(--sil-fg);">&#9662; ver</button></td>';
      tableHtml += '</tr>';

      // Detalle expandible.
      tableHtml += '<tr id="' + detalleId + '" style="display:none;">';
      tableHtml += '  <td colspan="7" style="padding:0; border-bottom: 1px solid var(--sil-border-light);">';

      if (g.hayCondicional) {
        var obs = g.sols.find(function (s) { return s.MatchType === 'Condicional' && s.Observacion; });
        var obsTxt = obs && obs.Observacion ? obs.Observacion : 'La solicitud tiene condiciones registradas. Verificar antes de asignar.';
        tableHtml += '    <div class="sil-modal-expanded sil-modal-expanded-obs">';
        tableHtml += '      <div class="sil-modal-obs-callout">';
        tableHtml += '        <b>&#9888; Observaciones:</b>';
        tableHtml += '        ' + escapeHtml(obsTxt);
        tableHtml += '      </div>';
        tableHtml += '    </div>';
      } else {
        tableHtml += '    <div class="sil-modal-expanded">';
      }

      // Tabla de días con +/-.
      tableHtml += '      <div style="padding: 10px 14px;">';
      tableHtml += '        <table class="sil-modal-table">';
      tableHtml += '          <thead><tr><th class="tl">Fecha sol.</th><th>Ingresada</th><th>Sols. TR</th><th>Cupos a asignar</th></tr></thead><tbody>';
      g.sols.forEach(function (s, sidx) {
        var sidChk = chkId;
        var sgId = 'vb-sg-' + idx + '-' + sidx;
        var maxDia = s.Cantidad || 1;
        tableHtml += '<tr style="background:#fff;">';
        tableHtml += '  <td class="tl">' + formatFechaCorta(s.FechaSolicitado) + '</td>';
        tableHtml += '  <td style="text-align:center; font-size:11px;">' + (s.AntiguedadDias || 0) + ' d&iacute;a(s)</td>';
        tableHtml += '  <td>' + maxDia + '</td>';
        tableHtml += '  <td>';
        tableHtml += '    <div class="sil-modal-qty">';
        tableHtml += '      <button type="button" data-vb-day-decr data-grp="' + idx + '" data-sg="' + sgId + '">&minus;</button>';
        tableHtml += '      <input type="number" min="0" max="' + maxDia + '" value="' + maxDia + '" id="' + sgId + '" data-vb-day-input data-grp="' + idx + '" />';
        tableHtml += '      <button type="button" data-vb-day-incr data-grp="' + idx + '" data-sg="' + sgId + '">&plus;</button>';
        tableHtml += '    </div>';
        tableHtml += '  </td>';
        tableHtml += '</tr>';
      });
      tableHtml += '        </tbody></table>';
      tableHtml += '      </div>';
      tableHtml += '    </div>';
      tableHtml += '  </td>';
      tableHtml += '</tr>';

      // Estado inicial.
      estado.seleccionados[idx] = {
        checked: true,
        max: maxAsignar,
        sum: maxAsignar,
        g: g,
        cupoId: cupo.Id
      };
    });

    tableHtml += '</tbody></table>';
    $('#vb-table-wrap').html(tableHtml);

    // Aplicar clases CSS a filas según estado.
    Object.keys(estado.seleccionados).forEach(function (k) {
      var s = estado.seleccionados[k];
      var chkId = 'vb-chk-' + k;
      var totalId = 'vb-total-' + k;
      $(document).on('change', '#' + chkId, function () {
        s.checked = this.checked;
        if (!s.checked) s.sum = 0; else s.sum = s.max;
        $('#' + totalId).text(s.sum);
        actualizarBarraVB();
      });
    });

    // Botón expandir/colapsar.
    $(document).off('click', '[data-vb-toggle]').on('click', '[data-vb-toggle]', function () {
      var idx = $(this).data('idx');
      var $det = $('#vb-detail-' + idx);
      var open = $det.is(':visible');
      $det.toggle(!open);
      $(this).html(open ? '&#9662; ver' : '&#9652; cerrar');
    });

    // +/− por día.
    $(document).off('click', '[data-vb-day-decr], [data-vb-day-incr]')
      .on('click', '[data-vb-day-decr], [data-vb-day-incr]', function () {
        var $b = $(this);
        var grp = $b.data('grp');
        var sg = $b.data('sg');
        var $inp = $('#' + sg);
        var max = parseInt($inp.attr('max'), 10) || 0;
        var cur = parseInt($inp.val(), 10) || 0;
        var inc = $b.data('vbDayIncr') !== undefined ? +1 : -1;
        cur = Math.max(0, Math.min(max, cur + inc));
        $inp.val(cur);
        recalcularGrupoVB(grp);
      });

    $(document).off('change', '[data-vb-day-input]').on('change', '[data-vb-day-input]', function () {
      var $inp = $(this);
      var max = parseInt($inp.attr('max'), 10) || 0;
      var cur = parseInt($inp.val(), 10) || 0;
      cur = Math.max(0, Math.min(max, cur));
      $inp.val(cur);
      recalcularGrupoVB($(this).data('grp'));
    });

    actualizarBarraVB();
  }

  function recalcularGrupoVB(grp) {
    var s = estado.seleccionados[grp];
    if (!s) return;
    var sum = 0;
    s.g.sols.forEach(function (_, idx) {
      var sgId = 'vb-sg-' + grp + '-' + idx;
      sum += parseInt($('#' + sgId).val(), 10) || 0;
    });
    s.sum = sum;
    if (s.checked) $('#vb-total-' + grp).text(sum);
    else $('#vb-total-' + grp).text(0);
    actualizarBarraVB();
  }

  function actualizarBarraVB() {
    var asignado = 0;
    Object.keys(estado.seleccionados).forEach(function (k) {
      var s = estado.seleccionados[k];
      if (s.checked) asignado += s.sum;
    });
    var max = estado.cupoActual && estado.cupoActual.CuposTotales ? estado.cupoActual.CuposTotales : 0;
    var pct = max > 0 ? Math.min(100, Math.round(asignado / max * 100)) : 0;
    $('#vb-progress-fill').css({ width: pct + '%', background: asignado >= max ? 'var(--sil-green)' : 'var(--sil-navy)' });
    $('#vb-progress-label').text(asignado + ' / ' + max);
    $('#vb-counter-asignados').text(asignado);
    $('#vb-progress-warn').toggleClass('is-visible', asignado < max);
  }

  // ============================================================
  // Confirmar / Accept / Reject
  // ============================================================
  function prepararYMostrarConfirmacionObs(onContinue) {
    var hayCond = Object.keys(estado.seleccionados).some(function (k) {
      var s = estado.seleccionados[k];
      if (!s.checked) return false;
      return s.g.hayCondicional;
    });
    if (!hayCond) { onContinue(); return; }

    var obsTxt = '(sin texto)';
    var solicitante = '';
    Object.keys(estado.seleccionados).forEach(function (k) {
      var s = estado.seleccionados[k];
      if (!s.g.hayCondicional) return;
      var solCond = s.g.sols.find(function (x) { return x.MatchType === 'Condicional'; });
      if (solCond) {
        if (solCond.Observacion) obsTxt = solCond.Observacion;
        solicitante = solCond.Vendedor || '';
      }
    });
    $('#confirm-obs-texto').html('<b>Observaciones:</b><br />&laquo;' + escapeHtml(obsTxt) + '&raquo;');
    $('#confirm-obs-solicitante').html(solicitante ? 'Solicitante: <strong>' + escapeHtml(solicitante) + '</strong>' : '');
    showOverlay('sil-modal-confirm-obs');

    // Botones.
    $(document).off('click', '[data-action="confirm-obs-confirmar"]').on('click', '[data-action="confirm-obs-confirmar"]', function () {
      hideOverlay('sil-modal-confirm-obs');
      onContinue();
    });
    $(document).off('click', '[data-action="confirm-obs-cancelar"]').on('click', '[data-action="confirm-obs-cancelar"]', function () {
      hideOverlay('sil-modal-confirm-obs');
    });
  }

  function doAccept(solicitudesAsignadas) {
    // Envía las asociaciones al backend (/CuposMatching/AceptarMatch).
    // Muestra overlay bloqueante, refresca la tabla al recibir respuesta OK.
    //
    // solicitudesAsignadas: Array<{ cupoId, solicitudId, matchType, cantidad? }>

    var cupo = estado.cupoActual;
    if (!cupo) return;

    // Si no hay solicitudes válidas, no hacer nada.
    if (!Array.isArray(solicitudesAsignadas) || solicitudesAsignadas.length === 0) return;

    var totalCupos = solicitudesAsignadas.reduce(function (acc, p) {
      return acc + (typeof p.cantidad === 'number' && p.cantidad > 0 ? p.cantidad : 1);
    }, 0);

    // Construir payload ShiftRequestAcceptDataViewModel con los pares
    // solicitud-cupo. Cada match → 1 entry en ShiftRequest y 1 en
    // CuposToBeDistributed (Cantidad=1 por constraint del backend).
    var shiftRequest = [];
    var cuposToBeDistributed = [];

    solicitudesAsignadas.forEach(function (pair) {
      var match = (cupo.Matches || []).find(function (m) { return m.Id === pair.solicitudId; });
      if (!match) return;

      var cantidad = (typeof pair.cantidad === 'number' && pair.cantidad > 0) ? pair.cantidad : 1;

      shiftRequest.push({
        Id: match.Id,
        CodigoGrano: match.CodigoGrano || (cupo.CodGrano ? parseInt(cupo.CodGrano, 10) || 0 : 0),
        Cantidad: cantidad,
        CuentaVendedor: match.Vendedor ? parseInt(match.Vendedor, 10) || 0 : 0,
        CuentaComprador: match.Comprador ? parseInt(match.Comprador, 10) || null : null,
        CodigoEstado: 0,
        // Normalizamos a ISO 8601 para que System.Text.Json del backend acepte
        // los campos sin chocar con el formato WCF /Date(...)/ que devuelve
        // SILData cuando no se configura un JsonConverter ISO.
        FechaCreacion: serializarFechaISO(match.FechaSolicitado) || new Date().toISOString(),
        FechaSolicitado: serializarFechaISO(match.FechaSolicitado) || new Date().toISOString(),
        CodigoCentro: ''
      });

      cuposToBeDistributed.push({
        Id: cupo.Id,
        CodGrano: cupo.CodGrano || '',
        NomGrano: cupo.NomGrano || '',
        CodVendSIL: cupo.CodVendSIL || '',
        NomVendSIL: cupo.NomVendSIL || '',
        CodCompSIL: cupo.CodCompSIL || '',
        NomCompSIL: cupo.NomCompSIL || '',
        CodDestino: cupo.CodDestino || '',
        NomDestino: cupo.NomDestino || '',
        Fecha: serializarFechaISO(cupo.Fecha),
        CentroCupo: ''
      });
    });

    if (shiftRequest.length === 0) return;

    showBlockingOverlay('Distribuyendo ' + totalCupos + ' cupo(s)...');

    return $.ajax({
      url: '/CuposMatching/AceptarMatch',
      method: 'POST',
      contentType: 'application/json',
      dataType: 'json',
      data: JSON.stringify({
        ShiftRequest: shiftRequest,
        CuposToBeDistributed: cuposToBeDistributed
      })
    }).done(function (resp) {
      hideBlockingOverlay();
      if (resp && resp.success) {
        if (window.SILMatching && typeof window.SILMatching.clearAsignaciones === 'function') {
          window.SILMatching.clearAsignaciones();
        }
        hideAllOverlays();

        // Refrescar tabla de distribución para reflejar el estado real del backend.
        if (typeof actualizarTablaContratos === 'function') {
          actualizarTablaContratos({ mostrarEstado: true });
        }

        if (typeof Swal !== 'undefined') {
          Swal.fire({
            icon: 'success',
            title: 'Distribución realizada',
            text: resp.message || 'La distribución se aplicó correctamente.',
            timer: 3500,
            showConfirmButton: false
          });
        }
      } else if (resp && resp.status === 409) {
        if (typeof Swal !== 'undefined') {
          Swal.fire({
            icon: 'warning',
            title: 'Conflicto de concurrencia',
            text: resp.message || 'La solicitud ya fue procesada por otro operador.',
            showConfirmButton: true
          });
        }
      } else {
        if (typeof Swal !== 'undefined') {
          Swal.fire({
            icon: 'error',
            title: 'No se pudo distribuir',
            text: (resp && resp.message) ? resp.message : 'El backend rechazó la distribución.',
            showConfirmButton: true
          });
        }
      }
    }).fail(function (xhr) {
      hideBlockingOverlay();
      console.warn('[Matching] AceptarMatch error:', xhr && xhr.statusText);
      if (typeof Swal !== 'undefined') {
        Swal.fire({
          icon: 'error',
          title: 'Error al distribuir',
          text: (xhr && xhr.responseJSON && xhr.responseJSON.message)
              ? xhr.responseJSON.message
              : 'No se pudo comunicar con el servidor.',
          showConfirmButton: true
        });
      }
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
  // Diálogos extras
  // ============================================================
  function mostrarConflicto(mensaje, fallos) {
    var html = '<strong>' + escapeHtml(mensaje) + '</strong>';
    if (fallos && fallos.length > 0) {
      html += '<ul style="margin-top:6px; padding-left: 16px;">';
      fallos.forEach(function (f) {
        html += '<li>Solicitud #' + f.SolicitudId + ': ' + escapeHtml(f.Motivo) + '</li>';
      });
      html += '</ul>';
    }
    $('#conflict-detail').html(html);
    showOverlay('sil-modal-conflict');
  }

  function mostrarRechazoAutomatico(resumen, lista) {
    $('#rechazo-auto-resumen').text((resumen || 2) + ' solicitudes');
    if (lista && lista.length > 0) {
      var html = lista.map(function (s) {
        return '&mdash; ' + escapeHtml(s);
      }).join('<br />');
      $('#rechazo-auto-lista').html(html);
    }
    showOverlay('sil-modal-rechazo-auto');
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
        $(this).removeClass('is-open');
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
        if (typeof Swal !== 'undefined') Swal.fire({ icon: 'info', title: 'Nada seleccionado', text: 'Ajustá las cantidades en el paso 1 antes de confirmar.' });
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
        if (typeof Swal !== 'undefined') Swal.fire({ icon: 'info', title: 'Nada seleccionado', text: 'Ajustá las cantidades en el paso 3 antes de confirmar.' });
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

    $(document).on('click', '[data-action="vb-confirmar"]', function () {
      var solicitudes = [];
      Object.keys(estado.seleccionados).forEach(function (k) {
        var s = estado.seleccionados[k];
        if (!s.checked) return;
        if (s.sum <= 0) return;
        // s.g.sols es la lista; tomamos el primer día (simplificación iteración 1).
        var firstSol = s.g.sols[0];
        if (!firstSol) return;
        solicitudes.push({
          cupoId: s.cupoId || 0,
          solicitudId: firstSol.Id,
          matchType: firstSol.MatchType,
          cantidad: s.sum
        });
      });
      if (solicitudes.length === 0) {
        if (typeof Swal !== 'undefined') Swal.fire({ icon: 'info', title: 'Nada seleccionado', text: 'Tildá al menos un solicitante o ajustá las cantidades.' });
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
