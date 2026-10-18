[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [ValidateSet('inspect', 'preview', 'compare', 'export', 'outline')]
    [string]$Command,

    [string]$Path,
    [string]$Source,
    [string]$Target,
    [string]$OutPath,
    [int]$Width = 0,
    [int]$Height = 0,
    [int]$DisplayWidth = 0,
    [int]$DisplayHeight = 0,
    [ValidateSet('Fit', 'Cover', 'Stretch')]
    [string]$CropMode = 'Fit',
    [int]$Radius = 2,
    [string]$Color = '#FFFFFF',
    [string[]]$Paths,
    [string[]]$Labels
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

function Get-AlphaBounds([System.Drawing.Bitmap]$bmp) {
    $minX = $bmp.Width
    $minY = $bmp.Height
    $maxX = -1
    $maxY = -1
    $nonZero = 0
    for ($y = 0; $y -lt $bmp.Height; $y++) {
        for ($x = 0; $x -lt $bmp.Width; $x++) {
            $c = $bmp.GetPixel($x, $y)
            if ($c.A -gt 8) {
                $nonZero++
                if ($x -lt $minX) { $minX = $x }
                if ($x -gt $maxX) { $maxX = $x }
                if ($y -lt $minY) { $minY = $y }
                if ($y -gt $maxY) { $maxY = $y }
            }
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
    return [pscustomobject]@{
        Width            = $bmp.Width
        Height           = $bmp.Height
        PixelFormat      = $bmp.PixelFormat.ToString()
        CornerAlpha      = ($corners -join ',')
        CornersTransparent = (($corners | Measure-Object -Maximum).Maximum -eq 0)
        BoundingBox      = "${minX},${minY} -> ${maxX},${maxY} (${bboxW}x${bboxH})"
        BBoxWidthRatio   = [math]::Round(($bboxW / [double]$bmp.Width) * 100, 1)
        BBoxHeightRatio  = [math]::Round(($bboxH / [double]$bmp.Height) * 100, 1)
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
                $images += [pscustomobject]@{ Path = $resolvedSources[$i]; Label = $label; Bitmap = $bitmap }
            }

            # One scale for every complete canvas; do not normalize subjects or alpha bounds.
            $maxWidth = ($images | ForEach-Object { $_.Bitmap.Width } | Measure-Object -Maximum).Maximum
            $maxHeight = ($images | ForEach-Object { $_.Bitmap.Height } | Measure-Object -Maximum).Maximum
            $scale = [math]::Min($DisplayWidth / [double]$maxWidth, $DisplayHeight / [double]$maxHeight)
            $pad = 24
            $headerHeight = 64
            $cellWidth = [math]::Max(160, $DisplayWidth)
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
                $left = $pad + ($i * ($cellWidth + $pad))
                $darkTop = $pad + $headerHeight
                $lightTop = $darkTop + $DisplayHeight + $pad
                $labelRect = New-Object System.Drawing.RectangleF($left, $pad, $cellWidth, 40)
                $g.DrawString($entry.Label, $labelFont, [System.Drawing.Brushes]::White, $labelRect, $textFormat)
                $metaRect = New-Object System.Drawing.RectangleF($left, ($pad + 42), $cellWidth, 18)
                $g.DrawString(("{0} x {1}" -f $entry.Bitmap.Width, $entry.Bitmap.Height), $metaFont, [System.Drawing.Brushes]::LightGray, $metaRect, $textFormat)
                $g.FillRectangle($lightBrush, $left, $lightTop, $cellWidth, $DisplayHeight)

                $drawW = [math]::Max(1, [int][math]::Round($entry.Bitmap.Width * $scale))
                $drawH = [math]::Max(1, [int][math]::Round($entry.Bitmap.Height * $scale))
                $x = $left + [int][math]::Floor(($cellWidth - $drawW) / 2.0)
                $yOffset = [int][math]::Floor(($DisplayHeight - $drawH) / 2.0)
                foreach ($top in @($darkTop, $lightTop)) {
                    $destination = New-Object System.Drawing.Rectangle($x, ($top + $yOffset), $drawW, $drawH)
                    $g.DrawImage($entry.Bitmap, $destination)
                }
                [pscustomobject]@{
                    Path = $entry.Path
                    Label = $entry.Label
                    SourceWidth = $entry.Bitmap.Width
                    SourceHeight = $entry.Bitmap.Height
                    DrawWidth = $drawW
                    DrawHeight = $drawH
                }
            }
            $outDir = Split-Path -Parent $resolvedOutput
            if (-not (Test-Path -LiteralPath $outDir)) {
                New-Item -ItemType Directory -Path $outDir -Force | Out-Null
            }
            $sheet.Save($resolvedOutput, [System.Drawing.Imaging.ImageFormat]::Png)
            [pscustomobject]@{
                OutputPath = $resolvedOutput
                Scale = $scale
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
            $info | Format-List
        } finally {
            $bmp.Dispose()
        }
    }

    'export' {
        if (-not $Source -or -not $Target -or $Width -le 0 -or $Height -le 0) {
            throw 'Specify -Source <png> -Target <png> -Width <int> -Height <int> [-CropMode Fit|Cover|Stretch].'
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
                $dstBmp.Save($Target, [System.Drawing.Imaging.ImageFormat]::Png)
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
}
