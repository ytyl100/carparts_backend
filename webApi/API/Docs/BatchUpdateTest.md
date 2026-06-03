# 批量更新 API 测试指南

## 已修复的问题

### 1. X 和 Y 坐标类型
- **问题**: Part 模型中的 X 和 Y 是 `int` 类型，但前端发送的是浮点数
- **解决**: 已将 X 和 Y 改为 `double` 类型以支持小数坐标

### 2. 参数绑定
- **问题**: 使用 `[FromBody]` 可能导致绑定问题
- **解决**: 移除 `[FromBody]` 特性，让 ASP.NET Core 自动推断

## 测试用例

### 测试 1: 完整的批量更新（包含新增和更新）

```bash
curl -X 'POST' \
  'http://localhost:5017/api/CarParts/batch-update' \
  -H 'accept: */*' \
  -H 'Authorization: Bearer YOUR_TOKEN' \
  -H 'Content-Type: application/json' \
  -d '{
  "UPDATED_PARTS": [
    {
      "id": "1771816907051",
      "subCategoryId": "GWM-haval-H9-05",
      "position": "1001w",
      "oeNumber": "OE-000-000",
      "standardName": "新零件",
      "originalName": "New Part",
      "quantity": "01",
      "note": "",
      "date": "",
      "x": 57.567807257408916,
      "y": 28.0,
      "imageUrl": "https://img.icons8.com/fluency/144/package2.png",
      "priceRecords": [
        {
          "brand": "1",
          "manufacturer": "1",
          "description": "1",
          "costExclTax": 2,
          "costInclTax": 2,
          "saleExclTax": 1,
          "saleInclTax": 1
        }
      ],
      "replacementParts": [],
      "adaptableModels": []
    },
    {
      "id": "1771816951812",
      "subCategoryId": "GWM-haval-H9-05",
      "position": "2666y66",
      "oeNumber": "OE-000-012",
      "standardName": "新零件2",
      "originalName": "New Part2",
      "quantity": "02",
      "note": "2",
      "date": "",
      "x": 25.089301111028995,
      "y": 27.5,
      "imageUrl": "https://img.icons8.com/fluency/144/package.png",
      "priceRecords": [],
      "replacementParts": [],
      "adaptableModels": []
    }
  ],
  "SUB_CATEGORIES_UPDATE": {
    "id": "GWM-haval-H9-05",
    "name": "发动机系统 Engine system",
    "image": "/haval_h9/page_33_img_101.png"
  }
}'
```

### 测试 2: 仅更新零件

```bash
curl -X 'POST' \
  'http://localhost:5017/api/CarParts/batch-update' \
  -H 'accept: */*' \
  -H 'Authorization: Bearer YOUR_TOKEN' \
  -H 'Content-Type: application/json' \
  -d '{
  "UPDATED_PARTS": [
    {
      "id": "existing_part_id",
      "subCategoryId": "GWM-haval-H9-05",
      "position": "1001",
      "oeNumber": "OE-001",
      "standardName": "更新的零件",
      "originalName": "Updated Part",
      "quantity": "05",
      "note": "已更新",
      "date": "",
      "x": 100.5,
      "y": 200.75,
      "imageUrl": "/path/to/image.png",
      "priceRecords": [],
      "replacementParts": [],
      "adaptableModels": []
    }
  ]
}'
```

### 测试 3: 仅更新子分类

```bash
curl -X 'POST' \
  'http://localhost:5017/api/CarParts/batch-update' \
  -H 'accept: */*' \
  -H 'Authorization: Bearer YOUR_TOKEN' \
  -H 'Content-Type: application/json' \
  -d '{
  "SUB_CATEGORIES_UPDATE": {
    "id": "GWM-haval-H9-05",
    "name": "发动机系统（更新）",
    "image": "/new/path/image.png"
  }
}'
```

### 测试 4: 整数坐标（向后兼容）

```bash
curl -X 'POST' \
  'http://localhost:5017/api/CarParts/batch-update' \
  -H 'accept: */*' \
  -H 'Authorization: Bearer YOUR_TOKEN' \
  -H 'Content-Type: application/json' \
  -d '{
  "UPDATED_PARTS": [
    {
      "id": "test_integer_coords",
      "subCategoryId": "GWM-haval-H9-05",
      "position": "1001",
      "oeNumber": "OE-001",
      "standardName": "测试零件",
      "originalName": "Test Part",
      "quantity": "01",
      "note": "",
      "date": "",
      "x": 100,
      "y": 200,
      "imageUrl": "/path/to/image.png",
      "priceRecords": [],
      "replacementParts": [],
      "adaptableModels": []
    }
  ]
}'
```

## 预期响应

