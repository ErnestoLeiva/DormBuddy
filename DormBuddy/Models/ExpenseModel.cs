using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DormBuddy.Models
{
    public class ExpenseModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? ExpenseName { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [Required]
        public string? SharedWith { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string? UserId { get; set; }

        [Required]
        public bool isSplit { get; set; }
    }
}