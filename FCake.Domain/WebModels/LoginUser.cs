using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FCake.Domain.WebModels
{
    public class LoginUser
    {
        [Required]
        [Display(Name = "用户名")]
        public string UserName { get; set; }
        [Required]
        [Display(Name = "密码")]
        public string Password { get; set; }
        [Display(Name = "记住密码?")]
        public bool IsRememberMe { get; set; }
    }
}