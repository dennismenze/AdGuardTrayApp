# 🎯 AdGuard Tray App - Alles-in-einem Development Tool
# DRY: Eine einzige Quelle für alle Git-Operationen mit GitHub Copilot

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
    [switch]$Push = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipCopilot = $false
)

# Import Copilot Helper
. "$PSScriptRoot\scripts\GitCopilotHelper.ps1"

# Hilfsfunktionen
function Write-Header {
    Write-Host @"
╔══════════════════════════════════════════════════════════╗
║          🛡️  AdGuard Tray App Tool Suite                ║
║               DRY Development Tool                      ║
╚══════════════════════════════════════════════════════════╝
"@ -ForegroundColor Blue
}

function Write-Success { param([string]$msg) Write-Host "✅ $msg" -ForegroundColor Green }
function Write-Error { param([string]$msg) Write-Host "❌ $msg" -ForegroundColor Red }
function Write-Warning { param([string]$msg) Write-Host "⚠️  $msg" -ForegroundColor Yellow }
function Write-Info { param([string]$msg) Write-Host "ℹ️  $msg" -ForegroundColor Cyan }

function Invoke-GitCommit {
    param(
        [Parameter(Mandatory=$false)]
        [string]$CustomMessage = "",
        
        [Parameter(Mandatory=$false)]
        [string]$CommitType = "general",
        
        [Parameter(Mandatory=$false)]
        [switch]$AllFiles = $false,
        
        [Parameter(Mandatory=$false)]
        [switch]$SkipCopilot = $false,
        
        [Parameter(Mandatory=$false)]
        [switch]$AutoConfirm = $false
    )
    
    Write-Info "Git Commit wird durchgeführt..."
    
    # Prüfe ob Git-Repository
    if (-not (Test-Path ".git")) {
        Write-Error "Kein Git-Repository gefunden!"
        return $false
    }
    
    # Dateien stagen
    if ($AllFiles) {
        git add .
        Write-Info "Alle Dateien gestaged"
    } else {
        $status = git status --porcelain
        if (-not $status) {
            Write-Warning "Keine Änderungen zum Committen gefunden"
            return $true
        }
        
        Write-Info "Geänderte Dateien:"
        git status --short
        git add .
        Write-Info "Alle Dateien gestaged"
    }
    
    # Commit-Nachricht generieren
    $commitMessage = ""
    
    if ($CustomMessage) {
        $commitMessage = $CustomMessage
    } elseif (-not $SkipCopilot) {        try {
            $prompt = Get-CommitPromptForChanges -CommitType $CommitType
            $copilotMessage = Get-GitCopilotCommitMessage -Prompt $prompt
            $commitMessage = $copilotMessage  # Ohne Co-authored-by
            Write-Success "Copilot-Nachricht erhalten: $copilotMessage"
        } catch {
            Write-Error "GitHub Copilot Fehler: $($_.Exception.Message)"
            Write-Info "Verwenden Sie -Message 'Ihre Nachricht' oder -SkipCopilot"
            return $false
        }
    } else {
        Write-Error "Keine Commit-Nachricht und Copilot übersprungen!"
        return $false
    }
      # Commit-Nachricht anzeigen
    Write-Info "Commit-Nachricht:"
    Write-Host $commitMessage -ForegroundColor White
    
    # Bestätigung (nur wenn nicht automatisch bestätigt)
    if (-not $AutoConfirm) {
        Write-Warning "Committen? (J/N)"
        $confirm = Read-Host
        if ($confirm -notmatch '^[jJyY]') {
            Write-Info "Abgebrochen."
            return $false
        }
    }
    
    # Commit durchführen
    try {
        git commit -m $commitMessage
        Write-Success "Commit erfolgreich erstellt"
        
        if ($Push) {
            $branch = git rev-parse --abbrev-ref HEAD
            git push origin $branch
            Write-Success "Erfolgreich zu Remote gepusht"
        }
        
        return $true
    } catch {
        Write-Error "Fehler beim Committen: $($_.Exception.Message)"
        return $false
    }
}

