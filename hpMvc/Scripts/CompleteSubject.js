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
    $('#btnSubmit').click(function() {
        if (!validateSave()) {
            return false;
        }
        return true;
    });
});

function validateSave(){
    var message;
    var isReason = false;
    var val = $.trim($('#NotCompletedReason').val());
    if (val) {
        isReason = true;
    }

    //you have to check the upload file for size and extension even if there is a reason
    if ($('#CgmUpload').val() === 'False') {
        var $uploadFile = $('#upload')[0];
        var fileName;
        if (! $uploadFile.files.length) {
            //if no file and there is a reason then valid is true
            if (isReason) {
                return true;
            }

            message = 'The CGM file upload is required.  Enter a reason if you can not provide this data.';
            alert(message);
            $('#upload').focus();
            return false;
        }
        else {
            fileName = $uploadFile.files[0].name;
            if ($uploadFile.files[0].size > 500000) {
                message = 'The CGM file upload file is too large.  Make sure you have selected the correct file to upload.';
                alert(message);
                return false;
            }
            var extension = fileName.split('.').pop().toUpperCase();
            if (extension != "TXT") {
                message = 'The CGM file upload file must end with .txt, make sure you have selected the correct file to upload.';
                alert(message);
                return false;
            }
            $('#CgmUpload').val('True');
        }
    }

    //no need for further validation if there is a reason so return true
    if (isReason) {
        return true;
    }

    if (!$('#DateCompleted').val()) {
        message = 'Date completed is required.  Enter a reason if you can not provide this data.';
        alert(message);
        $('#DateCompleted').focus();
        return false;
    }
    
    if ($('#Age2to16').is(':checked')) {
        if (!($('#CBCL').is(':checked'))) {
            message = 'You must certify with a check mark that CBCL has been collected and sent to the CCC.  Enter a reason if you can not provide this data.';
            alert(message);
            $('#CBCL').focus();
            return false;
        }
        if (!($('#PedsQL').is(':checked'))) {
            message = 'You must certify with a check mark that Peds-QL has been collected and sent to the CCC.  Enter a reason if you can not provide this data.';
            alert(message);
            $('#PedsQL').focus();
            return false;
        }
        if (!($('#Demographics').is(':checked'))) {
            message = 'You must certify with a check mark that subject demographics has been collected and sent to the CCC.  Enter a reason if you can not provide this data.';
            alert(message);
            $('#Demographics').focus();
            return false;
        }
        if (!($('#ContactInfo').is(':checked'))) {
            message = 'You must certify with a check mark that subject contact information has been collected.  Enter a reason if you can not provide this data.';
            alert(message);
            $('#ContactInfo').focus();
            return false;
        }
    }

    if (!($('#Age2to16').is(':checked')) && !($('#AgeNot2to16').is(':checked'))) {
        message = 'You must select one of the age checkboxes.';
        alert(message);
        $('#ContactInfo').focus();
        return false;
    }
    return true;
}