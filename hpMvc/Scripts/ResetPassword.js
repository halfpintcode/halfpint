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

    function containsNumeric(text) {
        var temp = text;
        for (var i = 0; i < temp.length; i++) {
            var c = temp.charAt(i);
            if ($.isNumeric(c)) {
                return true;
            }
        }
        return false;
    };

    function containsCapital(text) {
        var temp = text;
        for (var i = 0; i < temp.length; i++) {
            var c = temp.charAt(i);
            if ($.isNumeric(c)) {
                continue;
            }
            if (c === c.toUpperCase()) {
                return true;
            }
        }
        return false;
    };

    function validate() {
        var newPassword = $.trim($('#NewPassword').val());
        if ($.trim(newPassword).length === 0) {
            alert('New password is required');
            return false;
        }
        var confirmPassword = $.trim($('#ConfirmPassword').val());
        if ($.trim(confirmPassword).length === 0) {
            alert('New confirm password is required');
            return false;
        }

        if (newPassword !== confirmPassword) {
            alert("The new password and confirm password do not match!");
            return false;
        }

        if (!containsNumeric(newPassword)) {
            alert("The new password must contain at least one number!");
            return false;
        }

        if (!containsCapital(newPassword)) {
            alert("The new password must contain at least one capital letter!");
            return false;
        }
        return true;
    }

    $('#btnSave').click(function () {
        if (! validate()) {
            return;
        }
        var user = $.trim($('#UserName').val());
        var pwd = $.trim($('#NewPassword').val());
        var cPwd = $.trim($('#ConfirmPassword').val());
        var url = window.urlRoot + '/Account/ResetPassword/';
        $.post(url,
            { userName: user, NewPassword: pwd, ConfirmPassword: cPwd },
            function (data) {
                if (data) {
                    alert('Password has been reset!');
                    window.location = window.urlRoot + '/Home/Index';
                } else {
                    alert("Could not reset password!");
                };
            });
    });

});