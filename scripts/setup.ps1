# 🛠️ Setup Script für GitHub Copilot CLI und Release-Tools
# Installiert alle benötigten Tools für automatisches Committen und Releaseüberprüfung

param(
    [Parameter(Mandatory=$false)]
    [switch]$SkipCopilot = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$UpdateTools = $false
)

# Farben für die Ausgabe
function Write-Success { param([string]$msg) Write-Host "✅ $msg" -ForegroundColor Green }
function Write-Error { param([string]$msg) Write-Host "❌ $msg" -ForegroundColor Red }
function Write-Warning { param([string]$msg) Write-Host "⚠️  $msg" -ForegroundColor Yellow }
function Write-Info { param([string]$msg) Write-Host "ℹ️  $msg" -ForegroundColor Cyan }
function Write-Step { param([string]$msg) Write-Host "🚀 $msg" -ForegroundColor Magenta }

Write-Host @"
╔══════════════════════════════════════════════════════════╗
║          🛠️  AdGuard Tray App Setup Script               ║
║         GitHub Copilot CLI & Release Tools              ║
╚══════════════════════════════════════════════════════════╝
"@ -ForegroundColor Blue

# Prüfe Admin-Rechte
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")
if (-not $isAdmin) {
    Write-Warning "Dieses Skript sollte als Administrator ausgeführt werden für beste Ergebnisse"
}

# 1. Node.js prüfen/installieren
Write-Step "Prüfe Node.js Installation..."

$nodeVersion = $null
try {
    $nodeVersion = node --version 2>$null
    if ($nodeVersion) {
        $versionNumber = [Version]($nodeVersion -replace '^v', '')
        if ($versionNumber -ge [Version]"16.0.0") {
            Write-Success "Node.js $nodeVersion bereits installiert"
        } else {
            Write-Warning "Node.js Version zu alt ($nodeVersion). Mindestens v16.0.0 erforderlich"
            $UpdateTools = $true
        }
    }
} catch {
    Write-Warning "Node.js nicht gefunden"
}

if (-not $nodeVersion -or $UpdateTools) {
    Write-Info "Installiere Node.js..."
    
    if (Get-Command "winget" -ErrorAction SilentlyContinue) {
        try {
            winget install OpenJS.NodeJS
            Write-Success "Node.js über winget installiert"
        } catch {
            Write-Warning "winget Installation fehlgeschlagen. Bitte manuell von https://nodejs.org installieren"
        }
    } elseif (Get-Command "choco" -ErrorAction SilentlyContinue) {
        try {
            choco install nodejs -y
            Write-Success "Node.js über Chocolatey installiert"
        } catch {
            Write-Warning "Chocolatey Installation fehlgeschlagen"
        }
    } else {
        Write-Warning "Automatische Installation nicht möglich. Bitte Node.js manuell installieren:"
        Write-Info "Download: https://nodejs.org/en/download/"
        Write-Info "Nach der Installation starten Sie dieses Skript erneut"
        exit 1
    }
}

# 2. GitHub CLI prüfen/installieren
Write-Step "Prüfe GitHub CLI Installation..."

$ghVersion = $null
try {
    $ghVersion = gh --version 2>$null
    if ($ghVersion) {
        Write-Success "GitHub CLI bereits installiert: $($ghVersion[0])"
    }
} catch {
    Write-Warning "GitHub CLI nicht gefunden"
}

if (-not $ghVersion -or $UpdateTools) {
    Write-Info "Installiere GitHub CLI..."
    
    if (Get-Command "winget" -ErrorAction SilentlyContinue) {
        try {
            winget install GitHub.cli
            Write-Success "GitHub CLI über winget installiert"
        } catch {
            Write-Warning "winget Installation fehlgeschlagen"
        }
    } elseif (Get-Command "choco" -ErrorAction SilentlyContinue) {
        try {
            choco install gh -y
            Write-Success "GitHub CLI über Chocolatey installiert"
        } catch {
            Write-Warning "Chocolatey Installation fehlgeschlagen"
        }
    } else {
        Write-Warning "Automatische Installation nicht möglich. Bitte GitHub CLI manuell installieren:"
        Write-Info "Download: https://cli.github.com/"
    }
}

# 3. GitHub CLI und Copilot Extension prüfen/installieren
Write-Step "Prüfe GitHub CLI und Copilot Extension..."

