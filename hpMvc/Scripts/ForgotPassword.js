/// <reference path="jquery-1.7-vsdoc.js" />
$(function () {
    $('#waitGif').hide();
    $.ajaxSetup({
        beforeSend: function () {
            $('#waitGif').show();
        },
        complete: function () {
            $('#waitGif').hide();
        }
    });

    $('#btnSubmit').click(function () {
        var email = $.trim($('#email').val());
        var data = { email: email };
        var url = window.urlRoot + '/Account/ForgotPassword/';

        $.ajax({
            url: url,
            type: 'POST',
            data: data,
            success: function (data1) {
                alert(data1.Message);
                if (data1.IsSuccessful) {
                    window.location = window.urlRoot + '/Home/Index';
                }
            }
        });
    });
});