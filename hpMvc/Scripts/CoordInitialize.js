/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function () {
    //alert('');
    getActiveStudies();

    $('#Sites').change(function () {
        getActiveStudies();
    });

    function getActiveStudies() {
        $.ajax({
            url: urlRoot + '/Coordinator/GetActiveStudies',
            type: 'POST',
            data: { siteID: $('#Sites').val() },
            success: function (data) {
                var tbl = $('#tblActive tbody');
                tbl.empty();
                $.each(data, function (index, d) {
                    var tr = $('<tr><td>' + d.StudyID + '</td><td>' + d.sDateRandomized + '</td><td><input class="btnComplete" type="button" value="Set Completed" /></td></tr>')
                    tbl.append(tr);
                    $(tr).data('rowData', d);

                });
            }
        });
    }

    $('.btnComplete').live("click", function () {
        var data = $(this).parent().parent().data('rowData');
        window.location = urlRoot + '/Coordinator/CompleteStudy/?id=' + data.ID;
        //        $.ajax({
        //            url: urlRoot + '/Coordinator/CompleteStudy',
        //            type: 'POST',
        //            data: data,
        //            success: function (data) {
        //               alert('')
        //            }
        //        });
    });

});