var TOP500FA = function () {
    var show = function () {
        var fadatatable = null;

        function searchdata()
        {
            var pjkey = $('#pKey').val();
            if (pjkey == '')
            { return false; }

            var options = {
                loadingTips: "正在处理数据，请稍候...",
                backgroundColor: "#aaa",
                borderColor: "#fff",
                opacity: 0.8,
                borderColor: "#fff",
                TipsColor: "#000",
            }
            $.bootstrapLoading.start(options);

            $.post('/Project/ProjectTop500FAData',
               {
                   pjkey: pjkey
               }, function (output) {
                   $.bootstrapLoading.end();

                   if (fadatatable) {
                       fadatatable.destroy();
                       fadatatable = null;
                   }

                   if (output.hastop500)
                   {
                       $('#top500label').removeClass('top500warning').addClass('top500sucess');
                   }

                   $("#fatablecontent").empty();
                   
                   $.each(output.issuelist, function (i, val) {
                       var appendstr = '<tr>';
                       appendstr += '<td><a href="/Issue/UpdateIssue?issuekey=' + val.IssueKey + '" target="_blank">' + val.Summary + '</a></td>';
                       appendstr += '<td>' + val.Assignee + '</td>';

                       if (val.Resolution.indexOf('Pending') != -1 || val.Resolution.indexOf('Reopen') != -1) {
                           appendstr += '<td><font color="red">' + val.Resolution + '</font></td>';
                       }else if (val.Resolution.indexOf('Working') != -1) {
                           appendstr += '<td><font color="orange">' + val.Resolution + '</font></td>';
                       }
                       else {
                           appendstr += '<td><font color="green">' + val.Resolution + '</font></td>';
                       }

                       appendstr += '<td>' + val.ErrAbbr + '</td>';
                       appendstr += '<td>' + val.DueDateStr + '</td>';
                       appendstr += '</tr>';
                       $("#fatablecontent").append(appendstr);
                   });


                   fadatatable = $('#fadatatable').DataTable({
                       'iDisplayLength': 50,
                       'aLengthMenu': [[20, 50, 100, -1],
                       [20, 50, 100, "All"]],
                       "aaSorting": [],
                       "order": [],
                       dom: 'lBfrtip',
                       buttons: ['copyHtml5', 'csv', 'excelHtml5']
                   });
               });
        }

        $(function () {
            searchdata();
        });
    }
    
    return {
        init: function () {
            show();
        }
    }
}();