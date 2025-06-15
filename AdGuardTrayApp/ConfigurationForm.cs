using System;
using System.Windows.Forms;
using System.Net;
using System.Net.NetworkInformation;
using System.Linq;

namespace AdGuardTrayApp
{
    public partial class ConfigurationForm : Form
    {
        public AdGuardConfig Configuration { get; private set; }
        private ToolTip? toolTip;

        public ConfigurationForm(AdGuardConfig config)
        {
            Configuration = config;
            InitializeComponent();
            InitializeTooltips();
            LoadConfiguration();
        }

        /// <summary>
        /// Initialisiert die ToolTips für die Benutzeroberfläche
        /// </summary>
        private void InitializeTooltips()
        {
            toolTip = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 1000,
                ReshowDelay = 500,
                ShowAlways = true
            };

            // ToolTips für die verschiedenen Felder
            toolTip.SetToolTip(txtAdGuardHost, "Die URL Ihres AdGuard Home Servers (z.B. http://192.168.1.1:3000)");
            toolTip.SetToolTip(txtUsername, "Ihr AdGuard Home Benutzername");
            toolTip.SetToolTip(txtPassword, "Ihr AdGuard Home Passwort");
            toolTip.SetToolTip(txtTargetIP, "Die IP-Adresse dieses PCs, die entsperrt werden soll");
            toolTip.SetToolTip(btnDetectIP, "Automatische Erkennung der lokalen IP-Adresse");
            toolTip.SetToolTip(numDuration, "Dauer in Minuten, für die die Entsperrung aktiv bleibt");
            toolTip.SetToolTip(chkAutostart, "Anwendung automatisch mit Windows starten");
        }

        private void LoadConfiguration()
        {
            txtAdGuardHost!.Text = Configuration.AdGuardHost;
            txtUsername!.Text = Configuration.Username;
            txtPassword!.Text = Configuration.Password;
            
            // Auto-detect local IP if TargetClientIP is empty
            if (string.IsNullOrEmpty(Configuration.TargetClientIP))
            {
                Configuration.TargetClientIP = GetLocalNetworkIP();
            }
            
            txtTargetIP!.Text = Configuration.TargetClientIP;
            numDuration!.Value = Configuration.DurationMinutes;
            chkAutostart!.Checked = Configuration.AutostartEnabled;
        }

        /// <summary>
        /// Ermittelt die lokale Netzwerk-IP-Adresse des PCs
        /// </summary>
        /// <returns>Die lokale IP-Adresse im Format xxx.xxx.xxx.xxx</returns>
        private string GetLocalNetworkIP()
        {
            try
            {
                // Alle aktiven Netzwerk-Interfaces abrufen
                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.OperationalStatus == OperationalStatus.Up && 
                                ni.NetworkInterfaceType != NetworkInterfaceType.Loopback);

                foreach (var networkInterface in networkInterfaces)
                {
                    var ipProperties = networkInterface.GetIPProperties();
                    
                    foreach (var unicastAddress in ipProperties.UnicastAddresses)
                    {
                        var ip = unicastAddress.Address;
                        
                        // IPv4-Adresse im privaten Netzwerkbereich suchen
                        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
                            !IPAddress.IsLoopback(ip))
                        {
                            var ipBytes = ip.GetAddressBytes();
                            
                            // Prüfung auf private IP-Bereiche:
                            // 192.168.x.x (Klasse C)
                            // 10.x.x.x (Klasse A)  
                            // 172.16.x.x - 172.31.x.x (Klasse B)
                            if ((ipBytes[0] == 192 && ipBytes[1] == 168) ||
                                (ipBytes[0] == 10) ||
                                (ipBytes[0] == 172 && ipBytes[1] >= 16 && ipBytes[1] <= 31))
                            {
                                return ip.ToString();
                            }
                        }
                    }
                }

                // Fallback: Erste verfügbare IPv4-Adresse
                var hostEntry = Dns.GetHostEntry(Dns.GetHostName());
                var localIP = hostEntry.AddressList
                    .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && 
                                         !IPAddress.IsLoopback(ip));

                return localIP?.ToString() ?? "192.168.1.100"; // Default-Fallback
            }
            catch (Exception)
            {
                // Bei Fehler Standard-IP zurückgeben
                return "192.168.1.100";
            }
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

        /// <summary>
        /// Event-Handler für den IP-Erkennungs-Button
        /// </summary>
        private void BtnDetectIP_Click(object? sender, EventArgs e)
        {
            try
            {
                var detectedIP = GetLocalNetworkIP();
                txtTargetIP!.Text = detectedIP;
                  // Tooltip anzeigen
                if (toolTip != null)
                {
                    toolTip.Show($"IP-Adresse erkannt: {detectedIP}", btnDetectIP!, 2000);
                }
            }
            catch (Exception ex)
            {                MessageBox.Show($"Fehler beim Erkennen der IP-Adresse: {ex.Message}", "Fehler",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
