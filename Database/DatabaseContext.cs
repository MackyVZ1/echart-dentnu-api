
using echart_dentnu_api.Models;
using Microsoft.EntityFrameworkCore;

namespace echart_dentnu_api.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<tbdentalrecorduserModel> Tbdentalrecordusers { get; set; } = null!;
        public DbSet<tbroleModel> Tbroles { get; set; } = null!;
        public DbSet<tbclinicModel> Tbclinics { get; set; } = null!;

        public DbSet<tpatientModel> Tpatients { get; set; } = null!;



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Table mappings
            modelBuilder.Entity<tbdentalrecorduserModel>().ToTable("tbdentalrecorduser");
            modelBuilder.Entity<tbroleModel>().ToTable("tbrole");
            modelBuilder.Entity<tbclinicModel>().ToTable("tbclinic");
            modelBuilder.Entity<tpatientModel>().ToTable("t_patient");

            base.OnModelCreating(modelBuilder);
        }
    }
}
