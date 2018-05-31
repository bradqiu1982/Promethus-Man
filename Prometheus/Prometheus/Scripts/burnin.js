var BurnIn = function(){
    var show = function(){
        $('.date').datepicker({ autoclose: true, viewMode: "months", minViewMode: "months" });
        $('body').on('click', '#btn-search', function(){
            var sdate = $.trim($('#sdate').val());
            var edate = $.trim($('#edate').val());
            var wf_type = $.trim($('#vcseltypeselectlist').val());
            
            $.post('/DataAnalyze/MonthlyVcselYield', {
                 sdate: sdate,
                 edate: edate,
                 wf_type: wf_type
             }, function(output){
                 if (output.success) {
                     $('.v-content').empty();
                     var appendstr = "";
                     $.each(output.yieldarray, function (i, val) {
                         appendstr = '<div class="col-xs-6">' +
                                '<div class="v-box" id="'+val.id+'"></div>' +
                                '</div>';
                         $('.v-content').append(appendstr);
                         drawline(val);
                     })
                     $.each(output.failurearray, function (i, val) {
                         appendstr = '<div class="col-xs-6">' +
                                '<div class="v-box" id="' + val.id + '"></div>' +
                                '</div>';
                         $('.v-content').append(appendstr);
                         drawcolumn(val);
                     })
                     $('.v-content').append('<div class="v-lengend row"></div>');
                     var colorStr = "";
                     $.each(output.colors, function (i, val) {
                         colorStr = '<span class="span-fm label label-success" style="background-color: '+val+'">'+i+'</span>';
                         $('.v-lengend').append(colorStr);
                     })
                }
             })
        })
    }

    var distribution = function(){
        $('.date').datepicker({ autoclose: true, viewMode: "months", minViewMode: "months" });
        $.post('/DataAnalyze/WaferNumAutoCompelete', {}, function (output) {
            $('.wafer-no').tagsinput({
                freeInput: false,
                typeahead: {
                    source: output.data,
                    minLength: 0,
                    showHintOnFocus: true,
                    autoSelect: false,
                    selectOnBlur: false,
                    changeInputOnSelect: false,
                    changeInputOnMove: false,
                    afterSelect: function (val) {
                        this.$element.val("");
                    }
                }
            });
            defaultsearch();
        });

        function searchdata()
        {
            var sdate = $.trim($('#sdate').val());
            var edate = $.trim($('#edate').val());
            var wf_no = $.trim($('#wf-no').tagsinput('items'));
            var wf_type = $.trim($('#vcseltypeselectlist').val());
            var math_rect = $.trim($('#mathrectlist').val());
            var outlier = $('#outlier').val();

            if (wf_no == '')
            {
                wf_no = $.trim($('#wf-no').parent().find('input').eq(0).val());
            }


            if (wf_no == '' && (sdate == '' || edate == ''))
            {
                alert("Please input your query condition.");
                return false;
            }

            var options = {
                loadingTips: "load vcsel data.....",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/DataAnalyze/WaferDistributionRawData', {
                 sdate: sdate,
                 edate: edate,
                 wf_no: wf_no,
                 wf_type: wf_type,
                math_rect:math_rect,
                outlier:outlier
            }, function (output) {

                 if (output.success) {
                     $('.v-content').empty();
                     var appendstr = "";
                     $.each(output.yieldarray, function (i, val) {
                         appendstr = '<div class="col-xs-6">' +
                             '<div class="v-box" id="' + val.id + '"></div>' +
                             '</div>';
                         $('.v-content').append(appendstr);
                         drawline(val);
                     })

                     $.each(output.boxarray, function (i, val) {
                         if (val.id === 'variation_uniformity_pold_id') {
                             appendstr = '<div class="col-xs-12">' +
                               '<div class="v-box" id="' + val.id + '"></div>' +
                               '</div>';
                             $('.v-content').append(appendstr);
                             drawdbboxplot(val);
                         }
                         else {
                             appendstr = '<div class="col-xs-12">' +
                           '<div class="v-box" id="' + val.id + '"></div>' +
                           '</div>';
                             $('.v-content').append(appendstr);
                            drawboxplot(val);
                         }
                     })

                     $.each(output.failurearray, function (i, val) {
                         appendstr = '<div class="col-xs-6">' +
                                '<div class="v-box" id="' + val.id + '"></div>' +
                                '</div>';
                         $('.v-content').append(appendstr);
                         drawcolumn(val);
                     })
                     $('.v-content').append('<div class="v-lengend row"></div>');
                     var colorStr = "";
                     $.each(output.colors, function (i, val) {
                         colorStr = '<span class="span-fm label label-success" style="background-color: ' + val + '">' + i + '</span>';
                         $('.v-lengend').append(colorStr);
                     })

                 }
                 $.bootstrapLoading.end();
             })
        }

        $('body').on('click', '#btn-search', function(){
            searchdata();
        })

        function defaultsearch() {
            var wf_no = $.trim($('#wf-no').tagsinput('items'));
            if (wf_no == '') {
                wf_no = $.trim($('#wf-no').parent().find('input').eq(0).val());
            }
            if (wf_no != '') {
                searchdata();
            }
        }

        $('body').on('click', '#btn-wf-download', function () {
            var wafer_no = $.trim($('#m-wf-no').val());
            $('#boxplot-alert').modal('hide');
            window.open("/DataAnalyze/DownLoadWafer?" + "wf_no=" + wafer_no);
        })
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
                plotLines: [{
                    value: line_data.data.min.data,
                    color: line_data.data.min.color,
                    dashStyle: line_data.data.min.style,
                    width: 1
                }, {
                    value: line_data.data.max.data,
                    color: line_data.data.max.color,
                    dashStyle: line_data.data.max.style,
                    width: 1
                }]
            }, {
                opposite: true,
                title: {
                    text: 'Amount'
                }

            }],
            series: [{
                 name: line_data.cdata.name,
                 color: '#12cc92',
                 type: 'column',
                 data: line_data.cdata.data,
                 yAxis: 1
                },
                {
                    name: line_data.data.data.name,
                    type: 'line',
                    dataLabels: {
                        enabled: true,
                        color: line_data.data.data.color,
                    },
                    marker: {
                        radius: 2
                    },
                    enableMouseTracking: false,
                    data: line_data.data.data.data,
                    color: '#ffa500'
                }
             ],
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
                            var outputCSV = ' ,Input,Yield\r\n';
                            $(line_data.xAxis.data).each(function (i, val) {
                                outputCSV += val + "," + line_data.cdata.data[i] + ","
                                    + line_data.data.data.data[i] + ",\r\n";
                            });
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
                enabled: false,
            },
            yAxis: {
                min: col_data.yAxis.min,
                max: col_data.yAxis.max,
                title: {
                    text: col_data.yAxis.title
                }
            },
            tooltip: {
                headerFormat: '',
                pointFormatter:function()
                {
                    return (this.y == 0) ? '' : '<span style="color:' + this.color + '">' + this.name + '</span>: <b>' + ((col_data.coltype == 'percent') ? this.percentage : this.y) + '%</b><br/>';
                },
                shared: true
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

    var drawboxplot = function(boxplot_data){
        var options = {
            chart: {
                zoomType: 'xy',
                type: 'boxplot'
            },

            title: {
                text: boxplot_data.title
            },

            legend: {
                enabled: false
            },

            xAxis: {
                categories: boxplot_data.xAxis.data,
                title: {
                    text: boxplot_data.xAxis.title
                }
            },

            yAxis: {
                title: {
                    text: boxplot_data.yAxis.title
                }
            },
            plotOptions: {
                series: {
                    cursor: 'pointer',
                    events: {
                        click: function (event) {
                            $('#m-wf-no').val(event.point.category);
                            $('#boxplot-alert').modal('show')
                        }
                    }
                }
            },

            series: [{
                name: boxplot_data.data.name,
                data: boxplot_data.data.data,
                tooltip: {
                    headerFormat: '<em>{point.key}</em><br/>'
                }
            },
            {
                name: boxplot_data.line.name,
                color: boxplot_data.line.color,
                type: 'line',
                data: boxplot_data.line.data,
                lineWidth: boxplot_data.line.lineWidth
            },
            {
                name: 'Outlier',
                color: Highcharts.getOptions().colors[0],
                type: 'scatter',
                data: boxplot_data.outlierdata,
                marker: {
                    fillColor: Highcharts.getOptions().colors[0],
                    lineWidth: 1,
                    radius: 2,
                    lineColor: Highcharts.getOptions().colors[0]
                },
                tooltip: {
                    headerFormat: '',
                    pointFormat:"{point.y}"
                }
            }],
            exporting: {
                        menuItemDefinitions: {
                            fullscreen: {
                                onclick: function () {
                                    $('#' + boxplot_data.id).parent().toggleClass('chart-modal');
                                    $('#' + boxplot_data.id).highcharts().reflow();
                                },
                                text: 'Full Screen'
                            }
                        },
                        buttons: {
                        contextButton: {
                                    menuItems: ['fullscreen','printChart','separator','downloadPNG','downloadJPEG','downloadPDF','downloadSVG']
                            }
                        }
                    }
        };
        Highcharts.chart(boxplot_data.id, options);
    }
    var drawdbboxplot = function(dbboxplot_data){
        var options = {
            chart: {
                zoomType: 'xy',
                type: 'boxplot'
            },

            title: {
                text: dbboxplot_data.title
            },

            legend: {
                enabled: false
            },

            xAxis: {
                categories: dbboxplot_data.xAxis.data,
                title: {
                    text: dbboxplot_data.xAxis.title
                }
            },

            yAxis: [{
                title: {
                    text: dbboxplot_data.left.yAxis.title
                }
            },{
                opposite: true,
                title: {
                    text: dbboxplot_data.right.yAxis.title
                }

            }],
            plotOptions: {
                series: {
                    cursor: 'pointer',
                    events: {
                        click: function (event) {
                            $('#m-wf-no').val(event.point.category);
                            $('#boxplot-alert').modal('show')
                        }
                    }
                }
            },

            series: [{
                name: dbboxplot_data.left.data.name,
                data: dbboxplot_data.left.data.data,
                color: dbboxplot_data.left.data.color,
                tooltip: {
                    headerFormat: '<em>{point.key}</em><br/>'
                }
            },
            {
                 name: dbboxplot_data.left.line.name,
                 color: dbboxplot_data.left.line.color,
                 type: 'line',
                 data: dbboxplot_data.left.line.data,
                 lineWidth: dbboxplot_data.left.line.lineWidth
            },
            {
                name: dbboxplot_data.right.data.name,
                data: dbboxplot_data.right.data.data,
                color: dbboxplot_data.right.data.color,
                tooltip: {
                    headerFormat: '<em>{point.key}</em><br/>'
                },
                yAxis: 1
            },
             {
                 name: dbboxplot_data.right.line.name,
                 color: dbboxplot_data.right.line.color,
                 type: 'line',
                 data: dbboxplot_data.right.line.data,
                 lineWidth: dbboxplot_data.right.line.lineWidth,
                 yAxis: 1
             }
            ],
            exporting: {
                menuItemDefinitions: {
                    fullscreen: {
                        onclick: function () {
                            $('#' + dbboxplot_data.id).parent().toggleClass('chart-modal');
                            $('#' + dbboxplot_data.id).highcharts().reflow();
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
        Highcharts.chart(dbboxplot_data.id, options);
    }
    return {
        init: function(){
            show();
        },
        distribution: function(){
            distribution();
        }
    }
}();