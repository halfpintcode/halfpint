/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function () {
    $('#testMenu').hide();
    $('#divName').hide();
    $('#btnStart').attr('disabled', 'disabled');

    var empIDReq = $('#empIDRequired').val();
    if (empIDReq === "true") {
        var reg = $('#empIDRegex').val();
        //check if leading digits
        var leadDig = "";
        var c = "";
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

    //submit after complete
    $('#btnSubmit').click(function () {
        var userName = isNameSelected();

        if (userName.length === 0) {
            alert('Select or create a name')
            return;
        }

        var email = $.trim($('#sEmail').val());
        if (email.length === 0) {
            alert('Enter an email address')
            return;
        }
        if (!validateEmail(email)) {
            alert('Enter a valid email address')
            return;
        }

        $('#btnSubmit').attr('disabled', 'disabled');
        var data = { Name: userName, Email: email };
        var url = urlRoot + '/PostTests/Submit'
        $.ajax({
            url: url,
            type: 'POST',
            data: data,
            success: function (data) {
                if (data === 'no tests') {
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
        }
                
        var url = urlRoot + '/PostTests/CreateName'
        $('#btnCreate').attr('disabled', 'disabled');
        $.ajax({
            url: url,
            type: 'POST',
            data: { LastName: lastName, FirstName:firstName, EmpID: empID, Email: email },
            success: function (data) {
                //data.ReturnValue contains the id for the new staff record
                if (data.ReturnValue > 0) {
                    alert(firstName + ' ' + lastName + ' created successfully');
                    $('#ID').val(data.ReturnValue);
                    $('#testMenu').show();
                    $('#divClickHere').hide();
                    $('#sEmail').val($('#email').val());
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
        $('#btnStart').attr('disabled', 'disabled')

        var url = urlRoot + '/PostTests/GetTestsCompleted'
        var name = $("option:selected", $('#Users')).text();
        var id = $('#Users').val();
        var srcUrl = urlRoot + "/Content/images/check2.jpg";
        var img = '<img alt="" src=' + srcUrl + ' />'
        var text = '';
        var completed = '';
        $.ajax({
            url: url,
            type: 'POST',
            data: { Name: name, ID: id },
            success: function (data) {
                $('#sEmail').val(data.email);
                $.each(data.tests, function (index, d) {
                    switch (d.Name) {
                        case 'Overview':
                            //text = $('#Overview').children().text();
                            completed = '  (' + d.sDateCompleted + ') ';
                            $('#Overview').prepend(completed).prepend(img);
                            if (completed.length > 0) {
                                $('#Overview').addClass('completed');
                            }
                            break;
                        case 'Checks':
                            //text = $('#Checks').children().text();
                            completed = '  (' + d.sDateCompleted + ') ';
                            $('#Checks').prepend(completed).prepend(img);
                            if (completed.length > 0) {
                                $('#Checks').addClass('completed');
                            }
                            break;
                        case 'Medtronic':
                            //text = $('#Medtronic').children().text();
                            completed = '  (' + d.sDateCompleted + ') ';
                            $('#Medtronic').prepend(completed).prepend(img);
                            if (completed.length > 0) {
                                $('#Medtronic').addClass('completed');
                            }
                            break;
                        case 'NovaStatStrip':
                            //text = $('#NovaStatStrip').children().text();
                            completed = '  (' + d.sDateCompleted + ') ';
                            $('#NovaStatStrip').prepend(completed).prepend(img);
                            if (completed.length > 0) {
                                $('#NovaStatStrip').addClass('completed');
                            }
                            break;
                        case 'VampJr':
                            //text = $('#VampJr').children().text();
                            completed = '  (' + d.sDateCompleted + ') ';
                            //$('#VampJr').prepend(img).append(completed).children('a').remove().end().append(text);
                            $('#VampJr').prepend(completed).prepend(img);
                            if (completed.length > 0) {
                                $('#VampJr').addClass('completed');
                            }
                            break;
                    }
                });
            }
        });

        $('#testMenu').show();
    });

    //if a name was passed as part of the url then automatically click start
    if ($('#Users').val() !== "0") {
        $('#btnStart').click();
    }

    $('.aLnk').click(function (e) {

        var userName = isNameSelected();
        
        if (userName.length === 0) {
            e.preventDefault();
            alert('Select or create a name')
            return;
        }

        var id = $('#Users').val();
        var path = urlRoot + '/PostTests/' + $(this).parent().attr('id') + '/' + id + '?name=' + userName;
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
                e.preventDefault();
                alert('Select your name from the list');
                return;
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

});


 
function isInteger (s) 
{
    var reInteger = /^\d+$/;     
    return reInteger.test(s) 
}