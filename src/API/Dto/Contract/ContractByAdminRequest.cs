using System.ComponentModel.DataAnnotations;

namespace API.Dto.Contract;

public class ContractByAdminRequest
{
    [Required(ErrorMessage = "Không được để trống Id huấn luyện viên")]
    public string CoachId { get; set; }
    
    [Required(ErrorMessage = "Không được để trống mã khoá tập")]
    public int CourseId { get; set; }

    public string? Description { get; set; }
    public string? CancelReason { get; set; }
    
}