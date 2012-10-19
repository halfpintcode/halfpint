/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function () {
    $('#Phone').mask("999-999-9999");

    $('#Sites').val($('#SiteID').val());
    var isValid = $('#IsValid').val();
    
    $('#Sites').change(function () {
        var site = $(this).val();
        $('#SiteID').val(site);

        $('#staffInfo').slideUp('slow');
        $('#staffInfo').empty();

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
       
    if (isValid === "false") {    
        $('#btnCancel').click(function () {
            window.location = urlRoot + '/Coordinator/Index';
        });

        $('#updateForm').submit(function () {
            return updateFormSubmit();
        });

        //handle date completed enable and disable (enable only when checked)
        $(':input[type="checkbox"]').each(function () {
            var id = $(this).attr('id');
            if (id === "Active") {
                return;
            }

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

        $(':input[type="checkbox"]').change(function () {
            var id = $(this).attr('id');
            if (id === "Active") {
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
    }

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
                getStaffList(site);
            }
        });
    }

    function getStaffList(site) {
        var url = urlRoot + '/Coordinator/GetStaffForSite/?site=' + site
        $('#Users').empty();
        $.ajax({
            url: url,
            type: 'POST',
            data: {},
            success: function (data) {
                if (data) {
                    $.each(data, function (index, d) {
                        $('#Users').append("<option value='" + d.ID + "'>" + d.Name + "</option>");
                    });
                }
                else {
                    alert(data);
                }
            }
        });
    }

    $('#Users').change(function () {
        var user = $(this).val();

        $('#staffInfo').slideUp('slow', function () {
            $('#staffInfo').empty();

            if (user === "0") {
                return;
            }
            getUserInfo(user);
        });

    });
    
    function getUserInfo(user) {
        var url = urlRoot + '/Admin/GetStaffInfo/?user=' + user

        $.ajax({
            url: url,
            type: 'POST',
            data: {},
            success: function (data) {
                if (data) {
                    $('#staffInfo').append(data);
                    $('#staffInfo').slideDown('slow');

                    $('#btnCancel').click(function () {
                        window.location = urlRoot + '/Admin/Index';
                    });

                    $('#updateForm').submit(function () {
                        return updateFormSubmit();
                    });

                    $('#Roles').change(function () {
                        $('#Role').val($('#Roles').val());
                    });

                    //handle date completed enable and disable (enable only when checked)
                    $(':input[type="checkbox"]').each(function () {
                        var id = $(this).attr('id');
                        if (id === "Active") {
                            return;
                        }

                        if ($(this).is(":checked")) {
                            $(this).next().next().next().next().attr('disabled', false);
                            if ($(this).attr("id") === "HumanSubj") {
                                $(this).next().next().next().next().next().next().attr('disabled', false);
                            }
                        }
                        else {
                            $(this).next().next().next().next().val('');
                            $(this).next().next().next().next().attr('disabled', 'disabled');
                            if ($(this).attr("id") === "HumanSubj") {
                                $(this).next().next().next().next().next().next().val('');
                                $(this).next().next().next().next().next().next().attr('disabled', 'disabled');
                            }
                        }
                    });

                    $(':input[type="checkbox"]').change(function () {
                        var id = $(this).attr('id');
                        if (id === "Active") {
                            return;
                        }

                        if ($(this).is(":checked")) {
                            $(this).next().next().next().next().attr('disabled', false);
                            if ($(this).attr("id") === "HumanSubj") {
                                var doc = $.trim($(this).next().next().next().next().val());
                                //if (doc.length > 0) {
                                    $(this).next().next().next().next().next().next().attr('disabled', false);
                                //}
                            }
                        }
                        else {
                            $(this).next().next().next().next().val('');
                            $(this).next().next().next().next().attr('disabled', 'disabled');
                            if ($(this).attr("id") === "HumanSubj") {
                                $(this).next().next().next().next().next().next().val('');
                                $(this).next().next().next().next().next().next().attr('disabled', 'disabled');
                            }
                        }
                    });

                }
                else {
                    alert(data);
                }
            }
        });

    }

    function updateFormSubmit() {
        var model = {};
        //validation
        //model.OldRole = $('#OldRole').val();
        //model.Role = $('#Roles').val();

        var fName = $.trim($('#FirstName').val());
        if (fName.length === 0) {
            alert('First name is required')

            return false;
        }
        model.FirstName = fName;

        var lName = $.trim($('#LastName').val());
        if (lName.length === 0) {
            alert('Last name is required')
            return false;
        }
        model.LastName = lName;

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
        }
        model.EmployeeID = empID;

        var email = $.trim($('#Email').val());
        if (email.length === 0) {
            alert('Email address is required')
            return false;
        }
        if (!validateEmail(email)) {
            alert('Enter a valid email address')
            return false;
        }
//        if (isEmailDuplicate(email)) {
//            return;
//        }
        staffModel.Email = email;

        var isValid = true;
        $(':input[type="checkbox"]').each(function () {

            var doc = "";
            var doc2 = "";
            var dateLable = "";
            var docId = "";
            var retVal = "";

            var id = $(this).attr('id');
            if (id === "Active") {
                return true; //continue
            }

            var lable = $(this).next().next().text();

            model[id] = false;
            if ($(this).is(":checked")) {
                model[id] = true;

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
                model[docId] = doc;

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
                    model.HumanSubExp = doc2;
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
    }

});