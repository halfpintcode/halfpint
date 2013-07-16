/// <reference path="jquery-1.7.1-vsdoc.js" />
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

    $('#btnSave').click(function () {
        if (!validate()) {
            return;
        }
        var user = $.trim($('#userName').val());
        var pwd = $.trim($('#NewPassword').val());
        var url = window.urlRoot + '/Admin/ResetUserPassword/';
        var reset = $('#forceReset').is(':checked');

        $('#btnSave').attr('disabled','disabled');
        $.post(url,
            { userName: user, NewPassword: pwd, Reset: reset },
            function (data) {
                if (data) {
                    alert('Password has been reset!');
                }
            });

    });

    function validate() {
        var newPassword = $.trim($('#NewPassword').val());
        if ($.trim(newPassword).length === 0) {
            alert('New password is required')
            return false;
        }
        var confirmPassword = $.trim($('#ConfirmPassword').val());
        if ($.trim(confirmPassword).length === 0) {
            alert('New confirm password is required')
            return false;
        }

        if (newPassword !== confirmPassword) {
            alert("The new password and confirm password do not match!");
            return false;
        }

        return true;
    }
});