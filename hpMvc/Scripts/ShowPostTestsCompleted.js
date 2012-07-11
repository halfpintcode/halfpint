/// <reference path="jquery-1.7.1-vsdoc.js" />
/// <reference path="TableTools.js" />

$(function () {
    $("tr:odd").addClass("odd");

    TableTools.DEFAULTS.aButtons = ["print"];
    
    $("#ptTable").dataTable({
        "sDom": 'T<"clear">lfrtip'        
    }).rowGrouping();

})