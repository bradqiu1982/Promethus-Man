var JOMesStatus = function () {
    var show = function () {
        var jo = $("#jo").val();
        if (jo !== '')
        {
            $.post('/CustomerData/JOMesProgressAJAX', {jo:jo}, function (output) {
                if (output.success) {
                    drawcolumn(output.jodistribution);
                }
            });
        }
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
            yAxis: {
                title: {
                    text: col_data.yAxis.title
                }
            },
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
                            var outputCSV = 'JO,SN,WorkFlowStep,LastMoveDate\r\n';
                            $(col_data.data2export).each(function (i,val) {
                                outputCSV += val.JO + ',' + val.SN + ',' + val.WorkFlowStep + ',' + val.LastMoveDateStr + '\r\n';
                            });
                            outputCSV += "\r\n\r\n";
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
        show: function () {
            show();
        }
    }
}();