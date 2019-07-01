var YIELDMAIN = function () {
    var show = function () {
        function searchdata() {
            var pjkey = $('#pjkey').val();
            if (pjkey == '')
            { return false; }

            //var options = {
            //    loadingTips: "正在处理数据，请稍候...",
            //    backgroundColor: "#aaa",
            //    borderColor: "#fff",
            //    opacity: 0.8,
            //    borderColor: "#fff",
            //    TipsColor: "#000",
            //    sleep: 0
            //}
            //$.bootstrapLoading.start(options);

            $.post('/Project/PJJSONWeeklyTrendMain',
               {
                   pjkey: pjkey
               }, function (output) {
                   $('#weeklyyield').empty();
                   $('#weeklyyield').html(output.ydchart);
               });

            $.post('/Project/PJJSONBRTrendMain',
               {
                   pjkey: pjkey
               }, function (output) {
                   //$.bootstrapLoading.end();
                   $('#bryield').empty();
                   $('#bryield').html(output.brchartscript);
               });

        }

        $(function () {
            searchdata();
        });
    }

    return {
        init: function () {
            show();
        }
    }
}();