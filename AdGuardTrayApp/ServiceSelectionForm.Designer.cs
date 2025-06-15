namespace AdGuardTrayApp
{
    partial class ServiceSelectionForm
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
            tabControl = new TabControl();
            servicesTab = new TabPage();
            servicesListBox = new CheckedListBox();
            servicesButtonPanel = new Panel();
            deselectAllServicesButton = new Button();
            selectAllServicesButton = new Button();
            servicesLabel = new Label();
            rulesTab = new TabPage();
            customRulesListBox = new CheckedListBox();
            rulesButtonPanel = new Panel();
            deselectAllRulesButton = new Button();
            selectAllRulesButton = new Button();
            rulesLabel = new Label();
            buttonPanel = new Panel();
            cancelButton = new Button();
            saveButton = new Button();
            tabControl.SuspendLayout();
            servicesTab.SuspendLayout();
            servicesButtonPanel.SuspendLayout();
            rulesTab.SuspendLayout();
            rulesButtonPanel.SuspendLayout();
            buttonPanel.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Controls.Add(servicesTab);
            tabControl.Controls.Add(rulesTab);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 0);
            tabControl.Margin = new Padding(3, 2, 3, 2);
            tabControl.Name = "tabControl";
            tabControl.Padding = new Point(10, 10);
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(751, 514);
            tabControl.TabIndex = 0;
            // 
            // servicesTab
            // 
            servicesTab.Controls.Add(servicesListBox);
            servicesTab.Controls.Add(servicesButtonPanel);
            servicesTab.Controls.Add(servicesLabel);
            servicesTab.Location = new Point(4, 38);
            servicesTab.Margin = new Padding(3, 2, 3, 2);
            servicesTab.Name = "servicesTab";
            servicesTab.Padding = new Padding(9, 8, 9, 8);
            servicesTab.Size = new Size(743, 472);
            servicesTab.TabIndex = 0;
            servicesTab.Text = "Blockierte Dienste";
            servicesTab.UseVisualStyleBackColor = true;
            // 
            // servicesListBox
            // 
            servicesListBox.CheckOnClick = true;
            servicesListBox.Dock = DockStyle.Fill;
            servicesListBox.Font = new Font("Segoe UI", 9F);
            servicesListBox.FormattingEnabled = true;
            servicesListBox.Location = new Point(9, 46);
            servicesListBox.Margin = new Padding(3, 2, 3, 2);
            servicesListBox.MultiColumn = true;
            servicesListBox.Name = "servicesListBox";
            servicesListBox.Size = new Size(725, 388);
            servicesListBox.TabIndex = 1;
            // 
            // servicesButtonPanel
            // 
            servicesButtonPanel.Controls.Add(deselectAllServicesButton);
            servicesButtonPanel.Controls.Add(selectAllServicesButton);
            servicesButtonPanel.Dock = DockStyle.Bottom;
            servicesButtonPanel.Location = new Point(9, 434);
            servicesButtonPanel.Margin = new Padding(3, 2, 3, 2);
            servicesButtonPanel.Name = "servicesButtonPanel";
            servicesButtonPanel.Size = new Size(725, 30);
            servicesButtonPanel.TabIndex = 2;
            // 
            // deselectAllServicesButton
            // 
            deselectAllServicesButton.Location = new Point(105, 4);
            deselectAllServicesButton.Margin = new Padding(3, 2, 3, 2);
            deselectAllServicesButton.Name = "deselectAllServicesButton";
            deselectAllServicesButton.Size = new Size(88, 22);
            deselectAllServicesButton.TabIndex = 1;
            deselectAllServicesButton.Text = "Alle abwählen";
            deselectAllServicesButton.UseVisualStyleBackColor = true;
            deselectAllServicesButton.Click += DeselectAllServicesButton_Click;
            // 
            // selectAllServicesButton
            // 
            selectAllServicesButton.Location = new Point(9, 4);
            selectAllServicesButton.Margin = new Padding(3, 2, 3, 2);
            selectAllServicesButton.Name = "selectAllServicesButton";
            selectAllServicesButton.Size = new Size(88, 22);
            selectAllServicesButton.TabIndex = 0;
            selectAllServicesButton.Text = "Alle auswählen";
            selectAllServicesButton.UseVisualStyleBackColor = true;
            selectAllServicesButton.Click += SelectAllServicesButton_Click;
            // 
            // servicesLabel
            // 
            servicesLabel.Dock = DockStyle.Top;
            servicesLabel.Font = new Font("Segoe UI", 9F);
            servicesLabel.Location = new Point(9, 8);
            servicesLabel.Name = "servicesLabel";
            servicesLabel.Size = new Size(725, 38);
            servicesLabel.TabIndex = 0;
            servicesLabel.Text = "Wählen Sie die Dienste aus, die BLOCKIERT bleiben sollen:\r\n(✓ = Dienst wird blockiert, ☐ = Dienst wird entsperrt)";
            servicesLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // rulesTab
            // 
            rulesTab.Controls.Add(customRulesListBox);
            rulesTab.Controls.Add(rulesButtonPanel);
            rulesTab.Controls.Add(rulesLabel);
            rulesTab.Location = new Point(4, 38);
            rulesTab.Margin = new Padding(3, 2, 3, 2);
            rulesTab.Name = "rulesTab";
            rulesTab.Padding = new Padding(9, 8, 9, 8);
            rulesTab.Size = new Size(496, 282);
            rulesTab.TabIndex = 1;
            rulesTab.Text = "Benutzerdefinierte Filterregeln";
            rulesTab.UseVisualStyleBackColor = true;
            // 
            // customRulesListBox
            // 
            customRulesListBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            customRulesListBox.CheckOnClick = true;
            customRulesListBox.Font = new Font("Consolas", 8F);
            customRulesListBox.FormattingEnabled = true;
            customRulesListBox.IntegralHeight = false;
            customRulesListBox.Location = new Point(9, 45);
            customRulesListBox.Margin = new Padding(3, 2, 3, 2);
            customRulesListBox.Name = "customRulesListBox";
            customRulesListBox.Size = new Size(480, 214);
            customRulesListBox.TabIndex = 1;
            // 
            // rulesButtonPanel
            // 
            rulesButtonPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rulesButtonPanel.Controls.Add(deselectAllRulesButton);
            rulesButtonPanel.Controls.Add(selectAllRulesButton);
            rulesButtonPanel.Location = new Point(9, 258);
            rulesButtonPanel.Margin = new Padding(3, 2, 3, 2);
            rulesButtonPanel.Name = "rulesButtonPanel";
            rulesButtonPanel.Size = new Size(480, 30);
            rulesButtonPanel.TabIndex = 2;
            // 
            // deselectAllRulesButton
            // 
            deselectAllRulesButton.Location = new Point(105, 4);
            deselectAllRulesButton.Margin = new Padding(3, 2, 3, 2);
            deselectAllRulesButton.Name = "deselectAllRulesButton";
            deselectAllRulesButton.Size = new Size(88, 22);
            deselectAllRulesButton.TabIndex = 1;
            deselectAllRulesButton.Text = "Alle abwählen";
            deselectAllRulesButton.UseVisualStyleBackColor = true;
            deselectAllRulesButton.Click += DeselectAllRulesButton_Click;
            // 
            // selectAllRulesButton
            // 
            selectAllRulesButton.Location = new Point(9, 4);
            selectAllRulesButton.Margin = new Padding(3, 2, 3, 2);
            selectAllRulesButton.Name = "selectAllRulesButton";
            selectAllRulesButton.Size = new Size(88, 22);
            selectAllRulesButton.TabIndex = 0;
            selectAllRulesButton.Text = "Alle auswählen";
            selectAllRulesButton.UseVisualStyleBackColor = true;
            selectAllRulesButton.Click += SelectAllRulesButton_Click;
            // 
            // rulesLabel
            // 
            rulesLabel.Dock = DockStyle.Top;
            rulesLabel.Font = new Font("Segoe UI", 9F);
            rulesLabel.Location = new Point(9, 8);
            rulesLabel.Name = "rulesLabel";
            rulesLabel.Size = new Size(478, 38);
            rulesLabel.TabIndex = 0;
            rulesLabel.Text = "Filterregeln zum Deaktivieren auswählen (nur '#ADGUARD_TRAY_APP' Regeln):\r\n💡 Setup: AdGuard Home → Filter → Benutzerdefinierte Regeln → '#ADGUARD_TRAY_APP' anhängen";
            rulesLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // buttonPanel
            // 
            buttonPanel.Controls.Add(cancelButton);
            buttonPanel.Controls.Add(saveButton);
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.Location = new Point(0, 514);
            buttonPanel.Margin = new Padding(3, 2, 3, 2);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Padding = new Padding(9, 8, 9, 8);
            buttonPanel.Size = new Size(751, 50);
            buttonPanel.TabIndex = 1;
            // 
            // cancelButton
            // 
            cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(655, 10);
            cancelButton.Margin = new Padding(3, 2, 3, 2);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(88, 32);
            cancelButton.TabIndex = 1;
            cancelButton.Text = "Abbrechen";
            cancelButton.UseVisualStyleBackColor = true;
            // 
            // saveButton
            // 
            saveButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            saveButton.DialogResult = DialogResult.OK;
            saveButton.Location = new Point(487, 10);
            saveButton.Margin = new Padding(3, 2, 3, 2);
            saveButton.Name = "saveButton";
            saveButton.Size = new Size(159, 32);
            saveButton.TabIndex = 0;
            saveButton.Text = "Speichern und Anwenden";
            saveButton.UseVisualStyleBackColor = true;
            saveButton.Click += SaveButton_Click;
            // 
            // ServiceSelectionForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(751, 564);
            Controls.Add(tabControl);
            Controls.Add(buttonPanel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(3, 2, 3, 2);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ServiceSelectionForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "AdGuard Dienste und Filter verwalten";
            tabControl.ResumeLayout(false);
            servicesTab.ResumeLayout(false);
            servicesButtonPanel.ResumeLayout(false);
            rulesTab.ResumeLayout(false);
            rulesButtonPanel.ResumeLayout(false);
            buttonPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage servicesTab;
        private System.Windows.Forms.CheckedListBox servicesListBox;
        private System.Windows.Forms.Panel servicesButtonPanel;
        private System.Windows.Forms.Button deselectAllServicesButton;
        private System.Windows.Forms.Button selectAllServicesButton;
        private System.Windows.Forms.Label servicesLabel;
        private System.Windows.Forms.TabPage rulesTab;
        private System.Windows.Forms.CheckedListBox customRulesListBox;
        private System.Windows.Forms.Panel rulesButtonPanel;
        private System.Windows.Forms.Button deselectAllRulesButton;
        private System.Windows.Forms.Button selectAllRulesButton;
        private System.Windows.Forms.Label rulesLabel;
        private System.Windows.Forms.Panel buttonPanel;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button saveButton;
    }
}
