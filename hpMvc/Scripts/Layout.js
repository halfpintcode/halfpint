﻿/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function () {
    var role = $('#myRole').text();
    role = role.replace('My role: ', '');

    if (role === 'DCC' || role === 'Admin' || role === 'Coordinator') {
        if (role === 'DCC') {
            $('#adminLink').hide();
        }
        if (role === 'Coordinator') {
            $('#adminLink').hide();
            $('#dccLink').hide();
        }
    }    
    else {
        $('#adminLink').hide();
        $('#dccLink').hide();
        $('#coordLink').hide();
    }

    $.fn.center = function () {
        this.css({
            'position': 'fixed',
            'left': '50%',
            'top': '50%'
        });

        this.css({
            'margin-left': -this.width() / 2 + 'px',
            'margin-top': -this.height() / 2 + 'px'
        });

        return this;
    }

    $('#divUserMenu').dialog({ autoOpen: false });

    $('#userStuff').click(function () {

        if (role === "Nurse") {
            return;
        }

        $('#divUserMenu').dialog(
        {
            position: [$('#userStuff').position().left, $('#userStuff').position().top + 23],
            width: 160,
            title: 'User Profile',
            height: 65
        });
        $('#divUserMenu').dialog('open');
    });

    $('#aMyProfile').click(function (e) {
        e.preventDefault();
        window.location = urlRoot + '/Account/UserProfile';
    });

    $('#ulMainMenu').children().removeClass('current_page_item');
    var path = this.location.pathname;
    var target = "";
    if (path.indexOf('Staff') > -1 || path.indexOf('CalorieCalc') > -1 || path.indexOf('InitializeSubject') > -1 || path.indexOf('PostTests') > -1) {
        target = 'Staff';
    }
    if (path.indexOf('Families') > -1) {
        target = 'Families';
    }
    if (path.indexOf('Admin') > -1) { 
        target = 'Admin';
    }
    if (path.indexOf('Coordinator') > -1) { 
        target = 'Coordinator';
    }
    if (path.indexOf('Dcc') > -1) { 
        target = 'DCC';
    }
    if (path.indexOf('Account') > -1) {
        return;
    }

    if (target === '') {
        $('#ulMainMenu li').first().addClass('current_page_item');
    }
    else {

        $('#ulMainMenu li').each(function () {
            if ($(this).text() === target) {
                $(this).addClass('current_page_item');
            }
        });
    }

    $('#imgNih').click(function () {
        window.open("http://www.nih.gov");
    });

    $('#imgnhlbi').click(function () {
        window.open("http://www.nhlbi.nih.gov");
    });
})


var urlRoot = GetUrlRoot();

function GetUrlRoot() {
    var firstSlash = 0;
    var secondSlash = 0;
    firstSlash = location.href.indexOf("/", 9);
    secondSlash = location.href.indexOf("/", firstSlash + 1);
    return location.href.substr(0, secondSlash);
}

function numericsOnly(event) {
    // Allow: backspace, delete, tab and escape 
    if (event.keyCode == 46 || event.keyCode == 8 || event.keyCode == 9 || event.keyCode == 27 ||
    // Allow: Ctrl+A 
        (event.keyCode == 65 && event.ctrlKey === true) ||
    // Allow: home, end, left, right 
        (event.keyCode >= 35 && event.keyCode <= 39)) {
        // let it happen, don't do anything 
        return;
    }
    else {
        // Ensure that it is a number and stop the keypress 
        if ((event.keyCode < 48 || event.keyCode > 57) && (event.keyCode < 96 || event.keyCode > 105)) {
            event.preventDefault();
        }
    }
}
function numericsAndDecimalOnly(event, val) {
    // Allow: backspace, delete, tab and escape 
    if (event.keyCode == 46 || event.keyCode == 8 || event.keyCode == 9 || event.keyCode == 27 ||
    // Allow: Ctrl+A 
        (event.keyCode == 65 && event.ctrlKey === true) ||
    // Allow: home, end, left, right 
        (event.keyCode >= 35 && event.keyCode <= 39)) {
        // let it happen, don't do anything 
        return;
    }
    else {
        //if its a decimal there can be only one
        if ((event.keyCode === 110) || (event.keyCode === 190)) {
            if (val.indexOf('.') === -1) {
                return
            }
        }

        // Ensure that it is a number and stop the keypress 
        if ((event.keyCode < 48 || event.keyCode > 57) && (event.keyCode < 96 || event.keyCode > 105)) {
            event.preventDefault();
        }
    }
}

function validateEmail($email) {
    var emailReg = /^([\w-\.]+@([\w-]+\.)+[\w-]{2,4})?$/;
    return emailReg.test($email); 
}

function validateEmployeeID(reg, empID) {
    //var chbReg = /^\d{6}$/
    var re = new RegExp(reg); 
    return re.test(empID);
}