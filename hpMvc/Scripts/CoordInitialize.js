/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function () {
    //alert('');
    $('#Sites').change(function () {

        $.ajax({
            url: urlRoot + '/Coordinator/GetActiveStudies',
            type: 'POST',
            data: { siteID: $(this).val() },
            success: function (data) {
                var tbl = $('#tblActive tbody');
                tbl.empty();
                $.each(data, function (index, d) {
                    var tr = '<tr><td>' + d.StudyID + '</td><td>' + d.sDateRandomized + '</td><td><input class="btnComplete" type="button" value="Set Completed" /></td></tr>'
                    tbl.append(tr);
                });
            }
        });

    });

    $('.btnComplete').click(function () {

    });
});