# 🚀 AdGuard Tray App - Auto Release Script
# Automatisches Versionieren, Committen, Taggen und Deployen mit GitHub Copilot

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("patch", "minor", "major")]
    [string]$VersionType = "patch",
    
    [Parameter(Mandatory=$false)]
    [string]$CustomMessage = "",
    
    [Parameter(Mandatory=$false)]
    [switch]$DryRun = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipCopilot = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$Force = $false
)

# Konfiguration
$ProjectFile = "AdGuardTrayApp/AdGuardTrayApp.csproj"
$CommitTemplate = ".copilot/commit-messages.md"

# Farben für die Ausgabe
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

function Write-Success { param([string]$msg) Write-ColorOutput "✅ $msg" "Green" }
function Write-Error { param([string]$msg) Write-ColorOutput "❌ $msg" "Red" }
function Write-Warning { param([string]$msg) Write-ColorOutput "⚠️  $msg" "Yellow" }
function Write-Info { param([string]$msg) Write-ColorOutput "ℹ️  $msg" "Cyan" }
function Write-Step { param([string]$msg) Write-ColorOutput "🚀 $msg" "Magenta" }

# Header
Write-ColorOutput @"
╔══════════════════════════════════════════════════════════╗
║                🛡️  AdGuard Tray App                      ║
║            🚀 Automatisches Release-Skript              ║
╚══════════════════════════════════════════════════════════╝
"@ "Blue"

# Voraussetzungen prüfen
Write-Step "Überprüfe Voraussetzungen..."

# Git Repository prüfen
if (-not (Test-Path ".git")) {
    Write-Error "Kein Git-Repository gefunden!"
    exit 1
}

# Git Status prüfen
$gitStatus = git status --porcelain
if ($gitStatus -and -not $Force) {
    Write-Error "Arbeitsverzeichnis ist nicht sauber! Committen Sie Ihre Änderungen oder verwenden Sie -Force"
    Write-Info "Uncommitted files:"
    $gitStatus | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
    exit 1
}

# Branch prüfen
$currentBranch = git rev-parse --abbrev-ref HEAD
if ($currentBranch -ne "main" -and $currentBranch -ne "master" -and -not $Force) {
    Write-Warning "Sie sind nicht auf dem main/master Branch ($currentBranch). Fortfahren? (J/N)"
    $response = Read-Host
    if ($response -notmatch '^[jJyY]') {
        Write-Info "Abgebrochen."
        exit 0
    }
}

# Remote prüfen
$hasRemote = git remote | Where-Object { $_ -eq "origin" }
if (-not $hasRemote) {
    Write-Error "Kein 'origin' Remote gefunden!"
    exit 1
}

Write-Success "Alle Voraussetzungen erfüllt"

# Aktuelle Version ermitteln
Write-Step "Ermittle aktuelle Version..."

$latestTag = git describe --tags --abbrev=0 2>$null
if (-not $latestTag) {
    $currentVersion = "0.0.0"
    Write-Warning "Kein vorhandenes Tag gefunden. Starte mit Version 0.0.0"
} else {
    $currentVersion = $latestTag -replace '^v', ''
    Write-Info "Aktuelle Version: $currentVersion"
}

# Neue Version berechnen
Write-Step "Berechne neue Version..."

$versionParts = $currentVersion.Split('.')
$major = [int]$versionParts[0]
$minor = [int]$versionParts[1]
$patch = [int]$versionParts[2]

switch ($VersionType) {
    "major" { 
        $major++
        $minor = 0
        $patch = 0
    }
    "minor" { 
        $minor++
        $patch = 0
    }
    "patch" { 
        $patch++
    }
}

$newVersion = "$major.$minor.$patch"
$newTag = "v$newVersion"

Write-Info "Neue Version: $newVersion"
Write-Info "Neues Tag: $newTag"

if ($DryRun) {
    Write-Warning "DRY RUN - Keine Änderungen werden vorgenommen"
}

# Version in Projektdatei aktualisieren
Write-Step "Aktualisiere Version in Projektdatei..."

if (Test-Path $ProjectFile) {
    $projectContent = Get-Content $ProjectFile -Raw
    
    # Version in .csproj aktualisieren
    if ($projectContent -match '<Version>([^<]+)</Version>') {
        $projectContent = $projectContent -replace '<Version>([^<]+)</Version>', "<Version>$newVersion</Version>"
    } elseif ($projectContent -match '<PropertyGroup>') {
        $projectContent = $projectContent -replace '(<PropertyGroup[^>]*>)', "`$1`n    <Version>$newVersion</Version>"
    } else {
        Write-Error "Konnte Version in $ProjectFile nicht aktualisieren"
        exit 1
    }
    
    # AssemblyVersion aktualisieren falls vorhanden
    if ($projectContent -match '<AssemblyVersion>([^<]+)</AssemblyVersion>') {
        $projectContent = $projectContent -replace '<AssemblyVersion>([^<]+)</AssemblyVersion>', "<AssemblyVersion>$newVersion.0</AssemblyVersion>"
    }
    
    # FileVersion aktualisieren falls vorhanden
    if ($projectContent -match '<FileVersion>([^<]+)</FileVersion>') {
        $projectContent = $projectContent -replace '<FileVersion>([^<]+)</FileVersion>', "<FileVersion>$newVersion.0</FileVersion>"
    }
    
    if (-not $DryRun) {
        $projectContent | Set-Content $ProjectFile -Encoding UTF8
        Write-Success "Version in $ProjectFile aktualisiert"
    }
} else {
    Write-Warning "$ProjectFile nicht gefunden - Version wird nur als Tag gesetzt"
}

# Änderungen seit letztem Tag sammeln
Write-Step "Sammle Änderungen seit letztem Release..."

