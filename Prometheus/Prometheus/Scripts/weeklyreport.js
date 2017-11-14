var WeeklyReport = function () {
    var show = function () {
        $('.project-name').eq(0).next().removeClass("hidden");
        $('section').eq(0).children(1).attr('aria-expanded', 'true');
        $('section').eq(0).children(2).addClass('in');
        $('body').on('click', '.task-content tbody tr', function () {
            var ikey = $(this).attr('data-ikey');
            var itype = $(this).attr('data-itype');
            appendsummary($(this), itype, ikey);
        })
        $('body').on('click', '.project-name', function () {
            $('.project-info').addClass("hidden");
            $(this).next().removeClass("hidden");
            $(this).next().children('section').eq(0).children(1).attr('aria-expanded', 'true');
            $(this).next().children('section').eq(0).children(2).addClass('in');
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
                            '<td colspan="7">' +
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

        $('body').on('click', '.btn-save', function () {
            var pKey = $(this).attr('data-data-key');
            var sType = $(this).attr('data-data-module');
            var data = new Array();
            if (sType != 0 && sType != 1) {
                var $content = $(this).parent('.wr-operation').prev().find('.even');
                $($content).each(function () {
                    var arrtmp = new Array();
                    var ikey = $(this).attr('data-ikey');
                    var sumstr = $(this).find('textarea').val();
                    var mark = $(this).find('select').val();
                    if (sumstr != '') {
                        arrtmp.push(ikey, mark, sumstr);
                        data.push(arrtmp)
                    }
                })
            }
            else {
                var arrtmp = new Array();
                var sumstr = $(this).parent('.wr-operation').prev().find('textarea').val();
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
                    if (sType == 0) {
                        if ($content.parent('section').next().next().length > 0) {
                            $content.collapse('hide');
                            $content.parent('section').next().next().children('.content').collapse('show');
                        }
                        else {
                            window.location.reload();
                        }
                    }
                    else {
                        if ($content.parent('section').next('section').length > 0) {
                            $content.collapse('hide');
                            $content.parent('section').next('section').children('.content').collapse('show');
                        }
                        else {
                            window.location.reload();
                        }
                    }
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
            var m_yield = ($('#modal_yield').prop("checked")) ? 1 : 0;
            var m_icare = ($('#modal_icare').prop("checked")) ? 1 : 0;
            var m_task = ($('#modal_task').prop("checked"))?1:0;
            var m_criticalfailure = ($('#modal_criticalfailure').prop("checked"))?1:0;
            var m_rma = ($('#modal_rma').prop("checked"))?1:0;
            var m_debugtree = ($('#modal_debugtree').prop("checked"))?1:0;
            var m_others = ($('#modal_others').prop("checked")) ? 1 : 0;
            $.post('/User/SaveWeeklyReportSetting',
            {
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
    }


    return {
        init: function () {
            show();
        }
    };
}();