var WeeklyReport = function () {
    var show = function () {
        //$('.project-name').eq(0).next().removeClass("hidden");
        //$('section').eq(0).children(1).attr('aria-expanded', 'true');
        //$('section').eq(0).children(2).addClass('in');
        $('body').on('click', '.icare-content tbody tr', function () {
            var ikey = $(this).attr('data-ikey');
            var itype = $(this).attr('data-itype');
            appendsummary($(this), itype, ikey);
        })

        var appendsummary = function (object, itype, ikey) {
            var data_target = object.attr('data-target').substr(1);
            if ($('#' + data_target).length === 0) {
                $.post('/User/GetSummary',
                {
                    sType: itype,
                    iKey: ikey
                }, function (output) {
                    if (output.success && output.data.length > 0) {
                        var summary = "";
                        for (var i = 0; i < output.data.length; i++) {
                            summary += '<div data-id="' + output.data[i].ID + '">' +
                                            '<span class="col-xs-5">' + output.data[i].Summary + '</span>' +
                                            '<span class="col-xs-3">' + output.data[i].UserName.split('@')[0] + '</span>' +
                                            '<span class="col-xs-4">' + output.data[i].UpdateTime + '</span>' +
                                        '</div>';
                        }
                        var appendStr = '<tr>' +
                            '<td colspan="8">' +
                                '<div class="collapse in" id="' + data_target + '">' +
                                    '<div class="row well data-summary">' +
                                        '<label>Summary:</label>' +
                                        summary +
                                    '</div>' +
                                '</div>' +
                            '</td>' +
                        '</tr>';
                        $(appendStr).insertAfter(object);
                    }
                })
            }
        }

        $('body').on('click', '.project-name', function () {
            if ($(this).next().hasClass("hidden")) {
                $('.project-info').addClass("hidden");
                $(this).next().removeClass("hidden");
                //$(this).next().children('section').eq(0).children(1).attr('aria-expanded', 'true');
                //$(this).next().children('section').eq(0).children(2).addClass('in');
                //$(this).next().children('section').eq(0).children(2).removeAttr('style');
            }
            else {
                $(this).next().addClass("hidden");
            }
        })

        $('body').on('click', '.btn-save', function () {
            var pKey = $(this).attr('data-data-key');
            var sType = $(this).attr('data-data-module');
            var data = new Array();
            if (sType == 6) {
                var $content = $(this).parent('.wr-operation').prev().find('.even');
                $($content).each(function () {
                    var arrtmp = new Array();
                    var ikey = $(this).attr('data-ikey');
                    var sumstr = $(this).find('textarea').val().replace(/'/g, ' ');
                    var mark = $(this).find('select').val();
                    if (sumstr != '') {
                        arrtmp.push(ikey, mark, sumstr);
                        data.push(arrtmp)
                    }
                })
            }
            else if (sType != 0 && sType != 1) {
                var $content = $(this).parent('.wr-operation').prev().find('.tr-data');
                $($content).each(function () {
                    var arrtmp = new Array();
                    var ikey = $(this).attr('data-ikey');
                    var sumstr = '';
                    var mark = $(this).find('select').val();
                    var omark = $(this).find('select').prev().val();
                    if (mark != omark) {
                        arrtmp.push(ikey, mark, sumstr);
                        data.push(arrtmp);
                    }
                })
            }
            else {
                var arrtmp = new Array();
                var sumstr = $(this).parent('.wr-operation').prev().find('textarea').val().replace(/'/g, ' ');
                if (sumstr == '') {
                    return false;
                }
                arrtmp.push('', 0, sumstr);
                data.push(arrtmp)
            }
            var $content = $(this).parent('.wr-operation').parent('.content');
            $.post('/User/SaveWeeklyReport',
            {
                pKey: pKey,
                sType: sType,
                data: JSON.stringify(data)
            }, function (output) {
                if (output.success) {
                    alert("Save successfully!");
                }
                else {
                    alert("Failed to save!");
                }
            })

        })

        $('body').on('click', '.btn-cancel', function () {
            $(this).parent('.wr-operation').parent('.content').find('textarea').val('');
        })

        $('body').on('click', '#save_setting', function () {
            var cur_user = $('#current_user').val();
            var m_yield = ($('#modal_yield').prop("checked")) ? 1 : 0;
            var m_icare = ($('#modal_icare').prop("checked")) ? 1 : 0;
            var m_task = ($('#modal_task').prop("checked"))?1:0;
            var m_criticalfailure = ($('#modal_criticalfailure').prop("checked"))?1:0;
            var m_rma = ($('#modal_rma').prop("checked"))?1:0;
            var m_debugtree = ($('#modal_debugtree').prop("checked"))?1:0;
            var m_others = ($('#modal_others').prop("checked")) ? 1 : 0;
            $.post('/User/SaveWeeklyReportSetting',
            {
                cur_user: cur_user,
                m_yield: m_yield,
                m_icare: m_icare,
                m_task: m_task,
                m_criticalfailure: m_criticalfailure,
                m_rma: m_rma,
                m_debugtree: m_debugtree,
                m_others: m_others
            }, function (output) {
                if (output.success) {
                    window.location.reload();
                }
                else {
                    alert("Failed to update!");
                }
            })
        })

        $('body').on('change', '#userreportlist', function () {
            $('#myModal').modal('show');
            window.location.href = '/User/WeeklyReport?username='+$.trim($(this).val());
        })
    }


    return {
        init: function () {
            show();
        }
    };
}();