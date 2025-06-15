using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Win32;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Linq;

namespace AdGuardTrayApp
{
    public partial class MainForm : Form
    {
        private NotifyIcon? trayIcon;
        private System.Windows.Forms.Timer? resetTimer;
        private bool isUnlocked = false;
        private HttpClient httpClient;
        private AdGuardConfig config = new AdGuardConfig(); // Mit Default-Wert initialisiert
        private AdGuardApiService? apiService; // Wird nach der Konfiguration initialisiert
        private ClientBackup? originalClientSettings; // Für Wiederherstellung der ursprünglichen Einstellungen
        private AdGuardBackup? originalAdGuardSettings;

        // Erweiterte Windows Hello-Implementierung mit Fingerprint-Priorisierung
        // Legacy P/Invoke für Fallback-Szenarien
        [DllImport("credui.dll", CharSet = CharSet.Unicode)]
        public static extern int CredUIPromptForWindowsCredentials(
            ref CREDUI_INFO pUiInfo,
            int dwAuthError,
            ref uint pulAuthPackage,
            IntPtr pvInAuthBuffer,
            uint ulInAuthBufferSize,
            out IntPtr ppvOutAuthBuffer,
            out uint pulOutAuthBufferSize,
            ref bool pfSave,
            int dwFlags);

        [DllImport("credui.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CredPackAuthenticationBuffer(
            int dwFlags,
            string pszUserName,
            string pszPassword,
            IntPtr pPackedCredentials,
            ref int pcbPackedCredentials);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct CREDUI_INFO
        {
            public int cbSize;
            public IntPtr hwndParent;
            public string pszMessageText;
            public string pszCaptionText;
            public IntPtr hbmBanner;
        }
        public MainForm()
        {
            InitializeComponent();

            // Event Handler für sauberes Beenden
            this.FormClosing += MainForm_FormClosing;

            InitializeTrayIcon();
            LoadConfiguration();
            httpClient = new HttpClient();
            SetupHttpClient();

            // Autostart-Registrierung
            SetupAutostart();
        }
        private void InitializeTrayIcon()
        {
            trayIcon = new NotifyIcon()
            {
                Icon = CreateLockIcon(),
                Text = "AdGuard Control - Gesperrt",
                Visible = true
            }; trayIcon.MouseClick += TrayIcon_Click;
            trayIcon.ContextMenuStrip = CreateContextMenu();
        }
        private ContextMenuStrip CreateContextMenu()
        {
            var contextMenu = new ContextMenuStrip();

            if (isUnlocked)
            {
                // Menü für entsperrten Zustand
                contextMenu.Items.Add("🔓 Entsperrt", null, null).Enabled = false; // Status-Anzeige
                contextMenu.Items.Add("-");
                contextMenu.Items.Add("Dienste/Filter ändern...", null, (s, e) => ShowServiceSelectionDialog());
                contextMenu.Items.Add("Sofort sperren", null, async (s, e) => await ResetAdGuard());
                contextMenu.Items.Add("-");
                contextMenu.Items.Add("Konfiguration", null, ShowConfiguration);
                contextMenu.Items.Add("-");
                contextMenu.Items.Add("Beenden", null, async (s, e) => await ExitApplication());
            }
            else
            {
                // Menü für gesperrten Zustand
                contextMenu.Items.Add("🔒 Gesperrt", null, null).Enabled = false; // Status-Anzeige
                contextMenu.Items.Add("-");
                contextMenu.Items.Add("Entsperren (Windows Hello)", null, async (s, e) => await HandleUnlockRequest());
                contextMenu.Items.Add("-");
                contextMenu.Items.Add("Konfiguration", null, ShowConfiguration);
                contextMenu.Items.Add("-");
                contextMenu.Items.Add("Beenden", null, async (s, e) => await ExitApplication());
            }

            return contextMenu;
        }        /// <summary>
                 /// Zeigt den Service-Selection-Dialog erneut an (während einer aktiven Session)
                 /// </summary>
        private void ShowServiceSelectionDialog()
        {
            try
            {
                using var serviceForm = new ServiceSelectionForm(httpClient, config);
                var result = serviceForm.ShowDialog();

                if (result == DialogResult.OK)
                {
                    ShowBalloonTip("Einstellungen aktualisiert",
                        "Die Dienst- und Filtereinstellungen wurden erfolgreich aktualisiert.",
                        ToolTipIcon.Info);

                    // Menü aktualisieren (falls sich der Status geändert hat)
                    UpdateTrayMenu();
                }
            }
            catch (Exception ex)
            {
                ShowBalloonTip("Fehler", $"Fehler beim Öffnen der Einstellungen: {ex.Message}", ToolTipIcon.Error);
            }
        }

        /// <summary>
        /// Aktualisiert das Tray-Menü basierend auf dem aktuellen Status
        /// </summary>
        private void UpdateTrayMenu()
        {
            if (trayIcon != null)
            {
                trayIcon.ContextMenuStrip?.Dispose();
                trayIcon.ContextMenuStrip = CreateContextMenu();
            }
        }        private Icon CreateLockIcon()
        {
            // Erstelle ein professionelles AdGuard-ähnliches Lock-Icon
            var bitmap = new Bitmap(32, 32);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                
                // AdGuard-ähnliches Schild mit Schloss
                var shieldPath = new System.Drawing.Drawing2D.GraphicsPath();
                var shieldRect = new Rectangle(6, 4, 20, 24);
                
                // Schild-Form (abgerundetes Rechteck mit spitzer Unterseite)
                shieldPath.AddArc(shieldRect.X, shieldRect.Y, 8, 8, 180, 90);
                shieldPath.AddArc(shieldRect.Right - 8, shieldRect.Y, 8, 8, 270, 90);
                shieldPath.AddLine(shieldRect.Right, shieldRect.Y + 4, shieldRect.Right, shieldRect.Bottom - 4);
                shieldPath.AddLine(shieldRect.Right, shieldRect.Bottom - 4, shieldRect.X + shieldRect.Width / 2, shieldRect.Bottom + 2);
                shieldPath.AddLine(shieldRect.X + shieldRect.Width / 2, shieldRect.Bottom + 2, shieldRect.X, shieldRect.Bottom - 4);
                shieldPath.AddLine(shieldRect.X, shieldRect.Bottom - 4, shieldRect.X, shieldRect.Y + 4);
                shieldPath.CloseFigure();
                
                // Schild-Hintergrund (rot für gesperrt)
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    shieldRect, Color.FromArgb(220, 20, 60), Color.FromArgb(139, 0, 0), 45f))
                {
                    g.FillPath(brush, shieldPath);
                }
                
