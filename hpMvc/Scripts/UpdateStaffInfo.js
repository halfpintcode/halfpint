/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function () {
    $('#Sites').val($('#SiteID').val());

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
        $('#staffInfo').empty();

        var url = urlRoot + '/Coordinator/GetStaffInfo/?user=' + user
        
        $.ajax({
            url: url,
            type: 'POST',
            data: {},
            success: function (data) {
                if (data) {
                    $('#staffInfo').slideUp();
                    $('#staffInfo').append(data);
                    $('#staffInfo').slideDown();
                }
                else {
                    alert(data);
                }
            }
        });

    });

});