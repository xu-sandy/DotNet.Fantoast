using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCake.Domain.WebModels
{
    public class MemberSession
    {
        public MemberSession() { }
        public MemberSession(string id, string loginName, string memberName)
        {
            MemberId = id;
            LoginName = loginName;
            MemberName = memberName;
            DisplayName = string.IsNullOrEmpty(memberName) ? loginName.Substring(0, 3) + "****" + loginName.Substring(7, 4) : memberName;
        }
        public string MemberId { get; set; }
        public string LoginName { get; set; }
        public string MemberName { get; set; }
        public string DisplayName { get; set; }
    }
}
