/// <reference path="jquery-1.7-vsdoc.js" />
$(function () {
    $('#divName').hide();
    $('#btnEdit').attr('disabled', 'disabled');

    var empIDReq = $('#empIDRequired').val();
    if (empIDReq === "true") {
        $('#siteSpecific').show();
        $('#empIDmessage').hide();
        $('#empID').keydown(function (event) {
            numericsOnly(event, $(this).val());
        });
    }
    else {
        $('#siteSpecific').hide();
    }

    $('#waitGif').hide();
    $.ajaxSetup({
        beforeSend: function () {
            $('#waitGif').show();
        },
        complete: function () {
            $('#waitGif').hide();
        }
    });

    //show and hide the two differnment ways a user can get started
    //1) select name from a dropdown 2) create new name
    $('#clickHere').click(function (e) {
        e.preventDefault();
        if ($('#listName').is(':visible')) {
            $('#spanClickHere').text(' to select your name from a list if you have previously created one. ');
            $('#listName').hide();
            $('#divName').show();
        }
        else {
            $('#spanClickHere').text(' if you have not previously created a name. ');
            $('#divName').hide();
            $('#listName').show();
        }
    });

    //disable the start button when a user is selected
    //enable if default is selected
    $('#Users').change(function () {
        var user = $(this).val();
        if (user === 'Select Your Name') {
            $('#btnEdit').attr('disabled', 'disabled');
        }
        else {
            $('#btnEdit').attr('disabled', false);
        }

    });

    $('#btnEdit').click(function () {
        var name = $('#Users').val();
        var url = urlRoot + '/PostTestsAdmin/EditPostTest/' + name;
        window.location = url;
    });

    //create new user
    $('#btnCreate').click(function () {
        var firstName = $.trim($('#firstName').val());
        if (firstName.length === 0) {
            alert('first name is required');
            return;
        }
        var lastName = $.trim($('#lastName').val());
        if (lastName.length === 0) {
            alert('last name is required');
            return;
        }
        var email = $.trim($('#email').val());
        if (email.length === 0) {
            alert('email is required');
            return;
        }
        if (!validateEmail(email)) {
            alert('Enter a valid email address')
            return;
        }
        if (isEmailDuplicate(email)) {
            return;
        }

        if (!($('#empID').is(':hidden'))) {
            var empID = $.trim($('#empID').val());
            if (empID.length === 0) {
                alert('Employee id is required');
                return;
            }
            var regex = $('#empIDRegex').val();

            if (!validateEmployeeID(regex, empID)) {
                var message = 'Employee id must be a ' + $('#empIDMessage').val();
                alert(message);
                $('#empIDmessage').show();
                return;
            }
            if (isEmployeeIDDuplicate(empID)) {
                return;
            }
        }

        var name = firstName + ' ' + lastName;
        var url = urlRoot + '/PostTests/CreateName'
        $.ajax({
            url: url,
            type: 'POST',
            data: { LastName: lastName, FirstName: firstName, EmpID: empID, Email: email },
            success: function (data) {
                if (data.ReturnValue > 0) {
                    var url = urlRoot + '/PostTestsAdmin/EditPostTest/' + data.ReturnValue;
                    window.location = url;
                }
                else {
                    alert(data.Message);
                }
            }
        });


    });

    $('#email').blur(function () {
        var email = $.trim($('#email').val());

        if (email.length === 0) {
            return;
        }
        isEmailDuplicate(email);
    });

    //made as an async funtion because this is used in validation
    function isEmailDuplicate(email) {
        var retVal = false;
        $.ajax({
            async: false,
            url: urlRoot + '/PostTests/IsUserEmailDuplicate/?email=' + email,
            type: 'POST',
            data: {},
            success: function (data) {
                if (data.ReturnValue == 1) {
                    alert('This email is being used by ' + data.Message + '!\nIf this is the person you want then select this name from the list.\nIf it\'s not then contact the web master with the information.');
                    retVal = true;
                }
                if (data.ReturnValue == -1) {
                    alert('There was an error cheking the database for a duplicate email.\nPlease contact the coordinator if this error continues.');
                    retVal = false;
                }
            }
        });
        return retVal;
    }

    $('#empID').blur(function () {
        var empID = $.trim($('#empID').val());
        if (empID.length === 0) {
            return;
        }
        isEmployeeIDDuplicate(empID);
    });

    function isEmployeeIDDuplicate(empID) {
        var retVal = false;
        $.ajax({
            async: false,
            url: urlRoot + '/PostTests/IsUserEmployeeIdDuplicate/?employeeID=' + empID,
            type: 'POST',
            data: {},
            success: function (data) {
                if (data.ReturnValue == 1) {
                    alert('This employee ID is being used by ' + data.Message + '!\nIf this is the person you want then select this name from the list.\nIf it\'s not then contact the web master with the information.');
                    retVal = true;
                }
                if (data.ReturnValue == -1) {
                    alert('There was an error cheking the database for a duplicate employee ID.\nPlease contact the coordinator if this error continues.');
                    retVal = false;
                }
            }
        });
        return retVal;
    }
});