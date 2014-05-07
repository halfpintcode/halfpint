/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function () {
    var siteId = '';
    $('#runType').val('0');
    $('#report').hide();

    $("#accordion").accordion({
        activate: function (event, ui) {
            if (ui.newHeader.context.outerText == "Execute as service") {
                $('#runType').val(0);
            } 
            else {
                $('#runType').val(1);    
            }
            //$('#runType').val($("#accordion").accordion("option", "active"));
        }
    });

    $('#Sites').change(function () {
        $('#siteId').val($('#Sites').val());
        siteId = $('#siteId').val();
    });

    $('#btnSubmit').click(function () {

        if ($('#runType').val() === '1') {
            if (siteId == '') {
                alert('Select a site!');
                return false;
            }
        }
        $('#btnSubmit').attr('disabled', 'disabled');

        $("#spinner").ajaxStart(function () { $(this).show(); })
			   .ajaxStop(function () { $(this).hide(); });

        var url = window.urlRoot + '/Admin/PostTestsService';
        var data = $("form").serialize();

        $.ajax({
            url: url,
            //async: false,
            type: 'POST',
            data: data,
            success: function (data1) {
                if (data1 == 'Successful') {
                    alert('The serviece was run sucessfully!');
                    url = window.urlRoot + '/Admin/Index';
                    window.location = url;
                }
                else {
                    //report
                    $('#report').slideUp();
                    $('#report').empty();

                    $('<h3>' + data1.Name + ' - Post Tests Report</h3>').appendTo('#report');
                    $('</br>').appendTo('#report');

                    $("<h3> Total Staff: " + data1.PostTestNextDues.length + "</h3>").appendTo('#report');
                    $("<h3> Total Staff Post Tests Completed:" + data1.StaffCompleted + "</h3>").appendTo('#report');

                    var percent = 0;
                    if (data1.PostTestNextDues.length > 0)
                        percent = data1.StaffCompleted * 100 / data1.PostTestNextDues.length;
                    $("<h3> Total % Staff Post Tests Completed:" + percent.toFixed(0) + "%</h3>").appendTo('#report');

                    $('</br>').appendTo('#report');
                    var emailLists = data1.SiteEmailLists;
                    if (emailLists.CompetencyMissingList.length == 0) {
                        $('<h3>All staff members have completed their competency tests.</h3>').appendTo('#report');
                    } else {
                        //compentency
                        $('<h3>The following staff members have not completed a competency test.</h3>').appendTo('#report');
                        $("<table id='tblCompentency' style='border-collapse:collapse;' cellpadding='5' border='1'></table>").appendTo('#report');
                        $("<tr style='background-color:87CEEB'><th>Name</th><th>Role</th><th>Tests Not Completed</th><th>Email</th></tr>").appendTo('#tblCompentency');
                        $.each(emailLists.CompetencyMissingList, function (index, val) {
                            var email = "not entered";
                            if (val.Email != null)
                                email = val.Email;

                            var test = "";
                            
                            switch(siteId) {
                                case "1":
                                case "2":
                                case "9":
                                case "15":
                                case "33":
                                    if (val.Role == "Nurse") {
                                        if (!val.IsVampTested) {
                                            if (test.Length > 0)
                                                test += " and ";
                                            test += "Vamp Jr";
                                        }
                                    }
                                    break;
                                case "16":
                                case "22":
                                    break;
                                case "17":
                                case "23":
                                case "35":
                                    if (!val.IsNovaStatStripTested)
                                        test = "NovaStatStrip ";
                                    break;
                                default:
                                    if (!val.IsNovaStatStripTested)
                                        test = "NovaStatStrip ";
                                    if (val.Role == "Nurse") {
                                        if (!val.IsVampTested) {
                                            if (test.Length > 0)
                                                test += " and ";
                                            test += "Vamp Jr";
                                        }
                                    }
                                    break;
                            }
//                            if (!val.IsNovaStatStripTested)
//                                test = "NovaStatStrip ";
//                            if (val.Role === "Nurse") {
//                                if (!val.IsVampTested) {
//                                    if (test.Length > 0)
//                                        test += " and ";
//                                    test += "Vamp Jr";
//                                }
//                            }
                            $("<tr><td>" + val.Name + "</td><td>" + val.Role + "</td><td>" + test + "</td><td>" + email + "</td></tr>").appendTo('#tblCompentency');
                        });
                        $('</br>').appendTo('#report');
                    }

                    //email missing
                    if (emailLists.EmailMissingList.length > 0) {
                        $("<h3>The following staff members need to have their email address entered into the staff table.</h3>").appendTo('#report');
                        $("<table id='tblEmails' style='border-collapse:collapse;' cellpadding='5' border='1'></table>").appendTo('#report');
                        $("<tr style='background-color:87CEEB'><th>Name</th><th>Role</th></tr>").appendTo('#tblEmails');
                        $.each(emailLists.EmailMissingList, function (index, val) {
                            $("<tr><td>" + val.Name + "</td><td>" + val.Role + "</td></tr>").appendTo('#tblEmails');
                        });
                        $('</br>').appendTo('#report');
                    }

                    //employee id missing
                    if (emailLists.EmployeeIdMissingList.length > 0) {
                        $("<h3>The following staff members need to have their employee ID entered into the staff table.</h3>").appendTo('#report');
                        $("<table id='tblEmpIds' style='border-collapse:collapse;' cellpadding='5' border='1'></table>").appendTo('#report');
                        $("<tr style='background-color:87CEEB'><th>Name</th><th>Role</th><th>Email</th></tr>").appendTo('#tblEmpIds');
                        $.each(emailLists.EmployeeIdMissingList, function (index, val) {
                            var email = "not entered";
                            if (val.Email != null)
                                email = val.Email;

                            $("<tr><td>" + val.Name + "</td><td>" + val.Role + "</td><td>" + email + "</td></tr>").appendTo('#tblEmpIds');
                        });
                        $('</br>').appendTo('#report');
                    }

                    //new staff not completed
                    if (emailLists.NewStaffList.length > 0) {
                        $("<h3>The following new staff members have not completed their annual post tests.</h3>").appendTo('#report');
                        $("<table id='tblNewstaff' style='border-collapse:collapse;' cellpadding='5' border='1'></table>").appendTo('#report');
                        $("<tr style='background-color:87CEEB'><th>Name</th><th>Role</th><th>Email</th></tr>").appendTo('#tblNewstaff');
                        $.each(emailLists.NewStaffList, function (index, val) {
                            $("<tr><td>" + val.Name + "</td><td>" + val.Role + "</td><td>" + val.Email + "</td></tr>").appendTo('#tblNewstaff');
                        });
                        $('</br>').appendTo('#report');
                    }

                    //expired staff not completed
                    if (emailLists.ExpiredList.length > 0) {
                        $("<h3>The following expired staff members have not completed their annual post tests.</h3>").appendTo('#report');
                        $("<table id='tblExpired' style='border-collapse:collapse;' cellpadding='5' border='1'></table>").appendTo('#report');
                        $("<tr style='background-color:87CEEB'><th>Name</th><th>Role</th><th>Due Date</th><th>Email</th></tr>").appendTo('#tblExpired');
                        $.each(emailLists.ExpiredList, function (index, val) {
                            $("<tr><td>" + val.Name + "</td><td>" + val.Role + "</td><td>" + val.SNextDueDate + "</td><td>" + val.Email + "</td></tr>").appendTo('#tblExpired');
                        });
                        $('</br>').appendTo('#report');
                    }

                    //staff due not completed
                    if (emailLists.DueList.length > 0) {
                        $("<h3>The following staff members are due to take their annual post tests.</h3>").appendTo('#report');
                        $("<table id='tblDue' style='border-collapse:collapse;' cellpadding='5' border='1'></table>").appendTo('#report');
                        $("<tr style='background-color:87CEEB'><th>Name</th><th>Role</th><th>Due Date</th><th>Email</th></tr>").appendTo('#tblDue');
                        $.each(emailLists.DueList, function (index, val) {
                            $("<tr><td>" + val.Name + "</td><td>" + val.Role + "</td><td>" + val.SNextDueDate + "</td><td>" + val.Email + "</td></tr>").appendTo('#tblDue');
                        });
                        $('</br>').appendTo('#report');
                    }

                    //staff due not completed
                    if (emailLists.StaffTestsNotCompletedList.length > 0) {
                        $("<h3>The following staff members have not completed all post tests.</h3>").appendTo('#report');
                        $("<table id='tblNc' style='border-collapse:collapse;' cellpadding='5' border='1'></table>").appendTo('#report');
                        $("<tr style='background-color:87CEEB'><th>Name</th><th>Role</th><th>Email</th><th>Tests Not Completed</th></tr>").appendTo('#tblNc');
                        $.each(emailLists.StaffTestsNotCompletedList, function (index, val) {
                            if (val.TestsNotCompleted.length > 0) {
                                var email = "not entered";
                                if (val.Email != null)
                                    email = val.Email;

                                var row = "<tr><td>" + val.StaffName + "</td><td>" + val.Role + "</td><td>" + email + "</td><td>"; // ).appendTo('#tblNc');
                                $.each(val.TestsNotCompleted, function (index2, val2) {
                                    row = row + val2 + "</br>";
                                });
                                row = row + "</td></tr>";
                                $(row).appendTo('#tblNc');
                            }

                        });
                        $('</br>').appendTo('#report');
                    }


                    $('#btnSubmit').attr('disabled', false);
                    $('#report').slideDown();
                }
            }
        });
    });
});