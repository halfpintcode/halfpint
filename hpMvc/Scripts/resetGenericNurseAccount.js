$('#btnReset').click(function () {
    var userName = $("#userName").text();
    var passWord = $("#userPassword").text();
    
    var url = window.urlRoot + '/Coordinator/ResetSitePassword';
    var nurseUser = {
        UserName: userName,
        UserPassword: passWord
    };

    $.ajax({
        url: url,
        type: 'POST',
        data: nurseUser,
        success: function (data) {
            if (data) {
                alert(data);
            } else {
                alert(data);
            }
        }
    });

});