function Invoke-Release {
    param(
        [Parameter(Mandatory=$false)]
        [ValidateSet("patch", "minor", "major")]
        [string]$VersionType = "patch",
        
        [Parameter(Mandatory=$false)]
        [string]$CustomMessage = "",
        
        [Parameter(Mandatory=$false)]
        [switch]$DryRun = $false,
        
        [Parameter(Mandatory=$false)]
        [switch]$Force = $false,
        
        [Parameter(Mandatory=$false)]
        [switch]$AutoConfirm = $false
    )
    
    Write-Info "Release wird durchgeführt (Version: $VersionType)..."
    
    # Hole aktuelle Version
    $projectFile = "AdGuardTrayApp/AdGuardTrayApp.csproj"
    if (-not (Test-Path $projectFile)) {
        Write-Error "Projektdatei nicht gefunden: $projectFile"
        return $false
    }
    
    $content = Get-Content $projectFile -Raw
    if ($content -match '<Version>(\d+\.\d+\.\d+)</Version>') {
        $currentVersion = $matches[1]
    } else {
        Write-Error "Keine Version in Projektdatei gefunden"
        return $false
    }
    
    Write-Info "Aktuelle Version: $currentVersion"
    
    # Berechne neue Version
    $versionParts = $currentVersion -split '\.'
    switch ($VersionType) {
        "major" { 
            $versionParts[0] = [int]$versionParts[0] + 1
            $versionParts[1] = 0
            $versionParts[2] = 0
        }
        "minor" { 
            $versionParts[1] = [int]$versionParts[1] + 1
            $versionParts[2] = 0
        }
        "patch" { 
            $versionParts[2] = [int]$versionParts[2] + 1
        }
    }
    
    $newVersion = $versionParts -join '.'
    $newTag = "v$newVersion"
    
    Write-Info "Neue Version: $newVersion"
    Write-Info "Neues Tag: $newTag"
    
    if ($DryRun) {
        Write-Warning "DRY RUN - Keine Änderungen werden gemacht"
        return $true
    }
    
    # Release-Zusammenfassung
    Write-Host @"
🚀 Release-Zusammenfassung:
════════════════════════════════════════
ℹ️  Aktuelle Version: $currentVersion
ℹ️  Neue Version: $newVersion
ℹ️  Version Type: $VersionType
ℹ️  Tag: $newTag
ℹ️  Branch: $(git rev-parse --abbrev-ref HEAD)
────────────────────────────────────────
"@ -ForegroundColor Yellow
      if (-not $Force -and -not $AutoConfirm) {
        Write-Warning "Release erstellen? (J/N)"
        $confirm = Read-Host
        if ($confirm -notmatch '^[jJyY]') {
            Write-Info "Abgebrochen."
            return $false
        }
    }
    
    # Aktualisiere Version in Projektdatei
    $newContent = $content -replace '<Version>\d+\.\d+\.\d+</Version>', "<Version>$newVersion</Version>"
    Set-Content $projectFile $newContent -Encoding UTF8
    Write-Success "Version in $projectFile aktualisiert"
    
    # Commit mit GitHub Copilot oder Standard-Message
    if ($CustomMessage) {
        $releaseCommitMessage = $CustomMessage
    } else {
        $releaseCommitMessage = "📦 release: Version $newVersion`n`n- $VersionType bump von $currentVersion auf $newVersion`n- Bereit für Release-Deployment`n- AdGuard Tray App Release"
    }
    
    # Release Commit
    try {
        git add $projectFile
        git commit -m $releaseCommitMessage
        Write-Success "Release-Commit erstellt"
        
        git tag $newTag
        Write-Success "Tag $newTag erstellt"
        
        # Push to remote
        $branch = git rev-parse --abbrev-ref HEAD
        git push origin $branch
        git push origin $newTag
        Write-Success "Erfolgreich zu Remote gepusht"
        
        Write-Success "🎉 Release erfolgreich erstellt!"
        Write-Success "Version $newVersion wurde getaggt und gepusht"
        Write-Success "GitHub Actions wird automatisch das Release erstellen"
        Write-Info "GitHub Actions: https://github.com/dennismenze/AdGuardTrayApp/actions"
        
        return $true
    } catch {
        Write-Error "Fehler beim Release: $($_.Exception.Message)"
        return $false
    }
}

Write-Header

