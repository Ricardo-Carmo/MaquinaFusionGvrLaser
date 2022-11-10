namespace _22079AI
{
    partial class AdicionarOperador
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdicionarOperador));
            this.panel1 = new System.Windows.Forms.Panel();
            this.lstBoxNivelPermissao = new System.Windows.Forms.ListBox();
            this.btnOPAIniciarOPA = new System.Windows.Forms.Button();
            this.panel56 = new System.Windows.Forms.Panel();
            this.btnSair = new System.Windows.Forms.PictureBox();
            this.lblHeader = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtNomeOperador = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblOPAOperador = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel56.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnSair)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lstBoxNivelPermissao);
            this.panel1.Controls.Add(this.btnOPAIniciarOPA);
            this.panel1.Controls.Add(this.panel56);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.txtPassword);
            this.panel1.Controls.Add(this.txtNomeOperador);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.lblOPAOperador);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(371, 277);
            this.panel1.TabIndex = 0;
            // 
            // lstBoxNivelPermissao
            // 
            this.lstBoxNivelPermissao.Font = new System.Drawing.Font("Arial", 12F);
            this.lstBoxNivelPermissao.FormattingEnabled = true;
            this.lstBoxNivelPermissao.ItemHeight = 18;
            this.lstBoxNivelPermissao.Items.AddRange(new object[] {
            "Operador",
            "Administrador"});
            this.lstBoxNivelPermissao.Location = new System.Drawing.Point(164, 147);
            this.lstBoxNivelPermissao.Name = "lstBoxNivelPermissao";
            this.lstBoxNivelPermissao.Size = new System.Drawing.Size(189, 58);
            this.lstBoxNivelPermissao.TabIndex = 3;
            // 
            // btnOPAIniciarOPA
            // 
            this.btnOPAIniciarOPA.BackColor = System.Drawing.Color.White;
            this.btnOPAIniciarOPA.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOPAIniciarOPA.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOPAIniciarOPA.ForeColor = System.Drawing.Color.Black;
            this.btnOPAIniciarOPA.Location = new System.Drawing.Point(19, 225);
            this.btnOPAIniciarOPA.Name = "btnOPAIniciarOPA";
            this.btnOPAIniciarOPA.Size = new System.Drawing.Size(144, 30);
            this.btnOPAIniciarOPA.TabIndex = 4;
            this.btnOPAIniciarOPA.Text = "Confirmar";
            this.btnOPAIniciarOPA.UseVisualStyleBackColor = false;
            this.btnOPAIniciarOPA.Click += new System.EventHandler(this.btnOPAIniciarOPA_Click);
            // 
            // panel56
            // 
            this.panel56.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(204)))), ((int)(((byte)(255)))));
            this.panel56.Controls.Add(this.btnSair);
            this.panel56.Controls.Add(this.lblHeader);
            this.panel56.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel56.Location = new System.Drawing.Point(0, 0);
            this.panel56.Name = "panel56";
            this.panel56.Size = new System.Drawing.Size(371, 35);
            this.panel56.TabIndex = 80;
            // 
            // btnSair
            // 
            this.btnSair.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSair.Image = ((System.Drawing.Image)(resources.GetObject("btnSair.Image")));
            this.btnSair.Location = new System.Drawing.Point(336, 0);
            this.btnSair.Name = "btnSair";
            this.btnSair.Size = new System.Drawing.Size(35, 35);
            this.btnSair.TabIndex = 68;
            this.btnSair.TabStop = false;
            this.btnSair.Click += new System.EventHandler(this.btnSair_Click);
            // 
            // lblHeader
            // 
            this.lblHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(153)))));
            this.lblHeader.Location = new System.Drawing.Point(0, 0);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(371, 35);
            this.lblHeader.TabIndex = 0;
            this.lblHeader.Text = "   ";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblHeader.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblHeader_MouseDown);
            this.lblHeader.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lblHeader_MouseMove);
            this.lblHeader.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblHeader_MouseUp);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.White;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Calibri", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.ForeColor = System.Drawing.Color.Black;
            this.button1.Location = new System.Drawing.Point(209, 225);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(144, 30);
            this.button1.TabIndex = 5;
            this.button1.Text = "Cancelar";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtPassword
            // 
            this.txtPassword.BackColor = System.Drawing.Color.White;
            this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPassword.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPassword.Location = new System.Drawing.Point(164, 101);
            this.txtPassword.MaxLength = 20;
            this.txtPassword.Multiline = true;
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(189, 26);
            this.txtPassword.TabIndex = 2;
            this.txtPassword.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtNomeOperador
            // 
            this.txtNomeOperador.BackColor = System.Drawing.Color.White;
            this.txtNomeOperador.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtNomeOperador.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNomeOperador.Location = new System.Drawing.Point(164, 55);
            this.txtNomeOperador.MaxLength = 20;
            this.txtNomeOperador.Multiline = true;
            this.txtNomeOperador.Name = "txtNomeOperador";
            this.txtNomeOperador.Size = new System.Drawing.Size(189, 26);
            this.txtNomeOperador.TabIndex = 1;
            this.txtNomeOperador.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(12, 104);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(140, 19);
            this.label1.TabIndex = 93;
            this.label1.Text = "Password:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(12, 147);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(140, 19);
            this.label2.TabIndex = 93;
            this.label2.Text = "Nível Permissão:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblOPAOperador
            // 
            this.lblOPAOperador.BackColor = System.Drawing.Color.Transparent;
            this.lblOPAOperador.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOPAOperador.ForeColor = System.Drawing.Color.White;
            this.lblOPAOperador.Location = new System.Drawing.Point(12, 58);
            this.lblOPAOperador.Name = "lblOPAOperador";
            this.lblOPAOperador.Size = new System.Drawing.Size(140, 19);
            this.lblOPAOperador.TabIndex = 93;
            this.lblOPAOperador.Text = "Nome Operador:";
            this.lblOPAOperador.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // AdicionarOperador
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.ClientSize = new System.Drawing.Size(371, 277);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AdicionarOperador";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Gestão de Operadores";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AdicionarOperador_FormClosing);
            this.Load += new System.EventHandler(this.AdicionarOperador_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel56.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.btnSair)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel56;
        private System.Windows.Forms.PictureBox btnSair;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.TextBox txtNomeOperador;
        private System.Windows.Forms.Label lblOPAOperador;
        private System.Windows.Forms.ListBox lstBoxNivelPermissao;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.Button btnOPAIniciarOPA;
        public System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label label1;
    }
}