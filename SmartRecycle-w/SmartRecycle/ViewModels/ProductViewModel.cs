using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SmartRecycle.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "اسم المنتج مطلوب")]
        [Display(Name = "اسم المنتج")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "وصف المنتج مطلوب")]
        [Display(Name = "وصف المنتج")]
        public string Description { get; set; }
        
        [Required(ErrorMessage = "سعر النقاط مطلوب")]
        [Range(1, int.MaxValue, ErrorMessage = "يجب أن يكون سعر النقاط أكبر من صفر")]
        [Display(Name = "سعر النقاط")]
        public int PointsPrice { get; set; }
        
        [Display(Name = "متاح")]
        public bool IsAvailable { get; set; } = true;
        
        [Required(ErrorMessage = "المخزون مطلوب")]
        [Range(0, int.MaxValue, ErrorMessage = "يجب أن يكون المخزون 0 على الأقل")]
        [Display(Name = "المخزون")]
        public int Stock { get; set; }
        
        [Required(ErrorMessage = "الفئة مطلوبة")]
        [Display(Name = "الفئة")]
        public int CategoryId { get; set; }
        
        [Display(Name = "صورة المنتج")]
        public IFormFile ImageFile { get; set; }
        
        // For editing scenarios, to keep track of existing image
        public string ExistingImagePath { get; set; }
    }
}