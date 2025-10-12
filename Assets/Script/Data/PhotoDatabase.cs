using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

// 매우 단순한 JSON 파일 DB (MVP용)
// 파일: <persistentDataPath>/gallery.json
public static class PhotoDatabase
{
    static string DbPath => Path.Combine(Paths.Root, "gallery.json");

    [System.Serializable]
    class Wrapper { public List<PhotoEntry> items; }

    static List<PhotoEntry> _cache;

    static List<PhotoEntry> List
    {
        get
        {
            if (_cache != null) return _cache;

            if (File.Exists(DbPath))
            {
                var json = File.ReadAllText(DbPath);
                var w = JsonUtility.FromJson<Wrapper>(json);
                _cache = w?.items ?? new List<PhotoEntry>();
            }
            else
            {
                _cache = new List<PhotoEntry>();
            }
            return _cache;
        }
    }

    static void Save()
    {
        var w = new Wrapper { items = List };
        File.WriteAllText(DbPath, JsonUtility.ToJson(w, false));
    }

    public static void Upsert(PhotoEntry e)
    {
        int idx = List.FindIndex(x => x.Id == e.Id);
        if (idx >= 0) List[idx] = e; else List.Add(e);
        Save();
    }

    // 예시 조회: 최근 N일 (촬영 시각이 없으면 뒤로 밀림)
    public static IEnumerable<PhotoEntry> Recent(int days = 90)
    {
        var from = System.DateTime.Now.AddDays(-days);
        return List
            .Where(x => !x.TakenAt.HasValue || x.TakenAt.Value >= from)
            .OrderByDescending(x => x.TakenAt ?? System.DateTime.MinValue);
    }

    // 특정 날짜(로컬) 조회 예시
    public static IEnumerable<PhotoEntry> OnLocalDate(System.DateTime dateLocal)
    {
        return List.Where(x =>
        {
            if (!x.TakenAt.HasValue) return false;
            return x.TakenAt.Value.ToLocalTime().Date == dateLocal.Date;
        })
        .OrderBy(x => x.TakenAt ?? System.DateTime.MinValue);
    }
}