$ghInstalled = $false
try {
    $ghVersion = gh --version 2>$null
    if ($ghVersion) {
        Write-Success "GitHub CLI bereits installiert: $($ghVersion.Split("`n")[0])"
        $ghInstalled = $true
    }
} catch {
    Write-Info "GitHub CLI nicht gefunden"
}

if (-not $ghInstalled -or $UpdateTools) {
    Write-Info "Installiere GitHub CLI..."
    if (Get-Command "winget" -ErrorAction SilentlyContinue) {
        try {
            winget install GitHub.cli
            Write-Success "GitHub CLI über winget installiert"
            $ghInstalled = $true
        } catch {
            Write-Warning "Winget Installation fehlgeschlagen"
        }
    } elseif (Get-Command "choco" -ErrorAction SilentlyContinue) {
        try {
            choco install gh -y
            Write-Success "GitHub CLI über Chocolatey installiert"
            $ghInstalled = $true
        } catch {
            Write-Warning "Chocolatey Installation fehlgeschlagen"
        }
    } else {
        Write-Warning "Automatische Installation nicht möglich. Bitte GitHub CLI manuell installieren:"
        Write-Info "Download: https://cli.github.com/"
    }
}

# GitHub CLI Copilot Extension installieren
if ($ghInstalled -and -not $SkipCopilot) {
    Write-Step "Prüfe GitHub CLI Copilot Extension..."
    
    try {
        $extensions = gh extension list 2>$null
        if ($extensions -match "github/gh-copilot") {
            Write-Success "GitHub CLI Copilot Extension bereits installiert"
        } else {
            Write-Info "Installiere GitHub CLI Copilot Extension..."
            gh extension install github/gh-copilot --force
            Write-Success "GitHub CLI Copilot Extension installiert"
        }
    } catch {
        Write-Warning "Fehler bei Copilot Extension Installation: $($_.Exception.Message)"
    }
    
    # GitHub CLI Authentifizierung prüfen
    Write-Step "Prüfe GitHub CLI Authentifizierung..."
    
    try {
        $authStatus = gh auth status 2>$null
        if ($authStatus -match "Logged in") {
            Write-Success "GitHub CLI bereits authentifiziert"
        } else {
            Write-Info "GitHub CLI Authentifizierung erforderlich..."
            Write-Warning "Führen Sie aus: gh auth login --web"
            Write-Info "Oder starten Sie die Authentifizierung jetzt? (J/N)"
            $response = Read-Host
            if ($response -match '^[jJyY]') {
                gh auth login --web
            }
        }
    } catch {
        Write-Warning "Konnte GitHub CLI Authentifizierungsstatus nicht prüfen"
        Write-Info "Führen Sie manuell aus: gh auth login --web"
    }
    
    # PowerShell Copilot Aliase einrichten
    Write-Step "Richte PowerShell Copilot Aliase ein..."
    
    try {
        # Erstelle Copilot Profil
        $profileDir = Split-Path -Path $PROFILE -Parent
        $copilotProfile = Join-Path -Path $profileDir -ChildPath "gh-copilot.ps1"
        
        if (-not (Test-Path $copilotProfile) -or $UpdateTools) {
            Write-Info "Erstelle PowerShell Copilot Aliase..."
            gh copilot alias -- pwsh | Out-File -FilePath $copilotProfile -Force -Encoding UTF8
            Write-Success "Copilot Aliase erstellt: $copilotProfile"
            
            # Füge zum PowerShell Profil hinzu
            $profileContent = ""
            if (Test-Path $PROFILE) {
                $profileContent = Get-Content $PROFILE -Raw
            }
            
            $copilotImport = ". `"$copilotProfile`""
            if ($profileContent -notmatch [regex]::Escape($copilotImport)) {
                Write-Info "Füge Copilot Aliase zum PowerShell Profil hinzu..."
                Add-Content -Path $PROFILE -Value "`n# GitHub Copilot CLI Aliase`n$copilotImport" -Force
                Write-Success "PowerShell Profil aktualisiert"
                Write-Warning "Starten Sie PowerShell neu oder führen Sie aus: . `"$PROFILE`""
            } else {
                Write-Success "Copilot Aliase bereits im PowerShell Profil"
            }
        } else {
            Write-Success "PowerShell Copilot Aliase bereits konfiguriert"
        }
    } catch {
        Write-Warning "Fehler beim Einrichten der Copilot Aliase: $($_.Exception.Message)"
    }
}

# 4. Git Konfiguration prüfen
Write-Step "Prüfe Git Konfiguration..."

$gitUser = git config user.name 2>$null
$gitEmail = git config user.email 2>$null

if (-not $gitUser) {
    Write-Warning "Git user.name nicht konfiguriert"
    Write-Info "Benutzername eingeben:"
    $userName = Read-Host
    if ($userName) {
        git config --global user.name $userName
        Write-Success "Git user.name konfiguriert: $userName"
    }
} else {
    Write-Success "Git user.name: $gitUser"
}

if (-not $gitEmail) {
    Write-Warning "Git user.email nicht konfiguriert"
    Write-Info "E-Mail eingeben:"
    $userEmail = Read-Host
    if ($userEmail) {
        git config --global user.email $userEmail
        Write-Success "Git user.email konfiguriert: $userEmail"
    }
} else {
    Write-Success "Git user.email: $gitEmail"
}

# 7. Skript-Aliase einrichten
Write-Step "Richte PowerShell-Aliase ein..."

$profilePath = $PROFILE
$profileDir = Split-Path $profilePath -Parent

if (-not (Test-Path $profileDir)) {
    New-Item -ItemType Directory -Path $profileDir -Force | Out-Null
}

$aliasContent = @"

# AdGuard Tray App Release-Scripts
function Release-AdGuard { 
    param([string]`$Type = "patch", [switch]`$DryRun, [switch]`$Force)
    & "`$PSScriptRoot\..\scripts\auto-release.ps1" -VersionType `$Type -DryRun:`$DryRun -Force:`$Force
}

function Commit-Quick { 
    param([string]`$Message = "", [switch]`$All, [switch]`$Push)
    & "`$PSScriptRoot\..\scripts\quick-commit.ps1" -Message `$Message -All:`$All -Push:`$Push
}

Set-Alias -Name "release" -Value "Release-AdGuard"
Set-Alias -Name "qcommit" -Value "Commit-Quick"

# GitHub Copilot CLI Aliases (falls installiert)
if (Get-Command "github-copilot-cli" -ErrorAction SilentlyContinue) {
    function copilot { github-copilot-cli `$args }
    function cop { github-copilot-cli `$args }
}
"@

$existingProfile = ""
if (Test-Path $profilePath) {
    $existingProfile = Get-Content $profilePath -Raw
}

if ($existingProfile -notmatch "AdGuard Tray App Release-Scripts") {
    Add-Content -Path $profilePath -Value $aliasContent
    Write-Success "PowerShell-Aliase hinzugefügt zu: $profilePath"
    Write-Info "Starten Sie PowerShell neu oder führen Sie aus: . `$PROFILE"
} else {
    Write-Success "PowerShell-Aliase bereits konfiguriert"
}

