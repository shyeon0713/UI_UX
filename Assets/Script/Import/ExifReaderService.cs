using System;
using System.Linq;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

public static class ExifReaderService
{
    public static (DateTime? takenAt, double? lat, double? lon, int orientation) Read(string path)
    {
        var dirs = ImageMetadataReader.ReadMetadata(path);

        // 1) 촬영 시각 (DateTimeOriginal 우선)
        DateTime? taken = null;
        var sub = dirs.OfType<ExifSubIfdDirectory>().FirstOrDefault();
        if (sub != null && sub.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var dto))
            taken = dto;
        else if (sub != null && sub.TryGetDateTime(ExifDirectoryBase.TagDateTimeDigitized, out var dtd))
            taken = dtd;
        else
        {
            // 최후: Exif IFD0 DateTime (신뢰 낮음)
            var ifd0dt = dirs.OfType<ExifIfd0Directory>().FirstOrDefault();
            if (ifd0dt != null && ifd0dt.TryGetDateTime(ExifDirectoryBase.TagDateTime, out var dt))
                taken = dt;
        }

        // 2) GPS (GeoLocation 메서드 없이 직접 파싱)
        double? lat = null, lon = null;
        var gps = dirs.OfType<GpsDirectory>().FirstOrDefault();
        if (gps != null)
        {
            try
            {
                // 라벨(북/남, 동/서): "N","S","E","W"
                var latRef = gps.GetString(GpsDirectory.TagLatitudeRef);
                var lonRef = gps.GetString(GpsDirectory.TagLongitudeRef);

                // 도/분/초: Rational[] (length 3)
                var latDms = gps.GetRationalArray(GpsDirectory.TagLatitude);
                var lonDms = gps.GetRationalArray(GpsDirectory.TagLongitude);

                if (latDms != null && latDms.Length == 3 &&
                    lonDms != null && lonDms.Length == 3 &&
                    !string.IsNullOrEmpty(latRef) && !string.IsNullOrEmpty(lonRef))
                {
                    double latVal = DmsToDecimal(latDms);
                    double lonVal = DmsToDecimal(lonDms);

                    if (latRef.Equals("S", StringComparison.OrdinalIgnoreCase)) latVal = -latVal;
                    if (lonRef.Equals("W", StringComparison.OrdinalIgnoreCase)) lonVal = -lonVal;

                    lat = latVal;
                    lon = lonVal;
                }
            }
            catch { /* GPS 정보 없거나 파싱 실패 → null 유지 */ }
        }

        // 3) Orientation (썸네일 회전 보정용)
        int orientation = 1;
        var ifd0 = dirs.OfType<ExifIfd0Directory>().FirstOrDefault();
        if (ifd0 != null && ifd0.TryGetInt32(ExifDirectoryBase.TagOrientation, out var o))
            orientation = o;

        return (taken, lat, lon, orientation);
    }

    // DMS(도/분/초) → 소수점 각도
    static double DmsToDecimal(MetadataExtractor.Rational[] dms)
    {
        // dms[0]=degrees, dms[1]=minutes, dms[2]=seconds
        return dms[0].ToDouble() + dms[1].ToDouble() / 60.0 + dms[2].ToDouble() / 3600.0;
    }
}
