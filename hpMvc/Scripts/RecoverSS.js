/// <reference path="jquery-1.7-vsdoc.js" />
$(function () {
    $('#btnDownload').attr('disabled', 'disabled');
    
    $('#SS').change(function () {
        var selectedVal = $(this).val();
        if(selectedVal === "<Select spreadsheet>"){
            $('#btnDownload').attr('disabled', 'disabled');    
        }
        else{
            $('#btnDownload').attr('disabled', false);
        }
    });

    $('#btnDownload').click(function(){
        var studyId = $('#SS').val();
        var url = window.urlRoot + '/InitializeSubject/AlternateSSDownload/' + studyId;
        window.location.href = url;
    });
});