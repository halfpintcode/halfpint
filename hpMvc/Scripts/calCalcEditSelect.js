/// <reference path="jquery-1.7-vsdoc.js" />
$(function () {

    $('#CalStudyList').change(function () {
        $('#calcDates').empty();

        var studyId = $('#CalStudyList').val();
        if (studyId === "0") {
            return;
        }

        $.ajax({
            url: window.urlRoot + '/CalorieCalc/GetCalcDates',
            type: 'POST',
            data: { studyID: studyId },
            success: function (data) {
                var calcDates = $('#calcDates');
                calcDates.empty();
                var option;
                $.each(data, function (idx, val) {
                    option = "<option value='" + val.ID + "'>" + val.Name + "</option>";
                    calcDates.append(option);
                });
            }
        });


    });

    $('#btnSubmit').click(function () {
        var studyId = $('#CalStudyList').val();
        if (studyId === "0") {
            alert('Select a Study ID!');
            return;
        }

        var calId = $('#calcDates').val();
        if (calId === "0") {
            alert('Select a date');
            return;
        }

        $('#btnSubmit').attr('disabled', 'disabled');
        $.ajax({
            url: window.urlRoot + '/CalorieCalc/GetStudyInfo',
            type: 'POST',
            data: { calStudyID: calId },
            success: function (rdata) {
                window.location = window.urlRoot + '/CalorieCalc/Edit/?Id='
                    + rdata.Id + '&StudyId=' + rdata.StudyId + '&Weight='
                    + rdata.Weight + '&CalcDate=' + rdata.CalcDate + '&Hours='
                    + rdata.Hours;

            }
        });
    });
})