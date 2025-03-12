using System.ComponentModel.DataAnnotations;

namespace Identity.API.Models.ManageViewModels
{
    public class AddPhoneNumberViewModel
    {
        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; init; }
    }
}
