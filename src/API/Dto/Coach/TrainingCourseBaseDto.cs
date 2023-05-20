using System.ComponentModel.DataAnnotations;

namespace API.Dto.Coach;

public class TrainingCourseBaseDto
{
    [Required(ErrorMessage = "Tên không được phép để trống")]
    public string Name { get; set; }
    [Required(ErrorMessage = "Giá tiền không được để trống")]
    [Range(10000, long.MaxValue, ErrorMessage = "Giá tiền phải lớn hơn 10,000 VND")]
    public long Price { get; set; }
    [Required(ErrorMessage = "Số buổi không được để trống")]
    [Range(0, int.MaxValue, ErrorMessage = "Số buổi phải lớn hơn 0")]
    public int Duration { get; set; }
    public string? Description { get; set; }
}