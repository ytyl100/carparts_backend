# 批量更新问题修复文档

## 🔍 问题描述

前端发送批量更新请求到后端，但 `parts.json` 文件没有被更新。

### 原始请求示例

```json
{
  "UPDATED_PARTS": [{
    "id": "han_p29",
    "subCategoryId": "han_cc",
    "position": "10129",
    "oeNumber": "HCE2203020A",
    "standardName": "右前传动半轴总成",
    "originalName": "右前传动半轴总成-Right front drive half shaft assembly",
    "quantity": "01",
    "note": "(ACA3#)",
    "date": "",
    "x": 7,
    "y": 24,
    "imageUrl": "/han_ev/page_12_img_3.png",
    "priceRecords": [...],
    "replacementParts": [{
      "brand": "本田",
      "originalOe": "HCE2203020A",
      "replacementOe": "0",
      "note": "",
      "CostExclTax": 1,      // ❌ 错误：应该是小写 costExclTax
      "CostInclTax": 1,
      "SaleExclTax": 1,
      "SaleInclTax": 1
    }],
    "adaptableModels": [],
    "lastUpdated": "0001-01-01T00:00:00"
  }]
}
```

---

## 🐛 问题原因

### 1. **缺少批量更新端点** ❌
后端没有 `PUT /api/parts/batch` 端点来处理批量更新请求。

### 2. **字段名大小写不匹配** ⚠️
前端使用 PascalCase（首字母大写），但 C# 模型期望 camelCase（首字母小写）：

| 错误（前端） | 正确（后端期望） |
|-------------|----------------|
| `CostExclTax` | `costExclTax` |
| `CostInclTax` | `costInclTax` |
| `SaleExclTax` | `saleExclTax` |
| `SaleInclTax` | `saleInclTax` |

---

## ✅ 解决方案

### 已完成的修复

#### 1. **添加批量更新端点**

在 `PartsController.cs` 中添加：

```csharp
/// <summary>
/// Batch update multiple parts
/// </summary>
[HttpPut("batch")]
[Authorize(Roles = "admin,Admin,manager,Manager")]
public async Task<ActionResult<List<Part>>> BatchUpdate([FromBody] BatchUpdateRequest request)
{
    try
    {
        if (request.UPDATED_PARTS == null || request.UPDATED_PARTS.Count == 0)
        {
            return BadRequest(new { message = "No parts provided for update." });
        }

        _logger.LogInformation("Batch update request received for {Count} parts", request.UPDATED_PARTS.Count);

        var updatedParts = await _partService.BatchUpdateAsync(request.UPDATED_PARTS);
        
        return Ok(new 
        { 
            message = $"Successfully updated {updatedParts.Count} parts.",
            updatedCount = updatedParts.Count,
            parts = updatedParts
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error during batch update");
        return StatusCode(500, new { message = "An error occurred during batch update.", error = ex.Message });
    }
}
```

#### 2. **添加 Service 层方法**

在 `PartService.cs` 中添加：

```csharp
public async Task<List<Part>> BatchUpdateAsync(List<Part> parts)
{
    try
    {
        if (parts == null || parts.Count == 0)
        {
            _logger.LogWarning("Batch update called with empty or null parts list");
            return new List<Part>();
        }

        _logger.LogInformation("Starting batch update for {Count} parts", parts.Count);
        
        var updatedParts = new List<Part>();
        var allParts = await _jsonFileService.GetAllAsync();
        
        foreach (var updatedPart in parts)
        {
            try
            {
                var existingIndex = allParts.FindIndex(p => 
                    p.Id.Equals(updatedPart.Id, StringComparison.OrdinalIgnoreCase));
                
                if (existingIndex >= 0)
                {
                    // Update existing part
                    updatedPart.LastUpdated = DateTime.UtcNow;
                    allParts[existingIndex] = updatedPart;
                    updatedParts.Add(updatedPart);
                    _logger.LogInformation("Updated part: {Id}", updatedPart.Id);
                }
                else
                {
                    _logger.LogWarning("Part not found for update: {Id}", updatedPart.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating individual part: {Id}", updatedPart.Id);
                // Continue with next part instead of failing entire batch
            }
        }
        
        // Save all parts back to file
        await _jsonFileService.ReplaceAllAsync(allParts);
        
        _logger.LogInformation("Batch update completed. Updated {Count} parts", updatedParts.Count);
        return updatedParts;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error during batch update");
        throw;
    }
}
```

