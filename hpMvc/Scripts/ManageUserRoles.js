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

    $('#btnSave').click(function () {
        var url = urlRoot + '/Admin/ManageUserRoles/' + $('#userName').val();
        $.ajax({
            url: url,
            type: 'POST',
            data: $('#form1').serialize(),
            success: function () {
                alert('Roles have been assigned successfully');
                top.location.href = urlRoot + '/Admin/Index';
            }
        });

    });

});