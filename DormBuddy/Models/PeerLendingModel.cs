using System.ComponentModel.DataAnnotations;

namespace DormBuddy.Models
{
    public class PeerLendingModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The UserId field is required.")]
        public string? UserId { get; set; }  // unique key

        [Required(ErrorMessage = "The Borrower field is required.")]
        public string? BorrowerId { get; set; }

        [Required(ErrorMessage = "The Amount field is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "The DueDate field is required.")]
        public DateTime DueDate { get; set; }

        public bool IsRepaid { get; set; } = false;
    }
}
