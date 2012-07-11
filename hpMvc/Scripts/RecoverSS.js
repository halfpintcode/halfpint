/// <reference path="jquery-1.7-vsdoc.js" />
$(function () {
    $('#btnDownload').attr('disabled', 'disabled');
    
    $('#SS').change(function () {
        selectedVal = $(this).val();
        if(selectedVal === "<Select spreadsheet>"){
            $('#btnDownload').attr('disabled', 'disabled');    
        }
        else{
            $('#btnDownload').attr('disabled', false);
        }        
    });

    $('#btnDownload').click(function(){
        var studyID = $('#SS').val();
        var url = urlRoot + '/InitializeSubject/AlternateSSDownload/' + studyID;
        window.location.href = url;
    });
});