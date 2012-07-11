/// <reference path="jquery-1.7-vsdoc.js" />
$(function () {

    $('#CalStudyList').change(function () {
        $('#calcDates').empty();

        var studyID = $('#CalStudyList').val();
        if (studyID === "0") {            
            return;
        }

        $.ajax({
            url: urlRoot + '/CalorieCalc/GetCalcDates',
            type: 'POST',
            data: { studyID: studyID },
            success: function (data) {
                var calcDates = $('#calcDates');
                calcDates.empty();
                var option;
                $.each(data, function (idx, val) {
                    option = "<option value='" + val.ID + "'>" + val.Name + "</option>"
                    calcDates.append(option);
                });
            }
        });


    });

    $('#btnSubmit').click(function () {
        var studyID = $('#CalStudyList').val();
        if (studyID === "0") {
            alert('Select a Study ID!');
            return;
        }

        var calID = $('#calcDates').val();
        if (calID === "0") {
            alert('Select a date');
            return;
        }

        $('#btnSubmit').attr('disabled', 'disabled');
        $.ajax({
            url: urlRoot + '/CalorieCalc/GetStudyInfo',
            type: 'POST',
            data: { calStudyID: calID },
            success: function (rdata) {
                window.location = urlRoot + '/CalorieCalc/Edit/?ID='
                    + rdata.ID + '&StudyID=' + rdata.StudyID + '&Weight='
                    + rdata.Weight + '&CalcDate=' + rdata.CalcDate;

            }
        });
    });
})