/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function () {
    $("option").bind("dblclick", function () {
        var source = $(this).parent().attr("id")
        switch (source) {
            case "users":
                $('#addUser').click();
                break;
            case "selectedUsers":
                $('#removeUser').click();
                break;
            case "sites":
                $('#addSite').click();
                break;
            case "selectedSites":
                $('#removeSite').click();
                break;
            case "roles":
                $('#addRole').click();
                break;
            case "selectedRoles":
                $('#removeRole').click();
                break;
        }
    });

    $("#addSite,#removeSite").click(function (event) {

        var id = $(event.target).attr("id");
        var selectFrom = id == "addSite" ? "#sites" : "#selectedSites";
        var moveTo = id == "addSite" ? "#selectedSites" : "#sites";

        var selectedItems = $(selectFrom + " :selected").toArray();

        $(moveTo).append(selectedItems);
        selectedItems.remove;        
    });
    $("#addRole,#removeRole").click(function (event) {

        var id = $(event.target).attr("id");
        var selectFrom = id == "addRole" ? "#roles" : "#selectedRoles";
        var moveTo = id == "addRole" ? "#selectedRoles" : "#roles";

        var selectedItems = $(selectFrom + " :selected").toArray();

        $(moveTo).append(selectedItems);
        selectedItems.remove;
    });
    $("#addUser,#removeUser").click(function (event) {

        var id = $(event.target).attr("id");
        var selectFrom = id == "addUser" ? "#users" : "#selectedUsers";
        var moveTo = id == "addUser" ? "#selectedUsers" : "#users";

        var selectedItems = $(selectFrom + " :selected").toArray();

        $(moveTo).append(selectedItems);
        selectedItems.remove;
    });

    $('#btnSend').click(function () {

        //$(this).attr('disabled', 'disabled');
        var cri = {};
        cri.sites = getSites();
        cri.roles = getRoles();
        cri.users = getUsers();
        cri.subject = $('#subject').val();
        cri.body = $('#message').val();
        if (cri.sites.length === 0 && cri.roles.length === 0 && cri.users.length === 0) {
            alert('You must select at least one item from sites, roles or users');
            return;
        }
        var jnv = JSON.stringify(cri);

        $.ajax({
            url: urlRoot + '/Admin/BroadcastSend',
            type: 'POST',
            dataType: 'json',
            data: jnv,
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                alert(data);
                //                if (data.ReturnValue === 0) {
                //                    alert(data.Message);
                //                }
                //                else {
                //                    alert(data.Message);
                //                    if (isEdit) {
                //                        window.location = urlRoot + '/Staff/Index';
                //                        return;
                //                    }
                //                    $('#divSubmit').hide();
                //                    $('#divNewEntry').show();
                //                }

            }
        });
    });
});

function getSites() {
    var arrSites = [];

    $("#selectedSites > option").each(function (index) {
        //        arrSites[index] = {
        //            ID: $(this).val(),
        //            Name: $(this).text()
        //        }
        arrSites[index] = $(this).val();
    });

    return arrSites;
}

function getRoles() {
    var arrRoles = [];

    $("#selectedRoles > option").each(function (index) {
        arrRoles[index] = $(this).text();
    });

    return arrRoles;
}

function getUsers() {
    var arrUsers = [];

    $("#selectedUsers > option").each(function (index) {
        arrUsers[index] = $(this).val();
                
    });

    return arrUsers;
}