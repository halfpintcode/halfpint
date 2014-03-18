﻿/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function () {
    $('#btnSave').click(function () {

        var url = window.urlRoot + '/Inform/Save';
        var data = $("form").serialize();
        $.ajax({
            url: url,
            type: 'POST',
            data: data,
            success: function (rdata) {
                if (rdata.ReturnValue === 1) {
                    alert('InForm page saved successfully!');
                }
                else {
                    alert(rdata.Message);
                }
            }

        });

    });
});