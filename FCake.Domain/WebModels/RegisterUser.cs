using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FCake.Domain.WebModels
{
    public class RegisterUser
    {
        [Required]
        [Display(Name = "用户名")]
        public string UserName { get; set; }
        [Required]
        [Display(Name = "验证码")]
        public string Captchas { get; set; }
        [Required]
        [Display(Name = "密码")]
        public string Password { get; set; }
        [Required]
        [Display(Name = "短信验证码")]
        public string MsgVerifyCode { get; set; }
    }
}