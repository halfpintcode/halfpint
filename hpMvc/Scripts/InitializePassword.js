/// <reference path="jquery-1.7-vsdoc.js" />
/// <reference path="jquery.validate-vsdoc.js" />

$(function () {
    $('#divPassword').hide();
    var password;
    var studyID;

    $('#StudyID').mask("99-9999-9");
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
        window.open(urlRoot + '/InitializeSubject/ShowInitializePassword?studyID=' + studyID + '&password=' + password, '_blank');
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


        var url = urlRoot + '/InitializeSubject/InitializePassword/' + studyID;

        $.ajax({
            url: url,
            type: 'POST',
            data: { StudyID: studyID },
            success: function (data) {
                if (data.IsSuccessful) {
                    $('#yourPassword').remove();
                    password = data.Password;
                    $('#divPassword').prepend("<p id='yourPassword'>Your password is: <b>" + password + "</b> </p>");
                    $('#divPassword').show();
                    $('#btnSubmit').attr('disabled', 'disabled');
                }
                else {

                    alert(data.Message);
                }
            }
        });
        return false;
    });


});