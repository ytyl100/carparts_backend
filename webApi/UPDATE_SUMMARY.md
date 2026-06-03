# ReplacementParts 和 AdaptableModels 价格属性更新 - 完成总结

## ✅ 已完成的修改

### 1. **数据模型更新** ✅

#### ReplacementPart 类 (`webApi/API/Models/Part.cs`)
添加了4个价格属性：
```csharp
[Range(0, double.MaxValue)]
public decimal CostExclTax { get; set; } = 0;    // 不含税成本价

[Range(0, double.MaxValue)]
public decimal CostInclTax { get; set; } = 0;    // 含税成本价

[Range(0, double.MaxValue)]
public decimal SaleExclTax { get; set; } = 0;    // 不含税售价

[Range(0, double.MaxValue)]
public decimal SaleInclTax { get; set; } = 0;    // 含税售价
```

#### AdaptableModel 类 (`webApi/API/Models/Part.cs`)
同样添加了4个价格属性（与 ReplacementPart 相同）

---

### 2. **Controller 层更新** ✅

在 `webApi/API/Controllers/PartsController.cs` 中新增了以下端点：

#### ReplacementParts CRUD 操作：
- `GET /api/parts/{id}/replacements` - 获取所有替换配件
- `GET /api/parts/{id}/replacements/{replacementOe}` - 获取单个替换配件
- `POST /api/parts/{id}/replacements` - 添加替换配件
- `PUT /api/parts/{id}/replacements/{replacementOe}` - **[新增]** 更新替换配件
- `DELETE /api/parts/{id}/replacements/{replacementOe}` - 删除替换配件

#### AdaptableModels CRUD 操作：
- `GET /api/parts/{id}/models` - 获取所有适配车型
- `GET /api/parts/{id}/models/{modelCode}` - 获取单个适配车型
- `POST /api/parts/{id}/models` - 添加适配车型
- `PUT /api/parts/{id}/models/{modelCode}` - **[新增]** 更新适配车型
- `DELETE /api/parts/{id}/models/{modelCode}` - 删除适配车型

---

### 3. **Service 层更新** ✅

在 `webApi/API/Services/PartService.cs` 中新增了方法：

```csharp
// ReplacementParts
Task<Part?> UpdateReplacementPartAsync(string partId, string replacementOe, ReplacementPart updatedReplacementPart);

// AdaptableModels
Task<Part?> UpdateAdaptableModelAsync(string partId, string modelCode, AdaptableModel updatedAdaptableModel);
```

---

### 4. **接口定义更新** ✅

在 `webApi/API/Services/IPartService.cs` 中添加了方法签名

---

### 5. **数据迁移脚本** ✅

创建了两个 PowerShell 脚本用于更新现有 JSON 数据：

#### `webApi/update-parts-prices.ps1`
- 自动为 `parts.json` 中所有的 ReplacementParts 和 AdaptableModels 添加价格属性
- 默认值为 0
- 自动备份原文件

#### `webApi/update-carparts-prices.ps1`
- 同上，但针对 `carparts.json`

---

### 6. **文档** ✅

#### `webApi/API_REPLACEMENTPARTS_ADAPTABLEMODELS.md`
完整的 API 使用文档，包括：
- API 端点说明
- 请求/响应示例
- curl 测试示例
- 权限说明
- 部署步骤

---

## 🚀 部署步骤

### 步骤 1: 更新现有数据

```powershell
cd D:\djc\backend\webApi\webApi

# 更新 parts.json
.\update-parts-prices.ps1

# 更新 carparts.json（如果存在）
.\update-carparts-prices.ps1
```

**输出示例：**
```
📖 读取 API/Data/parts.json...
🔄 开始更新...
✅ 更新完成!
   - 更新的零件数量: 150
   - 更新的替换配件数量: 45
   - 更新的适配车型数量: 200
💾 已备份原文件到: API/Data/parts.json.backup.20250209_143022
✅已保存更新后的文件: API/Data/parts.json
```

### 步骤 2: 验证更新

查看更新后的 JSON 文件：

```powershell
# 查看某个零件的数据
Get-Content API/Data/parts.json | ConvertFrom-Json | Select-Object -First 1 | ConvertTo-Json -Depth 5
```

确认 ReplacementParts 和 AdaptableModels 包含新属性：
```json
{
  "replacementParts": [
    {
      "brand": "BYD",
      "originalOe": "...",
      "replacementOe": "...",
      "note": "...",
      "costExclTax": 0,
      "costInclTax": 0,
      "saleExclTax": 0,
      "saleInclTax": 0
    }
  ]
}
```

### 步骤 3: 编译和测试

```powershell
# 编译项目（已通过）
dotnet build

# 本地运行测试
dotnet run --project webApi
```

### 步骤 4: 发布到生产环境

```powershell
# 运行发布脚本
.\deploy-verified.ps1

# 上传到服务器
scp -r bin/Release/net8.0/publish/* user@cp.xhfair.com:/www/wwwroot/cp/publish/

# 重启应用
ssh user@cp.xhfair.com "systemctl restart your-app"
```

---

## 🧪 API 测试

### 1. 获取零件的所有替换配件

```bash
curl https://cp.xhfair.com/api/parts/dolphin_p1/replacements | jq
```

**期望输出：**
```json
[
  {
    "brand": "BYD",
    "originalOe": "EM2EU2803111",
    "replacementOe": "EM2EU2803115",
    "note": "升级版",
    "costExclTax": 0,
    "costInclTax": 0,
    "saleExclTax": 0,
    "saleInclTax": 0
  }
]
```

