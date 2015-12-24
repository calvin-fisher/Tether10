// Decompiled with JetBrains decompiler
// Type: TetherWindows.TetherForm
// Assembly: TetherWindows, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EC2D7451-4D97-4986-80E3-DF30C217B4F5
// Assembly location: C:\Program Files (x86)\ClockworkMod\Tether\TetherWindows.exe

using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TetherWindows
{
    public class TetherForm : Form
    {
        private NotifyIcon icon = new NotifyIcon();
        private Process nodeProcess;
        private bool exiting;
        private IContainer components;
        private TextBox nodeLog;
        private Button toggleButton;
        private Label label1;
        private PictureBox statusImage;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem downloadDriversToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripMenuItem exitToolStripMenuItem;
        private TextBox status;

        public TetherForm()
        {
            this.InitializeComponent();
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            this.icon.ContextMenu = new ContextMenu();
            MenuItem menuItem = new MenuItem();
            menuItem.Text = "Exit";
            this.icon.ContextMenu.MenuItems.Add(menuItem);
            menuItem.Click += new EventHandler(this.exit_Click);
            this.icon.Click += new EventHandler(this.icon_Click);
            this.icon.Icon = Icon.FromHandle(Resources.usb_off.GetHicon());
            this.icon.Visible = true;
        }

        private void exit_Click(object sender, EventArgs e)
        {
            this.exiting = true;
            this.Close();
        }

        private void icon_Click(object sender, EventArgs e)
        {
            if (!this.IsHandleCreated)
                return;
            this.Visible = true;
        }

        private static string GetDeviceGuid()
        {
            RegistryKey registryKey1 = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Class\\{4D36E972-E325-11CE-BFC1-08002BE10318}", true);
            string[] subKeyNames = registryKey1.GetSubKeyNames();
            string str = "";
            foreach (string name in subKeyNames)
            {
                try
                {
                    RegistryKey registryKey2 = registryKey1.OpenSubKey(name);
                    object obj = registryKey2.GetValue("ComponentId");
                    if (obj != null)
                    {
                        if (obj.ToString() == "tap0901")
                        {
                            str = registryKey2.GetValue("NetCfgInstanceId").ToString();
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
            return str;
        }

        private static string GetNetworkHumanName(string guid)
        {
            if (guid != "")
            {
                object obj = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Network\\{4D36E972-E325-11CE-BFC1-08002BE10318}\\" + guid + "\\Connection", true).GetValue("Name");
                if (obj != null)
                    return obj.ToString();
            }
            return "";
        }

        private void PumpProcess(Process process)
        {
            string line = string.Empty;
            while (!process.HasExited)
            {
                process.StandardInput.WriteLine("noop");
                int num = process.StandardOutput.Read();
                switch (num)
                {
                    case -1:
                        return;
                    case 13:
                    case 10:
                        if (!(line == string.Empty))
                        {
                            if (!this.IsHandleCreated)
                                return;
                            this.Invoke((Action)delegate
                            {
                                try
                                {
                                    if (string.IsNullOrEmpty(line))
                                        return;
                                    if (line.Contains("STATUS: Phone not detected by adb."))
                                    {
                                        this.statusImage.Image = (Image)Resources.usb_broken;
                                        this.icon.Icon = Icon.FromHandle(((Bitmap)this.statusImage.Image).GetHicon());
                                    }
                                    else if (line.Contains("STATUS: Tether has connected."))
                                    {
                                        this.statusImage.Image = (Image)Resources.usb_on;
                                        this.icon.Icon = Icon.FromHandle(((Bitmap)this.statusImage.Image).GetHicon());
                                    }
                                    else if (line.Contains("STATUS: Tether has disconnected."))
                                    {
                                        this.statusImage.Image = (Image)Resources.usb_pending;
                                        this.icon.Icon = Icon.FromHandle(((Bitmap)this.statusImage.Image).GetHicon());
                                    }
                                    else if (line.Contains("STATUS: Connected to phone. Waiting for tether connection."))
                                    {
                                        this.statusImage.Image = (Image)Resources.usb_pending;
                                        this.icon.Icon = Icon.FromHandle(((Bitmap)this.statusImage.Image).GetHicon());
                                    }
                                    if (line.StartsWith("STATUS: "))
                                        this.status.Text = line.Substring("STATUS: ".Length);

                                    LogMessage(line);
                                }
                                catch (Exception ex)
                                {
                                    LogMessage(ex.ToString());
                                }
                            });
                            line = string.Empty;
                            continue;
                        }
                        continue;
                    default:
                        line += (char)num;
                        continue;
                }
            }
        }

        private void StopIt()
        {
            this.icon.Icon = Icon.FromHandle(Resources.usb_off.GetHicon());
            this.statusImage.Image = (Image)Resources.usb_off;
            this.toggleButton.Text = "Start";
        }

        private void LogMessage(string message)
        { 
            this.Invoke((Action)delegate
            { 
                this.nodeLog.AppendText(message); 
                this.nodeLog.AppendText("\r\n"); 


                if (this.nodeLog.Text.Length > 20000) 
                    this.nodeLog.Text = this.nodeLog.Text.Substring(20000); 


                this.nodeLog.SelectionStart = this.nodeLog.Text.Length; 
                this.nodeLog.ScrollToCaret(); 
            }); 
        }

        private void toggleButton_Click(object sender, EventArgs e)
        {
            if (this.nodeProcess != null)
            {
                this.KillIt();
                this.StopIt();
            }
            else
            {
                this.toggleButton.Text = "Stop";
                var networkName = TetherForm.GetNetworkHumanName(TetherForm.GetDeviceGuid());
                Task.Run(() =>
                {
                    var executablePath = Path.GetDirectoryName(Application.ExecutablePath);
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        RedirectStandardOutput = true,
                        RedirectStandardInput = true,
                        FileName = Path.Combine(executablePath, "win32\\run.bat"),
                        Arguments = "\"" + networkName + "\""
                    };

                    try
                    {
                        this.nodeProcess = Process.Start(startInfo);
                        this.PumpProcess(this.nodeProcess);
                    }
                    catch (Exception ex)
                    {
                        LogMessage(ex.ToString());
                    }
                    finally
                    {
                        this.KillIt();
                        if (this.IsHandleCreated)
                            this.BeginInvoke((Action)delegate
                            {
                                try
                                {
                                    this.StopIt();
                                }
                                catch (Exception ex)
                                {
                                    LogMessage(ex.ToString());
                                }
                            });
                    }
                });
            }
        }

        private void KillIt()
        {
            if (this.nodeProcess == null)
                return;
            try
            {
                this.nodeProcess.StandardInput.WriteLine("quit");
            }
            catch (Exception ex)
            {
            }
            try
            {
                this.nodeProcess.StandardOutput.Close();
            }
            catch (Exception ex)
            {
            }
            try
            {
                this.nodeProcess.StandardInput.Close();
            }
            catch (Exception ex)
            {
            }
            try
            {
                this.nodeProcess.Kill();
            }
            catch (Exception ex)
            {
            }
            this.nodeProcess = (Process)null;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!this.exiting)
            {
                this.icon.ShowBalloonTip(5000, "Tether", "Tether is running in the background", ToolTipIcon.Info);
                e.Cancel = true;
                this.Visible = false;
            }
            else
            {
                this.icon.Visible = false;
                this.KillIt();
                base.OnClosing(e);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.exiting = true;
            this.Close();
        }

        private void downloadDriversToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.clockworkmod.com/tether/drivers");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
                this.components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.nodeLog = new TextBox();
            this.toggleButton = new Button();
            this.label1 = new Label();
            this.statusImage = new PictureBox();
            this.menuStrip1 = new MenuStrip();
            this.fileToolStripMenuItem = new ToolStripMenuItem();
            this.downloadDriversToolStripMenuItem = new ToolStripMenuItem();
            this.toolStripMenuItem1 = new ToolStripSeparator();
            this.exitToolStripMenuItem = new ToolStripMenuItem();
            this.status = new TextBox();
            ((ISupportInitialize)this.statusImage).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            this.nodeLog.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.nodeLog.BackColor = SystemColors.Window;
            this.nodeLog.Location = new Point(194, 40);
            this.nodeLog.Multiline = true;
            this.nodeLog.Name = "nodeLog";
            this.nodeLog.ReadOnly = true;
            this.nodeLog.ScrollBars = ScrollBars.Vertical;
            this.nodeLog.Size = new Size(428, 388);
            this.nodeLog.TabIndex = 0;
            this.toggleButton.Anchor = AnchorStyles.Left;
            this.toggleButton.Location = new Point(65, 405);
            this.toggleButton.Name = "toggleButton";
            this.toggleButton.Size = new Size(75, 23);
            this.toggleButton.TabIndex = 1;
            this.toggleButton.Text = "Start";
            this.toggleButton.UseVisualStyleBackColor = true;
            this.toggleButton.Click += new EventHandler(this.toggleButton_Click);
            this.label1.AutoSize = true;
            this.label1.Location = new Point(194, 24);
            this.label1.Name = "label1";
            this.label1.Size = new Size(59, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Tether Log";
            this.statusImage.Image = (Image)Resources.usb_off;
            this.statusImage.Location = new Point(10, 27);
            this.statusImage.Name = "statusImage";
            this.statusImage.Size = new Size(178, 273);
            this.statusImage.TabIndex = 3;
            this.statusImage.TabStop = false;
            this.menuStrip1.Items.AddRange(new ToolStripItem[1]
            {
        (ToolStripItem) this.fileToolStripMenuItem
            });
            this.menuStrip1.Location = new Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new Size(637, 24);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            this.fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[3]
            {
        (ToolStripItem) this.downloadDriversToolStripMenuItem,
        (ToolStripItem) this.toolStripMenuItem1,
        (ToolStripItem) this.exitToolStripMenuItem
            });
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            this.downloadDriversToolStripMenuItem.Name = "downloadDriversToolStripMenuItem";
            this.downloadDriversToolStripMenuItem.Size = new Size(167, 22);
            this.downloadDriversToolStripMenuItem.Text = "Download Drivers";
            this.downloadDriversToolStripMenuItem.Click += new EventHandler(this.downloadDriversToolStripMenuItem_Click);
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new Size(164, 6);
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new Size(167, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new EventHandler(this.exit_Click);
            this.status.BorderStyle = BorderStyle.None;
            this.status.Font = new Font("Microsoft Sans Serif", 16f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.status.Location = new Point(13, 307);
            this.status.Multiline = true;
            this.status.Name = "status";
            this.status.ReadOnly = true;
            this.status.Size = new Size(175, 92);
            this.status.TabIndex = 5;
            this.status.Text = "Tether is not running.";
            this.status.TextAlign = HorizontalAlignment.Center;
            this.AutoScaleDimensions = new SizeF(6f, 13f);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(637, 440);
            this.Controls.Add((Control)this.status);
            this.Controls.Add((Control)this.statusImage);
            this.Controls.Add((Control)this.label1);
            this.Controls.Add((Control)this.toggleButton);
            this.Controls.Add((Control)this.nodeLog);
            this.Controls.Add((Control)this.menuStrip1);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "TetherForm";
            this.Text = "Tether";
            ((ISupportInitialize)this.statusImage).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
