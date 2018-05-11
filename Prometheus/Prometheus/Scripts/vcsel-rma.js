var VCSEL_RMA = function(){
    var show = function(){
        $.post('/DataAnalyze/VcselRMADppmData', {}, function (output) {
            if (output.success) {
                drawline(output.dppmline);
            }
        });

        $.post('/DataAnalyze/VcselRMAMileStoneData', {}, function (output) {
            if (output.success) {
                drawcolumn(output.vcsel_milestone);
            }
        });
    }


    var drawline = function(line_data){
        var options = {
            chart: {
                zoomType: 'xy',
                type: 'line'
            },
            title: {
                text: line_data.title
            },
            xAxis: {
                categories: line_data.xAxis.data
            },
            yAxis: [{
                title: {
                    text: line_data.yAxis.title
                },
                min: line_data.yAxis.min,
                max: line_data.yAxis.max,
            }, {
                opposite: true,
                title: {
                    text: 'Amount'
                }
            }],
            series: [{
                name: line_data.data.cdata.name,
                color: line_data.data.cdata.color,
                type: 'column',
                data: line_data.data.cdata.data,
                yAxis: 1
            },{
                name: line_data.data.data.name,
                dataLabels: {
                    enabled: true,
                    color: line_data.data.data.color,
                },
                marker: {
                    radius: 4
                },
                enableMouseTracking: false,
                data: line_data.data.data.data
            }],
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
                            var outputCSV = ' ,Failure Mode,Failure Percent\r\n';
                            $(col_data.xAxis.data).each(function (i, val) {
                                $(col_data.data).each(function () {
                                    if (this.data[i].name != '' && this.data[i].y != 0) {
                                        outputCSV += val + "," + this.data[i].name + "," + this.data[i].y + ",\r\n";
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
        Highcharts.chart(line_data.id, options);
    }

    var drawcolumn = function(col_data){
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
            legend:{
                enabled: true,
            },
            yAxis: [{
                min: col_data.yAxis.min,
                max: col_data.yAxis.max,
                title: {
                    text: col_data.yAxis.title
                },
                plotLines: [{
                    value: col_data.time.data,
                    color: col_data.time.color,
                    dashStyle: col_data.time.style,
                    width: 1
                }]
            }],
            tooltip: {
                shared: false
            },
            plotOptions: {
                column: {
                    stacking: col_data.coltype
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
                            var outputCSV = ' ,Failure Mode,Failure Percent\r\n';
                            $(col_data.xAxis.data).each(function (i, val) {
                                $(col_data.data).each(function () {
                                    if (this.data[i].name != '' && this.data[i].y != 0) {
                                        outputCSV += val + "," + this.data[i].name + "," + this.data[i].y + ",\r\n";
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
        show: function(){
            show();
        }
    }
}();