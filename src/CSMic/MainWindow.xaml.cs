using System.IO;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Threading;
using NAudio.CoreAudioApi;
namespace CSMic;
public partial class MainWindow : Window {
 readonly AudioEngine engine=new(); readonly DispatcherTimer meter=new(){Interval=TimeSpan.FromMilliseconds(50)};
 readonly DispatcherTimer animation=new(); readonly DispatcherTimer testTimer=new(){Interval=TimeSpan.FromSeconds(8)};
 GifBitmapDecoder? gif; int gifIndex; bool ready,running, exitRequested; Settings settings=Settings.Load();
 readonly System.Windows.Forms.NotifyIcon tray = new();
 readonly System.Windows.Forms.ToolStripMenuItem openMenu = new(), exitMenu = new();
 public MainWindow() {
  L10n.Set(settings.Language); InitializeComponent(); LanguagePicker.SelectedIndex=L10n.Language=="es"?1:0; Refresh(); Profile.SelectedIndex=settings.Meme?1:0; Intensity.Value=settings.Intensity; Noise.Value=settings.Noise; ready=true;Changed(this,new RoutedEventArgs());
  tray.Icon=System.Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath!) ?? System.Drawing.SystemIcons.Application;
  tray.Text="CS MIC";
  tray.ContextMenuStrip=new System.Windows.Forms.ContextMenuStrip();
  openMenu.Click+=(_,_)=>RestoreWindow(); exitMenu.Click+=(_,_)=>ExitApplication();
  tray.ContextMenuStrip.Items.Add(openMenu); tray.ContextMenuStrip.Items.Add(exitMenu);
  tray.MouseClick+=(_,e)=>{if(e.Button==System.Windows.Forms.MouseButtons.Left) Dispatcher.BeginInvoke(new Action(RestoreWindow));};
  UpdateTrayLanguage(); tray.Visible=true;
  engine.Fault+=message=>Dispatcher.BeginInvoke(new Action(()=>{Stop();Status.Text=message;}));
  meter.Tick+=(_,_)=>{
   // A decibel display keeps quiet speech visible; this is signal level, not Windows gain.
   double input=running?MeterValue(engine.InputPeak):0, output=running?MeterValue(engine.OutputPeak):0;
   InputMeter.Value=input; OutputMeter.Value=output;
   InputPercent.Text=$"{Math.Round(input*100)} %"; OutputPercent.Text=$"{Math.Round(output*100)} %";
   if(running) Metrics.Text=string.Format(L10n.T("Metrics"),engine.DspMs,engine.QueueMs,engine.Dropped);
  }; meter.Start();
  using(var stream=System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Assets/brand.gif"))!.Stream) gif=new GifBitmapDecoder(stream,BitmapCreateOptions.PreservePixelFormat,BitmapCacheOption.OnLoad);
  animation.Tick+=(_,_)=>ShowFrame(); ShowFrame(); animation.Start();
  testTimer.Tick+=(_,_)=>{Stop();Status.Text=L10n.T("TestDone");};
  Closing+=OnClosing;
  Closed+=(_,_)=>{animation.Stop();meter.Stop();testTimer.Stop();engine.Dispose();tray.Visible=false;tray.Dispose();};
 }
 BitmapSource? composite,restore; Rect lastRect; int lastDisposal;
 static int Meta(BitmapFrame f,string query,int fallback=0) {try {return f.Metadata is BitmapMetadata m && m.ContainsQuery(query) ? Convert.ToInt32(m.GetQuery(query)) : fallback;}catch{return fallback;}}
 void ShowFrame() {
  var f=gif!.Frames[gifIndex];
  int width=gif.Frames[0].PixelWidth,height=gif.Frames[0].PixelHeight;
  if(gifIndex==0) {composite=null;restore=null;lastDisposal=0;}
  var visual=new DrawingVisual();
  using(var dc=visual.RenderOpen()) {
   var prior=lastDisposal==3?restore:composite;
   if(prior!=null) {
    if(lastDisposal==2) dc.PushClip(new CombinedGeometry(GeometryCombineMode.Exclude,new RectangleGeometry(new Rect(0,0,width,height)),new RectangleGeometry(lastRect)));
    dc.DrawImage(prior,new Rect(0,0,width,height));
    if(lastDisposal==2) dc.Pop();
   }
  }
  var background=new RenderTargetBitmap(width,height,96,96,PixelFormats.Pbgra32);background.Render(visual);background.Freeze();restore=background;
  lastRect=new Rect(Meta(f,"/imgdesc/Left"),Meta(f,"/imgdesc/Top"),f.PixelWidth,f.PixelHeight);
  using(var dc=visual.RenderOpen()) {dc.DrawImage(background,new Rect(0,0,width,height));dc.DrawImage(f,lastRect);}
  var next=new RenderTargetBitmap(width,height,96,96,PixelFormats.Pbgra32);next.Render(visual);next.Freeze();composite=next;Brand.Source=next;
  lastDisposal=Meta(f,"/grctlext/Disposal");
  animation.Interval=TimeSpan.FromMilliseconds(Math.Max(20,Meta(f,"/grctlext/Delay",10)*10));gifIndex=(gifIndex+1)%gif.Frames.Count;
 }
 static double MeterValue(float peak)=>peak<.001f?0:Math.Clamp((20*Math.Log10(peak)+60)/60,0,1);
 void Refresh() {
  try {var ins=engine.List(DataFlow.Capture);var outs=engine.List(DataFlow.Render);
   Input.ItemsSource=ins; Output.ItemsSource=outs;Monitor.ItemsSource=outs;
   Input.SelectedItem=ins.Find(x=>x.Id==settings.InputId) ?? ins.Find(x=>x.Name.Contains("Mic",StringComparison.OrdinalIgnoreCase) && !x.Name.Contains("CABLE",StringComparison.OrdinalIgnoreCase)) ?? ins.FirstOrDefault();
   Output.SelectedItem=outs.Find(x=>x.Id==settings.OutputId) ?? outs.Find(x=>x.Name.Contains("CABLE Input",StringComparison.OrdinalIgnoreCase));
   Monitor.SelectedItem=outs.Find(x=>x.Id==settings.MonitorId) ?? outs.Find(x=>!x.Name.Contains("CABLE",StringComparison.OrdinalIgnoreCase));
  } catch(Exception e) {Status.Text=L10n.T("EnumerateError")+e.Message;}
 }
 void UpdateTrayLanguage() {openMenu.Text=L10n.Language=="es"?"Abrir CS MIC":"Open CS MIC"; exitMenu.Text=L10n.Language=="es"?"Salir":"Exit";}
 void RestoreWindow() {Show();WindowState=WindowState.Normal;ShowInTaskbar=true;Activate();}
 void ExitApplication() {exitRequested=true;Close();}
 void OnClosing(object? sender,CancelEventArgs e) {if(exitRequested) return;e.Cancel=true;Hide();ShowInTaskbar=false;}
 void LanguageChanged(object sender,SelectionChangedEventArgs e) {
  if(!ready) return;
  L10n.Set(LanguagePicker.SelectedIndex==1?"es":"en"); UpdateTrayLanguage();
  Status.Text=L10n.T(running?"Streaming":"Ready"); Metrics.Text=L10n.T("Target");
  settings=settings with {Language=L10n.Language};
  try {settings.Save();} catch(Exception ex) {Status.Text=L10n.T("SaveError")+ex.Message;}
 }
 void RefreshClick(object sender,RoutedEventArgs e) {if(!running) Refresh();}
 void Changed(object sender,RoutedEventArgs e) {if(!ready) return; IntensityPercent.Text=$"{Math.Round(Intensity.Value*100)} %"; NoisePercent.Text=$"{Math.Round(Noise.Value*100)} %"; if(engine.Processor is {} p) {p.Meme=Profile.SelectedIndex==1;p.Intensity=(float)Intensity.Value;p.Noise=(float)Noise.Value;p.Bypass=Bypass.IsChecked==true;}}
 void Start(bool test) {
  if(running) Stop();
  if(Input.SelectedItem is not Endpoint input || Output.SelectedItem is not Endpoint output) {Status.Text="Selecciona un micrÃ³fono y CABLE Input. Actualiza la lista tras instalar VB-CABLE.";return;}
  var listen=(Listen.IsChecked==true || test) ? Monitor.SelectedItem as Endpoint : null;
  if((Listen.IsChecked==true || test) && (listen==null || listen.Id==output.Id || listen.Name.Contains("CABLE",StringComparison.OrdinalIgnoreCase))) {Status.Text="Selecciona auriculares fÃ­sicos para escucharte.";return;}
  try {engine.Start(input.Id,output.Id,listen?.Id); running=true;Changed(this,new RoutedEventArgs());SetControls();Status.Text=test?"Prueba activa durante 8 segundos. TambiÃ©n se envÃ­a a la salida virtual.":"Transmitiendo Â· selecciona CABLE Output en tu aplicaciÃ³n.";if(test) testTimer.Start();}
  catch(Exception e) {Status.Text="No se pudo iniciar. Comprueba RNNoise x64, permisos de micrÃ³fono y formato 48 kHz. "+e.Message;}
 }
 void SetControls() {Input.IsEnabled=Output.IsEnabled=Monitor.IsEnabled=Listen.IsEnabled=!running;StartButton.IsEnabled=!running;StopButton.IsEnabled=running;}
 void Stop() {testTimer.Stop();engine.Stop();running=false;SetControls();Status.Text=L10n.T("Stopped");}
 void StartClick(object sender,RoutedEventArgs e)=>Start(false);
 void StopClick(object sender,RoutedEventArgs e)=>Stop();
 void TestClick(object sender,RoutedEventArgs e)=>Start(true);
 void SaveClick(object sender,RoutedEventArgs e) {
  settings=new Settings{Language=L10n.Language,InputId=(Input.SelectedItem as Endpoint)?.Id,OutputId=(Output.SelectedItem as Endpoint)?.Id,MonitorId=(Monitor.SelectedItem as Endpoint)?.Id,Meme=Profile.SelectedIndex==1,Intensity=(float)Intensity.Value,Noise=(float)Noise.Value};
  try{settings.Save();Status.Text=L10n.T("Saved");}catch(Exception ex){Status.Text=L10n.T("SaveError")+ex.Message;}
 }
}
