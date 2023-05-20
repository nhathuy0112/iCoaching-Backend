namespace API.Dto.Client;

public class VoucherDto
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Desc { get; set; }
    public string Discount { get; set; }
    public bool IsUsed { get; set; }
}