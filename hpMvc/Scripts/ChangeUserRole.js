/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function () {
    var curRole, curUserName, newUserName, newRole, site, userId;
    $('#Sites').val($('#SiteID').val());
    clearSelected();
    //remove nurse role from select list
    $('#Roles >option').each(function() {
        if (this.text === "Nurse") {
            $(this).remove();
        }
    });
    site = $('#SiteID').val();
    getSiteEmployeeInfo();
    
    function clearSelected() {
        $('#divSelected').slideUp('fast');
        $('#btnSubmit').attr("disabled", "disabled");
        $('#userName').val('');
        newUserName = '';
        newRole = 'Coordinator';
        $('#Roles').val('Coordinator');
        $('#lblUserNameNotAvailable').hide();
        $('#lblUserNameAvailable').hide();
    };

    $('#userName').keyup(function() {
        newUserName = $(this).val();
        if (!newUserName) {
            $('#lblUserNameNotAvailable').hide();
            $('#lblUserNameAvailable').hide();
            $('#btnSubmit').attr("disabled", "disabled");
            return;
        }
        var url = window.urlRoot + '/Admin/IsMembershipUserNameDuplicate/?userName=' + newUserName;
        $.ajax({
            url: url,
            type: 'POST',
            data: {},
            success: function(data) {
                if (! data) {
                    $('#lblUserNameNotAvailable').hide();
                    $('#lblUserNameAvailable').show();
                    checkValidity();
                } else {
                    $('#lblUserNameNotAvailable').show();
                    $('#lblUserNameAvailable').hide();
                    $('#btnSubmit').attr("disabled", "disabled");  
                }
            }
        });
    });
    
    $('#Sites').change(function () {
        site = $(this).val();

        $('#SiteID').val(site);
        clearSelected();
        
        if (site !== "0") {
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

            getSiteEmployeeInfo();
        }
    });
    
    function getSiteEmployeeInfo() {
        $.ajax({
            url: window.urlRoot + '/Staff/GetSiteEmployeeInfo',
            type: 'POST',
            data: { site: site },
            success: function (data) {
                if (data.IsSuccessful) {
                    if (data.Stuff[0].Value === "true") {
                        $('#divEmpId').show();
                        $('#lblEmpIdMessage').text(data.Stuff[2].Value);
                    }
                    else {
                        $('#divEmpId').hide();
                        $('#empIDMessage').text("");
                    }
                }
            }
        });
    }

    $('#Users').change(function () {
        userId = $(this).val();
        clearSelected();
        if (userId === "0") {
            return;
        }

        setTimeout(
          function () {
              getSelectedUser();
          }, 500);


    });

    function getSelectedUser() {
        var url = window.urlRoot + '/Admin/GetStaffInfoForRoleChange/?userId=' + userId;
        curUserName = "";

        $.ajax({
            url: url,
            async: false,
            type: 'POST',
            data: {},
            success: function (data) {
                if (data) {
                    curRole = data.Role;
                    $('#lblCurrentRole').text(data.Role);
                    curUserName = data.UserName;
                    $('#lblCurrentUserName').text(data.UserName);
                    if (data.UserName) {
                        $('#spnUserName').hide();
                    } else {
                        $('#spnUserName').show();
                    }
                    $('#email').val(data.Email);
                    if (data.EmployeeID) {
                        $('#empId').val(data.EmployeeID);
                    }
                    $('#divSelected').slideDown();
                }
                else {
                    alert("No current user role");
                }
            }
        });
    }

    $('#Roles').change(function () {
        newRole = $(this).val();
        checkValidity();
    });

    function checkValidity() {
        if (newRole === curRole) {
            $('#btnSubmit').attr("disabled", "disabled");
            return false;
        }

        newUserName = $('#userName').val();
        if (newRole !== "Nurse") {
            if (!curUserName) {
                if (!newUserName) {
                    $('#btnSubmit').attr("disabled", "disabled");   
                }
            }
        }

        $('#btnSubmit').removeAttr("disabled");
        return true;
    }

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
        $('#btnSubmit').attr("disabled", "disabled");

        var url = window.urlRoot + '/Admin/ChangeUserRole';
        var email = $('#email').val();
        var empId = $('#empId').val();
        $.ajax({
            url: url,
            async: false,
            type: 'POST',
            data: {newUserName:newUserName, curUserName:curUserName, role:newRole, siteId:site, staffId:userId, empId:empId, email: email},
            success: function(data) {
                if (data.IsSuccessful) {
                    alert("Role change was successful");
                } else {
                    alert("Role change failed");
                }
            }
        });

    });
    
})