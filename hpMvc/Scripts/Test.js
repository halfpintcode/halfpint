/// <reference path="jquery-1.7-vsdoc.js" />

$(function () {
    $('#quick').menu();
});

function testOK(data) {
    alert(data.UserName);
};

function testFailure(data) {
    alert('you failed');
};