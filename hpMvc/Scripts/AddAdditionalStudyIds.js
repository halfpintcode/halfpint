// <reference path="jquery-1.7.1-vsdoc.js" />
$(function() {
    $('#spanWait').hide();

    $('#btnSubmit').click(function () {
        $('#btnSubmit').attr('disabled', 'disabled');
        $('#spanWait').show();
        document.forms[0].submit();

    });
})