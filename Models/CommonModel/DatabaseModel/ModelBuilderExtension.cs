using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace PEIU.Models.Database
{
    public static class ModelBuilderExtension
    {

        public static void ModelBuildUp(this ModelBuilder builder)
        {
            builder.Entity<AggregatorGroup>()
                .HasMany(x => x.AggregatorUsers)
                .WithOne(x => x.AggregatorGroup)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired();
            builder.Entity<AggregatorGroup>()
                .HasMany(x => x.ContractorUsers)
                .WithOne(x => x.AggregatorGroup)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired();

            builder.Entity<ContractorUser>()
                .HasMany(x => x.ContractorSite)
                .WithOne(x => x.ContractUser)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired();

            builder.Entity<ContractorSite>()
                .HasMany(x => x.ContractorAssets)
                .WithOne(x => x.ContractorSite)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired();
        }
    }
}
