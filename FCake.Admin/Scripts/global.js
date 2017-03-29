var pharos = pharos || {};



(function (para) {
    //私有变量
    var isload = true;              //是否加载
    var ischeck = true;           //是否检查
    var isadmin = false;          //是否为超级管理员
    var hasload = false;           //是否已加载
    var permissionlist = [];       //权限列表

    para.permission = {
        //获取当前目录ID
        getmenuid: function (u) {
            var url = u ? u : window.location.href + "";
            var pid = $("body").data(url);
            if (!pid) {
                u = "";
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
        },
        //开始检查权限
        startcheck: function () {
            if (ischeck == false)
                return;
            $("[data-check]").each(function () {
                $this = $(this);
                if ($this.hasClass("easyui-linkbutton")) {
                    $this.linkbutton(pharos.permission.checkcode($this.attr("data-check")) ? "enable" : "disable");
                }
            });
        },
        //根据权限代码判断是否有权限
        checkcode: function (code) {
            if (isadmin)
                return true;
            for (var i = 0, j; j = permissionlist[i]; i++) {
                if (j == code)
                    return true;
            }
            return false;
        },
        //设定是否检查
        ischeck:function(bool){
            ischeck = (bool == true);
        },
        //设定是否加载权限
        isload:function(bool){
            isload = (bool == true);
        },
        //加载权限
        load:function(){
            if (isload == false)
                return;
            pharos.permission.loadpermissions();
        },
        //获取当前权限，如果还没加载，则先加载后返回权限
        getpermissions: function () {
            if (hasload)
                return permissionlist;
            pharos.permission.loadpermissions();
            return permissionlist;
        },
        //加载权限(无需判断是否要加载)
        loadpermissions: function () {
            if (hasload)
                return;
            var url = window.location.href + "";
            if (url.indexOf("?") > -1) {
                url += "&checkpermission=0";
            }
            else {
                url += "?checkpermission=0";
            }
            $.ajax({
                url: url,
                type: 'get',
                dataType: 'json',
                async: false,
                success: function (data, status) {
                    isadmin = data.all;
                    permissionlist = data.data;
                    hasload = true;
                },
                error: function () {
                    isadmin = false;
                }
            });
        }
    }

    para.g = {
        //public
        getUrlParam: function (name) {
            //构造一个含有目标参数的正则表达式对象  
            var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)");
            //匹配目标参数  
            var r = window.location.search.substr(1).match(reg);
            //返回参数值  
            if (r != null) return unescape(r[2]);
            return null;
        },
        dialogDefaults: { modal: true, closed: true },
        //public
        openDialog: function (config) {
            config = config || {};

            var defaults = pharos.g.dialogDefaults;
            for (var i in defaults) {
                if (config[i] === undefined) config[i] = defaults[i];
            }
            if (String.isNE(config.id)) {
                config.id = "win_" + pharos.g.newGuid();
            }
            else {
                config.id = "win_" + config.id;
            }
            if (!String.isNE(config.url)) {
                if (String.isNE(config.window)) {
                    config.window = window.top;
                }
                var iframe = $('<iframe scrolling="auto" frameborder="0" style="width:100%;height:99%;""></iframe>');
                iframe.attr("src", config.url);
                var dialog = $('<div></div>').append(iframe);
                dialog.attr("id", config.id);

                var win = config.window;
                var oldDialog = win.$("body").find("#" + config.id);
                if (oldDialog[0] == null) {
                    win.$("body").append(dialog);
                } else {
                    oldDialog = dialog;
                }
                window.top.$("body").data("jq_" + config.id, $);
                window.top.$("body").data(config.id, win.$(dialog));
                win.$(dialog).window(config);
                win.$(dialog).window("open");
            }
        },
        currentMenuID: function () {
            var pid = $("body").data("menuid");
            if (!pid) {
                var url = window.location.href + "";
                if (url.indexOf("?") > -1) {
                    url += "&checkpermissionid=0";
                }
                else {
                    url += "?checkpermissionid=0";
                }

                $.ajax({
                    url: url,
                    type: 'get',
                    dataType: 'json',
                    async: false,
                    success: function (data, status) {
                        pid = data;
                        $("body").data("menuid", data);
                    },
                    error: function () {
                        pid = "";
                        $("body").data("menuid", null);
                    }
                });
            }
            return pid;

        },
        newGuid: function () {
            ///	<summary>
            ///		生成32位Guid的字符串
            ///	</summary>
            ///	<returns type="String" />
            var array = new Array("0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F");
            var guidArray = new Array(32);
            for (var i = 0; i < 21; i++)
                guidArray[i] = array[Math.floor(Math.random() * 16)];
            var baseTime = new Date().valueOf().toString(16).toUpperCase().substr(0, 11);
            for (var i = 21; i < 32; i++)
                guidArray[i] = baseTime.substr(i - 21, 1);
            var newGuid = "";
            var temp;
            var length = guidArray.length;
            for (var i = 0; i < 32; i++) {
                var _radom = Math.floor(Math.random() * length);
                if (i == 8 || i == 12 || i == 16 || i == 20)
                    newGuid += "" + guidArray[_radom];
                else
                    newGuid += guidArray[_radom];
                guidArray[_radom] = guidArray[length - 1];
                delete guidArray[length - 1];
                length--;
            }
            return newGuid;
        }
        
    };

        //json方法
    para.json = {
        //编辑JSON对象 json：对象 property：属性 value：属性值 
        //如果没有value 则删除属性
        //如果有属性而又要加入 则加入失败 但不会返回错误
        edit: function (json, property, value, isjoin) {
            //如果value被忽略
            if (typeof value == 'undefined') {
                delete json[property];
            }
            //如果不含有属性
            if (!(property in json)) {
                json[property] = value;
                return;
            }
            else {
                if (typeof isjoin == 'undefined') {
                    delete json[property];
                    json[property] = value;
                    return;
                }
                else {
                    json[property] = (json[property] + ',' + value);
                    return;
                }
            }
        },
        //将json转化成string格式
        tostring: function (O) {
            var S = [];
            var J = "";
            if (Object.prototype.toString.apply(O) === '[object Array]') {
                for (var i = 0; i < O.length; i++)
                    S.push(pharos.json.tostring(O[i]));
                J = '[' + S.join(',') + ']';
            }
            else if (Object.prototype.toString.apply(O) === '[object Date]') {
                J = "new Date(" + O.getTime() + ")";
            }
            else if (Object.prototype.toString.apply(O) === '[object RegExp]' || Object.prototype.toString.apply(O) === '[object Function]') {
                J = O.toString();
            }
            else if (Object.prototype.toString.apply(O) === '[object Object]') {
                for (var i in O) {
                    O[i] = typeof (O[i]) == 'string' ? '"' + O[i] + '"' : (typeof (O[i]) === 'object' ? pharos.json.tostring(O[i]) : O[i]);
                    S.push('"' + i.toString() + '":' + (O[i] == null ? 'null' : O[i].toString()));
                }
                J = '{' + S.join(',') + '}';
            }
            return J;
        },
        //将json转化成表单数据传回后台
        //type : post get
        submit: function (json, type, url) {
            $form = $('<form></form>').attr('method', type).attr('action', url);
            for (var i in json) {
                $form.append($("<input name='" + i.toString() + "'/>").val((json[i].toString())));
            }
            $("body").append($form);
            $form.hide();
            $form.submit();
        },
        //如果数据里面为json,返回json.targetProperty==targetValue的json.returnProperty,否则返回nullReturnValue
        getArrayValue: function (array, targetProperty, targetValue, returnProperty, nullReturnValue) {
            for (var i = 0; i < array.length; i++) {
                if (targetProperty in array[i] && array[i][targetProperty] == targetValue) {
                    if (returnProperty in array[i])
                        return array[i][returnProperty];
                }
            }
            return nullReturnValue;
        },
        //将表单转换成json
        formtojson: function (e) {
            var array = e.serializeArray();
            var json = {};
            $.each(array, function (index, value) {
                pharos.json.edit(json, value.name, value.value, true);
            });
            return json;
        }
    };
    var me = para.g;
})(pharos);

