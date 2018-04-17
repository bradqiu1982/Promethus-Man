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
                         console.log(i);
                         colorStr = '<span class="span-fm label label-success" style="background-color: '+val+'">'+i+'</span>';
                         $('.v-lengend').append(colorStr);
                     })
                }
             })
        })
    }

    var distribution = function(){
        $('.date').datepicker({ autoclose: true, viewMode: "months", minViewMode: "months" });
        $('body').on('click', '#btn-search', function(){
            var sdate = $.trim($('#sdate').val());
            var edate = $.trim($('#edate').val());
            var wf_no = $.trim($('#wf-no').val());
            // $.post('/', {
            //     sdate: sdate,
            //     edate: edate,
            //     wf_no: wf_no
            // }, function(output){
                var output = {
                    success: true,
                    data:{
                        po_ld: boxplot_po_ld,
                        po_uniformity: wafer_po_uniformity,
                        variation_uniformity_pold: variation_uniformity_pold
                    }
                }
                if(output.success){
                    drawboxplot(output.data.po_ld);
                    drawboxplot(output.data.po_uniformity);
                    drawdbboxplot(output.data.variation_uniformity_pold);
                }
            // })
        })
        var boxplot_po_ld = {
            id: 'wafer_po_ld',
            title: 'Delta PO_LD Distribution by Wafer',
            xAxis: {
                title: 'Wafer#',
                data: ['Wafer 1', 'Wafer 2', 'Wafer 3', 'Wafer 4', 'Wafer 5'],
            },
            yAxis: {
                title: 'Value',
                plotLines: {
                    value: '929',
                    color: '#ff3399',
                    text: 'Limit Value'
                }
            },
            data: {
                name: 'Delta PO_LD',
                data: [
                    [760, 801, 848, 895, 965],
                    [733, 853, 939, 980, 1080],
                    [714, 762, 817, 870, 918],
                    [724, 802, 806, 871, 950],
                    [834, 836, 864, 882, 910]
                ]
            },
            outlier: {
                name: 'Outlier',
                data: [
                    [0, 644],
                    [4, 718],
                    [4, 951],
                    [4, 969]
                ],
                color: '#ffc000',
                marker: {
                    fillColor: 'white',
                    lineWidth: 1,
                    lineColor: Highcharts.getOptions().colors[0]
                }
            },
            line: {
                name: 'Line',
                data: [848, 939, 817, 806, 864],
                color: '#ffc000',
                lineWidth: 1,
            }
        }
        var wafer_po_uniformity = {
            id: 'wafer_po_uniformity',
            title: 'Delta Po_Uniformity Distribution by Wafer',
            xAxis: {
                title: 'Wafer#',
                data: ['Wafer 1', 'Wafer 2', 'Wafer 3', 'Wafer 4', 'Wafer 5'],
            },
            yAxis: {
                title: 'Value',
                plotLines: {
                    value: '929',
                    color: '#ff3399',
                    text: 'Limit Value'
                }
            },
            data: {
                name: 'Delta Po_Uniformity',
                data: [
                    [760, 801, 848, 895, 965],
                    [733, 853, 939, 980, 1080],
                    [714, 762, 817, 870, 918],
                    [724, 802, 806, 871, 950],
                    [834, 836, 864, 882, 910]
                ]
            },
            outlier: {
                name: 'Outlier',
                data: [
                    [0, 644],
                    [4, 718],
                    [4, 951],
                    [4, 969]
                ],
                color: '#ffc000',
                marker: {
                    fillColor: 'white',
                    lineWidth: 1,
                    lineColor: Highcharts.getOptions().colors[0]
                }
            },
            line: {
                name: 'Line',
                data: [848, 939, 817, 806, 864],
                color: '#ffc000',
                lineWidth: 1,
            }
        }
        var variation_uniformity_pold = {
            id: 'variation_uniformity_pold',
            title: 'Variation_POLD_Delta & Uniformity_POLD_Delta Distribution by Wafer',
            xAxis: {
                title: 'Wafer#',
                data: ['Wafer 1', 'Wafer 2', 'Wafer 3', 'Wafer 4', 'Wafer 5'],
            },
            left: {
                yAxis: {
                    title: 'Variation_POLD_Delta',
                    plotLines: {
                        value: '929',
                        color: '#ff3399',
                        text: 'Limit Value',
                        style: 'dash'
                    }
                },
                data: {
                    name: 'Variation_POLD_Delta',
                    color: '#00b050',
                    data: [
                        [760, 801, 848, 895, 965],
                        [733, 853, 939, 980, 1080],
                        [714, 762, 817, 870, 918],
                        [724, 802, 806, 871, 950],
                        [834, 836, 864, 882, 910]
                    ]
                },
                outlier: {
                    name: 'Outlier',
                    data: [
                        [0, 644],
                        [4, 718],
                        [4, 951],
                        [4, 969]
                    ],
                    color: '#ffc000',
                    marker: {
                        fillColor: 'white',
                        lineWidth: 1,
                        lineColor: Highcharts.getOptions().colors[0]
                    }
                },
                line: {
                    name: 'Line',
                    data: [848, 939, 817, 806, 864],
                    color: '#ffc000',
                    lineWidth: 1,
                }
            },
            right:{
                yAxis: {
                    title: 'Uniformity_POLD_Delta',
                    plotLines: {
                        value: '980',
                        color: '#C9302C',
                        text: 'High Value',
                        style: 'solid'
                    }
                },
                data:{
                    name: 'Uniformity_POLD_Delta',
                    color: '#0099ff',
                    data: [
                        [834, 836, 864, 882, 910],
                        [714, 762, 817, 870, 918],
                        [724, 802, 806, 871, 950],
                        [760, 801, 848, 895, 965],
                        [733, 853, 939, 980, 1080]
                    ]
                },
                outlier: {
                    name: 'Outlier',
                    data: [
                        [4, 644],
                        [2, 718],
                        [1, 951],
                        [3, 969]
                    ],
                    color: '#ffc000',
                    marker: {
                        fillColor: '#ffc000',
                        lineWidth: 1,
                        lineColor: '#ffc000'
                    }
                },
                line:{
                    name: 'Line',
                    data: [864,817,806,848,939],
                    color: '#C9302C',
                    lineWidth: 1,
                }
            }
        }
    }
    var drawline = function(line_data){
        var options = {
            chart: {
                type: 'line'
            },
            title: {
                text: line_data.title
            },
            xAxis: {
                categories: line_data.xAxis.data
            },
            yAxis: {
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
            },
            series: [{
                name: line_data.data.data.name,
                dataLabels: {
                    enabled: true,
                    color: line_data.data.data.color,
                },
                marker: {
                    radius: 2
                },
                enableMouseTracking: false,
                data: line_data.data.data.data
            }]
        };
        Highcharts.chart(line_data.id, options);
    }
    var drawcolumn = function(col_data){
        var options = {
            chart: {
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
                pointFormat: '<span style="color:{point.color}">{point.name}</span>: <b>{point.percentage:.0f}%</b><br/>',
                shared: true
            },
            plotOptions: {
                column: {
                    stacking: 'percent'
                }
            },
            series: col_data.data
        };
        Highcharts.chart(col_data.id, options);
    }
    var drawboxplot = function(boxplot_data){
        var options = {
            chart: {
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
                },
                plotLines: [{
                    value: boxplot_data.yAxis.plotLines.value,
                    color: boxplot_data.yAxis.plotLines.color,
                    width: 1,
                    label: {
                        text: boxplot_data.yAxis.plotLines.text,
                        align: 'left',
                        style: {
                            color: 'gray'
                        }
                    }
                }]
            },

            series: [{
                name: boxplot_data.data.name,
                data: boxplot_data.data.data,
                tooltip: {
                    headerFormat: '<em>{point.key}</em><br/>'
                }
            }, {
                name: boxplot_data.outlier.name,
                color: boxplot_data.outlier.color,
                type: 'scatter',
                data: boxplot_data.outlier.data,
                marker: {
                    fillColor: boxplot_data.outlier.marker.fillColor,
                    lineWidth: boxplot_data.outlier.marker.lineWidth,
                    lineColor: boxplot_data.outlier.marker.lineColor,
                },
                tooltip: {
                    pointFormat: '{point.y}'
                }
            },{
                name: boxplot_data.line.name,
                color: boxplot_data.line.color,
                type: 'line',
                data: boxplot_data.line.data,
                lineWidth: boxplot_data.line.lineWidth
            }]
        };
        Highcharts.chart(boxplot_data.id, options);
    }
    var drawdbboxplot = function(dbboxplot_data){
        var options = {
            chart: {
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
                },
                plotLines: [{
                    value: dbboxplot_data.left.yAxis.plotLines.value,
                    color: dbboxplot_data.left.yAxis.plotLines.color,
                    width: 1,
                    dashStyle: dbboxplot_data.left.yAxis.plotLines.style,
                    label: {
                        text: dbboxplot_data.left.yAxis.plotLines.text,
                        align: 'left',
                        style: {
                            color: 'gray'
                        }
                    }
                }]
            },{
                opposite: true,
                title: {
                    text: dbboxplot_data.right.yAxis.title
                },
                plotLines: [{
                    value: dbboxplot_data.right.yAxis.plotLines.value,
                    color: dbboxplot_data.right.yAxis.plotLines.color,
                    width: 1,
                    dashStyle: dbboxplot_data.right.yAxis.plotLines.style,
                    label: {
                        text: dbboxplot_data.right.yAxis.plotLines.text,
                        align: 'right',
                        style: {
                            color: 'gray'
                        }
                    }
                }]
            }],

            series: [{
                name: dbboxplot_data.left.data.name,
                data: dbboxplot_data.left.data.data,
                color: dbboxplot_data.left.data.color,
                tooltip: {
                    headerFormat: '<em>{point.key}</em><br/>'
                }
            }, {
                name: dbboxplot_data.left.outlier.name,
                color: dbboxplot_data.left.outlier.color,
                type: 'scatter',
                data: dbboxplot_data.left.outlier.data,
                marker: {
                    fillColor: dbboxplot_data.left.outlier.marker.fillColor,
                    lineWidth: dbboxplot_data.left.outlier.marker.lineWidth,
                    lineColor: dbboxplot_data.left.outlier.marker.lineColor,
                },
                tooltip: {
                    pointFormat: '{point.y}'
                }
            // },{
            //     name: dbboxplot_data.left.line.name,
            //     color: dbboxplot_data.left.line.color,
            //     type: 'line',
            //     data: dbboxplot_data.left.line.data,
            //     lineWidth: dbboxplot_data.left.line.lineWidth
            },{
                name: dbboxplot_data.right.data.name,
                data: dbboxplot_data.right.data.data,
                color: dbboxplot_data.right.data.color,
                tooltip: {
                    headerFormat: '<em>{point.key}</em><br/>'
                },
                yAxis: 1
            }, {
                name: dbboxplot_data.right.outlier.name,
                color: dbboxplot_data.right.outlier.color,
                type: 'scatter',
                data: dbboxplot_data.right.outlier.data,
                marker: {
                    fillColor: dbboxplot_data.right.outlier.marker.fillColor,
                    lineWidth: dbboxplot_data.right.outlier.marker.lineWidth,
                    lineColor: dbboxplot_data.right.outlier.marker.lineColor,
                },
                tooltip: {
                    pointFormat: '{point.y}'
                },
                yAxis: 1
            }
            // ,{
            //     name: dbboxplot_data.right.line.name,
            //     color: dbboxplot_data.right.line.color,
            //     type: 'line',
            //     data: dbboxplot_data.right.line.data,
            //     lineWidth: dbboxplot_data.right.line.lineWidth,
            //     yAxis: 1
            // }
            ]
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