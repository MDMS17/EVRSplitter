using Microsoft.EntityFrameworkCore;

namespace EVRSplitter
{
    public class EVRContext : DbContext
    {
        private string _connectionString;
        public EVRContext(string connectionString)
        {
            _connectionString = connectionString;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionBuilder)
        {
            optionBuilder.UseSqlServer(_connectionString);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Query<EVRByTradingPartner>().ToView("vEVRByTradingPartner");
            modelBuilder.Query<EVRDetail>().ToView("vEVRDetail");
        }

        public DbQuery<EVRByTradingPartner> EVRByTradingPartners { get; set; }
        public DbQuery<EVRDetail> EVRDetails { get; set; }
        public DbSet<EncounterTrackModel> EncounterTrack { get; set; }
    }
}
