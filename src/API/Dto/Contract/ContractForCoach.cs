using Core.Entities.Status;

namespace API.Dto.Contract
{
    public class ContractForCoach
    {
        public int Id { get; set; }
        public string ClientName { get; set; }
        public string ClientGender { get; set; }
        public int ClientAge { get; set; }
        public string ClientEmail { get; set; }
        public string ClientPhoneNumber { get; set; }
        public string CourseName { get; set; }
        public int Duration { get; set; }
        public string? CancelReason { get; set; }
    }
}
