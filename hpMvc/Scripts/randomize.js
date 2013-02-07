/// <reference path="jquery-1.7-vsdoc.js" />

$(document).ready(function () {
    var sensorType = $('#sensorType').val();

    $("#spinner").ajaxStart(function () { $(this).show(); })
			   .ajaxStop(function () { $(this).hide(); });
    
    $('#startDownloadDialog').dialog({ autoOpen: false });
    $('#downloadDialog').dialog({ autoOpen: false });
    $('#altDownload').hide();
    $('#divMain').hide();
    $('.mod').hide();
    $('#divInfo').show();

    $('ul.sf-menu').superfish({

    });

    $('#MonitorDate').datepicker({
        beforeShow: function (input, inst) {
            $.datepicker._pos = $.datepicker._findPos(input); //this is the default position 
            var pos = $(this).position();
            $.datepicker._pos[0] = pos.left + 100; //left              
        }
    });

    $('#ExpirationDate').datepicker({
        beforeShow: function (input, inst) {
            $.datepicker._pos = $.datepicker._findPos(input); //this is the default position 
            var pos = $(this).position();
            $.datepicker._pos[0] = pos.left + 100; //left              
        }
    });

    $('#MonitorTime').mask("99:99");
    $('#studyID').mask("99-9999-9");
    $('#BodyWeight').keydown(function (event) {
        numericsAndDecimalOnly(event, $(this).val());
    });

    $('#btnLogin').click(function () {
        var studyID = $('#studyID').val();
        var siteID = $('#userSite').text().substr(0, 2);
        if (siteID !== studyID.substr(0, 2)) {
            alert('The study id for your site must begin with ' + siteID);
            return;
        }

        var passwordVal = $('#password').val();
        var url = urlRoot + '/InitializeSubject/ValidateLogin/' + studyID;

        $.ajax({
            url: url,
            type: 'POST',
            data: { password: passwordVal },
            success: function (data) {
                if (data.IsSuccessful) {

                    $('#mainTitle').text('Initialize Study ID: ' + studyID);
                    $('#divLogin').slideUp();
                    $('#divMain').slideDown();
                    $('#lnkInfo').parent().addClass("sfHover");
                    if (sensorType == "0") {
                        $('#divSensor').hide();
                        $('#liSensor').hide();
                        $('.sensorInfo').hide();
                    }
                    else if (sensorType == "1") {
                        $('#divDexcomVids').hide();
                        $('#divMedtronicVids').show();
                    }
                    else if (sensorType == "2") {
                        $('#h3SensorTitle').text('Start Dexcom Sensor');
                        $('#lnkSensor').text('Start Dexcom Sensor');
                        $('#lblSensorDate').text('Receiver Date');
                        $('#lblSensorTime').text('Receiver Time');
                        $('#lblSensorId').text('Receiver Serial Number');
                        $('#lblSensorTransmitterId').text('Transmitter Number');
                        var srcUrl = urlRoot + "/Content/images/sensorDex.bmp";
                        $('#imgSensor').attr('src', srcUrl);
                        $('#divDexcomVids').show();
                        $('#divMedtronicVids').hide();
                    }
                }
                else {
                    if (data.ReturnValue === -1) {
                        window.location.href = urlRoot + '/Error/Index/';
                    }
                    alert(data.Message);
                }
            }
        });
    });


    $('.nextButton').click(function () {
        var id = this.id;
        $('.mod').hide();
        $('ul li').removeClass("sfHover");
        switch (id) {
            case "btnInfoNext":
                $('#lnkInstruc').click();
                $('#lnkInstruc').parent().addClass("sfHover");
                break;
            case "btnInstrucNext":
                if (sensorType == "0") {
                    $('#lnkParams').click();
                    $('#lnkParams').parent().addClass("sfHover");
                }
                else {
                    $('#lnkSensor').click();
                    $('#lnkSensor').parent().addClass("sfHover");
                }
                break;
            case "btnSensorNext":
                $('#lnkParams').click();
                $('#lnkParams').parent().addClass("sfHover");
                break;
            case "btnParamsNext":
                $('#lnkVamp').click();
                $('#lnkVamp').parent().addClass("sfHover");
                break;
            case "btnVampNext":
                $('#lnkCalGluc').click();
                $('#lnkCalGluc').parent().addClass("sfHover");
                break;
            case "btnCalGlucNext":
                $('#lnkRandomize').click();
                $('#lnkRandomize').parent().addClass("sfHover");
                break;

        }

    });

    $('#divSideBar ul li a').click(function (e) {
        var $id = this.id;
        $('.mod').hide();
        e.preventDefault();
        switch ($id) {
            case 'lnkInfo':
                $('#divInfo').show();
                break;
            case 'lnkInstruc':
                $('#divInstruc').show();
                break;
            case 'lnkSensor':
                $('#divSensor').show();
                break;
            case 'lnkParams':
                $('#divParams').show();
                break;
            case 'lnkVamp':
                $('#divVamp').show();
                break;
            case 'lnkCalGluc':
                $('#divCalGluc').show();
                break;
            case 'lnkRandomize':
                $('#divRandomize').show();
                break;
        }
    });

    $('#btnDownload').click(function () {
        var studyId = $('#studyID').val();
        window.location = urlRoot + '/InitializeSubject/InitializeSs/' + studyId;
        $('#startDownloadDialog').dialog('close');
    });
    
    $('#btnInitialize').click(function () {
        if (!validate()) {
            return;
        }

        $('#btnInitialize').attr('disabled', 'disabled');
        
        $('#divValidResults').empty();

        var studyId = $('#studyID').val();
        var url = urlRoot + '/InitializeSubject/Initialize/' + studyId;
        var data = $("form").serialize();
        $.ajax({
            url: url,
            type: 'POST',
            data: data,
            success: function (data1) {
                if (data1.IsSuccessful) {
                    //window.location = urlRoot + '/InitializeSubject/InitializeSS/' + studyID;
                    //                    setTimeout(function () {
                    //                        window.location = urlRoot
                    //                    }, 30000);

                    $('#startDownloadDialog').dialog(
                        {
                            title: 'Download CHECKS',
                            height: 300,
                            width: 600,
                            show: 'blind',
                            close: downloadDialogClose,
                            hide: 'explode'
                        });
                    $('#startDownloadDialog').dialog('open');
                    $('#altDownload').show();
                }
                else {
                    var message = "<p>Summary of invalid entries</p><hr/>";
                    $.each(data1.ValidMessages, function (index, d) {
                        message = message +
                                  "<p class='validError'>" + "*" + d.DisplayName + " " + d.Message + "</p>"

                    });
                    alert(data1.Message);
                    $('#divValidResults').append(message).slideDown();
                }
            }
        });
    });

    function downloadDialogClose() {
        $('#downloadDialog').dialog(
        {
            title: 'Processing - Please Wait',
            height: 150,
            width: 450,
            show: 'blind',
            hide: 'explode'
        });
        $('#downloadDialog').dialog('open');

        setTimeout(destroyDialog, 5000);
    }

    function destroyDialog() {
        $('#downloadDialog').dialog("destroy");
    }

    $('#alnkAltDownload').click(function (e) {
        e.preventDefault();
        var studyID = $('#studyID').val();
        var url = urlRoot + '/InitializeSubject/AlternateSSDownload/' + studyID;
        window.location.href = url;
    });

    function validate() {
        if (sensorType === "1" || sensorType === "2") {
            var val = $('#MonitorDate').val();
            if (!val) {
                alert('Monitor Date is required');
                $('.mod').hide();
                $('#divSensor').show();
                $('#MonitorDate').focus();
                return false;
            }
            val = $('#MonitorTime').val();
            if (!val) {
                alert('Monitor Time is required');
                $('.mod').hide();
                $('#divSensor').show();
                $('#MonitorTime').focus();
                return false;
            }
            val = $('#MonitorID').val();
            if (!val) {
                alert('Monitor ID is required');
                $('.mod').hide();
                $('#divSensor').show();
                $('#MonitorID').focus();
                return false;
            }
            val = $('#TransmitterID').val();
            if (!val) {
                alert('Transmitter ID is required');
                $('.mod').hide();
                $('#divSensor').show();
                $('#TransmitterID').focus();
                return false;
            }
            val = $('#SensorLot').val();
            if (!val) {
                alert('Sensor Lot is required');
                $('.mod').hide();
                $('#divSensor').show();
                $('#SensorLot').focus();
                return false;
            }
            val = $('#ExpirationDate').val();
            if (!val) {
                alert('Expiration date is required');
                $('.mod').hide();
                $('#divSensor').show();
                $('#ExpirationDate').focus();
                return false;
            }

            val = $('#InserterFirstName').val();
            if (!val) {
                alert('Inserter First Name is required');
                $('.mod').hide();
                $('#divSensor').show();
                $('#InserterFirstName').focus();
                return false;
            }
            val = $('#InserterLastName').val();
            if (!val) {
                alert('Inserter Last Name is required');
                $('.mod').hide();
                $('#divSensor').show();
                $('#InserterLastName').focus();
                return false;
            }
            val = $('#SensorLocations').val();
            if (val === '0') {
                alert('Sensor Location is required');
                $('.mod').hide();
                $('#divSensor').show();
                $('#SensorLocations').focus();
                return false;
            }
        }
        val = $('#BodyWeight').val();
        if (!val) {
            alert('Body Weight is required');
            $('.mod').hide();
            $('#divParams').show();
            $('#BodyWeight').focus();
            return false;
        }
        if (val < 2 || val > 180) {
            alert('Body Weight must be between 2 and 180 (kg)');
            $('.mod').hide();
            $('#divParams').show();
            $('#BodyWeight').focus();
            return false;
        }
        val = $('#Concentrations').val();
        if (val === '') {
            alert('Insulin Concentration is required');
            $('.mod').hide();
            $('#divParams').show();
            $('#Concentrations').focus();
            return false;
        }
        if (!($('#OrderIns').is(':checked'))) {
            alert('Ordering insulin is required');
            $('.mod').hide();
            $('#divParams').show();
            $('#OrderIns').focus();
            return false;
        }
        if (!($('#chkVamp').is(':checked'))) {
            alert('Placing the Vamp Jr is required');
            $('.mod').hide();
            $('#divParams').show();
            $('#chkVamp').focus();
            return false;
        }
        if (!($('#chkCalGluc').is(':checked'))) {
            alert('Running a high and low QC on the Nova StatStrip glucose meter is required');
            $('.mod').hide();
            $('#divParams').show();
            $('#chkCalGluc').focus();
            return false;
        }
        return true;
    }

});

