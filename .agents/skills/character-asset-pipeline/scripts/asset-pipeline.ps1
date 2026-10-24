[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [ValidateSet('inspect', 'preview', 'compare', 'export', 'outline', 'official-poses', 'tile-pair', 'ui-preview')]
    [string]$Command,

    [string]$Path,
    [string]$Source,
    [string]$Target,
    [string]$OutPath,
    [string]$LockedTarget,
    [string]$BackgroundColor,
    [string]$LockedBackgroundColor,
    [string]$LayoutPath = (Join-Path $PSScriptRoot 'character-select-layout.json'),
    # Legacy compatibility: inspect now always returns a PSCustomObject.
    [switch]$AsObject,
    [int]$Width = 0,
    [int]$Height = 0,
    [int]$DisplayWidth = 0,
    [int]$DisplayHeight = 0,
    [ValidateSet('Fit', 'Cover', 'Stretch')]
    [string]$CropMode = 'Fit',
    [int]$Radius = 2,
    [string]$Color = '#FFFFFF',
    [string[]]$Paths,
    [string[]]$Labels,
    [ValidateSet('SharedScale', 'IndividualFit', 'SubjectFit')]
    [string]$CompareMode = 'SharedScale',
    [string]$GodotPath,
    [string]$GamePath = 'C:/Program Files (x86)/Steam/steamapps/common/Slay the Spire 2'
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

function Get-AlphaBytes([System.Drawing.Bitmap]$bmp) {
    $rect = New-Object System.Drawing.Rectangle(0, 0, $bmp.Width, $bmp.Height)
    $data = $bmp.LockBits($rect, [System.Drawing.Imaging.ImageLockMode]::ReadOnly, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        $bytes = New-Object byte[] ($data.Stride * $bmp.Height)
        [System.Runtime.InteropServices.Marshal]::Copy($data.Scan0, $bytes, 0, $bytes.Length)
        return [pscustomobject]@{ Bytes = $bytes; Stride = $data.Stride }
    } finally {
        $bmp.UnlockBits($data)
    }
}

function Get-AlphaBounds([System.Drawing.Bitmap]$bmp, [int]$alphaThreshold = 8) {
    $alpha = Get-AlphaBytes $bmp
    $bytes = $alpha.Bytes
    $stride = $alpha.Stride
    $minX = $bmp.Width
    $minY = $bmp.Height
    $maxX = -1
    $maxY = -1
    $nonZero = 0
    for ($y = 0; $y -lt $bmp.Height; $y++) {
        $row = $y * $stride
        $rowMin = -1
        $rowMax = -1
        for ($x = 0; $x -lt $bmp.Width; $x++) {
            # Format32bppArgb is stored as BGRA; alpha is the fourth byte.
            if ($bytes[$row + ($x * 4) + 3] -gt $alphaThreshold) {
                $nonZero++
                if ($rowMin -lt 0) { $rowMin = $x }
                $rowMax = $x
            }
        }
        if ($rowMin -ge 0) {
            if ($rowMin -lt $minX) { $minX = $rowMin }
            if ($rowMax -gt $maxX) { $maxX = $rowMax }
            if ($y -lt $minY) { $minY = $y }
            $maxY = $y
        }
    }
    $corners = @(
        $bmp.GetPixel(0, 0).A,
        $bmp.GetPixel($bmp.Width - 1, 0).A,
        $bmp.GetPixel(0, $bmp.Height - 1).A,
        $bmp.GetPixel($bmp.Width - 1, $bmp.Height - 1).A
    )
    $total = $bmp.Width * $bmp.Height
    $bboxW = if ($maxX -ge $minX) { $maxX - $minX + 1 } else { 0 }
    $bboxH = if ($maxY -ge $minY) { $maxY - $minY + 1 } else { 0 }
    $margins = if ($bboxW -gt 0) {
        [pscustomobject]@{ Left = $minX; Top = $minY; Right = $bmp.Width - 1 - $maxX; Bottom = $bmp.Height - 1 - $maxY }
    } else { $null }
    # Horizontal margins are relative to width and vertical margins to height.
    $minMarginRatio = if ($margins) {
        [math]::Round(([math]::Min([math]::Min($margins.Left, $margins.Right) / [double]$bmp.Width,
                [math]::Min($margins.Top, $margins.Bottom) / [double]$bmp.Height)) * 100, 1)
    } else { $null }
    return [pscustomobject]@{
        Width            = $bmp.Width
        Height           = $bmp.Height
        PixelFormat      = $bmp.PixelFormat.ToString()
        CornerAlpha      = ($corners -join ',')
        CornersTransparent = (($corners | Measure-Object -Maximum).Maximum -eq 0)
        BoundingBox      = "${minX},${minY} -> ${maxX},${maxY} (${bboxW}x${bboxH})"
        BBoxX            = $minX
        BBoxY            = $minY
        BBoxWidth        = $bboxW
        BBoxHeight       = $bboxH
        BBoxWidthRatio   = [math]::Round(($bboxW / [double]$bmp.Width) * 100, 1)
        BBoxHeightRatio  = [math]::Round(($bboxH / [double]$bmp.Height) * 100, 1)
        MarginsPx        = $margins
        MinMarginRatio   = $minMarginRatio
        OpaquePixelRatio = [math]::Round(($nonZero / [double]$total) * 100, 1)
    }
}

function New-ResizedBitmap([System.Drawing.Bitmap]$src, [int]$targetW, [int]$targetH, [string]$mode) {
    $dst = New-Object System.Drawing.Bitmap($targetW, $targetH, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($dst)
    try {
        $g.Clear([System.Drawing.Color]::Transparent)
        $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
        $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
        $g.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality

        if ($mode -eq 'Stretch') {
            $g.DrawImage($src, (New-Object System.Drawing.Rectangle(0, 0, $targetW, $targetH)))
        } elseif ($mode -eq 'Fit') {
            $scale = [math]::Min($targetW / [double]$src.Width, $targetH / [double]$src.Height)
            $drawW = [int][math]::Round($src.Width * $scale)
            $drawH = [int][math]::Round($src.Height * $scale)
            $posX = [int][math]::Floor(($targetW - $drawW) / 2.0)
            $posY = [int][math]::Floor(($targetH - $drawH) / 2.0)
            $g.DrawImage($src, (New-Object System.Drawing.Rectangle($posX, $posY, $drawW, $drawH)))
        } else {
            $scale = [math]::Max($targetW / [double]$src.Width, $targetH / [double]$src.Height)
            $cropW = [int][math]::Round($targetW / $scale)
            $cropH = [int][math]::Round($targetH / $scale)
            $srcX = [int][math]::Floor(($src.Width - $cropW) / 2.0)
            $srcY = [int][math]::Floor(($src.Height - $cropH) / 2.0)
            $destRect = New-Object System.Drawing.Rectangle(0, 0, $targetW, $targetH)
            $g.DrawImage($src, $destRect, $srcX, $srcY, $cropW, $cropH, [System.Drawing.GraphicsUnit]::Pixel)
        }
    } finally {
        $g.Dispose()
    }
    return $dst
}

switch ($Command) {
    'tile-pair' {
        if (-not $Source -or -not $Target -or -not $LockedTarget -or $Width -le 0 -or $Height -le 0 -or -not $BackgroundColor -or -not $LockedBackgroundColor) {
            throw 'Specify -Source -Target -LockedTarget -Width -Height -BackgroundColor -LockedBackgroundColor.'
        }
        if ([IO.Path]::GetFullPath($Target) -eq [IO.Path]::GetFullPath($LockedTarget)) { throw 'Normal and locked targets must differ.' }
        $src = [Drawing.Bitmap]::FromFile((Resolve-Path -LiteralPath $Source).Path)
        try { $alpha = New-ResizedBitmap $src $Width $Height 'Fit' } finally { $src.Dispose() }
        try {
            foreach ($locked in @($false, $true)) {
                $output = if ($locked) { $LockedTarget } else { $Target }
                $parent = Split-Path -Parent $output
                if ($parent) { New-Item -ItemType Directory -Force -Path $parent | Out-Null }
                $canvas = New-Object Drawing.Bitmap $Width,$Height
                $g = [Drawing.Graphics]::FromImage($canvas)
                try {
                    $bg = if ($locked) { $LockedBackgroundColor } else { $BackgroundColor }
                    $g.Clear([Drawing.ColorTranslator]::FromHtml($bg))
                    if ($locked) {
                        # Preserve the exact resized alpha; only RGB changes for the silhouette.
                        for ($y = 0; $y -lt $Height; $y++) { for ($x = 0; $x -lt $Width; $x++) {
                            $alpha.SetPixel($x,$y,[Drawing.Color]::FromArgb($alpha.GetPixel($x,$y).A,0,0,0))
                        }}
                    }
                    $g.DrawImageUnscaled($alpha,0,0)
                    $canvas.Save($output,[Drawing.Imaging.ImageFormat]::Png)
                } finally { $g.Dispose(); $canvas.Dispose() }
            }
        } finally { $alpha.Dispose() }
    }

    'ui-preview' {
        if (-not $Source -or -not $OutPath -or -not $LayoutPath) { throw 'Specify -Source -OutPath -LayoutPath <json>.' }
        # Layout coordinates belong to a supplied screen observation, not character-specific code.
        $layout = Get-Content -Raw -Encoding UTF8 -LiteralPath $LayoutPath | ConvertFrom-Json
        if ($layout.width -le 0 -or $layout.height -le 0 -or -not $layout.regions) { throw 'Layout needs positive width/height and regions.' }
        $src = [Drawing.Bitmap]::FromFile((Resolve-Path -LiteralPath $Source).Path)
        try { $canvas = New-ResizedBitmap $src $layout.width $layout.height 'Fit' } finally { $src.Dispose() }
        $g = [Drawing.Graphics]::FromImage($canvas)
        $panel = New-Object Drawing.SolidBrush ([Drawing.Color]::FromArgb(170,15,14,25))
        $font = New-Object Drawing.Font 'Malgun Gothic',18
        try {
            foreach ($region in $layout.regions) {
                if ($region.width -le 0 -or $region.height -le 0) { throw 'Each region needs positive width/height.' }
                $g.FillRectangle($panel,[int]$region.x,[int]$region.y,[int]$region.width,[int]$region.height)
                $g.DrawRectangle([Drawing.Pens]::Gold,[int]$region.x,[int]$region.y,[int]$region.width,[int]$region.height)
                $g.DrawString([string]$region.label,$font,[Drawing.Brushes]::WhiteSmoke,[float]($region.x+8),[float]($region.y+8))
            }
            $g.DrawString('UI 배치 확인용 합성 · 실제 게임 화면 아님',$font,[Drawing.Brushes]::WhiteSmoke,20,20)
            $parent = Split-Path -Parent $OutPath
            if ($parent) { New-Item -ItemType Directory -Force -Path $parent | Out-Null }
            $canvas.Save($OutPath,[Drawing.Imaging.ImageFormat]::Png)
        } finally { $font.Dispose(); $panel.Dispose(); $g.Dispose(); $canvas.Dispose() }
    }

    'compare' {
        if ($Paths.Count -lt 2 -or -not $OutPath -or $DisplayWidth -le 0 -or $DisplayHeight -le 0) {
            throw 'Specify -Paths @(<png>, <png>, ...) -OutPath <png> -DisplayWidth <int> -DisplayHeight <int> [-Labels @(<label>, ...)].'
        }
        if ($Labels.Count -gt 0 -and $Labels.Count -ne $Paths.Count) {
            throw 'Labels must have one entry per input image, or be omitted.'
        }
        $resolvedSources = @($Paths | ForEach-Object { (Resolve-Path -LiteralPath $_).Path })
        $resolvedOutput = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutPath)
        if ($resolvedSources -contains $resolvedOutput) {
            throw 'The comparison output must be separate from every input image.'
        }

        $images = @()
        $sheet = $null
        $g = $null
        $labelFont = $null
        $metaFont = $null
        $textFormat = $null
        $lightBrush = $null
        try {
            for ($i = 0; $i -lt $resolvedSources.Count; $i++) {
                $bitmap = New-Object System.Drawing.Bitmap($resolvedSources[$i])
                $label = if ($Labels.Count -gt 0) { $Labels[$i] } else { [System.IO.Path]::GetFileName($resolvedSources[$i]) }
                # Official Spine renders carry a soft ground shadow below alpha 128; measure the body above it.
                $bounds = if ($CompareMode -eq 'SubjectFit') { Get-AlphaBounds $bitmap 128 } else { $null }
                if ($bounds -and $bounds.BBoxWidth -eq 0) {
                    throw "SubjectFit found no visible pixels in $($resolvedSources[$i])."
                }
                $images += [pscustomobject]@{ Path = $resolvedSources[$i]; Label = $label; Bitmap = $bitmap; Bounds = $bounds }
            }

            # SharedScale preserves relative canvas sizes; IndividualFit fits each full canvas;
            # SubjectFit fits each alpha bounding box and aligns its bottom to one baseline.
            $maxWidth = ($images | ForEach-Object { $_.Bitmap.Width } | Measure-Object -Maximum).Maximum
            $maxHeight = ($images | ForEach-Object { $_.Bitmap.Height } | Measure-Object -Maximum).Maximum
            $scale = [math]::Min($DisplayWidth / [double]$maxWidth, $DisplayHeight / [double]$maxHeight)
            $pad = 24
            $headerHeight = 84
            $cellWidth = [math]::Max(160, $DisplayWidth)
            if ($CompareMode -eq 'SubjectFit') {
                # Every subject gets the same drawn height; cells widen for wide poses instead of shrinking them.
                $subjectHeight = $DisplayHeight * 0.9
                $widest = ($images | ForEach-Object { $_.Bounds.BBoxWidth * $subjectHeight / $_.Bounds.BBoxHeight } | Measure-Object -Maximum).Maximum
                $cellWidth = [math]::Max($cellWidth, [int][math]::Ceiling($widest / 0.9))
            }
            $sheetW = ($cellWidth * $images.Count) + ($pad * ($images.Count + 1))
            $sheetH = ($DisplayHeight * 2) + $headerHeight + ($pad * 3)
            $sheet = New-Object System.Drawing.Bitmap($sheetW, $sheetH, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
            $g = [System.Drawing.Graphics]::FromImage($sheet)
            $g.Clear([System.Drawing.ColorTranslator]::FromHtml('#1B1924'))
            $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
            $g.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
            $labelFont = New-Object System.Drawing.Font('Malgun Gothic', 14, [System.Drawing.FontStyle]::Bold, [System.Drawing.GraphicsUnit]::Pixel)
            $metaFont = New-Object System.Drawing.Font('Malgun Gothic', 11, [System.Drawing.FontStyle]::Regular, [System.Drawing.GraphicsUnit]::Pixel)
            $textFormat = New-Object System.Drawing.StringFormat
            $textFormat.Alignment = [System.Drawing.StringAlignment]::Center
            $textFormat.LineAlignment = [System.Drawing.StringAlignment]::Center
            $textFormat.Trimming = [System.Drawing.StringTrimming]::EllipsisCharacter
            $lightBrush = New-Object System.Drawing.SolidBrush([System.Drawing.ColorTranslator]::FromHtml('#E8E2D5'))

            $placements = for ($i = 0; $i -lt $images.Count; $i++) {
                $entry = $images[$i]
                $bounds = $entry.Bounds
                $imageScale = switch ($CompareMode) {
                    'IndividualFit' { [math]::Min($DisplayWidth / [double]$entry.Bitmap.Width, $DisplayHeight / [double]$entry.Bitmap.Height) }
                    'SubjectFit' { $subjectHeight / $bounds.BBoxHeight }
                    default { $scale }
                }
                $left = $pad + ($i * ($cellWidth + $pad))
                $darkTop = $pad + $headerHeight
                $lightTop = $darkTop + $DisplayHeight + $pad
                $labelRect = New-Object System.Drawing.RectangleF($left, $pad, $cellWidth, 40)
                $g.DrawString($entry.Label, $labelFont, [System.Drawing.Brushes]::White, $labelRect, $textFormat)
                $metaRect = New-Object System.Drawing.RectangleF($left, ($pad + 42), $cellWidth, 38)
                $modeLabel = switch ($CompareMode) { 'IndividualFit' { '개별 Fit' } 'SubjectFit' { '인물 Fit' } default { '공통 배율' } }
                $metadata = "원본 {0} x {1}`n{2}: x{3:0.####}" -f $entry.Bitmap.Width, $entry.Bitmap.Height, $modeLabel, $imageScale
                $g.DrawString($metadata, $metaFont, [System.Drawing.Brushes]::LightGray, $metaRect, $textFormat)
                $g.FillRectangle($lightBrush, $left, $lightTop, $cellWidth, $DisplayHeight)

                $drawW = [math]::Max(1, [int][math]::Round($entry.Bitmap.Width * $imageScale))
                $drawH = [math]::Max(1, [int][math]::Round($entry.Bitmap.Height * $imageScale))
                if ($CompareMode -eq 'SubjectFit') {
                    # Center the subject horizontally and place its lowest visible pixel 5% above the cell bottom.
                    $x = $left + [int][math]::Round(($cellWidth / 2.0) - (($bounds.BBoxX + ($bounds.BBoxWidth / 2.0)) * $imageScale))
                    $yOffset = [int][math]::Round(($DisplayHeight * 0.95) - (($bounds.BBoxY + $bounds.BBoxHeight) * $imageScale))
                } else {
                    $x = $left + [int][math]::Floor(($cellWidth - $drawW) / 2.0)
                    $yOffset = [int][math]::Floor(($DisplayHeight - $drawH) / 2.0)
                }
                foreach ($top in @($darkTop, $lightTop)) {
                    # Only SubjectFit can extend transparent canvas beyond the cell; the subject stays inside.
                    $g.SetClip((New-Object System.Drawing.Rectangle($left, $top, $cellWidth, $DisplayHeight)))
                    $destination = New-Object System.Drawing.Rectangle($x, ($top + $yOffset), $drawW, $drawH)
                    $g.DrawImage($entry.Bitmap, $destination)
                    $g.ResetClip()
                }
                [pscustomobject]@{
                    Path = $entry.Path
                    Label = $entry.Label
                    SourceWidth = $entry.Bitmap.Width
                    SourceHeight = $entry.Bitmap.Height
                    Scale = $imageScale
                    DrawWidth = $drawW
                    DrawHeight = $drawH
                    SubjectBox = $(if ($bounds) { $bounds.BoundingBox } else { $null })
                    SubjectDrawHeight = $(if ($bounds) { [int][math]::Round($bounds.BBoxHeight * $imageScale) } else { $null })
                    SubjectIsFullCanvas = $(if ($bounds) { $bounds.BBoxWidth -eq $entry.Bitmap.Width -and $bounds.BBoxHeight -eq $entry.Bitmap.Height } else { $null })
                }
            }
            $outDir = Split-Path -Parent $resolvedOutput
            if (-not (Test-Path -LiteralPath $outDir)) {
                New-Item -ItemType Directory -Path $outDir -Force | Out-Null
            }
            $sheet.Save($resolvedOutput, [System.Drawing.Imaging.ImageFormat]::Png)
            [pscustomobject]@{
                OutputPath = $resolvedOutput
                CompareMode = $CompareMode
                Scale = $(if ($CompareMode -eq 'SharedScale') { $scale } else { $null })
                DisplayWidth = $DisplayWidth
                DisplayHeight = $DisplayHeight
                SheetWidth = $sheetW
                SheetHeight = $sheetH
                Images = @($placements)
            }
        } finally {
            foreach ($resource in @($textFormat, $lightBrush, $metaFont, $labelFont, $g, $sheet)) {
                if ($null -ne $resource) { $resource.Dispose() }
            }
            foreach ($entry in $images) { $entry.Bitmap.Dispose() }
        }
    }

    'inspect' {
        if (-not $Path) { throw 'Specify -Path <png>.' }
        $resolved = (Resolve-Path -LiteralPath $Path).Path
        $bmp = New-Object System.Drawing.Bitmap($resolved)
        try {
            $info = Get-AlphaBounds $bmp
            $info
        } finally {
            $bmp.Dispose()
        }
    }

    'export' {
        if (-not $Source -or -not $Target -or $Width -le 0 -or $Height -le 0) {
            throw 'Specify -Source <png> -Target <png> -Width <int> -Height <int> [-CropMode Fit|Cover|Stretch] [-BackgroundColor <hex>].'
        }
        $resolvedSrc = (Resolve-Path -LiteralPath $Source).Path
        $targetDir = Split-Path -Parent $Target
        if ($targetDir -and -not (Test-Path -LiteralPath $targetDir)) {
            New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
        }
        $srcBmp = New-Object System.Drawing.Bitmap($resolvedSrc)
        try {
            $dstBmp = New-ResizedBitmap $srcBmp $Width $Height $CropMode
            try {
                if ($BackgroundColor) {
                    $backing = New-Object System.Drawing.Bitmap($Width, $Height)
                    $graphics = [System.Drawing.Graphics]::FromImage($backing)
                    try {
                        $graphics.Clear([System.Drawing.ColorTranslator]::FromHtml($BackgroundColor))
                        $graphics.DrawImageUnscaled($dstBmp, 0, 0)
                        $backing.Save($Target, [System.Drawing.Imaging.ImageFormat]::Png)
                    } finally { $graphics.Dispose(); $backing.Dispose() }
                } else {
                    $dstBmp.Save($Target, [System.Drawing.Imaging.ImageFormat]::Png)
                }
                Write-Host "Exported: $Target (${Width}x${Height}, Mode=$CropMode)"
            } finally {
                $dstBmp.Dispose()
            }
        } finally {
            $srcBmp.Dispose()
        }
    }

    'outline' {
        if (-not $Source -or -not $Target) {
            throw 'Specify -Source <png> -Target <png> [-Radius 2] [-Color "#FFFFFF"].'
        }
        $resolvedSrc = (Resolve-Path -LiteralPath $Source).Path
        $targetDir = Split-Path -Parent $Target
        if ($targetDir -and -not (Test-Path -LiteralPath $targetDir)) {
            New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
        }
        $parsedColor = [System.Drawing.ColorTranslator]::FromHtml($Color)
        $srcBmp = New-Object System.Drawing.Bitmap($resolvedSrc)
        try {
            $w = $srcBmp.Width
            $h = $srcBmp.Height
            $outBmp = New-Object System.Drawing.Bitmap($w, $h, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
            try {
                for ($y = 0; $y -lt $h; $y++) {
                    for ($x = 0; $x -lt $w; $x++) {
                        $maxA = 0
                        for ($dy = -$Radius; $dy -le $Radius; $dy++) {
                            $ny = $y + $dy
                            if ($ny -lt 0 -or $ny -ge $h) { continue }
                            for ($dx = -$Radius; $dx -le $Radius; $dx++) {
                                if (($dx * $dx + $dy * $dy) -gt ($Radius * $Radius + 1)) { continue }
                                $nx = $x + $dx
                                if ($nx -lt 0 -or $nx -ge $w) { continue }
                                $a = $srcBmp.GetPixel($nx, $ny).A
                                if ($a -gt $maxA) { $maxA = $a }
                            }
                        }
                        if ($maxA -gt 0) {
                            $outBmp.SetPixel($x, $y, [System.Drawing.Color]::FromArgb($maxA, $parsedColor.R, $parsedColor.G, $parsedColor.B))
                        } else {
                            $outBmp.SetPixel($x, $y, [System.Drawing.Color]::Transparent)
                        }
                    }
                }
                $outBmp.Save($Target, [System.Drawing.Imaging.ImageFormat]::Png)
                Write-Host "Generated outline: $Target (${w}x${h}, Radius=${Radius}px, Color=$Color)"
            } finally {
                $outBmp.Dispose()
            }
        } finally {
            $srcBmp.Dispose()
        }
    }

    'preview' {
        if (-not $Path -or -not $OutPath) {
            throw 'Specify -Path <png> -OutPath <png> [-DisplayWidth <int>] [-DisplayHeight <int>].'
        }
        $resolvedSrc = (Resolve-Path -LiteralPath $Path).Path
        $outDir = Split-Path -Parent $OutPath
        if ($outDir -and -not (Test-Path -LiteralPath $outDir)) {
            New-Item -ItemType Directory -Path $outDir -Force | Out-Null
        }
        $srcBmp = New-Object System.Drawing.Bitmap($resolvedSrc)
        try {
            $dw = if ($DisplayWidth -gt 0) { $DisplayWidth } else { $srcBmp.Width }
            $dh = if ($DisplayHeight -gt 0) { $DisplayHeight } else { $srcBmp.Height }
            $pad = 24
            $sheetW = ($dw * 2) + ($pad * 3)
            $sheetH = $dh + ($pad * 2)
            $sheet = New-Object System.Drawing.Bitmap($sheetW, $sheetH, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
            $g = [System.Drawing.Graphics]::FromImage($sheet)
            try {
                $darkBrush = New-Object System.Drawing.SolidBrush([System.Drawing.ColorTranslator]::FromHtml('#1B1924'))
                $lightBrush = New-Object System.Drawing.SolidBrush([System.Drawing.ColorTranslator]::FromHtml('#E8E2D5'))
                $g.FillRectangle($darkBrush, 0, 0, [int]($sheetW / 2), $sheetH)
                $g.FillRectangle($lightBrush, [int]($sheetW / 2), 0, ($sheetW - [int]($sheetW / 2)), $sheetH)
                $darkBrush.Dispose()
                $lightBrush.Dispose()

                $scaled = New-ResizedBitmap $srcBmp $dw $dh 'Fit'
                try {
                    $g.DrawImage($scaled, $pad, $pad, $dw, $dh)
                    $g.DrawImage($scaled, ($pad * 2) + $dw, $pad, $dw, $dh)
                } finally {
                    $scaled.Dispose()
                }
                $sheet.Save($OutPath, [System.Drawing.Imaging.ImageFormat]::Png)
                Write-Host "Saved preview sheet: $OutPath (Display=${dw}x${dh}, Sheet=${sheetW}x${sheetH})"
            } finally {
                $g.Dispose()
                $sheet.Dispose()
            }
        } finally {
            $srcBmp.Dispose()
        }
    }

    'official-poses' {
        if (-not $OutPath) { throw 'Specify -OutPath <directory> [-GodotPath <MegaDot console exe>] [-GamePath <Slay the Spire 2 folder>].' }
        if (-not $GodotPath) {
            $GodotPath = Join-Path $PSScriptRoot '../../../../.tools/megadot/MegaDot_v4.5.1-stable_mono_win64_console.exe'
        }
        $resolvedGodot = (Resolve-Path -LiteralPath $GodotPath).Path
        $pck = Join-Path $GamePath 'SlayTheSpire2.pck'
        $spine = Join-Path $GamePath 'libspine_godot.windows.template_release.x86_64.dll'
        foreach ($required in @($pck, $spine)) {
            if (-not (Test-Path -LiteralPath $required)) { throw "Missing installed game file: $required" }
        }
        $resolvedOut = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutPath)
        $hostProject = Join-Path $resolvedOut '_host'
        New-Item -ItemType Directory -Path $hostProject -Force | Out-Null
        $logs = @{ Engine = Join-Path $resolvedOut 'render.engine.log'; Out = Join-Path $resolvedOut 'render.stdout.log'; Err = Join-Path $resolvedOut 'render.stderr.log' }
        try {
            # The installed game's Spine extension renders its own skeleton resources; nothing is copied from the PCK.
            $spinePath = $spine -replace '\\', '/'
            [System.IO.File]::WriteAllText((Join-Path $hostProject 'project.godot'), "config_version=5`n")
            [System.IO.File]::WriteAllText((Join-Path $hostProject 'local_spine.gdextension'),
                "[configuration]`nentry_symbol=`"spine_godot_library_init`"`ncompatibility_minimum=`"4.1`"`n[libraries]`nwindows.x86_64=`"$spinePath`"`n")
            Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'render-official-poses.gd') -Destination $hostProject -Force
            # Like dev.ps1, pass a fresh --log-file and redirect streams so engine stderr cannot abort PowerShell.
            $arguments = @('--log-file', "`"$($logs.Engine)`"", '--path', "`"$hostProject`"", '--rendering-method', 'gl_compatibility',
                '--resolution', '64x64', '--position', '-10000,-10000', '--script', 'render-official-poses.gd', '--', "`"$pck`"", "`"$resolvedOut`"")
            $process = Start-Process -FilePath $resolvedGodot -ArgumentList $arguments -WorkingDirectory $hostProject -WindowStyle Hidden -Wait -PassThru `
                -RedirectStandardOutput $logs.Out -RedirectStandardError $logs.Err
            $output = @(Get-Content -LiteralPath $logs.Out -Encoding UTF8)
            $errors = Get-Content -LiteralPath $logs.Err -Encoding UTF8 -Raw
            $output | Where-Object { $_ -match '^CAPTURE' } | Write-Host
            if ($process.ExitCode -ne 0 -or "$($output -join "`n")`n$errors" -match 'SCRIPT ERROR:|Parse Error:' -or
                -not ($output -match '^CAPTURE_COMPLETE failures=0')) {
                throw "Official pose rendering failed (exit $($process.ExitCode)); see $($logs.Out), $($logs.Err) and $($logs.Engine)."
            }
        } finally {
            Remove-Item -LiteralPath $hostProject -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
}