$changesSinceLastTag = ""
if ($latestTag) {
    $commitLog = git log --pretty=format:"- %s" "$latestTag..HEAD" | Where-Object { $_ -ne "- " }
    if ($commitLog) {
        $changesSinceLastTag = "`n`nÄnderungen seit ${latestTag}:`n" + ($commitLog -join "`n")
    }
}

# GitHub Copilot für Commit-Nachricht verwenden
$commitMessage = ""

if (-not $SkipCopilot -and (Get-Command "gh" -ErrorAction SilentlyContinue)) {
    Write-Step "GitHub Copilot für Release-Commit-Nachricht..."
    
    try {
        # Git diff für Copilot erstellen
        $stagedChanges = git diff --cached --name-status
        if (-not $stagedChanges) {
            # Falls nichts gestaged ist, stage die Projektdatei
            if (-not $DryRun) {
                git add $ProjectFile
            }
            $stagedChanges = "M    $ProjectFile"
        }
        
        # Erstelle einen Release-spezifischen Prompt
        $prompt = "Git commit message for release version $newVersion ($VersionType bump from $currentVersion). Changes: $($stagedChanges -join ', ') - Use German with emoji format: 📦 release: Version $newVersion"
        
        Write-Host ""
        Write-Host "🤖 GitHub Copilot wird für Release-Nachricht gestartet..." -ForegroundColor Cyan
        Write-Host "Prompt: $prompt" -ForegroundColor Gray
        Write-Host "Folgen Sie den Anweisungen von Copilot und wählen Sie eine passende Release-Nachricht aus." -ForegroundColor Yellow
        Write-Host ""
        
        # Starte Copilot interaktiv
        & gh copilot suggest $prompt
        
        Write-Host ""
        Write-Info "Hat Copilot eine brauchbare Release-Nachricht vorgeschlagen? (J/N)"
        $useCopilot = Read-Host
        
        if ($useCopilot -match '^[jJyY]') {
            Write-Info "Geben Sie die gewählte Release-Commit-Nachricht ein:"
            $commitMessage = Read-Host
            
            if ($commitMessage) {
                $commitMessage += "`n`nCo-authored-by: GitHub Copilot <copilot@github.com>"
                Write-Success "Copilot Release-Nachricht übernommen!"
            }
        }
        
    } catch {
        Write-Warning "GitHub Copilot nicht verfügbar oder Fehler: $($_.Exception.Message)"
        $SkipCopilot = $true
    }
}

# Fallback: Manuelle oder Standard-Commit-Nachricht
if (-not $commitMessage -or $SkipCopilot) {
    if ($CustomMessage) {
        $commitMessage = $CustomMessage
    } else {
        $commitMessage = @"
📦 release: Version $newVersion

- Version bump von $currentVersion auf $newVersion ($VersionType)
- Bereit für automatisches Release-Deployment
- Aktualisierte Projektdatei mit neuer Versionsnummer$changesSinceLastTag
"@
    }
    Write-Info "Verwende $(if ($CustomMessage) { 'benutzerdefinierte' } else { 'Standard' })-Commit-Nachricht"
}

# Bestätigung anzeigen
Write-Step "Release-Zusammenfassung:"
Write-ColorOutput "════════════════════════════════════════" "Blue"
Write-Info "Aktuelle Version: $currentVersion"
Write-Info "Neue Version: $newVersion"
Write-Info "Version Type: $VersionType"
Write-Info "Tag: $newTag"
Write-Info "Branch: $currentBranch"
Write-ColorOutput "────────────────────────────────────────" "Blue"
Write-Info "Commit-Nachricht:"
Write-ColorOutput $commitMessage "White"
Write-ColorOutput "════════════════════════════════════════" "Blue"

if (-not $DryRun -and -not $Force) {
    Write-Warning "Möchten Sie fortfahren? (J/N)"
    $response = Read-Host
    if ($response -notmatch '^[jJyY]') {
        Write-Info "Abgebrochen."
        exit 0
    }
}

if ($DryRun) {
    Write-Warning "DRY RUN abgeschlossen - keine Änderungen vorgenommen"
    exit 0
}

# Git-Operationen durchführen
Write-Step "Führe Git-Operationen durch..."

try {
    # Änderungen stagen
    git add $ProjectFile
    
    # Commit erstellen
    git commit -m $commitMessage
    Write-Success "Commit erstellt"
    
    # Tag erstellen
    git tag -a $newTag -m "Release $newVersion"
    Write-Success "Tag $newTag erstellt"
    
    # Push zu Remote
    Write-Info "Pushe zu Remote Repository..."
    git push origin $currentBranch
    git push origin $newTag
    Write-Success "Erfolgreich zu Remote gepusht"
    
    Write-Step "🎉 Release erfolgreich erstellt!"
    Write-Success "Version $newVersion wurde getaggt und gepusht"
    Write-Success "GitHub Actions wird automatisch das Release erstellen"
    
    # Link zur Actions-Seite
    $repoUrl = git remote get-url origin
    if ($repoUrl -match 'github\.com[:/]([^/]+)/([^/.]+)') {
        $owner = $matches[1]
        $repo = $matches[2] -replace '\.git$', ''
        $actionsUrl = "https://github.com/$owner/$repo/actions"
        Write-Info "GitHub Actions: $actionsUrl"
    }
    
} catch {
    Write-Error "Fehler bei Git-Operationen: $($_.Exception.Message)"
    Write-Warning "Möglicherweise müssen Sie manuell aufräumen:"
    Write-Info "- git reset --soft HEAD~1  # Letzten Commit rückgängig"
    Write-Info "- git tag -d $newTag       # Tag löschen"
    exit 1
}

Write-Success "✨ Release-Prozess abgeschlossen!"
