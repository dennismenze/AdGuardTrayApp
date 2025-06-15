using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdGuardTrayApp
{
    public partial class ServiceSelectionForm : Form
    {        private readonly HttpClient httpClient;
        private readonly AdGuardConfig config;
        private readonly AdGuardApiService apiService;
        private List<BlockedService> allServices = new List<BlockedService>();
        private List<CustomFilterRule> customRules = new List<CustomFilterRule>();
        private List<string> currentlyBlockedServices = new List<string>();        private string targetClientName = ""; // Name des Ziel-Clients

        public ServiceSelectionForm(HttpClient httpClient, AdGuardConfig config)
        {
            this.httpClient = httpClient;
            this.config = config;
            this.apiService = new AdGuardApiService(httpClient, config);
            InitializeComponent();
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                saveButton.Enabled = false;
                saveButton.Text = "Laden...";

                // Lade verfügbare Dienste
                await LoadAvailableServices();

                // Lade aktuell blockierte Dienste
                await LoadCurrentlyBlockedServices();

                // Lade benutzerdefinierte Filterregeln
                await LoadCustomFilterRules();

                // UI aktualisieren
                PopulateServicesListBox();
                PopulateCustomRulesListBox();

                saveButton.Enabled = true;
                saveButton.Text = "Speichern und Anwenden";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der Daten: {ex.Message}", "Fehler",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }        private async Task LoadAvailableServices()
        {
            // Verwende zentralisierte API-Service-Methode
            allServices = await apiService.GetAvailableBlockedServicesAsync();
        }
        
        private async Task LoadCurrentlyBlockedServices()
        {
            // Verwende die zentralisierte Client-Suche aus AdGuardApiService
            var searchResult = await apiService.FindClientByIpAsync(config.TargetClientIP);
            
            targetClientName = searchResult.ClientName;
            currentlyBlockedServices.Clear();
            currentlyBlockedServices.AddRange(searchResult.CurrentBlockedServices);
        }        private async Task LoadCustomFilterRules()
        {
            // Verwende zentralisierte API-Service-Methode
            var allRules = await apiService.GetCustomFilterRulesAsync();
            System.Diagnostics.Debug.WriteLine($"[ServiceSelection Debug] Alle geladenen Regeln: {allRules.Count}");
              // Filtere nur App-verwaltete Regeln (bereits in API-Service vormarkiert)
            customRules = allRules.Where(rule => rule.IsAppManaged).ToList();
                
            System.Diagnostics.Debug.WriteLine($"[ServiceSelection Debug] App-verwaltete Regeln gefiltert: {customRules.Count}");
            foreach (var rule in customRules)
            {
                System.Diagnostics.Debug.WriteLine($"[ServiceSelection Debug] - {rule.Rule}");
            }
        }
        private void PopulateServicesListBox()
        {
            servicesListBox.Items.Clear();

            foreach (var service in allServices.OrderBy(s => s.Name))
            {
                // Emoji-Icons für bessere Darstellung
                var emoji = GetServiceEmoji(service.Id);
                var isBlocked = currentlyBlockedServices.Contains(service.Id);
                var displayText = $"{emoji} {service.Name}";
                var index = servicesListBox.Items.Add(displayText);

                // Markiere als ausgewählt, wenn der Dienst aktuell blockiert ist
                servicesListBox.SetItemChecked(index, isBlocked);
            }
        }

        private string GetServiceEmoji(string serviceId)
        {
            return serviceId.ToLower() switch
            {
                "youtube" => "🎥",
                "netflix" => "🎬",
                "facebook" => "📘",
                "instagram" => "📷",
                "twitter" or "x" => "🐦",
                "tiktok" => "🎵",
                "snapchat" => "👻",
                "discord" => "🎮",
                "twitch" => "🎪",
                "reddit" => "🤖",
                "pinterest" => "📌",
                "linkedin" => "💼",
                "amazon" => "📦",
                "ebay" => "🛒",
                "spotify" => "🎧",
                "apple_music" => "🍎",
                "skype" => "📞",
                "telegram" => "✈️",
                "whatsapp" => "💬",
                "viber" => "📱",
                "zoom" => "🔍",
                "teams" => "👥",
                "steam" => "🎮",
                "origin" => "🎯",
                "epic_games" => "🏆",
                "battle_net" => "⚔️",
                "pornhub" or "xvideos" or "xnxx" => "🔞",
                "gambling" or "bet365" or "pokerstars" => "🎰",
                "online_gaming" => "🎮",
                "dating" or "tinder" or "bumble" => "💘",
                "torrent" or "bittorrent" => "⬇️",
                _ => "🌐"
            };
        }        
        
        private void PopulateCustomRulesListBox()
        {
            System.Diagnostics.Debug.WriteLine($"[PopulateRules Debug] Starte mit {customRules.Count} Regeln");
            customRulesListBox.Items.Clear();

            foreach (var rule in customRules)
            {
                System.Diagnostics.Debug.WriteLine($"[PopulateRules Debug] Verarbeite Regel: {rule.Rule}");
                
                // Bestimme den Status der Regel
                var status = rule.IsEnabled ? "✅ Aktiv" : "⏸️ Deaktiviert";

                // Kürze die Regel für bessere Darstellung
                var ruleText = rule.Rule.Length > 60 ?
                    rule.Rule.Substring(0, 60) + "..." :
                    rule.Rule;

                var displayText = $"{status}: {ruleText}";
                var index = customRulesListBox.Items.Add(displayText);
                
                System.Diagnostics.Debug.WriteLine($"[PopulateRules Debug] Item hinzugefügt: {displayText}");

                // Markiere als ausgewählt, wenn die Regel aktiv ist (und deaktiviert werden soll)
                customRulesListBox.SetItemChecked(index, rule.IsEnabled);
            }// Hinweis anzeigen, wenn keine App-verwalteten Regeln gefunden wurden
            if (customRules.Count == 0)
            {
                customRulesListBox.Items.Add("ℹ️ Keine verwalteten Filterregeln gefunden.");
                customRulesListBox.Items.Add("");
                customRulesListBox.Items.Add("💡 So erstellen Sie verwaltete Filterregeln:");
                customRulesListBox.Items.Add("   1. AdGuard Home öffnen → Filter → Benutzerdefinierte Regeln");
                customRulesListBox.Items.Add("   2. Regeln mit '#ADGUARD_TRAY_APP' markieren");
                customRulesListBox.Items.Add("   3. Beispiele:");
                customRulesListBox.Items.Add("      ||facebook.com^#ADGUARD_TRAY_APP");
                customRulesListBox.Items.Add("      ||youtube.com^#ADGUARD_TRAY_APP");
                customRulesListBox.Items.Add("      @@||work-site.com^#ADGUARD_TRAY_APP");
            }
        }

        private async void SaveButton_Click(object? sender, EventArgs e)
        {
            try
            {
                saveButton.Enabled = false;
                saveButton.Text = "Wird gespeichert...";

                // Dienste entsperren (aus blocked_services entfernen)
                await UpdateBlockedServices();

                // Filterregeln deaktivieren
                await UpdateCustomFilterRules();

                MessageBox.Show("Einstellungen wurden erfolgreich angewendet!", "Erfolg",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Speichern: {ex.Message}", "Fehler",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                saveButton.Enabled = true;
                saveButton.Text = "Speichern und Anwenden";
            }
        }        private async Task UpdateBlockedServices()
        {
            var servicesToKeepBlocked = new List<string>();

            // Sammle alle ausgewählten Dienste, die blockiert bleiben sollen
            for (int i = 0; i < servicesListBox.Items.Count && i < allServices.Count; i++)
            {
                if (servicesListBox.GetItemChecked(i))
                {
                    var service = allServices.OrderBy(s => s.Name).ElementAt(i);
                    servicesToKeepBlocked.Add(service.Id);
                }
            }

            // Debug-Ausgabe
            System.Diagnostics.Debug.WriteLine($"[ServiceSelection Debug] Sende Client-Update für {targetClientName}:");
            System.Diagnostics.Debug.WriteLine($"[ServiceSelection Debug] Services to keep blocked: {string.Join(", ", servicesToKeepBlocked)}");
            System.Diagnostics.Debug.WriteLine($"[ServiceSelection Debug] New blocked services: {string.Join(", ", servicesToKeepBlocked)}");

            // Verwende zentralisierte API-Service-Methode
            var resultClientName = await apiService.CreateOrUpdateClientWithBlockedServicesAsync(
                config.TargetClientIP,
                servicesToKeepBlocked.ToArray()
            );

            // Aktualisiere den Client-Namen falls ein neuer Client erstellt wurde
            targetClientName = resultClientName;            // Verifikation: Prüfe, ob die Änderungen tatsächlich angewendet wurden
            var verificationSuccess = await apiService.VerifyClientUpdateAsync(targetClientName, servicesToKeepBlocked.ToArray());
            
            if (!verificationSuccess)
            {
                throw new Exception("Client-Update konnte nicht verifiziert werden. Möglicherweise wurden die Änderungen nicht korrekt angewendet.");
            }
        }        private async Task UpdateCustomFilterRules()
        {
            if (customRules.Count == 0)
                return;

            var rulesToDisable = new List<string>();

            // Sammle alle ausgewählten Regeln, die deaktiviert werden sollen
            for (int i = 0; i < customRulesListBox.Items.Count && i < customRules.Count; i++)
            {
                if (customRulesListBox.GetItemChecked(i))
                {
                    var rule = customRules[i];
                    rulesToDisable.Add(rule.Rule);
                }
            }

            if (rulesToDisable.Count == 0)
                return;

            // Lade alle aktuellen benutzerdefinierten Regeln
            var statusUrl = $"{config.AdGuardHost.TrimEnd('/')}/control/filtering/status";
            var statusResponse = await httpClient.GetAsync(statusUrl);
            var statusContent = await statusResponse.Content.ReadAsStringAsync();

            using var statusDoc = JsonDocument.Parse(statusContent);
            var allRules = new List<string>();

            if (statusDoc.RootElement.TryGetProperty("user_rules", out var rulesArray))
            {
                foreach (var ruleElement in rulesArray.EnumerateArray())
                {
                    var rule = ruleElement.GetString();
                    if (!string.IsNullOrEmpty(rule))
                    {
                        // Wenn die Regel deaktiviert werden soll, kommentiere sie aus
                        if (rulesToDisable.Any(r => r.Trim() == rule.Trim()))
                        {
                            // Regel ist nicht bereits auskommentiert
                            if (!rule.TrimStart().StartsWith("!"))
                            {
                                allRules.Add($"! {rule} ! Deaktiviert durch AdGuard Tray App");
                            }
                            else
                            {
                                allRules.Add(rule); // Bereits auskommentiert
                            }
                        }
                        else
                        {
                            allRules.Add(rule);
                        }
                    }
                }
            }

            // Aktualisiere die benutzerdefinierten Regeln
            var rulesUrl = $"{config.AdGuardHost.TrimEnd('/')}/control/filtering/set_rules";
            var rulesData = new { rules = allRules };
            var rulesJson = JsonSerializer.Serialize(rulesData);
            var rulesContent = new StringContent(rulesJson, Encoding.UTF8);
            rulesContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var rulesResponse = await httpClient.PostAsync(rulesUrl, rulesContent);

            if (!rulesResponse.IsSuccessStatusCode)
            {
                var errorContent = await rulesResponse.Content.ReadAsStringAsync();
                throw new Exception($"Fehler beim Aktualisieren der Filterregeln: {rulesResponse.StatusCode} - {errorContent}");
            }
        }

        private void SelectAllServicesButton_Click(object? sender, EventArgs e)
        {
            for (int i = 0; i < servicesListBox.Items.Count; i++)
            {
                servicesListBox.SetItemChecked(i, true);
            }
        }

        private void DeselectAllServicesButton_Click(object? sender, EventArgs e)
        {
            for (int i = 0; i < servicesListBox.Items.Count; i++)
            {
                servicesListBox.SetItemChecked(i, false);
            }
        }

        private void SelectAllRulesButton_Click(object? sender, EventArgs e)
        {
            for (int i = 0; i < customRulesListBox.Items.Count; i++)
            {
                customRulesListBox.SetItemChecked(i, true);
            }
        }

        private void DeselectAllRulesButton_Click(object? sender, EventArgs e)
        {
            for (int i = 0; i < customRulesListBox.Items.Count; i++)
            {
                customRulesListBox.SetItemChecked(i, false);
            }        }
    }

    public class BlockedService
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string IconSvg { get; set; } = "";
    }

    public class CustomFilterRule
    {
        public string Rule { get; set; } = "";
        public bool IsEnabled { get; set; } = true;
        public bool IsAppManaged { get; set; } = false;
    }
}
