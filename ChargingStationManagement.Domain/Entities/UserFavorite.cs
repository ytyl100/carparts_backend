// ChargingStationManagement.Domain/Entities/UserFavorite.cs
using System;

namespace ChargingStationManagement.Domain.Entities
{
    /// <summary>
    /// 用户收藏实体
    /// </summary>
    public class UserFavorite : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string StationId { get; private set; }
        public string StationName { get; private set; }
        public DateTime AddedDate { get; private set; }
        public int VisitCount { get; private set; }
        public DateTime? LastVisited { get; private set; }
        public string Notes { get; private set; }

        public UserFavorite(Guid userId, string stationId, string stationName)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (string.IsNullOrWhiteSpace(stationId))
                throw new ArgumentException("Station ID cannot be empty", nameof(stationId));

            UserId = userId;
            StationId = stationId;
            StationName = stationName ?? "Unknown Station";
            AddedDate = DateTime.UtcNow;
            VisitCount = 0;

            CreatedBy = "system";
        }

        public void RecordVisit()
        {
            VisitCount++;
            LastVisited = DateTime.UtcNow;
            Update();
        }

        public void UpdateNotes(string notes)
        {
            Notes = notes;
            Update();
        }
    }
}