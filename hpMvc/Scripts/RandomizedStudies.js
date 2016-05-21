$(function () {
    alert('');
    $('#Sites').change(function () {
        $.getJSON('@Url.Action("GetSiteRandomizedStudies")',
                { siteID: $(this).val() },
                function (result) {
                    var grid = $('#grid');
                    grid.empty();
                    grid.append(result.Data);
                    $('#spanTotal').text("Total Subjects Randomized: " + result.Count);
                },
                function (error) {
                    alert(error);
                });

    });
});