using System.Collections.Generic;

namespace TaskManagementWeb.Models
{
    public class TasksIndexViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectTitle { get; set; }
        public string MyRole { get; set; }
        public string StatusFilter { get; set; }
        public List<TaskRow> Tasks { get; set; }
    }
}