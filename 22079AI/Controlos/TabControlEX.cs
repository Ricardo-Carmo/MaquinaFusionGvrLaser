using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace _22079AI
{
    public class TabControlEx : TabControl
    {
        /// <summary>
        /// Gets or sets a value indicating whether the tab headers should be drawn
        /// </summary>
        [
        Description("Gets or sets a value indicating whether the tab headers should be drawn"),
        DefaultValue(true)
        ]
        public bool ShowTabHeaders { get; set; }
        public TabControlEx()
            : base()
        {
        }
        protected override void WndProc(ref Message m)
        {
            //Hide tabs by trapping the TCM_ADJUSTRECT message
            if (!ShowTabHeaders && m.Msg == 0x1328 && !DesignMode)
                m.Result = (IntPtr)1;
            else
                base.WndProc(ref m);
        }

        /// <summary>
        /// Intercept any key combinations that would change the active tab.
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            bool changeTabKeyCombination = (e.Control && (e.KeyCode == Keys.Tab || e.KeyCode == Keys.Next || e.KeyCode == Keys.Prior));

            if (!changeTabKeyCombination)
            {
                base.OnKeyDown(e);
            }
        }
    }
}