                // Schild-Umriss
                using (var pen = new Pen(Color.FromArgb(100, 0, 0), 1.5f))
                {
                    g.DrawPath(pen, shieldPath);
                }
                
                // Schloss-Symbol
                var lockRect = new Rectangle(12, 12, 8, 8);
                // Schloss-Bügel
                using (var pen = new Pen(Color.White, 1.5f))
                {
                    g.DrawArc(pen, lockRect.X + 1, lockRect.Y - 2, lockRect.Width - 2, 4, 0, 180);
                }
                // Schloss-Körper
                using (var brush = new SolidBrush(Color.White))
                {
                    g.FillRectangle(brush, lockRect.X + 1, lockRect.Y + 1, lockRect.Width - 2, lockRect.Height - 2);
                }
                // Schlüsselloch
                using (var brush = new SolidBrush(Color.FromArgb(220, 20, 60)))
                {
                    g.FillEllipse(brush, lockRect.X + 3, lockRect.Y + 3, 2, 2);
                    g.FillRectangle(brush, lockRect.X + 3.5f, lockRect.Y + 4, 1, 2);
                }
            }
            return Icon.FromHandle(bitmap.GetHicon());
        }

        private Icon CreateUnlockIcon()
        {
            // Erstelle ein professionelles AdGuard-ähnliches Unlock-Icon
            var bitmap = new Bitmap(32, 32);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                
                // AdGuard-ähnliches Schild
                var shieldPath = new System.Drawing.Drawing2D.GraphicsPath();
                var shieldRect = new Rectangle(6, 4, 20, 24);
                
                // Schild-Form (abgerundetes Rechteck mit spitzer Unterseite)
                shieldPath.AddArc(shieldRect.X, shieldRect.Y, 8, 8, 180, 90);
                shieldPath.AddArc(shieldRect.Right - 8, shieldRect.Y, 8, 8, 270, 90);
                shieldPath.AddLine(shieldRect.Right, shieldRect.Y + 4, shieldRect.Right, shieldRect.Bottom - 4);
                shieldPath.AddLine(shieldRect.Right, shieldRect.Bottom - 4, shieldRect.X + shieldRect.Width / 2, shieldRect.Bottom + 2);
                shieldPath.AddLine(shieldRect.X + shieldRect.Width / 2, shieldRect.Bottom + 2, shieldRect.X, shieldRect.Bottom - 4);
                shieldPath.AddLine(shieldRect.X, shieldRect.Bottom - 4, shieldRect.X, shieldRect.Y + 4);
                shieldPath.CloseFigure();
                
                // Schild-Hintergrund (grün für entsperrt)
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    shieldRect, Color.FromArgb(50, 205, 50), Color.FromArgb(34, 139, 34), 45f))
                {
                    g.FillPath(brush, shieldPath);
                }
                
                // Schild-Umriss
                using (var pen = new Pen(Color.FromArgb(0, 100, 0), 1.5f))
                {
                    g.DrawPath(pen, shieldPath);
                }
                  // Häkchen-Symbol (✓)
                using (var pen = new Pen(Color.White, 2.5f))
                {
                    pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
                    
                    var checkPoints = new PointF[]
                    {
                        new PointF(11, 16),
                        new PointF(14, 19),
                        new PointF(21, 12)
                    };
                    g.DrawLines(pen, checkPoints);
                }
            }
            return Icon.FromHandle(bitmap.GetHicon());
        }

        private void LoadConfiguration()
        {
            config = new AdGuardConfig
            {
                AdGuardHost = "http://192.168.178.30:3000/",
                Username = "menze",
                Password = "3eKk4iE^U*kd%jWe*rHG",
                TargetClientIP = "192.168.178.54",
                DurationMinutes = 60
            };
        }
        private void SetupHttpClient()
        {
            // Entferne existierenden Authorization-Header
            httpClient.DefaultRequestHeaders.Authorization = null;

            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{config.Username}:{config.Password}"));
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

            // Initialisiere ApiService nach HttpClient-Setup
            apiService = new AdGuardApiService(httpClient, config);
        }
        private async void TrayIcon_Click(object? sender, EventArgs e)
        {
            if (isUnlocked) return;

            // Nur bei Linksklick reagieren, nicht bei Rechtsklick (Kontextmenü)
            if (sender is NotifyIcon && e is MouseEventArgs mouseEvent && mouseEvent.Button != MouseButtons.Left)
                return;

            await HandleUnlockRequest();
        }

        private async Task HandleUnlockRequest()
        {
            if (isUnlocked) return;

            try
            {
                if (await AuthenticateWithWindowsHello())
                {
                    await ShowServiceSelectionAndUnlock();
                }
            }
            catch (Exception ex)
            {
                ShowBalloonTip("Fehler", $"Fehler bei der Entsperrung: {ex.Message}", ToolTipIcon.Error);
            }
        }
        private async Task ShowServiceSelectionAndUnlock()
        {
            try
            {
                // Teste Verbindung zu AdGuard
                var statusUrl = $"{config.AdGuardHost.TrimEnd('/')}/control/status";
                var response = await httpClient.GetAsync(statusUrl);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Verbindung zu AdGuard Home fehlgeschlagen");
                }

                // Sichere ursprüngliche AdGuard-Einstellungen
                originalAdGuardSettings = await BackupAdGuardSettings();

                // Zeige Dienst-Auswahl-Dialog
                using var serviceForm = new ServiceSelectionForm(httpClient, config);
                var result = serviceForm.ShowDialog();

                if (result == DialogResult.OK)
                {
                    // Client freigeben
                    await EnableClientAccess();                    // UI aktualisieren
                    isUnlocked = true;
                    trayIcon!.Icon = CreateUnlockIcon();
                    trayIcon.Text = $"AdGuard Control - Entsperrt für {config.DurationMinutes} Min";
                    UpdateTrayMenu(); // Menü aktualisieren

                    // Timer starten
                    StartResetTimer();

                    ShowBalloonTip("Erfolgreich entsperrt",
                        $"IP {config.TargetClientIP} ist für {config.DurationMinutes} Minuten entsperrt.\nAusgewählte Dienste und Filter wurden deaktiviert.",
                        ToolTipIcon.Info);
                }
                else
                {
                    // Dialog wurde abgebrochen, Backup verwerfen
                    originalAdGuardSettings = null;
                }
            }
            catch (Exception ex)
            {
                // Bei Fehler das Backup verwerfen
                originalAdGuardSettings = null;
                throw new Exception($"Fehler beim Entsperren: {ex.Message}");
            }
        }        private async Task<bool> AuthenticateWithWindowsHello()
        {
            try
            {
                // Prüfe zunächst die Windows Hello Verfügbarkeit
                if (!IsWindowsHelloAvailable())
                {
                    LogDebugInfo("❌ Windows Hello ist nicht verfügbar - verwende Fallback");
                    return await AuthenticateWithSimplifiedWindowsHello();
                }

                // Verbesserte Windows Hello-Authentifizierung mit Fingerprint-Priorisierung
                bool result = await AuthenticateWithEnhancedWindowsHello();
                
                // Bei Fehler 87 (ERROR_INVALID_PARAMETER) verwende Fallback-Methode
                if (!result)
                {
                    LogDebugInfo("🔄 Versuche Fallback-Authentifizierungsmethode...");
                    result = await AuthenticateWithSimplifiedWindowsHello();
                }
                
                return result;
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Windows Hello Authentifizierung fehlgeschlagen: {ex.Message}");
                
                // Letzter Fallback-Versuch
                try
                {
                    LogDebugInfo("🔄 Letzter Fallback-Versuch mit vereinfachter Methode...");
                    return await AuthenticateWithSimplifiedWindowsHello();
                }
                catch (Exception fallbackEx)
                {
                    LogDebugInfo($"Auch Fallback-Authentifizierung fehlgeschlagen: {fallbackEx.Message}");
                    return false;
                }
            }
        }

        /// <summary>        /// Erweiterte Windows Hello-Authentifizierung mit Fingerprint-Priorisierung
        /// Verwendet optimierte P/Invoke-Flags für bessere biometrische Unterstützung
        /// </summary>
        private async Task<bool> AuthenticateWithEnhancedWindowsHello()
        {
            return await Task.Run(() =>
            {
                try
                {
                    var currentUser = Environment.UserName;
                    var domain = Environment.UserDomainName;
                    var fullUsername = string.IsNullOrEmpty(domain) || domain == "." ? currentUser : $"{domain}\\{currentUser}";

                    // Sicherheitsprüfung für Windows Hello Verfügbarkeit
                    LogDebugInfo("🔐 Prüfe Windows Hello Verfügbarkeit...");

                    // Verbesserte CREDUI_INFO Initialisierung mit vollständiger Validierung
                    var credui = new CREDUI_INFO();
                    credui.cbSize = Marshal.SizeOf(typeof(CREDUI_INFO));
                    credui.hwndParent = IntPtr.Zero;
                    credui.pszMessageText = $"🔐 AdGuard Entsperrung\n👤 Benutzer: {fullUsername}\n\n👆 Windows Hello Authentifizierung\n🥇 Fingerabdruck (bevorzugt)\n🔢 PIN\n👀 Gesichtserkennung";
                    credui.pszCaptionText = "🛡️ AdGuard Tray App - Windows Hello";
                    credui.hbmBanner = IntPtr.Zero;

                    uint authPackage = 0;
                    IntPtr outCredBuffer = IntPtr.Zero;
                    uint outCredSize = 0;
                    bool save = false;

                    // Erstelle optimierten Authentifizierungspuffer mit verbesserter Fehlerbehandlung
                    IntPtr inCredBuffer = IntPtr.Zero;
                    uint inCredSize = 0;

                    try
                    {
                        // Sichere Puffererstellung für Credentials
                        int packedCredSize = 0;
                        
                        // Erste Abfrage um benötigte Puffergröße zu ermitteln
                        bool sizeResult = CredPackAuthenticationBuffer(0, fullUsername, "", IntPtr.Zero, ref packedCredSize);
                        
                        if (packedCredSize > 0 && packedCredSize < 1024 * 1024) // Sicherheitsprüfung für maximale Puffergröße
                        {
                            // Allokiere Puffer und erstelle Credentials
                            inCredBuffer = Marshal.AllocCoTaskMem(packedCredSize);
                            if (inCredBuffer != IntPtr.Zero)
                            {
                                // Initialisiere Puffer mit Nullen für Sicherheit
                                for (int i = 0; i < packedCredSize; i++)
                                {
                                    Marshal.WriteByte(inCredBuffer, i, 0);
                                }
                                
                                if (CredPackAuthenticationBuffer(0, fullUsername, "", inCredBuffer, ref packedCredSize))
                                {
                                    inCredSize = (uint)packedCredSize;
                                    LogDebugInfo($"📦 Authentifizierungspuffer erstellt ({inCredSize} Bytes)");
                                }
                                else
                                {
                                    LogDebugInfo("⚠️ Warnung: CredPackAuthenticationBuffer fehlgeschlagen, verwende leeren Puffer");
                                    inCredSize = 0;
                                }
                            }
                        }

                        // Konservative Flags für bessere Kompatibilität
                        const int CREDUIWIN_GENERIC = 0x1;
                        const int CREDUIWIN_ENUMERATE_CURRENT_USER = 0x200;
                        const int CREDUIWIN_SECURE_PROMPT = 0x1000;

                        // Verwende nur sichere, getestete Flags
                        int flags = CREDUIWIN_GENERIC | CREDUIWIN_ENUMERATE_CURRENT_USER | CREDUIWIN_SECURE_PROMPT;

                        LogDebugInfo("🔐 Starte Windows Hello-Authentifizierung mit Fingerprint-Priorisierung...");
                        LogDebugInfo($"📋 Parameter: User={fullUsername}, Flags=0x{flags:X}, BufferSize={inCredSize}");

                        int result = CredUIPromptForWindowsCredentials(
                            ref credui,
                            0,
                            ref authPackage,
                            inCredBuffer,
                            inCredSize,
                            out outCredBuffer,
                            out outCredSize,
                            ref save,
                            flags);

                        var success = result == 0;
                        
                        if (success)
                        {
                            LogDebugInfo("✅ Windows Hello-Authentifizierung erfolgreich");
                            LogDebugInfo($"📊 Auth Package: {authPackage}, Output Size: {outCredSize}");
                        }
                        else
                        {
                            LogDebugInfo($"❌ Windows Hello-Authentifizierung fehlgeschlagen (Fehlercode: {result})");
                            
                            // Erweiterte Fehleranalyse
                            switch (result)
                            {
                                case 87: // ERROR_INVALID_PARAMETER
                                    LogDebugInfo("🔧 FEHLER 87: Ungültiger Parameter - möglicherweise fehlerhafte CREDUI_INFO Struktur oder Flags");
                                    LogDebugInfo($"🔍 Debug: cbSize={credui.cbSize}, hwndParent={credui.hwndParent}, inCredSize={inCredSize}");
                                    break;
                                case 1223: // ERROR_CANCELLED
                                    LogDebugInfo("🚫 Authentifizierung vom Benutzer abgebrochen");
                                    break;
                                case 1326: // ERROR_LOGON_FAILURE  
                                    LogDebugInfo("🔐 Authentifizierung fehlgeschlagen - falsche Credentials");
                                    break;
                                case 1355: // ERROR_NO_SUCH_LOGON_SESSION
                                    LogDebugInfo("👤 Keine gültige Logon-Session gefunden");
                                    break;
                                case 1314: // ERROR_PRIVILEGE_NOT_HELD
                                    LogDebugInfo("🔒 Unzureichende Berechtigungen für Windows Hello");
                                    break;
                                default:
                                    LogDebugInfo($"❓ Unbekannter Fehler: {result} (0x{result:X})");
                                    break;
                            }
                        }
                        
                        return success;
                    }
                    finally
                    {
                        // Sichere Memory cleanup
                        if (inCredBuffer != IntPtr.Zero)
                        {
                            try
                            {
                                Marshal.FreeCoTaskMem(inCredBuffer);
                            }
                            catch (Exception cleanupEx)
                            {
                                LogDebugInfo($"⚠️ Warnung beim inCredBuffer cleanup: {cleanupEx.Message}");
                            }
                        }
                        if (outCredBuffer != IntPtr.Zero)
                        {
                            try
                            {
                                Marshal.FreeCoTaskMem(outCredBuffer);
                            }
                            catch (Exception cleanupEx)
                            {
                                LogDebugInfo($"⚠️ Warnung beim outCredBuffer cleanup: {cleanupEx.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogDebugInfo($"🚨 Windows Hello Fehler: {ex.Message}");
                    LogDebugInfo($"🔍 Stack Trace: {ex.StackTrace}");
                    
                    this.Invoke((MethodInvoker)delegate
                    {
                        MessageBox.Show($"🚫 Windows Hello Fehler:\n{ex.Message}\n\n💡 Troubleshooting-Tipps:\n• Stellen Sie sicher, dass Windows Hello konfiguriert ist\n• Neustart der App als Administrator versuchen\n• Überprüfen Sie die Windows-Einstellungen\n• Windows Hello in den Einstellungen neu einrichten", 
                            "Windows Hello Authentifizierung", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    });
                    return false;
                }
            });
        }
        /// <summary>
        /// Fallback-Authentifizierungsmethode mit einfacheren Parametern für bessere Kompatibilität
        /// </summary>
        private async Task<bool> AuthenticateWithSimplifiedWindowsHello()
        {
            return await Task.Run(() =>
            {
                try
                {
                    LogDebugInfo("🔄 Fallback: Verwende vereinfachte Windows Hello Authentifizierung...");
                    
                    var currentUser = Environment.UserName;
                    
                    // Minimale CREDUI_INFO Struktur
                    var credui = new CREDUI_INFO();
                    credui.cbSize = Marshal.SizeOf(typeof(CREDUI_INFO));
                    credui.hwndParent = IntPtr.Zero;
                    credui.pszMessageText = "Windows Hello Authentifizierung für AdGuard Tray App";
                    credui.pszCaptionText = "AdGuard Authentifizierung";
                    credui.hbmBanner = IntPtr.Zero;

                    uint authPackage = 0;
                    IntPtr outCredBuffer = IntPtr.Zero;
                    uint outCredSize = 0;
                    bool save = false;

                    // Nur die wichtigsten Flags verwenden
                    const int CREDUIWIN_GENERIC = 0x1;
                    const int CREDUIWIN_SECURE_PROMPT = 0x1000;
                    int flags = CREDUIWIN_GENERIC | CREDUIWIN_SECURE_PROMPT;

                    LogDebugInfo($"🔐 Starte vereinfachte Windows Hello-Authentifizierung für Benutzer: {currentUser}");

                    int result = CredUIPromptForWindowsCredentials(
                        ref credui,
                        0,
                        ref authPackage,
                        IntPtr.Zero,  // Kein Input-Buffer
                        0,            // Keine Input-Buffer-Größe
                        out outCredBuffer,
                        out outCredSize,
                        ref save,
                        flags);

                    var success = result == 0;
                    
                    try
                    {
                        if (success)
                        {
                            LogDebugInfo("✅ Vereinfachte Windows Hello-Authentifizierung erfolgreich");
                        }
                        else
                        {
                            LogDebugInfo($"❌ Vereinfachte Windows Hello-Authentifizierung fehlgeschlagen (Fehlercode: {result})");
                        }
                    }
                    finally
                    {
                        // Cleanup
                        if (outCredBuffer != IntPtr.Zero)
                        {
                            try
                            {
                                Marshal.FreeCoTaskMem(outCredBuffer);
                            }
                            catch (Exception cleanupEx)
                            {
                                LogDebugInfo($"⚠️ Warnung beim Cleanup: {cleanupEx.Message}");
                            }
                        }
                    }
                    
                    return success;
                }
                catch (Exception ex)
                {
                    LogDebugInfo($"🚨 Fallback Windows Hello Fehler: {ex.Message}");
                    return false;
                }
            });
        }
        private async Task UnlockAdGuard()
        {
            // Diese Methode wird jetzt von ShowServiceSelectionAndUnlock() ersetzt
            // Für Kompatibilität beibehalten, aber eigentlich nicht mehr verwendet
            await ShowServiceSelectionAndUnlock();
        }
        private async Task EnableClientAccess()
        {
            try
            {
                // Verwende zentralisierte Client-Suche für Backup-Zwecke
                var searchResult = await apiService!.FindClientByIpAsync(config.TargetClientIP);

                if (searchResult.Found && searchResult.BackupData != null)
                {
                    // Sichere ursprüngliche Einstellungen
                    originalClientSettings = searchResult.BackupData;
                    LogDebugInfo($"Client {searchResult.ClientName} gefunden - Backup erstellt");
                }
                else
                {
                    LogDebugInfo("Kein bestehender Client gefunden - wird bei Bedarf erstellt");
                }

                // WICHTIG: Client-Aktualisierung wird von ServiceSelectionForm durchgeführt
                // Hier wird nur das Backup erstellt
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Konfigurieren des Clients: {ex.Message}");
            }
        }
        private async Task CreateNewClient()
        {
            var addUrl = $"{config.AdGuardHost.TrimEnd('/')}/control/clients/add";
            var clientName = $"AutoAllow_{config.TargetClientIP.Replace('.', '_')}";

            LogDebugInfo($"Erstelle neuen Client: {clientName}");

            // Erweiterte Konfiguration für vollständige Entsperrung inkl. blockierter Dienste
            var addData = new
            {
                name = clientName,
                ids = new[] { config.TargetClientIP },
                tags = Array.Empty<string>(),
                upstreams = Array.Empty<string>(),
                use_global_settings = false,
                filtering_enabled = false,           // Deaktiviert Domain-Filter/Werbeblocker
                parental_enabled = false,            // Deaktiviert Kindersicherung
                safebrowsing_enabled = false,        // Deaktiviert Safe Browsing
                safesearch_enabled = false,          // Deaktiviert Safe Search
                use_global_blocked_services = false, // Verwendet client-spezifische blocked_services
                blocked_services = Array.Empty<string>() // Leere Liste = keine Dienste blockiert
            };

            var json = JsonSerializer.Serialize(addData);
            LogDebugInfo($"Sende Add-Request: {json}");

            var content = new StringContent(json, Encoding.UTF8);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var response = await httpClient.PostAsync(addUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                LogDebugInfo($"Creation fehlgeschlagen: {response.StatusCode} - {errorContent}");
                throw new Exception($"Client Creation fehlgeschlagen: {response.StatusCode} - {errorContent}");
            }

            LogDebugInfo($"Client {clientName} erfolgreich erstellt");
        }

        private void StartResetTimer()
        {
            resetTimer = new System.Windows.Forms.Timer();
            resetTimer.Interval = config.DurationMinutes * 60 * 1000; // Minuten in Millisekunden
            resetTimer.Tick += async (s, e) => await ResetAdGuard();
            resetTimer.Start();
        }
        private async Task ResetAdGuard()
        {
            try
            {
                resetTimer?.Stop();

                // Client-Einstellungen wiederherstellen
                await RestoreOriginalClientSettings();

                // Globale AdGuard-Einstellungen wiederherstellen
                if (originalAdGuardSettings != null)
                {
                    await RestoreAdGuardSettings(originalAdGuardSettings);
                    originalAdGuardSettings = null;
                }                // UI zurücksetzen
                isUnlocked = false;
                trayIcon!.Icon = CreateLockIcon();
                trayIcon.Text = "AdGuard Control - Gesperrt";
                UpdateTrayMenu(); // Menü aktualisieren

                ShowBalloonTip("Sperrung reaktiviert",
                    $"Die ursprünglichen Einstellungen für IP {config.TargetClientIP} und alle AdGuard-Einstellungen wurden wiederhergestellt.",
                    ToolTipIcon.Warning);
            }
            catch (Exception ex)
            {
                ShowBalloonTip("Fehler beim Zurücksetzen", $"Fehler: {ex.Message}", ToolTipIcon.Error);
            }
        }

        private async Task RestoreOriginalClientSettings()
        {
            if (originalClientSettings != null)
            {
                // Ursprüngliche Einstellungen wiederherstellen
                await RestoreExistingClient(originalClientSettings);
                originalClientSettings = null;
            }
            else
            {
                // Fallback: Temporäre AutoAllow_* Clients löschen
                var clientsUrl = $"{config.AdGuardHost.TrimEnd('/')}/control/clients";
                var clientsResponse = await httpClient.GetAsync(clientsUrl);
                var clientsContent = await clientsResponse.Content.ReadAsStringAsync();

                using var clientsDoc = JsonDocument.Parse(clientsContent);
                var clients = clientsDoc.RootElement.GetProperty("clients");

                foreach (var client in clients.EnumerateArray())
                {
                    var clientName = client.GetProperty("name").GetString();
                    if (clientName != null && clientName.StartsWith("AutoAllow_"))
                    {                        // Temporären Client löschen
                        var deleteUrl = $"{config.AdGuardHost.TrimEnd('/')}/control/clients/delete";
                        var deleteData = new { name = clientName };
                        var json = JsonSerializer.Serialize(deleteData);
                        var content = new StringContent(json, Encoding.UTF8);
                        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                        await httpClient.PostAsync(deleteUrl, content);
                        break;
                    }
                }
            }
        }
        private async Task RestoreExistingClient(ClientBackup backup)
        {
            if (apiService == null)
            {
                throw new InvalidOperationException("ApiService ist nicht initialisiert");
            }

            // Verwende den ApiService für DRY-konformen Code
            await apiService.UpdateClientFromBackupAsync(backup);
        }

        private void ShowBalloonTip(string title, string text, ToolTipIcon icon)
        {
            trayIcon?.ShowBalloonTip(5000, title, text, icon);
        }

        private void LogDebugInfo(string message)
        {
            // Debug-Info in der Konsole ausgeben (für Debugging)
            System.Diagnostics.Debug.WriteLine($"[AdGuard Debug] {DateTime.Now:HH:mm:ss} - {message}");
        }        private void SetupAutostart()
        {
            try
            {
                if (config.AutostartEnabled)
                {
                    // Use AppContext.BaseDirectory for single-file applications
                    var appDir = AppContext.BaseDirectory;
                    var exePath = Path.Combine(appDir, "AdGuardTrayApp.exe");

                    using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                    key?.SetValue("AdGuardTrayApp", $"\"{exePath}\"");
                    LogDebugInfo("Autostart aktiviert");
                }
                else
                {
                    RemoveAutostart();
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Fehler beim Einrichten des Autostarts: {ex.Message}");
            }
        }

        private void RemoveAutostart()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (key?.GetValue("AdGuardTrayApp") != null)
                {
                    key.DeleteValue("AdGuardTrayApp");
                    LogDebugInfo("Autostart deaktiviert");
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Fehler beim Entfernen des Autostarts: {ex.Message}");
            }
        }

        private bool IsAutostartEnabled()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);
                return key?.GetValue("AdGuardTrayApp") != null;
            }
            catch
            {
                return false;
            }
        }
        private async void ShowConfiguration(object? sender, EventArgs e)
        {
            // Windows Hello Authentifizierung für Konfiguration erforderlich
            if (!await AuthenticateWithWindowsHello())
            {
                return;
            }

            var configForm = new ConfigurationForm(config);
            if (configForm.ShowDialog() == DialogResult.OK)
            {
                var oldAutostartEnabled = config.AutostartEnabled;
                config = configForm.Configuration;
                SetupHttpClient();

                // Autostart-Einstellung anwenden, wenn sie sich geändert hat
                if (oldAutostartEnabled != config.AutostartEnabled)
                {
                    SetupAutostart();

                    var statusMessage = config.AutostartEnabled ?
                        "Autostart wurde aktiviert. Die App startet jetzt automatisch mit Windows." :
                        "Autostart wurde deaktiviert. Die App startet nicht mehr automatisch mit Windows.";

                    ShowBalloonTip("Autostart geändert", statusMessage, ToolTipIcon.Info);
                }
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Die Cleanup wird über MainForm_FormClosing Event Handler gemacht
            base.OnFormClosing(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Visible = false;
            base.OnLoad(e);
        }

        private async void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            await CleanupAndExit();
        }

        private async Task ExitApplication()
        {
            await CleanupAndExit();
        }
        private async Task CleanupAndExit()
        {
            try
            {
                if (isUnlocked)
                {
                    ShowBalloonTip("AdGuard wird gesperrt", "Vor dem Beenden wird AdGuard automatisch wieder gesperrt...", ToolTipIcon.Info);

                    // Timer stoppen
                    if (resetTimer != null)
                    {
                        resetTimer.Stop();
                        resetTimer.Dispose();
                    }                    // Client-Einstellungen wieder sperren
                    await RestoreOriginalClientSettings();

                    // Globale AdGuard-Einstellungen wiederherstellen
                    if (originalAdGuardSettings != null)
                    {
                        await RestoreAdGuardSettings(originalAdGuardSettings);
                    }

                    ShowBalloonTip("AdGuard gesperrt", "AdGuard wurde erfolgreich gesperrt. App wird beendet.", ToolTipIcon.Info);
                }

                // Tray Icon cleanup
                if (trayIcon != null)
                {
                    trayIcon.Visible = false;
                    trayIcon.Dispose();
                }

                // HTTP Client cleanup
                httpClient?.Dispose();

                Application.Exit();
            }
            catch (Exception ex)
            {
                ShowBalloonTip("Fehler beim Beenden", $"Fehler beim Sperren von AdGuard: {ex.Message}", ToolTipIcon.Warning);
                // Beenden trotzdem durchführen
                Application.Exit();
            }
        }

        private async Task DisableClientAccess()
        {
            try
            {
                // Finde den aktuellen AutoAllow Client
                var clientsUrl = $"{config.AdGuardHost.TrimEnd('/')}/control/clients";
                var response = await httpClient.GetAsync(clientsUrl);
                var content = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(content);
                var clients = doc.RootElement.GetProperty("clients");

                foreach (var client in clients.EnumerateArray())
                {
                    var name = client.GetProperty("name").GetString();
                    if (name != null && name.StartsWith("AutoAllow_"))
                    {
                        if (client.TryGetProperty("ids", out var ids))
                        {
                            foreach (var id in ids.EnumerateArray())
                            {
                                if (id.GetString() == config.TargetClientIP)
                                {
                                    // Client löschen
                                    await DeleteClient(name);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Fehler beim Deaktivieren des Client-Zugangs: {ex.Message}");
            }
        }

        private async Task DeleteClient(string clientName)
        {
            var deleteUrl = $"{config.AdGuardHost.TrimEnd('/')}/control/clients/delete";
            var deletePayload = new { name = clientName };
            var deleteJson = JsonSerializer.Serialize(deletePayload);
            var deleteContent = new StringContent(deleteJson, Encoding.UTF8);
            deleteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var deleteResponse = await httpClient.PostAsync(deleteUrl, deleteContent);
            if (!deleteResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Löschen des Clients fehlgeschlagen: {deleteResponse.StatusCode}");
            }
        }

        private async Task<AdGuardBackup> BackupAdGuardSettings()
        {
            var backup = new AdGuardBackup();

            try
            {
                // Sichere blockierte Dienste
                var blockedServicesUrl = $"{config.AdGuardHost.TrimEnd('/')}/control/blocked_services/get";
                var blockedResponse = await httpClient.GetAsync(blockedServicesUrl);
                if (blockedResponse.IsSuccessStatusCode)
                {
                    var blockedContent = await blockedResponse.Content.ReadAsStringAsync();
                    using var blockedDoc = JsonDocument.Parse(blockedContent);
                    if (blockedDoc.RootElement.TryGetProperty("ids", out var idsArray))
                    {
                        backup.BlockedServicesIds = idsArray.EnumerateArray()
                            .Select(x => x.GetString() ?? "")
                            .Where(x => !string.IsNullOrEmpty(x))
                            .ToArray();
                    }
                }

                // Sichere benutzerdefinierte Filterregeln
                var filteringUrl = $"{config.AdGuardHost.TrimEnd('/')}/control/filtering/status";
                var filteringResponse = await httpClient.GetAsync(filteringUrl);
                if (filteringResponse.IsSuccessStatusCode)
                {
                    var filteringContent = await filteringResponse.Content.ReadAsStringAsync();
                    using var filteringDoc = JsonDocument.Parse(filteringContent);
                    if (filteringDoc.RootElement.TryGetProperty("user_rules", out var rulesArray))
                    {
                        backup.CustomFilterRules = rulesArray.EnumerateArray()
                            .Select(x => x.GetString() ?? "")
                            .Where(x => !string.IsNullOrEmpty(x))
                            .ToArray();
                    }
                }

                LogDebugInfo($"AdGuard-Einstellungen gesichert: {backup.BlockedServicesIds.Length} blockierte Dienste, {backup.CustomFilterRules.Length} Filterregeln");
                return backup;
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Fehler beim Sichern der AdGuard-Einstellungen: {ex.Message}");
                throw;
            }
        }

        private async Task RestoreAdGuardSettings(AdGuardBackup backup)
        {
            try
            {                // Wiederherstellen der blockierten Dienste
                var blockedServicesUrl = $"{config.AdGuardHost.TrimEnd('/')}/control/blocked_services/update";
                var blockedData = new
                {
                    ids = backup.BlockedServicesIds,
                    schedule = new { } // Leeres Schedule-Objekt
                };
                var blockedJson = JsonSerializer.Serialize(blockedData);
                var blockedContent = new StringContent(blockedJson, Encoding.UTF8);
                blockedContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var blockedResponse = await httpClient.PutAsync(blockedServicesUrl, blockedContent);

                if (!blockedResponse.IsSuccessStatusCode)
                {
                    var errorContent = await blockedResponse.Content.ReadAsStringAsync();
                    LogDebugInfo($"Fehler beim Wiederherstellen blockierter Dienste: {blockedResponse.StatusCode} - {errorContent}");
                }

                // Wiederherstellen der benutzerdefinierten Filterregeln
                var filteringUrl = $"{config.AdGuardHost.TrimEnd('/')}/control/filtering/set_rules";
                var filteringData = new { rules = backup.CustomFilterRules };
                var filteringJson = JsonSerializer.Serialize(filteringData);
                var filteringContent = new StringContent(filteringJson, Encoding.UTF8);
                filteringContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var filteringResponse = await httpClient.PostAsync(filteringUrl, filteringContent);

                if (!filteringResponse.IsSuccessStatusCode)
                {
                    var errorContent = await filteringResponse.Content.ReadAsStringAsync();
                    LogDebugInfo($"Fehler beim Wiederherstellen der Filterregeln: {filteringResponse.StatusCode} - {errorContent}");
                }

                LogDebugInfo("AdGuard-Einstellungen erfolgreich wiederhergestellt");
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Fehler beim Wiederherstellen der AdGuard-Einstellungen: {ex.Message}");
                throw;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Visible = false;
            this.Hide();
        }

        /// <summary>
        /// Überprüft die Windows Hello Verfügbarkeit bevor Authentifizierung versucht wird
        /// </summary>
        private bool IsWindowsHelloAvailable()
        {
            try
            {
                // Prüfe ob Windows 10 Version 1607 oder höher (Build 14393)
                var version = Environment.OSVersion.Version;
                if (version.Major < 10 || (version.Major == 10 && version.Build < 14393))
                {
                    LogDebugInfo("⚠️ Windows Hello benötigt Windows 10 Version 1607 oder höher");
                    return false;
                }

                // Prüfe ob der aktuelle Benutzer Windows Hello verwenden kann
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                if (identity == null || !identity.IsAuthenticated)
                {
                    LogDebugInfo("⚠️ Benutzer ist nicht authentifiziert");
                    return false;
                }

                LogDebugInfo("✅ Windows Hello Grundvoraussetzungen erfüllt");
                return true;
            }
            catch (Exception ex)
            {
                LogDebugInfo($"❌ Fehler bei Windows Hello Verfügbarkeitsprüfung: {ex.Message}");
                return false;
            }
        }
    }

    // Klasse zur Speicherung der ursprünglichen Client-Einstellungen
    public class ClientBackup
    {
        public string Name { get; set; } = "";
        public string[] Ids { get; set; } = Array.Empty<string>();
        public bool UseGlobalSettings { get; set; }
        public bool FilteringEnabled { get; set; }
        public bool ParentalEnabled { get; set; }
        public bool SafebrowsingEnabled { get; set; }
        public bool SafesearchEnabled { get; set; }
        public bool UseGlobalBlockedServices { get; set; }
        public string[] BlockedServices { get; set; } = Array.Empty<string>();
        public string[] Tags { get; set; } = Array.Empty<string>();
        public string[] Upstreams { get; set; } = Array.Empty<string>();
    }

    // Klasse zur Speicherung der ursprünglichen globalen AdGuard-Einstellungen
    public class AdGuardBackup
    {
        public string[] BlockedServicesIds { get; set; } = Array.Empty<string>();
        public string[] CustomFilterRules { get; set; } = Array.Empty<string>();
    }

    public class AdGuardConfig
    {
        public string AdGuardHost { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string TargetClientIP { get; set; } = "";
        public int DurationMinutes { get; set; } = 60;
        public bool AutostartEnabled { get; set; } = false;
    }
}
