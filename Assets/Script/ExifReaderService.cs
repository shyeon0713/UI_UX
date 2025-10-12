using System.Linq;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

public static class ExifReaderService
{
    public static (System.DateTime? takenAt, double? lat, double? lon) Read(string path)
    {
        var dirs = ImageMetadataReader.ReadMetadata(path);

        // √‘øµ Ω√∞¢
        System.DateTime? taken = null;
        var sub = dirs.OfType<ExifSubIfdDirectory>().FirstOrDefault();
        if (sub != null && sub.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var dto)) taken = dto;

        // GPS
        double? lat = null, lon = null;
        var geo = dirs.OfType<GpsDirectory>().FirstOrDefault()?.GetGeoLocation();
        if (geo != null && !geo.IsZero) { lat = geo.Latitude; lon = geo.Longitude; }

        return (taken, lat, lon);
    }
}
