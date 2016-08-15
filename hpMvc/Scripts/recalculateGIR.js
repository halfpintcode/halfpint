$(function () {

    $('#btnRecalc').click(function () {
        var url = window.urlRoot + '/Admin/RecalculateGIR';
        $.ajax({
            url: url,
            type: 'POST',
            data: '{"test":"this"}',
            success: function (data1) {
                alert('ok');
            }
        });
    });
});