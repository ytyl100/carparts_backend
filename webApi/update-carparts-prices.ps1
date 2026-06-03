# 更新 carparts.json 中的 ReplacementParts 和 AdaptableModels，添加价格属性
# 运行此脚本前请备份 carparts.json

$jsonPath = "API/Data/carparts.json"

if (-not (Test-Path $jsonPath)) {
    Write-Host "⚠️  文件不存在: $jsonPath (跳过)" -ForegroundColor Yellow
    exit 0
}

Write-Host "📖 读取 $jsonPath..." -ForegroundColor Cyan

# 读取 JSON 文件
$jsonContent = Get-Content $jsonPath -Raw -Encoding UTF8
$carParts = $jsonContent | ConvertFrom-Json

$updatedCount = 0
$replacementPartsCount = 0
$adaptableModelsCount = 0

Write-Host "🔄 开始更新..." -ForegroundColor Yellow

foreach ($part in $carParts) {
    $partUpdated = $false
    
    # 更新 ReplacementParts
    if ($part.ReplacementParts -and $part.ReplacementParts.Count -gt 0) {
        foreach ($replacementPart in $part.ReplacementParts) {
            # 检查是否已有这些属性
            if (-not $replacementPart.PSObject.Properties['CostExclTax']) {
                $replacementPart | Add-Member -NotePropertyName 'CostExclTax' -NotePropertyValue 0 -Force
            }
            if (-not $replacementPart.PSObject.Properties['CostInclTax']) {
                $replacementPart | Add-Member -NotePropertyName 'CostInclTax' -NotePropertyValue 0 -Force
            }
            if (-not $replacementPart.PSObject.Properties['SaleExclTax']) {
                $replacementPart | Add-Member -NotePropertyName 'SaleExclTax' -NotePropertyValue 0 -Force
            }
            if (-not $replacementPart.PSObject.Properties['SaleInclTax']) {
                $replacementPart | Add-Member -NotePropertyName 'SaleInclTax' -NotePropertyValue 0 -Force
            }
            $replacementPartsCount++
            $partUpdated = $true
        }
    }
    
    # 更新 AdaptableModels
    if ($part.AdaptableModels -and $part.AdaptableModels.Count -gt 0) {
        foreach ($adaptableModel in $part.AdaptableModels) {
            # 检查是否已有这些属性
            if (-not $adaptableModel.PSObject.Properties['CostExclTax']) {
                $adaptableModel | Add-Member -NotePropertyName 'CostExclTax' -NotePropertyValue 0 -Force
            }
            if (-not $adaptableModel.PSObject.Properties['CostInclTax']) {
                $adaptableModel | Add-Member -NotePropertyName 'CostInclTax' -NotePropertyValue 0 -Force
            }
            if (-not $adaptableModel.PSObject.Properties['SaleExclTax']) {
                $adaptableModel | Add-Member -NotePropertyName 'SaleExclTax' -NotePropertyValue 0 -Force
            }
            if (-not $adaptableModel.PSObject.Properties['SaleInclTax']) {
                $adaptableModel | Add-Member -NotePropertyName 'SaleInclTax' -NotePropertyValue 0 -Force
            }
            $adaptableModelsCount++
            $partUpdated = $true
        }
    }
    
    if ($partUpdated) {
        $updatedCount++
    }
}

Write-Host "✅ 更新完成!" -ForegroundColor Green
Write-Host "   - 更新的零件数量: $updatedCount" -ForegroundColor Cyan
Write-Host "   - 更新的替换配件数量: $replacementPartsCount" -ForegroundColor Cyan
Write-Host "   - 更新的适配车型数量: $adaptableModelsCount" -ForegroundColor Cyan

if ($updatedCount -eq 0) {
    Write-Host "ℹ️  没有需要更新的数据" -ForegroundColor Blue
    exit 0
}

# 备份原文件
$backupPath = "$jsonPath.backup.$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Copy-Item $jsonPath $backupPath
Write-Host "💾 已备份原文件到: $backupPath" -ForegroundColor Green

# 保存更新后的 JSON
$updatedJson = $carParts | ConvertTo-Json -Depth 10 -Compress:$false
$updatedJson | Out-File $jsonPath -Encoding UTF8 -Force

Write-Host "✅ 已保存更新后的文件: $jsonPath" -ForegroundColor Green