#### 3. **更新接口定义**

在 `IPartService.cs` 中添加：

```csharp
Task<List<Part>> BatchUpdateAsync(List<Part> parts);
```

---

## 🔧 前端修复

### 修正字段名大小写

前端需要将 ReplacementParts 和 AdaptableModels 的价格字段改为 camelCase：

```javascript
// ❌ 错误
{
  "replacementParts": [{
    "brand": "本田",
    "originalOe": "HCE2203020A",
    "replacementOe": "0",
    "note": "",
    "CostExclTax": 1,     // 错误
    "CostInclTax": 1,
    "SaleExclTax": 1,
    "SaleInclTax": 1
  }]
}

// ✅ 正确
{
  "replacementParts": [{
    "brand": "本田",
    "originalOe": "HCE2203020A",
    "replacementOe": "0",
    "note": "",
    "costExclTax": 1,     // 正确
    "costInclTax": 1,
    "saleExclTax": 1,
    "saleInclTax": 1
  }]
}
```

### TypeScript/JavaScript 修复示例

```typescript
// 修正接口定义
interface ReplacementPart {
  brand: string;
  originalOe: string;
  replacementOe: string;
  note: string;
  costExclTax: number;  // camelCase
  costInclTax: number;
  saleExclTax: number;
  saleInclTax: number;
}

// 发送请求
const batchUpdateRequest = {
  UPDATED_PARTS: parts.map(part => ({
    ...part,
    replacementParts: part.replacementParts.map(rp => ({
      brand: rp.brand,
      originalOe: rp.originalOe,
      replacementOe: rp.replacementOe,
      note: rp.note,
      costExclTax: rp.costExclTax,  // 使用 camelCase
      costInclTax: rp.costInclTax,
      saleExclTax: rp.saleExclTax,
      saleInclTax: rp.saleInclTax
    })),
    adaptableModels: part.adaptableModels.map(am => ({
      ...am,
      costExclTax: am.costExclTax,  // 使用 camelCase
      costInclTax: am.costInclTax,
      saleExclTax: am.saleExclTax,
      saleInclTax: am.saleInclTax
    }))
  }))
};

// 发送请求
const response = await fetch('/api/parts/batch', {
  method: 'PUT',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify(batchUpdateRequest)
});
```

---

## 🧪 测试

### 1. 测试批量更新端点

```bash
curl -X PUT "https://cp.xhfair.com/api/parts/batch" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "UPDATED_PARTS": [{
      "id": "han_p29",
      "subCategoryId": "han_cc",
      "position": "10129",
      "oeNumber": "HCE2203020A",
      "standardName": "右前传动半轴总成",
      "originalName": "右前传动半轴总成-Right front drive half shaft assembly",
      "quantity": "01",
      "note": "(ACA3#)",
      "date": "",
      "x": 7,
      "y": 24,
      "imageUrl": "/han_ev/page_12_img_3.png",
      "priceRecords": [{
        "brand": "BYD",
        "manufacturer": "比亚迪",
        "description": "原厂品质",
        "costExclTax": 0,
        "costInclTax": 0,
        "saleExclTax": 23.89,
        "saleInclTax": 27
      }],
      "replacementParts": [{
        "brand": "本田",
        "originalOe": "HCE2203020A",
        "replacementOe": "0",
        "note": "",
        "costExclTax": 1,
        "costInclTax": 1,
        "saleExclTax": 1,
        "saleInclTax": 1
      }],
      "adaptableModels": [],
      "lastUpdated": "0001-01-01T00:00:00"
    }]
  }'
```

### 2. 期望响应

