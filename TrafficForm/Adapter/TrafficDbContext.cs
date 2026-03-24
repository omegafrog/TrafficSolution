using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TrafficForm.Domain;

namespace TrafficForm.Adapter
{
    public class TrafficDbContext : DbContext
    {
        public DbSet<HighwayFavorite> HighwayFavorites { get; set; }
        public DbSet<CoordinateFavorite> CoordinateFavorites { get; set; }

        public TrafficDbContext(DbContextOptions<TrafficDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HighwayFavorite>(entity =>
            {
                entity.HasKey(e => e.FavoriteId);
                entity.Property(e => e.HighwayNo).IsRequired();
                entity.Property(e => e.Name).HasMaxLength(100);

                // MapViewSnapshot을 Owned Type으로 매핑
                entity.OwnsOne(e => e.View, view =>
                {
                    view.Property(v => v.Latitude).HasColumnName("latitude");
                    view.Property(v => v.Longitude).HasColumnName("longitude");
                    view.Property(v => v.ZoomLevel).HasColumnName("zoom_level");
                    view.Property(v => v.MinLatitude).HasColumnName("min_latitude");
                    view.Property(v => v.MaxLatitude).HasColumnName("max_latitude");
                    view.Property(v => v.MinLongitude).HasColumnName("min_longitude");
                    view.Property(v => v.MaxLongitude).HasColumnName("max_longitude");
                });
            });

            modelBuilder.Entity<CoordinateFavorite>(entity =>
            {
                entity.HasKey(e => e.FavoriteId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

                // MapViewSnapshot을 Owned Type으로 매핑
                entity.OwnsOne(e => e.View, view =>
                {
                    view.Property(v => v.Latitude).HasColumnName("latitude");
                    view.Property(v => v.Longitude).HasColumnName("longitude");
                    view.Property(v => v.ZoomLevel).HasColumnName("zoom_level");
                    view.Property(v => v.MinLatitude).HasColumnName("min_latitude");
                    view.Property(v => v.MaxLatitude).HasColumnName("max_latitude");
                    view.Property(v => v.MinLongitude).HasColumnName("min_longitude");
                    view.Property(v => v.MaxLongitude).HasColumnName("max_longitude");
                });
            });
        }
    }
}
