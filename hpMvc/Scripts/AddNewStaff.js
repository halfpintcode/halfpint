/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function () {
    $('#siteSpecific').hide();

    $('#empID').keydown(function (event) {
        numericsOnly(event, $(this).val());
    });

    $('#Sites').change(function () {
        var site = $(this).val()
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

    $('#newForm').submit(function () {
        //validation
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

    });
});