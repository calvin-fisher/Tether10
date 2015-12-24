// Decompiled with JetBrains decompiler
// Type: TetherWindows.Program
// Assembly: TetherWindows, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EC2D7451-4D97-4986-80E3-DF30C217B4F5
// Assembly location: C:\Program Files (x86)\ClockworkMod\Tether\TetherWindows.exe

using System;
using System.Threading;
using System.Windows.Forms;

namespace TetherWindows
{
    internal static class Program
    {
        private static TetherForm form;
        private static Semaphore semaphore;

        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                Program.semaphore = Semaphore.OpenExisting("tether");
                Program.semaphore.Release();
                return;
            }
            catch (Exception ex1)
            {
                Program.semaphore = new Semaphore(0, 1, "tether");
                new Thread((ThreadStart)(() =>
                {
                    while (Program.semaphore != null)
                    {
                        if (Program.semaphore.WaitOne(1000))
                        {
                            try
                            {
                                Program.form.Invoke((Action)delegate
                                {
                                    Program.form.Visible = true;
                                });
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                })).Start();
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Application.Run((Form)(Program.form = new TetherForm()));
            }
            finally
            {
                Program.semaphore = (Semaphore)null;
            }
        }
    }
}
