﻿namespace ECommerce.Domain.Entities.HolooEntity;

public class HolooABail: BaseHolooEntity
{
    public string Fac_Code { get; set; }
    public string Fac_Type { get; set; }
    public string A_Code { get; set; }
    public short A_Index { get; set; }
    public double First_Article { get; set; }
    public double Few_Article { get; set; }
    public double Price_BS { get; set; }
    public double Unit_Few { get; set; }
    public double? DarsadTakhfif { get; set; }
    public double? TAKHFIFSATRIR { get; set; }
    public short VahedCode { get; set; }
    public string? ACode_C { get; set; }
}
