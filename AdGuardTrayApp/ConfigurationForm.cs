using System;
using System.Windows.Forms;

namespace AdGuardTrayApp
{    public partial class ConfigurationForm : Form
    {
        public AdGuardConfig Configuration { get; private set; }

        public ConfigurationForm(AdGuardConfig config)
        {
            Configuration = config;
            InitializeComponent();
            LoadConfiguration();
        }        private void LoadConfiguration()
        {
            txtAdGuardHost!.Text = Configuration.AdGuardHost;
            txtUsername!.Text = Configuration.Username;
            txtPassword!.Text = Configuration.Password;
            txtTargetIP!.Text = Configuration.TargetClientIP;
            numDuration!.Value = Configuration.DurationMinutes;
            chkAutostart!.Checked = Configuration.AutostartEnabled;
        }        private void BtnOK_Click(object? sender, EventArgs e)
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
