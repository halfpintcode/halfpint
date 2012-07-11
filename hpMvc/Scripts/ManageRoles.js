$(function () {
    $('#divNewRole').hide();
    
    $('#waitGif').hide();
    $.ajaxSetup({
        beforeSend: function () {
            $('#waitGif').show();
        },
        complete: function () {
            $('#waitGif').hide();
        }
    });

    $('#lnkNewRole').click(function () {
        $('#divMain').hide(1000);
        $('#divNewRole').show(1000);
        return false;
    });

    $('#btnCancel').click(function () {
        $('#divNewRole').hide(1000);
        $('#divMain').show(1000);

    });

    $('#btnSave').click(function () {
        var newRole = $('#txtRole').val();
        var url = urlRoot + '/Admin/ManageRoles';
        $.ajax({
            url: url,
            type: 'POST',
            data: { role: newRole },
            success: function () {
                alert(newRole + ' has been added');
                top.location.href = urlRoot + '/Admin/Index';
            }

        });
    });
       
});