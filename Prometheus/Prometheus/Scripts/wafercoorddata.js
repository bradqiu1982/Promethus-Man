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
    }
    return {
        init: function () {
            show();
        }
    }

}();