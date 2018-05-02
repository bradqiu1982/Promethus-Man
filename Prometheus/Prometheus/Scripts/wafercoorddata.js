var WaferCoordData = function () {
    var show = function () {
        $('.date').datepicker({ autoclose: true });
        $('body').on('click', '#btn-clear', function () {
            window.location.href = "/CustomerData/ReviewWaferCoordData";
        })
        $('body').on('click', '#btn-search', function () {
            var skey = $.trim($('#skey').val());
            var sdate = $.trim($('#sdate').val());
            var edate = $.trim($('#edate').val());
            window.location.href = "/CustomerData/ReviewWaferCoordData?sn=" + skey + "&sdate=" + sdate + "&edate=" + edate;
        })
        $('body').on('click', '.r-edit', function () {
            var id = $.trim($(this).data('id'));
            var sn = $.trim($(this).data('sn'));
            var waferno = $.trim($(this).data('waferno'));
            var x = $.trim($(this).data('x'));
            var y = $.trim($(this).data('y'));
            var bin = $.trim($(this).data('bin'));
            $("#m-id").val(id);
            $("#m-sn").html(sn);
            $("#m-waferno").val(waferno);
            $("#m-x").val(x);
            $("#m-y").val(y);
            $("#m-bin").val(bin);
            $("#modal-edit-wafercoorddata").modal("show");
        })
        $('body').on('click', '#btn-save', function () {
            var id = $.trim($('#m-id').val());
            var waferno = $.trim($('#m-waferno').val());
            var x = $.trim($('#m-x').val());
            var y = $.trim($('#m-y').val());
            var bin = $.trim($('#m-bin').val());
            $.post("/CustomerData/UpdateWaferCoordData",
            {
                id: id,
                waferno: waferno,
                x: x,
                y: y,
                bin: bin
            }, function (output) {
                if (output.success) {
                    window.location.reload();
                }
            })
        })
        $('body').on('click', '.r-delete', function () {
            if (confirm("Really to delete this wafer coordinate data ?")) {
                var id = $.trim($(this).data('id'));
                $.post('/CustomerData/InvaidWaferCoordData',
                {
                    id: id
                }, function (output) {
                    if (output.success) {
                        window.location.reload();
                    }
                    else {
                        alert("Failed to delete this data!");
                    }
                })
            }
        })
    }
    return {
        init: function () {
            show();
        }
    }

}();