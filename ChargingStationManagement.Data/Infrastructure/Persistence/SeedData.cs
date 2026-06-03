// ChargingStationManagement.Infrastructure/Persistence/SeedData.cs
using ChargingStationManagement.Domain.Entities;
using ChargingStationManagement.Domain.Enums;
using ChargingStationManagement.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChargingStationManagement.Infrastructure.Persistence
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext context)
        {
            // Ensure database is created
            context.Database.EnsureCreated();

            // Check if already seeded
            if (context.Operators.Any() || context.Stations.Any() || context.Users.Any())
                return;
            SeedRoles(context);
            // 1. Operators
            SeedOperators(context);

            // 2. Stations, Equipment, Connectors
            SeedStations(context);

            // 3. Users & Wallets
            SeedUsersAndWallets(context);

            context.SaveChanges();
        }

        private static void SeedOperators(AppDbContext context)
        {
            var operators = new[]
            {
                new Operator("123456789", "上海集成电路产业发展有限公司"),
                new Operator("987654321", "杭州小橘充电科技有限公司"),
                new Operator("456789123", "特斯拉（上海）有限公司")
            };

            // Set API credentials
            operators[0].UpdateApiCredentials("guangqi_token_123", "guangqi_secret_456");
            operators[1].UpdateApiCredentials("orange_token_789", "orange_secret_012");
            operators[2].UpdateApiCredentials("tesla_token_345", "tesla_secret_678");

            context.Operators.AddRange(operators);
            context.SaveChanges();
        }

        private static void SeedStations(AppDbContext context)
        {
            var operator1 = context.Operators.First(o => o.OperatorId == "123456789");
            var operator2 = context.Operators.First(o => o.OperatorId == "987654321");

            // --- Station 1: 张江高科技园充电站 ---
            var station1 = new Station(
                stationId: "GQ001",
                operatorId: operator1.Id.ToString(),
                stationName: "张江高科技园充电站",
                address: new Address("上海市浦东新区张江高科技园区祖冲之路"),
                location: new Coordinates(31.2045m, 121.6014m),
                source: "Guangqi",
                createdBy: "system"
            );

            // Update detailed info
            station1.UpdateBasicInfo(
                name: "张江高科技园充电站",
                address: new Address("上海市浦东新区张江高科技园区祖冲之路"),
                stationTel: "021-50801111",
                serviceTel: "400-123-4567",
                siteGuide: "园区A栋停车场B区",
                businessHours: "00:00-24:00"
            );

            // Set pictures (needs method – see addition below)
            station1.UpdatePictures("https://example.com/station1.jpg;https://example.com/station2.jpg");

            // Set park info (needs method – see addition below)
            station1.UpdateParkInfo("地下停车场，24小时开放");

            // Set rates
            var electricityRate = new Rate(1.5m, 0.2m, 0m, 0m);
            var serviceRate = new Rate(0m, 0.2m, 0m, 0m);
            var parkRate = new Rate(0m, 0m, 5m, 0m);
            station1.UpdateRates(electricityRate, serviceRate, parkRate);

            // --- Station 2: 西湖文化广场充电站 ---
            var station2 = new Station(
                stationId: "XJ001",
                operatorId: operator2.Id.ToString(),
                stationName: "西湖文化广场充电站",
                address: new Address("杭州市西湖区文三路西湖文化广场"),
                location: new Coordinates(30.2759m, 120.1551m),
                source: "Orange",
                createdBy: "system"
            );

            station2.UpdateBasicInfo(
                name: "西湖文化广场充电站",
                address: new Address("杭州市西湖区文三路西湖文化广场"),
                stationTel: "0571-88881111",
                serviceTel: "400-789-0123",
                siteGuide: "广场地下停车场A区",
                businessHours: "08:00-22:00"
            );

            station2.UpdatePictures("https://example.com/station3.jpg");
            station2.UpdateParkInfo("地下停车场，营业时间开放");

            var electricityRate2 = new Rate(1.4m, 0.25m, 0m, 0m);
            var serviceRate2 = new Rate(0m, 0.25m, 0m, 0m);
            var parkRate2 = new Rate(0m, 0m, 4m, 0m);
            station2.UpdateRates(electricityRate2, serviceRate2, parkRate2);

            context.Stations.AddRange(station1, station2);
            context.SaveChanges();

            // --- Equipment for Station 1 ---
            var equipment1 = new Equipment(
                equipmentId: "GQ001-EQ001",
                stationId: station1.Id,
                equipmentName: "一号桩",
                equipmentType: EquipmentType.FourWheeler,
                power: 22m,
                source: "Guangqi"
            );
            equipment1.UpdateTechnicalSpecs(
                name: "一号桩",
                manufacturer: "广汽充电设备",
                voltage: 220,
                current: 100,
                protocol: "OCPP1.6",
                firmware: "1.2.3"
            );

            var equipment2 = new Equipment(
                equipmentId: "GQ001-EQ002",
                stationId: station1.Id,
                equipmentName: "二号桩",
                equipmentType: EquipmentType.TwoWheeler,
                power: 7m,
                source: "Guangqi"
            );
            equipment2.UpdateTechnicalSpecs(
                name: "二号桩",
                manufacturer: "广汽充电设备",
                voltage: 220,
                current: 32,
                protocol: "OCPP1.6",
                firmware: "1.2.3"
            );

            // --- Equipment for Station 2 ---
            var equipment3 = new Equipment(
                equipmentId: "XJ001-EQ001",
                stationId: station2.Id,
                equipmentName: "A1充电桩",
                equipmentType: EquipmentType.FastCharger,
                power: 60m,
                source: "Orange"
            );
            equipment3.UpdateTechnicalSpecs(
                name: "A1充电桩",
                manufacturer: "小橘充电设备",
                voltage: 380,
                current: 158,
                protocol: "OCPP1.6",
                firmware: "2.0.1"
            );

            context.Equipment.AddRange(equipment1, equipment2, equipment3);
            context.SaveChanges();

            // --- Connectors ---
            // Connector for equipment1
            var connector1 = new Connector(
                connectorId: "GQ001-EQ001-1001",
                equipmentId: equipment1.Id,
                standard: ConnectorStandard.GB_T,
                power: 22m,
                connectorName: "GB/T连接器",
                source: "Guangqi"
            );
            connector1.SetTechnicalSpecs(500, 200, 100, "A01");

            // Connector for equipment2
            var connector2 = new Connector(
                connectorId: "GQ001-EQ001-1002",
                equipmentId: equipment2.Id,
                standard: ConnectorStandard.AC,
                power: 7m,
                connectorName: "交流连接器",
                source: "Guangqi"
            );
            connector2.SetTechnicalSpecs(250, 200, 32, "A02");

            // Connectors for equipment3 (two connectors)
            var connector3 = new Connector(
                connectorId: "XJ001-EQ001-2001",
                equipmentId: equipment3.Id,
                standard: ConnectorStandard.CCS,
                power: 60m,
                connectorName: "CCS快充连接器",
                source: "Orange"
            );
            connector3.SetTechnicalSpecs(1000, 200, 158, "B01");

            var connector4 = new Connector(
                connectorId: "XJ001-EQ001-2002",
                equipmentId: equipment3.Id,
                standard: ConnectorStandard.CHAdeMO,
                power: 60m,
                connectorName: "CHAdeMO连接器",
                source: "Orange"
            );
            connector4.SetTechnicalSpecs(1000, 200, 125, "B02");

            context.Connectors.AddRange(connector1, connector2, connector3, connector4);
            context.SaveChanges();

            // Link connectors to equipment (already done via foreign key)
        }

        //private static void SeedUsersAndWallets(AppDbContext context)
        //{
        //    // Create users
        //    var users = new[]
        //    {
        //        new User("USR202312010001", "张三"),
        //        new User("USR202312010002", "李四"),
        //        new User("USR202312010003", "王五")
        //    };

        //    context.Users.AddRange(users);
        //    context.SaveChanges();

        //    // Create wallets for each user with initial balance
        //    foreach (var user in users)
        //    {
        //        var wallet = new Wallet($"W{user.UserId}", user.Id);
        //        wallet.Deposit(1000m); // initial balance (method added – see below)
        //        context.Wallets.Add(wallet);
        //    }

        //    context.SaveChanges();
        //}

        private static void SeedRoles(AppDbContext context)
        {
            var roles = new[]
            {
        new Role("normal", "Regular user with basic privileges"),
        new Role("contributor", "Can edit content, manage own data"),
        new Role("admin", "Can manage users and content"),
        new Role("super_admin", "Full system access")
    };

            context.Roles.AddRange(roles);
            context.SaveChanges();
        }

        private static void SeedUsersAndWallets(AppDbContext context)
        {
            // Ensure roles exist
            var normalRole = context.Roles.First(r => r.Name == "normal");
            var contributorRole = context.Roles.First(r => r.Name == "contributor");
            var adminRole = context.Roles.First(r => r.Name == "admin");
            var superAdminRole = context.Roles.First(r => r.Name == "super_admin");

            // Create users
            var user1 = new User("USR202312010001", "张三");
            var user2 = new User("USR202312010002", "李四");
            var user3 = new User("USR202312010003", "王五");

            // Approve and set passwords
            user1.Approve("system");
            user2.Approve("system");
            user3.Approve("system");

            user1.SetPassword("123456");
            user2.SetPassword("123456");
            user3.SetPassword("123456");

            // Add users first
            context.Users.AddRange(user1, user2, user3);
            context.SaveChanges();  // 先保存用户

            // Now explicitly create UserRole entities
            var userRole1 = new UserRole(user1.Id, normalRole.Id, "system");
            var userRole2 = new UserRole(user2.Id, contributorRole.Id, "system");
            var userRole3 = new UserRole(user3.Id, adminRole.Id, "system");

            context.Set<UserRole>().AddRange(userRole1, userRole2, userRole3);
            context.SaveChanges();  // 保存角色关系

            // Create wallets
            var wallet1 = new Wallet($"W{user1.UserId}", user1.Id);
            var wallet2 = new Wallet($"W{user2.UserId}", user2.Id);
            var wallet3 = new Wallet($"W{user3.UserId}", user3.Id);

            wallet1.Deposit(1000m);
            wallet2.Deposit(1000m);
            wallet3.Deposit(1000m);

            context.Wallets.AddRange(wallet1, wallet2, wallet3);
            context.SaveChanges();  // 保存钱包
}

        public static async Task SeedRolesAsync(AppDbContext context)
        {
            if (!context.Roles.Any())
            {
                var roles = new[]
                {
                    new Role("normal", "Regular user with basic access"),
                    new Role("contributor", "User with contribution rights"),
                    new Role("admin", "Administrator with full access"),
                    new Role("super_admin", "Super administrator with system-wide access")
                };

                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
            }
        }
    }
}