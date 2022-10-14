function HighLight(){
    this.$container = $('.highlight-container');
    this.$highlights = $('.highlights');
    this.$textarea = $('textarea');
    this.$toggle = $('button');
    this.textHighlight = "";

    // yeah, browser sniffing sucks, but there are browser-specific quirks to handle that are not a matter of feature detection
    this.ua = window.navigator.userAgent.toLowerCase();
    this.isIE = !!this.ua.match(/msie|trident\/7|edge/);
    this.isWinPhone = this.ua.indexOf('windows phone') !== -1;
    this.isIOS = !this.isWinPhone && !!this.ua.match(/ipad|iphone|ipod/);

    this.onInput;
    this.bindEvents();

    if (this.isIOS) {
        this.fixIOS();
    }
}



HighLight.prototype.applyHighlights = function(text, textHighlight) {
    text = text
      .replace(textHighlight, '<mark>$&</mark>');
      //.replace(/\n$/g, '\n\n')

    if (this.isIE) {
        // IE wraps whitespace differently in a div vs textarea, this fixes it
        text = text.replace(/ /g, ' <wbr>');
    }

    return text;
}

HighLight.prototype.highLightText = function(text){
    this.textHighlight = text;
}

HighLight.prototype.handleInput = function(e) {
    var textArea = e.currentTarget;
    var text = textArea.value;
    if (this.onInput != undefined) this.textHighlight = this.onInput(textArea);
    var $highlight = $(textArea).prev(".backdrop").find(".highlights");
    if (this.textHighlight !== false) {
        var highlightedText = this.applyHighlights(text, this.textHighlight);
        $highlight.html(highlightedText);
    } else {
        $highlight.html("");
    }
}

HighLight.prototype.handleScroll = function (e) {
    var textArea = e.currentTarget;
    var scrollTop = $(textArea).scrollTop();
    var $backdrop = $(textArea).prev(".backdrop");
    $backdrop.scrollTop(scrollTop);

    var scrollLeft = $(textArea).scrollLeft();
    $backdrop.scrollLeft(scrollLeft);
}

HighLight.prototype.fixIOS = function() {
    // iOS adds 3px of (unremovable) padding to the left and right of a textarea, so adjust highlights div to match
    this.$highlights.css({
        'padding-left': '+=3px',
        'padding-right': '+=3px'
    });
}

HighLight.prototype.bindEvents = function () {
    var _this = this;
    this.$textarea.on({
        'input': function (e) { _this.handleInput(e) },
        'scroll': function (e) { _this.handleScroll(e) }
    });
}