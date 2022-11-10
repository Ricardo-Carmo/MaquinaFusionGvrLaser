using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _22079AI
{
    public partial class About : Form
    {
        private bool _dragging = false;
        private Point _start_point = new Point(0, 0);

        public About(string machineName)
        {
            InitializeComponent();

            lblHeader.Text = machineName;
            label1.Text = machineName;

            var version = Assembly.GetEntryAssembly().GetName().Version;
            var buildDateTime = new DateTime(2000, 1, 1).Add(new TimeSpan(TimeSpan.TicksPerDay * version.Build + TimeSpan.TicksPerSecond * 2 * version.Revision));
            label2.Text = "Versão " + Convert.ToString(version).Replace(",", ".") + " (" + buildDateTime.ToShortDateString() + ")";
            label4.Text = "Copyright © " + DateTime.Now.Year + " STREAK" + Environment.NewLine + "All rights reserved.";
        }

        private void btnSair_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lblHeader_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - _start_point.X, p.Y - _start_point.Y);
            }
        }

        private void lblHeader_MouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }

        private void lblHeader_MouseDown(object sender, MouseEventArgs e)
        {
            _dragging = true;  // _dragging is your variable flag
            _start_point = new Point(e.X, e.Y);
        }

        private void About_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void About_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Process.Start("bubbles.scr");
        }
    }
}
