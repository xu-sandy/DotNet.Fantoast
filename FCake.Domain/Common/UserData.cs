using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCake.Domain.Common
{
    public class UserData
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string OfficeId { get; set; }
        public bool IsAdmin { get; set; }

        public List<RoleData> Roles { get; set; }
    }

    public class RoleData {
        public string RoleId { get; set; }
        public string RoleCode { get; set; }
        public string RoleName { get; set; }
    }
}
