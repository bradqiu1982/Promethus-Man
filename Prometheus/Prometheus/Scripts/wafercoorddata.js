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
            if (x == "" || y == "" || bin == "" || waferno == "") {
                alert("please input Wafer No, X, Y, Bin");
                return false;
            }
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
                else {
                    alert("Failed to update");
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
        $('body').on('click', '#check_all', function () {
            var flg = $(this).prop('checked');
            $('.check-id').each(function () {
                $(this).prop("checked", flg);
            })
        })
        $('body').on('click', '.check-id', function () {
            if (!$(this).prop('checked')) {
                $('#check_all').prop('checked', false);
            }
            else {
                var flg = true;
                $('.check-id').each(function () {
                    if (!$(this).prop('checked')) {
                        flg = false;
                        return;
                    }
                })
                if (flg) {
                    $('#check_all').prop('checked', true);
                }
            }
        })
        $('body').on('click', '#batch-update', function () {
            var ids = new Array();
            var sns = new Array();
            $('.check-id:checked').each(function () {
                ids.push($(this).data('id'));
                sns.push($(this).data('sn'));
            })
            if (ids.length > 0 && sns.length > 0) {
                $('#m-ids').val(ids.join(','));
                $('#m-sns').html(sns.join(','));
                $('#modal-batch-edit').modal('show');
            }
            else {
                alert("Please select one SN at least.");
            }
        })
        $('body').on('click', '#btn-saves', function () {
            var ids = $.trim($('#m-ids').val());
            var bin = $.trim($('#m-bins').val());
            if (ids == "" || bin == "") {
                alert("please input bin");
                return false;
            }
            $.post('/CustomerData/BatchUpdateWaferCoordData',
            {
                ids: ids,
                bin: bin
            }, function () {
                if (output.success) {
                    window.location.reload();
                }
                else {
                    alert("Failed to update");
                }
            })

        })
        $('body').on('click', '#s-search', function () {
            window.location.href = "/CustomerData/NeoMapData";
        })
    }
    var search = function () {
        $('body').on('click', '#s-search', function () {
            var sn = $.trim($('#s-sn').val());
            var wafer_no = $.trim($('#s-wafer-no').val());
            var x = $.trim($('#s-x').val());
            var y = $.trim($('#s-y').val());
            if (wafer_no != "" && sn == "" && x == "" && y == "") {
                alert("Please input more info, like SN, Coord_X, Coord_Y");
                return false;
            }
            window.location.href = '/CustomerData/NeoMapData?sn=' + sn + '&wafer_no=' + wafer_no + '&coord_x=' + x + '&coord_y=' + y;
        })
    }
    return {
        init: function () {
            show();
        },
        search: function () {
            search();
        }
    }

}();