using System.Diagnostics;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
namespace CSMic;
public sealed record Endpoint(string Id,string Name) { public override string ToString()=>Name; }
public sealed class AudioEngine : IDisposable {
 readonly MMDeviceEnumerator devices=new();
 WasapiCapture? capture; WasapiOut? output,monitor; MMDevice? inputDevice,outputDevice,monitorDevice;
 BufferedWaveProvider? queue,monitorQueue; Processor? processor;
 readonly float[] frame=new float[480]; readonly byte[] packed=new byte[1920]; int position;
 public float InputPeak,OutputPeak; public double DspMs,QueueMs; public long Dropped;
 public Processor? Processor=>processor;
 public event Action<string>? Fault;
 public List<Endpoint> List(DataFlow flow) { using var list=devices.EnumerateAudioEndPoints(flow,DeviceState.Active); var result=new List<Endpoint>(); foreach(var d in list) { result.Add(new(d.ID,d.FriendlyName)); d.Dispose(); } return result; }
 public void Start(string input,string target,string? listen) {
  Stop(); Dropped=0; DspMs=QueueMs=0;
  try {
   processor=new(); inputDevice=devices.GetDevice(input); outputDevice=devices.GetDevice(target);
   queue=Buffer(); output=new(outputDevice,AudioClientShareMode.Shared,true,10); output.Init(queue);
   if(listen!=null) { monitorDevice=devices.GetDevice(listen); monitorQueue=Buffer(); monitor=new(monitorDevice,AudioClientShareMode.Shared,true,20); monitor.Init(new VolumeSampleProvider(monitorQueue.ToSampleProvider()){Volume=.35f}); }
   capture=new(inputDevice,true,10) { WaveFormat=WaveFormat.CreateIeeeFloatWaveFormat(48000,1) };
   capture.DataAvailable+=OnData;
   capture.RecordingStopped+=(_,e)=> { if(e.Exception!=null) Fault?.Invoke(L10n.T("CaptureError")+e.Exception.Message); };
   output.PlaybackStopped+=(_,e)=> {if(e.Exception!=null) Fault?.Invoke(L10n.T("OutputError")+e.Exception.Message);};
   output.Play(); monitor?.Play(); capture.StartRecording();
  } catch {Stop(); throw;}
 }
 static BufferedWaveProvider Buffer()=>new(WaveFormat.CreateIeeeFloatWaveFormat(48000,1),TimeSpan.FromMilliseconds(100)){DiscardOnBufferOverflow=true,ReadFully=true};
 void OnData(object? sender,WaveInEventArgs e) {
  try {
   for(int offset=0;offset<e.BytesRecorded;offset+=4) {
    float sample=BitConverter.ToSingle(e.Buffer,offset); frame[position++]=float.IsFinite(sample)?sample:0;
    if(position!=480) continue; position=0;
    float ip=0,op=0; foreach(float x in frame) ip=Math.Max(ip,Math.Abs(x));
    long start=Stopwatch.GetTimestamp(); processor!.Process(frame); DspMs=Stopwatch.GetElapsedTime(start).TotalMilliseconds;
    foreach(float x in frame) op=Math.Max(op,Math.Abs(x)); InputPeak=ip; OutputPeak=op;
    System.Buffer.BlockCopy(frame,0,packed,0,packed.Length);
    // Bound drift/backlog; never let an old voice packet accumulate indefinitely.
    if(queue!.BufferedDuration.TotalMilliseconds>40) {queue.ClearBuffer(); Dropped++;}
    queue.AddSamples(packed,0,packed.Length); QueueMs=queue.BufferedDuration.TotalMilliseconds;
    if(monitorQueue!=null) {if(monitorQueue.BufferedDuration.TotalMilliseconds>40) monitorQueue.ClearBuffer(); monitorQueue.AddSamples(packed,0,packed.Length);}
   }
  } catch(Exception ex) {Fault?.Invoke(L10n.T("AudioError")+ex.Message);}
 }
 public void Stop() {
  if(capture!=null) {capture.DataAvailable-=OnData; capture.StopRecording(); capture.Dispose(); capture=null;}
  output?.Stop(); monitor?.Stop(); output?.Dispose(); monitor?.Dispose(); output=null; monitor=null;
  processor?.Dispose(); processor=null; inputDevice?.Dispose(); outputDevice?.Dispose(); monitorDevice?.Dispose(); inputDevice=outputDevice=monitorDevice=null;
  queue=monitorQueue=null; position=0; InputPeak=OutputPeak=0;
 }
 public void Dispose() {Stop();devices.Dispose();}
}
