var pharos = pharos || {};

(function (para) {
    para.gridHelper = {
        refresh: function (gridId, url, callback) {
            var queryParams = $("body").GetPostData();
            if (callback != null) {
                callback();
            }
            else {
                me.searchGrid(gridId, queryParams, url);
            }
        },
        searchGrid: function (gridId, queryParams, url) {
            if (queryParams != null) {
                $('#' + gridId).datagrid('options').queryParams = queryParams;
            }
            if (url != null) {
                $('#' + gridId).datagrid('options').url = url;
            }
            $("#" + gridId).datagrid('reload');
        },
        formatColumn: function formatColumn(value, list) {
            for (var i = 0; i < list.length; i++) {
                if (list[i].value == value) {
                    if (list[i].value != "")
                        return list[i].text;
                }
            }
            return value;
        }
    };
    var me = para.gridHelper;
})(pharos);

/*网格通用删除
    jqGird => jquery Object => 网格对象
    url => string => ajax url
    message => string => confirm infomation
    data => json => 参数
    return => string
*/
function CommonGridDeleteSelectd(jqGird, url, message, data) {
    var row = checkSelected();
    if (row != null) {
        $.messager.confirm('系统提示!', message, function (r) {
            if (r) {
                $.ajax({
                    url: url,
                    dataType: 'json',
                    type: 'post',
                    data: data,
                    success: function (data, status) {
                        if (data.validate == false) {
                            alert(data.msg);
                        }
                        jqGird.datagrid('reload');
                    }
                });
            }
        });
    }
}
function checkSelected() {
    var row = $("#dg").datagrid("getSelected");
    if (row) {
        return row;
    }
    else {
        $.messager.alert('提示', '请先选中要操作的行');
        return null;
    }
}