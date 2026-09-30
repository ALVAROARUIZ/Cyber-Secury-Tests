using System.Diagnostics;
using System.Reflection;

namespace Wallabee.Launcher;

internal sealed class TrayApplication : ApplicationContext
{
    private readonly Control dispatcher = new();
    private readonly NotifyIcon icon;
    private readonly ContextMenuStrip menu = new();
    private readonly ToolStripMenuItem open;
    private readonly ToolStripMenuItem status;
    private string? address;
    public int ExitCode { get; private set; }

    internal TrayApplication(string[] args)
    {
        _ = dispatcher.Handle;
        using var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("Wallabee.Icon")!;
        icon = new NotifyIcon { Icon = new Icon(resource), Text = "WALLABEE · Iniciando", Visible = true, ContextMenuStrip = menu };
        status = new ToolStripMenuItem("Iniciando WALLABEE…") { Enabled = false };
        menu.Items.Add(status);
        open = new ToolStripMenuItem("Abrir WALLABEE", null, (_, _) => OpenPanel()) { Enabled = false };
        menu.Items.Add(open);
        menu.Items.Add("Abrir carpeta del laboratorio", null, (_, _) => OpenFolder("lab"));
        menu.Items.Add("Abrir registros", null, (_, _) => OpenFolder("logs"));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Salir de WALLABEE", null, (_, _) => { status.Text = "Cerrando…"; Program.RequestShutdown(); });
        icon.DoubleClick += (_, _) => OpenPanel();
        Program.Ready = url => dispatcher.BeginInvoke(() =>
        {
            address = url; open.Enabled = true; status.Text = "WALLABEE en ejecución";
            icon.Text = "WALLABEE · Panel en vivo";
        });
        _ = Task.Run(async () =>
        {
            var result = await Program.RunAsync(args);
            dispatcher.BeginInvoke(() =>
            {
                ExitCode = result;
                if (result != 0) MessageBox.Show(Program.LastError ?? "No se pudo iniciar WALLABEE.", "WALLABEE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ExitThread();
            });
        });
    }

    private void OpenPanel() { if (address is not null) Program.OpenBrowser(address); }
    private static void OpenFolder(string name)
    {
        var path = Path.Combine(BundleInstaller.DataRoot, name);
        Directory.CreateDirectory(path);
        Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing) { Program.Ready = null; icon.Visible = false; icon.Icon?.Dispose(); icon.Dispose(); menu.Dispose(); dispatcher.Dispose(); }
        base.Dispose(disposing);
    }
}
