// ChargingStationManagement.Domain/Entities/NotificationPreferences.cs
namespace ChargingStationManagement.Domain.Entities
{
    public class NotificationPreferences
    {
        private bool v1;
        private bool v2;
        private bool v3;
        private bool v4;

        public NotificationPreferences(bool v1, bool v2, bool v3, bool v4)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
            this.v4 = v4;
        }

        public bool EmailNotifications { get; set; }
        public bool SmsNotifications { get; set; }
        public bool PushNotifications { get; set; }
        public bool MarketingNotifications { get; set; }
        public TimeSpan? QuietHoursStart { get; set; }
        public TimeSpan? QuietHoursEnd { get; set; }
    }
}