/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function() {
    $('#btnSend').click(function() {
        var url = window.urlRoot + '/Admin/TestEmail/';
        var email = $('#Email').val();
        $.post(url,
            { Email:email},
            function (data) {
                alert(data);
            });
    });
});