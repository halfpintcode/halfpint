/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function () {
    //alert('');
    var site = $('#SiteID').val();
    getActiveSubjects(site);

    $('#Sites').change(function () {
        getActiveStudies();
    });

    function getActiveSubjects(site) {
        var siteID = "";
        if (site) {
            siteID = site;
        }
        else {
            siteID = $('#Sites').val();
        }

        $.ajax({
            url: urlRoot + '/Coordinator/GetActiveSubjects',
            type: 'POST',
            data: { siteID: siteID },
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
        window.location = urlRoot + '/Coordinator/CompleteSubject/?id=' + data.ID;
        //        $.ajax({
        //            url: urlRoot + '/Coordinator/CompleteSubject',
        //            type: 'POST',
        //            data: data,
        //            success: function (data) {
        //               alert('')
        //            }
        //        });
    });

});