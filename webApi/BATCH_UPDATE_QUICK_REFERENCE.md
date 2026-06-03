# 批量更新 Quick Reference

## 🚀 快速开始

### 正确的请求格式

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
      "costExclTax": 1,     // ✅ camelCase
      "costInclTax": 1,
      "saleExclTax": 1,
      "saleInclTax": 1
    }],
    "adaptableModels": [{
      "brand": "BYD",
      "region": "中国",
      "modelName": "海豚",
      "productionDate": "2021-2024",
      "modelCode": "ACA3",
      "costExclTax": 100,   // ✅ camelCase
      "costInclTax": 113,
      "saleExclTax": 150,
      "saleInclTax": 169.50
    }],
    "lastUpdated": "0001-01-01T00:00:00"
  }]
}
```

---

## ❌ 常见错误

### 1. 字段名大小写错误

```json
// ❌ 错误 - PascalCase
{
  "replacementParts": [{
    "CostExclTax": 1,  // 错误！
    "CostInclTax": 1
  }]
}

// ✅ 正确 - camelCase
{
  "replacementParts": [{
    "costExclTax": 1,  // 正确
    "costInclTax": 1
  }]
}
```

### 2. 缺少必填字段

```json
// ❌ 错误 - 缺少必填字段
{
  "replacementParts": [{
    "brand": "本田"
    // 缺少 originalOe 和 replacementOe
  }]
}

// ✅ 正确
{
  "replacementParts": [{
    "brand": "本田",
    "originalOe": "HCE2203020A",  // 必填
    "replacementOe": "0",           // 必填
    "note": "",
    "costExclTax": 0,
    "costInclTax": 0,
    "saleExclTax": 0,
    "saleInclTax": 0
  }]
}
```

---

## 🔧 前端代码示例

### TypeScript/JavaScript

```typescript
// 正确的接口定义
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

interface AdaptableModel {
  brand: string;
  region: string;
  modelName: string;
  productionDate: string;
  modelCode: string;
  costExclTax: number;  // camelCase
  costInclTax: number;
  saleExclTax: number;
  saleInclTax: number;
}

// 批量更新函数
async function batchUpdateParts(parts: Part[]) {
  const response = await fetch('/api/parts/batch', {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${getToken()}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      UPDATED_PARTS: parts
    })
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message);
  }

  return await response.json();
}
```

### React 示例

```jsx
const handleBatchUpdate = async (updatedParts) => {
  try {
    setLoading(true);
    
    const response = await fetch('/api/parts/batch', {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        UPDATED_PARTS: updatedParts
      })
    });

    if (!response.ok) {
      throw new Error('Batch update failed');
    }

    const result = await response.json();
    console.log(`Updated ${result.updatedCount} parts`);
    
    // 刷新数据
    fetchParts();
  } catch (error) {
    console.error('Error:', error);
    alert('Batch update failed: ' + error.message);
  } finally {
    setLoading(false);
  }
};
```

---

## 📝 字段名对照表

| C# 模型 | JSON 字段 | 类型 | 必填 |
|---------|-----------|------|------|
| `CostExclTax` | `costExclTax` | decimal | ❌ |
| `CostInclTax` | `costInclTax` | decimal | ❌ |
| `SaleExclTax` | `saleExclTax` | decimal | ❌ |
| `SaleInclTax` | `saleInclTax` | decimal | ❌ |
| `Brand` | `brand` | string | ✅ |
| `OriginalOe` | `originalOe` | string | ✅ |
| `ReplacementOe` | `replacementOe` | string | ✅ |
| `ModelCode` | `modelCode` | string | ✅ |
| `ModelName` | `modelName` | string | ✅ |

---

## 🧪 测试

### curl 测试

```bash
curl -X PUT "https://cp.xhfair.com/api/parts/batch" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d @- <<'EOF'
{
  "UPDATED_PARTS": [{
    "id": "han_p29",
    "subCategoryId": "han_cc",
    "position": "10129",
    "oeNumber": "HCE2203020A",
    "standardName": "右前传动半轴总成",
    "originalName": "右前传动半轴总成",
    "quantity": "01",
    "note": "",
    "date": "",
    "x": 7,
    "y": 24,
    "imageUrl": "",
    "priceRecords": [],
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
}
EOF
```

### PowerShell 测试

```powershell
.\test-batch-update.ps1
```

---

## 📞 故障排查

| 错误 | 原因 | 解决方案 |
|------|------|---------|
| 400 Bad Request | JSON 格式错误或字段名错误 | 检查字段名是否为 camelCase |
| 401 Unauthorized | 未提供 token 或 token 无效 | 提供有效的 JWT token |
| 403 Forbidden | 权限不足 | 确保用户角色为 admin 或 manager |
| 404 Not Found | 端点不存在 | 确认 URL 为 `/api/parts/batch` |
| 500 Internal Server Error | 服务器错误 | 检查应用日志 |

---

## ✅ 检查清单

在发送批量更新请求前：

- [ ] URL 正确：`PUT /api/parts/batch`
- [ ] 提供有效的 JWT token（admin 或 manager 角色）
- [ ] 字段名使用 camelCase（如 `costExclTax`）
- [ ] 所有必填字段已提供
- [ ] 零件 ID 存在于数据库中
- [ ] JSON 格式正确

---

## 🎯 快速修复

如果批量更新失败，尝试以下步骤：

1. **检查字段名**
   ```bash
   # 在请求体中搜索大写字母开头的价格字段
   grep -E "Cost|Sale" request.json
   # 应该全部是小写开头：costExclTax, saleExclTax
   ```

2. **验证 JSON 格式**
   ```bash
   cat request.json | jq .
   # 如果有语法错误，jq 会报错
   ```

3. **测试单个零件更新**
   ```bash
   curl -X PUT "https://cp.xhfair.com/api/parts/han_p29" \
     -H "Authorization: Bearer $TOKEN" \
     -H "Content-Type: application/json" \
     -d '{"id":"han_p29","subCategoryId":"han_cc",...}'
   ```

4. **查看服务器日志**
   ```bash
   ssh user@server "tail -f /var/log/app.log | grep -E 'Batch|ERROR'"
   ```

---

快速参考完成！🚀
