/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function () {
    $('#btnGetExceptions').click(function () {
        var url = window.urlRoot + '/Admin/CgmImportExceptions/';
        var email = $('#Email').val();
        $.post(url,
            { },
            function (data) {
                var $div = $('#divExceptions').empty();
                $.each(data, function(idx, val) {
                    var head = "<h4>" + val.Site + "</h4>";
                    $div.append(head);
                    $div.append("<ul>");
                    if (val.Notifications.length > 0) {
                        $.each(val.Notifications, function(idx2, notice) {
                            $div.append("<li>" + notice.Message + "</li>");
                        });
                    } else {
                        $div.append("<li>" + 'No exceptions for this site!' + "</li>");
                    }
                    $div.append("</ul>");
                });
            });
    });
});