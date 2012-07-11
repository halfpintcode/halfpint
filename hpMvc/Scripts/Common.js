var hp = hp || {};

hp.Utils = function () {

};

hp.Utils.prototype = function() {

    var GetUrlRoot = function () {
        var firstSlash = 0;
        var secondSlash = 0;
        firstSlash = location.href.indexOf("/", 9);
        secondSlash = location.href.indexOf("/", firstSlash + 1);
        return location.href.substr(0, secondSlash);
    };
    return {
        urlRool: GetUrlRoot()
    };

}();

