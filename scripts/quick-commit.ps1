# 🔧 Quick Commit Script mit GitHub Copilot
# Für schnelle Commits ohne Versionierung

param(
    [Parameter(Mandatory=$false)]
    [string]$Message = "",
    
    [Parameter(Mandatory=$false)]
    [switch]$All = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$Push = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipCopilot = $false
)

# Farben für die Ausgabe
function Write-Success { param([string]$msg) Write-Host "✅ $msg" -ForegroundColor Green }
function Write-Error { param([string]$msg) Write-Host "❌ $msg" -ForegroundColor Red }
function Write-Warning { param([string]$msg) Write-Host "⚠️  $msg" -ForegroundColor Yellow }
function Write-Info { param([string]$msg) Write-Host "ℹ️  $msg" -ForegroundColor Cyan }

Write-Host "🚀 Quick Commit mit GitHub Copilot" -ForegroundColor Blue

# Prüfe ob Git-Repository
if (-not (Test-Path ".git")) {
    Write-Error "Kein Git-Repository gefunden!"
    exit 1
}

# Dateien stagen
if ($All) {
    git add .
    Write-Info "Alle Dateien gestaged"
} else {
    $status = git status --porcelain
    if (-not $status) {
        Write-Warning "Keine Änderungen zum Committen gefunden"
        exit 0
    }
    
    Write-Info "Geänderte Dateien:"
    git status --short
    
    Write-Warning "Alle Dateien stagen? (J/N) oder spezifische Dateien eingeben:"
    $response = Read-Host
    
    if ($response -match '^[jJyY]') {
        git add .
        Write-Info "Alle Dateien gestaged"
    } elseif ($response -and $response -notmatch '^[nN]') {
        # Spezifische Dateien stagen
        $files = $response -split '\s+'
        foreach ($file in $files) {
            if (Test-Path $file) {
                git add $file
                Write-Info "Datei gestaged: $file"
            } else {
                Write-Warning "Datei nicht gefunden: $file"
            }
        }
    } else {
        Write-Info "Keine Dateien gestaged. Beende."
        exit 0
    }
}

# Commit-Nachricht generieren
$commitMessage = ""

if (-not $SkipCopilot -and -not $Message) {
    Write-Info "Versuche GitHub Copilot für Commit-Nachricht..."
    
    try {
        # Staged Changes anzeigen
        $stagedFiles = git diff --cached --name-status
        
        Write-Info "Staged Changes:"
        $stagedFiles | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
        
        # GitHub CLI Copilot verwenden
        if (Get-Command "gh" -ErrorAction SilentlyContinue) {
            Write-Info "GitHub Copilot für Commit-Nachricht..."
            
            # Erstelle einen simplen Prompt
            $prompt = "Git commit message for these changes: $($stagedFiles -join ', ') - Use German with emoji and conventional commits format"
            
            Write-Host ""
            Write-Host "🤖 GitHub Copilot wird gestartet..." -ForegroundColor Cyan
            Write-Host "Prompt: $prompt" -ForegroundColor Gray
            Write-Host "Folgen Sie den Anweisungen von Copilot und wählen Sie eine passende Nachricht aus." -ForegroundColor Yellow
            Write-Host ""
            
            # Starte Copilot interaktiv
            & gh copilot suggest $prompt
            
            Write-Host ""
            Write-Info "Hat Copilot eine brauchbare Commit-Nachricht vorgeschlagen? (J/N)"
            $useCopilot = Read-Host
            
            if ($useCopilot -match '^[jJyY]') {
                Write-Info "Geben Sie die gewählte Commit-Nachricht ein:"
                $commitMessage = Read-Host
                
                if ($commitMessage) {
                    $commitMessage += "`n`nCo-authored-by: GitHub Copilot <copilot@github.com>"
                    Write-Success "Copilot-Nachricht übernommen!"
                }
            }
        } else {
            Write-Warning "GitHub CLI nicht gefunden. Installiere mit: winget install GitHub.cli"
        }
        
    } catch {
        Write-Warning "GitHub Copilot Fehler: $($_.Exception.Message)"
    }
}

# Fallback: Manuelle Eingabe
if (-not $commitMessage) {
    if ($Message) {
        $commitMessage = $Message
    } else {
        Write-Info "GitHub Copilot Template (.copilot/commit-messages.md):"
        Write-Host "🚀 feat: Neue Features" -ForegroundColor Green
        Write-Host "🐛 fix: Bugfixes" -ForegroundColor Green  
        Write-Host "🔧 config: Konfigurationsänderungen" -ForegroundColor Green
        Write-Host "📝 docs: Dokumentation" -ForegroundColor Green
        Write-Host "🎨 style: Code-Formatierung" -ForegroundColor Green
        Write-Host "♻️ refactor: Code-Refactoring" -ForegroundColor Green
        Write-Host ""
        Write-Info "Commit-Nachricht eingeben:"
        $commitMessage = Read-Host
        
        if (-not $commitMessage) {
            Write-Error "Keine Commit-Nachricht eingegeben!"
            exit 1
        }
    }
}

# Commit-Nachricht anzeigen
Write-Info "Commit-Nachricht:"
Write-Host $commitMessage -ForegroundColor White

# Bestätigung
Write-Warning "Committen? (J/N)"
$confirm = Read-Host
if ($confirm -notmatch '^[jJyY]') {
    Write-Info "Abgebrochen."
    exit 0
}

# Commit durchführen
try {
    git commit -m $commitMessage
    Write-Success "Commit erfolgreich erstellt"
    
    # Optional pushen
    if ($Push) {
        $branch = git rev-parse --abbrev-ref HEAD
        git push origin $branch
        Write-Success "Erfolgreich zu Remote gepusht"
    } else {
        Write-Info "Zum Pushen verwenden Sie: git push"
        Write-Info "Oder führen Sie das Skript mit -Push aus"
    }
    
} catch {
    Write-Error "Fehler beim Committen: $($_.Exception.Message)"
    exit 1
}

Write-Success "✨ Commit abgeschlossen!"
