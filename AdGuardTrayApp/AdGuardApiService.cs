using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AdGuardTrayApp
{
    public class ClientSearchResult
    {
        public bool Found { get; set; }
        public string ClientName { get; set; } = "";
        public ClientBackup? BackupData { get; set; }
        public string[] CurrentBlockedServices { get; set; } = Array.Empty<string>();
    }

    public class AdGuardApiService
    {
        private readonly HttpClient httpClient;
        private readonly AdGuardConfig config;

        public AdGuardApiService(HttpClient httpClient, AdGuardConfig config)
        {
            this.httpClient = httpClient;
            this.config = config;
        }
        public async Task UpdateClientAsync(string clientName, string clientIP, string[] blockedServices,
            bool filteringEnabled = false, bool parentalEnabled = false, bool safebrowsingEnabled = false,
            bool safesearchEnabled = false, bool useGlobalSettings = false, bool useGlobalBlockedServices = false,
            string[]? tags = null, string[]? upstreams = null)
        {
            var url = $"{config.AdGuardHost.TrimEnd('/')}/control/clients/update";
            var updateData = new
            {
                name = clientName,
                data = new
                {
                    name = clientName,
                    ids = new[] { clientIP },
                    tags = tags ?? Array.Empty<string>(),
                    upstreams = upstreams ?? Array.Empty<string>(),
                    use_global_settings = useGlobalSettings,
                    filtering_enabled = filteringEnabled,
                    parental_enabled = parentalEnabled,
                    safebrowsing_enabled = safebrowsingEnabled,
                    safesearch_enabled = safesearchEnabled,
                    use_global_blocked_services = useGlobalBlockedServices,
                    blocked_services = blockedServices
                }
            };

            await SendJsonRequestAsync(url, updateData, HttpMethod.Post);
        }

        public async Task UpdateClientFromBackupAsync(ClientBackup backup)
        {
            await UpdateClientAsync(
                backup.Name,
                backup.Ids[0], // Assuming first ID is the primary IP
                backup.BlockedServices,
                backup.FilteringEnabled,
                backup.ParentalEnabled,
                backup.SafebrowsingEnabled,
                backup.SafesearchEnabled,
                backup.UseGlobalSettings,
                backup.UseGlobalBlockedServices,
                backup.Tags,
                backup.Upstreams
            );
        }

        public async Task DeleteClientAsync(string clientName)
        {
            var url = $"{config.AdGuardHost.TrimEnd('/')}/control/clients/delete";
            var deleteData = new { name = clientName };
            await SendJsonRequestAsync(url, deleteData, HttpMethod.Post);
        }

        public async Task<string> GetAsync(string endpoint)
        {
            var url = $"{config.AdGuardHost.TrimEnd('/')}{endpoint}";
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"GET {endpoint} fehlgeschlagen: {response.StatusCode}");
            }

            return await response.Content.ReadAsStringAsync();
        }
        private async Task SendJsonRequestAsync(string url, object data, HttpMethod method)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            // Debug-Ausgabe für detaillierte API-Analyse
            System.Diagnostics.Debug.WriteLine($"[API Debug] Sende {method} Request an: {url}");
            System.Diagnostics.Debug.WriteLine($"[API Debug] JSON Payload: {json}");

            HttpResponseMessage response = method.Method switch
            {
                "GET" => await httpClient.GetAsync(url),
                "POST" => await httpClient.PostAsync(url, content),
                "PUT" => await httpClient.PutAsync(url, content),
                "DELETE" => await httpClient.DeleteAsync(url),
                _ => throw new ArgumentException($"Unsupported HTTP method: {method}")
            };

            var responseContent = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"[API Debug] Response Status: {response.StatusCode}");
            System.Diagnostics.Debug.WriteLine($"[API Debug] Response Content: {responseContent}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"API Request fehlgeschlagen: {response.StatusCode} - {responseContent}");
            }
        }

        /// <summary>
        /// Sucht nach einem Client basierend auf der IP-Adresse und gibt alle relevanten Informationen zurück
        /// </summary>
        public async Task<ClientSearchResult> FindClientByIpAsync(string targetIp)
        {
            var result = new ClientSearchResult();
            
            try
            {
                var clientsUrl = $"{config.AdGuardHost.TrimEnd('/')}/control/clients";
                var response = await httpClient.GetAsync(clientsUrl);
                
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Fehler beim Laden der Clients: {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);

                if (doc.RootElement.TryGetProperty("clients", out var clientsArray))
                {
                    foreach (var client in clientsArray.EnumerateArray())
                    {
                        if (client.TryGetProperty("ids", out var ids))
                        {
                            foreach (var id in ids.EnumerateArray())
                            {
                                if (id.GetString() == targetIp)
                                {
                                    result.Found = true;
                                    result.ClientName = client.GetProperty("name").GetString() ?? "";
                                    result.BackupData = CreateClientBackupFromJson(client);
                                    
                                    // Lade blockierte Services
                                    if (client.TryGetProperty("blocked_services", out var blockedServicesArray))
                                    {
                                        result.CurrentBlockedServices = blockedServicesArray.EnumerateArray()
                                            .Select(s => s.GetString() ?? "")
                                            .Where(s => !string.IsNullOrEmpty(s))
                                            .ToArray();
                                    }
                                    
                                    return result;
                                }
                            }
                        }
                    }
                }

                // Client nicht gefunden - generiere Namen für neuen Client
                if (!result.Found)
                {
                    result.ClientName = $"AutoAllow_{targetIp.Replace('.', '_')}";
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler bei der Client-Suche: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Erstellt ein ClientBackup-Objekt aus JSON-Daten
        /// </summary>
        private ClientBackup CreateClientBackupFromJson(JsonElement client)
        {
            return new ClientBackup
            {
                Name = client.GetProperty("name").GetString() ?? "",
                Ids = client.GetProperty("ids").EnumerateArray().Select(x => x.GetString() ?? "").ToArray(),
                UseGlobalSettings = client.TryGetProperty("use_global_settings", out var ugs) ? ugs.GetBoolean() : true,
                FilteringEnabled = client.TryGetProperty("filtering_enabled", out var fe) ? fe.GetBoolean() : true,
                ParentalEnabled = client.TryGetProperty("parental_enabled", out var pe) ? pe.GetBoolean() : true,
                SafebrowsingEnabled = client.TryGetProperty("safebrowsing_enabled", out var sbe) ? sbe.GetBoolean() : true,
                SafesearchEnabled = client.TryGetProperty("safesearch_enabled", out var sse) ? sse.GetBoolean() : true,
                UseGlobalBlockedServices = client.TryGetProperty("use_global_blocked_services", out var ugbs) ? ugbs.GetBoolean() : true,
                BlockedServices = client.TryGetProperty("blocked_services", out var bs) ?
                    bs.EnumerateArray().Select(x => x.GetString() ?? "").ToArray() : Array.Empty<string>(),
                Tags = client.TryGetProperty("tags", out var tags) ?
                    tags.EnumerateArray().Select(x => x.GetString() ?? "").ToArray() : Array.Empty<string>(),
                Upstreams = client.TryGetProperty("upstreams", out var ups) ?
                    ups.EnumerateArray().Select(x => x.GetString() ?? "").ToArray() : Array.Empty<string>()
            };
        }

        /// <summary>
        /// Erstellt oder aktualisiert einen Client mit den angegebenen blockierten Services
        /// </summary>
        public async Task<string> CreateOrUpdateClientWithBlockedServicesAsync(string targetIp, string[] blockedServices)
        {
            var searchResult = await FindClientByIpAsync(targetIp);
            
            if (searchResult.Found)
            {
                // Bestehenden Client aktualisieren
                await UpdateClientAsync(
                    searchResult.ClientName,
                    targetIp,
                    blockedServices,
                    filteringEnabled: false,
                    parentalEnabled: false,
                    safebrowsingEnabled: false,
                    safesearchEnabled: false,
                    useGlobalSettings: false,
                    useGlobalBlockedServices: false
                );
                
                System.Diagnostics.Debug.WriteLine($"[AdGuard Debug] Client {searchResult.ClientName} aktualisiert");
                return searchResult.ClientName;
            }
            else
            {
                // Neuen Client erstellen
                await CreateClientAsync(searchResult.ClientName, targetIp, blockedServices);
                System.Diagnostics.Debug.WriteLine($"[AdGuard Debug] Neuer Client {searchResult.ClientName} erstellt");
                return searchResult.ClientName;
            }
        }

        /// <summary>
        /// Erstellt einen neuen Client
        /// </summary>
        public async Task CreateClientAsync(string clientName, string clientIP, string[] blockedServices,
            bool filteringEnabled = false, bool parentalEnabled = false, bool safebrowsingEnabled = false,
            bool safesearchEnabled = false, bool useGlobalSettings = false, bool useGlobalBlockedServices = false,
            string[]? tags = null, string[]? upstreams = null)
        {
            var url = $"{config.AdGuardHost.TrimEnd('/')}/control/clients/add";
            var addData = new
            {
                name = clientName,
                ids = new[] { clientIP },
                tags = tags ?? Array.Empty<string>(),
                upstreams = upstreams ?? Array.Empty<string>(),
                use_global_settings = useGlobalSettings,
                filtering_enabled = filteringEnabled,
                parental_enabled = parentalEnabled,
                safebrowsing_enabled = safebrowsingEnabled,
                safesearch_enabled = safesearchEnabled,
                use_global_blocked_services = useGlobalBlockedServices,
                blocked_services = blockedServices
            };

            await SendJsonRequestAsync(url, addData, HttpMethod.Post);
        }

        /// <summary>
        /// Verifiziert, ob ein Client korrekt aktualisiert wurde
        /// </summary>
        public async Task<bool> VerifyClientUpdateAsync(string clientName, string[] expectedBlockedServices)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[Verification Debug] Verifiziere Client-Update für {clientName}...");

                var searchResult = await FindClientByIpAsync(config.TargetClientIP);
                
                if (!searchResult.Found || searchResult.ClientName != clientName)
                {
                    System.Diagnostics.Debug.WriteLine($"[Verification Debug] ❌ Client '{clientName}' nicht gefunden!");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"[Verification Debug] Erwartete blocked_services: [{string.Join(", ", expectedBlockedServices)}]");
                System.Diagnostics.Debug.WriteLine($"[Verification Debug] Tatsächliche blocked_services: [{string.Join(", ", searchResult.CurrentBlockedServices)}]");

                // Vergleiche Listen
                var expectedSet = new HashSet<string>(expectedBlockedServices);
                var actualSet = new HashSet<string>(searchResult.CurrentBlockedServices);

                if (expectedSet.SetEquals(actualSet))
                {
                    System.Diagnostics.Debug.WriteLine($"[Verification Debug] ✅ Verifikation erfolgreich - Client wurde korrekt aktualisiert!");
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[Verification Debug] ❌ Verifikation fehlgeschlagen - Client wurde NICHT korrekt aktualisiert!");
                    System.Diagnostics.Debug.WriteLine($"[Verification Debug] Unterschiede gefunden:");
                    foreach (var missing in expectedSet.Except(actualSet))
                    {
                        System.Diagnostics.Debug.WriteLine($"[Verification Debug]   - Fehlt: {missing}");
                    }
                    foreach (var extra in actualSet.Except(expectedSet))
                    {
                        System.Diagnostics.Debug.WriteLine($"[Verification Debug]   - Extra: {extra}");
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Verification Debug] Fehler bei Verifikation: {ex.Message}");
                return false;
            }
        }        /// <summary>
        /// Lädt alle verfügbaren blockierbaren Services von AdGuard
        /// </summary>
        public async Task<List<BlockedService>> GetAvailableBlockedServicesAsync()
        {
            try
            {
                var url = $"{config.AdGuardHost.TrimEnd('/')}/control/blocked_services/all";
                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Fehler beim Laden der verfügbaren blockierten Dienste: {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);

                var services = new List<BlockedService>();
                
                // AdGuard API gibt ein Objekt mit "blocked_services" Array zurück
                if (doc.RootElement.TryGetProperty("blocked_services", out var servicesArray))
                {
                    foreach (var service in servicesArray.EnumerateArray())
                    {
                        if (service.TryGetProperty("id", out var id) && 
                            service.TryGetProperty("name", out var name))
                        {
                            services.Add(new BlockedService
                            {
                                Id = id.GetString() ?? "",
                                Name = name.GetString() ?? "",
                                IconSvg = service.TryGetProperty("icon_svg", out var icon) ? icon.GetString() ?? "" : ""
                            });
                        }
                    }
                }

                return services.OrderBy(s => s.Name).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Laden der verfügbaren Services: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Lädt alle benutzerdefinierten Filterregeln
        /// </summary>
        public async Task<List<CustomFilterRule>> GetCustomFilterRulesAsync()
        {
            try
            {
                var url = $"{config.AdGuardHost.TrimEnd('/')}/control/filtering/status";
                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Fehler beim Laden der Filterregeln: {response.StatusCode}");
                }                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[FilterRules Debug] API Response: {content}");
                using var doc = JsonDocument.Parse(content);

                var rules = new List<CustomFilterRule>();
                if (doc.RootElement.TryGetProperty("user_rules", out var rulesArray))
                {
                    System.Diagnostics.Debug.WriteLine($"[FilterRules Debug] Gefundene user_rules: {rulesArray.GetArrayLength()}");
                    
                    foreach (var rule in rulesArray.EnumerateArray())
                    {
                        var ruleText = rule.GetString();                        if (!string.IsNullOrEmpty(ruleText))
                        {
                            System.Diagnostics.Debug.WriteLine($"[FilterRules Debug] Prüfe Regel: {ruleText}");
                            
                            var isAppManaged = ruleText.Contains("#ADGUARD_TRAY_APP") || ruleText.Contains("! ADGUARD_TRAY_APP");
                            System.Diagnostics.Debug.WriteLine($"[FilterRules Debug] App-verwaltet: {isAppManaged}");
                            
                            rules.Add(new CustomFilterRule
                            {
                                Rule = ruleText,
                                IsEnabled = !ruleText.TrimStart().StartsWith("!"),
                                IsAppManaged = isAppManaged
                            });
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[FilterRules Debug] Keine 'user_rules' Property gefunden!");
                }

                return rules;
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Laden der Filterregeln: {ex.Message}", ex);
            }
        }
    }
}
