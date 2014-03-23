/// <reference path="~/Scripts/jquery-1.7.1-vsdoc.js" />
/// <reference path="~/Scripts/jquery-ui-1.10.3.js" />
/// <reference path="~/Scripts/Layout.js" />
$(document).ready(function () {
    var startDate;
    var endDate;

    $('#StudyList').change(function () {
        $('#studyId').val($(this).val());
        $('#subjectId').val($("#StudyList option:selected").text());
    });

    $('#StartDate').datepicker({
        beforeShow: function (input) {
            $.datepicker._pos = $.datepicker._findPos(input); //this is the default position 
            var pos = $(this).position();
            $.datepicker._pos[0] = pos.left + 100; //left              
        }
    });
    $('#StartDate').datepicker('setDate', 'today');
    $('#EndDate').datepicker({
        beforeShow: function (input) {
            $.datepicker._pos = $.datepicker._findPos(input); //this is the default position 
            var pos = $(this).position();
            $.datepicker._pos[0] = pos.left + 100; //left,

        }
    });
    $('#EndDate').datepicker('setDate', 'today');

    $('#StartDate').change(function () {
        if (!validateDates()) {
            alert("End date must be later than start date!");
        };
    });
    $('#EndDate').change(function () {
        if (!validateDates()) {
            alert("End date must be later than start date!");
        };
    });

    function validateDates() {

        startDate = $('#StartDate').datepicker('getDate');
        endDate = $('#EndDate').datepicker('getDate');

        if (endDate < startDate) {
            return false;
        }
        return true;
    }

    function validate() {
        var studyId = $('#studyId').val();
        if (!studyId) {
            alert('Select a subject');
            return false;
        }
        return true;
    }



    $('#btnRun').click(function () {
        if (!validate()) {
            return;
        }

        //$('#btnRun').attr('disabled', 'disabled');
        var studyId = $('#studyId').val();

        var subjectId = $("#StudyList option:selected").text();
        startDate = $('#StartDate').val();
        endDate = $('#EndDate').val();

        var url = urlRoot + '/Coordinator/ChecksNovaBloodGlucoseReport?subjectId=' + subjectId + '&studyId=' + studyId
            + '&startDate=' + startDate + '&endDate=' + endDate;
        window.location.href = url;
        //        var data = $("form").serialize();
        //        $.ajax({
        //            url: url,
        //            type: 'POST',
        //            data: data,
        //            success: function (data1) {
        //                alert(data1);
        //                $('#divReport').empty();
        //                $('#divReport').html(data1);
        //            }
        //        });
    });
});