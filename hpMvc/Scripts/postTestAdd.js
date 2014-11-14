/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function () {
    $('.datePicker').datepicker({
        beforeShow: function (input, inst) {
            $.datepicker._pos = $.datepicker._findPos(input); //this is the default position 
            var pos = $(this).position();
            $.datepicker._pos[0] = pos.left + 100; //left              
        }
    });
    
    $('#save').click(function () {
        if (!validate()) {
            alert("A date is required for all tests!");
            return;
        }

        var url = window.urlRoot + '/PostTestsAdmin/AddPostTest';
        var data = $("form").serialize();
        $.ajax({
            url: url,
            type: 'POST',
            data: data,
            success: function (data1) {
                if (data1 === 1) {
                    alert('Post tests were saved successfully');
                }
                else if (data1 === 0) {
                    alert('A date is required for all tests.');
                }
                else {
                    alert('There was a problem saving post tests.  This problem has been reported to the administrator.');
                }
            }

        });

    });

    function validate() {
        var isVal = true;
        $('.datePicker').each(function(index) {
            if (! $(this).val()) {
                isVal = false;
            }
        });

        return isVal;
    }

});


