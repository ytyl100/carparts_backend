// ChargingStationManagement.Domain/Enums/StationStatus.cs
namespace ChargingStationManagement.Domain.Enums
{
    /// <summary>
    /// 充电站状态
    /// </summary>
    public enum StationStatus
    {
        Unknown = 0,            // 未知
        UnderConstruction = 1,  // 建设中
        Offline = 5,           // 关闭下线
        Maintenance = 6,       // 维护中
        Normal = 50            // 正常使用
    }

    /// <summary>
    /// 连接器状态
    /// </summary>
    public enum ConnectorStatus
    {
        Offline = 0,           // 离网
        Idle = 1,              // 空闲
        OccupiedNotCharging = 2, // 占用(未充电)
        OccupiedCharging = 3,  // 占用(充电中)
        Reserved = 4,          // 占用(预约锁定)
        Fault = 255            // 故障
    }

    /// <summary>
    /// 停车位状态
    /// </summary>
    public enum ParkStatus
    {
        Unknown = 0,           // 未知
        Idle = 10,             // 空闲
        Occupied = 50          // 占用
    }

    /// <summary>
    /// 地锁状态
    /// </summary>
    public enum LockStatus
    {
        Unknown = 0,           // 未知
        Unlocked = 10,         // 已解锁
        Locked = 50            // 已上锁
    }

    /// <summary>
    /// 充电状态
    /// </summary>
    public enum ChargeStatus
    {
        Starting = 1,          // 启动中
        Charging = 2,          // 充电中
        Stopping = 3,          // 停止中
        Finished = 4,          // 已结束
        Unknown = 5            // 未知
    }

    /// <summary>
    /// 设备类型
    /// </summary>
    public enum EquipmentType
    {
        TwoWheeler = 1,        // 二轮充电桩
        FourWheeler = 2,       // 四轮充电桩
        FastCharger = 3,       // 快充桩
        SwapStation = 4        // 换电站
    }

    /// <summary>
    /// 充电模式
    /// </summary>
    public enum ChargingMode
    {
        TimeBased = 1,         // 按时间计费
        EnergyBased = 2,       // 按电量计费
        TimeCard = 3           // 充电时卡
    }

    /// <summary>
    /// 交易类型
    /// </summary>
    public enum TransactionType
    {
        Recharge = 1,          // 充值
        Consumption = 2,       // 消费
        Refund = 3,            // 退款
        Commission = 4         // 佣金分成
    }

    /// <summary>
    /// 用户类型
    /// </summary>
    public enum UserType
    {
        Normal = 1,            // 普通用户
        Agent = 2,             // 代理商
        Administrator = 3,     // 管理员
        System = 4             // 系统用户
    }

    /// <summary>
    /// 连接器类型标准
    /// </summary>
    public enum ConnectorStandard
    {
        GB_T = 1,              // 国标
        CCS = 2,               // 联合充电系统
        CHAdeMO = 3,           // 日本标准
        Tesla = 4,             // 特斯拉专用
        AC = 5,                 // 交流充电
        GBT_AC = 6
    }

    /// <summary>
    /// 支付方式
    /// </summary>
    public enum PaymentMethod
    {
        Wallet = 1,            // 钱包支付
        WeChat = 2,            // 微信支付
        Alipay = 3,            // 支付宝
        BankCard = 4,          // 银行卡
        Credit = 5             // 信用支付
    }

    /// <summary>
    /// 订单状态
    /// </summary>
    public enum OrderStatus
    {
        Created = 1,           // 已创建
        Charging = 2,          // 充电中
        Completed = 3,         // 已完成
        Cancelled = 4,         // 已取消
        Refunded = 5           // 已退款
    }
}