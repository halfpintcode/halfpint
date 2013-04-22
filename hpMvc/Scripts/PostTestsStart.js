/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function () {
    var empId = "";
    var siteId = $('#siteId').val();
    var allTestsCompleted = true;

    $('#testMenu').hide();
    $('#divName').hide();
    $('#btnStart').attr('disabled', 'disabled');

    $("#spinner").ajaxStart(function () { $(this).show(); })
			   .ajaxStop(function () { $(this).hide(); });


    //employee id is site specific
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

        //$('#empIDExtraLabel').hide();
        if (siteId === '13') {
            //$('#empIDExtraLabel').show();
            $('#lblEmpID').text('Enter the last 4 digits of your employee id:');
        }
        $('#siteSpecific').show();
        $('#empIDmessage').hide();
        $('#empID').keydown(function (event) {
            window.numericsOnly(event, $(this).val());
        });
    }
    else {
        $('#siteSpecific').hide();
    }

    //show and hide the two differnment ways a user can get started
    //1) select name from a dropdown 2) create new name
    //this is for the nurse role, for all other roles the name will
    //be atuomatically selected
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

    //enable the start button when a user is selected
    //disable if default is selected
    $('#Users').change(function () {
        var user = $(this).val();
        if (user === 'Select Your Name') {
            $('#btnStart').attr('disabled', 'disabled');
        }
        else {
            $('#btnStart').attr('disabled', false);
        }

    });

    //submit tests for email notifications after complete
    $('#btnSubmit').click(function () {
        var userName = isNameSelected();
        var id = $('#Users').val();

        if (userName.length === 0) {
            alert('Select or create a name');
            return;
        }

        var email = $.trim($('#sEmail').val());
        if (email.length === 0) {
            alert('Enter an email address');
            return;
        }
        if (!window.validateEmail(email)) {
            alert('Enter a valid email address');
            return;
        }
        if (!($('#empID').is(':hidden'))) {
            empId = $.trim($('#empID').val());
        }

        $('#btnSubmit').attr('disabled', 'disabled');
        var data = { Name: userName, Email: email, ID: id, EmployeeId: empId };
        var url = window.urlRoot + '/PostTests/Submit';
        $.ajax({
            url: url,
            type: 'POST',
            data: data,
            success: function (data1) {
                if (data1 === 'no tests') {
                    alert('You must complete all required tests before submitting');
                    $('#btnSubmit').attr('disabled', false);
                }
                else {
                    alert('Your completed tests have been submitted');
                }
            }
        });

    });

    //create new user
    $('#btnCreate').click(function () {
        //        wasCreateClicked = true;
        //        validationTimer = null;
        //console.log('create click');

        var firstName = $.trim($('#firstName').val());
        if (firstName.length === 0) {
            alert('first name is required');
            //wasCreateClicked = false;
            return;
        }
        var lastName = $.trim($('#lastName').val());
        if (lastName.length === 0) {
            alert('last name is required');
            //wasCreateClicked = false;
            return;
        }
        var email = $.trim($('#email').val());
        if (email.length === 0) {
            alert('email is required');
            //wasCreateClicked = false;
            return;
        }
        if (!window.validateEmail(email)) {
            alert('Enter a valid email address');
            //wasCreateClicked = false;
            return;
        }
        if (isEmailDuplicate(email)) {
            //wasCreateClicked = false;
            return;
        }

        if (!($('#empID').is(':hidden'))) {
            empId = $.trim($('#empID').val());
            if (empId.length === 0) {
                alert('Employee id is required');
                //wasCreateClicked = false;
                return;
            }
            var regex = $('#empIDRegex').val();

            if (!window.validateEmployeeID(regex, empId)) {
                var message = 'Employee id must be a ';
                if (siteId === '13') {
                    message = 'Employee id must be the ';
                }
                message = message + $('#empIDMessage').val();
                alert(message);
                $('#empIDmessage').show();
                //wasCreateClicked = false;
                return;
            }

            var bIsDupe = isEmployeeIdDuplicate();
            //console.log('bIsDupe:' + bIsDupe);
            if (bIsDupe) {


                //wasCreateClicked = false;
                return;
            }

        }

        var url = window.urlRoot + '/PostTests/CreateName';
        $('#btnCreate').attr('disabled', 'disabled');
        $.ajax({
            url: url,
            type: 'POST',
            data: { LastName: lastName, FirstName: firstName, EmpID: empId, Email: email },
            success: function (data) {
                //data.ReturnValue contains the id for the new staff record
                if (data.ReturnValue > 0) {
                    alert(firstName + ' ' + lastName + ' created successfully');

                    url = window.urlRoot + '/PostTests/Initialize/' + data.ReturnValue;
                    window.location = url;

                }
                else {
                    alert(data.Message);
                    $('#btnCreate').attr('disabled', false);
                }
            }
        });


    });

    //start with a selected user
    $('#btnStart').click(function () {
        $('#divClickHere').hide();
        $('#Users').attr('disabled', 'disabled');
        $('#btnStart').attr('disabled', 'disabled');
        $('#btnSubmit').attr('disabled', 'disabled');
        
        var url = window.urlRoot + '/PostTests/GetTestsCompletedActive';
        var name = $("option:selected", $('#Users')).text();
        var id = $('#Users').val();
        var srcUrlCheck = window.urlRoot + "/Content/images/check2.jpg";
        var imgCheck = '<img alt="" src=' + srcUrlCheck + ' />';
        var srcUrlX = window.urlRoot + "/Content/images/x.jpg";
        var imgX = '<img alt="" src=' + srcUrlX + ' />';

        var completed = '';
        $.ajax({
            url: url,
            type: 'POST',
            data: { Name: name, ID: id },
            success: function (data) {
                $('#sEmail').val(data.email);
                allTestsCompleted = true;
                $.each(data.tests, function (index, d) {
                    var $li = $('<li id="' + d.PathName + '"><a href="#">' + d.Name + ' Post Test</a></li>');
                    $('#testMenu ul').append($li);
                    if (d.IsCompleted) {
                        if (d.IsExpiring || d.IsExpired) {
                            $li.prepend('&nbsp;').prepend(d.sDateCompleted).prepend(imgX);
                            allTestsCompleted = false;
                        } else {
                            $li.prepend('&nbsp;').prepend(d.sDateCompleted).prepend(imgCheck);
                        }
                        $li.addClass('completed');
                    } else {
                        allTestsCompleted = false;
                        $li.prepend('&nbsp;').prepend(imgX);
                    }
                });
                if (siteId === "2") {
                    $('#btnSubmit').attr('disabled', false);
                }
                if (allTestsCompleted) {
                    $('#btnSubmit').attr('disabled', false);
                }
            }
        });

        $('#testMenu').show();
    });

    //if a name was passed as part of the url then automatically click start
    if ($('#Users').val() !== "0") {
        $('#btnStart').click();
        $('#btnStart').hide();
    }


    $('#testMenu ul').on('click', "li", function () {
        var test = ($(this).attr("id"));
        var userName = isNameSelected();

        if (userName.length === 0) {
            alert('Select or create a name');
            return;
        }
        var id = $('#Users').val();
        var path = window.urlRoot + '/PostTests/' + test + '/' + id + '?name=' + userName;
        if ($(this).hasClass('completed')) {
            path = path + '&completed=true';
        }

        window.location = path;
    });

    $('.aLnk').click(function (e) {

        var userName = isNameSelected();

        if (userName.length === 0) {
            e.preventDefault();
            alert('Select or create a name');
            return;
        }

        var id = $('#Users').val();
        var path = window.urlRoot + '/PostTests/' + $(this).parent().attr('id') + '/' + id + '?name=' + userName;
        if ($(this).parent().hasClass('completed')) {
            path = path + '&completed=true';
        }

        $(this).attr('href', path);


    });

    function isNameSelected() {
        var userName = '';
        var val = '';

        //if selected from list
        if ($('#listName').is(':visible')) {
            val = $('#Users').val();
            if (val === '0') {
                //e.preventDefault();
                alert('Select your name from the list');
                return userName;
            }
            userName = $("option:selected", $('#Users')).text();
        }
        else {
            //check first and last names
            var fName = $('#firstName').val();
            var lName = $('#lastName').val();

            userName = $.trim(fName) + ' ' + $.trim(lName);
        }
        return userName;
    }

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
                    alert('This email is being used by ' + data.Message + '!\nIf this is you then select your name from the list.\nIf it\'s not you then contact the coordinator.');
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
    //        console.log('blur');
    //        validationTimer = setTimeout(function () {
    //            validationTimer = null;
    //            console.log('validationTimer');
    //            console.log('in blur - wasCreateClicked:' + wasCreateClicked);
    //            if (wasCreateClicked) {
    //                return;
    //            }
    //            var empId = $.trim($('#empID').val());
    //            if (empId.length === 0) {
    //                return;
    //            }
    //            isEmployeeIdDuplicate(empId);
    //        }, 200);
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


 
function isInteger (s) 
{
    var reInteger = /^\d+$/;
    return reInteger.test(s);
}