using System.ComponentModel.DataAnnotations;

namespace DormBuddy.Models {
    public class ForgotPasswordViewModel {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}