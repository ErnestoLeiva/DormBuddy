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

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [Required]
        public string? SharedWith { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? CreatedBy { get; set; }

        [Required]
        public bool isSplit { get; set; }
        


    }

}
