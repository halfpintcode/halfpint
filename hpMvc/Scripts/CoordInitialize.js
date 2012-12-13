﻿/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function () {
    //alert('');
    var site = $('#SiteID').val();
    getActiveSubjects(site);

    $('#Sites').change(function () {
        getActiveSubjects();
    });

    $('#showCleared').change(function () {
        //        if (($(this).is(':checked'))) {
        //            $('#spanCleared').text('show non-cleared subjects');
        //        }
        //        else {
        //            $('#spanCleared').text('show cleared subjects');
        //        }
        getActiveSubjects();
    });

    function getActiveSubjects(site) {
        var siteID = "";
        if (site) {
            siteID = site;
        }
        else {
            siteID = $('#Sites').val();
        }

        var showCleared = false;
        if (($('#showCleared').is(':checked'))) {
            showCleared = true;
        }

        $.ajax({
            url: urlRoot + '/Coordinator/GetActiveSubjects',
            type: 'POST',
            data: { siteID: siteID, showCleared: showCleared },
            success: function (data) {
                var buttonText = "Set Completed";
                if (showCleared) {
                    buttonText = "Edit Completed";
                }
                var tbl = $('#tblActive tbody');
                tbl.empty();
                $.each(data, function (index, d) {
                    var tr = $('<tr><td>' + d.StudyID + '</td><td>' + d.sDateRandomized + '</td><td><input class="btnComplete" type="button" value="' + buttonText + '" /></td><td><input class="btnGraph" type="button" value="View Graph" /></td> </tr>');
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

    $('.btnGraph').live("click", function() {
        var rowData = $(this).parent().parent().data('rowData');
        $.ajax({
            url: urlRoot + '/Coordinator/GetGraphUrl/' + rowData.StudyID,
            type: 'POST',
            success: function(data) {
                alert(data);
            }
        });
    });
})