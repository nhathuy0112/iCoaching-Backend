using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.EFCore.Config;

public class CoachingRequestConfiguration : IEntityTypeConfiguration<CoachingRequest>
{
    public void Configure(EntityTypeBuilder<CoachingRequest> builder)
    {
        builder.HasKey(cr => new { cr.Id });
        
        builder.HasOne(cr => cr.Coach)
            .WithMany(coach => coach.CoachRequests)
            .HasForeignKey(cr => cr.CoachId);

        builder.HasOne(cr => cr.Client)
            .WithMany(client => client.ClientRequests)
            .HasForeignKey(cr => cr.ClientId);

        builder.HasOne(cr => cr.Course)
            .WithMany(course => course.CoachingRequest)
            .HasForeignKey(cr => cr.CourseId);
    }
}