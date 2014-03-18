/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function () {
    $('#btnSave').click(function () {
        var vtoken = $("input[name=__RequestVerificationToken]").val();
        var url = window.urlRoot + '/Meetings/Save';
        var data = $("form").serialize();
        $.ajax({
            url: url,
            type: 'POST',
            data: data,
            success: function (rdata) {
                if (rdata.ReturnValue === 1) {
                    alert('Meetings page saved successfully!');
                }
                else {
                    alert(rdata.Message);
                }
            }

        });

    });
});