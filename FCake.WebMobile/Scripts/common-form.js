
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