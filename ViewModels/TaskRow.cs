namespace TaskManagementWeb.Models
{
    public class TaskRow
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string AssignedUserName { get; set; }
    }
}
