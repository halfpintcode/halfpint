/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function () {
    $('#Phone').mask("999-999-9999");

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
    
    $('#empID').keydown(function (event) {
        numericsOnly(event, $(this).val());
    });

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
                        $('#siteSpecific').show();
                        $('#empIDmessage').hide();
                    }
                    else {
                        $('#EmployeeID').val("");
                        $('#empIDRequired').val("");
                        $('#empIDRegex').val("");
                        $('#empIDMessage').text("");
                        $('#siteSpecific').hide();
                    }
                }

            }
        });
    }
    //todo - use ajax to check for duplicate
    //    $('#UserName').blur(function () {
    //        var uName = $.trim($('#UserName').val());
    //    });
    //    $('#Email').blur(function () {
    //        var email = $.trim($('#Email').val());
    //    });

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

    $('#newForm').submit(function () {
        var staffModel = {};
        //validation


//        var role = $('#Roles').val();
//        if (role === "Select a role") {
//            alert('Role is required')
//            return false;
//        }
//        staffModel.Role = role;

//        var uName = $.trim($('#UserName').val());
//        if (uName.length === 0) {
//            alert('User name is required')
//            return false;
//        }
//        staffModel.UserName = uName;

//        var fName = $.trim($('#FirstName').val());
//        if (fName.length === 0) {
//            alert('First name is required')

//            return false;
//        }
//        staffModel.FirstName = fName;

//        var lName = $.trim($('#LastName').val());
//        if (lName.length === 0) {
//            alert('Last name is required')
//            return false;
//        }
//        staffModel.LastName = lName;

//        var email = $.trim($('#Email').val());
//        if (email.length === 0) {
//            alert('Email address is required')
//            return false;
//        }
//        if (!validateEmail(email)) {
//            alert('Enter a valid email address')
//            return false;
//        }
//        staffModel.Email = email;

//        if (!($('#EmployeeID').is(':hidden'))) {
//            var empID = $.trim($('#EmployeeID').val());
//            if (empID.length === 0) {
//                alert('Employee id is required');
//                return false;
//            }
//            var regex = $('#empIDRegex').val();

//            if (!validateEmployeeID(regex, empID)) {
//                var message = 'Employee id must be a ' + $('#empIDMessage').val();
//                alert(message);
//                $('#empIDmessage').show();
//                return false;
//            }
//        }
//        staffModel.EmployeeID = empID;

//        var isValid = true;
//        $(':input[type="checkbox"]').each(function () {
//            var doc = "";
//            var doc2 = "";
//            var dateLable = "";
//            var docId = "";
//            var retVal = "";

//            var id = $(this).attr('id');
//            if (id === "SendEmail") {
//                return true; //continue
//            }

//            var lable = $(this).next().next().text();

//            staffModel[id] = false;
//            if ($(this).is(":checked")) {
//                staffModel[id] = true;

//                if (id === "HumanSubj") {
//                    dateLable = "Date started";
//                }
//                else {
//                    dateLable = "Date completed";
//                }

//                doc = $.trim($(this).next().next().next().next().val());
//                docId = $(this).next().next().next().next().attr('id')
//                if (doc.length === 0) {
//                    alert(dateLable + " is required for " + lable);
//                    isValid = false;
//                    return false;
//                }
//                else {
//                    retVal = isValidDate(doc);
//                    if (retVal === "InvalidFormat") {
//                        alert(dateLable + " is not a valid format for " + lable + " - valid format: mm/dd/yyyy");
//                        isValid = false;
//                        return false;
//                    }
//                    if (retVal === "InvalidDate") {
//                        alert(dateLable + " is not a valid date for " + lable);
//                        isValid = false;
//                        return false;
//                    }
//                }
//                staffModel[docId] = doc;

//                if ($(this).attr("id") === "HumanSubj") {
//                    doc2 = $.trim($(this).next().next().next().next().next().next().next().val());
//                    if (doc2.length === 0) {
//                        alert("Date expired is required for " + lable);
//                        isValid = false;
//                        return false;
//                    }
//                    else {
//                        if (retVal === "InvalidDate") {
//                            alert("Date expired is not a valid date for " + lable);
//                            isValid = false;
//                            return false;
//                        }
//                    }

//                    //if we made it to here then we have dates for both start and expired
//                    var dStart = new Date(doc);
//                    var dExpir = new Date(doc2);
//                    if (doc > doc2) {
//                        alert("Date expired must be later than date started for Human Subject Training");
//                        isValid = false;
//                        return false;
//                    }
//                    staffModel.HumanSubExp = doc2;
//                }
//            }
//        });

//        if (!isValid) {
//            return false;
//        }

//        var site = $('#Sites').val();
//        if (site === "0") {
//            alert('Site is required')
//            return false;
//        }



    });
});