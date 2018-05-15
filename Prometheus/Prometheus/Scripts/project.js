var Project = function () {
    var transfer = function () {
        $('body').on("click", "#btn-submit", function () {
            var pkey = $.trim($('#pkey').val());
            var trans_to = $.trim($('#transfer-to').val());
            var melist = $.trim($('#melist').val());
            if (trans_to == "") {
                alert("Please choose Transfer-To");
                return false;
            }
            if (melist == "") {
                alert("Please input Manufacturer Engineer");
                return false;
            }
            $.post('/Project/SubmitTransfer',
            {
                pKey: pkey,
                trans_to: trans_to,
                melist: melist
            }, function (output) {
                if (output.success) {
                    window.location.href = '/Project/ProjectDetail?ProjectKey=' + pkey;
                }
                else {
                    alert("Failed to transfer");
                }
            })
        })
    }
    return {
        transfer: function () {
            transfer();
        }   
    }
}();