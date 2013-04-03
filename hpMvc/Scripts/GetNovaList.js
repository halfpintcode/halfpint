/// <reference path="jquery-1.7.1-vsdoc.js" />


$(function () {
    $('#btnDownload').attr('disabled', 'disabled');

    $('#Files').change(function() {
        var selectedVal = $(this).val();
        if (selectedVal == "Select file") {
            $('#btnDownload').attr('disabled', 'disabled');
        } else {
            $('#btnDownload').attr('disabled', false);
        }
    });

    $('#btnDownload').click(function () {
        var siteCode = $('#siteCode').val();
        var selectedFile = $('#Files').val();

        var url = window.urlRoot + '/Coordinator/GetNovaListDownload/?fileName=' + selectedFile + '&siteCode=' + siteCode;
        window.location.href = url;
    });

})