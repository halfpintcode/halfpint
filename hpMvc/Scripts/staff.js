/// <reference path="jquery-1.7.1-vsdoc.js" />
//var $j = jQuery.noConflict();
$(document).ready(function () {

    var roles = $('#myRole').text();
    var userSite = $('#userSite').text();
    if (roles.indexOf('Nurse') > -1) {
        $('.StaffOnly').hide();
        $('.InvestigatorOnly').hide();
        $('.AdminDCC').hide();
        $('.AdminDCCInvestigator').hide();
    }
    if (roles.indexOf('Coord') > -1) {
        $('.NurseOnly').hide();
        $('.InvestigatorOnly').hide();
        $('.AdminDCC').hide();
        $('.AdminDCCInvestigator').hide();
    }
    if (roles.indexOf('Investigator') > -1) {
        $('.AdminDCC').hide();
        $('.NurseOnly').hide();
    }
    if (roles.indexOf('DCC') > -1) {
        $('.NotDcc').hide();
        $('.NurseOnly').hide();
    }
    if (roles.indexOf('Admin') === -1) {
        $('.AdminOnly').hide();
    }
    if (userSite !== "01:CHB") {
        $('.CHBOnly').hide();
    }
    if (userSite == "01:CHB") {
        $('.CHBHide').hide();
    }
});

