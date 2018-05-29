var IDashboard = function () {
    var show = function () {
        $('.date').datepicker({ autoclose: true });
        GetChartData();
        $('body').on('click', '#btn-clear', function () {
            window.location.href = "/User/IDashboard";
        })
        $('body').on('click', '#btn-search', function () {
            var sdate = $.trim($('#sdate').val());
            var edate = $.trim($('#edate').val());
            var uname = $.trim($('#uname').val());
            window.location.href = "/User/IDashboard?uname=" + uname + "&sdate=" + sdate + "&edate=" + edate;
        })
        function GetChartData() {
            var sdate = $.trim($('#sdate').val());
            var edate = $.trim($('#edate').val());
            var uname = $.trim($('#uname').val());
            $.post('/User/GetDashboardData', {
                uname: uname,
                sdate: sdate,
                edate: edate
            }, function (output) {
                if (output.success) {
                    drawline(output.tdata);
                    drawline(output.bdata);
                    drawline(output.cdata);
                    drawline(output.ddata);

                }
            })
        }
        var drawline = function (data) {
            var options = {
                chart: {
                    type: 'line'
                },
                title: {
                    text: data.title
                },
                xAxis: {
                    categories: data.xAxis.data
                },
                yAxis: {
                    title: {
                        text: data.yAxis.title
                    }
                },
                plotOptions: {
                    series: {
                        cursor: 'pointer',
                        events: {
                            click: function (event) {
                                var uname = $.trim($('#uname').val());
                                window.open("/User/DashboardDetail?uname=" + uname + "&type=" + data.type + "&date=" + event.point.category);
                            }
                        }
                    }
                },
                series: data.data,
                exporting: {
                    menuItemDefinitions: {
                        fullscreen: {
                            onclick: function () {
                                $('#' + data.id).parent().toggleClass('chart-modal');
                                $('#' + data.id).highcharts().reflow();
                            },
                            text: 'Full Screen'
                        }
                    },
                    buttons: {
                        contextButton: {
                            menuItems: ['fullscreen', 'printChart', 'separator', 'downloadPNG', 'downloadJPEG', 'downloadPDF', 'downloadSVG']
                        }
                    }
                }
            };
            Highcharts.chart(data.id, options);
        }

    }
    var detail = function () {
        $('body').on('click', '.analysis-detail', function () {
            var id = $.trim($(this).data('id'));
            $.post('/Project/RetrieveErrorCommentsByAnalyzeID',
            {
                id: id
            }, function (output) {
                $('#span-title').val(output.title.content);
                $('#des-info').html(output.failuredetail.content);
                $('#des-reporter').html(output.failuredetail.reporter);
                $('#des-datetime').html(output.failuredetail.time);
                $('#rc-info').html(output.rootcause.content);
                $('#rc-reporter').html(output.rootcause.reporter);
                $('#rc-datetime').html(output.rootcause.time);
                $('#res-info').html(output.result.content);
                $('#res-reporter').html(output.result.reporter);
                $('#res-datetime').html(output.result.time);
                $('#analysis-modal').modal('show');
            })
        })
    }
    return {
        init: function () {
            show();
        },
        detail: function () {
            detail();
        }
    }

}();