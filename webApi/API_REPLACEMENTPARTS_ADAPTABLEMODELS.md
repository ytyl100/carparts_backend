# ReplacementParts 和 AdaptableModels 价格属性更新 - API 文档

## 📋 概述

本文档说明如何使用新增的 API 端点来管理零件的 **ReplacementParts**（替换配件）和 **AdaptableModels**（适配车型）的价格信息。

### 新增的价格属性

在 `ReplacementPart` 和 `AdaptableModel` 中新增了以下4个价格属性：

```json
{
  "CostExclTax": 0,      // 不含税成本价
  "CostInclTax": 0,      // 含税成本价
  "SaleExclTax": 0,      // 不含税售价
  "SaleInclTax": 0       // 含税售价
}
```

---

## 🔧 ReplacementParts（替换配件）API

### 1. 获取所有替换配件

**请求:**
```http
GET /api/parts/{partId}/replacements
```

**响应示例:**
```json
[
  {
    "brand": "BYD",
    "originalOe": "EM2EU2803111",
    "replacementOe": "EM2EU2803112",
    "note": "通用型",
    "costExclTax": 100.00,
    "costInclTax": 113.00,
    "saleExclTax": 150.00,
    "saleInclTax": 169.50
  }
]
```

### 2. 获取单个替换配件

**请求:**
```http
GET /api/parts/{partId}/replacements/{replacementOe}
```

**响应示例:**
```json
{
  "brand": "BYD",
  "originalOe": "EM2EU2803111",
  "replacementOe": "EM2EU2803112",
  "note": "通用型",
  "costExclTax": 100.00,
  "costInclTax": 113.00,
  "saleExclTax": 150.00,
  "saleInclTax": 169.50
}
```

### 3. 添加替换配件

**请求:**
```http
POST /api/parts/{partId}/replacements
Authorization: Bearer {token}
Content-Type: application/json

{
  "brand": "BYD",
  "originalOe": "EM2EU2803111",
  "replacementOe": "EM2EU2803115",
  "note": "升级版",
  "costExclTax": 120.00,
  "costInclTax": 135.60,
  "saleExclTax": 180.00,
  "saleInclTax": 203.40
}
```

**权限要求:** `admin`, `Admin`, `manager`, `Manager`

### 4. 更新替换配件

**请求:**
```http
PUT /api/parts/{partId}/replacements/{replacementOe}
Authorization: Bearer {token}
Content-Type: application/json

{
  "brand": "BYD",
  "originalOe": "EM2EU2803111",
  "replacementOe": "EM2EU2803115",
  "note": "升级版 - 更新价格",
  "costExclTax": 110.00,
  "costInclTax": 124.30,
  "saleExclTax": 165.00,
  "saleInclTax": 186.45
}
```

**权限要求:** `admin`, `Admin`, `manager`, `Manager`

### 5. 删除替换配件

**请求:**
```http
DELETE /api/parts/{partId}/replacements/{replacementOe}
Authorization: Bearer {token}
```

**权限要求:** `admin`, `Admin`, `manager`, `Manager`

---

## 🚗 AdaptableModels（适配车型）API

### 1. 获取所有适配车型

**请求:**
```http
GET /api/parts/{partId}/models
```

**响应示例:**
```json
[
  {
    "brand": "BYD",
    "region": "中国",
    "modelName": "海豚",
    "productionDate": "2021-2024",
    "modelCode": "ACA3",
    "costExclTax": 100.00,
    "costInclTax": 113.00,
    "saleExclTax": 150.00,
    "saleInclTax": 169.50
  }
]
```

### 2. 获取单个适配车型

**请求:**
```http
GET /api/parts/{partId}/models/{modelCode}
```

**响应示例:**
```json
{
  "brand": "BYD",
  "region": "中国",
  "modelName": "海豚",
  "productionDate": "2021-2024",
  "modelCode": "ACA3",
  "costExclTax": 100.00,
  "costInclTax": 113.00,
  "saleExclTax": 150.00,
  "saleInclTax": 169.50
}
```

### 3. 添加适配车型

**请求:**
```http
POST /api/parts/{partId}/models
Authorization: Bearer {token}
Content-Type: application/json

{
  "brand": "BYD",
  "region": "中国",
  "modelName": "海豚Plus",
  "productionDate": "2023-2024",
  "modelCode": "ACA4",
  "costExclTax": 120.00,
  "costInclTax": 135.60,
  "saleExclTax": 180.00,
  "saleInclTax": 203.40
}
```

**权限要求:** `admin`, `Admin`, `manager`, `Manager`

### 4. 更新适配车型

**请求:**
```http
PUT /api/parts/{partId}/models/{modelCode}
Authorization: Bearer {token}
Content-Type: application/json

{
  "brand": "BYD",
  "region": "中国",
  "modelName": "海豚Plus",
  "productionDate": "2023-2024",
  "modelCode": "ACA4",
  "costExclTax": 110.00,
  "costInclTax": 124.30,
  "saleExclTax": 165.00,
  "saleInclTax": 186.45
}
```

