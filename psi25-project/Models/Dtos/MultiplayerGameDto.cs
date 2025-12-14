using System;
using System.Collections.Generic;

namespace psi25_project.Models.Dtos
{
    /// <summary>
    /// Represents the state of a multiplayer game including all players and the current round.
    /// Sent to frontend for real-time updates.
    /// </summary>
    public class MultiplayerGameDto
    {
        public Guid GameId { get; set; }
        public Guid RoomId { get; set; }

        public int CurrentRound { get; set; }
        public int TotalRounds { get; set; }

        public double? RoundLatitude { get; set; }
        public double? RoundLongitude { get; set; }

        public List<MultiplayerPlayerDto> Players { get; set; } = new();
    }

    /// <summary>
    /// Represents a single player's state in a multiplayer game.
    /// </summary>
    public class MultiplayerPlayerDto
    {
        public Guid PlayerId { get; set; }
        public string DisplayName { get; set; } = "";
        public int Score { get; set; }
        public bool Finished { get; set; }
        public double? LastGuessLatitude { get; set; }
        public double? LastGuessLongitude { get; set; }

        public double? DistanceMeters { get; set; }
    }

    /// <summary>
    /// Returned when a player submits a guess for a round.
    /// Indicates updated score and whether round is finished.
    /// </summary>
    public class RoundResultDto
    {
        public Guid PlayerId { get; set; }
        public int Score { get; set; }
        public double DistanceMeters { get; set; }
        public bool RoundFinished { get; set; }
    }

    /// <summary>
    /// Represents past multiplayer games for a room, used in lobby leaderboard table.
    /// </summary>
    public class GameResultDto
    {
        public Guid GameId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public List<PlayerScoreDto> PlayerScores { get; set; } = new();
    }

    /// <summary>
    /// Represents a single player's score in a past game.
    /// </summary>
    public class PlayerScoreDto
    {
        public Guid PlayerId { get; set; }
        public string DisplayName { get; set; } = "";
        public int Score { get; set; }
    }
}
