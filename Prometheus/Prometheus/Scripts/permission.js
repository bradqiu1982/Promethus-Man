var UserPermission = function () {
    var memberlist = function () {
        $('body').on('click', '#btn-add-member', function () {
            $('#m-uid').val('');
            $('#modal-jobnum').val('');
            $('#modal-mail').val('');
            $('#modal-name').val('');
            $('#modal-tel').val('');
            $('#m-title').html('Add Member');
            $('#modal-add-member').modal('show');
        })
        $('body').on('click', '#m-btn-submit', function () {
            var uid = $('#m-uid').val();
            var jobnum = $('#modal-jobnum').val();
            var mail = $('#modal-mail').val();
            var name = $('#modal-name').val();
            var tel = $('#modal-tel').val();
            if (mail == '') {
                alert("Please input email!");
                return false;
            }
            if (uid == '') {
                $.post('/Permission/AddMember', {
                    jobnum: jobnum,
                    mail: mail,
                    name: name,
                    tel: tel
                }, function (output) {
                    if (output.success) {
                        window.location.reload();
                    }
                    else {
                        alert("Failed to Add Member!");
                    }
                })
            }
            else {
                $.post('/Permission/UpdateMember', {
                    uid: uid,
                    jobnum: jobnum,
                    mail: mail,
                    name: name,
                    tel: tel
                }, function (output) {
                    if (output.success) {
                        window.location.reload();
                    }
                    else {
                        alert("Failed to Update Member!");
                    }
                })

            }
        })
        $('body').on('click', '.pages', function () {
            var keywords = $.trim($('#tb-search').val());
            if (keywords != "") {
                window.location.href = '/Permission/MemberList?pageno=' + $(this).attr('data-data') + "&keywords=" + keywords;
            }
            else {
                window.location.href = '/Permission/MemberList?pageno=' + $(this).attr('data-data');
            }
        })
        $('body').on('click', '.del-member', function () {
            if (confirm("Do you really want to delete this user ?")) {
                var user_id = $(this).attr('data-id');
                if (user_id) {
                    $.post('/Permission/DelMember', {
                        user_id: user_id
                    }, function (output) {
                        if (output.success) {
                            window.location.href = "/Permission/MemberList";
                        }
                        else {
                            alert("Failed to delete");
                        }
                    })
                }
            }
        })
        $('body').on('click', '.edit-member', function () {
            var uid = $(this).attr('data-id');
            var jobnum = $(this).parent().parent().children().eq(0).html();
            var mail = $(this).parent().parent().children().eq(1).html();
            var name = $(this).parent().parent().children().eq(2).html();
            var tel = $(this).parent().parent().children().eq(3).html();

            $('#m-uid').val(uid);
            $('#modal-jobnum').val(jobnum);
            $('#modal-mail').val(mail);
            $('#modal-name').val(name);
            $('#modal-tel').val(tel);

            $('#m-title').html('Edit Member');
            $('#modal-add-member').modal('show');
        })
        $('body').on('click', '#m-btn-search', function () {
            var keywords = $.trim($('#tb-search').val());
            if (keywords == "") {
                alert("Please input search keywords!");
                return false;
            }
            window.location.href = '/Permission/MemberList?pageno=1&keywords=' + keywords;
        })
    }
    var grouplist = function () {
        $('body').on('click', '#btn-add-group', function () {
            $('#m-gid').val('');
            $('#modal-gname').val('');
            $('#modal-comment').val('');
            $('#m-title').html('Add Group');
            $('#modal-add-group').modal('show');
        })
        $('body').on('click', '#m-btn-submit', function () {
            var gid = $('#m-gid').val();
            var gname = $('#modal-gname').val();
            var comment = $('#modal-comment').val();
            if (gname == '') {
                alert("Please input group name!");
                return false;
            }
            if (gid == '') {
                $.post('/Permission/AddGroup', {
                    gname: gname,
                    comment: comment
                }, function (output) {
                    if (output.success) {
                        window.location.reload();
                    }
                    else {
                        alert("Failed to Add Group!");
                    }
                })
            }
            else {
                $.post('/Permission/UpdateGroup', {
                    gid: gid,
                    gname: gname,
                    comment: comment
                }, function (output) {
                    if (output.success) {
                        window.location.reload();
                    }
                    else {
                        alert("Failed to Update Group!");
                    }
                })

            }
        })
        $('body').on('click', '.pages', function () {
            var keywords = $.trim($('#tb-search').val());
            if (keywords != "") {
                window.location.href = '/Permission/GroupList?pageno=' + $(this).attr('data-data') + "&keywords=" + keywords;
            }
            else {
                window.location.href = '/Permission/GroupList?pageno=' + $(this).attr('data-data');
            }
        })
        $('body').on('click', '.del-group', function () {
            if (confirm("Do you really want to delete this group, if you delete this group, the members under this group will be remove too ?")) {
                var gid = $(this).attr('data-id');
                if (gid) {
                    $.post('/Permission/DelGroup', {
                        gid: gid
                    }, function (output) {
                        if (output.success) {
                            window.location.href = "/Permission/GroupList";
                        }
                        else {
                            alert("Failed to delete");
                        }
                    })
                }
            }
        })
        $('body').on('click', '.edit-group', function () {
            var gid = $(this).attr('data-id');
            var gname = $(this).parent().parent().children().eq(1).html();
            var comment = $(this).parent().parent().children().eq(2).html();

            $('#m-gid').val(gid);
            $('#modal-gname').val(gname);
            $('#modal-comment').val(comment);

            $('#m-title').html('Edit Group');
            $('#modal-add-group').modal('show');
        })
        $('body').on('click', '#g-btn-search', function () {
            var keywords = $.trim($('#tb-search').val());
            if (keywords == "") {
                alert("Please input search keywords!");
                return false;
            }
            window.location.href = '/Permission/GroupList?pageno=1&keywords=' + keywords;
        })
        $('body').on('click', '.edit-gmember', function () {
            var gid = $(this).attr('data-id');
            $('#eg-gid').val(gid);
            $('.group-members').empty();
            $.post('/Permission/GetMembers',
                {
                    gid: gid
                }, function (output) {
                    if (output.success) {
                        if (output.gMembers.length > 0) {
                            $.each(output.gMembers, function (i, val) {
                                $('.group-members').append(
                                    '<div class="group-member-panel" data-val="' + val.ID + '" data-name="' + val.Name + '">' +
                                    '<span>' + val.Name + '</span>' +
                                    '<span class="glyphicon glyphicon-remove del-group-member"></span>' +
                                    '</div >'
                                );
                            })
                        }
                        if (output.aMembers.length > 0) {
                            autoCompleteFill('eg-memlist', output.aMembers);
                        }
                    }
                });
            $('#modal-edit-member').modal('show');
        })
        $('body').on('click', '.del-group-member, .del-group-role', function () {
            $(this).parent().remove();
        })
        $('body').on('click', '#eg-btn-submit', function () {
            var uIds = new Array();
            var gId = $('#eg-gid').val();
            $('.group-member-panel').each(function () {
                uIds.push($(this).attr('data-val'));
            });
            if (uIds.length <= 0) {
                alert("Please add one member at least!");
                return false;
            }
            $.post('/Permission/EditGroupMember',
                {
                    gId: gId,
                    uIds: uIds
                }, function (output) {
                    if (output.success) {
                        $('#' + gId).children().eq(3).children().eq(0).html(uIds.length);
                        $('#modal-edit-member').modal('hide');
                    }
                    else {
                        alert("Failed to edit group members");
                    }
                })
        })
        $('body').on('click', '.edit-grole', function () {
            var gid = $(this).attr('data-id');
            $('#er-gid').val(gid);
            $('.group-roles').empty();
            $.post('/Permission/GetGroupRoles',
                {
                    gid: gid
                }, function (output) {
                    if (output.success) {
                        if (output.gRoles.length > 0) {
                            $.each(output.gRoles, function (i, val) {
                                $('.group-roles').append(
                                    '<div class="group-role-panel" data-val="' + val.ID + '" data-name="' + val.Name + '">' +
                                    '<span>' + val.Name + '</span>' +
                                    '<span class="glyphicon glyphicon-remove del-group-role"></span>' +
                                    '</div >'
                                );
                            })
                        }
                        if (output.aRoles.length > 0) {
                            autoCompleteFill('er-rolelist', output.aRoles);
                        }
                    }
                });
            $('#modal-edit-role').modal('show');
        })
        $('body').on('click', '#er-btn-submit', function () {
            var rIds = new Array();
            var gId = $('#er-gid').val();
            $('.group-role-panel').each(function () {
                rIds.push($(this).attr('data-val'));
            });
            if (rIds.length <= 0) {
                alert("Please add one Role at least!");
                return false;
            }
            $.post('/Permission/EditGroupRole',
                {
                    gId: gId,
                    rIds: rIds
                }, function (output) {
                    if (output.success) {
                        $('#modal-edit-role').modal('hide');
                    }
                    else {
                        alert("Failed to edit group role");
                    }
                })
        })
        function autoCompleteFill(id, values) {
            $('#' + id).autoComplete('destroy');
            $('#' + id).autoComplete({
                minChars: 0,
                source: function (term, suggest) {
                    var choices = values;
                    var suggestions = [];
                    $.each(choices, function (i, val) {
                        if (~(val.ID + ' ' + val.Name).toLowerCase().indexOf(term))
                            suggestions.push(val);
                    })
                    suggest(suggestions);
                },
                renderItem: function (item, search) {
                    search = search.replace(/[-\/\\^$*+?.()|[\]{}]/g, '\\$&');
                    var re = new RegExp("(" + search.split(' ').join('|') + ")", "gi");
                    return '<div class="autocomplete-suggestion" data-value="' + item.ID + '" data-name="' + item.Name + '"> ' + item.Name.replace(re, "<b>$1</b>") + '</div>';
                },
                onSelect: function (e, term, item) {
                    var flg = false;
                    $('.group-member-panel').each(function () {
                        if ($(this).attr('data-val') == item.data('value')) {
                            flg = true;
                            return;
                        }
                    });
                    if (!flg) {
                        $('.group-members').append(
                            '<div class="group-member-panel" data-val="' + item.data('value') + '" data-name="' + item.data('name') + '">' +
                            '<span>' + item.data('name') + '</span>' +
                            '<span class="glyphicon glyphicon-remove del-group-member"></span>' +
                            '</div >'
                        );
                    }
                }
            });
        }
    }
    var rolelist = function () {

    }
    var menus = function () {
        $('body').on('click', '#btn-add-menu', function () {
            $('#m-mid').val('');
            $('#modal-name').val('');
            $('#modal-url').val('');
            $('#modal-pid').val('');
            $('#modal-oid').val('');
            $('#m-title').html('Add Menu');
            //$.post('/Permission/GetAllMenus',
            //{

            //}, function () {

            //})
            $('#modal-add-menu').modal('show');
        })
        $('body').on('click', '#m-btn-submit', function () {
            var mid = $('#m-mid').val();
            var name = $('#modal-name').val();
            var url = $('#modal-url').val();
            var pid = $('#modal-pid').val();
            var oid = $('#modal-oid').val();
            if (name == '') {
                alert("Please input Menu Name!");
                return false;
            }
            if (url == '') {
                alert("Please input Menu Url!");
                return false;
            }
            if (oid == '') {
                alert("Please input Menu order!");
                return false;
            }
            if (mid == '') {
                $.post('/Permission/AddMenu', {
                    name: name,
                    url: url,
                    pid: pid,
                    oid: oid
                }, function (output) {
                    if (output.success) {
                        window.location.reload();
                    }
                    else {
                        alert("Failed to Add Menu!");
                    }
                })
            }
            else {
                $.post('/Permission/UpdateMenu', {
                    mid: mid,
                    name: name,
                    url: url,
                    pid: pid,
                    oid: oid
                }, function (output) {
                    if (output.success) {
                        window.location.reload();
                    }
                    else {
                        alert("Failed to Update Menu!");
                    }
                })

            }
        })
        $('body').on('click', '.pages', function () {
            var keywords = $.trim($('#tb-search').val());
            if (keywords != "") {
                window.location.href = '/Permission/Menus?pageno=' + $(this).attr('data-data') + "&keywords=" + keywords;
            }
            else {
                window.location.href = '/Permission/Menus?pageno=' + $(this).attr('data-data');
            }
        })
        $('body').on('click', '.del-menu', function () {
            if (confirm("Do you really want to delete this menu ?")) {
                var mid = $(this).attr('data-id');
                if (mid) {
                    $.post('/Permission/DelMenu', {
                        mid: mid
                    }, function (output) {
                        if (output.success) {
                            window.location.href = "/Permission/Menus";
                        }
                        else {
                            alert("Failed to delete");
                        }
                    })
                }
            }
        })
        $('body').on('click', '.edit-menu', function () {
            var mid = $(this).attr('data-id');
            var name = $(this).parent().parent().children().eq(1).html();
            var url = $(this).parent().parent().children().eq(2).html();
            var pid = $(this).parent().parent().children().eq(3).attr("data-pid");
            var oid = $(this).parent().parent().children().eq(4).html();
            var pname = $(this).parent().parent().children().eq(3).html();

            $('#m-mid').val(mid);
            $('#modal-name').val(name);
            $('#modal-url').val(url);
            $('#modal-pid').val(pname);
            $('#modal-pid').attr("data-pid", pid);
            $('#modal-oid').val(oid);

            $('#m-title').html('Edit Menu');
            $('#modal-add-menu').modal('show');
        })
        $('body').on('click', '#m-btn-search', function () {
            var keywords = $.trim($('#tb-search').val());
            if (keywords == "") {
                alert("Please input search keywords!");
                return false;
            }
            window.location.href = '/Permission/Menus?pageno=1&keywords=' + keywords;
        })
    }
    var functions = function () {
        $.post('/Permission/GetMenuList', {
        }, function (output) {
            if (output.success) {
                autoCompleteFill('modal-menulist', output.Menus);
            }
        })
        $('body').on('click', '#btn-add-func', function () {
            $('#m-fid').val('');
            $('#modal-name').val('');
            $('#modal-url').val('');
            $('#modal-menulist').val('');
            $('#modal-menulist').attr('data-id', '');
            $('#m-title').html('Add Function');
            $('#modal-add-func').modal('show');
        })
        $('body').on('click', '#m-btn-submit', function () {
            var fid = $('#m-fid').val();
            var name = $('#modal-name').val();
            var url = $('#modal-url').val();
            var mid = $('#modal-menulist').attr('data-id');
            if (name == '') {
                alert("Please input Function Name!");
                return false;
            }
            if (url == '') {
                alert("Please input Function Url!");
                return false;
            }
            if (mid == '') {
                alert("Please select Menu!");
                return false;
            }
            if (fid == '') {
                $.post('/Permission/AddFunction', {
                    name: name,
                    url: url,
                    mid: mid
                }, function (output) {
                    if (output.success) {
                        window.location.reload();
                    }
                    else {
                        alert("Failed to Add Function!");
                    }
                })
            }
            else {
                $.post('/Permission/UpdateFunction', {
                    fid: fid,
                    name: name,
                    url: url,
                    mid: mid
                }, function (output) {
                    if (output.success) {
                        window.location.reload();
                    }
                    else {
                        alert("Failed to Update Function!");
                    }
                })

            }
        })
        $('body').on('click', '.pages', function () {
            var keywords = $.trim($('#tb-search').val());
            if (keywords != "") {
                window.location.href = '/Permission/Functions?pageno=' + $(this).attr('data-data') + "&keywords=" + keywords;
            }
            else {
                window.location.href = '/Permission/Functions?pageno=' + $(this).attr('data-data');
            }
        })
        $('body').on('click', '.del-func', function () {
            if (confirm("Do you really want to delete this function ?")) {
                var fid = $(this).attr('data-id');
                if (fid) {
                    $.post('/Permission/DelFunction', {
                        fid: fid
                    }, function (output) {
                        if (output.success) {
                            window.location.href = "/Permission/Functions";
                        }
                        else {
                            alert("Failed to delete");
                        }
                    })
                }
            }
        })
        $('body').on('click', '.edit-func', function () {
            var fid = $(this).attr('data-id');
            var name = $(this).parent().parent().children().eq(1).html();
            var url = $(this).parent().parent().children().eq(3).html();
            var mid = $(this).parent().parent().children().eq(2).attr("data-id");
            var mname = $(this).parent().parent().children().eq(2).html();

            $('#m-fid').val(fid);
            $('#modal-name').val(name);
            $('#modal-url').val(url);
            $('#modal-menulist').val(mname);
            $('#modal-menulist').attr("data-id", mid);

            $('#m-title').html('Edit Function');
            $('#modal-add-func').modal('show');
        })
        $('body').on('click', '#m-btn-search', function () {
            var keywords = $.trim($('#tb-search').val());
            if (keywords == "") {
                alert("Please input search keywords!");
                return false;
            }
            window.location.href = '/Permission/Functions?pageno=1&keywords=' + keywords;
        })

        function autoCompleteFill(id, values) {
            $('#' + id).autoComplete({
                minChars: 0,
                source: function (term, suggest) {
                    var choices = values;
                    var suggestions = [];
                    $.each(choices, function (i, val) {
                        if (~(val.ID + ' ' + val.Name).toLowerCase().indexOf(term))
                            suggestions.push(val);
                    })
                    suggest(suggestions);
                },
                renderItem: function (item, search) {
                    search = search.replace(/[-\/\\^$*+?.()|[\]{}]/g, '\\$&');
                    var re = new RegExp("(" + search.split(' ').join('|') + ")", "gi");
                    return '<div class="autocomplete-suggestion" data-value="' + item.ID + '" data-name="' + item.Name + '"> ' + item.Name.replace(re, "<b>$1</b>") + '</div>';
                },
                onSelect: function (e, term, item) {
                    $('#' + id).attr('data-id', item.data('value'));
                    $('#' + id).val(item.data('name'));
                }
            });
        }
    }
    var operations = function () {
        $('body').on('click', '#btn-add-op', function () {
            $('#m-oid').val('');
            $('#modal-name').val('');
            $('#m-title').html('Add Operation');
            $('#modal-add-op').modal('show');
        })
        $('body').on('click', '#m-btn-submit', function () {
            var oid = $('#m-oid').val();
            var name = $('#modal-name').val();
            if (name == '') {
                alert("Please input Operation Name!");
                return false;
            }
            if (oid == '') {
                $.post('/Permission/AddOperation', {
                    name: name
                }, function (output) {
                    if (output.success) {
                        window.location.reload();
                    }
                    else {
                        alert("Failed to Add Operation!");
                    }
                })
            }
            else {
                $.post('/Permission/UpdateOperation', {
                    oid: oid,
                    name: name
                }, function (output) {
                    if (output.success) {
                        window.location.reload();
                    }
                    else {
                        alert("Failed to Update Operation!");
                    }
                })

            }
        })
        $('body').on('click', '.pages', function () {
            var keywords = $.trim($('#tb-search').val());
            if (keywords != "") {
                window.location.href = '/Permission/Operations?pageno=' + $(this).attr('data-data') + "&keywords=" + keywords;
            }
            else {
                window.location.href = '/Permission/Operations?pageno=' + $(this).attr('data-data');
            }
        })
        $('body').on('click', '.del-op', function () {
            if (confirm("Do you really want to delete this Operation ?")) {
                var oid = $(this).attr('data-id');
                if (oid) {
                    $.post('/Permission/DelOperation', {
                        oid: oid
                    }, function (output) {
                        if (output.success) {
                            window.location.href = "/Permission/Operations";
                        }
                        else {
                            alert("Failed to delete");
                        }
                    })
                }
            }
        })
        $('body').on('click', '.edit-op', function () {
            var oid = $(this).attr('data-id');
            var name = $(this).parent().parent().children().eq(1).html();

            $('#m-oid').val(oid);
            $('#modal-name').val(name);

            $('#m-title').html('Edit Operation');
            $('#modal-add-op').modal('show');
        })
        $('body').on('click', '#m-btn-search', function () {
            var keywords = $.trim($('#tb-search').val());
            if (keywords == "") {
                alert("Please input search keywords!");
                return false;
            }
            window.location.href = '/Permission/Operations?pageno=1&keywords=' + keywords;
        })

    }

    
    return {
        memberlist: function () {
            memberlist();
        },
        grouplist: function () {
            grouplist();
        },
        rolelist: function () {
            rolelist();
        },
        menus: function () {
            menus();
        },
        functions: function () {
            functions();
        },
        operations: function () {
            operations();
        }
    }
}();