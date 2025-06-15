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
    # Staged Changes anzeigen
    $stagedFiles = git diff --cached --name-status
    
    Write-Info "Staged Changes:"
    $stagedFiles | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
    
    # GitHub CLI Copilot verwenden - Pflicht, kein Fallback
    if (-not (Get-Command "gh" -ErrorAction SilentlyContinue)) {
        Write-Error "GitHub CLI nicht gefunden! Installiere mit: winget install GitHub.cli"
        exit 1
    }
      Write-Host "🤖 Verwende GitHub Copilot..." -ForegroundColor Cyan
      # Erstelle einen detaillierten Git-spezifischen Prompt
    $changedFilesText = ($stagedFiles | ForEach-Object { $_ }) -join ', '
    
    # Hole detaillierte Diff-Informationen für besseren Kontext
    $diffSummary = git diff --cached --stat --summary 2>$null
    $diffLines = git diff --cached --unified=1 2>$null | Select-Object -First 20
    
    # Erstelle einen präzisen Prompt mit Kontext
    $prompt = @"
Create a specific German git commit message with emoji for these changes:

Files changed: $changedFilesText

Diff summary:
$($diffSummary -join "`n")

Code changes context:
$($diffLines -join "`n")

Requirements:
- Use German language
- Start with appropriate emoji (🚀 feat, 🐛 fix, 🔧 config, 📝 docs, 🎨 style, ♻️ refactor)
- Be specific about what was changed, not just which files
- Use conventional commit format: "emoji type: specific description"
- Keep under 72 characters for the first line
"@
      try {
        # Verwende Start-Process für korrekte stdin-Übertragung
        $psi = New-Object System.Diagnostics.ProcessStartInfo
        $psi.FileName = "gh"
        $psi.Arguments = "copilot suggest -t git `"$prompt`""
        $psi.UseShellExecute = $false
        $psi.RedirectStandardInput = $true
        $psi.RedirectStandardOutput = $true
        $psi.RedirectStandardError = $true
        $psi.CreateNoWindow = $true
        
        $process = New-Object System.Diagnostics.Process
        $process.StartInfo = $psi
        $process.Start()
        
        # Sende "1" + Enter für "Copy command to clipboard"
        $process.StandardInput.WriteLine("1")
        $process.StandardInput.Close()
        
        # Lese Ausgabe
        $stdout = $process.StandardOutput.ReadToEnd()
        $stderr = $process.StandardError.ReadToEnd()
        $process.WaitForExit()
        
        $copilotResult = @()
        if ($stdout) { $copilotResult += $stdout -split "`n" }
        if ($stderr) { $copilotResult += $stderr -split "`n" }if ($copilotResult) {
            # Konvertiere Array zu String für besseres Parsing
            $copilotText = $copilotResult -join " "
            
            # Verschiedene Patterns für Commit-Messages
            $patterns = @(
                'git commit -m ["'']([^"'']+)["'']',  # Einfache Message
                'git commit -m ["''](.+?)["''] -m ["''](.+?)["'']',  # Multi-line Message
                '# Suggestion:\s*git commit -m ["'']([^"'']+)["'']'  # Mit Suggestion Header
            )
            
            foreach ($pattern in $patterns) {
                if ($copilotText -match $pattern) {
                    $suggestedMessage = $matches[1]
                    
                    # Fix Encoding-Probleme (ersetze kaputte Emoji-Codes)
                    $suggestedMessage = $suggestedMessage -replace '­ƒÜÇ', '🚀'
                    $suggestedMessage = $suggestedMessage -replace '├ä', 'Ä'
                    $suggestedMessage = $suggestedMessage -replace '├╝', 'ü'
                    $suggestedMessage = $suggestedMessage -replace '├ñ', 'ö'
                    $suggestedMessage = $suggestedMessage -replace 'Ã¤', 'ä'
                    $suggestedMessage = $suggestedMessage -replace 'Ã¼', 'ü'
                    $suggestedMessage = $suggestedMessage -replace 'Ã¶', 'ö'
                    
                    # Falls Multi-line Message, füge zweite Zeile hinzu
                    if ($matches.Count -gt 2 -and $matches[2]) {
                        $secondLine = $matches[2] -replace '├ä', 'Ä' -replace '├╝', 'ü' -replace '├ñ', 'ö'
                        $commitMessage = $suggestedMessage + "`n`n" + $secondLine + "`n`nCo-authored-by: GitHub Copilot <copilot@github.com>"
                    } else {
                        $commitMessage = $suggestedMessage + "`n`nCo-authored-by: GitHub Copilot <copilot@github.com>"
                    }
                    
                    Write-Success "Copilot-Nachricht erhalten: $suggestedMessage"
                    break
                }
            }
            
            # Fallback: Suche nach anderen Commit-Message Patterns
            if (-not $commitMessage) {
                foreach ($line in $copilotResult) {
                    if ($line -match '^[🚀🐛🔧📝🎨♻️].*:.*' -and $line.Length -gt 10 -and $line.Length -lt 150) {
                        $cleanLine = $line -replace '­ƒÜÇ', '🚀' -replace '├ä', 'Ä' -replace '├╝', 'ü'
                        $commitMessage = $cleanLine + "`n`nCo-authored-by: GitHub Copilot <copilot@github.com>"
                        Write-Success "Copilot-Nachricht gefunden: $cleanLine"
                        break
                    }
                }
            }
        }
        
        if (-not $commitMessage) {
            Write-Error "Keine brauchbare Commit-Message von Copilot erhalten"
            Write-Info "Copilot Ausgabe war: $($copilotResult -join '; ')"
            exit 1
        }
        
    } catch {
        Write-Error "GitHub Copilot Fehler: $($_.Exception.Message)"
        exit 1
    }
}

# Fallback: Nur wenn explizit eine Nachricht übergeben wurde
if (-not $commitMessage) {
    if ($Message) {
        $commitMessage = $Message
    } else {
        Write-Error "Keine Commit-Nachricht von Copilot erhalten und keine manuelle Nachricht übergeben!"
        Write-Info "Verwenden Sie -Message 'Ihre Nachricht' oder -SkipCopilot für manuelle Eingabe"
        exit 1
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
