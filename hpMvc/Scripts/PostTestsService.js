/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function () {
    $(function () {
        $("#accordion").accordion();
    });
    $('#btnSubmit').click(function() {
       
        $('#btnSubmit').attr('disabled', 'disabled');

        $("#spinner").ajaxStart(function () { $(this).show(); })
			   .ajaxStop(function () { $(this).hide(); });

        var url = window.urlRoot + '/Admin/PostTestsService';
        var data = $("form").serialize();

        $.ajax({
            url: url,
            type: 'POST',
            data: data,
            success: function (data1) {
                if (data1 == 'Successful') {
                    alert('The serviece was run sucessfully!');
                    url = window.urlRoot + '/Admin/Index';
                    window.location = url;
                }
                else {
                    $('#btnSubmit').attr('disabled', false);
                    alert('Could not run the service at this time, please try later.');
                }
            }
        });
    });
});