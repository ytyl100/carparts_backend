VehicleHierarchy API
=============================
# 获取所有车辆层级
GET /api/VehicleHierarchy

# 根据品牌ID获取车辆层级
GET /api/VehicleHierarchy/brands/5

# 获取所有车辆代码
GET /api/VehicleHierarchy/codes

# 根据代码查询车辆信息
GET /api/VehicleHierarchy/codes/BYD7009BEV

# 获取品牌的所有车辆代码
GET /api/VehicleHierarchy/brands/5/codes

# 获取品牌和地区的车辆代码
GET /api/VehicleHierarchy/brands/5/regions/一般地区/codes

# 获取品牌和车型的车辆代码
GET /api/VehicleHierarchy/brands/5/models/汉 EV/codes

# 创建品牌层级
POST /api/VehicleHierarchy/brands/9
{
  "name": "新品牌",
  "regions": {...}
}

# 更新品牌层级
PUT /api/VehicleHierarchy/brands/9

# 删除品牌层级
DELETE /api/VehicleHierarchy/brands/9

# 添加地区
POST /api/VehicleHierarchy/brands/5/regions/新地区

# 添加车型
POST /api/VehicleHierarchy/brands/5/regions/一般地区/models/新车型

MainCategory API
===============================
# 获取所有主分类
GET /api/MainCategories

# 根据ID获取主分类
GET /api/MainCategories/m1_BYD7009BEV

# 根据车辆代码获取主分类
GET /api/MainCategories/vehicle/BYD7009BEV

# 获取默认主分类
GET /api/MainCategories/defaults

# 创建主分类
POST /api/MainCategories
{
  "name": "高压电池/电机/电控",
  "icon": "fa-battery-full",
  "vehicleCode": "BYD7009BEV",
  "isDefault": false
}

# 更新主分类
PUT /api/MainCategories/m1_BYD7009BEV

# 删除主分类
DELETE /api/MainCategories/m1_BYD7009BEV

SubCategory API
===============================
# 获取所有子分类
GET /api/SubCategories

# 根据ID获取子分类
GET /api/SubCategories/dolphin_fb

# 根据父ID获取子分类
GET /api/SubCategories/parent/BYD7004BEV13

# 获取默认子分类
GET /api/SubCategories/defaults

# 创建子分类
POST /api/SubCategories
{
  "name": "前保险杠及附件",
  "code": "8101(0001)",
  "parentId": "BYD7004BEV13",
  "image": "/dolphin/page_38_img_1.png",
  "isDefault": false
}

# 更新子分类
PUT /api/SubCategories/dolphin_fb

# 删除子分类
DELETE /api/SubCategories/dolphin_fb

Parts API
===============================
# 获取所有零件
GET /api/Parts

# 根据ID获取零件
GET /api/Parts/dolphin_p1

# 根据子分类ID获取零件
GET /api/Parts/subcategory/dolphin_fb

# 根据OE号获取零件
GET /api/Parts/oe/EM2EU2803111

# 根据位置获取零件
GET /api/Parts/position/60101

# 搜索零件
POST /api/Parts/search
{
  "subCategoryId": "dolphin_fb",
  "oeNumber": "EM2EU",
  "standardName": "保险杠",
  "minPrice": 10,
  "maxPrice": 50,
  "brand": "BYD"
}

# 创建零件
POST /api/Parts
{
  "subCategoryId": "dolphin_fb",
  "position": "60101",
  "oeNumber": "EM2EU2803111",
  "standardName": "前保险杠上本体",
  "originalName": "前保险杠上本体-Front Bumper",
  "quantity": "01",
  "note": "(ACA3#)",
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
  "replacementParts": [],
  "adaptableModels": []
}

# 更新零件
PUT /api/Parts/dolphin_p1

# 删除零件
DELETE /api/Parts/dolphin_p1