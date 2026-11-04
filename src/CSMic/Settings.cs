using System.IO;
using System.Text.Json;
namespace CSMic;
public sealed record Settings {
 public string Language {get;init;} = "en";
 public string? InputId {get;init;} public string? OutputId {get;init;} public string? MonitorId {get;init;}
 public bool Meme {get;init;} public float Intensity {get;init;} = .5f; public float Noise {get;init;} = .75f;
 static string FilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CSMic", "ajustes.json");
 public static Settings Load() { try { return JsonSerializer.Deserialize<Settings>(File.ReadAllText(FilePath)) ?? new(); } catch { return new(); } }
 public void Save() { Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!); File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions {WriteIndented=true})); }
}
