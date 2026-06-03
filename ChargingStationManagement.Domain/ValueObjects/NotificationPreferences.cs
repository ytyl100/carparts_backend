// ChargingStationManagement.Domain/ValueObjects/NotificationPreferences.cs
namespace ChargingStationManagement.Domain.ValueObjects
{
    /// <summary>
    /// 通知偏好值对象
    /// </summary>
    public class NotificationPreferences : ValueObject
    {
        public bool EmailNotifications { get; }           // 邮件通知
        public bool SmsNotifications { get; }             // 短信通知
        public bool PushNotifications { get; }            // 推送通知
        public bool MarketingNotifications { get; }       // 营销通知
        public string QuietHoursStart { get; }            // 免打扰开始时间
        public string QuietHoursEnd { get; }              // 免打扰结束时间

        public NotificationPreferences(
            bool email = true,
            bool sms = true,
            bool push = true,
            bool marketing = false,
            string quietStart = "22:00",
            string quietEnd = "08:00")
        {
            EmailNotifications = email;
            SmsNotifications = sms;
            PushNotifications = push;
            MarketingNotifications = marketing;
            QuietHoursStart = quietStart;
            QuietHoursEnd = quietEnd;
        }

        public bool IsQuietHours()
        {
            if (!TimeSpan.TryParse(QuietHoursStart, out var start) ||
                !TimeSpan.TryParse(QuietHoursEnd, out var end))
            {
                return false;
            }

            var now = DateTime.Now.TimeOfDay;

            if (start < end)
            {
                // 同一天内的免打扰时段
                return now >= start && now <= end;
            }
            else
            {
                // 跨天的免打扰时段
                return now >= start || now <= end;
            }
        }

        public bool ShouldNotify(string notificationType)
        {
            if (IsQuietHours())
                return false;

            return notificationType.ToLower() switch
            {
                "email" => EmailNotifications,
                "sms" => SmsNotifications,
                "push" => PushNotifications,
                "marketing" => MarketingNotifications,
                _ => true
            };
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return EmailNotifications;
            yield return SmsNotifications;
            yield return PushNotifications;
            yield return MarketingNotifications;
            yield return QuietHoursStart;
            yield return QuietHoursEnd;
        }
    }
}