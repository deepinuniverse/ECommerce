﻿using System.ComponentModel.DataAnnotations;

namespace Entities;

public class Supplier : BaseEntity
{
    [Display(Name = "نام")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "حداقل 3 و حداکثر 50 کاراکتر")]
    [Required(ErrorMessage = @"{0} را وارد کنید")]
    public string Name { get; set; }

    [Display(Name = "توضیحات")] public string? Description { get; set; }

    [Display(Name = "موبایل")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "تعداد ارقام غلط است")]
    public string? Mobile { get; set; }

    [Display(Name = "تلفن")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "تعداد ارقام غلط است")]
    public string? Tell { get; set; }

    [Display(Name = "آدرس")] public string? Address { get; set; }

    [Display(Name = "آدرس وب سایت")]
    [StringLength(300, ErrorMessage = "لطفا آدرس صفحه اول را وارد کنید، تعداد کاراکتر نباید بیشتر از 300 باشد")]
    [Url]
    public string? Website { get; set; }

    [Display(Name = "ایمیل")]
    [StringLength(50, ErrorMessage = "تعداد کاراکتر نباید بیشتر از 50 باشد")]
    [EmailAddress]
    public string? Email { get; set; }

    [Display(Name = "نام رابط")]
    [StringLength(50, ErrorMessage = "تعداد کاراکتر نباید بیشتر از 50 باشد")]
    public string? ConnectionName { get; set; }

    //foreignKey
    public ICollection<Product>? Products { get; set; }
}