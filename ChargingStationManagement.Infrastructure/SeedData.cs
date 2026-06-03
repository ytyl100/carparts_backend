// ChargingStationManagement.Infrastructure/SeedData.cs
using ChargingStationManagement.Domain.Entities;
using ChargingStationManagement.Domain.Enums;
using ChargingStationManagement.Domain.ValueObjects;
using ChargingStationManagement.Infrastructure.Identity;
using Microsoft.Extensions.Logging;

namespace ChargingStationManagement.Infrastructure.Persistence
{
    public static class SeedData
    {
        public static void Initialize(ChargingStationDbContext context)
        {
            // 确保数据库已创建
            context.Database.EnsureCreated();

            // 检查是否有数据
            if (context.Operators.Any() || context.Stations.Any() || context.Users.Any())
            {
                return; // 数据库已有数据
            }

            // 种子数据
            SeedOperators(context);
            SeedStations(context);
            SeedUsers(context);

            context.SaveChanges();
        }

        // 替换 SeedOperators 方法中的 Operator 初始化方式，使用 UpdateApiCredentials 方法设置 ApiToken 和 ApiSecret

        private static void SeedOperators(ChargingStationDbContext context)
        {
            var operators = new List<Operator>
            {
                new Operator(
                    operatorId: "123456789",
                    operatorName: "上海集成电路产业发展有限公司",
                    operatorTel: "021-50800000",
                    operatorRegAddress: "上海市浦东新区张江高科技园区",
                    operatorNote: "主要负责张江核心区的充电基础设施投建与运营。",
                    electricityProfitRate: 0.8m,
                    serviceProfitRate: 0.1m,
                    parkProfitRate: 0.1m,
                    apiBaseUrl: "https://api.guangqi.com/evcs/v1/",
                    createdBy: "system"),
                new Operator(
                    operatorId: "987654321",
                    operatorName: "杭州小橘充电科技有限公司",
                    operatorTel: "0571-88888888",
                    operatorRegAddress: "杭州市西湖区文三路",
                    operatorNote: "专注于智能充电桩研发和运营。",
                    electricityProfitRate: 0.7m,
                    serviceProfitRate: 0.2m,
                    parkProfitRate: 0.1m,
                    apiBaseUrl: "https://api.orange.com/evcs/v1/",
                    createdBy: "system"),
                new Operator(
                    operatorId: "456789123",
                    operatorName: "特斯拉（上海）有限公司",
                    operatorTel: "400-919-0707",
                    operatorRegAddress: "上海市浦东新区临港新片区",
                    operatorNote: "特斯拉超级充电网络运营商。",
                    electricityProfitRate: 0.9m,
                    serviceProfitRate: 0.05m,
                    parkProfitRate: 0.05m,
                    apiBaseUrl: "https://api.tesla.com/evcs/v1/",
                    createdBy: "system")
            };

            // 设置 ApiToken 和 ApiSecret
            operators[0].UpdateApiCredentials("guangqi_token_123", "guangqi_secret_456");
            operators[1].UpdateApiCredentials("orange_token_789", "orange_secret_012");
            operators[2].UpdateApiCredentials("tesla_token_345", "tesla_secret_678");

            context.Operators.AddRange(operators);
        }