switch ($Action) {
    "setup" {
        Write-Info "Führe Setup aus..."
        & "$PSScriptRoot\scripts\setup.ps1"
    }
    
    "commit" {
        Write-Info "Führe Git Commit aus..."
        $success = Invoke-GitCommit -CustomMessage $Message -AllFiles:$true -SkipCopilot:$SkipCopilot
        if ($success -and $Push) {
            Write-Info "Push wird durchgeführt..."
            $branch = git rev-parse --abbrev-ref HEAD
            git push origin $branch
            Write-Success "Erfolgreich zu Remote gepusht"
        }
    }
    
    "release" {
        Write-Info "Führe Release aus (Version: $Version)..."
        Invoke-Release -VersionType $Version -CustomMessage $Message -DryRun:$DryRun -Force:$Force
    }    "hotfix" {
        Write-Info "Hotfix-Workflow..."
        
        # 1. Commit falls Änderungen vorhanden (zeige Message, aber noch nicht committen)
        $status = git status --porcelain
        $commitMessage = ""
        if ($status) {
            # Generiere Commit-Message und zeige sie an
            if ($Message) {
                $commitMessage = $Message
            } elseif (-not $SkipCopilot) {
                try {
                    # Dateien stagen für Copilot-Analyse
                    git add .
                    Write-Info "Alle Dateien gestaged"
                    
                    $prompt = Get-CommitPromptForChanges -CommitType "general"
                    $copilotMessage = Get-GitCopilotCommitMessage -Prompt $prompt
                    $commitMessage = $copilotMessage
                    Write-Success "Copilot-Nachricht erhalten: $copilotMessage"
                } catch {
                    Write-Error "GitHub Copilot Fehler: $($_.Exception.Message)"
                    Write-Info "Verwenden Sie -Message 'Ihre Nachricht' oder -SkipCopilot"
                    exit 1
                }
            } else {
                Write-Error "Keine Commit-Nachricht und Copilot übersprungen!"
                exit 1
            }
            
            Write-Info "Commit-Nachricht:"
            Write-Host $commitMessage -ForegroundColor White
        }
        
        # 2. Hole Release-Informationen und zeige Zusammenfassung
        $projectFile = "AdGuardTrayApp/AdGuardTrayApp.csproj"
        $content = Get-Content $projectFile -Raw
        if ($content -match '<Version>(\d+\.\d+\.\d+)</Version>') {
            $currentVersion = $matches[1]
        } else {
            Write-Error "Keine Version in Projektdatei gefunden"
            exit 1
        }
        
        $versionParts = $currentVersion -split '\.'
        $versionParts[2] = [int]$versionParts[2] + 1
        $newVersion = $versionParts -join '.'
        $newTag = "v$newVersion"
        
        # 3. Zeige komplette Zusammenfassung
        Write-Host @"
🚀 Hotfix-Zusammenfassung:
════════════════════════════════════════
ℹ️  Aktuelle Version: $currentVersion
ℹ️  Neue Version: $newVersion
ℹ️  Version Type: patch
ℹ️  Tag: $newTag
ℹ️  Branch: $(git rev-parse --abbrev-ref HEAD)
────────────────────────────────────────
"@ -ForegroundColor Yellow
          # 4. JETZT die kombinierte Bestätigung
        if (-not $Force -and -not $DryRun) {
            Write-Warning "Committen und Hotfix-Release erstellen? (J/N) [Standard: J]"
            $confirm = Read-Host
            if ($confirm -eq "" -or $confirm -match '^[jJyY]') {
                # Continue - Standard ist Ja (Enter = Ja)
            } else {
                Write-Info "Abgebrochen."
                exit 0
            }
        }
        
        if ($DryRun) {
            Write-Warning "DRY RUN - Keine Änderungen würden gemacht"
            exit 0
        }
        
        # 5. Jetzt alles ausführen (automatisch)
        if ($status) {
            try {
                git commit -m $commitMessage
                Write-Success "Commit erfolgreich erstellt"
            } catch {
                Write-Error "Fehler beim Committen: $($_.Exception.Message)"
                exit 1
            }
        }
          # 6. Release erstellen (automatisch, ohne nochmalige Version-Anzeige)
        $newContent = $content -replace '<Version>\d+\.\d+\.\d+</Version>', "<Version>$newVersion</Version>"
        Set-Content $projectFile $newContent -Encoding UTF8
        Write-Success "Version in $projectFile aktualisiert"
        
        # Release Commit und Tag
        try {
            git add $projectFile
            git commit -m "📦 release: Version $newVersion`n`n- patch bump von $currentVersion auf $newVersion`n- Bereit für Release-Deployment`n- AdGuard Tray App Release"
            Write-Success "Release-Commit erstellt"
            
            git tag $newTag
            Write-Success "Tag $newTag erstellt"
            
            # Push to remote
            $branch = git rev-parse --abbrev-ref HEAD
            git push origin $branch
            git push origin $newTag
            Write-Success "Erfolgreich zu Remote gepusht"
            
            Write-Success "🎉 Release erfolgreich erstellt!"
            Write-Success "Version $newVersion wurde getaggt und gepusht"
            Write-Success "GitHub Actions wird automatisch das Release erstellen"
            Write-Info "GitHub Actions: https://github.com/dennismenze/AdGuardTrayApp/actions"
            
        } catch {
            Write-Error "Fehler beim Release: $($_.Exception.Message)"
            exit 1
        }
    }
      "feature" {
        Write-Info "Feature-Workflow..."
        
        # 1. Commit falls Änderungen vorhanden (zeige Message, aber noch nicht committen)
        $status = git status --porcelain
        $commitMessage = ""
        if ($status) {
            # Generiere Commit-Message und zeige sie an
            if ($Message) {
                $commitMessage = $Message
            } elseif (-not $SkipCopilot) {
                try {
                    # Dateien stagen für Copilot-Analyse
                    git add .
                    Write-Info "Alle Dateien gestaged"
                    
                    $prompt = Get-CommitPromptForChanges -CommitType "general"
                    $copilotMessage = Get-GitCopilotCommitMessage -Prompt $prompt
                    $commitMessage = $copilotMessage
                    Write-Success "Copilot-Nachricht erhalten: $copilotMessage"
                } catch {
                    Write-Error "GitHub Copilot Fehler: $($_.Exception.Message)"
                    Write-Info "Verwenden Sie -Message 'Ihre Nachricht' oder -SkipCopilot"
                    exit 1
                }
            } else {
                Write-Error "Keine Commit-Nachricht und Copilot übersprungen!"
                exit 1
            }
            
            Write-Info "Commit-Nachricht:"
            Write-Host $commitMessage -ForegroundColor White
        }
        
        # 2. Hole Release-Informationen und zeige Zusammenfassung
        $projectFile = "AdGuardTrayApp/AdGuardTrayApp.csproj"
        $content = Get-Content $projectFile -Raw
        if ($content -match '<Version>(\d+\.\d+\.\d+)</Version>') {
            $currentVersion = $matches[1]
        } else {
            Write-Error "Keine Version in Projektdatei gefunden"
            exit 1
        }
        
        $versionParts = $currentVersion -split '\.'
        $versionParts[1] = [int]$versionParts[1] + 1
        $versionParts[2] = 0
        $newVersion = $versionParts -join '.'
        $newTag = "v$newVersion"
        
        # 3. Zeige komplette Zusammenfassung
        Write-Host @"
🚀 Feature-Zusammenfassung:
════════════════════════════════════════
ℹ️  Aktuelle Version: $currentVersion
ℹ️  Neue Version: $newVersion
ℹ️  Version Type: minor
ℹ️  Tag: $newTag
ℹ️  Branch: $(git rev-parse --abbrev-ref HEAD)
────────────────────────────────────────
"@ -ForegroundColor Yellow
          # 4. JETZT die kombinierte Bestätigung
        if (-not $Force -and -not $DryRun) {
            Write-Warning "Committen und Feature-Release erstellen? (J/N) [Standard: J]"
            $confirm = Read-Host
            if ($confirm -eq "" -or $confirm -match '^[jJyY]') {
                # Continue - Standard ist Ja (Enter = Ja)
            } else {
                Write-Info "Abgebrochen."
                exit 0
            }
        }
        
        if ($DryRun) {
            Write-Warning "DRY RUN - Keine Änderungen würden gemacht"
            exit 0
        }
        
        # 5. Jetzt alles ausführen (automatisch)
        if ($status) {
            try {
                git commit -m $commitMessage
                Write-Success "Commit erfolgreich erstellt"
            } catch {
                Write-Error "Fehler beim Committen: $($_.Exception.Message)"
                exit 1
            }
        }
          # 6. Release erstellen (automatisch, ohne nochmalige Version-Anzeige)
        $newContent = $content -replace '<Version>\d+\.\d+\.\d+</Version>', "<Version>$newVersion</Version>"
        Set-Content $projectFile $newContent -Encoding UTF8
        Write-Success "Version in $projectFile aktualisiert"
        
        # Release Commit und Tag
        try {
            git add $projectFile
            git commit -m "📦 release: Version $newVersion`n`n- minor bump von $currentVersion auf $newVersion`n- Bereit für Release-Deployment`n- AdGuard Tray App Release"
            Write-Success "Release-Commit erstellt"
            
            git tag $newTag
            Write-Success "Tag $newTag erstellt"
            
            # Push to remote
            $branch = git rev-parse --abbrev-ref HEAD
            git push origin $branch
            git push origin $newTag
            Write-Success "Erfolgreich zu Remote gepusht"
            
            Write-Success "🎉 Release erfolgreich erstellt!"
            Write-Success "Version $newVersion wurde getaggt und gepusht"
            Write-Success "GitHub Actions wird automatisch das Release erstellen"
            Write-Info "GitHub Actions: https://github.com/dennismenze/AdGuardTrayApp/actions"
            
        } catch {
            Write-Error "Fehler beim Release: $($_.Exception.Message)"
            exit 1
        }
    }"status" {
        Write-Info "Git Status..."
        git status
        Write-Info "Git Log (letzte 5 Commits)..."
        git log --oneline -5
        Write-Info "Git Tags (letzte 5)..."
        git tag --sort=-version:refname | Select-Object -First 5
    }
}

Write-Success "✨ $Action abgeschlossen!"
