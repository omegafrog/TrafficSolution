using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
                    if (!TryParseHighwayNo(refNo, out int highwayNo))
                    {
                        Debug.WriteLine($"[OpenStreetDbRepository.findAdjacentHighways] skipped non-numeric ref='{refNo}' road_name='{road_name}'");
                        continue;
                    }

                    if (!highways.ContainsKey(highwayNo))
                    {
                        highways.Add(highwayNo, new HighWay { ReferenceNumber = refNo, Name = road_name });
                    }
                }
            }
            return highways;



        }

        internal async Task<List<RoadNameCandidate>> findRoadNameCandidates(
            string roadName,
            double minLatitude,
            double minLongitude,
            double maxLatitude,
            double maxLongitude)
        {
            await using var conn = await GetConnection();
            string sql = """
                WITH bounds AS (
                    SELECT ST_Transform(
                        ST_MakeEnvelope(@minLon, @minLat, @maxLon, @maxLat, 4326),
                        3857
                    ) AS geom
                ),
                center_point AS (
                    SELECT ST_Transform(
                        ST_SetSRID(
                            ST_Point((@minLon + @maxLon) / 2.0, (@minLat + @maxLat) / 2.0),
                            4326
                        ),
                        3857
                    ) AS geom
                )
                SELECT
                    COALESCE(l.name, l.ref, '이름없음') AS road_name,
                    COALESCE(l.ref, '') AS ref_no,
                    ST_Y(ST_Transform(ST_Centroid(l.way), 4326)) AS latitude,
                    ST_X(ST_Transform(ST_Centroid(l.way), 4326)) AS longitude,
                    ROUND(ST_Distance(l.way, center_point.geom)::numeric, 2)::double precision AS distance_m
                FROM planet_osm_line l
                CROSS JOIN bounds
                CROSS JOIN center_point
                WHERE l.highway IN ('motorway', 'trunk')
                  AND l.way && bounds.geom
                  AND ST_Intersects(l.way, bounds.geom)
                  AND (
                        LOWER(COALESCE(l.name, '')) LIKE '%' || LOWER(@roadName) || '%'
                        OR LOWER(COALESCE(l.ref, '')) LIKE '%' || LOWER(@roadName) || '%'
                  )
                ORDER BY road_name ASC, ref_no ASC, distance_m ASC;
                """;

            await using var command = new NpgsqlCommand(sql, conn);
            command.Parameters.AddWithValue("roadName", roadName?.Trim() ?? string.Empty);
            command.Parameters.AddWithValue("minLat", minLatitude);
            command.Parameters.AddWithValue("minLon", minLongitude);
            command.Parameters.AddWithValue("maxLat", maxLatitude);
            command.Parameters.AddWithValue("maxLon", maxLongitude);

            await using var reader = await command.ExecuteReaderAsync();
            List<RoadNameCandidate> candidates = new List<RoadNameCandidate>();

            while (await reader.ReadAsync())
            {
                string roadNameText = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                string refNoText = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                double latitude = reader.IsDBNull(2) ? 0 : reader.GetDouble(2);
                double longitude = reader.IsDBNull(3) ? 0 : reader.GetDouble(3);
                double distanceMeters = reader.IsDBNull(4) ? 0 : reader.GetDouble(4);

                foreach (string refNo in refNoText.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    if (!TryParseHighwayNo(refNo, out int highwayNo))
                    {
                        Debug.WriteLine($"[OpenStreetDbRepository.findRoadNameCandidates] skipped non-numeric ref='{refNo}' roadName='{roadNameText}'");
                        continue;
                    }

                    candidates.Add(new RoadNameCandidate
                    {
                        HighwayNo = highwayNo,
                        ReferenceNumber = refNo,
                        HighwayName = roadNameText,
                        Latitude = latitude,
                        Longitude = longitude,
                        DistanceMeters = distanceMeters
                    });
                }
            }

            Debug.WriteLine(
                $"[OpenStreetDbRepository.findRoadNameCandidates] roadName='{roadName?.Trim() ?? string.Empty}', bounds=({minLongitude}, {minLatitude})-({maxLongitude}, {maxLatitude}), rawCandidateCount={candidates.Count}");

            foreach (RoadNameCandidate candidate in candidates.Take(10))
            {
                Debug.WriteLine(
                    $"[OpenStreetDbRepository.findRoadNameCandidates] candidate highwayNo={candidate.HighwayNo}, ref='{candidate.ReferenceNumber}', name='{candidate.HighwayName}', lat={candidate.Latitude}, lon={candidate.Longitude}, distanceM={candidate.DistanceMeters}");
            }

            return candidates;
        }

        private static bool TryParseHighwayNo(string refNo, out int highwayNo)
        {
            if (int.TryParse(refNo, out highwayNo))
            {
                return true;
            }

            string digitsOnly = new string(refNo.Where(char.IsDigit).ToArray());
            return !string.IsNullOrWhiteSpace(digitsOnly) && int.TryParse(digitsOnly, out highwayNo);
        }

    }
}
