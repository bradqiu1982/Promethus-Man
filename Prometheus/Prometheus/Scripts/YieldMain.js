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

    var errorshow = function () {
        function searchdata()
        {
            var pjkey = $('#pjkey').val();
            var wks = $('#wks').val();

            if (pjkey == '' || wks == '')
            { return false;}

            $.post('/Project/ErrorDistribution', {
                pjkey: pjkey,
                wks: wks
            }, function (output) {
                drawcolumn(output.chartdata)
            })
        }

        $(function () {
            searchdata();
        });

        var drawcolumn = function (col_data) {
            var options = {
                chart: {
                    zoomType: 'xy',
                    type: 'column',
                },
                title: {
                    text: 'Final Test Failure Summary'
                },
                xAxis: {
                    categories: col_data.xaxis
                },
                legend: {
                    enabled: true,
                },
                yAxis: [{
                    title: {
                        text: 'Count'
                    }
                }],
                tooltip: {
                    headerFormat: '',
                    pointFormatter: function () {
                        return (this.y == 0) ? '' : '<span>' + this.series.name + '</span>: <b>' + this.y + '</b><br/>';
                    },
                    shared: true
                },
                plotOptions: {
                    column: {
                        stacking: 'normal',
                        dataLabels: {
                            color: (Highcharts.theme && Highcharts.theme.dataLabelsColor) || 'black',
                            formatter: function () {
                                return (this.y != 0) ? this.y : "";
                            }
                        }
                    }
                },
                series: col_data.data,
                exporting: {
                    menuItemDefinitions: {
                        fullscreen: {
                            onclick: function () {
                                $('#' + col_data.id).parent().toggleClass('chart-modal');
                                $('#' + col_data.id).highcharts().reflow();
                            },
                            text: 'Full Screen'
                        },
                        datalabel: {
                            onclick: function () {
                                var labelflag = !this.series[0].options.dataLabels.enabled;
                                $.each(this.series, function (idx, val) {
                                    var opt = val.options;
                                    opt.dataLabels.enabled = labelflag;
                                    val.update(opt);
                                })
                            },
                            text: 'Data Label'
                        },
                        copycharts: {
                            onclick: function () {
                                var svg = this.getSVG({
                                    chart: {
                                        width: this.chartWidth,
                                        height: this.chartHeight
                                    }
                                });
                                var c = document.createElement('canvas');
                                c.width = this.chartWidth;
                                c.height = this.chartHeight;
                                canvg(c, svg);
                                var dataURL = c.toDataURL("image/png");
                                var img = new Image();
                                img.src = dataURL;

                                copyImgToClipboard(img);
                            },
                            text: 'copy 2 clipboard'
                        }
                    },
                    buttons: {
                        contextButton: {
                            menuItems: ['fullscreen', 'datalabel', 'copycharts', 'printChart', 'separator', 'downloadPNG', 'downloadJPEG', 'downloadPDF', 'downloadSVG']
                        }
                    }
                }
            };
            Highcharts.chart(col_data.id, options);
        }
    }

    return {
        init: function () {
            show();
        },
        errorinit: function () {
            errorshow();
        }
    }
}();