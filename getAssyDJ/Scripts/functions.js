function getVirtDir() {
    var Path = location.host;
    var VirtualDirectory;
    if (Path.indexOf("localhost") >= 0 && Path.indexOf(":") >= 0) {
        VirtualDirectory = "";

    }
    else {
        var pathname = window.location.pathname;
        var VirtualDir = pathname.split('/');
        VirtualDirectory = VirtualDir[1];
        VirtualDirectory = '/' + VirtualDirectory;
    }
    return VirtualDirectory;
}

function getAssyDjs() {
    //blockV2("");
    var filter;
    $("#wdw").hide("fade");
    if ($("#selSMT").is(':checked')) {
        filter = "1";
    } else {
        filter = "0";
    }
    //alert(filter);
    $.ajax({
        method: "POST",
        url: getVirtDir() + "/Controllers/getAssyDJs.ashx",
        data: { dj_groups: $("#djs").val(), filter : filter },
        success: function (data) {
            //removeOverlay();
            r = jQuery.parseJSON(data);
            if (r.result === "true") {
                //$("#btnDownload").html("<a href='" + getVirtDir() + "/Reports/SMT/" + r.html + "'>Descargar reporte</a>");
                $("#djsDtl").html(r.html);
                var table = $('#tblDJs').DataTable({
                    "paging": true,
                    "searching": true,
                    "lengthChange": false,
                    "pageLength": 15,
                    "info": false,
                    "autoWidth": true,
                    "ordering": true,
                /*    "responsive": true,*/
                    dom: 'Bfrtip',
                    buttons: [
                        'excel'
                    ]
                    //'language': { 'url': getVirtDir() + '/Scripts/Spanish.json' }
                });
                //new $.fn.dataTable.FixedHeader(table);
                //table.responsive.rebuild();
                //table.responsive.recalc();
                $("#wdw").show("fade");
            }
            return false;
        },
        error: function () { }
    });
}