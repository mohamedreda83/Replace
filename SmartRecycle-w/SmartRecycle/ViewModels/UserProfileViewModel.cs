using System.ComponentModel.DataAnnotations;

namespace SmartRecycle.ViewModels
{
    public class UserProfileViewModel
    {
        public int ? Id { get; set; }

        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        [Display(Name = "اسم المستخدم")]
        public string ? Username { get; set; }

        [Display(Name = "كلمة المرور الجديدة")]
        [MinLength(6, ErrorMessage = "كلمة المرور يجب أن تكون 6 أحرف على الأقل")]
        public string ? NewPassword { get; set; }

        [Display(Name = "تأكيد كلمة المرور")]
        [Compare("NewPassword", ErrorMessage = "كلمة المرور وتأكيدها غير متطابقين")]
        public string ? ConfirmPassword { get; set; }

        [Display(Name = "النقاط")]
        public int ? Points { get; set; }
    }
}