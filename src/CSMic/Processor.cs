using System.Runtime.InteropServices;
using NAudio.Dsp;
namespace CSMic;
public sealed class Processor : IDisposable {
 [DllImport("rnnoise", CallingConvention=CallingConvention.Cdecl)] static extern IntPtr rnnoise_create();
 [DllImport("rnnoise", CallingConvention=CallingConvention.Cdecl)] static extern void rnnoise_destroy(IntPtr state);
 [DllImport("rnnoise", CallingConvention=CallingConvention.Cdecl)] static extern float rnnoise_process_frame(IntPtr state, float[] output, float[] input);
 readonly IntPtr state;
 readonly float[] nativeIn = new float[480], clean = new float[480];
 readonly BiQuadFilter high = BiQuadFilter.HighPassFilter(48000, 85, .707f);
 readonly BiQuadFilter presence = BiQuadFilter.PeakingEQ(48000, 1800, .8f, 4);
 readonly BiQuadFilter low = BiQuadFilter.LowPassFilter(48000, 6200, .707f);
 float envelope, smoothIntensity, memePreamp=1;
 public volatile bool Bypass; public volatile bool Meme; public volatile float Intensity=.5f, Noise=.75f;
 public Processor() { state=rnnoise_create(); if(state==IntPtr.Zero) throw new InvalidOperationException(L10n.T("RnnoiseError")); }
 public void Process(float[] frame) {
  for(int i=0;i<480;i++) nativeIn[i]=frame[i]*32768;
  rnnoise_process_frame(state, clean, nativeIn);
  float rms=0; for(int i=0;i<480;i++) rms+=frame[i]*frame[i]; rms=MathF.Sqrt(rms/480);
  float desiredPreamp=Math.Clamp(.05f/Math.Max(rms,.005f),1f,4f);
  memePreamp+=(desiredPreamp-memePreamp)*.03f;
  for(int i=0;i<480;i++) {
   float dry=frame[i];
   float x=clean[i]/32768; // RNNoise introduces a frame of delay; partial wet mixes would comb-filter speech.
   // Noise slider blends attenuation against a one-frame delayed dry signal.
   // The delayed dry path always remains audible in meme mode. RNNoise can
   // suppress quiet consonants; a hard gate would remove them completely.
   float wet=Meme ? .55f+.3f*Math.Clamp(Noise,0,1) : .5f+.5f*Math.Clamp(Noise,0,1);
   x=delayed[i]*(1-wet)+x*wet; delayed[i]=dry;
   x=high.Transform(x);
   envelope=MathF.Max(MathF.Abs(x),envelope*.9995f);
   if(!Meme && envelope>.18f) x*=MathF.Pow(.18f/envelope,.67f);
   smoothIntensity+=(Intensity-smoothIntensity)*.002f;
   float colored=low.Transform(presence.Transform(x));
   float drive=1+smoothIntensity*28;
   float saturated=MathF.Tanh(colored*memePreamp*drive)*.94f;
   float effectMix=Math.Clamp(smoothIntensity*1.5f,0,1);
   x=Meme ? x*(1-effectMix)+saturated*effectMix : x;
   frame[i]=Math.Clamp(Bypass ? dry : x,-.95f,.95f);
  }
 }
 readonly float[] delayed=new float[480];
 public void Dispose() => rnnoise_destroy(state);
}
