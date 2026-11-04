using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;
namespace CSMic;
public static class L10n {
 static readonly Dictionary<string,string> en=Load("en"),es=Load("es");
 public static string Language {get;private set;}="en";
 static Dictionary<string,string> Load(string language) {
  using var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream($"CSMic.Strings.{language}.json")!;
  return JsonSerializer.Deserialize<Dictionary<string,string>>(stream)!;
 }
 public static string T(string key) => (Language=="es"?es:en).GetValueOrDefault(key,en.GetValueOrDefault(key,key));
 public static void Set(string language) {
  Language=language=="es"?"es":"en";
  foreach(var pair in (Language=="es"?es:en)) Application.Current.Resources[pair.Key]=pair.Value;
 }
}
