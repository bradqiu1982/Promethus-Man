var CPKANALYSE = function () {
    var show = function () {

        $("#StartDate").datepicker({
            changeMonth: true,
            changeYear: true,
            dateFormat: 'yy-mm-dd'
        });
        $("#EndDate").datepicker({
            changeMonth: true,
            changeYear: true,
            dateFormat: 'yy-mm-dd'
        });

        $(function () {
            initpj();
            fillproject();
        });

        function initpj()
        {
            $('#projectlist').val('');
            $('#mestablelist').val('');
            $('#pnlist').val('');
            $('#queryparam').val('');
            $('#cornid').val('');
            $('#lowlimit').val('');
            $('#highlimit').val('');
            $('#StartDate').val('');
            $('#EndDate').val('');
            $('.cpkoutcla').val('');
            $('.v-content').empty();
            $('#sourcedata').attr('href', '#');
        }

        function initmes()
        {
            $('#mestablelist').val('');
            $('#pnlist').val('');
            $('#queryparam').val('');
            $('#cornid').val('');
            $('#lowlimit').val('');
            $('#highlimit').val('');

            $('#queryparam').attr('readonly', true);
            $('#cornid').attr('readonly', true);
            $('#lowlimit').attr('readonly', true);
            $('#highlimit').attr('readonly', true);
        }

        function initparam()
        {
            $('#queryparam').val('');
            $('#cornid').val('');
            $('#lowlimit').val('');
            $('#highlimit').val('');

            $('#cornid').attr('readonly', true);
            $('#lowlimit').attr('readonly', true);
            $('#highlimit').attr('readonly', true);
        }

        function fillproject()
        {
            $('#mestablelist').attr('readonly', true);
            $('#pnlist').attr('readonly', true);
            $('#queryparam').attr('readonly', true);
            $('#cornid').attr('readonly', true);
            $('#lowlimit').attr('readonly', true);
            $('#highlimit').attr('readonly', true);

            $.post('/DataAnalyze/GetStandardPJList', {}, function (output)
            {
                if ($('#projectlist').data('autocomplete') || $('#projectlist').data('uiAutocomplete')) {
                    $('#projectlist').autoComplete("destroy");
                }
                $('#projectlist').autoComplete({
                    minChars: 0,
                    source: function (term, suggest) {
                        term = term.toLowerCase();
                        var choices = output.pjdata;
                        var suggestions = [];
                        for (i = 0; i < choices.length; i++)
                            if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                        suggest(suggestions);
                    },
                    onSelect: function (event, term, item) {
                        enablemestabpn();
                    }
                });
            })
        }

        $('#projectlist').keyup(function (event) {
            if (event.keyCode === 13) {
                enablemestabpn();
            }
        });

        $('#mestablelist,#pnlist').focus(function (event) {
            var pj = $('#projectlist').val();
            var readonly = $('#mestablelist').attr('readonly');
            if (pj != '' && readonly)
            {
                enablemestabpn();
            }
        });

        function enablemestabpn()
        {
            var pj = $('#projectlist').val();
            if (pj != '')
            {
                //alert('enablemestabpn()');

                initmes();

                $.post('/DataAnalyze/GetMESTabPN',
                    { pj:pj },
                    function (output)
                    {
                        if ($('#mestablelist').data('autocomplete') || $('#mestablelist').data('uiAutocomplete')) {
                            $('#mestablelist').autoComplete("destroy");
                        }
                        $('#mestablelist').autoComplete({
                            minChars: 0,
                            source: function (term, suggest) {
                                term = term.toLowerCase();
                                var choices = output.mestablist;
                                var suggestions = [];
                                for (i = 0; i < choices.length; i++)
                                    if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                                suggest(suggestions);
                            },
                            onSelect: function (event, term, item) {
                                enableparam();
                            }
                        });

                        $('#pnlist').tagsinput({
                            freeInput: false,
                            typeahead: {
                                source: output.pnlist,
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
                    });

                $('#mestablelist').attr('readonly', false);
                $('#pnlist').attr('readonly', false);
            }
        }

        $('#mestablelist').keyup(function (event) {
            if (event.keyCode === 13) {
                enableparam();
            }
        });

        $('#queryparam').focus(function (event) {
            var mestab = $('#mestablelist').val();
            var readonly = $('#queryparam').attr('readonly');
            if (mestab != '' && readonly)
            {
                enableparam();
            }
        });

        function enableparam() {
            var mestab = $('#mestablelist').val();
            if (mestab != '') {
                //alert('enableparam()');

                initparam();

                $.post('/DataAnalyze/GetMESParam',
                    { mestab: mestab },
                    function (output) {
                        if ($('#queryparam').data('autocomplete') || $('#queryparam').data('uiAutocomplete')) {
                            $('#queryparam').autoComplete("destroy");
                        }
                        $('#queryparam').autoComplete({
                            minChars: 0,
                            source: function (term, suggest) {
                                term = term.toLowerCase();
                                var choices = output.paramlist;
                                var suggestions = [];
                                for (i = 0; i < choices.length; i++)
                                    if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                                suggest(suggestions);
                            },
                            onSelect: function (event, term, item) {
                                enablelimit();
                            }
                        });
                    });

                $('#queryparam').attr('readonly', false);
            }
        }

        $('#queryparam').keyup(function (event) {
            if (event.keyCode === 13) {
                enablelimit();
            }
        });

        $('#lowlimit,#highlimit,#cornid').focus(function (event) {
            var readonly = $('#lowlimit').attr('readonly');
            var param = $('#queryparam').val();
            if (param != '' && readonly)
            {
                enablelimit();
            }
        });

        function enablelimit()
        {
            $('#cornid').val('');
            $('#lowlimit').val('');
            $('#highlimit').val('');
            var param = $('#queryparam').val();
            if (param != '')
            {
                $.post('/DataAnalyze/GetMESLimit',
                    { param: param },
                    function (output) {
                        if ($('#cornid').data('autocomplete') || $('#cornid').data('uiAutocomplete'))
                        {
                            $('#cornid').autoComplete("destroy");
                            $('#lowlimit').autoComplete("destroy");
                            $('#highlimit').autoComplete("destroy");
                        }

                        $('#cornid').autoComplete({
                            minChars: 0,
                            source: function (term, suggest) {
                                term = term.toLowerCase();
                                var choices = output.cornlist;
                                var suggestions = [];
                                for (i = 0; i < choices.length; i++)
                                    if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                                suggest(suggestions);
                            }
                        });
                        $('#lowlimit').autoComplete({
                            minChars: 0,
                            source: function (term, suggest) {
                                term = term.toLowerCase();
                                var choices = output.lowlimitlist;
                                var suggestions = [];
                                for (i = 0; i < choices.length; i++)
                                    if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                                suggest(suggestions);
                            }
                        });
                        $('#highlimit').autoComplete({
                            minChars: 0,
                            source: function (term, suggest) {
                                term = term.toLowerCase();
                                var choices = output.highlimitlist;
                                var suggestions = [];
                                for (i = 0; i < choices.length; i++)
                                    if (~choices[i].toLowerCase().indexOf(term)) suggestions.push(choices[i]);
                                suggest(suggestions);
                            }
                        });
                    });
            }
            $('#cornid').attr('readonly', false);
            $('#lowlimit').attr('readonly', false);
            $('#highlimit').attr('readonly', false);
        }

        $('body').on('click', '#QueryCPK', function () {

            var pj = $('#projectlist').val();
            var mestab = $('#mestablelist').val();
            var param = $('#queryparam').val();
            var lowlimit = $('#lowlimit').val();
            var cornid = $('#cornid').val();
            var highlimit = $('#highlimit').val();
            var startdate = $('#StartDate').val();
            var enddate = $('#EndDate').val();

            var pnlist = $.trim($('#pnlist').tagsinput('items'));
            if (pnlist == '') {
                pnlist = $.trim($('#pnlist').parent().find('input').eq(0).val());
                if (pnlist.indexOf(',') != -1)
                {
                    alert('Character , should not exist in PN DES!');
                    return false;
                }
            }

            if (pj == ''
                || mestab == ''
                || param == ''
                || (lowlimit == '' && highlimit == '')
                || cornid == ''
                || startdate == ''
                || enddate == ''
                || pnlist == '') {
                alert('Please complete your query condition!');
                return false;
            }

            var allpasslist = $('#allpasslist').val();
            var databaselist = $('#databaselist').val();

            $.post('/DataAnalyze/QueryCPK', {
                pj: pj,
                mestab: mestab,
                param: param,
                cornid: cornid,
                lowlimit: lowlimit,
                highlimit: highlimit,
                startdate: startdate,
                enddate: enddate,
                pnlist: pnlist,
                pass: allpasslist,
                database: databaselist
            },
            function (output) {
                $('.cpkoutcla').val('');
                $('.v-content').empty();
                $('#sourcedata').attr('href', '#');

                if (output.success) {
                    $('#datafrom').val(output.datafrom);
                    $('#isnormal').val(output.isnormal);
                    $('#mean').val(output.mean);
                    $('#stddev').val(output.stddev);
                    $('#realcpk').val(output.realcpk);
                    $('#dppm').val(output.dppm);

                    $('#probmin').val(output.probmin);
                    $('#probmax').val(output.probmax);
                    $('#gcpkmin').val(output.gcpkmin);
                    $('#gcpkmax').val(output.gcpkmax);
                    $('#rcpkmin').val(output.rcpkmin);
                    $('#rcpkmax').val(output.rcpkmax);
                    $('#rdppmmin').val(output.rdppmmin);
                    $('#rdppmmax').val(output.rdppmmax);

                    $('#meanmin').val(output.meanmin);
                    $('#meanmax').val(output.meanmax);

                    $('#stddevmin').val(output.stddevmin);
                    $('#stddevmax').val(output.stddevmax);

                    $('#cpkmin').val(output.cpkmin);
                    $('#cpkmax').val(output.cpkmax);
                    $('#cpkmax').val(output.cpkmax);
                    $('#sourcedata').attr('href', output.sourcedata);

                    $.each(output.chartlist, function (i, val) {
                         appendstr = '<div class="col-xs-12">' +
                                '<div class="v-box" id="' + val.id + '"></div>' +
                                '</div>';
                         $('.v-content').append(appendstr);
                         drawcolumn(val);
                     })
                }
                else {
                    alert(output.msg);
                }
                fillproject();
            });
        });

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
                min:col_data.xmin,
                max: col_data.xmax,
                plotLines: [{
                    value: col_data.mean,
                    color: 'gray',
                    dashStyle: 'shortdash',
                    width: 2,
                    label: {
                        text: 'Mean'
                    }
                }, {
                    value: col_data.left3stddev,
                    color: 'red',
                    dashStyle: 'shortdash',
                    width: 2,
                    label: {
                        text: '3-sigma'
                    }
                }, {
                    value: col_data.right3stddev,
                    color: 'red',
                    dashStyle: 'shortdash',
                    width: 2,
                    label: {
                        text: '3-sigma'
                    }
                }, {
                    value: col_data.lowbound,
                    color: 'green',
                    dashStyle: 'shortdash',
                    width: 2,
                    label: {
                        text: 'low bound'
                    }
                }, {
                    value: col_data.upperbound,
                    color: 'green',
                    dashStyle: 'shortdash',
                    width: 2,
                    label: {
                        text: 'upper bound'
                    }
                }]
            },
            legend: {
                enabled: false,
            },
            yAxis: {
                title: {
                    text:'Frequence'
                }
            },
            tooltip: {
                pointFormat: '{point.x} : <b>{point.y}</b>'
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
                        menuItems: ['fullscreen', 'datalabel', 'copycharts', 'printChart', 'separator', 'downloadPNG', 'downloadJPEG', 'downloadPDF', 'downloadSVG']
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