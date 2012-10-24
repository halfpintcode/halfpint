/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function () {
    var role = $('#Role').val();
    if ($('#Age2to16').is(':checked')) {
        $('#overTwo').show();
    }
    else {
        $('#overTwo').hide();
    }

    if ($('#CgmUpload').val() === 'True') {
        if (role === 'Admin') {
            $('#spanUploadAdmin').show();
            $('#spanUpload').hide();
        }
        else {
            $('#spanUpload').show();
            $('#upload').hide();
            $('#spanUploadAdmin').hide();
        }
    }
    else {
        $('#spanUploadAdmin').hide();
        $('#spanUpload').hide();
        $('#upload').show();
    }

    $('#Age2to16').click(function () {
        if ($(this).is(':checked')) {
            $('#AgeNot2to16').attr('checked', false);
        }
        $('#overTwo').slideToggle('slow', function () {
            if (!$(this).is(":visible")) {
                $('#CBCL').attr('checked', false);
                $('#PedsQL').attr('checked', false);
                $('#Demographics').attr('checked', false);
                $('#ContactInfo').attr('checked', false);
            }
        });
    });

    $('#AgeNot2to16').click(function () {
        if ($(this).is(':checked')) {
            if ($('#Age2to16').is(':checked')) {
                $('#CBCL').attr('checked', false);
                $('#PedsQL').attr('checked', false);
                $('#Demographics').attr('checked', false);
                $('#ContactInfo').attr('checked', false);
                
                $('#Age2to16').attr('checked', false);
                $('#overTwo').slideUp('slow');
            }
        }
    });
    $('#btnSubmit').click(function () {
        if (!validateSave()) {
            return false;
        }
    })
});

function validateSave(){
    var message = '';
    var val = $.trim($('#NotCompletedReason').val());
    if(val){
        return true;
    }

    if (!$('#DateCompleted').val()) {
        message = 'Date completed is required.  Enter a reason if you can not provide this data.'
        alert(message);
        $('#DateCompleted').focus();
        return false;
    }

    if ($('#CgmUpload').val() === 'False') {
        if (!$('#upload').val()) {
            message = 'The CGM file upload is required.  Enter a reason if you can not provide this data.'
            alert(message);
            $('#upload').focus();
            return false;
        }
        else {
            $('#CgmUpload').val('True');
        }
    }

    if (($('#Age2to16').is(':checked'))) {
        if (!($('#CBCL').is(':checked'))) {
            message = 'You must certify with a check mark that CBCL has been collected and sent to the CCC.  Enter a reason if you can not provide this data.'
            alert(message);
            $('#CBCL').focus();
            return false;
        }
        if (!($('#PedsQL').is(':checked'))) {
            message = 'You must certify with a check mark that Peds-QL has been collected and sent to the CCC.  Enter a reason if you can not provide this data.'
            alert(message);
            $('#PedsQL').focus();
            return false;
        }
        if (!($('#Demographics').is(':checked'))) {
            message = 'You must certify with a check mark that subject demographics has been collected and sent to the CCC.  Enter a reason if you can not provide this data.'
            alert(message);
            $('#Demographics').focus();
            return false;
        }
        if (!($('#ContactInfo').is(':checked'))) {
            message = 'You must certify with a check mark that subject contact information has been collected.  Enter a reason if you can not provide this data.'
            alert(message);
            $('#ContactInfo').focus();
            return false;
        }
    }
    return true;
}