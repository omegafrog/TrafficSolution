import math
import subprocess

# 이름, min_lon, min_lat, max_lon, max_lat
regions = [
    ("seoul",      126.76, 37.42, 127.18, 37.70),
    ("gyeonggi",   126.35, 36.89, 127.86, 38.30),
    ("incheon",    126.20, 37.20, 126.98, 37.80),
    ("gangwon",    127.02, 37.02, 129.36, 38.62),
    ("chungbuk",   127.27, 36.00, 128.66, 37.30),
    ("chungnam",   125.95, 35.97, 127.65, 37.03),
    ("jeonbuk",    126.35, 35.18, 127.95, 36.15),
    ("jeonnam",    125.05, 33.90, 127.95, 35.55),
    ("gyeongbuk",  128.05, 35.45, 129.60, 37.55),
    ("gyeongnam",  127.55, 34.55, 129.05, 35.95),
    ("busan",      128.75, 35.00, 129.35, 35.40),
    ("ulsan",      129.05, 35.35, 129.50, 35.75),
    ("daegu",      128.35, 35.65, 128.85, 36.05),
    ("daejeon",    127.20, 36.20, 127.55, 36.50),
    ("gwangju",    126.75, 35.05, 127.05, 35.25),
    ("jeju",       126.10, 33.10, 126.98, 33.60),
]

ZOOM_MIN = 9
ZOOM_MAX = 10
THREADS = 6
CONTAINER = "map"

def lon2tilex(lon, z):
    return int((lon + 180.0) / 360.0 * (2 ** z))

def lat2tiley(lat, z):
    lat_rad = math.radians(lat)
    n = 2.0 ** z
    return int((1.0 - math.asinh(math.tan(lat_rad)) / math.pi) / 2.0 * n)

for name, min_lon, min_lat, max_lon, max_lat in regions:
    print(f"=== {name} ===")
    for z in range(ZOOM_MIN, ZOOM_MAX + 1):
        min_x = lon2tilex(min_lon, z)
        max_x = lon2tilex(max_lon, z)
        min_y = lat2tiley(max_lat, z)
        max_y = lat2tiley(min_lat, z)

        cmd = [
            "docker", "compose", "exec", CONTAINER,
            "render_list",
            "--num-threads", str(THREADS),
            "-z", str(z),
            "-Z", str(z),
            "-x", str(min_x),
            "-X", str(max_x),
            "-y", str(min_y),
            "-Y", str(max_y),
        ]
        print(" ".join(cmd))
        subprocess.run(cmd, check=True)