using Core.Entities.Status;

namespace API.Dto.Certification
{
    public class CertRequestInfo
    {
        public int CertId { get; set; }
        public string Username { get; set; }
        public string Fullname { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string AvatarUrl { get; set; }
        public string Status { get; set; }
    }
}
