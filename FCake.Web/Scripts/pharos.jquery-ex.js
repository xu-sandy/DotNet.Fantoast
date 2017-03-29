/*jquery扩展==================================================*/

(function ($) {
    $.fn.extend({
        //输入控件值验证方法
        controlValidate: function (type, required) {
            type = type.toLocaleString();
            required = required | true;
            var val = $(this).val();
            if (!required) {
                if (val == "") {
                    return true;
                }
            }
            switch (type) {
                case "cnmobile":
                    return (/^1[3-8]\d{9}$/i.test(val));
                case "postcode":
                    return (/^[1-9]\d{5}$/i.test(val));
                case "cntel":
                    return (/^(\d{4}-|\d{3}-)?(\d{8}|\d{7})$/i.test(val));
                case "email":
                    return (/\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*/i.test(val));
                default:
                    return false;
            }
        }
    });
})(jQuery);
/*jquery扩展结束==============================================*/

/*
**************************************************************
*/

/*javascript扩展==============================================*/
/*placeholder=================================*/
/*判断浏览器是否支持 placeholder属性*/
function isPlaceholder() {
    var input = document.createElement('input');
    return 'placeholder' in input;
}
$(function () {
    if (!isPlaceholder()) {//不支持placeholder 用jquery来完成  
        $(document).ready(function () {
            if (!isPlaceholder()) {
                $("input").not("input[type='password']").each(//把input绑定事件 排除password框  
                    function () {
                        if ($(this).val() == "" && $(this).attr("placeholder") != "") {
                            $(this).val($(this).attr("placeholder"));
                            $(this).focus(function () {
                                if ($(this).val() == $(this).attr("placeholder")) $(this).val("");
                            });
                            $(this).blur(function () {
                                if ($(this).val() == "") $(this).val($(this).attr("placeholder"));
                            });
                        }
                    });
                //对password框的特殊处理1.创建一个text框 2获取焦点和失去焦点的时候切换  
                $("input[type=password]").each(function (i, item) {
                    var pwdVal = $(item).attr('placeholder');
                    var newId = $(item).attr("id") + "pwdPlaceholder";
                    $(item).after('<input class="input_public" id="' + newId + '" type="text" value=' + pwdVal + ' autocomplete="off" />');
                    var pwdPlaceholder = $('#' + newId);
                    pwdPlaceholder.show();
                    $(item).hide();

                    pwdPlaceholder.focus(function () {
                        pwdPlaceholder.hide();
                        $(item).show();
                        $(item).focus();
                    });

                    $(item).blur(function () {
                        if ($(item).val() == '') {
                            pwdPlaceholder.show();
                            $(item).hide();
                        }
                    });
                });
            }
        });

    }
});
/*placeholder end=============================*/
/*Date========================================*/
// 对Date的扩展，将 Date 转化为指定格式的String   
// 月(M)、日(d)、小时(h)、分(m)、秒(s)、季度(q) 可以用 1-2 个占位符，   
// 年(y)可以用 1-4 个占位符，毫秒(S)只能用 1 个占位符(是 1-3 位的数字)   
// 例子：   
// (new Date()).Format("yyyy-MM-dd hh:mm:ss.S") ==> 2006-07-02 08:09:04.423   
// (new Date()).Format("yyyy-M-d h:m:s.S")      ==> 2006-7-2 8:9:4.18   
Date.prototype.format = function (fmt) { //author: meizz   
    var o = {
        "M+": this.getMonth() + 1,                 //月份   
        "d+": this.getDate(),                    //日   
        "h+": this.getHours(),                   //小时   
        "m+": this.getMinutes(),                 //分   
        "s+": this.getSeconds(),                 //秒   
        "q+": Math.floor((this.getMonth() + 3) / 3), //季度   
        "S": this.getMilliseconds()             //毫秒   
    };
    if (/(y+)/.test(fmt))
        fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
    for (var k in o)
        if (new RegExp("(" + k + ")").test(fmt))
            fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
    return fmt;
};
/*Date结束====================================*/

/*String======================================*/
/*判断txt值是否为null或空字符串
    txt => string => 源字符串
    return => bool
*/
String.isNullOrEmpty = function (txt) {
    if (txt === undefined || txt === null || txt === "") {
        return true;
    }
    return false;
}
String.isNE = String.isNullOrEmpty;
/*判断txt值是否为真
    txt => string => 字符串对象
    return => bool
*/
String.isTrue = function (txt) {
    return !!txt;
}
/*格式化字符串
    txt => string => 原始字符串
    obj => array => 替换值集合
    return => string
*/
String.format = function (txt, objs) {
    if (typeof (Ext) == "undefined" || $.isArray(objs)) {
        var t = null;
        txt = txt.replace(/\{[0-9]+\}/gi, function ($1) {
            t = objs[$1.replace(/[\{\}]+/gi, "")];
            if (t != null)
                return t;
            return $1;
        });
        return txt;
    }
    else {
        var a = Ext.toArray(arguments, 1);
        return txt.replace(/\{(\d+)\}/g, function (c, d) { return a[d]; });
    }
}
/*String结束==================================*/

/*Array=======================================*/
Array.prototype.indexOf = function (e) {
    for (var i = 0, j; j = this[i]; i++) {
        if (j == e) { return i; }
    }
    return -1;
}
/*Array结束===================================*/

/*Number======================================*/
// Extend the default Number object with a formatMoney() method:
// usage: someVar.formatMoney(decimalPlaces, symbol, thousandsSeparator, decimalSeparator)
// defaults: (2, "$", ",", ".")
Number.prototype.formatMoney = function (places, symbol, thousand, decimal) {
    places = !isNaN(places = Math.abs(places)) ? places : 2;
    symbol = symbol !== undefined ? symbol : "￥";
    thousand = thousand || ",";
    decimal = decimal || ".";
    var number = this,
        negative = number < 0 ? "-" : "",
        i = parseInt(number = Math.abs(+number || 0).toFixed(places), 10) + "",
        j = (j = i.length) > 3 ? j % 3 : 0;
    return symbol + negative + (j ? i.substr(0, j) + thousand : "") + i.substr(j).replace(/(\d{3})(?=\d)/g, "$1" + thousand) + (places ? decimal + Math.abs(number - i).toFixed(places).slice(2) : "");
};
/*Number结束==================================*/
/*javascript扩展结束==========================================*/




/*============================================================*/