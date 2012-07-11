/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function () {
    $('#btnSave').click(function () {

        var url = urlRoot + '/Inform/Save';
        var data = $("form").serialize();
        $.ajax({
            url: url,
            type: 'POST',
            data: data,
            success: function (data) {
                if (data.ReturnValue === 1) {
                    alert('InForm page saved successfully!');
                }
                else {
                    alert(data.Message);
                }
            }

        });

    });
});