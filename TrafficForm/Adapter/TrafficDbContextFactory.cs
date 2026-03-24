using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TrafficForm.Adapter
{
    // dotnet ef 마이그레이션 명령어를 위한 팩토리
    public class TrafficDbContextFactory : IDesignTimeDbContextFactory<TrafficDbContext>
    {
        public TrafficDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TrafficDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=gis;Username=renderer;Password=renderer");
            return new TrafficDbContext(optionsBuilder.Options);
        }
    }
}
