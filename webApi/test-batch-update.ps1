# 批量更新 API 测试脚本

# 配置
$baseUrl = "https://cp.xhfair.com"
$token = "YOUR_JWT_TOKEN_HERE"  # 替换为实际的 JWT token

Write-Host "🧪 测试批量更新 API" -ForegroundColor Cyan
Write-Host "=" * 60 -ForegroundColor Gray

# 测试数据
$testData = @{
    UPDATED_PARTS = @(
        @{
            id = "han_p29"
            subCategoryId = "han_cc"
            position = "10129"
            oeNumber = "HCE2203020A"
            standardName = "右前传动半轴总成"
            originalName = "右前传动半轴总成-Right front drive half shaft assembly"
            quantity = "01"
            note = "(ACA3#)"
            date = ""
            x = 7
            y = 24
            imageUrl = "/han_ev/page_12_img_3.png"
            priceRecords = @(
                @{
                    brand = "BYD"
                    manufacturer = "比亚迪"
                    description = "原厂品质"
                    costExclTax = 0
                    costInclTax = 0
                    saleExclTax = 23.89
                    saleInclTax = 27
                    currency = $null
                }
            )
            replacementParts = @(
                @{
                    brand = "本田"
                    originalOe = "HCE2203020A"
                    replacementOe = "0"
                    note = ""
                    costExclTax = 10.50  # 使用 camelCase
                    costInclTax = 11.87
                    saleExclTax = 15.00
                    saleInclTax = 16.95
                }
            )
            adaptableModels = @()
            lastUpdated = "0001-01-01T00:00:00"
        }
    )
}

# 转换为 JSON
$jsonBody = $testData | ConvertTo-Json -Depth 10

Write-Host "`n📤 发送请求..." -ForegroundColor Yellow
Write-Host "URL: $baseUrl/api/parts/batch" -ForegroundColor Cyan
Write-Host "方法: PUT" -ForegroundColor Cyan

try {
    # 发送请求
    $response = Invoke-RestMethod -Uri "$baseUrl/api/parts/batch" `
        -Method Put `
        -Headers @{
            "Authorization" = "Bearer $token"
            "Content-Type" = "application/json"
        } `
        -Body $jsonBody `
        -ErrorAction Stop

    Write-Host "`n✅ 请求成功!" -ForegroundColor Green
    Write-Host "`n📥 响应:" -ForegroundColor Cyan
    $response | ConvertTo-Json -Depth 10 | Write-Host

    Write-Host "`n📊 统计:" -ForegroundColor Yellow
    Write-Host "  - 更新数量: $($response.updatedCount)" -ForegroundColor White
    Write-Host "  - 消息: $($response.message)" -ForegroundColor White
}
catch {
    Write-Host "`n❌ 请求失败!" -ForegroundColor Red
    Write-Host "错误: $($_.Exception.Message)" -ForegroundColor Red
    
    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode.value__
        Write-Host "状态码: $statusCode" -ForegroundColor Red
        
        try {
            $errorBody = $_.ErrorDetails.Message
            Write-Host "错误详情: $errorBody" -ForegroundColor Red
        }
        catch {
            Write-Host "无法读取错误详情" -ForegroundColor Red
        }
    }
}

Write-Host "`n" + ("=" * 60) -ForegroundColor Gray
Write-Host "💡 提示:" -ForegroundColor Yellow
Write-Host "  1. 确保已替换 YOUR_JWT_TOKEN_HERE 为实际的 token" -ForegroundColor White
Write-Host "  2. 确保 token 具有 admin 或 manager 角色" -ForegroundColor White
Write-Host "  3. 检查 parts.json 中是否存在 han_p29 这个零件 ID" -ForegroundColor White
Write-Host "  4. 注意字段名使用 camelCase（如 costExclTax）" -ForegroundColor White
