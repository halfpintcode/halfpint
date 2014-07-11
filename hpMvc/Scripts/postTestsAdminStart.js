/// <reference path="jquery-1.7-vsdoc.js" />
$(function () {
    var empId = "";
    var siteId = $('#siteId').val();

    $('#divName').hide();
    $('#btnEdit').attr('disabled', 'disabled');

    $("#spinner").ajaxStart(function () { $(this).show(); })
			   .ajaxStop(function () { $(this).hide(); });

    var empIdReq = $('#empIDRequired').val();
    if (empIdReq === "true") {
        var reg = $('#empIDRegex').val();
        //check if leading digits and display them if any
        var leadDig = "";
        var c;
        for (var i = 0; i < reg.length; i++) {
            //skip i=0
            if (i === 0) {
                continue;
            }
            c = reg[i];
            if (isInteger(c)) {
                leadDig = leadDig + c.toString();
            }
            else {
                break;
            }
        }
        if (leadDig.length) {
            $('#empID').val(leadDig);
        }

        if (siteId === '13') {
            //$('#empIDExtraLabel').show();
            $('#lblEmpID').text('Enter the last 4 digits of your employee id:');
        }
        $('#siteSpecific').show();
        $('#empIDmessage').hide();
//        $('#empID').keydown(function (event) {
//            window.numericsOnly(event, $(this).val());
//        });
    }
    else {
        $('#siteSpecific').hide();
    }
    
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
        var url = window.urlRoot + '/PostTestsAdmin/EditPostTest/' + name;
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
        if (!window.validateEmail(email)) {
            alert('Enter a valid email address');
            return;
        }
        if (isEmailDuplicate(email)) {
            return;
        }

        if (!($('#empID').is(':hidden'))) {
            empId = $.trim($('#empID').val());
            if (empId.length === 0) {
                alert('Employee id is required');
                return;
            }
            var regex = $('#empIDRegex').val();

            if (!window.validateEmployeeID(regex, empId)) {
                var message = 'Employee id must be a ' + $('#empIDMessage').val();
                if (siteId === '13') {
                    message = 'Employee id must be the ';
                }
                message = message + $('#empIDMessage').val();
                alert(message);
                $('#empIDmessage').show();
                return;
            }
            if (isEmployeeIdDuplicate()) {
                return;
            }
        }

        var url = window.urlRoot + '/PostTests/CreateName';
        $.ajax({
            url: url,
            type: 'POST',
            data: { LastName: lastName, FirstName: firstName, EmpID: empId, Email: email },
            success: function (data) {
                if (data.ReturnValue > 0) {
                    url = window.urlRoot + '/PostTestsAdmin/EditPostTest/' + data.ReturnValue;
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
            url: window.urlRoot + '/PostTests/IsUserEmailDuplicate/?email=' + email,
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

//    $('#empID').blur(function () {
//        empId = $.trim($('#empID').val());
//        if (empId.length === 0) {
//            return;
//        }
//        isEmployeeIdDuplicate(empId);
//    });

    function isEmployeeIdDuplicate() {
        var retVal = false;
        
        $.ajax({
            async: false,
            url: window.urlRoot + '/PostTests/IsUserEmployeeIdDuplicate/?employeeID=' + empId,
            type: 'POST',
            data: {},
            success: function (data) {
                if (data.ReturnValue == 1) {
                    if (siteId === '13') {
                        var nextNum = data.Bag;
                        alert('This employee ID is being used, the Halfpint website will use ' + nextNum + ' as your Halfpint employee Id');
                        $('#empID').val(nextNum);
                        empId = nextNum;
                        retVal = false;
                    }
                    else {
                        alert('This employee ID is being used by ' + data.Message + '!\nIf this is you then select your name from the list.\nIf it\'s not you then contact the coordinator.');
                        retVal = true;
                    }
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

function isInteger(s) {
    var reInteger = /^\d+$/;
    return reInteger.test(s);
}