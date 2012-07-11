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
    
    //get the current user email
    var url = urlRoot + '/Account/GetUserEmail';

    $.ajax({
        url: url,
        type: 'post',
        success: function (data) {
            if (data.IsSuccessful) {
                $('#emailAddress').val(data.Message);
            }

        }
    });

    $('#btnSaveEmail').click(function () {
        var email = $('#emailAddress').val();
        if ($.trim(email).length === 0) {
            alert('The email address is required!');
            return;
        }
        if (!validateEmail(email)) {
            alert('Not a valid email format!');
            return;
        }

        var url = urlRoot + '/Account/UpdateUserEmail';
        $.ajax({
            url: url,
            data: { NewEmail: email },
            type: 'post',
            success: function (data) {
                if (data.IsSuccessful) {
                    alert('Your email address has been updated.');
                }

            }
        });
    });
});