using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using PEIU.Models.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PEIU.Service.WebApiService
{
    public class AccountDataContext : IdentityDbContext<UserAccount, 
        Role, 
        string, 
        UserClaim, 
        UserRole, 
        IdentityUserLogin<string>, 
        IdentityRoleClaim<string>, 
        IdentityUserToken<string>>,         IDesignTimeDbContextFactory<AccountDataContext>
    {
        public DbSet<AggregatorGroup> AggregatorGroups { get; set; }
        public DbSet<VwContractorsite> VwContractorsites { get; set; }
        public DbSet<VwAggregatoruser> VwAggregatorusers { get; set; }
        public DbSet<UserLog> UserLogs { get; set; }

        public DbSet<SupervisorUser> SupervisorUsers { get; set; }
        //public DbSet<VwTemporarycontractoruser> VwTemporarycontractorusers { get; set; }
        public DbSet<VwContractoruser> VwContractorusers { get; set; }
        public DbSet<AggregatorUser> AggregatorUsers { get; set; }
        public DbSet<ContractorUser> ContractorUsers { get; set; }
        public DbSet<ContractorSite> ContractorSites { get; set; }
        public DbSet<ContractorAsset> ContractorAssets { get; set; }
        public DbSet<TemporaryContractorSite> TemporaryContractorSites { get; set; }

        public DbSet<TemporaryContractorAsset> TemporaryContractorAssets { get; set; }

        public AccountDataContext() { }

        public AccountDataContext(DbContextOptions<AccountDataContext> options) : base(options)
        {
            
            //AccountRecordContext s;s.Find()
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<UserAccount>().ToTable("UserAccounts");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<UserClaim>().ToTable("UserClaims");
            builder.Entity<UserRole>().ToTable("UserRoles");
            builder.Entity<Role>().ToTable("Roles");
            builder.Entity<UserAccount>()
                .HasOne<ContractorUser>(x => x.Contractor)
                .WithOne(x => x.User)
                .HasForeignKey<ContractorUser>(ad => ad.UserId);
            builder.Entity<UserAccount>()
                .HasOne<AggregatorUser>(x => x.Aggregator)
                .WithOne(x => x.User)
                .HasForeignKey<AggregatorUser>(ad => ad.UserId);
            builder.Entity<UserAccount>()
                .HasOne<SupervisorUser>(x => x.Supervisor)
                .WithOne(x => x.User)
                .HasForeignKey<SupervisorUser>(ad => ad.UserId);
        }

        public AccountDataContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
            var builder = new DbContextOptionsBuilder<AccountDataContext>();
            var connectionString = configuration.GetConnectionString("peiu_account_connnectionstring");
            builder.UseMySql(connectionString);
            return new AccountDataContext(builder.Options);
        }

        //protected override void OnModelCreating(ModelBuilder builder)
        //{
        //    base.OnModelCreating(builder);
        //    builder.Entity<AssetDevices>().HasKey(x => x.PK);
        //    builder.Entity<AssetLocation>().HasKey(m => m.SiteId);
        //    builder.Entity<ReservedAssetLocation>().HasKey(x => x.ID);

        //    //builder.Entity<IdentityUser>().ToTable("MyUsers").Property(p => p.Id).HasColumnName("UserId");
        //    //builder.Entity<AccountModel>().ToTable("MyUsers").Property(p => p.Id).HasColumnName("UserId");
        //    //builder.Entity<IdentityUserRole<string>>().ToTable("MyUserRoles");
        //    //builder.Entity<IdentityUserLogin<string>>().ToTable("MyUserLogins");
        //    //builder.Entity<IdentityUserClaim<int>>().ToTable("MyUserClaims");
        //    //builder.Entity<IdentityRole>().ToTable("MyRoles");
        //    //// shadow properties
        //    //builder.Entity<EventLogData>().Property<DateTime>("UpdatedTimestamp");
        //    //builder.Entity<SourceInfo>().Property<DateTime>("UpdatedTimestamp");


        //}
    }
}
