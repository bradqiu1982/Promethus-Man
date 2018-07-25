var SNSTATUS = function () {
    var show = function () {
        var mywafertable = null;

        $('#marks').focus();

        $('body').on('keypress', '#marks', function (e) {
            if (e.keyCode == 13) {
                var all_marks = $.trim($(this).val()).split('\n');
                var cur_marks = new Array();
                var arr_count = new Array();
                $.each(all_marks, function (i, val) {
                    if (val != "") {
                        if (arr_count[val]) {
                            alert(val + " has already existed.");
                            arr_count[val]++;
                        }
                        else {
                            arr_count[val] = 1;
                            cur_marks.push(val);
                        }
                    }
                })
                $('#total-marks').html(cur_marks.length);
                $('#marks').val(cur_marks.join('\n'));
            }
        })

        function RefreshWaferTable(withoqm) {
            var all_marks = $.trim($('#marks').val()).split('\n');
            var cur_marks = new Array();
            var arr_count = new Array();
            $.each(all_marks, function (i, val) {
                if (val != "") {
                    if (arr_count[val]) {
                        arr_count[val]++;
                    }
                    else {
                        arr_count[val] = 1;
                        cur_marks.push(val);
                    }
                }
            })
            if (cur_marks.length === 0) {
                alert("查询条件不可为空！");
                return false;
            }
            $('#marks').val(cur_marks.join('\n'));
            var options = {
                loadingTips: "正在处理数据，请稍候...",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);
            $.post('/CustomerData/QuerySNStatus',
           {
               marks: JSON.stringify(cur_marks),
               OQM: withoqm
           }, function (output) {
               var idx = 0;
               var datacont = output.data.length;

               if (mywafertable) {
                   mywafertable.destroy();
               }

               $("#WaferTableID").empty();

               for (idx = 0; idx < datacont; idx++) {
                   var line = output.data[idx];
                   if (line.ErrAbbr != "PASS") {
                       $("#WaferTableID").append('<tr class="NGBKG"><td>' + line.ModuleSerialNum + '</td><td>' + line.WhichTest  + '</td><td>' + line.ErrAbbr + '</td><td>' + line.TestTimeStr + '</td><td>' + line.TestStation + '</td><td>' + line.PN + '</td><td>' + line.ModuleType + '</td></tr>');
                   }
                   else {
                       $("#WaferTableID").append('<tr class="GOODBKG"><td>' + line.ModuleSerialNum + '</td><td>' + line.WhichTest + '</td><td>' + line.ErrAbbr + '</td><td>' + line.TestTimeStr + '</td><td>' + line.TestStation + '</td><td>' + line.PN + '</td><td>' + line.ModuleType + '</td></tr>');
                   }
               }
               $.bootstrapLoading.end();
               mywafertable = $('#mywafertable').DataTable({
                   'iDisplayLength': 50,
                   'aLengthMenu': [[20, 50, 100, -1],
                   [20, 50, 100, "All"]],
                   "aaSorting": [],
                   "order": []
               });

           })
        }


        $('body').on('click', '#btn-marks-submit', function () {
            RefreshWaferTable("FALSE");
        })

        $('body').on('click', '#btn-marks-OQM', function (){
            if (confirm('请确认查询数据将作为 OQM 测试数据建档')) {
                RefreshWaferTable("TRUE");
            }
        })

        $('body').on('click', '#btn-marks-clean', function () {
            $('#total-marks').html(0);
            $('#marks').val('');

            if (mywafertable) {
                mywafertable.destroy();
                mywafertable = null;
            }
            $("#WaferTableID").empty();
        })

        $('body').on('click', '.op-download', function () {

            var all_marks = $.trim($('#marks').val()).split('\n');
            var cur_marks = new Array();
            var arr_count = new Array();
            $.each(all_marks, function (i, val) {
                if (val != "") {
                    if (arr_count[val]) {
                        arr_count[val]++;
                    }
                    else {
                        arr_count[val] = 1;
                        cur_marks.push(val);
                    }
                }
            })
            if (cur_marks.length === 0) {
                alert("查询条件不可为空！");
                return false;
            }

            $('#marks').val(cur_marks.join('\n'));

            var options = {
                loadingTips: "正在处理数据，请稍候...",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/CustomerData/DownloadSNLatestStatus',
            {
                marks: JSON.stringify(cur_marks)
            }, function (output) {
                $.bootstrapLoading.end();
                window.open(output.data, '_blank');
            })
        })
    }
    return {
        INIT: function () {
            show();
        }
    }
}();