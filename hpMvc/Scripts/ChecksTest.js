/// <reference path="jquery-1.7-vsdoc.js" />
$(function () {
    $('#btnSubmit').click(function () {
        var url = urlRoot + '/PostTests/Checks'
        var data = $("form").serialize();
        $.ajax({
            url: url,
            type: 'POST',
            data: data,
            success: function (data) {
                if (data.IsSuccessful) {
                    alert('Congratulations, ' + data.Message);
                }
                else {
                    var message = 'You bad, ' + data.Message + '\n';
                    $.each(data.Messages, function (index, d) {
                        message = message + '\n' + d;
                    });
                    alert(message);
                }
            }
        });
    });
});