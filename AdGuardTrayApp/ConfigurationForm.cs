using System;
using System.Windows.Forms;

namespace AdGuardTrayApp
{
    public partial class ConfigurationForm : Form
    {
        public AdGuardConfig Configuration { get; private set; }

        private TextBox? txtAdGuardHost;
        private TextBox? txtUsername;
        private TextBox? txtPassword;
        private TextBox? txtTargetIP;
        private NumericUpDown? numDuration;
        private CheckBox? chkAutostart;
        private Button? btnOK;
        private Button? btnCancel;
        private Button? btnTest;

        public ConfigurationForm(AdGuardConfig config)
        {
            Configuration = config;
            InitializeComponent();
            LoadConfiguration();
        }

        private void InitializeComponent()
        {
            this.Text = "AdGuard Konfiguration";
            this.Size = new System.Drawing.Size(400, 350);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            var lblHost = new Label { Text = "AdGuard Host:", Location = new System.Drawing.Point(10, 15), Size = new System.Drawing.Size(120, 23) };
            txtAdGuardHost = new TextBox { Location = new System.Drawing.Point(140, 12), Size = new System.Drawing.Size(230, 23) };

            var lblUsername = new Label { Text = "Benutzername:", Location = new System.Drawing.Point(10, 45), Size = new System.Drawing.Size(120, 23) };
            txtUsername = new TextBox { Location = new System.Drawing.Point(140, 42), Size = new System.Drawing.Size(230, 23) };

            var lblPassword = new Label { Text = "Passwort:", Location = new System.Drawing.Point(10, 75), Size = new System.Drawing.Size(120, 23) };
            txtPassword = new TextBox { Location = new System.Drawing.Point(140, 72), Size = new System.Drawing.Size(230, 23), UseSystemPasswordChar = true };

            var lblTargetIP = new Label { Text = "Ziel-IP:", Location = new System.Drawing.Point(10, 105), Size = new System.Drawing.Size(120, 23) };
            txtTargetIP = new TextBox { Location = new System.Drawing.Point(140, 102), Size = new System.Drawing.Size(230, 23) };

            var lblDuration = new Label { Text = "Dauer (Minuten):", Location = new System.Drawing.Point(10, 135), Size = new System.Drawing.Size(120, 23) };
            numDuration = new NumericUpDown
            {
                Location = new System.Drawing.Point(140, 132),
                Size = new System.Drawing.Size(100, 23),
                Minimum = 1,
                Maximum = 1440,
                Value = 60
            };

            chkAutostart = new CheckBox
            {
                Text = "Mit Windows starten (Autostart)",
                Location = new System.Drawing.Point(10, 165),
                Size = new System.Drawing.Size(250, 23),
                CheckAlign = System.Drawing.ContentAlignment.MiddleLeft
            };

            btnTest = new Button
            {
                Text = "Verbindung testen",
                Location = new System.Drawing.Point(10, 210),
                Size = new System.Drawing.Size(120, 30)
            };
            btnTest.Click += BtnTest_Click;

            btnOK = new Button
            {
                Text = "OK",
                Location = new System.Drawing.Point(210, 260),
                Size = new System.Drawing.Size(75, 30),
                DialogResult = DialogResult.OK
            };
            btnOK.Click += BtnOK_Click;

            btnCancel = new Button
            {
                Text = "Abbrechen",
                Location = new System.Drawing.Point(295, 260),
                Size = new System.Drawing.Size(75, 30),
                DialogResult = DialogResult.Cancel
            };

            this.Controls.AddRange(new Control[]
            {
                lblHost, txtAdGuardHost,
                lblUsername, txtUsername,
                lblPassword, txtPassword,
                lblTargetIP, txtTargetIP,
                lblDuration, numDuration,
                chkAutostart,
                btnTest, btnOK, btnCancel
            });

            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
        }
        private void LoadConfiguration()
        {
            txtAdGuardHost!.Text = Configuration.AdGuardHost;
            txtUsername!.Text = Configuration.Username;
            txtPassword!.Text = Configuration.Password;
            txtTargetIP!.Text = Configuration.TargetClientIP;
            numDuration!.Value = Configuration.DurationMinutes;
            chkAutostart!.Checked = Configuration.AutostartEnabled;
        }
        private void BtnOK_Click(object? sender, EventArgs e)
        {
            Configuration = new AdGuardConfig
            {
                AdGuardHost = txtAdGuardHost!.Text.Trim(),
                Username = txtUsername!.Text.Trim(),
                Password = txtPassword!.Text,
                TargetClientIP = txtTargetIP!.Text.Trim(),
                DurationMinutes = (int)numDuration!.Value,
                AutostartEnabled = chkAutostart!.Checked
            };
        }
        private async void BtnTest_Click(object? sender, EventArgs e)
        {
            try
            {
                btnTest!.Enabled = false;
                btnTest.Text = "Teste...";

                using var httpClient = new System.Net.Http.HttpClient();
                var credentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{txtUsername!.Text}:{txtPassword!.Text}"));
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials}");

                var host = txtAdGuardHost!.Text.Trim().TrimEnd('/');
                var testUrl = $"{host}/control/status";

                var response = await httpClient.GetAsync(testUrl);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Verbindung erfolgreich!", "Test erfolgreich",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"Verbindung fehlgeschlagen: {response.StatusCode}", "Test fehlgeschlagen",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Verbindungsfehler: {ex.Message}", "Test fehlgeschlagen",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnTest!.Enabled = true;
                btnTest.Text = "Verbindung testen";
            }
        }
    }
}
