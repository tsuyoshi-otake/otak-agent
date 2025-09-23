# PowerShell script to generate Microsoft Store assets from base icon
param(
    [string]$SourceIcon = "..\src\OtakAgent.App\Resources\IDLE.ico"
)

# Create Images directory
$ImagesDir = ".\Images"
if (-not (Test-Path $ImagesDir)) {
    New-Item -ItemType Directory -Path $ImagesDir | Out-Null
}

# Function to create placeholder image using System.Drawing
function Create-PlaceholderImage {
    param(
        [int]$Width,
        [int]$Height,
        [string]$OutputPath,
        [string]$Text = "OA"
    )

    Add-Type -AssemblyName System.Drawing

    $bitmap = New-Object System.Drawing.Bitmap $Width, $Height
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)

    # Background color (light blue)
    $bgColor = [System.Drawing.Color]::FromArgb(70, 130, 180)
    $graphics.Clear($bgColor)

    # Draw text
    $fontSize = [Math]::Min($Width, $Height) * 0.3
    $font = New-Object System.Drawing.Font("Arial", $fontSize, [System.Drawing.FontStyle]::Bold)
    $brush = [System.Drawing.Brushes]::White
    $format = New-Object System.Drawing.StringFormat
    $format.Alignment = [System.Drawing.StringAlignment]::Center
    $format.LineAlignment = [System.Drawing.StringAlignment]::Center

    $rect = New-Object System.Drawing.RectangleF(0, 0, $Width, $Height)
    $graphics.DrawString($Text, $font, $brush, $rect, $format)

    # Save image
    $bitmap.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)

    # Cleanup
    $graphics.Dispose()
    $bitmap.Dispose()
    $font.Dispose()
}

# Define required assets
$assets = @(
    @{ Name = "Square150x150Logo.png"; Width = 150; Height = 150 },
    @{ Name = "Square44x44Logo.png"; Width = 44; Height = 44 },
    @{ Name = "Square44x44Logo.targetsize-24_altform-unplated.png"; Width = 24; Height = 24 },
    @{ Name = "Wide310x150Logo.png"; Width = 310; Height = 150 },
    @{ Name = "SmallTile.png"; Width = 71; Height = 71 },
    @{ Name = "LargeTile.png"; Width = 310; Height = 310 },
    @{ Name = "StoreLogo.png"; Width = 50; Height = 50 },
    @{ Name = "SplashScreen.png"; Width = 620; Height = 300 }
)

# Generate each asset
foreach ($asset in $assets) {
    $outputPath = Join-Path $ImagesDir $asset.Name
    Write-Host "Generating $($asset.Name)..."
    Create-PlaceholderImage -Width $asset.Width -Height $asset.Height -OutputPath $outputPath
}

Write-Host "All assets generated successfully!" -ForegroundColor Green