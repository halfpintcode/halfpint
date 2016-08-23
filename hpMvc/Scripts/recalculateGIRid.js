$(function () {
    $("#spinner").ajaxStart(function () { $(this).show(); })
			   .ajaxStop(function () { $(this).hide(); });
    $('#btnRecalc').click(function () {
        $('#btnRecalc').attr('disabled', 'disabled');
        var id = $('#id').val();
        var calcDate = $('#calcDate').val();
        var url = window.urlRoot + '/Admin/RecalculateGIRbyId/';
        
        $.ajax({
            url: url,
            type: 'POST',
            data: {subjectId:id, calcDate:calcDate},
            success: function (gir) {
                alert('gir updated to ' + gir);
            },
            complete: function () {
                $('#btnRecalc').attr('disabled', false);
            }
        });
    });
});