        private static void SeedStations(ChargingStationDbContext context)
        {
            var operator1 = context.Operators.First(o => o.OperatorId == "123456789");
            var operator2 = context.Operators.First(o => o.OperatorId == "987654321");

            // 将 StationTel 和 ServiceTel 的赋值方式改为构造函数参数传递，
            // 因为 Station 的 StationTel/ServiceTel 属性没有公开的 set 访问器。
            // 需要在 Station 构造函数参数中传递这两个值。
            // 假设 Station 构造函数支持 stationTel 和 serviceTel 参数（如不支持需调整 Station 类）。
            // 下面为修正后的 SeedStations 方法相关部分：

            var stations = new List<Station>
            {
                // 广汽充电站
                new Station(
                    stationId: "GQ001",
                    operatorId: operator1.Id.ToString(),
                    stationName: "张江高科技园充电站",
                    address: new Address("上海市浦东新区张江高科技园区祖冲之路", "上海", "上海市", "中国", "201203"),
                    location: new Coordinates(31.2045m, 121.6014m),
                    source: "Guangqi",
                    createdBy: "system",
                    stationType: 1,
                    stationStatus: (int)StationStatus.Normal,
                    parkNums: 20,
                    siteGuide: "园区A栋停车场B区",
                    pictures: "https://example.com/station1.jpg;https://example.com/station2.jpg",
                    matchCars: "所有电动汽车",
                    parkInfo: "地下停车场，24小时开放",
                    businessHours: "00:00-24:00",
                    electricityRate: new Rate(1.5m, 0.2m, 0m, 0m),
                    serviceRate: new Rate(0m, 0.2m, 0m, 0m),
                    parkRate: new Rate(0m, 0m, 5m, 0m),
                    stationTel: "021-50801111", // 直接传递
                    serviceTel: "400-123-4567"  // 直接传递
                ),

                // 小橘充电站
                new Station(
                    stationId: "XJ001",
                    operatorId: operator2.Id.ToString(),
                    stationName: "西湖文化广场充电站",
                    address: new Address("杭州市西湖区文三路西湖文化广场", "杭州", "浙江省", "中国", "310000"),
                    location: new Coordinates(30.2759m, 120.1551m),
                    source: "Orange",
                    createdBy: "system",
                    stationType: 2,
                    stationStatus: (int)StationStatus.Normal,
                    parkNums: 15,
                    siteGuide: "广场地下停车场A区",
                    pictures: "https://example.com/station3.jpg",
                    matchCars: "所有电动汽车",
                    parkInfo: "地下停车场，营业时间开放",
                    businessHours: "08:00-22:00",
                    electricityRate: new Rate(1.4m, 0.25m, 0m, 0m),
                    serviceRate: new Rate(0m, 0.25m, 0m, 0m),
                    parkRate: new Rate(0m, 0m, 4m, 0m),
                    stationTel: "0571-88881111", // 直接传递
                    serviceTel: "400-789-0123"   // 直接传递
                )
            };

            context.Stations.AddRange(stations);
            context.SaveChanges();

            // 为充电站添加设备
            var station1 = context.Stations.First(s => s.StationId == "GQ001");
            var station2 = context.Stations.First(s => s.StationId == "XJ001");

            // 将 EquipmentModel、ManufacturerId、ManufacturerName、ProductionDate、Voltage、Current、PowerConfig、CommunicationProtocol、FirmwareVersion
            // 等属性的赋值方式从对象初始化器移到 Equipment 构造函数或通过公开方法/构造参数传递。
            // 假设 Equipment 没有这些属性的公开 set 访问器，需通过构造函数或方法设置。
            // 下面为修正后的 SeedStations 方法相关部分：

            var equipment = new List<Equipment>
            {
                // 广汽充电站的设备
                new Equipment(
                    equipmentId: "GQ001-EQ001",
                    stationId: station1.Id,
                    equipmentName: "一号桩",
                    equipmentType: EquipmentType.FourWheeler,
                    power: 22m,
                    source: "Guangqi",
                    createdBy: "system",
                    equipmentModel: "GQ-22KW",
                    manufacturerId: "123456789",
                    manufacturerName: "广汽充电设备",
                    productionDate: new DateTime(2023, 1, 1),
                    voltage: 220,
                    current: 100,
                    powerConfig: new PowerConfiguration(7m, 22m, 22m),
                    communicationProtocol: "OCPP1.6",
                    firmwareVersion: "1.2.3"
                ),

                new Equipment(
                    equipmentId: "GQ001-EQ002",
                    stationId: station1.Id,
                    equipmentName: "二号桩",
                    equipmentType: EquipmentType.TwoWheeler,
                    power: 7m,
                    source: "Guangqi",
                    createdBy: "system",
                    equipmentModel: "GQ-7KW",
                    manufacturerId: "123456789",
                    manufacturerName: "广汽充电设备",
                    productionDate: new DateTime(2023, 2, 1),
                    voltage: 220,
                    current: 32,
                    powerConfig: new PowerConfiguration(3m, 7m, 7m),
                    communicationProtocol: "OCPP1.6",
                    firmwareVersion: "1.2.3"
                ),
                
                // 小橘充电站的设备
                new Equipment(
                    equipmentId: "XJ001-EQ001",
                    stationId: station2.Id,
                    equipmentName: "A1充电桩",
                    equipmentType: EquipmentType.FastCharger,
                    power: 60m,
                    source: "Orange",
                    createdBy: "system",
                    equipmentModel: "XJ-60KW",
                    manufacturerId: "987654321",
                    manufacturerName: "小橘充电设备",
                    productionDate: new DateTime(2023, 3, 1),
                    voltage: 380,
                    current: 158,
                    powerConfig: new PowerConfiguration(30m, 60m, 60m),
                    communicationProtocol: "OCPP1.6",
                    firmwareVersion: "2.0.1"
                )
            };

            context.Equipment.AddRange(equipment);
            context.SaveChanges();

            // 为设备添加连接器
            var equipment1 = context.Equipment.First(e => e.EquipmentId == "GQ001-EQ001");
            var equipment2 = context.Equipment.First(e => e.EquipmentId == "GQ001-EQ002");
            var equipment3 = context.Equipment.First(e => e.EquipmentId == "XJ001-EQ001");

            // 将 Connector 的属性赋值方式改为构造函数参数或通过公开方法设置
            // 假设 Connector 没有 VoltageUpperLimits、VoltageLowerLimits、Current、ParkNo 的公开 set 访问器，
            // 需要在 Connector 构造函数中添加这些参数，或通过构造函数传递。
            // 下面为修正后的 SeedStations 方法相关部分：

            var connectors = new List<Connector>
            {
                // 广汽设备1的连接器
                new Connector(
                    connectorId: "1",
                    equipmentId: equipment1.Id,
                    standard: ConnectorStandard.GB_T,
                    power: 22m,
                    connectorName: "GB/T连接器",
                    source: "Guangqi",
                    createdBy: "system",
                    voltageUpperLimits: 500,
                    voltageLowerLimits: 200,
                    current: 100,
                    parkNo: "A01"
                ),

                // 广汽设备2的连接器
                new Connector(
                    connectorId: "1",
                    equipmentId: equipment2.Id,
                    standard: ConnectorStandard.AC,
                    power: 7m,
                    connectorName: "交流连接器",
                    source: "Guangqi",
                    createdBy: "system",
                    voltageUpperLimits: 250,
                    voltageLowerLimits: 200,
                    current: 32,
                    parkNo: "A02"
                ),

                // 小橘设备的连接器
                new Connector(
                    connectorId: "1",
                    equipmentId: equipment3.Id,
                    standard: ConnectorStandard.CCS,
                    power: 60m,
                    connectorName: "CCS快充连接器",
                    source: "Orange",
                    createdBy: "system",
                    voltageUpperLimits: 1000,
                    voltageLowerLimits: 200,
                    current: 158,
                    parkNo: "B01"
                ),

                new Connector(
                    connectorId: "2",
                    equipmentId: equipment3.Id,
                    standard: ConnectorStandard.CHAdeMO,
                    power: 60m,
                    connectorName: "CHAdeMO连接器",
                    source: "Orange",
                    createdBy: "system",
                    voltageUpperLimits: 1000,
                    voltageLowerLimits: 200,
                    current: 125,
                    parkNo: "B02"
                )
            };

            context.Connectors.AddRange(connectors);
            context.SaveChanges();
        }

