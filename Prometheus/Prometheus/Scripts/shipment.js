var Shipment = function () {
    var show = function () {
        $('.date').datepicker({ autoclose: true, viewMode: "months", minViewMode: "months" });
        $('body').on('click', '#btn-search', function () {
            var sdate = $.trim($('#sdate').val());
            var edate = $.trim($('#edate').val());

            $.post('/DataAnalyze/ShipmentDistribution', {
                sdate: sdate,
                edate: edate
            }, function (output) {
                if (output.success) {
                    $('.v-content').empty();
                    var appendstr = "";

                    $.each(output.shipdataarray, function (i, val) {
                        appendstr = '<div class="col-xs-12">' +
                               '<div class="v-box" id="' + val.id + '"></div>' +
                               '</div>';
                        $('.v-content').append(appendstr);
                        drawcolumn(val);
                    })
                }
            })
        })

        $('body').on('click', '#btn-download', function () {
            var sdate = $.trim($('#sdate').val());
            var edate = $.trim($('#edate').val());
            var myurl = '/DataAnalyze/DownloadShipmentData?sdate=' + sdate + '&edate=' + edate;
            window.open(myurl, '_blank');
        })
        
    }

    var drawcolumn = function (col_data) {
        var options = {
            chart: {
                zoomType: 'xy',
                type: 'column'
            },
            title: {
                text: col_data.title
            },
            xAxis: {
                categories: col_data.xAxis.data
            },
            legend: {
                enabled: true,
            },
            yAxis: [{
                title: {
                    text: col_data.yAxis.title
                },
                stackLabels: {
                    enabled: true,
                    style: {
                        fontWeight: 'bold',
                        color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                    }
                }
            }, {
                opposite: true,
                title: {
                    text: 'DPPM'
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
                    stacking: 'normal'
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
                    exportdata: {
                        onclick: function () {
                            var filename = col_data.title + '.csv';
                            var outputCSV = ' ,Customer,Ship QTY\r\n';
                            $(col_data.xAxis.data).each(function (i, val) {
                                $(col_data.data).each(function () {
                                    if (this.name != '' && (this.data[i] != 0 || this.name.indexOf('DPPM')>=0)) {
                                        outputCSV += val + "," + this.name + "," + this.data[i] + ",\r\n";
                                    }
                                });
                            })
                            var blobby = new Blob([outputCSV], { type: 'text/csv;chartset=utf-8' });
                            $(exportLink).attr({
                                'download': filename,
                                'href': window.URL.createObjectURL(blobby),
                                'target': '_blank'
                            });
                            exportLink.click();
                        },
                        text: 'Export Data'
                    }
                },
                buttons: {
                    contextButton: {
                        menuItems: ['fullscreen', 'exportdata', 'printChart', 'separator', 'downloadPNG', 'downloadJPEG', 'downloadPDF', 'downloadSVG']
                    }
                }
            }
        };
        Highcharts.chart(col_data.id, options);
    }

return {
        init: function () {
            show();
        }
    }
}();