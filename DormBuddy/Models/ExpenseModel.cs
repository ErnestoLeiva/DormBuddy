using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DormBuddy.Models
{
    public class ExpenseModel
    {

        [Key]
        public int Id { get; set; }
        
        [Required, StringLength(100)]
        public string ExpenseName { get; set; } = string.Empty; // Initialized to avoid null issues

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [StringLength(191)]
        public string UserId { get; set; } = string.Empty; // Initialized to avoid null issues

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = new ApplicationUser(); // Initialize to avoid null warnings

        public ICollection<ApplicationUser> SharedWith { get; set; } = new List<ApplicationUser>();
    }

}
