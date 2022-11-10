namespace _22079AI
{
    partial class SingleInspectionItem
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
            this.lblId = new System.Windows.Forms.Label();
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
            this.WindowControl.Size = new System.Drawing.Size(746, 96);
            this.WindowControl.TabIndex = 40;
            this.WindowControl.WindowSize = new System.Drawing.Size(746, 96);
            this.WindowControl.HMouseUp += new HalconDotNet.HMouseEventHandler(this.WindowControl_HMouseUp);
            // 
            // lblId
            // 
            this.lblId.AutoSize = true;
            this.lblId.BackColor = System.Drawing.Color.Black;
            this.lblId.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblId.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblId.ForeColor = System.Drawing.Color.Red;
            this.lblId.Location = new System.Drawing.Point(0, 83);
            this.lblId.Name = "lblId";
            this.lblId.Size = new System.Drawing.Size(22, 13);
            this.lblId.TabIndex = 41;
            this.lblId.Text = "---";
            // 
            // SingleInspectionItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.lblId);
            this.Controls.Add(this.WindowControl);
            this.Name = "SingleInspectionItem";
            this.Size = new System.Drawing.Size(746, 96);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public HalconDotNet.HWindowControl WindowControl;
        private System.Windows.Forms.Label lblId;
    }
}
