var VCSEL_RMA = function(){
    var show = function () {
        var rate = $("#vcselratelist").val();

        $.post('/DataAnalyze/VcselRMADppmData',
            { rate : rate },
            function (output) {
            if (output.success) {
                drawline(output.dppmline);
            }
        });

        $.post('/DataAnalyze/VcselVSTimeData', {}, function (output) {
            if (output.success) {
                drawshipline(output.shipdatedata);
                drawshipline(output.accumulatedata);
            }
        });

        $.post('/DataAnalyze/VcselRMAMileStoneData', {}, function (output) {
            if (output.success) {
                drawcolumn(output.vcsel_milestone);
            }
        });

        $.post('/DataAnalyze/VcselRMAMileStoneData?datetype=SHIPDATE', {}, function (output) {
            if (output.success) {
                drawcolumn(output.vcsel_milestone);
            }
        });


    }

    var drawshipline = function(line_data)
    {
        var options = {
            chart: {
                type: 'line'
            },
            title: {
                text: line_data.title
            },
            xAxis: {
                categories: line_data.xaxis
            },
            yAxis: {
                title: {
                    text: 'Failures'
                }
            },
            series: line_data.data,
            exporting: {
                menuItemDefinitions: {
                    fullscreen: {
                        onclick: function () {
                            $('#' + line_data.id).parent().toggleClass('chart-modal');
                            $('#' + line_data.id).highcharts().reflow();
                        },
                        text: 'Full Screen'
                    },
                    exportdata: {
                        onclick: function () {
                            var filename = line_data.title + '.csv';
                            var outputCSV = 'xAxis\r\n';
                            $(line_data.xaxis).each(function (i, val) {
                                    outputCSV += val + "," ;
                            })
                            outputCSV += "\r\n";
                            $(line_data.data).each(function (i, val) {
                                outputCSV += val.name + "\r\n";
                                $(val.data).each(function (i, sval) { 
                                    outputCSV += sval + ",";
                                })
                                outputCSV += "\r\n";
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
                            //var imgtag = '<img src="' + dataURL + '"/>';

                            var img = new Image();
                            img.src = dataURL;

                            copyImgToClipboard(img);
                        },
                        text: 'copy 2 clipboard'
                    }
                },
                buttons: {
                    contextButton: {
                        menuItems: ['fullscreen', 'exportdata', 'datalabel', 'copycharts', 'printChart', 'separator', 'downloadPNG', 'downloadJPEG', 'downloadPDF', 'downloadSVG']
                    }
                }
            }
        };
        Highcharts.chart(line_data.id, options);
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
                categories: line_data.xAxis.data,
                plotBands: line_data.plotbands
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
            plotOptions: {
                series: {
                    cursor: 'pointer',
                    events: {
                        click: function (event) {
                            var wafer = event.point.category;
                            $('#waferval').html(wafer);
                            $('#waferyield').attr('href', '/DataAnalyze/WaferDistribution?defaultwafer='+wafer);
                            $('#wafertestdata').attr('href', '/DataAnalyze/DownLoadWafer?wf_no=' + wafer + '&withfilter=false');
                            $.post('/DataAnalyze/RetrieveVcselRMARawData',
                                {
                                    wafer: wafer
                                },
                                function (outputdata) {
                                    $('#ramrawbody').empty();
                                    $.each(outputdata.waferdatalist, function (i, val) {
                                        var rmalink = '<td> </td>';
                                        if (val.IssueKey != '')
                                        {
                                            rmalink = '<td><a href="/Issue/UpdateIssue?issuekey=' + val.IssueKey + '" target="_blank" >Report</a></td>'
                                        }

                                        var appendstr = '<tr>' +
                                            '<td>' + (i+1) + '</td>' +
                                            '<td>'+val.SN+'</td>'+
                                            '<td>'+val.PN+'</td>'+
                                            '<td>' + val.VcselType + '</td>' +
                                            '<td>' + val.ProductType + '</td>' +
                                            '<td>' + val.ShipDate + '</td>' +
                                            '<td>' + val.RMAOpenDate + '</td>' +
                                            '<td>' + val.Customer + '</td>' +
                                            rmalink
                                            + '</tr>';
                                        $('#ramrawbody').append(appendstr);
                                    });
                                    $('#rmarawdata').modal('show')
                            })
                            
                        }
                    }
                }
            },
            series: [{
                name: line_data.data.cdata.name,
                color: '#ff3399',
                type: 'column',
                data: line_data.data.cdata.data,
                yAxis: 1
            },{
                name: line_data.data.data.name,
                dataLabels: {
                    enabled: false,
                    color: line_data.data.data.color,
                },
                marker: {
                    radius: 4
                },
                enableMouseTracking: true,
                data: line_data.data.data.data
            }],
            exporting: {
                menuItemDefinitions: {
                    fullscreen: {
                        onclick: function () {
                            $('#' + line_data.id).parent().toggleClass('chart-modal');
                            $('#' + line_data.id).highcharts().reflow();
                        },
                        text: 'Full Screen'
                    },
                    exportdata: {
                        onclick: function () {
                            var filename = line_data.title + '.csv';
                            var outputCSV = 'Wafer_No,Output,Dppm\r\n';
                            $(line_data.xAxis.data).each(function (i, val) {
                                $(line_data.data).each(function () {
                                    outputCSV += val + "," + this.cdata.data[i] + "," + this.data.data[i] + ",\r\n";
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
                            //var imgtag = '<img src="' + dataURL + '"/>';

                            var img = new Image();
                            img.src = dataURL;

                            copyImgToClipboard(img);
                        },
                        text: 'copy 2 clipboard'
                    }
                },
                buttons: {
                    contextButton: {
                        menuItems: ['fullscreen', 'exportdata', 'datalabel', 'copycharts', 'printChart', 'separator', 'downloadPNG', 'downloadJPEG', 'downloadPDF', 'downloadSVG']
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
                            var outputCSV = 'Date,Type,Amount\r\n';
                            var milestonecsv = 'Date,Milestone\r\n';
                            $(col_data.xAxis.data).each(function (i, val) {
                                $(col_data.data).each(function () {
                                    if (this.name == 'Milestone') {
                                        $(this.data).each(function() {
                                            if (val == col_data.xAxis.data[this.x]) {
                                                milestonecsv += val + "," + this.name.replace(',', ';').replace('<br/>', ' / ') + "\r\n";
                                            }
                                        })
                                    }
                                    else {
                                        if (this.data[i] != 0) {
                                            outputCSV += val + "," + this.name + "," + this.data[i] + "\r\n";
                                        }
                                    }
                                });
                            })
                            outputCSV += "\r\n\r\n" + milestonecsv;
                            var blobby = new Blob([outputCSV], { type: 'text/csv;chartset=utf-8' });
                            $(exportLink).attr({
                                'download': filename,
                                'href': window.URL.createObjectURL(blobby),
                                'target': '_blank'
                            });
                            exportLink.click();
                        },
                        text: 'Export Data'
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
                            //var imgtag = '<img src="' + dataURL + '"/>';

                            var img = new Image();
                            img.src = dataURL;

                            copyImgToClipboard(img);
                        },
                        text: 'copy 2 clipboard'
                    }
                },
                buttons: {
                    contextButton: {
                        menuItems: ['fullscreen', 'exportdata', 'datalabel', 'copycharts', 'printChart', 'separator', 'downloadPNG', 'downloadJPEG', 'downloadPDF', 'downloadSVG']
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