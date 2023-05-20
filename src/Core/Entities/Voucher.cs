namespace Core.Entities;

public class Voucher : BaseEntity
{
    public string Code { get; set; }
    public int Discount { get; set; }
    public string Desc { get; set; }
    public bool IsUsed { get; set; }
    public string ClientId { get; set; }
    public AppUser Client { get; set; }
}