/// <reference path="jquery-1.7-vsdoc.js" />
$(function () {
    var selectedVal = $('#StudyIDList').val()
    $('#password').text(selectedVal);

    $('#StudyIDList').change(function () {
        selectedVal = $(this).val();
        $('#password').text(selectedVal);
    });
});