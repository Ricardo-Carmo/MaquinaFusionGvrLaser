namespace _22079AI
{

    partial class CamObject
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.WindowControl = new HalconDotNet.HWindowControl();
            this.SuspendLayout();
            // 
            // WindowControl
            // 
            this.WindowControl.BackColor = System.Drawing.Color.Black;
            this.WindowControl.BorderColor = System.Drawing.Color.Black;
            this.WindowControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.WindowControl.ImagePart = new System.Drawing.Rectangle(0, 0, 4912, 3684);
            this.WindowControl.Location = new System.Drawing.Point(0, 0);
            this.WindowControl.Name = "WindowControl";
            this.WindowControl.Size = new System.Drawing.Size(428, 336);
            this.WindowControl.TabIndex = 39;
            this.WindowControl.WindowSize = new System.Drawing.Size(428, 336);
            // 
            // CamObject
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.WindowControl);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "CamObject";
            this.Size = new System.Drawing.Size(428, 336);
            this.ResumeLayout(false);

        }

        #endregion

        public HalconDotNet.HWindowControl WindowControl;
    }
}
