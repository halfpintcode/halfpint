$(function () {
    $("#spinner").ajaxStart(function () { $(this).show(); })
			   .ajaxStop(function () { $(this).hide(); });
    $('#btnRecalc').click(function () {
        $('#btnRecalc').attr('disabled', 'disabled');
        var url = window.urlRoot + '/Admin/RecalculateGIR';
        $.ajax({
            url: url,
            type: 'POST',
            data: '{"test":"this"}',
            success: function (gir) {
                alert("Update completed!");
            },
            complete: function () {
                $('#btnRecalc').attr('disabled', false);
            }
        });
    });
});