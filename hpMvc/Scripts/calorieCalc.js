/// <reference path="jquery-1.7.1-vsdoc.js" />
$(function () {
	var isEdit = false;
	var resultsTotal = 0;
	var infusionTotal = 0;
	var pnCho = 0;
	var pnProtein = 0;
	var pnLipid = 0;
	var enCho = 0;
	var enProtein = 0;
	var enLipid = 0;
	var tempID = 0;

	//$("form").submit(function () { return false; }); 
	$("#spinner").ajaxStart(function () { $(this).show(); })
			   .ajaxStop(function () { $(this).hide(); });

	//#region initialize
	$('#tabs').tabs();

	$('#bodyWeight').keydown(function (event) {
		numericsAndDecimalOnly(event, $(this).val());
	});

	$('.keyDecimal').keydown(function (event) {
		numericsAndDecimalOnly(event, $(this).val());
	});

	$('.infusions1, .infusions2, .infusions3, .infusions4').attr("disabled", "disabled");

	$('#calcDate').datepicker();

	$('#divNewEntry').hide();

	$('#divNewFormula').dialog({
		autoOpen: false,
		width: 800,
		modal: true,
		title: 'Add New Formula'
	});

	$('#divNewAdditive').dialog({
		autoOpen: false,
		width: 800,
		modal: true,
		title: 'Add New Additive'
	});

	$.ajax({
		url: urlRoot + '/CalorieCalc/GetFormulaData',
		type: 'POST',
		data: {},
		success: function (rdata) {
			$.each(rdata, function (index, val) {
				var id = val.ID;
				var option = $("#FormulaList option[value='" + id + "']");
				option.data('formula', { protein: val.ProteinKcal,
					cho: val.ChoKcal,
					lipid: val.LipidKcal,
					kCalMl: val.Kcal_ml
				});
			});
		}
	});

	$.ajax({
		url: urlRoot + '/CalorieCalc/GetAdditiveData',
		type: 'POST',
		data: {},
		success: function (rdata) {
			$.each(rdata, function (index, val) {
				var id = val.ID;
				var option = $("#AdditiveList option[value='" + id + "']");
				option.data('additive', { protein: val.ProteinKcal,
					cho: val.ChoKcal,
					lipid: val.LipidKcal,
					kCalUnit: val.Kcal_unit,
					unit: val.Unit,
					unitName: val.UnitName
				});
			});
		}
	});

	$('#StudyList').change(function () {
		var studyID = $(this).val();
		//get the weight if available
		if (studyID > 0) {
			$.ajax({
				url: urlRoot + '/CalorieCalc/GetCalctWeight/',
				type: 'POST',
				data: { studyID: studyID },
				success: function (rdata) {
					if (rdata > 0) {
						$('#bodyWeight').val(rdata);
					}
				}
			});
		}
	});

	//change the unit to match the selected additive
	$("#AdditiveList").change(function () {
		var option = $("#AdditiveList option:selected");
		var data = option.data('additive');
		$('#spanUnit').text(data.unitName);
	});

	//dialogs
	$('#btnAddManual').click(function () {
		$('#divNewFormula').dialog('open');
	});

	$('#btnAdditiveManual').click(function () {
		$('#divNewAdditive').dialog('open');
	});

	$('#btnNewDay').click(function () {
		clearAll();
		$('#calcDate').val("");
		$('#calcDate').focus();
		$(window).scrollTop(0);
		$('#divSubmit').show();
		$('#divNewEntry').hide();
	});

	$('#btnNewStudy').click(function () {
		clearAll();
		$('#calcDate').val("");
		$('#bodyWeight').val("");
		$('#StudyList').val("0");
		$('#StudyList').focus();
		$(window).scrollTop(0);
		$('#divSubmit').show();
		$('#divNewEntry').hide();
	});

	$('#btnCancel').click(function () {
		window.location = urlRoot + '/Staff/Index';
	});

	$('#btnClear').click(function () {
		if (confirm('Are you sure you want to clear all entries for ' + $('#calcDate').val())) {
			clearAll();
		}
	});

	function clearAll() {
		$('#DexCons1').val("0").change();
		$('#DexCons2').val("0").change();
		$('#DexCons3').val("0").change();
		$('#DexCons4').val("0").change();
		$('#selParenteral').empty();
		$('#selEnteral').empty();
		clearOther();

		$('.nTotal').text("0");
		resultsTotal = 0;
		infusionTotal = 0;
		pnCho = 0;
		pnProtein = 0;
		pnLipid = 0;
		enCho = 0;
		enProtein = 0;
		enLipid = 0;
		tempID = 0;
		recalculateResultsTotal();
	}

	function clearOther() {
		$('#breastFeeding').attr('checked', false);
		$('#solidFoods').attr('checked', false);
		$('#drinks').attr('checked', false);
		$('#other').attr('checked', false);
		$('#otherText').val('');
	}

	$('#btnNewDay').click(function () {

	});

	$('#DexCons1').val("");
	$('#DexCons1').val("0").change();
	$('#DexCons2').val("0").change();
	$('#DexCons3').val("0").change();
	$('#DexCons4').val("0").change();
	//#endregion


	//#region submit
	$('#btnSubmit').click(function () {
		if (!validateSubmit()) {
			return;
		}
		
		var CalStudyInfo = {
			ID: 0,
			StudyID: $('#StudyList').val(),
            SStudyID: $('#StudyList option:selected').text(),
			Weight: $('#bodyWeight').val(),
			CalcDate: $('#calcDate').val(),
			TotalCals: $('#totalCalIntake').text()
		};

		var InfusionData = getInfusionData();
		var ParenteralData = getParenteralData();
		var EnteralData = getEnteralData();
		var AdditiveData = getAdditiveData();
		var CalOtherNutrition = {
			ID: 0,
			CalStudyID: 0,
			BreastFeeding: $('#breastFeeding').is(":checked"),
			SolidFoods: $('#solidFoods').is(":checked"),
			Drinks: $('#drinks').is(":checked"),
			Other: $('#other').is(":checked"),
			OtherText: $('#otherText').val()
		}

		var cri = {};
		cri.calStudyID = $('#calStudyID').val();
		cri.cas = AdditiveData;
		cri.ces = EnteralData;
		cri.pis = ParenteralData;
		cri.cic = InfusionData;
		cri.csi = CalStudyInfo;
		cri.con = CalOtherNutrition;
		var jnv = JSON.stringify(cri);

		$.ajax({
			url: urlRoot + '/CalorieCalc/Save',
			type: 'POST',
			dataType: 'json',
			data: jnv,
			contentType: 'application/json; charset=utf-8',
			success: function (data) {
				if (data.ReturnValue === 0) {
					alert(data.Message);
				}
				else {
					alert(data.Message);
					if (isEdit) {
						window.location = urlRoot + '/Staff/Index';
						return;
					}
					$('#divSubmit').hide();
					$('#divNewEntry').show();
				}

			}
		});

	});

	function getAdditiveData() {
		var arrAdd = [];
		var formulaID = 0;
		var vol = 0;

		$("#selAdditive > option").each(function (index) {
			arrAdd[index] = {
				AdditiveID: $(this).val(),
				Volume: $(this).data('additive').volume
			}
		});

		return arrAdd;
	}

	function getEnteralData() {
		var arrEren = [];
		var formulaID = 0;
		var vol = 0;

		$("#selEnteral > option").each(function (index) {
			arrEren[index] = {
				FormulaID: $(this).val(),
				Volume: $(this).data('enteral').volume
			}


		});

		return arrEren;
	}

	function getParenteralData() {
		var arrParen = [];
		var dex = 0;
		var amn = 0;
		var lip = 0;
		var vol = 0;
		var text = "";
		var aText;
		var aaText;

		$("#selParenteral > option").each(function (index) {
			text = this.text;
			aText = text.split(',');
			dex = 0;
			amn = 0;
			lip = 0;
			vol = 0;
			$(aText).each(function (index, val) {
				aaText = val.split(":");
				switch ($.trim(aaText[0])) {
					case "Dextrose":
						dex = parseInt(aaText[1]);
						break;
					case "Amino Acid":
						amn = parseInt(aaText[1]);
						break;
					case "Lipid Concentration":
						lip = parseInt(aaText[1]);
						break;
					case "Volume":
						vol = parseInt(aaText[1]);
						break;
				}

				//alert(aaText[0]);
				//alert(parseInt(aaText[1]));
			});
			arrParen[index] = {
				DexPercent: dex,
				AminoPercent: amn,
				LipidPercent: lip,
				Volume: vol
			}
		});
		return arrParen
	}

	function getInfusionData() {
		var arrInfuse = [];

		var kCalMl = 0;
		var vol = 0;
		var icounter = 0;
		var jcounter = 0;

		for (var j = 1; j < 5; j++) {

			kCalMl = $('#DexCons' + j).val();
			if (kCalMl === "0") {
				continue;
			}

			var volumes = new Array();
			jcounter = 0;
			$('.infusions' + j).each(function (ndx, val) {
				vol = $(this).val();
				if (vol.length > 0) {
					volumes[jcounter] = vol;
					jcounter++;
				}

			});
			arrInfuse[icounter] = { DexValue: kCalMl, Volumes: volumes };
			icounter++;
		}

		return arrInfuse
	}

	function validateSubmit() {
		if ($('#StudyList').val() === "0") {
			alert('You must select a study id!');
			$('#StudyList').focus();
			return false;
		}
		var val = $('#bodyWeight').val();
		if (val.length === 0) {
			alert('Weight is required');
			$('#bodyWeight').focus();
			return false;
		}
		val = $('#calcDate').val();
		if (val.length === 0) {
			alert('Date is required');
			$('#calcDate').focus();
			return false;
		}
		return true;
	}
	//#endregion

	//#region edit initialize
	if ($('#calStudyID').val() > 0) {
		isEdit = true;
		$('#StudyList').attr("disabled", "disabled");
		$('#bodyWeight').attr("disabled", "disabled");
		$('#calcDate').attr("disabled", "disabled");

		var infuseData = { studyID: $('#StudyList').val(), calcDate: $('#calcDate').val() }
		var jinfuseData = JSON.stringify(infuseData);

		$.ajax({
			url: urlRoot + '/CalorieCalc/GetAllData',
			type: 'POST',
			dataType: 'json',
			contentType: 'application/json; charset=utf-8',
			data: jinfuseData,
			success: function (rdata) {
				//infusions
				$.each(rdata.calInfusionCol, function (index, val) {
					var idx = index + 1;
					//dextrose
					$('#DexCons' + idx).val(val.DexValue);
					$('#DexCons' + idx).change();

					//volumes
					$.each(val.Volumes, function (index2, val2) {
						$('.infusions' + idx).eq(index2).val(val2);

					});
					$('.infusions' + idx).change();
				});
				//otherNutrition
				if (rdata.calOtherNutrition.BreastFeeding) {
					$('#breastFeeding').attr('checked', 'checked');
				}
				if (rdata.calOtherNutrition.SolidFoods) {
					$('#solidFoods').attr('checked', 'checked');
				}
				if (rdata.calOtherNutrition.Drinks) {
					$('#drinks').attr('checked', 'checked');
				}
				if (rdata.calOtherNutrition.Other) {
					$('#other').attr('checked', 'checked');
				}
				$('#otherText').val(rdata.calOtherNutrition.OtherText);

				//parenterals
				var parDex = 0;
				var parAmi = 0;
				var parLip = 0;
				var parLipVal = 0;
				var parVol = 0;
				var optText = "";
				var option = "";
				var parProteinKcal = 0;
				var parChoKcal = 0;
				var parLipidKcal = 0;

				$.each(rdata.calParenterals, function (index3, val3) {
					parDex = val3.DexPercent;
					parAmi = val3.AminoPercent;
					parLip = val3.LipidPercent;
					parVol = val3.Volume;
					if (parDex > 0) {
						parProteinKcal = Math.round(parAmi * 0.04 * parVol)
						pnProtein = pnProtein + parProteinKcal;
						parChoKcal = Math.round(parDex * 0.034 * parVol);
						pnCho = pnCho + parChoKcal;
						optText = "Dextrose:" + parDex + "%, Amino Acid:" + parAmi + "%, Volume:" + parVol + "mL";
						tempID++;

						option = "<option value='" + tempID + "' >" + optText + "</option>";
						$('#selParenteral').append(option);

						option = $("#selParenteral option[value='" + tempID + "']");
						$(option).data('parenteral', { protein: parProteinKcal, cho: parChoKcal, lipid: 0 });
					}
					else { //lipid
						switch (parLip) {
							case 10:
								parLipVal = 1.1;
								break;
							case 20:
								parLipVal = 2.0;
								break;
							case 30:
								parLipVal = 3.0;
								break;
						}
						parLipidKcal = Math.round(parLipVal * parVol);
						pnLipid = pnLipid + parLipidKcal;
						optText = "Lipid Concentration:" + parLip + "%, Volume:" + parVol + "mL";
						tempID++;
						option = "<option value='" + tempID + "' >" + optText + "</option>";
						$('#selParenteral').append(option);

						option = $("#selParenteral option[value='" + tempID + "']");
						$(option).data('parenteral', { protein: 0, cho: 0, lipid: parLipidKcal });
					}
				});
				$.each(rdata.calEnterals, function (index, val) {
					var formulaID = val.FormulaID;
					var vol = val.Volume;

					var option = $("#FormulaList option[value='" + formulaID + "']");
					var optData = option.data('formula');

					var fprotein = parseFloat(optData.protein);
					var fcho = parseFloat(optData.cho);
					var flipid = parseFloat(optData.lipid);
					var fkCalMl = parseFloat(optData.kCalMl);

					var optText = "Formula: " + option.text() + ", Volume: " + vol + " mL";
					var option2 = "<option value='" + formulaID + "'>" + optText + "</option>";
					$('#selEnteral').append(option2);

					vol = parseInt(vol);
					var kCals = vol * fkCalMl;

					var enProteinKcal = kCals * (fprotein / 100);
					var enChoKcal = kCals * (fcho / 100);
					var enLipidKcal = kCals * (flipid / 100);

					var option2 = $("#selEnteral option[value='" + formulaID + "']");
					option2.data('enteral', { protein: enProteinKcal, cho: enChoKcal, lipid: enLipidKcal, volume: vol });

					enProtein = enProtein + Math.round(enProteinKcal);
					enCho = enCho + Math.round(enChoKcal);
					enLipid = enLipid + Math.round(enLipidKcal);
				});
				$.each(rdata.calAdditives, function (index, val) {
					var additiveID = val.AdditiveID;
					var vol = val.Volume;

					var option = $("#AdditiveList option[value='" + additiveID + "']");
					var optData = option.data('additive');

					var fprotein = parseFloat(optData.protein);
					var fcho = parseFloat(optData.cho);
					var flipid = parseFloat(optData.lipid);
					var fkCalUnit = parseFloat(optData.kCalUnit);

					var optText = "Additive: " + option.text() + ", Volume: " + vol + " " + optData.unitName;
					var option2 = "<option value='" + additiveID + "'>" + optText + "</option>";
					$('#selAdditive').append(option2);

					vol = parseInt(vol);
					var kCals = vol * fkCalUnit;

					var enProteinKcal = kCals * (fprotein / 100);
					var enChoKcal = kCals * (fcho / 100);
					var enLipidKcal = kCals * (flipid / 100);

					var option2 = $("#selAdditive option[value='" + additiveID + "']");
					option2.data('additive', { protein: enProteinKcal, cho: enChoKcal, lipid: enLipidKcal, volume: vol });

					enProtein = enProtein + Math.round(enProteinKcal);
					enCho = enCho + Math.round(enChoKcal);
					enLipid = enLipid + Math.round(enLipidKcal);
				});

				$('#parenteralLipid').text(pnLipid);
				$('#parenteralCHO').text(pnCho);
				$('#parenteralProtein').text(pnProtein);
				$('#enteralProtein').text(enProtein);
				$('#enteralCHO').text(enCho);
				$('#enteralLipid').text(enLipid);
				recalculateResultsTotal();
			}
		});
	}
	else {
		$('#btnCancel').hide();
	}
	//#endregion

	//#region infusion
	$('.infusions1').change(function () {
		var total = 0;
		$('.infusions1').each(function () {
			var val = $(this).val();
			if (val) {
				total = total + parseInt(val);
			}
		});
		$('#infuse1Total').html("<strong>" + total + "</strong>");
		recalculateResultsTotal();
	});

	$('.infusions2').change(function () {
		var total = 0;
		$('.infusions2').each(function () {
			var val = $(this).val();
			if (val) {
				total = total + parseInt(val);
			}
		});
		$('#infuse2Total').html("<strong>" + total + "</strong>");
		recalculateResultsTotal();
	});

	$('.infusions3').change(function () {
		var total = 0;
		$('.infusions3').each(function () {
			var val = $(this).val();
			if (val) {
				total = total + parseInt(val);
			}
		});
		$('#infuse3Total').html("<strong>" + total + "</strong>");
		recalculateResultsTotal();
	});

	$('.infusions4').change(function () {
		var total = 0;
		$('.infusions4').each(function () {
			var val = $(this).val();
			if (val) {
				total = total + parseInt(val);
			}
		});
		$('#infuse4Total').html("<strong>" + total + "</strong>");
		recalculateResultsTotal();
	});

	$('#DexCons1').change(function () {
		var val = $('#DexCons1 option:selected').val();
		if (val === "0") {
			$('.infusions1').attr("disabled", "disabled");
			$('.infusions1').each(function () {
				$(this).val("");
			});
			$('#infuse1Total').html("<strong>0</strong>");
		}
		else {
			$('.infusions1').removeAttr("disabled");
		}
		recalculateResultsTotal();
	});
	$('#DexCons2').change(function () {
		var val = $('#DexCons2 option:selected').val();
		if (val === "0") {
			$('.infusions2').attr("disabled", "disabled");
			$('.infusions2').each(function () {
				$(this).val("");
			});
			$('#infuse2Total').html("<strong>0</strong>");
		}
		else {
			$('.infusions2').removeAttr("disabled");
		}
		recalculateResultsTotal();
	});
	$('#DexCons3').change(function () {
		var val = $('#DexCons3 option:selected').val();
		if (val === "0") {
			$('.infusions3').attr("disabled", "disabled");
			$('.infusions3').each(function () {
				$(this).val("");
			});
			$('#infuse3Total').html("<strong>0</strong>");
		}
		else {
			$('.infusions3').removeAttr("disabled");
		}
		recalculateResultsTotal();
	});
	$('#DexCons4').change(function () {
		var val = $('#DexCons4 option:selected').val();
		if (val === "0") {
			$('.infusions4').attr("disabled", "disabled");
			$('.infusions4').each(function () {
				$(this).val("");
			});
			$('#infuse4Total').html("<strong>0</strong>");
		}
		else {
			$('.infusions4').removeAttr("disabled");
		}
		recalculateResultsTotal();
	});

	

	//#endregion

	//#region parenteral
	$('#btnAddDex').click(function () {
		if (!validateDex()) {
			return;
		}

		var valDex = $('#txtDex').val();
		var valAm = $('#txtAm').val();
		var valVol = $('#txtPNVol').val();
		var optText = "Dextrose:" + valDex + "%, Amino Acid:" + valAm + "%, Volume:" + valVol + "mL";

		var pnChoKcal = Math.round(valDex * 0.034 * valVol);
		pnCho = pnCho + pnChoKcal;

		var pnProteinKcal = Math.round(valAm * 0.04 * valVol)
		pnProtein = pnProtein + pnProteinKcal;

		$('#parenteralCHO').text(pnCho);
		$('#parenteralProtein').text(pnProtein);

		tempID++;

		var option = "<option value='" + tempID + "' >" + optText + "</option>";
		$('#selParenteral').append(option);

		option = $("#selParenteral option[value='" + tempID + "']");
		$(option).data('parenteral', { protein: pnProteinKcal, cho: pnChoKcal, lipid: 0 });

		recalculateResultsTotal();
		$('#txtDex').val("");
		$('#txtAm').val("");
		$('#txtPNVol').val("");
	});

	$('#btnAddLip').click(function () {
		if (!validateLip()) {
			return;
		}

		var lipid = $('#LipidConc option:selected').text();
		var vol = $('#txtLPVol').val();
		var optText = "Lipid Concentration:" + lipid + "%, Volume:" + vol + "mL";

		var kCal = $('#LipidConc option:selected').val();

		var pnLipidKcal = Math.round(kCal * vol);
		pnLipid = pnLipid + pnLipidKcal;
		$('#parenteralLipid').text(pnLipid);

		tempID++;
		var option = "<option value='" + tempID + "' >" + optText + "</option>";
		$('#selParenteral').append(option);

		option = $("#selParenteral option[value='" + tempID + "']");
		$(option).data('parenteral', { protein: 0, cho: 0, lipid: pnLipidKcal });

		$('#LipidConc').val("0");
		$('#txtLPVol').val("");
		recalculateResultsTotal();
	});

	$('#btnRemoveParenteral').click(function () {
		var $selected = $("#selParenteral option:selected")
		$selected.each(function () {
			var option = $(this).text();
			var data = $(this).data('parenteral');

			pnProtein = pnProtein - data.protein;
			pnCho = pnCho - data.cho;
			pnLipid = pnLipid - data.lipid;

			$('#parenteralCHO').text(pnCho);
			$('#parenteralProtein').text(pnProtein);
			$('#parenteralLipid').text(pnLipid);

		});
		$("#selParenteral option:selected").remove();
		recalculateResultsTotal();
	});

	function validateLip() {
		var val = $('#LipidConc option:selected').text();
		if ($.trim(val) === 'Select') {
			alert('Select a lipid concentration');
			return false;
		}
		val = $('#txtLPVol').val();
		if (val.length === 0) {
			alert('Volume is required');
			return false;
		}
		return true;
	}

	function validateDex() {
		var val = $('#txtDex').val();
		if (val.length === 0) {
			alert('Dextrose % is required');
			return false;
		};

		val = $('#txtAm').val();
		if (val.length === 0) {
			alert('Amino Acid % is required');
			return false;
		}

		val = $('#txtPNVol').val();
		if (val.length === 0) {
			alert('Volume is required');
			return false;
		}
		return true;
	}

	//#endregion

	//#region enteral
	$('#btnAddFormula').click(function () {
		if (!validateAddFormula()) {
			return;
		}

		var valFormula = $('#FormulaList option:selected').text();
		var fid = $('#FormulaList').val();
		var valVol = $('#txtVolFormula').val();
		var optText = "Formula: " + valFormula + ", Volume: " + valVol + " mL";

		var option = "<option value='" + fid + "'>" + optText + "</option>";
		$('#selEnteral').append(option);

		var data = $("#FormulaList option:selected").data('formula');
		var fprotein = parseFloat(data.protein);
		var fcho = parseFloat(data.cho);
		var flipid = parseFloat(data.lipid);
		var fkCalMl = parseFloat(data.kCalMl);

		valVol = parseFloat(valVol);
		var kCals = valVol * fkCalMl;

		var enProteinKcal = kCals * (fprotein / 100);
		var enChoKcal = kCals * (fcho / 100);
		var enLipidKcal = kCals * (flipid / 100);

		//add data to option
		option = $("#selEnteral option[value='" + fid + "']");
		$(option).data('enteral', { protein: enProteinKcal, cho: enChoKcal, lipid: enLipidKcal, volume: valVol });

		//update results
		enProtein = enProtein + Math.round(enProteinKcal);
		enCho = enCho + Math.round(enChoKcal);
		enLipid = enLipid + Math.round(enLipidKcal);
		$('#enteralProtein').text(enProtein);
		$('#enteralCHO').text(enCho);
		$('#enteralLipid').text(enLipid);
		recalculateResultsTotal();

		$('#FormulaList').val("0");
		$('#txtVolFormula').val("");
	});

	$('#btnRemoveEnteral').click(function () {
		var $selected = $("#selEnteral option:selected")
		$selected.each(function () {
			var option = $(this).text();
			var data = $(this).data('enteral');

			enProtein = enProtein - Math.round(data.protein);
			enCho = enCho - Math.round(data.cho);
			enLipid = enLipid - Math.round(data.lipid);
			$('#enteralProtein').text(enProtein);
			$('#enteralCHO').text(enCho);
			$('#enteralLipid').text(enLipid);
		});

		$("#selEnteral option:selected").remove();
		recalculateResultsTotal();
	});

	$('#btnSaveNewFormula').click(function () {
		if (!validateNewFormula()) {
			return;
		}

		var valN = $.trim($('#newFormulaName').val());
		var valP = $('#newProtein').val();
		var valC = $('#newCHO').val();
		var valL = $('#newLipid').val();
		var valK = $('#newkCalMl').val();

		$.ajax({
			data: { ID: '0', Name: valN, ProteinKcal: valP, ChoKcal: valC, LipidKcal: valL, Kcal_ml: valK },
			url: urlRoot + '/CalorieCalc/AddNewFormula',
			type: 'POST',
			success: function (rdata) {
				if (rdata.ReturnValue === 1) {
					var option = "<option value=" + rdata.Bag.ID + " >" + rdata.Bag.Name + "</option>";
					$("#FormulaList").append(option);
					option = $("#FormulaList option[value='" + rdata.Bag.ID + "']");
					option.data('formula', { protein: rdata.Bag.ProteinKcal,
						cho: rdata.Bag.ChoKcal,
						lipid: rdata.Bag.LipidKcal,
						kCalMl: rdata.Bag.Kcal_ml
					});
					//var dat = $(option).data('formula');
					//alert('option data:' + $(option).data('formula'));
					$("#FormulaList").val(rdata.Bag.ID);
					//alert('selected valule: ' + $("#FormulaList").val());
					alert(rdata.Bag.Name + ' was added successfully!');
					$('#divNewFormula').dialog('close');
				}
			}

		});
	});

	$('#btnCloseNewFormula').click(function () {
		$('#divNewFormula').dialog('close');
	});

	function validateAddFormula() {
		var val = $('#FormulaList option:selected').text();
		if (val === 'Select') {
			alert('Select a formula');
			return false;
		}
		val = $('#txtVolFormula').val();
		if (val.length === 0) {
			alert('Volume is required');
			return false;
		}
		return true;
	}

	function validateNewFormula() {
		var valN = $.trim($('#newFormulaName').val());
		if (valN.length === 0) {
			alert('Formula name is required');
			$('#newFormulaName').focus();
			return false;
		}
		var valP = $('#newProtein').val();
		if (valP.length === 0) {
			alert('Protein % of kcal is required');
			$('#newProtein').focus();
			return false;
		}
		var valC = $('#newCHO').val();
		if (valC.length === 0) {
			alert('CHO % of kcal is required');
			$('#newCHO').focus();
			return false;
		}
		var valL = $('#newLipid').val();
		if (valL.length === 0) {
			alert('Lipid % of kcal is required');
			$('#newLipid').focus();
			return false;
		}

		var valK = $('#newkCalMl').val();
		if (valK.length === 0) {
			alert('kCal per mL is required');
			$('#newkCalMl').focus();
			return false;
		}

		if (parseInt(valP) + parseInt(valC) + parseInt(valL) !== 100) {
			alert("The sum of Protein, CHO and Lipid must equal 100%");
			return false;
		}

		return true;
	}

	//#endregion

	//#region additives
	$('#btnAddAdditive').click(function () {
		if (!validateAddAdditive()) {
			return;
		}

		var valFormula = $('#AdditiveList option:selected').text();
		var fid = $('#AdditiveList').val();
		var valVol = $('#txtAdditiveAmt').val();
		var data = $('#AdditiveList option:selected').data('additive');

		var optText = "Additive: " + valFormula + ", Amount: " + valVol + " " + data.unitName;

		var option = "<option value='" + fid + "'>" + optText + "</option>";
		$('#selAdditive').append(option);

		var fprotein = parseFloat(data.protein);
		var fcho = parseFloat(data.cho);
		var flipid = parseFloat(data.lipid);
		var fkCalUnit = parseFloat(data.kCalUnit);

		valVol = parseFloat(valVol);
		var kCals = valVol * fkCalUnit;

		var enProteinKcal = kCals * (fprotein / 100);
		var enChoKcal = kCals * (fcho / 100);
		var enLipidKcal = kCals * (flipid / 100);

		//add data to option
		option = $("#selAdditive option[value='" + fid + "']");
		$(option).data('additive', { protein: enProteinKcal, cho: enChoKcal, lipid: enLipidKcal, volume: valVol });

		//update results
		enProtein = enProtein + Math.round(enProteinKcal);
		enCho = enCho + Math.round(enChoKcal);
		enLipid = enLipid + Math.round(enLipidKcal);
		$('#enteralProtein').text(enProtein);
		$('#enteralCHO').text(enCho);
		$('#enteralLipid').text(enLipid);
		recalculateResultsTotal();

		$('#AdditiveList').val("0");
		$('#txtAdditiveAmt').val("");
		$('#spanUnit').text("");
	});

	$('#btnRemoveAdditive').click(function () {
		var $selected = $("#selAdditive option:selected")
		$selected.each(function () {
			var option = $(this).text();
			var data = $(this).data('additive');

			enProtein = enProtein - Math.round(data.protein);
			enCho = enCho - Math.round(data.cho);
			enLipid = enLipid - Math.round(data.lipid);
			$('#enteralProtein').text(enProtein);
			$('#enteralCHO').text(enCho);
			$('#enteralLipid').text(enLipid);
		});

		$("#selAdditive option:selected").remove();
		recalculateResultsTotal();
	});

	$('#btnSaveNewAdditive').click(function () {
		if (!validateNewAdditive()) {
			return;
		}

		var valN = $.trim($('#newAdditiveName').val());
		var valP = $('#newAdditiveProtein').val();
		var valC = $('#newAdditiveCHO').val();
		var valL = $('#newAdditiveLipid').val();
		var valK = $('#newkCalUnit').val();
		var valU = $('#Units').val();
		var valUn = $('#Units option:selected').text();
		valN = valN + ' (' + valUn + ')';

		$.ajax({
			data: { ID: '0', Name: valN, ProteinKcal: valP, ChoKcal: valC, LipidKcal: valL, Kcal_Unit: valK, Unit: valU, UnitName: valUn },
			url: urlRoot + '/CalorieCalc/AddNewAdditive',
			type: 'POST',
			success: function (rdata) {
				if (rdata.ReturnValue === 1) {
					var option = "<option value=" + rdata.Bag.ID + " >" + rdata.Bag.Name + "</option>";
					$("#AdditiveList").append(option);
					option = $("#AdditiveList option[value='" + rdata.Bag.ID + "']");
					option.data('additive', { protein: rdata.Bag.ProteinKcal,
						cho: rdata.Bag.ChoKcal,
						lipid: rdata.Bag.LipidKcal,
						kCalUnit: rdata.Bag.Kcal_unit,
						unit: rdata.Bag.Unit,
						unitName: rdata.Bag.UnitName
					});
					//var dat = $(option).data('formula');
					//alert('option data:' + $(option).data('formula'));
					$("#AdditiveList").val(rdata.Bag.ID);
					$('#spanUnit').text(rdata.Bag.UnitName);
					//alert('selected valule: ' + $("#FormulaList").val());
					alert(rdata.Bag.Name + ' was added successfully!');
					$('#divNewAdditive').dialog('close');
				}
				else {
					alert(rdata.Message);
				}
			}

		});
	});

	$('#btnCloseNewAdditive').click(function () {
		$('#divNewAdditive').dialog('close');
	});
	function validateNewAdditive() {
		var valN = $.trim($('#newAdditiveName').val());
		if (valN.length === 0) {
			alert('Additive name is required');
			$('#newAdditiveName').focus();
			return false;
		}
		var valP = $('#newAdditiveProtein').val();
		if (valP.length === 0) {
			alert('Protein % of kcal is required');
			$('#newAdditiveProtein').focus();
			return false;
		}
		var valC = $('#newAdditiveCHO').val();
		if (valC.length === 0) {
			alert('CHO % of kcal is required');
			$('#newAdditiveCHO').focus();
			return false;
		}
		var valL = $('#newAdditiveLipid').val();
		if (valL.length === 0) {
			alert('Lipid % of kcal is required');
			$('#newAdditiveLipid').focus();
			return false;
		}

		var valK = $('#newkCalUnit').val();
		if (valK.length === 0) {
			alert('kCal per unit is required');
			$('#newkCalUnit').focus();
			return false;
		}

		if (parseInt(valP) + parseInt(valC) + parseInt(valL) !== 100) {
			alert("The sum of Protein, CHO and Lipid must equal 100%");
			return false;
		}

		var val = $('#Units option:selected').text();
		if (val === 'Select') {
			alert('Select a unit');
			$('#Units').focus();
			return false;
		}
		return true;
	}

	function validateAddAdditive() {
		var val = $('#AdditiveList option:selected').text();
		if (val === 'Select') {
			alert('Select an additive');
			return false;
		}
		val = $('#txtAdditiveAmt').val();
		if (val.length === 0) {
			alert('Amount is required');
			return false;
		}
		return true;
	}

	//#endregion

	function recalculateResultsTotal() {
		calculateInfusionsTotal();
		resultsTotal = parseInt(infusionTotal + pnCho + pnProtein + pnLipid + enCho + enProtein + enLipid);
		$('#totalCalIntake').text(resultsTotal);

		recalculateCalsPerKilo();
	}

	function calculateInfusionsTotal() {
		infusionTotal = 0;
		var kCalMl = 0;
		var colTotal = 0;

		kCalMl = $('#DexCons1 option:selected').val();
		if (kCalMl !== "0") {
			colTotal = parseInt($('#infuse1Total').text());
			infusionTotal = infusionTotal + Math.round(kCalMl * colTotal);
		}

		kCalMl = $('#DexCons2 option:selected').val();
		if (kCalMl !== "0") {
			colTotal = parseInt($('#infuse2Total').text());
			infusionTotal = infusionTotal + Math.round(kCalMl * colTotal);
		}

		kCalMl = $('#DexCons3 option:selected').val();
		if (kCalMl !== "0") {
			colTotal = parseInt($('#infuse3Total').text());
			infusionTotal = infusionTotal + Math.round(kCalMl * colTotal);
		}

		kCalMl = $('#DexCons4 option:selected').val();
		if (kCalMl !== "0") {
			colTotal = parseInt($('#infuse4Total').text());
			infusionTotal = infusionTotal + Math.round(kCalMl * colTotal);
		}
	}

	function recalculateCalsPerKilo() {
		var weight = $('#bodyWeight').val();
		if (weight.length === 0) {
			alert('You must enter a body weight in order to calculate Calories per kilo per day');
			return;
		}

		if (weight === 0) {
			alert('You must enter a body weight greater than 0 in order to calculate Calories per kilo per day');
			return;
		}
		var infuseTotPer = infusionTotal / weight;
		var pnChoPer = pnCho / weight;
		var pnProteinPer = pnProtein / weight;
		var pnLipidPer = pnLipid / weight;
		var enChoPer = enCho / weight;
		var enProteinPer = enProtein / weight;
		var enLipidPer = enLipid / weight;

		$('#parenteralLipidPer').text(Math.round(pnLipidPer));
		$('#parenteralCHOPer').text(Math.round(pnChoPer));
		$('#parenteralProteinPer').text(Math.round(pnProteinPer));
		$('#enteralProteinPer').text(Math.round(enProteinPer));
		$('#enteralCHOPer').text(Math.round(enChoPer));
		$('#enteralLipidPer').text(Math.round(enLipidPer));
		$('#totalCalIntakePer').text(Math.round(resultsTotal/weight));
	}
});