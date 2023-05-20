using System.ComponentModel.DataAnnotations;
using API.Attribute;

namespace API.Dto.User;

public class BaseUserProfile
{
    [Required(ErrorMessage = "Email không được phép để trống")]
    [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Họ và tên không được phép để trống")]
    public string Fullname { get; set; }
    
    [Required(ErrorMessage = "Ngày tháng năm sinh không được phép để trống")]
    [ValidDob]
    public string Dob { get; set; }
    
    [Required(ErrorMessage = "Giới tính không được phép để trống")]
    [RegularExpression("^(Male|Female|Other)$", ErrorMessage = "Giới tính chỉ được chọn Male, Female hoặc Other")]
    public string Gender { get; set; }
    
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [RegularExpression("^(84|0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string PhoneNumber { get; set; }

}