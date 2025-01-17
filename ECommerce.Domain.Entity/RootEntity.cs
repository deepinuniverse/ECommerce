﻿namespace ECommerce.Domain.Entities;

public abstract class RootEntity : BaseEntity
{
    public int? CreatorUserId { get; set; }
    public int? EditorUserId { get; set; }
    public DateTime CreatedDate { get; set; } 
    public DateTime UpdatedDate { get; set; } 
}
