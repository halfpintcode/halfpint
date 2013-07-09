/// <reference path="jquery-1.7.1-vsdoc.js" />


$(function () {
    $('#btnDownload').attr('disabled', 'disabled');


    $('#Sites').change(function () {
        var selectedVal = $(this).val();
        if (selectedVal === 0) {
            $('#Files').empty();
            $('#Files').append("<option value='0'>Select file</option>");
            return;
        }
        var selectedSite = $('#Sites option:selected').text();
        var url = window.urlRoot + '/Admin/GetSavedSensorInfoSiteChange/' + selectedSite;

        $.post(url + '',
            { site: selectedSite },
            function (data) {
                $('#Files').empty();
                if (!data.length) {
                    $('#Files').append("<option value=''>No files found</option>");
                    $('#btnDownload').attr('disabled', 'disabled');
                } else {
                    $('#btnDownload').attr('disabled', false);
                    $.each(data, function (index, d) {
                        $('#Files').append("<option value='" + d + "'>" + d + "</option>");
                    });
                }
            });

    });

    $('#btnDownload').click(function () {
        var selectedSite = $('#Sites option:selected').text();
        var selectedFile = $('#Files').val();

        var url = window.urlRoot + '/Admin/GetSavedSensorInfoDownload/?fileName=' + selectedFile + '&site=' + selectedSite;
        window.location.href = url;
    });

})