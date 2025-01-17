﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ECommerce.Domain.Entities;

public class Category : RootEntity
{
    [StringLength(50, MinimumLength = 3, ErrorMessage = @"حداقل 3 و حداکثر 50 کاراکتر")]
    [Display(Name = "نام")]
    [Required(ErrorMessage = @"{0} را وارد کنید")]
    public string Name { get; set; }

    //  عمق گروه را مشخص می کند
    [Range(0, 5, ErrorMessage = "زیرشاخه بالاتر از 5 امکانپذیر نیست")]
    public int Depth { get; set; } = 0;

    // لیست پدر های گروه جاری را نشان می دهد مثلا 2/3/4/5
    [StringLength(100, MinimumLength = 3, ErrorMessage = @"حداقل 3 و حداکثر 100 کاراکتر")]
    [Display(Name = "آدرس")]
    [Required(ErrorMessage = @"{0} را وارد کنید")]
    public string Path { get; set; }

    [Display(Name = "فعال")] public bool IsActive { get; set; } = true;

    [Display(Name = "ترتیب نمایش")] public int DisplayOrder { get; set; } = 0;

    [Display(Name = "عکس")]
    [Required(ErrorMessage = @"{0} را وارد کنید")]
    public string ImagePath { get; set; } = string.Empty;

    //ForeignKey
    public int? ParentId { get; set; }
    public Category? Parent { get; set; }

    [JsonIgnore] public ICollection<Category>? Categories { get; set; } = new List<Category>();

    [JsonIgnore] public ICollection<SlideShow>? SlideShows { get; set; }

    [JsonIgnore] public ICollection<Product>? Products { get; set; }

    //public ICollection<ProductAttributeGroup> AttributeGroups { get; set; }

    public int? DiscountId { get; set; }
    public Discount? Discount { get; set; }
}