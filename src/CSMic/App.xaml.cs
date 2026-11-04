using System.Windows;
namespace CSMic;
public partial class App : Application {
 protected override void OnStartup(StartupEventArgs e) {
  base.OnStartup(e);
  if(e.Args.Length==2 && (e.Args[0]=="--diagnostics" || e.Args[0]=="--capture-check")) {Shutdown(Diagnostics.Run(e.Args[1],e.Args[0]=="--capture-check"));return;}
  new MainWindow().Show();
 }
}
