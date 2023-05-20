namespace API.Dto.Certification
{
    public class CertRequestDetail : CertRequestInfo
    {
        public string AvatarUrl { get; set; }
        public List<string> IdImages { get; set; }
        public List<string> CertImages { get; set; }
    }
}
