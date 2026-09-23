using System.Collections.Generic;

namespace TaskManagementWeb.Models
{
    public class MembersAddViewModel
    {
        public int ProjectId { get; set; }
        public List<Users> AvailableUsers { get; set; }
    }
}