$(function () {
    $('.btnReset').click(function () {
        var tr = $(this).parents('tr:first');
        var userName = tr.find(".userName").text();
        var passWord = tr.find(".userPassword").text();
        //console.log(userName + ',' + passWord);
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
})