//(function () {
//    $(function () {
//        $(document).on("click", ".gridly .delete", function (event) {
//            var $this;
//            event.preventDefault();
//            event.stopPropagation();
//            $this = $(this);
//            $this.closest('.brick').remove();
//            return $('.gridly').gridly('layout');
//        });
//        return $('.gridly').gridly();
//    });

//}).call(this);


var ProjectDetail = function () {
    var init = function () {
        $('.gridly').gridly();
        $('body').on('click', '.edit-pro-grid', function () {
            if ($(this).hasClass("fa-toggle-on")) {
                $(this).removeClass("fa-toggle-on").addClass("fa-toggle-off");
            } else {
                $(this).removeClass("fa-toggle-off").addClass("fa-toggle-on");
            }
        })
        $('body').on('click', '#save-sort', function () {
            var sortvals = new Array();
            $('.brick').each(function (i) {
                sortvals.push({
                    x: parseInt($(this).css('left').substring(0, $(this).css('left').length - 2)),
                    y: parseInt($(this).css('top').substring(0, $(this).css('top').length - 2)),
                    val: $(this).data('sort-val'),
                    visible: 1,
                });
            });
            var pKey = $('#pKey').val();
            $.post('/Project/SavePersonalProModule',
            {
                pKey: pKey,
                sortvals: JSON.stringify(sortvals),
            }, function (output) {
                if (output.success) {
                    window.location.reload();
                }
                else {
                    alert("Failed");
                }
            })
        })
    }
    return {
        init: function () {
            init();
        }
    }
}();