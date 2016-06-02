/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function () {

    var phoneFormat = "999-999-9999";

    $('#Phone').mask(phoneFormat);


    //this is for the http post
    var siteVal = $('#Sites').val();
    if (siteVal > 0) {
        siteChange(siteVal);
    }
    else {
        $('#siteSpecific').hide();
    }

    //this is for the http post
    $('#Role').val($('#Roles').val());

//    $('#empID').keydown(function (event) {
//        numericsOnly(event, $(this).val());
//    });

    //handle date completed enable and disable (enable only when checked)
    $(':input[type="checkbox"]').each(function () {
        if ($(this).is(":checked")) {
            $(this).next().next().next().next().attr('disabled', false);
            if ($(this).attr("id") === "HumanSubj") {
                $(this).next().next().next().next().next().next().next().attr('disabled', false);
            }
        }
        else {
            $(this).next().next().next().next().val('');
            $(this).next().next().next().next().attr('disabled', 'disabled');
            if ($(this).attr("id") === "HumanSubj") {
                $(this).next().next().next().next().next().next().next().val('');
                $(this).next().next().next().next().next().next().next().attr('disabled', 'disabled');
            }
        }
    });

    $('#Roles').change(function () {
        $('#Role').val($('#Roles').val());
    });

    $('#Sites').change(function () {
        var site = $(this).val();
        $('#SiteID').val(site);

        //employee ID 
        if (site === "0") {
            $('#EmployeeID').val("");
            $('#empIDRequired').val("");
            $('#empIDRegex').val("");
            $('#empIDMessage').val("");
            $('#phoneFormat').val("");
            $('#phoneMessage').val("");
            $('#siteSpecific').hide();
        }
        else {
            siteChange(site);
        } //else site/employee ID

    }); //$('#Sites').change

    function siteChange(site) {
        $.ajax({
            url: urlRoot + '/Staff/GetSiteEmployeeInfo',
            type: 'POST',
            data: { site: site },
            success: function (data) {

                if (data.IsSuccessful) {
                    
                    if (data.Stuff[0].Value === "true") {
                        $('#empIDRequired').val("true");
                        $('#empIDRegex').val(data.Stuff[1].Value);
                        $('#empIDMessage').val(data.Stuff[2].Value);
                        $('#phoneFormat').val(data.Stuff[3].Value);
                        phoneFormat = $('#phoneFormat').val();
                        $('#phoneMessage').val(data.Stuff[4].Value);
                        $('#siteSpecific').show();
                        $('#empIDmessage').hide();
                    }
                    else {
                        $('#phoneFormat').val(data.Stuff[3].Value);
                        phoneFormat = $('#phoneFormat').val();
                        $('#phoneMessage').val(data.Stuff[4].Value);
                        $('#EmployeeID').val("");
                        $('#empIDRequired').val("");
                        $('#empIDRegex').val("");
                        $('#empIDMessage').text("");
                        $('#siteSpecific').hide();
                    }
                    $('#Phone').mask(phoneFormat);
                }

            }
        });
    }
    //use ajax to check for duplicate
    $('#UserName').blur(function () {
        var userName = $.trim($('#UserName').val());
        if (userName) {
            isUserNameDuplicate(userName);
        }
    });
    function isUserNameDuplicate(userName) {
        $.ajax({
            async: false,
            url: urlRoot + '/Admin/IsUserNameDuplicate/' + userName,
            type: 'POST',
            data: {},
            success: function (data) {
                if (data.ReturnValue == 1) {
                    alert('This user name is being used by ' + data.Message + '!');
                    retVal = true;
                }
                if (data.ReturnValue == -1) {
                    alert('There was an error cheking the database for a duplicate user name.');
                    retVal = false;
                }
            }
        });
    }

    $('#Email').blur(function () {
        var email = $.trim($('#Email').val());
        if (email) {
            isEmailDuplicate(email);    
        }
    });
    function isEmailDuplicate(email) {
        $.ajax({
            async: false,
            url: urlRoot + '/Admin/IsUserEmailDuplicate/?email=' + email,
            type: 'POST',
            data: {},
            success: function (data) {
                if (data.ReturnValue == 1) {
                    alert('This email is being used by ' + data.Message + '!');
                    retVal = true;
                }
                if (data.ReturnValue == -1) {
                    alert('There was an error cheking the database for a duplicate email.');
                    retVal = false;
                }
            }
        });
    }

    $('#EmployeeID').blur(function () {
        var empID = $.trim($('#EmployeeID').val());
        if (empID) {
            isEmployeeIDDuplicate(empID);    
        }
    });

    function isEmployeeIDDuplicate(empID) {
        var site = $('#Sites').val();
        var retVal = false;
        $.ajax({
            async: false,
            url: urlRoot + '/Admin/IsUserEmployeeIdDuplicate/?employeeID=' + empID + '&site=' + site,
            type: 'POST',
            data: {},
            success: function (data) {
                if (data.ReturnValue == 1) {
                    alert('This employee ID is being used by ' + data.Message + '!');
                    retVal = true;
                }
                if (data.ReturnValue == -1) {
                    alert('There was an error cheking the database for a duplicate employee ID.');
                    retVal = false;
                }
            }
        });
        return retVal;
    }

    $(':input[type="checkbox"]').change(function () {
        var id = $(this).attr('id');
        if (id === "SendEmail") {
            return;
        }

        if ($(this).is(":checked")) {
            $(this).next().next().next().next().attr('disabled', false);
            if ($(this).attr("id") === "HumanSubj") {
                var doc = $.trim($(this).next().next().next().next().val());
                //                if (doc.length > 0) {
                $(this).next().next().next().next().next().next().next().attr('disabled', false);
                //                }
            }
        }
        else {
            $(this).next().next().next().next().val('');
            $(this).next().next().next().next().attr('disabled', 'disabled');
            if ($(this).attr("id") === "HumanSubj") {
                $(this).next().next().next().next().next().next().next().val('');
                $(this).next().next().next().next().next().next().next().attr('disabled', 'disabled');
            }
        }
    });

    $('#btnClose').click(function () {
        window.location = urlRoot + '/Admin/Index';
    });

    $('#newForm').submit(function () {
        var staffModel = {};

        //validation
        var role = $('#Roles').val();
        if (role === "Select a role") {
            alert('Role is required')
            return false;
        }
        staffModel.Role = role;

        var uName = $.trim($('#UserName').val());
        if (uName.length === 0) {
            alert('User name is required')
            return false;
        }
        if (isUserNameDuplicate(uName)) {
            return false;
        }
        staffModel.UserName = uName;

        var fName = $.trim($('#FirstName').val());
        if (fName.length === 0) {
            alert('First name is required')

            return false;
        }
        staffModel.FirstName = fName;

        var lName = $.trim($('#LastName').val());
        if (lName.length === 0) {
            alert('Last name is required')
            return false;
        }
        staffModel.LastName = lName;

        var email = $.trim($('#Email').val());
        if (email.length === 0) {
            alert('Email address is required')
            return false;
        }
        if (!validateEmail(email)) {
            alert('Enter a valid email address')
            return false;
        }
        if (isEmailDuplicate(email)) {
            return;
        }
        staffModel.Email = email;

        if (!($('#EmployeeID').is(':hidden'))) {
            var empID = $.trim($('#EmployeeID').val());
            if (empID.length === 0) {
                alert('Employee id is required');
                return false;
            }
            var regex = $('#empIDRegex').val();

            if (!validateEmployeeID(regex, empID)) {
                var message = 'Employee id must be a ' + $('#empIDMessage').val();
                alert(message);
                $('#empIDmessage').show();
                return false;
            }
            if (isEmployeeIDDuplicate(empID)) {
                return false;
            }
        }
        staffModel.EmployeeID = empID;

        var isValid = true;
        $(':input[type="checkbox"]').each(function () {
            var doc = "";
            var doc2 = "";
            var dateLable = "";
            var docId = "";
            var retVal = "";

            var id = $(this).attr('id');
            if (id === "SendEmail") {
                return true; //continue
            }

            var lable = $(this).next().next().text();

            staffModel[id] = false;
            if ($(this).is(":checked")) {
                staffModel[id] = true;

                if (id === "HumanSubj") {
                    dateLable = "Date started";
                }
                else {
                    dateLable = "Date completed";
                }

                doc = $.trim($(this).next().next().next().next().val());
                docId = $(this).next().next().next().next().attr('id')
                if (doc.length === 0) {
                    alert(dateLable + " is required for " + lable);
                    isValid = false;
                    return false;
                }
                else {
                    retVal = isValidDate(doc);
                    if (retVal === "InvalidFormat") {
                        alert(dateLable + " is not a valid format for " + lable + " - valid format: mm/dd/yyyy");
                        isValid = false;
                        return false;
                    }
                    if (retVal === "InvalidDate") {
                        alert(dateLable + " is not a valid date for " + lable);
                        isValid = false;
                        return false;
                    }
                }
                staffModel[docId] = doc;

                if ($(this).attr("id") === "HumanSubj") {
                    doc2 = $.trim($(this).next().next().next().next().next().next().next().val());
                    if (doc2.length === 0) {
                        alert("Date expired is required for " + lable);
                        isValid = false;
                        return false;
                    }
                    else {
                        if (retVal === "InvalidDate") {
                            alert("Date expired is not a valid date for " + lable);
                            isValid = false;
                            return false;
                        }
                    }

                    //if we made it to here then we have dates for both start and expired
                    var dStart = new Date(doc);
                    var dExpir = new Date(doc2);
                    if (doc > doc2) {
                        alert("Date expired must be later than date started for Human Subject Training");
                        isValid = false;
                        return false;
                    }
                    staffModel.HumanSubExp = doc2;
                }
            }
        });

        if (!isValid) {
            return false;
        }

        var site = $('#Sites').val();
        if (site === "0") {
            alert('Site is required')
            return false;
        }
    });
});