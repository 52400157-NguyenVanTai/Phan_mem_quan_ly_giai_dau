using Microsoft.AspNetCore.Mvc;
using EsportManager.Services;
using EsportManager.Models;

namespace EsportManager.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.FeaturedTournaments = MockData.Tournaments.Where(t => t.IsFeatured).Take(4).ToList();
            ViewBag.LiveMatches = MockData.Matches.Where(m => m.Status == MatchStatus.Live).ToList();
            ViewBag.UpcomingMatches = MockData.Matches.Where(m => m.Status == MatchStatus.Scheduled).OrderBy(m => m.ScheduledAt).Take(5).ToList();
            ViewBag.TopTeams_LQ = MockData.Teams.Where(t => t.PrimaryGame == GameType.LienQuan).OrderByDescending(t => t.Points).Take(5).ToList();
            ViewBag.TopTeams_FF = MockData.Teams.Where(t => t.PrimaryGame == GameType.FreeFire).OrderByDescending(t => t.Points).Take(5).ToList();
            ViewBag.RecentResults = MockData.Matches.Where(m => m.Status == MatchStatus.Completed).OrderByDescending(m => m.ScheduledAt).Take(4).ToList();
            return View();
        }
        public IActionResult Error() => View();
    }

    public class TournamentController : Controller
    {
        public IActionResult Index(string? game, string? status, string? search)
        {
            var q = MockData.Tournaments.AsQueryable();
            if (!string.IsNullOrEmpty(game) && Enum.TryParse<GameType>(game, out var g)) q = q.Where(t => t.Game == g);
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<TournamentStatus>(status, out var s)) q = q.Where(t => t.Status == s);
            if (!string.IsNullOrEmpty(search)) q = q.Where(t => t.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
            ViewBag.Game = game; ViewBag.Status = status; ViewBag.Search = search;
            return View(q.OrderByDescending(t => t.IsFeatured).ThenByDescending(t => t.ViewCount).ToList());
        }

        public IActionResult Detail(int id)
        {
            var t = MockData.Tournaments.FirstOrDefault(x => x.Id == id);
            if (t == null) return NotFound();
            t.ViewCount++;
            ViewBag.ApprovedTeams = MockData.Registrations.Where(r => r.TournamentId == id && r.Status == RegistrationStatus.Approved).Select(r => r.Team).ToList();
            ViewBag.Matches = MockData.Matches.Where(m => m.TournamentId == id).OrderBy(m => m.Round).ToList();
            ViewBag.UserRegistration = MockData.Registrations.FirstOrDefault(r => r.TournamentId == id && r.TeamId == 1);
            return View(t);
        }

        public IActionResult Bracket(int id)
        {
            var t = MockData.Tournaments.FirstOrDefault(x => x.Id == id);
            if (t == null) return NotFound();
            ViewBag.Matches = MockData.Matches.Where(m => m.TournamentId == id).OrderBy(m => m.Round).ToList();
            ViewBag.MaxRound = MockData.Matches.Where(m => m.TournamentId == id).Any() ? MockData.Matches.Where(m => m.TournamentId == id).Max(m => m.Round) : 1;
            ViewBag.Teams = MockData.Registrations.Where(r => r.TournamentId == id && r.Status == RegistrationStatus.Approved).Select(r => r.Team).ToList();
            return View(t);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public IActionResult Create(Tournament model)
        {
            model.Id = MockData.Tournaments.Count + 1;
            model.OrganizerId = 1;
            model.Status = TournamentStatus.Upcoming;
            model.Slug = model.Name.ToLower().Replace(" ", "-");
            MockData.Tournaments.Add(model);
            TempData["Success"] = "Tạo giải đấu thành công!";
            return RedirectToAction("Detail", new { id = model.Id });
        }

        [HttpPost]
        public IActionResult Register(int tournamentId, int teamId)
        {
            if (!MockData.Registrations.Any(r => r.TournamentId == tournamentId && r.TeamId == teamId))
            {
                MockData.Registrations.Add(new TournamentRegistration
                {
                    Id = MockData.Registrations.Count + 1,
                    TournamentId = tournamentId,
                    TeamId = teamId,
                    Team = MockData.Teams.FirstOrDefault(t => t.Id == teamId),
                    Status = RegistrationStatus.Pending,
                    RegisteredAt = DateTime.Now
                });
            }
            TempData["Success"] = "Đã gửi đơn đăng ký! Chờ admin duyệt.";
            return RedirectToAction("Detail", new { id = tournamentId });
        }
    }

    public class TeamController : Controller
    {
        public IActionResult Index(string? game)
        {
            var teams = MockData.Teams.AsQueryable();
            if (!string.IsNullOrEmpty(game) && Enum.TryParse<GameType>(game, out var g)) teams = teams.Where(t => t.PrimaryGame == g);
            ViewBag.Game = game;
            return View(teams.OrderByDescending(t => t.Points).ToList());
        }

        public IActionResult Detail(int id)
        {
            var team = MockData.Teams.FirstOrDefault(t => t.Id == id);
            if (team == null) return NotFound();
            ViewBag.MatchHistory = MockData.Matches.Where(m => (m.Team1Id == id || m.Team2Id == id) && m.Status == MatchStatus.Completed).OrderByDescending(m => m.ScheduledAt).Take(8).ToList();
            ViewBag.Registrations = MockData.Registrations.Where(r => r.TeamId == id).ToList();
            return View(team);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public IActionResult Create(Team model)
        {
            model.Id = MockData.Teams.Count + 1;
            model.CaptainId = HttpContext.Session.GetInt32("UserId") ?? 1;
            model.Captain = MockData.Users.FirstOrDefault(u => u.Id == model.CaptainId);
            model.CreatedAt = DateTime.Now;
            MockData.Teams.Add(model);
            TempData["Success"] = "Tạo đội thành công!";
            return RedirectToAction("Detail", new { id = model.Id });
        }
    }

    public class MatchController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.LiveMatches = MockData.Matches.Where(m => m.Status == MatchStatus.Live).ToList();
            ViewBag.Scheduled = MockData.Matches.Where(m => m.Status == MatchStatus.Scheduled).OrderBy(m => m.ScheduledAt).ToList();
            ViewBag.Completed = MockData.Matches.Where(m => m.Status == MatchStatus.Completed).OrderByDescending(m => m.ScheduledAt).ToList();
            return View();
        }

        public IActionResult Detail(int id)
        {
            var m = MockData.Matches.FirstOrDefault(x => x.Id == id);
            if (m == null) return NotFound();
            return View(m);
        }

        [HttpPost]
        public IActionResult SubmitResult(int matchId, int t1s, int t2s)
        {
            var m = MockData.Matches.FirstOrDefault(x => x.Id == matchId);
            if (m != null)
            {
                m.Team1Score = t1s; m.Team2Score = t2s;
                m.WinnerId = t1s > t2s ? m.Team1Id : m.Team2Id;
                m.Winner = t1s > t2s ? m.Team1 : m.Team2;
                m.Status = MatchStatus.Completed;
                m.Team1Confirmed = true;
                if (m.Winner != null && t1s > t2s) { m.Team1?.GetType(); if (m.Team1 != null) m.Team1.Wins++; if (m.Team2 != null) m.Team2.Losses++; }
                else { if (m.Team2 != null) m.Team2.Wins++; if (m.Team1 != null) m.Team1.Losses++; }
            }
            TempData["Success"] = "Kết quả đã gửi. Chờ đội đối phương xác nhận.";
            return RedirectToAction("Detail", new { id = matchId });
        }
    }

    public class AccountController : Controller
    {
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = MockData.Users.FirstOrDefault(u => u.Email == email);
            if (user != null && !user.IsBanned)
            {
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("UserName", user.Username);
                HttpContext.Session.SetString("IngameName", user.IngameName);
                HttpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString());
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = user?.IsBanned == true ? "Tài khoản đã bị khóa." : "Email hoặc mật khẩu không đúng.";
            return View();
        }

        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(User model)
        {
            model.Id = MockData.Users.Count + 1;
            model.CreatedAt = DateTime.Now;
            MockData.Users.Add(model);
            HttpContext.Session.SetInt32("UserId", model.Id);
            HttpContext.Session.SetString("UserName", model.Username);
            HttpContext.Session.SetString("IngameName", model.IngameName);
            HttpContext.Session.SetString("IsAdmin", "False");
            TempData["Success"] = "Tạo tài khoản thành công! Chào mừng bạn!";
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout() { HttpContext.Session.Clear(); return RedirectToAction("Index", "Home"); }

        public IActionResult Profile(int? id)
        {
            int uid = id ?? HttpContext.Session.GetInt32("UserId") ?? 1;
            var user = MockData.Users.FirstOrDefault(u => u.Id == uid);
            if (user == null) return NotFound();
            ViewBag.Teams = MockData.Teams.Where(t => t.CaptainId == uid).ToList();
            ViewBag.MatchHistory = MockData.Matches.Where(m => m.Status == MatchStatus.Completed).Take(6).ToList();
            return View(user);
        }
    }

    public class RankingController : Controller
    {
        public IActionResult Index(string? game)
        {
            ViewBag.LQ = MockData.Teams.Where(t => t.PrimaryGame == GameType.LienQuan).OrderByDescending(t => t.Points).ToList();
            ViewBag.FF = MockData.Teams.Where(t => t.PrimaryGame == GameType.FreeFire).OrderByDescending(t => t.Points).ToList();
            ViewBag.ActiveTab = game ?? "LienQuan";
            return View();
        }
    }

    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.TotalUsers = MockData.Users.Count;
            ViewBag.TotalTeams = MockData.Teams.Count;
            ViewBag.TotalTournaments = MockData.Tournaments.Count;
            ViewBag.LiveMatches = MockData.Matches.Count(m => m.Status == MatchStatus.Live);
            ViewBag.PendingReg = MockData.Registrations.Count(r => r.Status == RegistrationStatus.Pending);
            ViewBag.OngoingTournaments = MockData.Tournaments.Count(t => t.Status == TournamentStatus.Ongoing);
            ViewBag.RecentMatches = MockData.Matches.OrderByDescending(m => m.ScheduledAt).Take(5).ToList();
            return View();
        }
        public IActionResult Users() => View(MockData.Users);
        public IActionResult Tournaments() => View(MockData.Tournaments);
        public IActionResult Registrations() => View(MockData.Registrations.Where(r => r.Status == RegistrationStatus.Pending).ToList());

        [HttpPost] public IActionResult Approve(int id) { var r = MockData.Registrations.FirstOrDefault(x => x.Id == id); if (r != null) r.Status = RegistrationStatus.Approved; TempData["Success"] = "Đã duyệt đăng ký."; return RedirectToAction("Registrations"); }
        [HttpPost] public IActionResult Reject(int id) { var r = MockData.Registrations.FirstOrDefault(x => x.Id == id); if (r != null) r.Status = RegistrationStatus.Rejected; TempData["Success"] = "Đã từ chối đăng ký."; return RedirectToAction("Registrations"); }
        [HttpPost] public IActionResult BanUser(int id) { var u = MockData.Users.FirstOrDefault(x => x.Id == id); if (u != null) u.IsBanned = !u.IsBanned; TempData["Success"] = $"User #{id} đã bị {(MockData.Users.FirstOrDefault(x => x.Id == id)?.IsBanned == true ? "ban" : "unban")}."; return RedirectToAction("Users"); }
        [HttpPost] public IActionResult SetMatchLive(int id) { var m = MockData.Matches.FirstOrDefault(x => x.Id == id); if (m != null) m.Status = MatchStatus.Live; return RedirectToAction("Index"); }
    }
}