### 成功响应 (200 OK)
```json
{
  "updatedParts": [
    {
      "id": "1771816907051",
      "subCategoryId": "GWM-haval-H9-05",
      "position": "1001w",
      "oeNumber": "OE-000-000",
      "standardName": "新零件",
      "originalName": "New Part",
      "quantity": "01",
      "note": "",
      "date": "",
      "x": 57.567807257408916,
      "y": 28.0,
      "imageUrl": "https://img.icons8.com/fluency/144/package2.png",
      "priceRecords": [...],
      "replacementParts": [],
      "adaptableModels": [],
      "lastUpdated": "2026-02-22T12:00:00Z"
    },
    {
      "id": "1771816951812",
      "subCategoryId": "GWM-haval-H9-05",
      "position": "2666y66",
      "oeNumber": "OE-000-012",
      "standardName": "新零件2",
      "originalName": "New Part2",
      "quantity": "02",
      "note": "2",
      "date": "",
      "x": 25.089301111028995,
      "y": 27.5,
      "imageUrl": "https://img.icons8.com/fluency/144/package.png",
      "priceRecords": [],
      "replacementParts": [],
      "adaptableModels": [],
      "lastUpdated": "2026-02-22T12:00:00Z"
    }
  ],
  "updatedSubCategory": {
    "id": "GWM-haval-H9-05",
    "name": "发动机系统 Engine system",
    "code": "8701(0001)",
    "parentId": "m1_GW4C20B",
    "image": "/haval_h9/page_33_img_101.png",
    "isDefault": false,
    "createdDate": "2025-01-01T00:00:00Z",
    "lastUpdated": "2026-02-22T12:00:00Z"
  },
  "success": true,
  "message": "Batch update completed successfully"
}
```

### 错误响应示例

#### 1. 子分类不存在 (400 Bad Request)
```json
{
  "success": false,
  "message": "SubCategory with ID 'invalid_id' not found"
}
```

#### 2. 请求体为空 (400 Bad Request)
```json
{
  "success": false,
  "message": "Request body is required"
}
```

#### 3. 验证错误 (400 Bad Request)
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "$.UPDATED_PARTS[0].id": [
      "The Id field is required."
    ]
  }
}
```

## 数据类型说明

### Part 对象必填字段
- `id` (string) - 零件ID
- `subCategoryId` (string) - 子分类ID
- `position` (string, max 50) - 位置编号
- `oeNumber` (string, max 100) - OE号
- `standardName` (string, max 200) - 标准名称

### Part 对象可选字段
- `originalName` (string, max 300) - 原始名称
- `quantity` (string, max 20) - 数量
- `note` (string, max 200) - 备注
- `date` (string, max 50) - 日期
- `x` (double) - X坐标（支持小数）
- `y` (double) - Y坐标（支持小数）
- `imageUrl` (string, max 500) - 图片URL
- `priceRecords` (array) - 价格记录
- `replacementParts` (array) - 替换零件
- `adaptableModels` (array) - 适配车型

### SubCategoryUpdateDto 字段
- `id` (string, required) - 子分类ID
- `name` (string) - 名称
- `image` (string) - 图片路径

## 注意事项

1. **坐标类型**: X 和 Y 现在支持小数（double 类型），可以传入整数或浮点数
2. **认证**: 所有请求都需要有效的 JWT token
3. **子分类**: 只更新已存在的子分类，不会新增
4. **零件**: 根据 ID 判断是更新还是新增
5. **时间戳**: `lastUpdated` 会自动更新为 UTC 时间

## PowerShell 测试脚本

```powershell
# 设置变量
$token = "YOUR_JWT_TOKEN"
$baseUrl = "http://localhost:5017"

# 测试请求
$body = @{
    UPDATED_PARTS = @(
        @{
            id = "test_part_001"
            subCategoryId = "GWM-haval-H9-05"
            position = "1001"
            oeNumber = "OE-001"
            standardName = "测试零件"
            originalName = "Test Part"
            quantity = "01"
            note = ""
            date = ""
            x = 57.5
            y = 28.75
            imageUrl = "/test/image.png"
            priceRecords = @()
            replacementParts = @()
            adaptableModels = @()
        }
    )
    SUB_CATEGORIES_UPDATE = @{
        id = "GWM-haval-H9-05"
        name = "发动机系统"
        image = "/test/image.png"
    }
} | ConvertTo-Json -Depth 10

# 发送请求
$response = Invoke-RestMethod `
    -Uri "$baseUrl/api/CarParts/batch-update" `
    -Method Post `
    -Headers @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    } `
    -Body $body

# 显示结果
$response | ConvertTo-Json -Depth 10
```

## 前端 JavaScript/TypeScript 示例

```typescript
interface BatchUpdateRequest {
  UPDATED_PARTS: Part[];
  SUB_CATEGORIES_UPDATE?: {
    id: string;
    name: string;
    image: string;
  };
}

async function batchUpdateParts(data: BatchUpdateRequest, token: string) {
  try {
    const response = await fetch('http://localhost:5017/api/CarParts/batch-update', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(data)
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Batch update failed');
    }

    const result = await response.json();
    console.log('Update successful:', result);
    return result;
  } catch (error) {
    console.error('Batch update error:', error);
    throw error;
  }
}

// 使用示例
const updateData: BatchUpdateRequest = {
  UPDATED_PARTS: [
    {
      id: "1771816907051",
      subCategoryId: "GWM-haval-H9-05",
      position: "1001w",
      oeNumber: "OE-000-000",
      standardName: "新零件",
      originalName: "New Part",
      quantity: "01",
      note: "",
      date: "",
      x: 57.567807257408916,  // 支持小数
      y: 28.0,                // 支持小数
      imageUrl: "https://img.icons8.com/fluency/144/package2.png",
      priceRecords: [],
      replacementParts: [],
      adaptableModels: []
    }
  ],
  SUB_CATEGORIES_UPDATE: {
    id: "GWM-haval-H9-05",
    name: "发动机系统 Engine system",
    image: "/haval_h9/page_33_img_101.png"
  }
};

batchUpdateParts(updateData, yourToken);
```
