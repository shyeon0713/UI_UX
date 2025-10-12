using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

public static class Paths
{
    public static string Root => Application.persistentDataPath;
    public static string Photos => Path.Combine(Root, "photos");
    public static string Thumbs => Path.Combine(Root, "thumbs");
}

public sealed class PhotoEntry
{
    public string Id;
    public string FilePath;
    public string ThumbPath;
    public DateTime? TakenAt;
    public double? Latitude;
    public double? Longitude;
    public string Weekday;
}

public static class ImportPipeline
{
    public static System.Collections.IEnumerator RunCoroutine(
        string[] srcPaths,
        Action<int, int> onProgress = null,
        int thumbSize = 512,
        int yieldEvery = 8
    )
    {
        if (srcPaths == null || srcPaths.Length == 0) yield break;

        Directory.CreateDirectory(Paths.Photos);
        Directory.CreateDirectory(Paths.Thumbs);

        int done = 0;
        int total = srcPaths.Length;

        for (int i = 0; i < total; i++)
        {
            string src = srcPaths[i];
            string id = Sha1OfFile(src);

            string dst = Path.Combine(Paths.Photos, id + ".jpg");
            string thumb = Path.Combine(Paths.Thumbs, id + $"_{thumbSize}.jpg");

            if (!File.Exists(dst))
                File.Copy(src, dst, overwrite: false);

            // EXIF
            var (taken, lat, lon, orientation) = ExifReaderService.Read(dst);

            // 썸네일 (메인 스레드)
            if (!File.Exists(thumb))
                MakeSquareThumb(dst, thumb, thumbSize, orientation);

            var entry = new PhotoEntry
            {
                Id = id,
                FilePath = dst,
                ThumbPath = thumb,
                TakenAt = taken,
                Latitude = lat,
                Longitude = lon,
                Weekday = taken?.DayOfWeek.ToString()
            };

            PhotoDatabase.Upsert(entry);

            done++;
            onProgress?.Invoke(done, total);

            if (i % yieldEvery == 0) yield return null;
        }
    }

    static string Sha1OfFile(string path)
    {
        using var fs = File.OpenRead(path);
        using var sha = SHA1.Create();
        var b = sha.ComputeHash(fs);
        return string.Concat(b.Select(x => x.ToString("x2")));
    }

    // Orientation(1,3,6,8) 보정 포함
    static void MakeSquareThumb(string src, string dst, int size, int orientation)
    {
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.LoadImage(File.ReadAllBytes(src), markNonReadable: false);

        // 회전 보정
        tex = ApplyOrientation(tex, orientation);

        // 중앙 정사각 크롭
        int side = Mathf.Min(tex.width, tex.height);
        int x = (tex.width - side) / 2;
        int y = (tex.height - side) / 2;

        var crop = new Texture2D(side, side, TextureFormat.RGBA32, false);
        crop.SetPixels(tex.GetPixels(x, y, side, side));
        crop.Apply();

        var resized = Scale(crop, size, size);
        File.WriteAllBytes(dst, resized.EncodeToJPG(85));

        UnityEngine.Object.Destroy(crop);
        UnityEngine.Object.Destroy(tex);
        UnityEngine.Object.Destroy(resized);
    }

    static Texture2D ApplyOrientation(Texture2D src, int orientation)
    {
        // 간단 대응: 1=원본, 3=180°, 6=90°CW, 8=270°CW
        switch (orientation)
        {
            case 3: return RotateTexture(src, 180);
            case 6: return RotateTexture(src, 90);
            case 8: return RotateTexture(src, 270);
            default: return src;
        }
    }

    static Texture2D RotateTexture(Texture2D tex, int degrees)
    {
        int w = tex.width, h = tex.height;
        Texture2D rot;
        if (degrees == 180) { rot = new Texture2D(w, h, tex.format, false); }
        else { rot = new Texture2D(h, w, tex.format, false); }

        float cx = (w - 1) / 2f, cy = (h - 1) / 2f;
        float rad = degrees * Mathf.Deg2Rad;

        for (int y = 0; y < rot.height; y++)
            for (int x = 0; x < rot.width; x++)
            {
                // 역변환(목표→원본)
                float u, v;
                if (degrees == 180)
                {
                    u = w - 1 - x;
                    v = h - 1 - y;
                }
                else if (degrees == 90)
                {
                    u = y;
                    v = w - 1 - x;
                }
                else // 270
                {
                    u = h - 1 - y;
                    v = x;
                }
                rot.SetPixel(x, y, tex.GetPixel((int)u, (int)v));
            }
        rot.Apply();
        UnityEngine.Object.Destroy(tex);
        return rot;
    }

    static Texture2D Scale(Texture2D s, int w, int h)
    {
        var d = new Texture2D(w, h, s.format, false);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float u = (x + 0.5f) / w;
                float v = (y + 0.5f) / h;
                d.SetPixel(x, y, s.GetPixelBilinear(u, v));
            }
        d.Apply();
        return d;
    }
}
