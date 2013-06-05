/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function () {
    var site, user;

    //$('#Sites').val($('#SiteID').val());

    $('#Sites').change(function () {
        site = $(this).val();
        $('#SiteID').val(site);

        //        $('#staffInfo').slideUp('slow');
        //        $('#staffInfo').empty();

        if (site === "0") {
            $('#empIDRequired').val("");
            $('#empIDRegex').val("");
            $('#empIDMessage').val("");
        }
        else {
            siteChange(site);
        }

    }); //$('#Sites').change

    function siteChange() {
        var url = window.urlRoot + '/Coordinator/GetStaffForSite/?site=' + site;
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
                } else {
                    alert(data);
                }
            }
        });
    }

    $('#Users').change(function () {
        user = $(this).val();

        $('#staffInfo').slideUp('slow', function () {
            $('#staffInfo').empty();

            if (user === "0") {
                return;
            }
            getUserSubscriptions();
        });

    });

    function getUserSubscriptions() {
        var url = window.urlRoot + '/Notifications/GetSubscriptionInfo/?user=' + user;
        $.ajax({
            url: url,
            type: 'POST',
            data: {},
            success: function (data) {
                
            }
        });
    }

});