$(function() {
    
    $('#btnReport').click(function() {
        var siteVal = $('#Sites').val();

        $("#spinner").ajaxStart(function () { $(this).show(); })
			   .ajaxStop(function () { $(this).hide(); });

        var url = window.urlRoot + '/Admin/RandomizedChecksRowsCompleted/';
        $.post(url,
            {site:siteVal},
            function (data) {
                alert(data);
            });
    });
});