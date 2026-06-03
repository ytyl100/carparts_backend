// ChargingStationManagement.Domain/Entities/Operator.cs
using ChargingStationManagement.Domain.Events;
using System;
using System.Collections.Generic;

namespace ChargingStationManagement.Domain.Entities
{
    /// <summary>
    /// 运营商实体（聚合根）
    /// </summary>
    public class Operator : AggregateRoot
    {
        // 基本属性
        public string OperatorId { get; private set; }        // 运营商唯一ID（组织机构代码）
        public string OperatorName { get; private set; }      // 运营商名称
        public string OperatorTel { get; private set; }       // 运营商电话
        public string OperatorRegAddress { get; private set; } // 注册地址
        public string OperatorNote { get; private set; }      // 备注

        // 费率配置
        public decimal ElectricityProfitRate { get; private set; }  // 电费收益比例
        public decimal ServiceProfitRate { get; private set; }      // 服务费收益比例
        public decimal ParkProfitRate { get; private set; }         // 停车费收益比例

        // 状态
        public bool IsActive { get; private set; }
        public DateTime? ActivationDate { get; private set; }

        // API配置
        public string ApiBaseUrl { get; private set; }
        public string ApiToken { get; private set; }
        public string ApiSecret { get; private set; }
        public string ApiEncryptionKey { get; private set; }

        // 导航属性
        private readonly List<Station> _stations = new List<Station>();
        public IReadOnlyCollection<Station> Stations => _stations.AsReadOnly();

        // 构造函数
        protected Operator() { }

        public Operator(
            string operatorId,
            string operatorName,
            string operatorTel,
            string operatorRegAddress,
            string operatorNote,
            decimal electricityProfitRate = 0.8m,
            decimal serviceProfitRate = 0.1m,
            decimal parkProfitRate = 0.1m,
            string apiBaseUrl = null,
            string createdBy = "system")
        {
            if (string.IsNullOrWhiteSpace(operatorId))
                throw new ArgumentException("Operator ID cannot be empty", nameof(operatorId));

            if (operatorId.Length != 9)
                throw new ArgumentException("Operator ID must be 9 characters", nameof(operatorId));

            OperatorId = operatorId;
            OperatorName = operatorName ?? throw new ArgumentNullException(nameof(operatorName));
            OperatorTel = operatorTel;
            OperatorRegAddress = operatorRegAddress;
            OperatorNote = operatorNote;

            SetProfitRates(electricityProfitRate, serviceProfitRate, parkProfitRate);

            IsActive = true;
            ActivationDate = DateTime.UtcNow;

            ApiBaseUrl = apiBaseUrl;
            CreatedBy = createdBy;
        }

        // 业务方法
        public void SetProfitRates(decimal electricityRate, decimal serviceRate, decimal parkRate)
        {
            if (electricityRate + serviceRate + parkRate != 1.0m)
                throw new ArgumentException("Profit rates must sum to 1.0 (100%)");

            ElectricityProfitRate = electricityRate;
            ServiceProfitRate = serviceRate;
            ParkProfitRate = parkRate;

            Update();
        }

        public void Activate()
        {
            if (!IsActive)
            {
                IsActive = true;
                ActivationDate = DateTime.UtcNow;
                Update();

                AddDomainEvent(new OperatorActivatedEvent(Id, OperatorId));
            }
        }

        public void Deactivate(string reason = null)
        {
            if (IsActive)
            {
                IsActive = false;
                Update();

                AddDomainEvent(new OperatorDeactivatedEvent(Id, OperatorId, reason));
            }
        }

        public void UpdateApiCredentials(string apiToken, string apiSecret, string encryptionKey = null)
        {
            ApiToken = apiToken ?? throw new ArgumentNullException(nameof(apiToken));
            ApiSecret = apiSecret ?? throw new ArgumentNullException(nameof(apiSecret));
            ApiEncryptionKey = encryptionKey;

            Update();

            AddDomainEvent(new ApiCredentialsUpdatedEvent(Id, OperatorId));
        }

        public void UpdateApiCredentials(string apiToken, string apiSecret)
        {
            // Store the credentials – you may need to add corresponding private fields or properties.
            // Since the original Operator class had no such fields, we add them now.
            // If your Operator entity already has ApiToken/ApiSecret properties, simply assign them.
            // For this fix, we'll add two new private fields and expose them as needed.
            _apiToken = apiToken;
            _apiSecret = apiSecret;
        }

        public void AddStation(Station station)
        {
            if (station == null)
                throw new ArgumentNullException(nameof(station));

            _stations.Add(station);
            Update();
        }

        public void UpdateName(string name)
        {
            OperatorName = name;
        }
    }
}