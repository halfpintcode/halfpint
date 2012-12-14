/// <reference path="jquery-1.7-vsdoc.js" />
/// <reference path="jquery.validate-vsdoc.js" />

$(function () {
    if (urlRoot.indexOf("hpTest") > -1) {
        $('.hpProd').hide();
    }
    else {
        if (!confirm("Please confirm that this is an actual patient \u0028not a test subject\u0029.\nYou will need the date and time the consent was obtained if this is an actual patient.")) {
            window.location = urlRoot + '/Staff/Index';
        }        
    }

    $('#divPassword').hide();
    var password;
    var studyID;

    $('#StudyID').mask("99-9999-9");
    $('#ConsentTime').mask("99:99");

    $('#ConsentDate').datepicker({
        beforeShow: function (input, inst) {
            $.datepicker._pos = $.datepicker._findPos(input); //this is the default position 
            var pos = $(this).position();
            $.datepicker._pos[0] = pos.left + 100; //left              
        }
    });

    //extend validator
    $.validator.addMethod("StudyID", function (value, element) {
        return this.optional(element) || /\d{2}-\d{4}-\d$/.test(value);
    }, " *Must match format: nn-nnnn-n");

    //form validator
    var $validator = $("#mainForm").validate({
        rules: {
            StudyID: {
                required: true,
                StudyID: true
            }
        },
        debug: false
    });

    $('#lnkPrintPage').click(function (e) {
        e.preventDefault();
        window.open(urlRoot + '/InitializeSubject/PrintPassword?studyID=' + studyID + '&password=' + password, '_blank');
    });

    $('#btnCancel').click(function () {
        window.location = urlRoot + '/Staff/Index';
    });

    $('#btnSubmit').click(function () {
        var $isValid = $validator.form();
        if (!$isValid) {
            alert('The Study ID you entered is not in a valid format');
            return false;
        }

        studyID = $('#StudyID').val();
        var siteID = $('#userSite').text().substr(0, 2);
        if (siteID !== studyID.substr(0, 2)) {
            alert('The study id for your site must begin with ' + siteID);
            return false;
        }

        if (urlRoot.indexOf("hpProd") > -1) {
            var time = $.trim($('#ConsentTime').val());
            if (time.length === 0) {
                alert("Consent time is required");
                return false;
            }
            if (!isValidTime(time)) {
                alert("Invalid time! Time format example: 23:59");
                return false;
            }
        }


        var url = urlRoot + '/InitializeSubject/InitializePassword/' + studyID;
        var data = $("form").serialize();
        $.ajax({
            url: url,
            type: 'POST',
            data: data,
            success: function (data) {
                if (data.IsSuccessful) {
                    $('#yourPassword').remove();
                    password = data.Password;
                    $('#divPassword').prepend("<p id='yourPassword'>Your password is: <b>" + password + "</b> </p>");
                    $('#divPassword').show();
                    $('#btnSubmit').attr('disabled', 'disabled');
                    $('#btnCancel').attr('disabled', 'disabled');
                }
                else {

                    alert(data.Message);
                }
            }
        });
        return false;
    });


});