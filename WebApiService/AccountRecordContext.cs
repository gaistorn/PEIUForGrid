using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PES.Models;
using Power21.PEIUEcosystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PES.Service.WebApiService
{
    public class AccountRecordContext : IdentityDbContext<AccountModel>
    {
        public DbSet<DevicesInfos> DevicesInfos { get; set; }
        public DbSet<AssetLocation> AssetLocations { get; set; }

        public AccountRecordContext(DbContextOptions<AccountRecordContext> options) : base(options)
        {
            
            //AccountRecordContext s;s.Find()
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<DevicesInfos>().HasKey(x => x.DeviceId);
            builder.Entity<AssetLocation>().HasKey(m => m.PK);

            //// shadow properties
            //builder.Entity<EventLogData>().Property<DateTime>("UpdatedTimestamp");
            //builder.Entity<SourceInfo>().Property<DateTime>("UpdatedTimestamp");


        }
    }

}
