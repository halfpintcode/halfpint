/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function () {
    $('.spanDate').each(function () {
        var val = $(this).children().eq(1).val();
        if (!val) {
            $(this).hide();
        }

    });

    $(':checkbox').click(function () {
        if ($(this).attr('checked')) {
            $(this).nextAll().eq(3).show();
        }
        else {
            var el = $(this).nextAll().eq(3).hide().children().eq(1).val('');
            //alert($(this).nextAll().eq(3).children().eq(1).val());
        }
    })

    $('#save').click(function () {
        if (!validatePostEditAdmin()) {
            return;
        }

        var url = urlRoot + '/PostTestsAdmin/EditPostTest';
        var data = $("form").serialize();
        $.ajax({
            url: url,
            type: 'POST',
            data: data,
            success: function (data) {
                if (data === 1) {
                    alert('Post tests were saved successfully');
                }
                else {
                    alert('There was a problem saving post tests.  This problem has been reported to the administrator.');
                }
            }

        });

    });

});


function validatePostEditAdmin() {
    var retVal = true;
    var msg = '';
    var idx = 0;
    var that;

    $(':checkbox').each(function (index) {
        if ($(this).attr('checked')) {
            var name = $(this).attr('id');
            var val = $(this).nextAll().eq(3).children().eq(1).val();
            if (!val) {
                msg = 'Date completed is required for ' + name;
                that = $(this);
                idx = index;
                retVal = false;
                return false;
            }
        }

    });
    if(!retVal){
        that.nextAll().eq(3).children().eq(1).focus();
        alert(msg); 
    }           
    return retVal;
}