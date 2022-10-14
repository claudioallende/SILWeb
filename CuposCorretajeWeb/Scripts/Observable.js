function Observable() {
    this.observers = [];
}

Observable.prototype.subscribe = function (f) {
    this.observers.push(f);
}

Observable.prototype.unsubscribe = function (f) {
    this.observers = this.observers.filter(function (subscriber) { return subscriber !== f });
}

Observable.prototype.notify = function (data) {
    this.observers.forEach(function(observer) { observer(data) });
}

function ObservableArray(array) {
    this.observers = new Observable();
    this.array = array;
}

ObservableArray.prototype.subscribe = function (f) {
    this.observers.subscribe(f);
}

ObservableArray.prototype.unsubscribe = function (f) {
    this.observers.unsubscribe(f);
}

ObservableArray.prototype.notify = function (data) {
    this.observers.notify(data);
}

ObservableArray.prototype.push = function (obj) {
    this.array.push(obj);
    this.observers.notify(obj);
}

ObservableArray.prototype.remove = function (obj) {
    this.array.splice(this.array.indexOf(obj), 1);
    this.observers.notify(obj);
}