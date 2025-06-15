namespace AdGuardTrayApp
{
    partial class ConfigurationForm
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
            System.Windows.Forms.Label lblHost;
            System.Windows.Forms.Label lblUsername;
            System.Windows.Forms.Label lblPassword;
            System.Windows.Forms.Label lblTargetIP;
            System.Windows.Forms.Label lblDuration;
            lblHost = new System.Windows.Forms.Label();
            lblUsername = new System.Windows.Forms.Label();
            lblPassword = new System.Windows.Forms.Label();
            lblTargetIP = new System.Windows.Forms.Label();
            lblDuration = new System.Windows.Forms.Label();
            this.txtAdGuardHost = new System.Windows.Forms.TextBox();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtTargetIP = new System.Windows.Forms.TextBox();
            this.numDuration = new System.Windows.Forms.NumericUpDown();
            this.chkAutostart = new System.Windows.Forms.CheckBox();
            this.btnDetectIP = new System.Windows.Forms.Button();
            this.btnTest = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numDuration)).BeginInit();
            this.SuspendLayout();
            // 
            // lblHost
            // 
            lblHost.AutoSize = true;
            lblHost.Location = new System.Drawing.Point(10, 15);
            lblHost.Name = "lblHost";
            lblHost.Size = new System.Drawing.Size(120, 20);
            lblHost.TabIndex = 0;
            lblHost.Text = "AdGuard Host:";
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Location = new System.Drawing.Point(10, 45);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new System.Drawing.Size(120, 20);
            lblUsername.TabIndex = 2;
            lblUsername.Text = "Benutzername:";
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new System.Drawing.Point(10, 75);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new System.Drawing.Size(120, 20);
            lblPassword.TabIndex = 4;
            lblPassword.Text = "Passwort:";
            // 
            // lblTargetIP
            // 
            lblTargetIP.AutoSize = true;
            lblTargetIP.Location = new System.Drawing.Point(10, 105);
            lblTargetIP.Name = "lblTargetIP";
            lblTargetIP.Size = new System.Drawing.Size(120, 20);
            lblTargetIP.TabIndex = 6;
            lblTargetIP.Text = "Ziel-IP:";
            // 
            // lblDuration
            // 
            lblDuration.AutoSize = true;
            lblDuration.Location = new System.Drawing.Point(10, 135);
            lblDuration.Name = "lblDuration";
            lblDuration.Size = new System.Drawing.Size(120, 20);
            lblDuration.TabIndex = 8;
            lblDuration.Text = "Dauer (Minuten):";
            // 
            // txtAdGuardHost
            // 
            this.txtAdGuardHost.Location = new System.Drawing.Point(140, 12);
            this.txtAdGuardHost.Name = "txtAdGuardHost";
            this.txtAdGuardHost.Size = new System.Drawing.Size(230, 27);
            this.txtAdGuardHost.TabIndex = 1;
            // 
            // txtUsername
            // 
            this.txtUsername.Location = new System.Drawing.Point(140, 42);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(230, 27);
            this.txtUsername.TabIndex = 3;
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(140, 72);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(230, 27);
            this.txtPassword.TabIndex = 5;
            this.txtPassword.UseSystemPasswordChar = true;
            // 
            // txtTargetIP
            // 
            this.txtTargetIP.Location = new System.Drawing.Point(140, 102);
            this.txtTargetIP.Name = "txtTargetIP";
            this.txtTargetIP.Size = new System.Drawing.Size(180, 27);
            this.txtTargetIP.TabIndex = 7;
            // 
            // btnDetectIP
            // 
            this.btnDetectIP.Location = new System.Drawing.Point(325, 102);
            this.btnDetectIP.Name = "btnDetectIP";
            this.btnDetectIP.Size = new System.Drawing.Size(45, 27);
            this.btnDetectIP.TabIndex = 8;
            this.btnDetectIP.Text = "🔍";
            this.btnDetectIP.UseVisualStyleBackColor = true;
            this.btnDetectIP.Click += new System.EventHandler(this.BtnDetectIP_Click);
            // 
            // numDuration
            // 
            this.numDuration.Location = new System.Drawing.Point(140, 132);
            this.numDuration.Maximum = new decimal(new int[] {
            1440,
            0,
            0,
            0});
            this.numDuration.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numDuration.Name = "numDuration";
            this.numDuration.Size = new System.Drawing.Size(100, 27);
            this.numDuration.TabIndex = 9;
            this.numDuration.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // chkAutostart
            // 
            this.chkAutostart.AutoSize = true;
            this.chkAutostart.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chkAutostart.Location = new System.Drawing.Point(10, 165);
            this.chkAutostart.Name = "chkAutostart";
            this.chkAutostart.Size = new System.Drawing.Size(250, 24);
            this.chkAutostart.TabIndex = 10;
            this.chkAutostart.Text = "Mit Windows starten (Autostart)";
            this.chkAutostart.UseVisualStyleBackColor = true;
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(10, 210);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(120, 30);
            this.btnTest.TabIndex = 11;
            this.btnTest.Text = "Verbindung testen";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.BtnTest_Click);
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(210, 260);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 30);
            this.btnOK.TabIndex = 12;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(295, 260);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 30);
            this.btnCancel.TabIndex = 13;
            this.btnCancel.Text = "Abbrechen";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // ConfigurationForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(384, 302);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.chkAutostart);
            this.Controls.Add(this.numDuration);
            this.Controls.Add(lblDuration);
            this.Controls.Add(this.btnDetectIP);
            this.Controls.Add(this.txtTargetIP);
            this.Controls.Add(lblTargetIP);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(lblPassword);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(lblUsername);
            this.Controls.Add(this.txtAdGuardHost);
            this.Controls.Add(lblHost);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigurationForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AdGuard Konfiguration";
            ((System.ComponentModel.ISupportInitialize)(this.numDuration)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox txtAdGuardHost;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.TextBox txtTargetIP;
        private System.Windows.Forms.NumericUpDown numDuration;
        private System.Windows.Forms.CheckBox chkAutostart;
        private System.Windows.Forms.Button btnDetectIP;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}