### 2. 添加带价格的替换配件

```bash
curl -X POST "https://cp.xhfair.com/api/parts/dolphin_p1/replacements" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "brand": "BYD",
    "originalOe": "EM2EU2803111",
    "replacementOe": "EM2EU2803120",
    "note": "新版本",
    "costExclTax": 100.00,
    "costInclTax": 113.00,
    "saleExclTax": 150.00,
    "saleInclTax": 169.50
  }'
```

### 3. 更新替换配件价格

```bash
curl -X PUT "https://cp.xhfair.com/api/parts/dolphin_p1/replacements/EM2EU2803120" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "brand": "BYD",
    "originalOe": "EM2EU2803111",
    "replacementOe": "EM2EU2803120",
    "note": "促销价",
    "costExclTax": 90.00,
    "costInclTax": 101.70,
    "saleExclTax": 135.00,
    "saleInclTax": 152.55
  }'
```

### 4. 获取零件的所有适配车型

```bash
curl https://cp.xhfair.com/api/parts/dolphin_p1/models | jq
```

---

## 📊 数据结构对比

### 更新前（旧数据）

```json
{
  "replacementParts": [
    {
      "brand": "BYD",
      "originalOe": "EM2EU2803111",
      "replacementOe": "EM2EU2803115",
      "note": "升级版"
    }
  ]
}
```

### 更新后（新数据）

```json
{
  "replacementParts": [
    {
      "brand": "BYD",
      "originalOe": "EM2EU2803111",
      "replacementOe": "EM2EU2803115",
      "note": "升级版",
      "costExclTax": 0,
      "costInclTax": 0,
      "saleExclTax": 0,
      "saleInclTax": 0
    }
  ]
}
```

---

## ✅ 功能验证清单

- [x] ReplacementPart 模型包含4个价格属性
- [x] AdaptableModel 模型包含4个价格属性
- [x] Controller 支持 ReplacementParts 的完整 CRUD 操作
- [x] Controller 支持 AdaptableModels 的完整 CRUD 操作
- [x] Service 层实现了 Update 方法
- [x] 接口定义包含新方法
- [x] 数据迁移脚本可正常运行
- [x] 项目编译成功
- [x] API 文档完整

---

## 📝 API 端点总结

### ReplacementParts

| 方法 | 端点 | 描述 | 权限 |
|------|------|------|------|
| GET | `/api/parts/{id}/replacements` | 获取所有 | 无 |
| GET | `/api/parts/{id}/replacements/{oe}` | 获取单个 | 无 |
| POST | `/api/parts/{id}/replacements` | 添加 | admin/manager |
| PUT | `/api/parts/{id}/replacements/{oe}` | 更新 | admin/manager |
| DELETE | `/api/parts/{id}/replacements/{oe}` | 删除 | admin/manager |

### AdaptableModels

| 方法 | 端点 | 描述 | 权限 |
|------|------|------|------|
| GET | `/api/parts/{id}/models` | 获取所有 | 无 |
| GET | `/api/parts/{id}/models/{code}` | 获取单个 | 无 |
| POST | `/api/parts/{id}/models` | 添加 | admin/manager |
| PUT | `/api/parts/{id}/models/{code}` | 更新 | admin/manager |
| DELETE | `/api/parts/{id}/models/{code}` | 删除 | admin/manager |

---

## 🎯 下一步建议

### 1. 前端集成

创建前端组件来管理这些数据：

```typescript
// ReplacementPartForm.tsx
interface ReplacementPartForm {
  brand: string;
  originalOe: string;
  replacementOe: string;
  note: string;
  costExclTax: number;
  costInclTax: number;
  saleExclTax: number;
  saleInclTax: number;
}

// API 调用示例
async function updateReplacementPart(
  partId: string, 
  replacementOe: string, 
  data: ReplacementPartForm
) {
  const response = await fetch(
    `/api/parts/${partId}/replacements/${replacementOe}`,
    {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(data)
    }
  );
  return response.json();
}
```

### 2. 价格计算工具

添加自动计算含税/不含税价格的功能：

```typescript
// 计算含税价（13%增值税）
const TAX_RATE = 0.13;

function calculateInclTax(exclTax: number): number {
  return exclTax * (1 + TAX_RATE);
}

function calculateExclTax(inclTax: number): number {
  return inclTax / (1 + TAX_RATE);
}
```

### 3. 批量导入功能

考虑添加批量导入替换配件和适配车型的 API：

```http
POST /api/parts/{id}/replacements/batch
POST /api/parts/{id}/models/batch
```

---

## 📞 问题排查

### 问题 1: JSON 文件更新后数据未生效

**解决方案：**
1. 清除应用缓存
2. 重启应用
3. 检查文件权限

### 问题 2: API 返回 401 未授权

**解决方案：**
1. 确认 JWT token 有效
2. 检查用户角色是否为 `admin` 或 `manager`
3. 查看应用日志

### 问题 3: 价格字段验证失败

**解决方案：**
- 确保所有价格字段 >= 0
- 使用 decimal 类型（不是 double）
- 前端验证数值格式

---

## 🎉 总结

本次更新成功为 ReplacementParts 和 AdaptableModels 添加了价格管理功能，包括：

✅ **4个价格属性**（不含税成本、含税成本、不含税售价、含税售价）  
✅ **完整的 CRUD API**（增删改查）  
✅ **数据迁移脚本**（自动更新现有数据）  
✅ **完整的文档**（API 使用说明）  
✅ **编译通过**（无错误）

现在可以开始使用新的 API 来管理替换配件和适配车型的价格信息了！🚀
