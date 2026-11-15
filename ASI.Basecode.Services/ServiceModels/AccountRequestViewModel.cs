using System.ComponentModel.DataAnnotations;
using ASI.Basecode.Resources.Constants;

namespace ASI.Basecode.Services.ServiceModels
{
    /// <summary>
    /// Public membership request fields. The annotations are the single validation surface
    /// shared by the MVC form and the service boundary.
    /// </summary>
    public class AccountRequestViewModel
    {
        [Required(ErrorMessage = "Student number is required.")]
        [RegularExpression(ProfileOptions.StudentNumberPattern, ErrorMessage = "Enter an 8-digit student number starting with 2.")]
        [StringLength(50, ErrorMessage = "Student number cannot exceed 50 characters.")]
        [Display(Name = "Student number")]
        public string StudentNumber { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(255, ErrorMessage = "Full name cannot exceed 255 characters.")]
        [Display(Name = "Full name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Course is required.")]
        [RegularExpression("^(BSCS|BSIT)$", ErrorMessage = "Choose BSCS or BSIT.")]
        [StringLength(100, ErrorMessage = "Course cannot exceed 100 characters.")]
        [Display(Name = "Course")]
        public string Course { get; set; }

        [Required(ErrorMessage = "Year level is required.")]
        [Range(1, 4, ErrorMessage = "Choose a year level from 1 to 4.")]
        [Display(Name = "Year level")]
        public int? YearLevel { get; set; }

        [Required(ErrorMessage = "Section is required.")]
        [RegularExpression(ProfileOptions.SectionPattern, ErrorMessage = "Choose a section from A to F.")]
        [StringLength(50, ErrorMessage = "Section cannot exceed 50 characters.")]
        [Display(Name = "Section")]
        public string Section { get; set; }

        [RegularExpression(ProfileOptions.MobileNumberPattern, ErrorMessage = "Enter an 11-digit mobile number starting with 09.")]
        [StringLength(30, ErrorMessage = "Mobile number cannot exceed 30 characters.")]
        [Display(Name = "Mobile number")]
        public string MobileNumber { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression(ProfileOptions.PasswordPattern, ErrorMessage = "Use at least 8 characters with a letter and a digit.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm your password.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }
    }
}
