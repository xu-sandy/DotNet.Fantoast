//datagrid中toolbar功能需要在easyui代码中加入以下代码  
///////////////////////查找 
//if (btn == "-") {
//    $("<td><div class=\"datagrid-btn-separator\"></div></td>").appendTo(tr);
//}
///////////////////////添加以下代码 
//else if (Object.prototype.toString.call(btn) === "[object String]") {
//    $(btn).addClass("datagrid-toolbar").appendTo(tb);
//    $(btn).show();
//}

(function ($) {
    var addDefaultTemp;

    //datagrid
    $.fn.easyui_datagrid = function (options) {
        var index;
        var opts = $.extend({}, $.fn.easyui_datagrid.defaults, options);
        addDefaultTemp = opts.addDefault;
        if (opts.type == '')
            return;

        var typemsg = gettype(opts.type);

        return this.each(function () {
            $this = $(this);



            //生成表格
            //标题
            if (opts.title && (opts.title != '' || opts.type != '')) {
                $this.datagrid({
                    title: (opts.title == true || opts.title == '') ? opts.type : opts.title
                });
            }

            //栏位
            var columns = [];

            if (opts.columns_show.length != 0) {
                $.each(opts.columns_show, function (j, value) {
                    //标题
                    var titlename = opts.columns_title.length > j ? (opts.columns_title[j] != '' ? opts.columns_title[j] : value) : value;
                    //编辑
                    var editor = opts.columns_editor.length > j ? ($.isEmptyObject(opts.columns_editor[j]) ? { type: 'text' } : opts.columns_editor[j]) : { type: 'text' };
                    //格式化
                    var formatter = opts.columns_formatter.length > j ? (opts.columns_formatter[j] == '-' ? '' : opts.columns_formatter[j]) : '';
                    //宽度
                    var width = opts.columns_width.length > j ? opts.columns_width[j] : '';
                    //选择框
                    var checkbox = opts.columns_checkbox.length > j ? opts.columns_checkbox[j] : false;

                    columns.push({ field: value, title: titlename, width: width, editor: editor, formatter: formatter, checkbox: checkbox });
                });
            }
            else {
                for (var i = 0, j = 0; i < typemsg.columns.length; i++) {
                    if (opts.columns_notitle.indexOf(typemsg.columns[i]) > -1) { continue; }
                    //标题
                    var titlename = opts.columns_title.length > j ? (opts.columns_title[j] != '' ? opts.columns_title[j] : typemsg.columns[i]) : typemsg.columns[i];
                    //编辑
                    var editor = opts.columns_editor.length > j ? ($.isEmptyObject(opts.columns_editor[j]) ? { type: 'text' } : opts.columns_editor[j]) : { type: 'text' };
                    //格式化
                    var formatter = opts.columns_formatter.length > j ? (opts.columns_formatter[j] == '-' ? '' : opts.columns_formatter[j]) : '';
                    //宽度
                    var width = opts.columns_width.length > j ? opts.columns_width[j] : '';
                    //选择框
                    var checkbox = opts.columns_checkbox.length > j ? opts.columns_checkbox[j] : false;

                    columns.push({ field: typemsg.columns[i], title: titlename, width: width, editor: editor, formatter: formatter, checkbox: checkbox });
                    j++;
                }
            }

            //双击是否可修改
            if (opts.toolbar.indexOf('edit') > -1 || opts.feedback == false) {
                $this.datagrid({
                    onDblClickRow: function (rowIndex, rowData) {
                        //如果不可修改 那么双击失效
                        if (opts.toolbar_disable.length > opts.toolbar.indexOf('edit')) {
                            if (opts.toolbar_disable[opts.toolbar.indexOf('edit')])
                                return;
                        }
                        if (index != null) {
                            $this.datagrid('endEdit', index);
                        }
                        else {
                            index = rowIndex;
                            $this.datagrid('beginEdit', index);
                        }
                    },
                    onClickRow: function (rowIndex, rowData) {
                        if (index != null) {
                            $this.datagrid('endEdit', index);
                        }
                    },
                    onAfterEdit: function (rowIndex, rowData, changes) {
                        if (opts.feedback == false) {
                            index = null;
                            return;
                        }
                        $.ajax({
                            url: '/common/savedata?type=' + opts.type + "&permissionmenuid=" + getmenuid(),
                            data: rowData,
                            type: 'post',
                            dataType: 'json',
                            success: function (data, status) {
                                if (data.validate == false) {
                                    alert(data.msg);
                                    //$this.datagrid('reload');
                                    //index = null;
                                }
                                else {
                                    $this.datagrid('reload');
                                    index = null;
                                }
                            },
                            error: function () {
                                alert("数据未处理，可能原因是系统异常或权限不足。");
                                //$this.datagrid('reload');
                                //index = null;
                            }
                        })
                    }
                });
            }

            $this.datagrid({
                border: opts.border,
                fit: opts.fit,
                rownumbers: opts.rownumbers,
                queryParams: opts.queryParams,
                columns: [columns],
                CheckOnSelect: opts.CheckOnSelect,
                onLoadSuccess: opts.onLoadSuccess,
                onDblClickRow:opts.onDblClickRow,
                SelectOnCheck: opts.SelectOnCheck,
                url: opts.url == '' ? '/common/getdata?type=' + opts.type + "&permissionmenuid=" + getmenuid() : opts.url,
                pagination: opts.pagination,
                singleSelect: opts.singleSelect,
                onEditCallBack: opts.onEditCallBack
            });

            //detail
            if (opts.detail_url != '') {

                $this.datagrid({
                    view: detailview,
                    detailFormatter: function (index, row) {
                        return '<div class="ddv" style="padding:5px 0"></div>';
                    },
                    onExpandRow: function (index, row) {
                        var url = opts.detail_url.indexOf('?') > -1 ?
                            opts.detail_url + '&detailid=' + eval('row.' + opts.idField) :
                            opts.detail_url + '?detailid=' + eval('row.' + opts.idField);
                        var ddv = $this.datagrid('getRowDetail', index).find('div.ddv');
                        ddv.panel({
                            height: opts.detail_height,
                            border: false,
                            cache: false,
                            content: '<iframe scrolling="auto" frameborder="0" src="' + url + '" style="width:100%;height:99%;"></iframe>',
                            onLoad: function () {
                                $this.datagrid('fixDetailRowHeight', index);
                            }
                        });
                        $this.datagrid('fixDetailRowHeight', index);
                    }
                });
            }

            //toolbar
            if (opts.toolbar.length > 0) {
                var toolbar = [];
                if (Object.prototype.toString.call(opts.toolbar) != '[object Array]') {
                    toolbar = opts.toolbar;
                }
                else {
                    for (var i = 0; i < opts.toolbar.length; i++) {
                        var codes = pharos.permission.getpermissions();
                        var disabled = false;
                        if (opts.permissionCode.length > i && opts.permissionCode[i] != '')
                            disabled = codes.indexOf(opts.permissionCode[i]) == -1;
                        switch (opts.toolbar[i]) {
                            case "add":
                                toolbar.push({
                                    iconCls: 'icon-add',
                                    disabled: disabled,
                                    text: '添加',
                                    handler: function () { index = datagrid_additem(index, $this, opts); }
                                });
                                break;
                            case "del":
                                toolbar.push({
                                    iconCls: 'icon-remove',
                                    disabled: disabled,
                                    text: '删除',
                                    handler: function () { index = datagrid_removeitem(index, $this, opts.type, opts); }
                                });
                                break;
                            case "del1":
                                toolbar.push({
                                    iconCls: 'icon-remove',
                                    disabled: disabled,
                                    text: '删除',
                                    handler: function () { index = datagrid_removeitem1(index, $this, opts.type, opts); }
                                });
                                break;
                            case "edit":
                                toolbar.push({
                                    iconCls: 'icon-edit',
                                    disabled: disabled,
                                    text: '修改',
                                    handler: function () { index = datagrid_edititem(index, $this, opts); }
                                });
                                break;
                            case "save":
                                toolbar.push({
                                    iconCls: 'icon-save',
                                    disabled: disabled,
                                    text: '保存',
                                    handler: function () {
                                        if (index != null) {
                                            $this.datagrid('endEdit', index);
                                        }
                                    }
                                });
                                break;
                            case "reload":
                                toolbar.push({
                                    iconCls: 'icon-reload',
                                    text: '刷新',
                                    disabled: disabled,
                                    handler: function () {
                                        $this.datagrid('reload');
                                        index = null;
                                    }
                                });
                                break;
                            default:
                                toolbar.push(opts.toolbar[i]);
                                break;
                        }
                    }
                }

                $this.datagrid({
                    toolbar: toolbar
                });
            }   //end toolbar
        });
    };
    //treegrid
    $.fn.easyui_treegrid = function (options) {
        var index;

        var opts = $.extend({}, $.fn.easyui_treegrid.defaults, options);

        if (opts.type == '')
            return;

        var typemsg = gettype(opts.type);

        return this.each(function () {
            $this = $(this);
            //栏位
            var columns = [];
            for (var i = 0, j = 0; i < typemsg.columns.length; i++) {
                if (opts.columns_notitle.indexOf(typemsg.columns[i]) > -1) { continue; }
                //标题
                var titlename = opts.columns_title.length > j ? (opts.columns_title[j] != '' ? opts.columns_title[j] : typemsg.columns[i]) : typemsg.columns[i];
                //编辑
                var editor = opts.columns_editor.length > j ? ($.isEmptyObject(opts.columns_editor[j]) ? { type: 'text' } : opts.columns_editor[j]) : { type: 'text' };
                //格式化
                var formatter = opts.columns_formatter.length > j ? (opts.columns_formatter[j] == '-' ? '' : opts.columns_formatter[j]) : '';
                //宽度
                var width = opts.columns_width.length > j ? opts.columns_width[j] : '';
                //选择框
                var checkbox = opts.columns_checkbox.length > j ? opts.columns_checkbox[j] : false;

                columns.push({ field: typemsg.columns[i], title: titlename, width: width, editor: editor, formatter: formatter, checkbox: checkbox });
                j++;
            }

            //标题
            if (opts.title && (opts.title != '' || opts.type != '')) {
                $this.treegrid({
                    title: (opts.title == true || opts.title == '') ? opts.type : opts.title
                });
            }

            //双击是否可修改
            if (opts.toolbar.indexOf('edit') > -1) {
                $this.treegrid({
                    onDblClickRow: function (rowData) {
                        //如果不可修改 那么双击失效
                        if (opts.toolbar_disable.length > opts.toolbar.indexOf('edit')) {
                            if (opts.toolbar_disable[opts.toolbar.indexOf('edit')])
                                return;
                        }
                        if (index != null) {
                            $this.treegrid('endEdit', eval("rowData." + opts.idField));
                        }
                        else {
                            index = eval("rowData." + opts.idField);
                            $this.treegrid('beginEdit', index);
                        }
                    },
                    onClickRow: function (rowData) {
                        if (index != null) {
                            $this.treegrid('endEdit', index);
                        }
                    },
                    onAfterEdit: function (rowData, changes) {
                        if (eval("rowData." + opts.idField) == "0") {
                            eval("rowData." + opts.idField + "='';");
                        }
                        $.ajax({
                            url: '/common/savedata?type=' + opts.type + "&permissionmenuid=" + getmenuid(),
                            data: rowData,
                            type: 'post',
                            dataType: 'json',
                            success: function (data, status) {
                                if (data.validate == false) {
                                    alert(data.msg);
                                    //$this.treegrid('reload');
                                    //index = null;
                                }
                                else {
                                    $this.treegrid('reload');
                                    index = null;
                                }
                            },
                            error: function () {
                                alert("数据未处理，可能原因是系统异常或权限不足。");
                                //$this.datagrid('reload');
                                //index = null;
                            }
                        })
                    }
                });
            }

            $this.treegrid({
                url: opts.url == '' ? '/common/gettreedata?type=' + opts.type + "&permissionmenuid=" + getmenuid() : opts.url,
                idField: opts.idField,
                columns: [columns],
                fit: opts.fit,
                border: opts.border,
                treeField: opts.treeField
            });

            //toolbar
            if (opts.toolbar.length > 0) {
                var toolbar = [];
                if (Object.prototype.toString.call(opts.toolbar) != '[object Array]') {
                    toolbar = opts.toolbar;
                }
                else {
                    for (var i = 0; i < opts.toolbar.length; i++) {
                        var codes = pharos.permission.getpermissions();
                        var disabled = false;
                        if (opts.permissionCode.length > i && opts.permissionCode[i] != '')
                            disabled = codes.indexOf(opts.permissionCode[i]) == -1;
                        switch (opts.toolbar[i]) {
                            case "add":
                                toolbar.push({
                                    iconCls: 'icon-add',
                                    text: '添加',
                                    disabled: disabled,
                                    handler: function () { index = treegrid_additem(index, $this, opts); }
                                });
                                break;
                            case "del":
                                toolbar.push({
                                    iconCls: 'icon-remove',
                                    text: '删除',
                                    disabled: disabled,
                                    handler: function () { index = treegrid_removeitem(index, $this, opts.type); }
                                });
                                break;
                            case "edit":
                                toolbar.push({
                                    iconCls: 'icon-edit',
                                    text: '修改',
                                    disabled: disabled,
                                    handler: function () { index = treegrid_edititem(index, $this, opts.idField); }
                                });
                                break;
                            case "save":
                                toolbar.push({
                                    iconCls: 'icon-save',
                                    text: '保存',
                                    disabled: disabled,
                                    handler: function () {
                                        if (index != null) {
                                            $this.treegrid('endEdit', index);
                                        }
                                    }
                                });
                                break;
                            case "reload":
                                toolbar.push({
                                    iconCls: 'icon-reload',
                                    text: '刷新',
                                    disabled: disabled,
                                    handler: function () {
                                        $this.treegrid('reload');
                                        index = null;
                                    }
                                });
                                break;
                            default:
                                toolbar.push(opts.toolbar[i]);
                                break;
                        }
                    }
                }

                $this.treegrid({
                    toolbar: toolbar
                });
            }



        });
    }


    //私有函数：debugging
    //获取类数据
    function gettype(typename) {
        var d = {};
        $.ajax({
            url: '/common/gettypemsg?type=' + typename,
            async: false,
            type: 'post',
            dataType: 'json',
            success: function (data, status) {
                d = data;
            }
        });
        return d;
    }
    function getmenuid() {
        var url = window.location.href + "";
        var pid = $("body").data(url);
        if (!pid) {
            var u = "";
            if (url.indexOf("?") > -1) {
                u = (url + "&checkpermissionid=0");
            }
            else {
                u = (url + "?checkpermissionid=0");
            }

            $.ajax({
                url: u,
                type: 'get',
                dataType: 'json',
                async: false,
                success: function (data, status) {
                    pid = data;
                    $("body").data(url, data);
                },
                error: function () {
                    pid = "";
                    $("body").data(url, null);
                }
            });
        }
        return pid;
    }



    //datagrid私有方法
    function datagrid_additem(index, e, opts) {
        var temp = $.extend(true, {}, addDefaultTemp);
        if (opts.feedback == false && index != null) {
            e.datagrid('endEdit', index);
            index = null;
        }
        if (index == null || index == undefined) {
            e.datagrid('insertRow', {
                index: 0,
                row: opts.feedback ? temp : {}
            });
            e.datagrid('beginEdit', 0);
            index = 0;
        }
        return index;
    }
    function datagrid_removeitem(index, e, type, opts) {
        if (opts.feedback == false && index == null) {
            var r = e.datagrid('getSelected');
            if (r != null) e.datagrid('deleteRow', e.datagrid('getRowIndex', r))
            return index;
        }



        var row = e.datagrid('getSelected');
        if (row != null) {
            $.messager.confirm('系统提示!', '确定删除吗', function (r) {
                if (r) {
                    $.ajax({
                        url: '/common/deletedata?type=' + type + "&permissionmenuid" + getmenuid(),
                        dataType: 'json',
                        type: 'post',
                        data: row,
                        async: false,
                        success: function (data, status) {
                            if (data.validate == false) {
                                alert(data.msg);
                            }
                            e.datagrid('reload');
                            index = null;
                        }
                    });
                }
            });
        }
        else {
            $.messager.alert('警告', '请选择要删除行');
        }
        return index;
    }
    function datagrid_removeitem1(index, e, type, opts) {
        if (index == null) {
            var r = e.datagrid('getSelected');
            if (r != null) e.datagrid('deleteRow', e.datagrid('getRowIndex', r))
            return index;
        }
    }
    function datagrid_edititem(index, e, opts) {
        if (index == null) {
            var rowSelect = e.datagrid('getSelections');
            if (rowSelect == null || rowSelect == undefined || rowSelect.length <= 0) {
                $.messager.alert('警告', '请选择要修改行'); return;
            }
            index = e.datagrid('getRowIndex', e.datagrid('getSelected'));
            e.datagrid('beginEdit', index);
            opts.onEditCallBack(index,e);
        }
        return index;
    }
    //treegrid私有方法
    function treegrid_additem(index, e, opts) {
        if (index == null) {
            var a = eval("[{" + opts.treeField + ":''," + opts.idField + ":'0'}]");
            var sr = e.treegrid("getSelected");
            if (sr) {
                index = eval("sr." + opts.idField) + "";
                if (opts.PidField != '') {
                    a = [$.extend(a[0], eval("[{" + opts.PidField + ":'" + index + "'}]")[0])];
                }
                $.each(opts.baseField, function (index, value) {
                    a = [$.extend(a[0], eval("[{" + value + ":'" + eval("sr." + value) + "'}]")[0])];
                });
            }
            e.treegrid('append', {
                parent: index,
                data: a
            }).treegrid("beginEdit", 0);
            index = 0;
        }
        return index;
    }
    function treegrid_removeitem(index, e, type) {
        var row = e.treegrid('getSelected');
        if (row != null) {
            $.messager.confirm('系统提示!', '确定删除吗', function (r) {
                if (r) {
                    $.ajax({
                        url: '/common/deletedata?type=' + type + "&permissionmenuid" + getmenuid(),
                        dataType: 'json',
                        type: 'post',
                        data: row,
                        async: false,
                        success: function (data, status) {
                            if (data.validate == false) {
                                alert(data.msg);
                            }
                            e.treegrid('reload');
                            index = null;
                        }
                    });
                }
            });
        }
        else {
            $.messager.alert('警告', '请选择要删除行');
        }
        return index;
    }
    function treegrid_edititem(index, e, idfield) {
        if (index == null) {
            var rowSelect = e.treegrid('getSelections');
            if (rowSelect == null || rowSelect == undefined || rowSelect.length <= 0) {
                return;
            }
            index = eval("rowSelect[0]." + idfield);
            e.treegrid('beginEdit', index);
        }
        else {
            $.messager.alert('警告', '请选择要修改行');
        }
        return index;
    }



    //参数
    //datagrid
    $.fn.easyui_datagrid.defaults = {
        type: '',//后台类完整名称
        title: '',
        border: false,
        url: '',
        idField: '',//主键
        fit: true,
        pagination: true,
        singleSelect: true,
        feedback: true,
        rownumbers: true,
        columns_title: [],
        columns_notitle: ['Id'],
        columns_editor: [],
        columns_formatter: [],
        columns_width: [],
        columns_checkbox: [],
        columns_show: [],
        showcheck: false,
        SelectOnCheck: true,
        CheckOnSelect: true,
        detail_url: '',
        detail_height: 500,
        queryParams: {},
        toolbar: ['add', 'del', 'edit', 'save', 'reload'],
        permissionCode: [],
        addDefault: {},
        toolbar_disable: [],
        onLoadSuccess: function (data) { },
        onDblClickRow: function (data) { },
        onEditCallBack: function (index, el) { }
    };
    //treegrid
    $.fn.easyui_treegrid.defaults = {
        type: '',//后台类完整名称
        title: '',
        border: false,
        treeField: '',
        url: '',
        idField: 'Id',//主键
        PidField: '',
        fit: true,
        columns_title: [],
        columns_notitle: ['Id'],
        columns_editor: [],
        columns_formatter: [],
        toolbar_disable: [],
        columns_width: [],
        columns_checkbox: [],
        baseField: [],
        toolbar: ['add', 'del', 'edit', 'save', 'reload'],
        permissionCode: []
    };

    $.fn.easyui_msgBox = function (options) {
        var self = this;
        self.defaults = {
            isInterval: true,
            intervalNum: 60000,
            intervalLoad: $.noop,
            hasShow: false
        };
        self.jQMessager = null;
        self.interval = null;
        self.initInterval = function () {
            if (self.options.isInterval) {
                self.interval = setInterval(function () { self.options.intervalLoad(); }, self.options.intervalNum);
            }
        };
        self.clear = function () {
            if (this.interval != null) {
                clearInterval(this.interval);
            }
        };
        self.options = $.extend({}, this.defaults, options || {});
        self.initInterval();
        return self;
    };
})(jQuery);


