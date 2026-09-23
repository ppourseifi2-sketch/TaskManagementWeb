using System.Collections.Generic;

namespace TaskManagementWeb.Models
{
    public class MembersIndexViewModel
    {
        public int ProjectId { get; set; }
        public List<MemberRow> Members { get; set; }
    }
}