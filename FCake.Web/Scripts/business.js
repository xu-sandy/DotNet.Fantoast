//手机格式判断
function checkFormatMobile(str,required) {
    if(required==null){
        required = true;
    }
    if (required) {
        if (String.isNE(str))
            return false;
        if (str != "" && !(/^1[3-8]\d{9}$/i.test(str))) {
            return false;
        }
    }
    else {
        if (str != "" && !(/^1[3-8]\d{9}$/i.test(str))) {
            return false;
        }
        return true;
    }
}
//加减框控件改变值
function changeNum(obj, changeValue) {
    var input = $(obj).parent().find("input");
    var value = input.val();
    try {
        value = value.replace(/\D/g, '');
        value = Number(value);
    }
    catch (e) {
        value = 0;
    }
    var newValue = Number(value) + changeValue;
    if (newValue < 1) {
        newValue = 1;
    }
    input.val(newValue).change();
}

//检查是否输入整形，否则更改为1
function checkInt(obj) {
    var value = $(obj).val();
    try {
        value = value.replace(/\D/g, '');
        if (value < 1)
            value = 1;
        $(obj).val(Number(value));
        $(obj).trigger("change");
    }
    catch (e) {
        $(obj).val(1);
        $(obj).trigger("change");
    }
}

//更改导航购物车数量值
function savecartnum(e, num, id) {
    $.ajax({
        url: '/cart/ChangeCartItemNum/' + id + "?num=" + num,
        dataType: 'json',
        type: 'post',
        success: function (data, status) {
            if (data.validate) {
                $(e).val(data.num);
                //更改导航显示的购物车数量
                $(".CartCount").text(data.cartnum);
            }
        },
        error: function () {

        }
    });
}

//日期格式化：yyyy.MM.dd
function formatDateType1(value) {
    if (!String.isNE(value)) {
        //value = value.replace('-', '/');
        value = value.toString().replace(/-/g, "/");
        var newData = new Date(value);
        return newData.format('yyyy.MM.dd');
    }
}
//日期时间格式化:yyyy-MM-dd hh:ss
function formatDateTime(value) {
    if (!String.isNE(value)) {
        //value = value.replace('-', '/');
        value = value.toString().replace(/-/g, "/");
        var newData = new Date(value);
        return newData.format('yyyy-MM-dd hh:ss');
    }
}

