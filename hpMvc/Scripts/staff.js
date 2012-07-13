/// <reference path="jquery-1.7.1-vsdoc.js" />
//var $j = jQuery.noConflict();
$(document).ready(function () {
    $('ul.sf-menu').superfish({
        delay: 1000,
        animation: { opacity: 'show', height: 'show' },
        dropShadows: true
    });

    var roles = $('#myRole').text();
    var userSite = $('#userSite').text();
    if (roles.indexOf('Nurse') > -1) {
        $('.StaffOnly').hide();
        $('.InvestigatorOnly').hide();
        $('.AdminDCC').hide();
        $('.AdminDCCInvestigator').hide();
    }
    if (roles.indexOf('Coord') > -1) {
        $('.InvestigatorOnly').hide();
        $('.AdminDCC').hide();
        $('.AdminDCCInvestigator').hide();
    }
    if (roles.indexOf('Investigator') > -1) {
        $('.AdminDCC').hide();
    }
    if (roles.indexOf('DCC') > -1) {
        $('.NotDcc').hide();
    }

    if (userSite !== "01:CHB") {
        $('.CHBOnly').hide();
    }
    if (userSite == "01:CHB") {
        $('.CHBHide').hide();
    }



    //this is one way to filter file downloads
    $('li .StaffOnly').click(function (e) {
        //        e.preventDefault();
        //        alert('staff only')
    });
});

