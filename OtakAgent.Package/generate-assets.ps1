# PowerShell script to generate Microsoft Store assets from character images
param(
    [string]$SourceImage = "..\src\OtakAgent.App\Resources\kairu.gif",
    [string]$FallbackIcon = "..\src\OtakAgent.App\Resources\app.ico"
)

# Create Images directory
$ImagesDir = ".\Images"
if (-not (Test-Path $ImagesDir)) {
    New-Item -ItemType Directory -Path $ImagesDir | Out-Null
}

# Function to create store asset image
function Create-StoreAssetImage {
    param(
        [int]$Width,
        [int]$Height,
        [string]$OutputPath,
        [string]$SourceImagePath
    )

    Add-Type -AssemblyName System.Drawing

    # Create new bitmap with specified dimensions
    $bitmap = New-Object System.Drawing.Bitmap $Width, $Height
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias

    # Background color - soft gradient or solid color
    $bgColor = [System.Drawing.Color]::FromArgb(240, 248, 255)  # Alice Blue background
    $graphics.Clear($bgColor)

    try {
        # Load the source image (handle GIF by taking first frame)
        if (Test-Path $SourceImagePath) {
            $sourceImg = [System.Drawing.Image]::FromFile((Resolve-Path $SourceImagePath))

            # Calculate scaling to fit the character nicely
            $scale = [Math]::Min($Width * 0.7 / $sourceImg.Width, $Height * 0.7 / $sourceImg.Height)
            $newWidth = [int]($sourceImg.Width * $scale)
            $newHeight = [int]($sourceImg.Height * $scale)

            # Center the character
            $x = ($Width - $newWidth) / 2
            $y = ($Height - $newHeight) / 2

            # Draw the character
            $destRect = New-Object System.Drawing.Rectangle([int]$x, [int]$y, $newWidth, $newHeight)
            $graphics.DrawImage($sourceImg, $destRect)

            $sourceImg.Dispose()
        } else {
            # Fallback to text if image not found
            $fontSize = [Math]::Min($Width, $Height) * 0.4
            $font = New-Object System.Drawing.Font("Segoe UI", $fontSize, [System.Drawing.FontStyle]::Bold)
            $brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(70, 130, 180))
            $format = New-Object System.Drawing.StringFormat
            $format.Alignment = [System.Drawing.StringAlignment]::Center
            $format.LineAlignment = [System.Drawing.StringAlignment]::Center

            $rect = New-Object System.Drawing.RectangleF(0, 0, $Width, $Height)
            $graphics.DrawString("🐸", $font, $brush, $rect, $format)

            $font.Dispose()
            $brush.Dispose()
        }
    } catch {
        Write-Warning "Could not load source image: $_"
        # Create simple fallback
        $fontSize = [Math]::Min($Width, $Height) * 0.3
        $font = New-Object System.Drawing.Font("Segoe UI", $fontSize, [System.Drawing.FontStyle]::Bold)
        $brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(70, 130, 180))
        $format = New-Object System.Drawing.StringFormat
        $format.Alignment = [System.Drawing.StringAlignment]::Center
        $format.LineAlignment = [System.Drawing.StringAlignment]::Center

        $rect = New-Object System.Drawing.RectangleF(0, 0, $Width, $Height)
        $graphics.DrawString("OA", $font, $brush, $rect, $format)

        $font.Dispose()
        $brush.Dispose()
    }

    # Save image
    $bitmap.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)

    # Cleanup
    $graphics.Dispose()
    $bitmap.Dispose()
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

# Check if source image exists
$resolvedSourcePath = $null
if (Test-Path $SourceImage) {
    $resolvedSourcePath = Resolve-Path $SourceImage
} elseif (Test-Path (Join-Path (Get-Location) $SourceImage)) {
    $resolvedSourcePath = Resolve-Path (Join-Path (Get-Location) $SourceImage)
} else {
    Write-Warning "Source image not found at $SourceImage, using fallback design"
}

# Generate each asset
foreach ($asset in $assets) {
    $outputPath = Join-Path $ImagesDir $asset.Name
    Write-Host "Generating $($asset.Name)..."
    if ($resolvedSourcePath) {
        Create-StoreAssetImage -Width $asset.Width -Height $asset.Height -OutputPath $outputPath -SourceImagePath $resolvedSourcePath
    } else {
        Create-StoreAssetImage -Width $asset.Width -Height $asset.Height -OutputPath $outputPath -SourceImagePath "nonexistent"
    }
}

Write-Host "All assets generated successfully!" -ForegroundColor Green
Write-Host "Assets use the Kairu character from: $SourceImage" -ForegroundColor Cyan