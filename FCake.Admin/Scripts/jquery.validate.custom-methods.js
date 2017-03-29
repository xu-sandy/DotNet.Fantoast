jQuery.validator.addMethod("rule_username", function (value, element) {
  return this.optional(element) || /^\w(\w|\d|_){1,20}$/i.test(value);
}, "2至20位字母、数字或下划线，首字母必须是字母");

jQuery.validator.addMethod("rule_password", function (value, element) {
  return this.optional(element) || /^[^\s]{6,20}$/i.test(value);
}, "6至20位任意字符，不能包含任意的空白符，包括空格，制表符(Tab)，换行符，中文全角空格等");

jQuery.validator.addMethod("rule_cn_phone", function (value, element) {
  return this.optional(element) || /^(\d{4}-|\d{3}-)?(\d{8}|\d{7})$/i.test(value);
}, "无效的固定电话号码，示例：0123-12345678");

jQuery.validator.addMethod("rule_cn_mobile", function (value, element) {
  return this.optional(element) || /^1[3-8]\d{9}$/i.test(value);
}, "无效的手机号码");

jQuery.validator.addMethod("rule_qq", function (value, element) {
  return this.optional(element) || /^[1-9]\d{4,}$/i.test(value);
}, "无效的QQ号码");

jQuery.validator.addMethod("rule_cn_idno", function (value, element) {
  return this.optional(element) || /^\d{15}(\d\d[0-9xX])?$/i.test(value);
}, "无效的身份证号码");
// 邮政编码验证 
jQuery.validator.addMethod("rule_cn_iszipcode", function (value, element) {
  var tel = /^[0-9]{6}$/;
  return this.optional(element) || (tel.test(value));
}, "请正确填写您的邮政编码");
// 金额验证
jQuery.validator.addMethod("rule_cn_decimal", function (value, element) {
    return this.optional(element) || /^(?!0+(?:\.0+)?$)(?:[1-9]\d*|0)(?:\.\d{1,2})?$/.test(value);
}, "金额必须大于0并且只能精确到分");
