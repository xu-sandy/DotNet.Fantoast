using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCake.Domain.WebModels
{
    public class UserSession
    {
        public UserSession() { }
        public UserSession(string userId,string userName,string fullName) {
            UserId = userId;
            UserName = userName;
            FullName = fullName;
        }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
    }
}
