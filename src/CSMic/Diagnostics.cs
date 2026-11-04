using System.Diagnostics;
using System.IO;
using System.Text.Json;
using NAudio.CoreAudioApi;
using NAudio.Wave;
namespace CSMic;
// Explicit local integration check. Capture mode never writes or transmits audio.
public static class Diagnostics {
 public static int Run(string path,bool capture=false) {
  var results=new Dictionary<string,object>(); int exit=0;
  try {
   using var engine=new AudioEngine();
   results["captureDevices"]=engine.List(DataFlow.Capture);results["renderDevices"]=engine.List(DataFlow.Render);
   results["localization"]=true;
   foreach(string language in new[]{"en","es"}) {L10n.Set(language); if(L10n.T("Start")=="Start" || L10n.T("Ready")=="Ready") throw new Exception("Missing localization.");}
   var timings=new List<double>(); float peak=0; double energy=0;
   using(var dsp=new Processor()) {
    var frame=new float[480];
    foreach(bool meme in new[]{false,true}) {
     dsp.Meme=meme;dsp.Intensity=1;
     for(int block=0;block<500;block++) {
      for(int i=0;i<480;i++) frame[i]=.4f*MathF.Sin(2*MathF.PI*220*(block*480+i)/48000);
      long start=Stopwatch.GetTimestamp(); dsp.Process(frame);timings.Add(Stopwatch.GetElapsedTime(start).TotalMilliseconds);
      foreach(float sample in frame) {if(!float.IsFinite(sample)||Math.Abs(sample)>.95001f) throw new Exception("Invalid DSP output.");peak=Math.Max(peak,Math.Abs(sample));energy+=sample*sample;}
     }
    }
    dsp.Bypass=true;Array.Fill(frame,2f);dsp.Process(frame);
    if(frame.Any(v=>Math.Abs(v-.95f)>.0001f)) throw new Exception("Bypass safety limiter failed.");
   }
   if(energy<.01) throw new Exception("DSP produced silence.");
   timings.Sort();results["dsp"]=new {blocks=timings.Count,peak,medianMs=timings[timings.Count/2],p95Ms=timings[(int)(timings.Count*.95)],maxMs=timings[^1],limiterAndBypass="passed"};
   if(capture) results["capture"]=Task.Run(CheckCapture).GetAwaiter().GetResult();
   results["passed"]=true;
  } catch(Exception ex) {results["passed"]=false;results["error"]=ex.ToString();exit=1;}
  try {File.WriteAllText(Path.GetFullPath(path),JsonSerializer.Serialize(results,new JsonSerializerOptions {WriteIndented=true}));}catch{return 2;}
  return exit;
 }
 static object CheckCapture() {
  using var devices=new MMDeviceEnumerator();
  using var all=devices.EnumerateAudioEndPoints(DataFlow.Capture,DeviceState.Active);
  string? id=null; foreach(var device in all) {using(device) {if(device.FriendlyName.Contains("Mic",StringComparison.OrdinalIgnoreCase) && !device.FriendlyName.Contains("CABLE",StringComparison.OrdinalIgnoreCase)) {id=device.ID;break;}}}
  if(id==null) return new {status="skipped",reason="No physical microphone found"};
  using var input=devices.GetDevice(id);
  using var dsp=new Processor();
#pragma warning disable CS0618 // Same compatibility capture path as the main engine.
  using var recorder=new WasapiCapture(input,true,10) {WaveFormat=WaveFormat.CreateIeeeFloatWaveFormat(48000,1)};
#pragma warning restore CS0618
  int blocks=0,pos=0;float peak=0;string? failure=null;var frame=new float[480];
  recorder.DataAvailable+=(_,e)=> {for(int i=0;i<e.BytesRecorded;i+=4) {frame[pos++]=BitConverter.ToSingle(e.Buffer,i);if(pos==480) {pos=0;dsp.Process(frame);blocks++;foreach(float sample in frame) peak=Math.Max(peak,Math.Abs(sample));}}};
  recorder.RecordingStopped+=(_,e)=>failure=e.Exception?.Message;
  recorder.StartRecording();Thread.Sleep(3000);recorder.StopRecording();
  if(failure!=null || blocks==0) throw new Exception("Capture check failed: "+(failure??"No audio packets"));
  return new {status="passed",blocks,peak,sampleRate=48000,channels=1,audioSaved=false,audioTransmitted=false};
 }

}
