var WeeklyReport = function () {
    var show = function () {
        $('body').on('click', '.task-content tbody tr', function () {
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
                    $content.collapse('hide');
                    $content.parent('section').next('section').children('.content').collapse('show');
                }
                else {
                    alert("Failed to save!");
                }
            })

        })

        $('body').on('click', '.btn-cancel', function () {
            $(this).parent('.wr-operation').parent('.content').find('textarea').val('');
        })
    }


    return {
        init: function () {
            show();
        }
    };
}();