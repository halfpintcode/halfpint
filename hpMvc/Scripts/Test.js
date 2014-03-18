/// <reference path="jquery-1.7-vsdoc.js" />

$(function () {
    var date = new Date();
    var sDate = date.getMonth() + 1 + '/' + date.getDate() + '/' + date.getFullYear();
    var sTime = date.getHours() + ':' + date.getMinutes() + ':' + date.getSeconds();
    var vtoken = $("input[name=__RequestVerificationToken]").val();

    $('#date').val(sDate);
    $('#time').val(sTime);

    
    
    $('#btnSubmit').click(function() {
        $.ajax({
            url: window.urlRoot + '/Test/GetDate/',
            async: false,
            type: 'POST',
            data: { date: sDate, time: sTime, "__RequestVerificationToken": vtoken},
            success: function (rdata) {
                if (rdata == 1) {
                    alert('This date is ' + sDate + ' and the time is ' + sTime);
                };
            }
        });
    });
});