# 8. VS Code Settings für Copilot (falls VS Code installiert)
if (Get-Command "code" -ErrorAction SilentlyContinue) {
    Write-Step "Konfiguriere VS Code für GitHub Copilot..."
    
    $vscodeSettingsPath = "$env:APPDATA\Code\User\settings.json"
    
    if (Test-Path $vscodeSettingsPath) {
        try {
            $settings = Get-Content $vscodeSettingsPath -Raw | ConvertFrom-Json
            
            # Copilot Commit Message Konfiguration hinzufügen
            if (-not $settings."github.copilot.chat.commitMessageGeneration.instructions") {
                $settings | Add-Member -NotePropertyName "github.copilot.chat.commitMessageGeneration.instructions" -NotePropertyValue @(
                    @{ "file" = ".copilot/commit-messages.md" }
                ) -Force
                
                $settings | ConvertTo-Json -Depth 10 | Set-Content $vscodeSettingsPath
                Write-Success "VS Code Copilot-Konfiguration aktualisiert"
            } else {
                Write-Success "VS Code Copilot bereits konfiguriert"
            }
        } catch {
            Write-Warning "Konnte VS Code Settings nicht aktualisieren: $($_.Exception.Message)"
        }
    } else {
        Write-Info "VS Code Settings-Datei nicht gefunden - wird bei erstem Start erstellt"
    }
}

# Abschluss
Write-Step "✨ Setup abgeschlossen!"
Write-Success "Verfügbare Befehle:"
Write-Info "  release patch    - Patch-Version Release (1.0.0 -> 1.0.1)"
Write-Info "  release minor    - Minor-Version Release (1.0.0 -> 1.1.0)"  
Write-Info "  release major    - Major-Version Release (1.0.0 -> 2.0.0)"
Write-Info "  qcommit -All     - Quick Commit aller Änderungen"
Write-Info "  qcommit -Push    - Quick Commit mit automatischem Push"

Write-Success "GitHub Copilot Integration:"
Write-Info "  - Automatische Commit-Nachrichten mit 'qcommit'"
Write-Info "  - Template in .copilot/commit-messages.md"
Write-Info "  - VS Code Integration für Commit Message Generation"

if (-not $SkipCopilot) {
    Write-Warning "Vergessen Sie nicht, GitHub Copilot CLI zu authentifizieren:"
    Write-Info "  github-copilot-cli auth"
}

Write-Info "Starten Sie PowerShell neu, um die neuen Aliase zu verwenden!"
