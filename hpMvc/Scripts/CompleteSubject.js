/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function () {
    if ($('#Older2').is(':checked')) {
        $('#overTwo').show();
    }
    else {
        $('#overTwo').hide();
    }

    $('#Older2').click(function () {
        $('#overTwo').slideToggle('slow', function () {
            if (!$(this).is(":visible")) {
                $('#CBCL').attr('checked', false);
                $('#PedsQL').attr('checked', false);
                $('#Demographics').attr('checked', false);
                $('#ContactInfo').attr('checked', false);
            }
        });
    });

});