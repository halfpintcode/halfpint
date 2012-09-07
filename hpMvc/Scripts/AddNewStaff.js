/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function () {
    $('#siteSpecific').hide();

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


    $('#Sites').change(function () {
        var site = $(this).val();
        $('#SiteID').val(site);

        //employee ID 
        if (site === "0") {
            $('#empIDRequired').val("");
            $('#empIDRegex').val("");
            $('#empIDMessage').val("");
            $('#siteSpecific').hide();
        }
        else {
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
                            $('#empIDRequired').val("");
                            $('#empIDRegex').val("");
                            $('#empIDMessage').text("");
                            $('#siteSpecific').hide();
                        }
                    }

                }
            });
        } //else site/employee ID

    }); //$('#Sites').change

    //todo - use ajax to check for duplicate
    //    $('#UserName').blur(function () {
    //        var uName = $.trim($('#UserName').val());
    //    });
    //    $('#Email').blur(function () {
    //        var email = $.trim($('#Email').val());
    //    });

    $(':input[type="checkbox"]').change(function () {
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
        //validation


        //        var role = $('#Roles').val();
        //        if (role === "Select a role") {
        //            alert('Role is required')
        //            return false;
        //        }

        //        var uName = $.trim($('#UserName').val());
        //        if (uName.length === 0) {
        //            alert('User name is required')
        //            return false;
        //        }


        //        var fName = $.trim($('#FirstName').val());
        //        if (fName.length === 0) {
        //            alert('First name is required')

        //            return false;
        //        }

        //        var lName = $.trim($('#LastName').val());
        //        if (lName.length === 0) {
        //            alert('Last name is required')
        //            return false;
        //        }

        //        var email = $.trim($('#Email').val());
        //        if (email.length === 0) {
        //            alert('Email address is required')
        //            return false;
        //        }
        //        if (!validateEmail(email)) {
        //            alert('Enter a valid email address')
        //            return false;
        //        }

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

        //        if ($('#NovaStatStrip').is(":checked")) {
        //            var doc = $.trim($('#NovaStatStripDoc').val());
        //            if (doc.length === 0) {
        //                alert("Date completed is required for Nova Stat Strip");
        //                return false;
        //            }

        //            var retVal = isValidDate(doc);

        //            if (retVal === "InvalidFormat") {
        //                alert("Invalid date format for Nova Stat Strip date completed. Use this format: mm/dd/yyyy");
        //                return false;
        //            }
        //            if (retVal === "InvalidDate") {
        //                alert("Invalid date for Nova Stat Strip date completed.");
        //                return false;
        //            }
        //        }

        var isValid = true;
        $(':input[type="checkbox"]').each(function () {
            var lable = "";
            var doc = "";
            var doc2 = "";
            var dateLable = "";
            if ($(this).is(":checked")) {
                doc = $.trim($(this).next().next().next().next().val());
                if (doc.length === 0) {
                    if ($(this).attr("id") === "HumanSubj") {
                        dateLable = "Date started";
                    }
                    else {
                        dateLable = "Date completed";
                    }
                    lable = $(this).next().next().text();
                    alert(dateLable + " is required for " + lable);
                    isValid = false;
                    return false;
                }
                if ($(this).attr("id") === "HumanSubj") {
                    doc2 = $.trim($(this).next().next().next().next().next().next().next().val());
                    if (doc2.length === 0) {
                        var lable = $(this).next().next().text();
                        alert("Date expired is required for " + lable);
                        isValid = false;
                        return false;
                    }
                    //if we made it to here then we have dates for both start and expired
                    var dStart = new Date(doc);
                    var dExpir = new Date(doc2);
                    if (doc > doc2) {
                        alert("Date expired must be later than date started for Human Subject Training");
                        isValid = false;
                        return false;
                    }
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