**权限要求:** `admin`, `Admin`, `manager`, `Manager`

### 5. 删除适配车型

**请求:**
```http
DELETE /api/parts/{partId}/models/{modelCode}
Authorization: Bearer {token}
```

**权限要求:** `admin`, `Admin`, `manager`, `Manager`

---

## 📝 完整的零件数据结构示例

```json
{
  "id": "dolphin_p1",
  "subCategoryId": "dolphin_fb",
  "position": "60101",
  "oeNumber": "EM2EU2803111",
  "standardName": "前保险杠上本体",
  "originalName": "前保险杠上本体-Front Bumper",
  "quantity": "01",
  "note": "(ACA3#)",
  "date": "",
  "x": 1,
  "y": 7,
  "imageUrl": "/dolphin/page_38_img_2.png",
  "priceRecords": [
    {
      "brand": "BYD",
      "manufacturer": "比亚迪",
      "description": "原厂品质",
      "costExclTax": 0,
      "costInclTax": 0,
      "saleExclTax": 23.89,
      "saleInclTax": 27.0
    }
  ],
  "replacementParts": [
    {
      "brand": "BYD",
      "originalOe": "EM2EU2803111",
      "replacementOe": "EM2EU2803115",
      "note": "升级版",
      "costExclTax": 110.00,
      "costInclTax": 124.30,
      "saleExclTax": 165.00,
      "saleInclTax": 186.45
    }
  ],
  "adaptableModels": [
    {
      "brand": "BYD",
      "region": "中国",
      "modelName": "海豚",
      "productionDate": "2021-2024",
      "modelCode": "ACA3",
      "costExclTax": 100.00,
      "costInclTax": 113.00,
      "saleExclTax": 150.00,
      "saleInclTax": 169.50
    }
  ],
  "lastUpdated": "0001-01-01T00:00:00"
}
```

---

## 🧪 测试示例（使用 curl）

### 添加带价格的替换配件

```bash
curl -X POST "https://cp.xhfair.com/api/parts/dolphin_p1/replacements" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "brand": "BYD",
    "originalOe": "EM2EU2803111",
    "replacementOe": "EM2EU2803115",
    "note": "升级版",
    "costExclTax": 110.00,
    "costInclTax": 124.30,
    "saleExclTax": 165.00,
    "saleInclTax": 186.45
  }'
```

### 更新替换配件价格

```bash
curl -X PUT "https://cp.xhfair.com/api/parts/dolphin_p1/replacements/EM2EU2803115" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "brand": "BYD",
    "originalOe": "EM2EU2803111",
    "replacementOe": "EM2EU2803115",
    "note": "升级版 - 促销价",
    "costExclTax": 100.00,
    "costInclTax": 113.00,
    "saleExclTax": 140.00,
    "saleInclTax": 158.20
  }'
```

### 获取零件的所有替换配件

```bash
curl -X GET "https://cp.xhfair.com/api/parts/dolphin_p1/replacements"
```

---

## 📊 价格计算说明

### 含税价格计算（以13%增值税为例）

```javascript
// 从不含税价计算含税价
const costInclTax = costExclTax * 1.13;
const saleInclTax = saleExclTax * 1.13;

// 从含税价计算不含税价
const costExclTax = costInclTax / 1.13;
const saleExclTax = saleInclTax / 1.13;
```

---

## ⚠️ 注意事项

1. **权限控制**
   - 查询操作（GET）不需要认证
   - 增删改操作需要 `admin` 或 `manager` 角色

2. **数据验证**
   - 价格字段必须 >= 0
   - `brand`, `originalOe`, `replacementOe` 为必填字段（ReplacementPart）
   - `brand`, `modelName`, `modelCode` 为必填字段（AdaptableModel）

3. **唯一性**
   - ReplacementPart 通过 `replacementOe` 识别
   - AdaptableModel 通过 `modelCode` 识别
   - 同一个 Part 下不能有重复的 `replacementOe` 或 `modelCode`

4. **Linux 大小写问题**
   - 文件名使用小写：`parts.json`（不是 `Parts.json`）
   - API 路径不区分大小写
   - JSON 属性名遵循 camelCase

---

## 🚀 部署步骤

1. **更新现有 JSON 数据**
```powershell
# 进入项目目录
cd webApi

# 运行更新脚本
.\update-parts-prices.ps1
.\update-carparts-prices.ps1
```

2. **重新发布应用**
```powershell
.\deploy-verified.ps1
```

3. **测试 API**
```bash
# 测试获取替换配件
curl https://cp.xhfair.com/api/parts/dolphin_p1/replacements

# 测试获取适配车型
curl https://cp.xhfair.com/api/parts/dolphin_p1/models
```

---

## 📞 技术支持

如有问题，请检查：
1. JSON 文件格式是否正确
2. API 权限是否配置正确
3. 应用日志中是否有错误信息
4. 数据库/JSON 文件是否有写入权限
