/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function () {
    $('#testForm').hide();

    var completed = $('#completed').val();
    if (completed === 'true') {
        $('#showHideTest').hide();
        $('#testIntro').hide();
    }

    $('#showHideTest').click(function (e) {
        e.preventDefault();
        var text = $(this).text();
        if (text === "Take the test") {
            $(this).text('Hide the test');
            $('#testForm').slideDown('slow');
        }
        else {
            $(this).text('Take the test');
            $('#testForm').slideUp('slow');
        }
    });

    $('#btnSubmit').click(function () {
        $('#btnSubmit').attr('disabled', 'disabled');

        var test = $('#test').val();
        var url = urlRoot + '/PostTests/' + test;
        var data = $("form").serialize();
        
        $.ajax({
            url: url,
            type: 'POST',
            data: data,
            success: function (data1) {
                if (data1.IsSuccessful) {
                    alert('Congratulations! ' + data1.Message);
                    var id = $('#id').val();
                    url = urlRoot + '/PostTests/Initialize/' + id;
                    window.location = url;
                }
                else {
                    $('#btnSubmit').attr('disabled', false);

                    var message = 'Please review the video and take the test again. ' + data1.Message + '\n';
                    $.each(data1.Messages, function (index, d) {
                        message = message + '\n' + d;
                    });
                    alert(message);
                }
            }
        });
    });
});