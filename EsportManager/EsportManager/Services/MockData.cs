//using EsportManager.Models;

//namespace EsportManager.Services
//{
//    public static class MockData
//    {
//        public static List<User> Users = new()
//        {
//            new User { Id=1, Username="admin", Email="admin@esport.vn", IngameName="AdminGod", GameUID_LQ="LQ#100001", GameUID_FF="FF#200001", Rank_LQ="Thách Đấu", Rank_FF="Heroic", IsAdmin=true, IsVerified=true, TotalWins=0, TotalLosses=0 },
//            new User { Id=2, Username="shadow_x", Email="shadow@gmail.com", IngameName="ShadowX", GameUID_LQ="LQ#112233", GameUID_FF="FF#445566", Rank_LQ="Cao Thủ", Rank_FF="Grandmaster", IsVerified=true, TotalWins=45, TotalLosses=12 },
//            new User { Id=3, Username="phoenix_vn", Email="phoenix@gmail.com", IngameName="PhoenixVN", GameUID_LQ="LQ#998877", GameUID_FF="FF#112244", Rank_LQ="Thách Đấu", Rank_FF="Heroic", IsVerified=true, TotalWins=62, TotalLosses=18 },
//            new User { Id=4, Username="blaze_fire", Email="blaze@gmail.com", IngameName="BlazeKing", GameUID_LQ="LQ#556677", GameUID_FF="FF#778899", Rank_LQ="Kim Cương", Rank_FF="Diamond", IsVerified=true, TotalWins=38, TotalLosses=21 },
//            new User { Id=5, Username="storm_rider", Email="storm@gmail.com", IngameName="StormRider", GameUID_LQ="LQ#334455", GameUID_FF="FF#223344", Rank_LQ="Thách Đấu", Rank_FF="Grandmaster", IsVerified=false, TotalWins=29, TotalLosses=15 },
//            new User { Id=6, Username="dark_blade", Email="dark@gmail.com", IngameName="DarkBlade", GameUID_LQ="LQ#667788", GameUID_FF="FF#889900", Rank_LQ="Cao Thủ", Rank_FF="Heroic", IsVerified=true, TotalWins=51, TotalLosses=23 },
//        };

//        public static List<Team> Teams = new()
//        {
//            new Team { Id=1, Name="Team Shadow", Tag="SDW", PrimaryGame=GameType.LienQuan, CaptainId=2, Wins=18, Losses=4, Points=2150, Country="VN", Description="Đội Liên Quân hàng đầu miền Nam" },
//            new Team { Id=2, Name="Phoenix Esports", Tag="PHX", PrimaryGame=GameType.LienQuan, CaptainId=3, Wins=22, Losses=6, Points=2380, Country="VN", Description="Vô địch VCS mùa hè 2025" },
//            new Team { Id=3, Name="Blaze Gaming", Tag="BLZ", PrimaryGame=GameType.FreeFire, CaptainId=4, Wins=15, Losses=8, Points=1920, Country="VN", Description="Free Fire Squad chuyên nghiệp" },
//            new Team { Id=4, Name="Storm Raiders", Tag="STM", PrimaryGame=GameType.FreeFire, CaptainId=5, Wins=12, Losses=10, Points=1740, Country="VN", Description="Rush play specialists" },
//            new Team { Id=5, Name="Dark Legion", Tag="DRK", PrimaryGame=GameType.LienQuan, CaptainId=6, Wins=20, Losses=7, Points=2260, Country="VN", Description="Strategic & disciplined gameplay" },
//            new Team { Id=6, Name="Neon Wolves", Tag="NWF", PrimaryGame=GameType.FreeFire, CaptainId=2, Wins=9, Losses=13, Points=1580, Country="VN", Description="Rising stars từ Hà Nội" },
//            new Team { Id=7, Name="Inferno Squad", Tag="INF", PrimaryGame=GameType.FreeFire, CaptainId=3, Wins=16, Losses=6, Points=2050, Country="VN", Description="TOP 3 Free Fire National 2025" },
//            new Team { Id=8, Name="Dragon Force", Tag="DRG", PrimaryGame=GameType.LienQuan, CaptainId=4, Wins=11, Losses=12, Points=1690, Country="VN", Description="Comeback kings" },
//        };

