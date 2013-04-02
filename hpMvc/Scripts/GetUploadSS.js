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
        var url = window.urlRoot + '/Admin/GetSavedChecksSiteChange/' + selectedVal;

        $.post(url + '',
            { site: selectedVal },
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
        var selectedSite = $('#Sites').val();
        var selectedFile = $('#Files').val();

        var url = window.urlRoot + '/Admin/GetSavedChecksDownload/?fileName=' + selectedFile + '&site=' + selectedSite;
        window.location.href = url;
    });

})