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
                            autoCompleteFill('eg-memlist', output.aMembers, 1);
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
                                    '<div class="group-role-panel" data-val="' + val.RoleID + '" data-name="' + val.RoleName + '">' +
                                    '<span>' + val.RoleName + '</span>' +
                                    '<span class="glyphicon glyphicon-remove del-group-role"></span>' +
                                    '</div >'
                                );
                            })
                        }
                        if (output.aRoles.length > 0) {
                            autoCompleteFill('er-rolelist', output.aRoles, 2);
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
        function autoCompleteFill(id, values, type) {
            $('#' + id).autoComplete('destroy');
            $('#' + id).autoComplete({
                minChars: 0,
                source: function (term, suggest) {
                    var choices = values;
                    var suggestions = [];
                    $.each(choices, function (i, val) {
                        if (type == 1) {
                            if (~(val.ID + ' ' + val.Name).toLowerCase().indexOf(term))
                                suggestions.push(val);
                        }
                        else {
                            if (~(val.RoleID + ' ' + val.Name).toLowerCase().indexOf(term))
                                suggestions.push(val);
                        }
                    })
                    suggest(suggestions);
                },
                renderItem: function (item, search) {
                    search = search.replace(/[-\/\\^$*+?.()|[\]{}]/g, '\\$&');
                    var re = new RegExp("(" + search.split(' ').join('|') + ")", "gi");
                    return '<div class="autocomplete-suggestion" data-value="' + item.ID + '" data-name="' + item.Name + '"> ' + item.Name.replace(re, "<b>$1</b>") + '</div>';
                },
                onSelect: function (e, term, item) {
                    var panel_container = "";
                    var group_container = "";
                    var del_class = "";
                    if (type == 1) {
                        panel_container = "group-member-panel";
                        group_container = "group-members";
                        del_class = "del-group-member";
                    }
                    else {
                        panel_container = "group-role-panel";
                        group_container = "group-roles";
                        del_class = "del-group-role";
                    }
                    var flg = false;
                    $('.' + panel_container).each(function () {
                        if ($(this).attr('data-val') == item.data('value')) {
                            flg = true;
                            return;
                        }
                    });
                    if (!flg) {
                        $('.' + group_container).append(
                            '<div class="' + panel_container+'" data-val="' + item.data('value') + '" data-name="' + item.data('name') + '">' +
                            '<span>' + item.data('name') + '</span>' +
                            '<span class="glyphicon glyphicon-remove ' + del_class+'"></span>' +
                            '</div >'
                        );
                    }
                }
            });
        }
    }
    var rolelist = function () {
        $.post('/Permission/GetRolePermission',
        {
            rid: ''
        }, function (output) {
            if (output.success) {
                if (output.rPermissions.length > 0) {
                    $.each(output.rPermissions, function (i, val) {
                        $('.r-permissions').append(
                            '<div class="role-permission-panel" data-ids="' + val.MenuID + '_' + val.FunctionID + '_' + val.OperationID + '">' +
                            '<span>' + val.MenuName + ' | ' + val.FunctionName + ' | ' + val.OperationName + '</span>' +
                            '<span class="glyphicon glyphicon-remove del-role-permission"></span>' +
                            '</div >'
                        );
                    })
                }
                if (output.mList.length > 0) {
                    autoCompleteFill('m-mid', output.mList, 1);
                }
                if (output.oList.length > 0) {
                    autoCompleteFill('m-oid', output.oList, 3);
                }
            }
        })
        $('body').on('click', '#btn-add-role', function () {
            $('#m-name').val('');
            $('#m-comment').val('');
            $('#m-oid').val('');
            $('#m-rid').val('');
            $('#m-mid').val('');
            $('#m-fid').val('');
            $('.r-permissions').empty();
            $('#modal-edit-role').modal('show');
        })
        $('body').on('click', '.role-permissions', function () {
            var rid = $(this).data("id");
            $('#m-rid').val(rid);
            $('#m-name').val($(this).parent().parent().children().eq(1).html());
            $('#m-comment').val($(this).parent().parent().children().eq(3).html());
            $('#m-mid').val('');
            $('#m-fid').val('');
            $('#m-oid').val('');
            $('.r-permissions').empty();
            $.post('/Permission/GetRolePermission',
            {
                rid: rid
            }, function (output) {
                if (output.success) {
                    if (output.rPermissions.length > 0) {
                        $.each(output.rPermissions, function (i, val) {
                            $('.r-permissions').append(
                                '<div class="role-permission-panel" data-ids="' + val.MenuID + '_' + val.FunctionID + '_' + val.OperationID + '">' +
                                '<span>' + val.MenuName + ' | ' + val.FunctionName + ' | ' + val.OperationName + '</span>' +
                                '<span class="glyphicon glyphicon-remove del-role-permission"></span>' +
                                '</div >'
                            );
                        })
                    }
                    $('#modal-edit-role').modal('show');
                }
            })
        })
        $('body').on('click', '.edit-role', function () {
            var rid = $(this).attr('data-id');
            $('#m-rid').val(rid);
            $('#m-name').val($(this).parent().parent().children().eq(1).html());
            $('#m-comment').val($(this).parent().parent().children().eq(3).html());
            $('#m-mid').val('');
            $('#m-fid').val('');
            $('#m-oid').val('');
            $('.r-permissions').empty();
            $.post('/Permission/GetRolePermission',
            {
                rid: rid
            }, function (output) {
                if (output.success) {
                    if (output.rPermissions.length > 0) {
                        $.each(output.rPermissions, function (i, val) {
                            $('.r-permissions').append(
                                '<div class="role-permission-panel" data-ids="' + val.MenuID + '_' + val.FunctionID + '_' + val.OperationID + '">' +
                                '<span>' + val.MenuName + ' | ' + val.FunctionName + ' | ' + val.OperationName + '</span>' +
                                '<span class="glyphicon glyphicon-remove del-role-permission"></span>' +
                                '</div >'
                            );
                        })
                    }
                    $('#modal-edit-role').modal('show');
                }
            });
        })
        $('body').on('click', '.del-role', function () {
            if (!confirm("Really to delete this role?")) {
                return false;
            }
            var rid = $(this).attr('data-id');
            $.post('/Permission/DelRole',
            {
                rId: rid
            }, function (output) {
                if (output.success) {
                    window.location.reload();
                }
                else {
                    alert("Failed to Delete");
                }
            })
        })
        $('body').on('click', '#m-btn-search', function () {
            var keywords = $.trim($('#tb-search').val());
            if (keywords == "") {
                alert("Please input search keywords!");
                return false;
            }
            window.location.href = '/Permission/RoleList?pageno=1&keywords=' + keywords;
        })
        $('body').on('click', '.pages', function () {
            var keywords = $.trim($('#tb-search').val());
            if (keywords != "") {
                window.location.href = '/Permission/RoleList?pageno=' + $(this).attr('data-data') + "&keywords=" + keywords;
            }
            else {
                window.location.href = '/Permission/RoleList?pageno=' + $(this).attr('data-data');
            }
        })
        $('body').on('click', '.del-role-permission', function () {
            $(this).parent().remove();
        })
        $('body').on('click', '#m-btn-submit', function () {
            var mIds = new Array();
            var rId = $('#m-rid').val();
            var rName = $('#m-name').val();
            var comment = $('#m-comment').val();
            if (rName == "") {
                alert("Please input role name");
                return false;
            }
            $('.role-permission-panel').each(function () {
                mIds.push($(this).attr('data-ids'));
            });
            if (mIds.length <= 0) {
                alert("Please add permissions!");
                return false;
            }
            if (rId == '') {
                $.post('/Permission/AddRole',
                {
                    rName: rName,
                    mIds: mIds,
                    comment: comment
                }, function (output) {
                    if (output.success) {
                        window.location.reload();
                    }
                    else {
                        alert("Failed to add Role");
                    }
                })
            }
            else {
                $.post('/Permission/EditRole',
                {
                    rId: rId,
                    rName: rName,
                    mIds: mIds,
                    comment: comment
                }, function (output) {
                    if (output.success) {
                        window.location.reload();
                    }
                    else {
                        alert("Failed to edit role");
                    }
                })
            }
        })
        function autoCompleteFill(id, values, mflg) {
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
                    $('#' + id).val(item.data('name'));
                    if (mflg == 1) {
                        //menu select
                        $('#m-ids').attr('data-mid', item.data('value'));
                        $('#m-ids').attr('data-mname', item.data('name'));
                        // function source
                        $.post('/Permission/GetMenuFunctions', {
                            mid: item.data('value')
                        }, function (output) {
                            if (output.success) {
                                autoCompleteFill('m-fid', output.mFunctions, 2);
                            }
                        })
                    }
                    else if (mflg == 2) {
                        //function select
                        $('#m-ids').attr('data-fid', item.data('value'));
                        $('#m-ids').attr('data-fname', item.data('name'));
                    }
                    else if (mflg == 3) {
                        // operation select
                        $('#m-ids').attr('data-oid', item.data('value'));
                        $('#m-ids').attr('data-oname', item.data('name'));
                    }
                    var mid = $('#m-ids').attr('data-mid');
                    var fid = $('#m-ids').attr('data-fid');
                    var oid = $('#m-ids').attr('data-oid');
                    if (mid != '' && fid != '' && oid != '') {
                        var mids = mid + '_' + fid + '_' + oid;
                        $('.role-permission-panel').each(function () {
                            if ($(this).attr('data-ids') == mids ) {
                                flg = true;
                                return;
                            }
                        });
                        if (!flg) {
                            var mnames = $('#m-ids').attr('data-mname') + ' | ' + $('#m-ids').attr('data-fname') + ' | ' + $('#m-ids').attr('data-oname');
                            $('.r-permissions').append(
                                '<div class="role-permission-panel" data-ids="' + mids + '">' +
                                '<span>' + mnames + '</span>' +
                                '<span class="glyphicon glyphicon-remove del-role-permission"></span>' +
                                '</div >'
                            );
                        }

                        $('#m-ids').attr('data-mid', '');
                        $('#m-ids').attr('data-fid', '');
                        $('#m-ids').attr('data-oid', '');
                        $('#m-ids').attr('data-mname', '');
                        $('#m-ids').attr('data-fname', '');
                        $('#m-ids').attr('data-oname', '');
                        $('#m-mid').val('');
                        $('#m-fid').val('');
                        $('#m-oid').val('');
                    }
                }
            });
        }
    }
    var menus = function () {
        $.post('/Permission/GetMenuList',
        {}, function (output) {
            if (output.success) {
                autoCompleteFill('modal-pid', output.Menus)
            }
        })
        $('body').on('click', '#btn-add-menu', function () {
            $('#m-mid').val('');
            $('#modal-name').val('');
            $('#modal-url').val('');
            $('#modal-imgurl').val('');
            $('#modal-pid').val('');
            $('#modal-oid').val('');
            $('#m-title').html('Add Menu');
            $('#modal-add-menu').modal('show');
        })
        $('body').on('click', '#m-btn-submit', function () {
            var mid = $('#m-mid').val();
            var name = $('#modal-name').val();
            var url = $('#modal-url').val();
            var imgurl = $('#modal-imgurl').val();
            var pid = $('#modal-pid').data('id');
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
                    imgurl: imgurl,
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
                    imgurl: imgurl,
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
            var imgurl = $(this).parent().parent().children().eq(3).html();
            var pid = $(this).parent().parent().children().eq(4).attr("data-pid");
            var oid = $(this).parent().parent().children().eq(5).html();
            var pname = $(this).parent().parent().children().eq(4).html();

            $('#m-mid').val(mid);
            $('#modal-name').val(name);
            $('#modal-url').val(url);
            $('#modal-imgurl').val(imgurl);
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
    var functions = function () {
        $.post('/Permission/GetMenuList', {
            type: 1
        }, function (output) {
            if (output.success) {
                autoCompleteFill('modal-menulist', output.Menus);
            }
        })
        $('body').on('click', '#btn-add-func', function () {
            $('#m-fid').val('');
            $('#modal-name').val('');
            $('#modal-url').val('');
            $('#modal-imgurl').val('');
            $('#modal-menulist').val('');
            $('#modal-menulist').attr('data-id', '');
            $('#m-title').html('Add Function');
            $('#modal-add-func').modal('show');
        })
        $('body').on('click', '#m-btn-submit', function () {
            var fid = $('#m-fid').val();
            var name = $('#modal-name').val();
            var url = $('#modal-url').val();
            var imgurl = $('#modal-imgurl').val();
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
                    imgurl: imgurl,
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
                    imgurl: imgurl,
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
            var imgurl = $(this).parent().parent().children().eq(4).html();
            var mid = $(this).parent().parent().children().eq(2).attr("data-id");
            var mname = $(this).parent().parent().children().eq(2).html();

            $('#m-fid').val(fid);
            $('#modal-name').val(name);
            $('#modal-url').val(url);
            $('#modal-imgurl').val(imgurl);
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
    var applyforpermission = function () {
        $.post('/Permission/GetRolePermission',
        {
            rid: ''
        }, function (output) {
            if (output.success) {
                if (output.rPermissions.length > 0) {
                    $.each(output.rPermissions, function (i, val) {
                        $('.r-permissions').append(
                            '<div class="role-permission-panel" data-ids="' + val.MenuID + '_' + val.FunctionID + '_' + val.OperationID + '">' +
                            '<span>' + val.MenuName + ' | ' + val.FunctionName + ' | ' + val.OperationName + '</span>' +
                            '<span class="glyphicon glyphicon-remove del-role-permission"></span>' +
                            '</div >'
                        );
                    })
                }
                if (output.mList.length > 0) {
                    autoCompleteFill('mid', output.mList, 1);
                }
                if (output.oList.length > 0) {
                    autoCompleteFill('oid', output.oList, 3);
                }
            }
        })
        $('body').on('click', '#btn-submit', function () {
            var comment = $('#comment').val();
            var Ids = new Array();
            $('.role-permission-panel').each(function () {
                Ids.push($(this).attr('data-ids'));
            });

            if (Ids.length == 0) {
                alert("Please select permission");
                return false;
            }

            $.post('/Permission/AddUserPermissionRequest',
            {
                Ids: Ids,
                comment: comment,
            }, function (output) {
                if (output.success) {
                    window.location.href = '/Permission/PermissionRequest';
                }
                else {
                    alert('Failed to apply');
                }
            })
        })
        $('body').on('click', '.del-role-permission', function () {
            $(this).parent().remove();
        })
        $('body').on('click', '#btn-cancel', function () {
            window.location.reload();
        })
        function autoCompleteFill(id, values, mflg) {
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
                    $('#' + id).val(item.data('name'));
                    if (mflg == 1) {
                        //menu select
                        $('#ids').attr('data-mid', item.data('value'));
                        $('#ids').attr('data-mname', item.data('name'));
                        // function source
                        $.post('/Permission/GetMenuFunctions', {
                            mid: item.data('value')
                        }, function (output) {
                            if (output.success) {
                                autoCompleteFill('fid', output.mFunctions, 2);
                            }
                        })
                    }
                    else if (mflg == 2) {
                        //function select
                        $('#ids').attr('data-fid', item.data('value'));
                        $('#ids').attr('data-fname', item.data('name'));
                    }
                    else if (mflg == 3) {
                        // operation select
                        $('#ids').attr('data-oid', item.data('value'));
                        $('#ids').attr('data-oname', item.data('name'));
                    }
                    var mid = $('#ids').attr('data-mid');
                    var fid = $('#ids').attr('data-fid');
                    var oid = $('#ids').attr('data-oid');
                    if (mid != '' && fid != '' && oid != '') {
                        var mids = mid + '_' + fid + '_' + oid;
                        $('.role-permission-panel').each(function () {
                            if ($(this).attr('data-ids') == mids) {
                                flg = true;
                                return;
                            }
                        });
                        if (!flg) {
                            var mnames = $('#ids').attr('data-mname') + ' | ' + $('#ids').attr('data-fname') + ' | ' + $('#ids').attr('data-oname');
                            $('.r-permissions').append(
                                '<div class="role-permission-panel" data-ids="' + mids + '">' +
                                '<span>' + mnames + '</span>' +
                                '<span class="glyphicon glyphicon-remove del-role-permission"></span>' +
                                '</div >'
                            );
                        }

                        $('#ids').attr('data-mid', '');
                        $('#ids').attr('data-fid', '');
                        $('#ids').attr('data-oid', '');
                        $('#ids').attr('data-mname', '');
                        $('#ids').attr('data-fname', '');
                        $('#ids').attr('data-oname', '');
                        $('#mid').val('');
                        $('#fid').val('');
                        $('#oid').val('');
                    }
                }
            });
        }
    }
    var permissionrequest = function () {
        $('body').on('click', '.upr-approve', function () {
            if (!confirm("You will approve this request, are you really do this operation?")) {
                return false;
            }
            var id = $(this).attr("data-id");
            $.post('/Permission/ApprovePermissionRequest',
            {
                id: id
            }, function (output) {
                if (output.success) {
                    window.location.reload();
                }
                else {
                    alert("Failed");
                }
            })
        })
        $('body').on('click', '.upr-deny', function () {
            if (!confirm("You will Deny this request, are you really do this operation?")) {
                return false;
            }
            var id = $(this).attr("data-id");
            $.post('/Permission/DenyPermissionRequest',
            {
                id: id
            }, function (output) {
                if (output.success) {
                    window.location.reload();
                }
                else {
                    alert("Failed");
                }
            })
        })
        $('body').on('click', '.upr-complete', function () {
            var id = $(this).attr("data-id");
            $.post('/Permission/CompletePermissionRequest',
            {
                id: id
            }, function (output) {
                if (output.success) {
                    window.location.reload();
                }
                else {
                    alert("Failed: You have not operate this request!");
                }
            })
        })
        $('body').on('click', '#m-btn-search', function () {
            var keywords = $.trim($('#tb-search').val());
            if (keywords == "") {
                alert("Please input search keywords!");
                return false;
            }
            window.location.href = '/Permission/PermissionRequest?pageno=1&keywords=' + keywords;
        })
        $('body').on('click', '.pages', function () {
            var keywords = $.trim($('#tb-search').val());
            if (keywords != "") {
                window.location.href = '/Permission/PermissionRequest?pageno=' + $(this).attr('data-data') + "&keywords=" + keywords;
            }
            else {
                window.location.href = '/Permission/PermissionRequest?pageno=' + $(this).attr('data-data');
            }
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
        },
        applyforpermission: function () {
            applyforpermission();
        },
        permissionrequest: function () {
            permissionrequest();
        }
    }
}();