using System.Windows;
namespace CSMic;
public partial class App : System.Windows.Application {
 protected override void OnStartup(StartupEventArgs e) {
  base.OnStartup(e);
  if(e.Args.Length==2 && (e.Args[0]=="--diagnostics" || e.Args[0]=="--capture-check")) {Shutdown(Diagnostics.Run(e.Args[1],e.Args[0]=="--capture-check"));return;}
  try { new MainWindow().Show(); }
  catch(Exception ex) { var path=System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"CSMic","startup-error.txt"); System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!); System.IO.File.WriteAllText(path,ex.ToString()); Shutdown(1); }
 }
}