/*====================jquery扩展====================*/

(function ($) {
    $.fn.GetPostData = function () {
        var data = {};

        $(this).find(".datacontrol").each(function (i, value) {
            var field = $(value).attr("name");
            if (field == null) {
                field = $(value).attr("id");
            }
            if (value.tagName == "INPUT") {
                if (value.type == "checkbox") {
                    if ($(value).prop("checked") == true) {
                        if (data[field]) {
                            var a = +$(value).val();
                            if (a == "") {
                                a = "1";
                            }
                            data[field] = data[field] + "," + a;
                        } else {
                            var a = +$(value).val();
                            data[field] = "1"
                        }
                    }
                }
                else if (value.type == "radio") {
                    if ($(value).attr("checked") == true) {
                        data[field] = $(value).val();
                    }
                }
                else {
                    if ($(value).val() != "") {
                        data[field] = $(value).val();
                    }
                }
            }

            else if (value.tagName == "SELECT") {
                if ($(value).val() != "") {
                    alert(data[field] + "||" + $(value).val());
                    //data[field] = $(value).val();
                    data[field] = $(value).combo("getValue");
                }
            }
            else if (value.tagName == "DIV") {
                data[field] = $(value).html();
            }
            else if (value.tagName == "IMG") {
                data[field] = $(value).attr("src");
            }
            else if (value.tagName == "SPAN") {
                data[field] = $(value).html();
            }
            else if (value.tagName == "TEXTAREA") {
                if ($(value).val() != "") {
                    data[field] = $(value).val();
                }
            }

        });
        return data;
    };
    $.fn.extend({
        json: function () {
            var array = this.serializeArray();
            var b = "";
            $.each(array, function (index, value) {
                if (b == "")
                    b = value.name + ":'" + value.value + "'";
                else
                    b += "," + value.name + ":'" + value.value + "'";
            });

            return eval("({" + b + "})");
        }
    });
})(jQuery);


/*====================javascript扩展====================*/

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
Array.prototype.indexOf = function (e) {
    for (var i = 0, j; j = this[i]; i++) {
        if (j == e) { return i; }
    }
    return -1;
}

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



