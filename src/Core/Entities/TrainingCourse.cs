using Core.Entities.Auth;

namespace Core.Entities;

public class TrainingCourse : BaseEntity
{
    public string Name { get; set; }
    public string CoachId { get; set; }
    public AppUser Coach { get; set; }
    public long Price { get; set; }
    public int Duration { get; set; }
    public string? Description { get; set; }
    public ICollection<CoachingRequest> CoachingRequest { get; set; }
}