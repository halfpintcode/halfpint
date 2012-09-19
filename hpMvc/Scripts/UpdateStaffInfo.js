﻿/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function () {
    $('#Sites').val($('#SiteID').val());

    var isValid = $('#IsValid').val();

    if (isValid === "true") {
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
    }
    else {
        $('#btnCancel').click(function () {
            window.location = urlRoot + '/Coordinator/Index';
        });

        $('#updateForm').submit(function () {
            return updateFormSubmit();
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
        var url = urlRoot + '/Coordinator/GetStaffInfo/?user=' + user

        $.ajax({
            url: url,
            type: 'POST',
            data: {},
            success: function (data) {
                if (data) {
                    $('#staffInfo').append(data);
                    $('#staffInfo').slideDown('slow');

                    $('#btnCancel').click(function () {
                        window.location = urlRoot + '/Coordinator/Index';
                    });

                    $('#updateForm').submit(function () {
                        return updateFormSubmit();
                    });

                }
                else {
                    alert(data);
                }
            }
        });

    }

    function updateFormSubmit() {
        var staffModel = {};
        //validation

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
        staffModel.EmployeeID = empID;

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
    }

});