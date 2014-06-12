using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImplantPlayer
{
    /// <summary>
    /// Console windows displays all console messages coming from the
    /// system and implants.
    /// </summary>
    public partial class ConsoleForm : Form
    {
        public ConsoleForm()
        {
            InitializeComponent();

            // hide instead of close
            this.FormClosing += (sender, e) =>
            {
                this.Hide();
                e.Cancel = true;
            };

            this.Shown += (sender, e) =>
                {
                    if (needsReload)
                    {
                        Refresh();
                    }
                };
        }

        private bool needsReload = true;

        /// <summary>
        /// Reloads all console messages.
        /// </summary>
        public void Refresh()
        {
            needsReload = false;

            txtConsole.Clear();
            foreach (var m in LPToolKit.Session.UserSession.Current.Console.Messages.AllItems)
            {
                Append(m.Timestamp, m.Message, m.Source);
            }
        }

        private void Append(DateTime time, string msg, string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                source = "system";
            }

            var c = source == "system" ? Color.Red : Color.Black;

            string s = string.Format("[{0:t} {1}] {2}", time, source, msg);

            this.BeginInvoke((MethodInvoker)delegate()
            {
                var startIndex = txtConsole.TextLength;
                txtConsole.Text += s;

                txtConsole.Select(startIndex, s.Length);
                txtConsole.SelectionColor = c;
                txtConsole.DeselectAll();

                txtConsole.ScrollToCaret();
            });
        }

        /// <summary>
        /// Adds a message to display in the console window.
        /// </summary>
        public void AddMessage(DateTime time, string msg, string source = null)
        {
            try
            {
                Append(time, msg, source ?? "system");
            }
            catch
            {
                // reload the window later if we got an error
                needsReload = true;
            }
        }
    }
}
