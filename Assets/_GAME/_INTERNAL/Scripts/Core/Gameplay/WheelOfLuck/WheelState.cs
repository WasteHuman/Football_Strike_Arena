using System;

namespace Core.WheelOfLuck
{
    public sealed class WheelState
    {
        public int FreeSpins { get; set; }
        public DateTimeOffset NextAvailableUtc { get; set; }
        public bool IsSpinning { get; set; }
        public WheelReward PendingReward { get; set; }

        public bool IsAvailable => DateTimeOffset.UtcNow >= NextAvailableUtc;
    }
}
