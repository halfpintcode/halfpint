$(function() {
    $(function () {
        //initialize with chb
        getData(1);

        $('#Sites').change(function () {
            getData($(this).val());
        });

        function getData(site) {
            $("#spinner").ajaxStart(function () { $(this).show(); })
			   .ajaxStop(function () { $(this).hide(); });
            var url = window.urlRoot + '/Admin/GetRadomizedChecksCompletedRows/';

            $.getJSON(url,
                { siteID: site },
                function (result) {
                    var grid = $('#grid');
                    grid.empty();
                    grid.append(result.Data);
                });

        }
    });
    
});