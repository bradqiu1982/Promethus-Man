var ProMilestones = function(){
    var init = function(){
        var chartdata = {
            id: 'pro-charts',
            title: 'Project Detail',
            xAxis: {
                data: ['2016-08', '2016-09', '2016-10', '2016-11', '2016-12', '2017-01', '2017-02', '2017-03', '2017-04', '2017-05', '2017-06', '2017-07', '2017-08', '2017-09', '2017-10', '2017-11', '2017-12', '2018-01', '2018-02', '2018-03', '2018-04', '2018-05']
            },
            yAxis: {
                0: {
                    title: 'Yield (%)',
                    min: 0,
                    max: 100,
                },
                1: {
                    title: 'Amount',
                },
            },
            data: {
                yield_data:{
                    name: 'Yield',
                    color: '#5CB85C',
                    data: [94.5, 91, 94.5, 97, 94.5, 94.5, 91, 94.5, 97, 94.5, 94.5, 91, 94.5, 97, 94.5, 95, 94.5, 92, 94.5, 85, 94.5, 93]
                },
                amount_data:{
                    name: 'Amount',
                    color: '#ff3399',
                    data: [1091, 809, 1293, 1001, 800, 900, 1120, 1091, 809, 1293, 900, 1120, 1091, 809, 1293, 1001, 800, 987, 920, 1120, 1032, 1409]
                },
                rma_data:{
                    name: 'RMA',
                    color: '#00b050',
                    data:[
                        {
                            x: 0, 
                            y: 20,
                            name: 'CDR',
                        },
                        {
                            x: 2, 
                            y: 15,
                            name: 'CDR',

                        },{
                            x: 3, 
                            y: 15,
                            name: 'VCSEL',
                        },{
                            x: 3, 
                            y: 25,
                            name: 'CDR',
                            }
                    ]
                },
                plotBands:[{
                    color: '#efffff',
                    from: 14.5,
                    to: 15.5,
                    label: {
                        text: '* action',
                        style: {
                            color: '#000',
                            fontSize: '0px',
                        },
                        y: 50.0,
                        align:'left'
                    },
                },{
                    color: '#ffffef',
                    from: 16.5,
                    to: 17.5,
                    label: {
                        text: '* Add public action',
                        style: {
                            color: '#000',
                            fontSize: '0px',
                        },
                        y: 62.0,
                        align: 'left'
                    },
                },{
                    color: '#ffefff',
                    from: 17.5,
                    to: 18.5,
                    label: {
                        text: '* Add public action',
                        style: {
                            color: '#000',
                            fontSize: '0px',
                        },
                        y: 74.0,
                        align: 'left'
                    },
                }],
            }
        };
        $('.date').datepicker({ autoclose: true });
        $.post('/Project/GetProMileStoneData',
        {
            pKey: $.trim($('#pKey').val())
        }, function (output) {
            output = {
                data: chartdata
            };
            drawchart(output.data);
         })
        $('body').on('click', '.add-action', function(){
            $('#m-actionid').val('');
            $('#m-date').val('');
            $('#m-action').val('');
            $('input[name=m-type]').prop('checked', false);
            $('#modal-milestone-edit').modal("show");
        })

        $('body').on('click', '.edit-actions', function(){
            var actionid = $(this).parent().parent().attr("data-actionid");
            var date = $(this).parent().parent().find('.l-date').html();
            var action = $(this).parent().parent().find('.l-content').html();
            var ispublish = $(this).prev().attr('data-ispublish');

            $('#m-actionid').val(actionid);
            $('#m-date').val(date);
            $('#m-action').val(action);
            $('input[name=m-type]').each(function(){
                if($(this).val() == ispublish){
                    $(this).prop('checked', true);
                }
            })
            $('#modal-milestone-edit').modal("show");
        })

        $('body').on('click', '#m-save', function () {
            var pKey = $.trim($('#pKey').val());
            var actionid = $('#m-actionid').val();
            var date = $('#m-date').val();
            var action = $('#m-action').val();
            var ispublish = $('input[name=m-type]:checked').val();
            if(date == '' || action == '' || ispublish == undefined){
                alert('please input required info');
                return false;
            }
            $.post('/Project/AddProMileStone', {
                 pKey: pKey,
                 actionid: actionid,
                 date: date,
                 action: action,
                 ispublish: ispublish,
             }, function(output){
                 if (output.success) {
                     window.location.reload();
                 }
             });
        })

        $('body').on('click', '.invalid-actions', function(){
            if(confirm("Really to invalid this action?")){
                var actionid = $(this).parent().parent().attr("data-actionid");
                $.post('/Project/InvalidProMileStone', {
                    actionid: actionid
                }, function(output){
                    if (output.success) {
                        window.location.reload();
                    }
                })
            }
        })

        $('body').on('click', '.public-actions', function () {
            var actionid = $(this).parent().parent().attr("data-actionid");
            var ispublish = $(this).attr('data-ispublish');
            $.post('/Project/UpdateProMileStone', {
                actionid: actionid,
                ispublish: ispublish
            }, function (output) {
                if (output.success) {
                    window.location.reload();
                }
                else {
                    alert("Failed to operate");
                }
            })
        })

        $('body').on('click', '#btn-search', function(){
            var sDate = $.trim($('#sdate').val());
            var eDate = $.trim($('#edate').val());
            var pKey = $.trim($('#pKey').val());
            window.location.href = "/Project/ProjectMileStone?ProjectKey=" + pKey + "&sDate=" + sDate + "&eDate=" + eDate;
        })

        $('body').on('click', '#btn-clear', function(){
            $('#sdate').val('');
            $('#edate').val('');
            var pKey = $.trim($('#pKey').val());
            window.location.href = "/Project/ProjectMileStone?ProjectKey=" + pKey;
        })

        function drawchart(data){
            var options = {
                chart: {
                    type: 'line',
                    zoomType: 'x',
                },
                title: {
                    text: data.title
                },
                xAxis: {
                    categories: data.xAxis.data,
                    plotBands: data.data.plotBands,
                    events: {
                        afterSetExtremes: function (event) {
                            if ((event.max - event.min) < 7) {
                                $(this.options.plotBands).each(function () {
                                    this.label.style.fontSize = '14px';
                                });
                                this.chart.xAxis[0].update();
                            }
                            else {
                                $(this.options.plotBands).each(function () {
                                    this.label.style.fontSize = '0px';
                                });
                                this.chart.xAxis[0].update();
                            }
                        }
                    }
                },
                yAxis: [{
                    title: {
                        text: data.yAxis[0].title
                    },
                    min: data.yAxis[0].min,
                    max: data.yAxis[0].max
                },
                {
                    title: {
                        text: data.yAxis[1].title
                    },
                    opposite: true,
                }],
                series: [{
                    name: data.data.yield_data.name,
                    dataLabels: {
                        enabled: true,
                        color: data.data.yield_data.color,
                    },
                    marker: {
                        radius: 2
                    },
                    enableMouseTracking: false,
                    data: data.data.yield_data.data
                },{
                    type: 'line',
                    yAxis: 1,
                    name: data.data.amount_data.name,
                    color: data.data.amount_data.color,
                    marker: {
                        radius: 3
                    },
                    data: data.data.amount_data.data,
                },{
                    type: 'scatter',
                    color: data.data.rma_data.color,
                    name: data.data.rma_data.name,
                    enableMouseTracking: false,
                    dataLabels:{
                        enabled: true,
                        align: 'left',
                        verticalAlign: 'middle',
                        formatter: function(){
                            return this.point.name;
                        },
                        style: {
                            color: '#555',
                            fontSize: '8px',
                            fontWeight: 'bold',
                            textOutline: null,
                        },
                    },
                    marker: {
                        symbol: 'diamond'
                    },
                    data: data.data.rma_data.data
                }],
                exporting: {
                    menuItemDefinitions: {
                        fullscreen: {
                            onclick: function () {
                                $('#' + data.id).parent().toggleClass('chart-modal');
                                $('#' + data.id).highcharts().reflow();
                            },
                            text: 'Full Screen'
                        },
                        exportdata: {
                            onclick: function () {
                                var filename = data.title + '.csv';
                                var outputCSV = 'Date,Output,Yield\r\n';
                                $(data.xAxis.data).each(function (i, val) {
                                    outputCSV += val + "," + data.data.amount_data.data[i]
                                        + "," + data.data.yield_data.data[i] + ",\r\n";
                                });
                                outputCSV += "\r\n\r\n";
                                outputCSV += "Date,RMA RootCause,\r\n";
                                $(data.data.rma_data.data).each(function (i, val) {
                                    outputCSV += data.xAxis.data[val.x] + "," + val.name + ",\r\n";
                                });
                                outputCSV += "\r\n\r\n";
                                outputCSV += "Date,MileStones,\r\n";
                                $(data.data.plotBands).each(function () {
                                    outputCSV += data.xAxis.data[this.from + 0.5] + "," + this.label.text.replace("<br/>", " ") + ",\r\n";
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
            Highcharts.chart(data.id, options);
        }
    }
    return {
        init: function(){
            init();
        }
    }
}();