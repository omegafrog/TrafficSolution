using Npgsql;
using System.Text;
using TrafficForm.App;
using TrafficForm.Domain;

namespace TrafficForm.Adapter
{
    public class VdsRepository
    {
        private readonly string datasource = "Host=localhost;Port=5432;Database=gis;Username=renderer;Password=renderer";

        private async Task<NpgsqlConnection> GetConnection()
        {
            var conn = new NpgsqlConnection(datasource);
            await conn.OpenAsync();
            return conn;
        }

        // lat/lon 범위 안에 있고 highwayNo를 가지는 VdsId 리스트를 찾는 메소드 
        internal async  Task<Dictionary<string, Tuple<double, double>>> findVdsIdIn(int highwayNo, double minLatitude, double minLongitude, double maxLatitude, double maxLongitude)
        {
            await using var conn = await GetConnection();

            StringBuilder builder = new StringBuilder();
            string query = """
                select v."VDS_ID", vl."X좌표값", vl."Y좌표값"  from vds v
                left join vds_loc vl 
                on v."노선번호" = vl."노선번호" and v."지점이정" = vl."이정"
                where vl."X좌표값" >= @minLatitude and vl."X좌표값" <= @maxLatitude and vl."Y좌표값" >= @minLongitude and vl."Y좌표값" <= @maxLongitude and v."노선번호" = @highwayNo
                """;
            NpgsqlCommand command = new NpgsqlCommand(query, conn);
            command.Parameters.AddWithValue("minLatitude", minLatitude);
            command.Parameters.AddWithValue("maxLatitude", maxLatitude);
            command.Parameters.AddWithValue("minLongitude", minLongitude);
            command.Parameters.AddWithValue("maxLongitude", maxLongitude);
            command.Parameters.AddWithValue("highwayNo", highwayNo);
            await using var reader = await command.ExecuteReaderAsync();
            Dictionary<string, Tuple<double, double>> vdsLoc = new Dictionary<string, Tuple<double, double>>();
            while(await reader.ReadAsync())
            {
                string vdsId = reader.GetString(0);
                double latitude = reader.GetDouble(1);
                double longitude = reader.GetDouble(2);
                if (!vdsLoc.ContainsKey(vdsId))
                {
                    vdsLoc.Add(vdsId, new Tuple<double, double>(latitude, longitude));
                }
            }
            return vdsLoc;
        }

        internal async Task<List<Location>> findAllVdsLoc()
        {
            await using var conn = await GetConnection();

            StringBuilder builder = new StringBuilder();
            string query = """
                select v."VDS_ID", vl."X좌표값", vl."Y좌표값"  from vds v
                left join vds_loc vl 
                on v."노선번호" = vl."노선번호" and v."지점이정" = vl."이정"
                where vl."X좌표값" >= @minLatitude and vl."X좌표값" <= @maxLatitude and vl."Y좌표값" >= @minLongitude and vl."Y좌표값" <= @maxLongitude
                """;
            NpgsqlCommand command = new NpgsqlCommand(query, conn);
            command.Parameters.AddWithValue("minLatitude", UpdateSelectedPosTrafficInfoCommand.MIN_LATITUDE);
            command.Parameters.AddWithValue("maxLatitude", UpdateSelectedPosTrafficInfoCommand.MAX_LATITUDE);
            command.Parameters.AddWithValue("minLongitude", UpdateSelectedPosTrafficInfoCommand.MIN_LONGITUDE);
            command.Parameters.AddWithValue("maxLongitude", UpdateSelectedPosTrafficInfoCommand.MAX_LONGITUDE);
            await using var reader = await command.ExecuteReaderAsync();
            List<Location> locations = new List<Location>();
            while (await reader.ReadAsync())
            {
                string vdsId = reader.GetString(0);
                double latitude = reader.GetDouble(1);
                double longitude = reader.GetDouble(2);
                locations.Add(new Location { Name = vdsId, Latitude = latitude, Longitude = longitude });
            } 
            return locations;
        }
    }
}