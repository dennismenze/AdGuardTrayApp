# 🎯 AdGuard Tray App - Alle Tools in einem Skript
# Komplettes Release-Management mit einem Befehl

param(
    [Parameter(Mandatory=$true, Position=0)]
    [ValidateSet("setup", "commit", "release", "hotfix", "feature", "status")]
    [string]$Action,
    
    [Parameter(Mandatory=$false)]
    [string]$Message = "",
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("patch", "minor", "major")]
    [string]$Version = "patch",
    
    [Parameter(Mandatory=$false)]
    [switch]$DryRun = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$Force = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$Push = $false
)

# Hilfsfunktionen
function Write-Header {
    Write-Host @"
╔══════════════════════════════════════════════════════════╗
║          🛡️  AdGuard Tray App Tool Suite                ║
║               Alles-in-einem Release Tool               ║
╚══════════════════════════════════════════════════════════╝
"@ -ForegroundColor Blue
}

function Write-Success { param([string]$msg) Write-Host "✅ $msg" -ForegroundColor Green }
function Write-Error { param([string]$msg) Write-Host "❌ $msg" -ForegroundColor Red }
function Write-Warning { param([string]$msg) Write-Host "⚠️  $msg" -ForegroundColor Yellow }
function Write-Info { param([string]$msg) Write-Host "ℹ️  $msg" -ForegroundColor Cyan }

Write-Header

switch ($Action) {
    "setup" {
        Write-Info "Führe Setup aus..."
        & "$PSScriptRoot\scripts\setup.ps1"
    }
    
    "commit" {
        Write-Info "Führe Quick Commit aus..."
        $params = @{}
        if ($Message) { $params.Message = $Message }
        if ($Push) { $params.Push = $true }
        & "$PSScriptRoot\scripts\quick-commit.ps1" @params -All
    }
    
    "release" {
        Write-Info "Führe Release aus (Version: $Version)..."
        $params = @{ VersionType = $Version }
        if ($Message) { $params.CustomMessage = $Message }
        if ($DryRun) { $params.DryRun = $true }
        if ($Force) { $params.Force = $true }
        & "$PSScriptRoot\scripts\auto-release.ps1" @params
    }    "hotfix" {
        Write-Info "Hotfix-Workflow..."
        
        # 1. Quick Commit falls Änderungen vorhanden
        $status = git status --porcelain
        if ($status) {
            if ($Message) {
                # Benutzerdefinierte Nachricht verwenden
                & "$PSScriptRoot\scripts\quick-commit.ps1" -Message $Message -All
            } else {
                # GitHub Copilot für intelligente Commit-Nachricht nutzen
                & "$PSScriptRoot\scripts\quick-commit.ps1" -All
            }
        }
        
        # 2. Patch Release
        & "$PSScriptRoot\scripts\auto-release.ps1" -VersionType patch -Force:$Force -DryRun:$DryRun
    }
    
    "feature" {
        Write-Info "Feature-Workflow..."
        
        # 1. Quick Commit falls Änderungen vorhanden  
        $status = git status --porcelain
        if ($status) {
            if ($Message) {
                # Benutzerdefinierte Nachricht verwenden
                & "$PSScriptRoot\scripts\quick-commit.ps1" -Message $Message -All
            } else {
                # GitHub Copilot für intelligente Commit-Nachricht nutzen
                & "$PSScriptRoot\scripts\quick-commit.ps1" -All
            }
        }
        
        # 2. Minor Release
        & "$PSScriptRoot\scripts\auto-release.ps1" -VersionType minor -Force:$Force -DryRun:$DryRun
    }
    
    "status" {
        Write-Info "Repository Status:"
        Write-Host "═══════════════════════════════════════" -ForegroundColor Blue
        
        # Git Status
        Write-Info "Git Status:"
        git status --short
        
        # Aktuelle Version
        $currentTag = git describe --tags --abbrev=0 2>$null
        if ($currentTag) {
            Write-Info "Aktuelle Version: $currentTag"
        } else {
            Write-Warning "Keine Version (Tags) gefunden"
        }
        
        # Branch
        $branch = git rev-parse --abbrev-ref HEAD
        Write-Info "Aktueller Branch: $branch"
        
        # Remote Status
        try {
            $ahead = git rev-list --count HEAD..origin/$branch 2>$null
            $behind = git rev-list --count origin/$branch..HEAD 2>$null
            if ($ahead -eq 0 -and $behind -eq 0) {
                Write-Success "Repository ist synchron mit Remote"
            } else {
                Write-Warning "Repository ist $behind Commits ahead, $ahead Commits behind"
            }
        } catch {
            Write-Warning "Konnte Remote-Status nicht ermitteln"
        }
        
        # Tools Status
        Write-Host "═══════════════════════════════════════" -ForegroundColor Blue
        Write-Info "Tool Status:"
        
        # Node.js
        $nodeVersion = node --version 2>$null
        if ($nodeVersion) {
            Write-Success "Node.js: $nodeVersion"
        } else {
            Write-Error "Node.js nicht installiert"
        }
        
        # GitHub CLI
        $ghVersion = gh --version 2>$null | Select-Object -First 1
        if ($ghVersion) {
            Write-Success "GitHub CLI: $ghVersion"
        } else {
            Write-Error "GitHub CLI nicht installiert"
        }
        
        # GitHub Copilot CLI
        try {
            $copilotStatus = github-copilot-cli auth status 2>$null
            if ($copilotStatus -match "authenticated") {
                Write-Success "GitHub Copilot: Authentifiziert"
            } else {
                Write-Warning "GitHub Copilot: Nicht authentifiziert"
            }
        } catch {
            Write-Error "GitHub Copilot CLI nicht installiert"
        }
    }
}

# Hilfe anzeigen
if ($Action -eq "setup") {
    Write-Host @"

📋 Verfügbare Befehle nach dem Setup:

🔧 Grundlegende Befehle:
  .\dev.ps1 status                    - Repository und Tool Status
  .\dev.ps1 commit                    - Quick Commit aller Änderungen
  .\dev.ps1 commit -Push              - Commit + Push
  .\dev.ps1 commit -Message "text"    - Commit mit eigener Nachricht

🚀 Release Befehle:
  .\dev.ps1 release                   - Patch Release (1.0.0 → 1.0.1)
  .\dev.ps1 release -Version minor    - Minor Release (1.0.0 → 1.1.0)
  .\dev.ps1 release -Version major    - Major Release (1.0.0 → 2.0.0)
  .\dev.ps1 release -DryRun          - Nur anzeigen, nicht ausführen

⚡ Workflow Befehle:
  .\dev.ps1 hotfix                   - Commit + Patch Release
  .\dev.ps1 feature                  - Commit + Minor Release
  .\dev.ps1 hotfix -Message "text"   - Mit eigener Commit-Nachricht

🎯 Beispiele:
  .\dev.ps1 setup                              # Einmalige Einrichtung
  .\dev.ps1 status                             # Status prüfen
  .\dev.ps1 commit -Message "🐛 fix: Bug XYZ"  # Spezifischer Commit
  .\dev.ps1 hotfix -DryRun                     # Hotfix testen
  .\dev.ps1 release -Version minor             # Feature Release

"@ -ForegroundColor Yellow
}
