using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.EFCore.Config;

public class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.HasKey(c => new { c.Id });
        
        builder.HasOne(c => c.Coach)
            .WithMany(coach => coach.CoachContracts)
            .HasForeignKey(c => c.CoachId);

        builder.HasOne(c => c.Client)
            .WithMany(client => client.ClientContracts)
            .HasForeignKey(c => c.ClientId);
    }
}