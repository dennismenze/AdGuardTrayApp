# Build script for local development and testing
param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [switch]$AllPlatforms,
    [switch]$CreateMSI
)

$projectPath = "AdGuardTrayApp/AdGuardTrayApp.csproj"
$outputDir = "builds"

# Clean previous builds
if (Test-Path $outputDir) {
    Remove-Item -Recurse -Force $outputDir
}
New-Item -ItemType Directory -Path $outputDir | Out-Null

if ($AllPlatforms) {
    $runtimes = @(
        @{ Runtime = "win-x64"; Arch = "x64" },
        @{ Runtime = "win-x86"; Arch = "x86" },
        @{ Runtime = "win-arm64"; Arch = "arm64" }
    )
} else {
    $arch = if ($Runtime -eq "win-x64") { "x64" } elseif ($Runtime -eq "win-x86") { "x86" } else { "arm64" }
    $runtimes = @(@{ Runtime = $Runtime; Arch = $arch })
}

foreach ($rtInfo in $runtimes) {
    $rt = $rtInfo.Runtime
    $arch = $rtInfo.Arch
    Write-Host "Building for $rt ($arch)..." -ForegroundColor Green
    
    $buildOutput = "$outputDir/$rt"
    
    # Build and publish
    dotnet publish $projectPath `
        --configuration $Configuration `
        --runtime $rt `
        --self-contained true `
        --output $buildOutput `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:PublishTrimmed=false
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Build for $rt completed successfully" -ForegroundColor Green
        
        # Create ZIP archive
        $zipName = "AdGuardTrayApp-windows-$arch"
        $zipPath = "$outputDir/$zipName.zip"
        Compress-Archive -Path "$buildOutput/*" -DestinationPath $zipPath -Force
        Write-Host "📦 ZIP Archive created: $zipPath" -ForegroundColor Blue
        
        # Create MSI installer if requested
        if ($CreateMSI) {
            Write-Host "🔨 Creating MSI installer for $arch..." -ForegroundColor Yellow
            
            # Check if WiX is available
            try {
                wix --version | Out-Null
                
                # Prepare installer directory
                $installerBuild = "./installer-build/$rt"
                New-Item -ItemType Directory -Force -Path $installerBuild | Out-Null
                Copy-Item "$buildOutput/AdGuardTrayApp.exe" $installerBuild
                
                # Build MSI
                $msiPath = "$outputDir/AdGuardTrayApp-windows-$arch.msi"
                wix build Installer/AdGuardTrayApp.wxs -arch $arch -out $msiPath -d "SourceDir=$installerBuild"
                
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "📦 MSI Installer created: $msiPath" -ForegroundColor Blue
                } else {
                    Write-Host "❌ MSI creation failed for $rt" -ForegroundColor Red
                }
            }
            catch {
                Write-Host "⚠️ WiX Toolset not found. Install with: dotnet tool install --global wix" -ForegroundColor Yellow
            }
        }
    } else {
        Write-Host "❌ Build for $rt failed" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Build summary:" -ForegroundColor Yellow
Get-ChildItem $outputDir -Name | ForEach-Object { 
    Write-Host "  📁 $_" -ForegroundColor Cyan 
}
