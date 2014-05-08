/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function () {
    var curRole, curUserName, newRole, newUserName;

    $('#Sites').val($('#SiteID').val());
    $('#divSelected').hide();

    $('#Sites').change(function () {
        var site = $(this).val();

        $('#SiteID').val(site);

        $('#staffInfo').slideUp('slow');
        $('#staffInfo').empty();

        if (site === "0") {
            clearSelected();
        }
        else {
            siteChange(site);
        }

    });

    var clearSelected = function () {
        $('#divSelected').hide();
    };

    function siteChange(site) {
        $.ajax({
            url: window.urlRoot + '/Staff/GetSiteEmployeeInfo',
            type: 'POST',
            data: { site: site },
            success: function (data) {
                if (data.IsSuccessful) {
                    if (data.Stuff[0].Value === "true") {
                        $('#empIDRequired').val("true");
                        $('#empIDRegex').val(data.Stuff[1].Value);
                        $('#empIDMessage').val(data.Stuff[2].Value);
                    }
                    else {
                        $('#empIDRequired').val("");
                        $('#empIDRegex').val("");
                        $('#empIDMessage').text("");
                    }
                }
                getStaffList(site);
            }
        });
    }

    function getStaffList(site) {
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
                }
                else {
                    alert(data);
                }
            }
        });
    }

    $('#Users').change(function () {
        var userId = $(this).val();
        if (userId === "0") {
            clearSelected();
            return;
        }
        getUserRole(userId);


    });

    $('#Roles').change(function () {
        newRole = $(this).val();
    });

    $('#btnSubmit').click(function () {
        if (newRole === curRole) {
            alert('New role and current role are the same');
            return;
        }
        newUserName = $('#userName').val();
        if (newRole !== "Nurse") {
            if (!curUserName) {
                if (!newUserName) {
                    alert('Enter a user name');
                    return;
                }
            }
        }

    });

    var getUserRole = function (userId) {
        var url = window.urlRoot + '/Admin/GetUserRoleAndUserName/?userId=' + userId;

        $.ajax({
            url: url,
            type: 'POST',
            data: {},
            success: function (data) {
                if (data) {
                    curRole = data.Role;
                    $('#lblCurrentRole').text(data.Role);
                    curUserName = data.userName;
                    $('#lblCurrentUserName').text(data.UserName);
                    if (data.UserName) {
                        $('#spnUserName').hide();
                    } else {
                        $('#spnUserName').show();
                    }
                    $('#divSelected').show();
                }
                else {
                    alert("No current user role");
                }
            }
        });
    };


})