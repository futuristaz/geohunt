using System;
using System.ComponentModel.DataAnnotations;

namespace psi25_project.Models
{
    public class MultiplayerPlayer
    {
        public Guid Id { get; set; }

        public Guid GameId { get; set; }
        public MultiplayerGame Game { get; set; } = null!;

        public Guid PlayerId { get; set; }
        public Player Player { get; set; } = null!;

        public int Score { get; set; } = 0;

        // Ready/Finished in the session
        public bool IsReady { get; set; } = false;
        public bool Finished { get; set; } = false;

        // Latest guess information (optional but useful)
        public double? LastGuessLatitude { get; set; }
        public double? LastGuessLongitude { get; set; }
        public double? DistanceMeters { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;
    }
}
