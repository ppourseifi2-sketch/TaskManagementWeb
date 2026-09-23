using System.Collections.Generic;

namespace TaskManagementWeb.Models
{
    public class TasksEditViewModel
    {
        public int ProjectId { get; set; }
        public ProjectTask Task { get; set; }
        public List<Users> ProjectUsers { get; set; }
    }
}