/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function () {
    var site = $('#SiteID').val();
    getActiveSubjects(site);

    $('#showGraphDialog').dialog({ autoOpen: false });
    
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

    function getActiveSubjects(siteSelected) {
        var siteID = "";
        if (siteSelected) {
            siteID = siteSelected;
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

    $('.btnGraph').live("click", function () {
        var rowData = $(this).parent().parent().data('rowData');
        $.ajax({
            url: urlRoot + '/Coordinator/GetGraphUrl/' + rowData.StudyID,
            type: 'POST',
            success: function (data) {
                if (data === 'Chart not available') {
                    alert(data);
                } else {
                    $('#showGraphDialog').empty();
                    
                    var imgUrl = 'file:\\\\\\' + data.path1;
                    var $img = "<img  src='" + imgUrl + "' />";
                    $('#showGraphDialog').append($img);
                    var imgUrl2 = 'file:\\\\\\' + data.path2;
                    var $img2 = "<img  src='" + imgUrl2 + "' />";
                    $('#showGraphDialog').append($img2);
                    

                    $('#showGraphDialog').dialog(
                    {
                        title: 'Chart for subject id:' + data.studyID,
                        height: 380,
                        width: 1140,
                        show: 'blind',
                        hide: 'explode'
                    });
                    $('#showGraphDialog').dialog('open');
                }
            }
        });
    });
})