using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DormBuddy.Models{
    public class TaskModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The UserId field is required.")]
        public string UserId { get; set; }  // unique key

        [Required(ErrorMessage = "The TaskName field is required.")]
        public string TaskName { get; set; }

        [Required(ErrorMessage = "The AssignedTo field is required.")]
        public string AssignedTo { get; set; }

        [Required(ErrorMessage = "The DueDate field is required.")]
        public DateTime DueDate { get; set; }

        public bool IsCompleted { get; set; }
    }
}
