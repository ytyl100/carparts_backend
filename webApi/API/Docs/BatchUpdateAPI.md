# 批量更新 API 文档

## 接口概述
新增的批量更新接口允许同时更新多个零件和子分类信息。

## API 端点
```
POST /api/CarParts/batch-update
```

## 请求头
```
Authorization: Bearer {your-jwt-token}
Content-Type: application/json
```

## 请求体格式

```json
{
  "UPDATED_PARTS": [
    {
      "id": "yuan_p44",
      "subCategoryId": "yuan_cc_2022",
      "position": "40002",
      "oeNumber": "EM2E10018001",
      "standardName": "动力总成右悬置支架总成1",
      "originalName": "动力总成右悬置支架总成-Right suspension bracket assembly of powert",
      "quantity": "01",
      "note": "(ACA3#)",
      "date": "",
      "x": 54,
      "y": 56,
      "imageUrl": "/yuan_plus/page_4_img_9.png",
      "priceRecords": [
        {
          "brand": "BYD",
          "manufacturer": "比亚迪",
          "description": "原厂品质",
          "costExclTax": 0,
          "costInclTax": 0,
          "saleExclTax": 23.89,
          "saleInclTax": 27,
          "currency": null
        }
      ],
      "replacementParts": [],
      "adaptableModels": []
    },
    {
      "id": "1771813658402",
      "subCategoryId": "yuan_cc_2022",
      "position": "00000",
      "oeNumber": "OE-000-000",
      "standardName": "新零件4",
      "originalName": "New Part",
      "quantity": "01",
      "note": "444",
      "date": "",
      "x": 50,
      "y": 50,
      "imageUrl": "https://img.icons8.com/fluency/144/package.png",
      "priceRecords": [
        {
          "brand": "1",
          "manufacturer": "1",
          "description": "1",
          "costExclTax": 0,
          "costInclTax": 0,
          "saleExclTax": 0,
          "saleInclTax": 0
        }
      ],
      "replacementParts": [
        {
          "brand": "11",
          "originalOe": "OE-000-000",
          "replacementOe": "1",
          "note": "1"
        }
      ],
      "adaptableModels": [
        {
          "brand": "11",
          "region": "1",
          "modelName": "1",
          "productionDate": "1",
          "modelCode": "1"
        }
      ]
    }
  ],
  "SUB_CATEGORIES_UPDATE": {
    "id": "yuan_cc_2022",
    "name": "底盘件 Chassis components2",
    "image": "/yuan_plus/page_4_img_10.png"
  }
}
```

## 功能说明

### 零件更新 (UPDATED_PARTS)
- 如果零件 ID 已存在于 `carparts.json` 中，则**更新**该零件
- 如果零件 ID 不存在，则**新增**该零件
- 更新时会自动设置 `lastUpdated` 字段为当前UTC时间

### 子分类更新 (SUB_CATEGORIES_UPDATE)
- 仅当子分类 ID 存在于 `subcategorys.json` 中时才会更新
- 只更新提供的字段（`name` 和 `image`）
- 更新时会自动设置 `lastUpdated` 字段为当前UTC时间
- **不会新增**子分类

## 响应格式

### 成功响应 (200 OK)
```json
{
  "updatedParts": [
    {
      "id": "yuan_p44",
      "subCategoryId": "yuan_cc_2022",
      "position": "40002",
      "oeNumber": "EM2E10018001",
      "standardName": "动力总成右悬置支架总成1",
      "originalName": "动力总成右悬置支架总成-Right suspension bracket assembly of powert",
      "quantity": "01",
      "note": "(ACA3#)",
      "date": "",
      "x": 54,
      "y": 56,
      "imageUrl": "/yuan_plus/page_4_img_9.png",
      "priceRecords": [...],
      "replacementParts": [],
      "adaptableModels": [],
      "lastUpdated": "2026-02-22T10:30:00Z"
    },
    ...
  ],
  "updatedSubCategory": {
    "id": "yuan_cc_2022",
    "name": "底盘件 Chassis components2",
    "code": "8105(000100)",
    "parentId": "m2_BYD7003BEVA3",
    "image": "/yuan_plus/page_4_img_10.png",
    "isDefault": false,
    "createdDate": "2025-01-01T00:00:00Z",
    "lastUpdated": "2026-02-22T10:30:00Z"
  },
  "success": true,
  "message": "Batch update completed successfully"
}
```

### 错误响应 - 子分类不存在 (400 Bad Request)
```json
{
  "success": false,
  "message": "SubCategory with ID 'yuan_cc_2022' not found"
}
```

### 错误响应 - 批量更新失败 (400 Bad Request)
```json
{
  "success": false,
  "message": "Batch update failed",
  "error": "详细错误信息"
}
```

## 使用示例

### 使用 cURL
```bash
curl -X POST "https://your-api.com/api/CarParts/batch-update" \
  -H "Authorization: Bearer your-jwt-token" \
  -H "Content-Type: application/json" \
  -d @batch-update-request.json
```

### 使用 JavaScript (Fetch API)
```javascript
const response = await fetch('https://your-api.com/api/CarParts/batch-update', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    UPDATED_PARTS: [...],
    SUB_CATEGORIES_UPDATE: {...}
  })
});

const result = await response.json();
console.log(result);
```

## 注意事项

1. **认证要求**: 此接口需要有效的 JWT token
2. **数据验证**: 所有必填字段都会进行验证
3. **原子性**: 零件更新和子分类更新是独立的操作
4. **ID 格式**: 确保使用正确的 ID 格式
5. **时间戳**: `lastUpdated` 字段会自动更新为 UTC 时间
6. **子分类限制**: 子分类只能更新已存在的记录，不能新增

## 实现细节

### 代码位置
- **Controller**: `webApi/API/Controllers/CarPartsController.cs`
- **Service**: `webApi/API/Services/CarPartService.cs`
- **SubCategory Service**: `webApi/API/Services/SubCategoryService.cs`
- **DTO**: `webApi/API/Models/DTOs/BatchUpdateRequest.cs`

### 数据存储
- 零件数据存储在: `webApi/API/Data/carparts.json`
- 子分类数据存储在: `webApi/API/Data/subcategorys.json`