```json
{
  "message": "Successfully updated 1 parts.",
  "updatedCount": 1,
  "parts": [{
    "id": "han_p29",
    "subCategoryId": "han_cc",
    "position": "10129",
    "oeNumber": "HCE2203020A",
    "standardName": "右前传动半轴总成",
    "originalName": "右前传动半轴总成-Right front drive half shaft assembly",
    "quantity": "01",
    "note": "(ACA3#)",
    "date": "",
    "x": 7,
    "y": 24,
    "imageUrl": "/han_ev/page_12_img_3.png",
    "priceRecords": [{
      "brand": "BYD",
      "manufacturer": "比亚迪",
      "description": "原厂品质",
      "costExclTax": 0,
      "costInclTax": 0,
      "saleExclTax": 23.89,
      "saleInclTax": 27,
      "currency": null
    }],
    "replacementParts": [{
      "brand": "本田",
      "originalOe": "HCE2203020A",
      "replacementOe": "0",
      "note": "",
      "costExclTax": 1,
      "costInclTax": 1,
      "saleExclTax": 1,
      "saleInclTax": 1
    }],
    "adaptableModels": [],
    "lastUpdated": "2025-02-09T15:30:45.1234567Z"
  }]
}
```

---

## 📋 部署步骤

### 1. 重新编译和发布

```powershell
cd D:\djc\backend\webApi\webApi

# 发布
.\deploy-verified.ps1
```

### 2. 上传到服务器

```bash
scp -r bin/Release/net8.0/publish/* user@cp.xhfair.com:/www/wwwroot/cp/publish/
```

### 3. 重启应用

```bash
ssh user@cp.xhfair.com "systemctl restart your-app"
```

### 4. 测试批量更新

```bash
# 测试端点是否可用
curl -X PUT "https://cp.xhfair.com/api/parts/batch" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"UPDATED_PARTS": []}'

# 应该返回
# {"message": "No parts provided for update."}
```

---

## ⚠️ 注意事项

### 1. **权限要求**
批量更新端点需要 `admin` 或 `manager` 角色。

### 2. **数据验证**
- 所有必填字段必须提供
- 价格字段必须 >= 0
- ID 必须存在于 parts.json 中

### 3. **错误处理**
- 如果某个零件更新失败，其他零件仍会继续更新
- 详细错误信息会记录在日志中
- 返回实际更新成功的零件列表

### 4. **性能考虑**
- 批量更新会一次性加载所有零件到内存
- 建议每次批量更新不超过 100 个零件
- 大量更新请分批进行

---

## 🔍 故障排查

### 问题 1: 返回 404 Not Found

**原因**: 端点不存在或路由错误

**解决方案**:
```bash
# 检查端点是否存在
curl -I https://cp.xhfair.com/api/parts/batch

# 应该返回 405 Method Not Allowed（如果端点存在但使用了错误的方法）
# 或 401 Unauthorized（如果端点存在但未授权）
```

### 问题 2: 返回 400 Bad Request

**原因**: JSON 反序列化失败

**解决方案**:
1. 检查字段名大小写
2. 确保 JSON 格式正确
3. 查看应用日志获取详细错误信息

### 问题 3: 更新部分成功

**原因**: 部分零件 ID 不存在或数据无效

**解决方案**:
- 查看响应中的 `updatedCount`
- 检查应用日志中的警告信息
- 验证所有零件 ID 是否存在

### 问题 4: 字段大小写问题

**原因**: 前端使用了 PascalCase 而不是 camelCase

**解决方案**:
确保前端使用正确的字段名：
- ✅ `costExclTax`
- ❌ `CostExclTax`

---

## 📊 API 端点总结

| 方法 | 端点 | 描述 | 权限 |
|------|------|------|------|
| PUT | `/api/parts/batch` | 批量更新零件 | admin/manager |
| PUT | `/api/parts/{id}` | 更新单个零件 | admin/manager |
| GET | `/api/parts` | 获取所有零件 | 无 |
| GET | `/api/parts/{id}` | 获取单个零件 | 无 |

---

## ✅ 验证清单

- [x] 批量更新端点已添加
- [x] Service 层方法已实现
- [x] 接口定义已更新
- [x] 编译成功
- [x] 支持部分成功（不会因单个零件失败而终止整个批量操作）
- [x] 详细的日志记录
- [x] 错误处理完善

---

## 🎉 总结

现在批量更新功能已经完全实现！前端需要确保：

1. ✅ 使用 `PUT /api/parts/batch` 端点
2. ✅ 字段名使用 camelCase（`costExclTax` 而不是 `CostExclTax`）
3. ✅ 请求体格式为 `{"UPDATED_PARTS": [...]}`
4. ✅ 提供有效的 JWT token（admin 或 manager 角色）

部署后即可正常使用批量更新功能！🚀