        // 替换 SeedUsers 方法中 User 对象属性赋值方式，使用合适的方法或构造函数设置属性
        private static void SeedUsers(ChargingStationDbContext context)
        {
            var passwordHasher = new PasswordHasher(LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<PasswordHasher>());

            var users = new List<User>
            {
                new User(
                    userId: "USR202312010001",
                    phoneNumber: "13800138000",
                    name: "张三",
                    userType: UserType.Normal,
                    createdBy: "system"),
                new User(
                    userId: "USR202312010002",
                    phoneNumber: "13900139000",
                    name: "李四",
                    userType: UserType.Agent,
                    createdBy: "system"),
                new User(
                    userId: "USR202312010003",
                    phoneNumber: "13700137000",
                    name: "王五",
                    userType: UserType.Administrator,
                    createdBy: "system")
            };

            // 设置用户详细信息
            users[0].UpdateProfile(
                email: "zhangsan@example.com",
                name: "张三",
                dateOfBirth: new DateTime(1990, 1, 1),
                gender: Gender.Male,
                emergencyContact: null,
                emergencyPhone: null
            );
            users[1].UpdateProfile(
                email: "lisi@example.com",
                name: "李四",
                dateOfBirth: new DateTime(1991, 2, 2),
                gender: Gender.Female,
                emergencyContact: null,
                emergencyPhone: null
            );
            users[2].UpdateProfile(
                email: "admin@chargingstation.com",
                name: "王五",
                dateOfBirth: new DateTime(1988, 3, 3),
                gender: Gender.Male,
                emergencyContact: null,
                emergencyPhone: null
            );

            // 设置身份信息（如有专用方法，否则略过）
            // users[0].SetIdentityNumber("310101199001011234"); // 如果有此方法
            // users[1].SetIdentityNumber("310101199102022345");
            // users[2].SetIdentityNumber("310101198803033456");

            // 设置验证状态（如有专用方法，否则略过）
            users[0].VerifyAccount("SMS");
            users[1].VerifyAccount("SMS");
            users[2].VerifyAccount("Admin");

            // 设置注册来源（如有专用方法，否则略过）
            // users[0].SetRegistrationSource("App");
            // users[1].SetRegistrationSource("Web");
            // users[2].SetRegistrationSource("System");

            // 设置密码
            foreach (var user in users)
            {
                var (hash, salt) = passwordHasher.HashPassword("123456");
                user.SetPassword(hash, salt);
            }

            context.Users.AddRange(users);
            context.SaveChanges();

            // 其余代码保持不变
            // ...
        }
    }
}