//        public static List<Tournament> Tournaments = new()
//        {
//            new Tournament {
//                Id=1, Name="FAG Liên Quân Mobile Open Cup 2026", Slug="fag-lq-open-2026",
//                Game=GameType.LienQuan, Format=TournamentFormat.SingleElimination,
//                MatchFormat=MatchFormat.BO3, MaxTeams=16, Status=TournamentStatus.Ongoing,
//                RegistrationStart=new DateTime(2026,3,1), RegistrationEnd=new DateTime(2026,3,20),
//                CheckInStart=new DateTime(2026,3,21), TournamentStart=new DateTime(2026,3,22),
//                TournamentEnd=new DateTime(2026,4,30),
//                OrganizerId=1, IsFeatured=true, IsPublic=true, ViewCount=12450, Region="Vietnam",
//                PrizePool="30,000,000 VNĐ", Prize1st="15,000,000 VNĐ", Prize2nd="9,000,000 VNĐ", Prize3rd="6,000,000 VNĐ",
//                Description="Giải đấu Liên Quân Mobile lớn nhất mùa xuân 2026 dành cho tất cả game thủ Việt Nam.",
//                Rules="- Mỗi đội 5-7 người\n- Check-in bắt buộc trước 30 phút\n- Dùng Custom Room\n- Nghiêm cấm cheat/hack\n- Admin có quyền quyết định cuối cùng"
//            },
//            new Tournament {
//                Id=2, Name="Free Fire Squad Championship 2026", Slug="ff-squad-champ-2026",
//                Game=GameType.FreeFire, Format=TournamentFormat.RoundRobin,
//                MatchFormat=MatchFormat.BO1, MaxTeams=12, Status=TournamentStatus.Registration,
//                RegistrationStart=new DateTime(2026,4,1), RegistrationEnd=new DateTime(2026,4,25),
//                CheckInStart=new DateTime(2026,4,26), TournamentStart=new DateTime(2026,4,27),
//                TournamentEnd=new DateTime(2026,5,20),
//                OrganizerId=1, IsFeatured=true, IsPublic=true, ViewCount=8920, Region="Vietnam",
//                PrizePool="20,000,000 VNĐ", Prize1st="10,000,000 VNĐ", Prize2nd="6,000,000 VNĐ", Prize3rd="4,000,000 VNĐ",
//                Description="Free Fire Squad Championship 2026 - Đấu trường cho các đội Free Fire chuyên nghiệp và bán chuyên.",
//                Rules="- Squad 4 người\n- Map: Bermuda, Purgatory, Kalahari (random)\n- Anti-cheat: game thủ phải stream màn hình\n- Kết quả tính theo điểm hạng"
//            },
//            new Tournament {
//                Id=3, Name="LQ Summer Invitational 2026", Slug="lq-summer-invite-2026",
//                Game=GameType.LienQuan, Format=TournamentFormat.DoubleElimination,
//                MatchFormat=MatchFormat.BO5, MaxTeams=8, Status=TournamentStatus.Upcoming,
//                RegistrationStart=new DateTime(2026,5,1), RegistrationEnd=new DateTime(2026,5,15),
//                CheckInStart=new DateTime(2026,5,16), TournamentStart=new DateTime(2026,5,20),
//                TournamentEnd=new DateTime(2026,6,15),
//                OrganizerId=1, IsFeatured=true, IsPublic=true, ViewCount=5670, Region="Vietnam",
//                PrizePool="50,000,000 VNĐ", Prize1st="25,000,000 VNĐ", Prize2nd="15,000,000 VNĐ", Prize3rd="10,000,000 VNĐ",
//                Description="Giải mời cho 8 đội Liên Quân hàng đầu Việt Nam tranh tài bo5 đỉnh cao."
//            },
//            new Tournament {
//                Id=4, Name="Free Fire Solos Showdown", Slug="ff-solos-2026",
//                Game=GameType.FreeFire, Format=TournamentFormat.Swiss,
//                MatchFormat=MatchFormat.BO1, MaxTeams=32, Status=TournamentStatus.Registration,
//                RegistrationStart=new DateTime(2026,4,5), RegistrationEnd=new DateTime(2026,4,30),
//                CheckInStart=new DateTime(2026,5,1), TournamentStart=new DateTime(2026,5,3),
//                TournamentEnd=new DateTime(2026,5,25),
//                OrganizerId=1, IsFeatured=false, IsPublic=true, ViewCount=3210, Region="Vietnam",
//                PrizePool="10,000,000 VNĐ", Prize1st="5,000,000 VNĐ", Prize2nd="3,000,000 VNĐ", Prize3rd="2,000,000 VNĐ",
//                Description="Solo carry showdown cho Free Fire players. Hệ thống Swiss đảm bảo fairness."
//            },
//            new Tournament {
//                Id=5, Name="LQ Ranked Rumble Season 1", Slug="lq-ranked-rumble-s1",
//                Game=GameType.LienQuan, Format=TournamentFormat.RoundRobin,
//                MatchFormat=MatchFormat.BO3, MaxTeams=8, Status=TournamentStatus.Completed,
//                RegistrationStart=new DateTime(2026,1,10), RegistrationEnd=new DateTime(2026,1,25),
//                CheckInStart=new DateTime(2026,1,26), TournamentStart=new DateTime(2026,1,28),
//                TournamentEnd=new DateTime(2026,3,10),
//                OrganizerId=1, IsFeatured=false, IsPublic=true, ViewCount=9800, Region="Vietnam",
//                PrizePool="15,000,000 VNĐ", Prize1st="8,000,000 VNĐ", Prize2nd="4,500,000 VNĐ", Prize3rd="2,500,000 VNĐ",
//                Description="Season 1 đã kết thúc. Phoenix Esports vô địch!"
//            },
//        };

