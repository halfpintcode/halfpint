/// <reference path="jquery-1.7.1-vsdoc.js" />


$(function () {

    $('#Sites').change(function () {
        var selectedVal = $(this).val();
        var url = urlRoot + '/Admin/SiteSelected/' + selectedVal;
        $.post(url + '',
            { site: selectedVal },
            function (data) {
                $('#Users').empty();
                $.each(data, function (index, d) {
                    $('#Users').append("<option value='"+ d.UserName + "'>" + d.UserName + "</option>")
                });

            })


    });


    $('#Users').change(function () {
        var selectedVal = $(this).val();
        var url = urlRoot + '/Admin/UserSelected/' + selectedVal;
        $('.tblRoles tBody').empty();
        $.post(url + '',
            {},
            function (data) {
                $.each(data, function (index, d) {
                    var tr = '<tr> <td>' + d + '</td> </tr>'
                    $('.tblRoles tBody').append(tr)
                });

            })


    });

    $('#deleteUser').click(function (e) {
        var userName = $('#Users').val();
        if (userName === 'Select user') {
            alert('Select a user')
            e.preventDefault();
            return false;
        }
        if (!confirm('Are you sure you want to delete user ' + userName)) {
            alert('cancel');
            e.preventDefault();
            return false;
        }
        e.preventDefault();
        var url = urlRoot + '/Admin/RemoveUser/' + userName;
        $.post(url + '',
            {},
            function (data) {
                alert(data.Message);
                if (data.ReturnValue == 1) {
                    top.location.href = urlRoot + '/Admin/Index';
                }
            });



    });

    $('.aLnkClick').click(function (e) {
        var userName = $('#Users').val();
        if (userName === 'Select user') {
            alert('Select a user')
            e.preventDefault();
            return false;
        }

        var path = '';
        if ($(this).text() === "Reset user password") {
            window.location = urlRoot + '/Admin/ResetUserPassword/' + userName;
        }
        else {
            window.location = urlRoot + '/Admin/ManageUserRoles/' + userName;
        }
        //$(this).attr('href', path);


    });

});