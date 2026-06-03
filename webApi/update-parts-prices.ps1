# 更新 parts.json 中的 ReplacementParts 和 AdaptableModels，添加价格属性
# 运行此脚本前请备份 parts.json

$jsonPath = "API/Data/parts.json"

if (-not (Test-Path $jsonPath)) {
    Write-Host "❌ 文件不存在: $jsonPath" -ForegroundColor Red
    exit 1
}

Write-Host "📖 读取 $jsonPath..." -ForegroundColor Cyan

# 读取 JSON 文件
$jsonContent = Get-Content $jsonPath -Raw -Encoding UTF8
$parts = $jsonContent | ConvertFrom-Json

$updatedCount = 0
$replacementPartsCount = 0
$adaptableModelsCount = 0

Write-Host "🔄 开始更新..." -ForegroundColor Yellow

foreach ($part in $parts) {
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

# 备份原文件
$backupPath = "$jsonPath.backup.$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Copy-Item $jsonPath $backupPath
Write-Host "💾 已备份原文件到: $backupPath" -ForegroundColor Green

# 保存更新后的 JSON
$updatedJson = $parts | ConvertTo-Json -Depth 10 -Compress:$false
$updatedJson | Out-File $jsonPath -Encoding UTF8 -Force

Write-Host "✅ 已保存更新后的文件: $jsonPath" -ForegroundColor Green
Write-Host "`n📋 下一步:" -ForegroundColor Yellow
Write-Host "   1. 检查更新后的 JSON 文件" -ForegroundColor White
Write-Host "   2. 运行应用测试新的 API 端点" -ForegroundColor White
Write-Host "   3. 如有问题，可从备份文件恢复: $backupPath" -ForegroundColor White