//        public static List<Match> Matches = new()
//        {
//            new Match { Id=1, TournamentId=1, Round=1, RoundName="Vòng 1/8", Team1Id=1, Team2Id=8, WinnerId=1, Team1Score=2, Team2Score=0, Status=MatchStatus.Completed, ScheduledAt=new DateTime(2026,3,22,19,0,0), RoomId="LQ2026001" },
//            new Match { Id=2, TournamentId=1, Round=1, RoundName="Vòng 1/8", Team1Id=2, Team2Id=5, WinnerId=2, Team1Score=2, Team2Score=1, Status=MatchStatus.Completed, ScheduledAt=new DateTime(2026,3,22,20,0,0), RoomId="LQ2026002" },
//            new Match { Id=3, TournamentId=1, Round=2, RoundName="Tứ Kết", Team1Id=1, Team2Id=2, Team1Score=1, Team2Score=1, Status=MatchStatus.Live, ScheduledAt=DateTime.Now, RoomId="LQ2026003", StreamUrl="https://www.youtube.com/live/demo" },
//            new Match { Id=4, TournamentId=1, Round=2, RoundName="Tứ Kết", Team1Id=5, Team2Id=8, Status=MatchStatus.Scheduled, ScheduledAt=DateTime.Now.AddHours(2), RoomId="LQ2026004" },
//            new Match { Id=5, TournamentId=1, Round=3, RoundName="Bán Kết", Status=MatchStatus.Scheduled, ScheduledAt=DateTime.Now.AddDays(3), Team1Id=1, Team2Id=2 },
//            new Match { Id=6, TournamentId=2, Round=1, RoundName="Bảng A", Team1Id=3, Team2Id=4, Status=MatchStatus.Scheduled, ScheduledAt=DateTime.Now.AddDays(5), RoomId="FF2026001" },
//            new Match { Id=7, TournamentId=5, Round=3, RoundName="Chung Kết", Team1Id=2, Team2Id=5, WinnerId=2, Team1Score=2, Team2Score=0, Status=MatchStatus.Completed, ScheduledAt=new DateTime(2026,3,10,20,0,0) },
//        };

//        public static List<TournamentRegistration> Registrations = new()
//        {
//            new TournamentRegistration { Id=1, TournamentId=1, TeamId=1, Status=RegistrationStatus.Approved },
//            new TournamentRegistration { Id=2, TournamentId=1, TeamId=2, Status=RegistrationStatus.Approved },
//            new TournamentRegistration { Id=3, TournamentId=1, TeamId=5, Status=RegistrationStatus.Approved },
//            new TournamentRegistration { Id=4, TournamentId=1, TeamId=8, Status=RegistrationStatus.Approved },
//            new TournamentRegistration { Id=5, TournamentId=2, TeamId=3, Status=RegistrationStatus.Pending },
//            new TournamentRegistration { Id=6, TournamentId=2, TeamId=4, Status=RegistrationStatus.Pending },
//            new TournamentRegistration { Id=7, TournamentId=2, TeamId=6, Status=RegistrationStatus.Pending },
//            new TournamentRegistration { Id=8, TournamentId=2, TeamId=7, Status=RegistrationStatus.Pending },
//            new TournamentRegistration { Id=9, TournamentId=4, TeamId=3, Status=RegistrationStatus.Pending },
//            new TournamentRegistration { Id=10, TournamentId=4, TeamId=7, Status=RegistrationStatus.Pending },
//        };

//        static MockData()
//        {
//            foreach (var t in Teams) t.Captain = Users.FirstOrDefault(u => u.Id == t.CaptainId);
//            foreach (var t in Tournaments)
//            {
//                t.Organizer = Users.FirstOrDefault(u => u.Id == t.OrganizerId);
//                t.Registrations = Registrations.Where(r => r.TournamentId == t.Id).ToList();
//                t.Matches = Matches.Where(m => m.TournamentId == t.Id).ToList();
//            }
//            foreach (var m in Matches)
//            {
//                m.Team1 = Teams.FirstOrDefault(t => t.Id == m.Team1Id);
//                m.Team2 = Teams.FirstOrDefault(t => t.Id == m.Team2Id);
//                m.Winner = m.WinnerId.HasValue ? Teams.FirstOrDefault(t => t.Id == m.WinnerId) : null;
//                m.Tournament = Tournaments.FirstOrDefault(t => t.Id == m.TournamentId);
//            }
//            foreach (var r in Registrations) r.Team = Teams.FirstOrDefault(t => t.Id == r.TeamId);
//        }
//    }
//}
