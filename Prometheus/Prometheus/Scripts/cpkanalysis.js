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
            fillproject();
        });

        function initpj()
        {
            $('#projectlist').val('');
            $('#mestablelist').val('');
            $('#pnlist').val('');
            $('#queryparam').val('');
            $('#lowlimit').val('');
            $('#highlimit').val('');
            $('#StartDate').val('');
            $('#EndDate').val('');
                
            $('#mestablelist').attr('readonly', true);
            $('#pnlist').attr('readonly', true);
            $('#queryparam').attr('readonly', true);
            $('#lowlimit').attr('readonly', true);
            $('#highlimit').attr('readonly', true);
        }

        function initmes()
        {
            $('#mestablelist').val('');
            $('#pnlist').val('');
            $('#queryparam').val('');
            $('#lowlimit').val('');
            $('#highlimit').val('');

            $('#queryparam').attr('readonly', true);
            $('#lowlimit').attr('readonly', true);
            $('#highlimit').attr('readonly', true);
        }

        function initparam()
        {
            $('#queryparam').val('');
            $('#lowlimit').val('');
            $('#highlimit').val('');

            $('#lowlimit').attr('readonly', true);
            $('#highlimit').attr('readonly', true);
        }

        function fillproject()
        {
            initpj();

            $.post('/DataAnalyze/GetStandardPJList', {}, function (output)
            {
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

        $('#lowlimit,#highlimit').focus(function (event) {
            var readonly = $('#lowlimit').attr('readonly');
            var param = $('#queryparam').val();
            if (param != '' && readonly)
            {
                enablelimit();
            }
        });

        function enablelimit()
        {
            $('#lowlimit').val('');
            $('#highlimit').val('');
            var param = $('#queryparam').val();
            if (param != '')
            {
                $.post('/DataAnalyze/GetMESLimit',
                    { param: param },
                    function (output) {
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
            $('#lowlimit').attr('readonly', false);
            $('#highlimit').attr('readonly', false);
        }

        $('body').on('click', '#QueryCPK', function () {

            var pj = $('#projectlist').val();
            var mestab = $('#mestablelist').val();
            var param = $('#queryparam').val();
            var lowlimit = $('#lowlimit').val();
            var highlimit = $('#highlimit').val();
            var startdate = $('#StartDate').val();
            var enddate = $('#EndDate').val();

            var pnlist = $.trim($('#pnlist').tagsinput('items'));
            if (pnlist == '') {
                pnlist = $.trim($('#pnlist').parent().find('input').eq(0).val());
            }

            if (pj == ''
                || mestab == ''
                || param == ''
                || (lowlimit == '' && highlimit == '')
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
                lowlimit: lowlimit,
                highlimit: highlimit,
                startdate: startdate,
                enddate: enddate,
                pnlist: pnlist,
                pass: allpasslist,
                database: databaselist
            },
            function (output) {

            });
        });

    }

    return {
        init: function () {
            show();
        }
    }
}();