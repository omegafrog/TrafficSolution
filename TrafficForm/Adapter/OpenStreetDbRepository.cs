using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using TrafficForm.Domain;

namespace TrafficForm.Adapter
{
    internal class OpenStreetDbRepository
    {
        private readonly string datasource = "Host=localhost;Port=5432;Database=gis;Username=renderer;Password=renderer";

        private async Task<NpgsqlConnection>  GetConnection()
        {
            var conn = new NpgsqlConnection(datasource);
            await conn.OpenAsync();
            return conn;
        }
        internal async Task<Dictionary<int, HighWay>> findAdjacentHighways(double latitude, double longitude)
        {
            await using var conn = await GetConnection();
            var geom = "WITH pt AS (SELECT ST_Transform(ST_SetSRID(ST_Point(@lon, @lat), 4326), 3857) AS geom)";
            var select = """
                SELECT
                 COALESCE(l.name, l.ref, '이름없음') AS road_name,
                 l.ref,
                 l.highway,
                 ROUND(ST_Distance(l.way, pt.geom)::numeric, 2) AS distance_m

                """;
            var from = """
                FROM planet_osm_line l
                """;
            var join = """
                
                CROSS JOIN pt
                WHERE l.highway IN ('motorway', 'trunk')
                  AND l.way && ST_Expand(pt.geom, 5000)
                  AND ST_DWithin(l.way, pt.geom, 5000)
                  AND ST_Contains(
                      ST_MakeEnvelope(125.0, 33.0, 131.0, 39.0, 4326),
                      ST_Transform(ST_Centroid(l.way), 4326)
                  )
                ORDER BY l.way <-> pt.geom
                LIMIT 10;
                """;
            StringBuilder sql = new StringBuilder(geom);
            sql.Append(select).Append(from).Append(join);
            var command = new NpgsqlCommand(sql.ToString(), conn);
            command.Parameters.AddWithValue("lat", latitude);
            command.Parameters.AddWithValue("lon", longitude);

            await using var reader = await command.ExecuteReaderAsync();

            Dictionary<int, HighWay> highways = new Dictionary<int, HighWay>();
            while(await reader.ReadAsync())
            {
                string road_name = reader.IsDBNull(1)?"": reader.GetString(0);
                string refNoString = reader.IsDBNull(1)?"":reader.GetString(1);
                if (string.IsNullOrEmpty(refNoString))
                {
                    continue;
                }

                foreach(string refNo in refNoString.Split(";"))
                {
                    if (!highways.ContainsKey(int.Parse(refNo)))
                    {
                        highways.Add(int.Parse(refNo), new HighWay { ReferenceNumber = refNo, Name = road_name });
                    }
                }
            }
            return highways;



        }

    }
}
