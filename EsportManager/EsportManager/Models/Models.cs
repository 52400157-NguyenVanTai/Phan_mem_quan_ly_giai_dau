namespace EsportManager.Models
{
    public enum GameType
    {
        LienQuan,   // Arena of Valor
        FreeFire    // Garena Free Fire
    }

    public enum TournamentFormat { SingleElimination, DoubleElimination, RoundRobin, Swiss }
    public enum MatchFormat { BO1, BO3, BO5 }
    public enum TournamentStatus { Upcoming, Registration, CheckIn, Ongoing, Completed, Cancelled }
    public enum MatchStatus { Scheduled, CheckIn, Live, Completed, Disputed }
    public enum RegistrationStatus { Pending, Approved, Rejected, CheckedIn }
    public enum PlayerRole
    {
        // Liên Quân roles
        Jungler, Support, Carry, Mid, Roamer,
        // Free Fire roles  
        IGL, Fragger, Sniper, Scout
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string IngameName { get; set; } = "";
        public string GameUID_LQ { get; set; } = "";   // Liên Quân UID
        public string GameUID_FF { get; set; } = "";   // Free Fire UID
        public string Avatar { get; set; } = "";
        public string Rank_LQ { get; set; } = "Unranked";   // Liên Quân rank
        public string Rank_FF { get; set; } = "Bronze";     // Free Fire rank
        public bool IsVerified { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsBanned { get; set; }
        public int TotalWins { get; set; }
        public int TotalLosses { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Tag { get; set; } = "";        // Short team tag e.g. "DRG"
        public string Logo { get; set; } = "";
        public string Description { get; set; } = "";
        public GameType PrimaryGame { get; set; }
        public int CaptainId { get; set; }
        public User? Captain { get; set; }
        public List<TeamMember> Members { get; set; } = new();
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Points { get; set; }
        public string Country { get; set; } = "VN";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class TeamMember
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public string Role { get; set; } = "Member";   // Captain / Member / Substitute
        public string InGameRole { get; set; } = "";   // Jungler, IGL, etc.
        public DateTime JoinedAt { get; set; } = DateTime.Now;
    }

    public class Tournament
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public string Description { get; set; } = "";
        public string Rules { get; set; } = "";
        public string Banner { get; set; } = "";
        public GameType Game { get; set; }
        public TournamentFormat Format { get; set; }
        public MatchFormat MatchFormat { get; set; }
        public int MaxTeams { get; set; }
        public int MinPlayers { get; set; } = 5;
        public int MaxPlayers { get; set; } = 8;
        public TournamentStatus Status { get; set; }
        public string Prize1st { get; set; } = "";
        public string Prize2nd { get; set; } = "";
        public string Prize3rd { get; set; } = "";
        public string PrizePool { get; set; } = "";
        public DateTime RegistrationStart { get; set; }
        public DateTime RegistrationEnd { get; set; }
        public DateTime CheckInStart { get; set; }
        public DateTime TournamentStart { get; set; }
        public DateTime TournamentEnd { get; set; }
        public int OrganizerId { get; set; }
        public User? Organizer { get; set; }
        public List<TournamentRegistration> Registrations { get; set; } = new();
        public List<Match> Matches { get; set; } = new();
        public bool IsFeatured { get; set; }
        public bool IsPublic { get; set; } = true;
        public int ViewCount { get; set; }
        public string Region { get; set; } = "Vietnam";
    }

    public class TournamentRegistration
    {
        public int Id { get; set; }
        public int TournamentId { get; set; }
        public Tournament? Tournament { get; set; }
        public int TeamId { get; set; }
        public Team? Team { get; set; }
        public RegistrationStatus Status { get; set; } = RegistrationStatus.Pending;
        public string? Roster { get; set; }    // JSON list of player IDs
        public DateTime RegisteredAt { get; set; } = DateTime.Now;
        public string? Note { get; set; }
    }

    public class Match
    {
        public int Id { get; set; }
        public int TournamentId { get; set; }
        public Tournament? Tournament { get; set; }
        public int Round { get; set; }
        public string RoundName { get; set; } = "";
        public int? BracketPosition { get; set; }
        public int Team1Id { get; set; }
        public Team? Team1 { get; set; }
        public int Team2Id { get; set; }
        public Team? Team2 { get; set; }
        public int? WinnerId { get; set; }
        public Team? Winner { get; set; }
        public int Team1Score { get; set; }
        public int Team2Score { get; set; }
        public MatchStatus Status { get; set; } = MatchStatus.Scheduled;
        public DateTime ScheduledAt { get; set; }
        public string? RoomId { get; set; }
        public string? RoomPassword { get; set; }
        public string? StreamUrl { get; set; }
        public string? ProofUrl { get; set; }
        public bool Team1Confirmed { get; set; }
        public bool Team2Confirmed { get; set; }
        public string? Notes { get; set; }
        // Game-specific data
        public string? GameData { get; set; }  // JSON for kills, MVP, etc.
    }

    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string Type { get; set; } = "info";   // info/success/warning/danger
        public string? Link { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
