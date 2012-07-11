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
        var url = urlRoot + '/Account/ForgotPassword/';

        $.ajax({
            url: url,
            type: 'POST',
            data: data,
            success: function (data) {
                alert(data.Message);
                if (data.IsSuccessful) {
                    window.location = urlRoot + '/Home/Index';
                }
            }
        });
    });
});