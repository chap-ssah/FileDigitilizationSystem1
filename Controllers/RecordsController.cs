using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FileDigitilizationSystem.Data;
using FileDigitilizationSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

[Authorize(Roles = "RecordsTeam")]
public class RecordsController : Controller
{
    private readonly ApplicationDbContext _db;
  
    private readonly IWebHostEnvironment _env;


    public RecordsController(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    // ─── DASHBOARD ───────────────────────────────────────────────────────────────
    public async Task<IActionResult> Dashboard()
    {
        var pendingCount = await _db.FileRequests.CountAsync(r => !r.Handled);
        var digitizedCount = await _db.FileRecords.CountAsync(r => r.IsDigital);
        var notes = await _db.Notifications
                                     .OrderByDescending(n => n.Timestamp)
                                     .Take(5)
                                     .ToListAsync();

        var vm = new RecordsDashboardViewModel
        {
            PendingRequests = pendingCount,
            DigitizedFiles = digitizedCount,
            Notifications = notes
        };
        return View(vm);
    }

    // ─── REQUESTED FILES ─────────────────────────────────────────────────────────
    public async Task<IActionResult> Requests()
    {
        var list = await _db.FileRequests
                           .Where(r => !r.Handled)  // ← only show not‐yet‐handled
                           .OrderByDescending(r => r.CreatedAt)
                           .ToListAsync();
        return View(list);
    }



    // ─── DIGITIZED FILES ─────────────────────────────────────────────────────────
    public async Task<IActionResult> Digitized()
    {
        var list = await _db.FileRecords
                            .Where(r => r.IsDigital)
                            .OrderByDescending(r => r.CreatedAt)
                            .ToListAsync();
        return View(list);
    }


    // ─── SEARCH DIGITIZED ────────────────────────────────────────────────────────
    public async Task<IActionResult> Search(string q, string province, string location, string type)
    {
        var query = _db.FileRecords.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(r => r.Reference.Contains(q)
                                  || r.ApplicantName.Contains(q));
        if (!string.IsNullOrEmpty(province))
            query = query.Where(r => r.Province == province);
        if (!string.IsNullOrEmpty(location))
            query = query.Where(r => r.Location == location);
        if (!string.IsNullOrEmpty(type))
            query = query.Where(r => r.ApplicantType == type);

        ViewBag.ProvinceList = new SelectList(await _db.FileRecords.Select(r => r.Province).Distinct().ToListAsync());
        ViewBag.LocationList = new SelectList(await _db.FileRecords.Select(r => r.Location).Distinct().ToListAsync());
        ViewBag.ApplicantTypeList = new SelectList(await _db.FileRecords.Select(r => r.ApplicantType).Distinct().ToListAsync());

        var results = await query.ToListAsync();
        return View(results);
    }
    private static readonly List<string> Provinces = new()
{
    "Bulawayo", "Harare", "Manicaland", "Mashonaland Central", "Mashonaland East",
    "Mashonaland West", "Masvingo", "Matabeleland North", "Matabeleland South", "Midlands"
};

    private static readonly Dictionary<string, List<string>> LocationsByProvince = new()
    {
        ["Manicaland"] = new List<string>
    {
        "Birchenough Bridge", "Buhera", "Cashel", "Chimanimani", "Chipinge", "Chisumbanje", "Craigmore",
        "Dorowa", "Gumira", "Hauna", "Headlands", "Juliasdale", "Junction Gate", "Massi Kessi", "Mount Selinda",
        "Murambinda", "Mutambara", "Mutare", "Nyanga", "Nyanyadzi", "Nyazura", "Odzi", "Penhalonga", "Rusape",
        "Sakubva", "Sanyatwe", "Stapleford", "Tandaai", "Tizvione", "Troutbeck", "Tsanzaguru", "Tsunga",
        "Watsomba", "Zimunya"
    },
        ["Mashonaland Central"] = new List<string>
    {
        "Bindura", "Centenary", "Concession", "Domboshava", "Guruve", "Jumbo", "Kanyemba", "Madziwa Mine",
        "Manhenga", "Mazowe", "Melfort", "Mount Darwin", "Mukumbura", "Mushumbi Pools", "Musweswenedi",
        "Muzarabani District", "Mvurwi", "Shamva", "Tengenenge"
    },
        ["Mashonaland East"] = new List<string>
    {
        "Arcturus", "Beatrice", "Bromley", "Chivhu", "Goromonzi", "Kotwa", "Macheke", "Marondera",
        "Mount Hampden", "Murewa", "Mutoko", "Nyabira", "Nyamapanda", "Rocky Spruit", "Ruwa", "Shinga", "Wilton"
    },
        ["Mashonaland West"] = new List<string>
    {
        "Alaska", "Banket", "Battlefields", "Caesar", "Chakari", "Charara", "Chegutu", "Chirundu", "Chinhoyi",
        "Darwendale", "Eiffel Flats", "Eldorado", "Feock", "Gadzema", "Golden Valley", "Kadoma", "Kariba", "Karoi",
        "Kildonan", "Kutama", "Lion's Den", "Madadzi", "Magunje", "Makuti", "Makwiro", "Mhangura", "Mubayira",
        "Muriel", "Murombedzi", "Mutorashanga", "Muzvezve"
    }
        // You can continue adding for other provinces if needed
    };

    // ─── NEW FILE ENTRY ──────────────────────────────────────────────────────────
    public async Task<IActionResult> Create(int? requestId)
    {
        var model = new FileRecord();
        if (requestId.HasValue)
        {
            var req = await _db.FileRequests.FindAsync(requestId.Value);
            if (req != null)
            {
                model.Reference = req.Reference;
                model.ApplicantName = req.ApplicantName;
                model.ApplicantType = req.ApplicantType;
                model.Province = req.Province;
                model.Location = req.Suburb;
                ViewBag.RequestId = requestId;
            }
        }
        PopulateLists(model);
        return View(model);
    }




    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        FileRecord model,
        int? requestId,
        IFormFile scannedFile           // single file upload
    )
    {
        // mark digitalized if coming via a request
        model.IsDigital = requestId.HasValue;
        model.Status = requestId.HasValue ? "Digitized" : "Pending";

        if (!ModelState.IsValid)
        {
            ViewBag.RequestId = requestId;
            PopulateLists(model);
            return View(model);
        }

        // 1) handle file save
        if (scannedFile != null && scannedFile.Length > 0)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueName = Guid.NewGuid().ToString("N")
                               + Path.GetExtension(scannedFile.FileName);
            var fullPath = Path.Combine(uploadsFolder, uniqueName);

            using var fs = new FileStream(fullPath, FileMode.Create);
            await scannedFile.CopyToAsync(fs);

            model.FilePath = Path.Combine("uploads", uniqueName).Replace("\\", "/");
        }

        // 2) stamp metadata
        model.CreatedAt = DateTime.UtcNow;
        model.CreatedById = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // 3) add record
        _db.FileRecords.Add(model);

        // 4) if digitizing, mark the request handled
        if (requestId.HasValue)
        {
            var req = await _db.FileRequests.FindAsync(requestId.Value);
            if (req != null)
            {
                req.Handled = true;
                _db.FileRequests.Update(req);
            }
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Digitized));
    }


    [HttpGet]
    public IActionResult GetLocationsByProvince(string province)
    {
        if (string.IsNullOrEmpty(province))
        {
            return Json(new List<string>());
        }

        if (LocationsByProvince.TryGetValue(province, out var locations))
        {
            return Json(locations);
        }

        return Json(new List<string>());
    }

    private void PopulateLists(FileRecord model)
    {
        ViewBag.ProvinceList = new SelectList(Provinces, model.Province);
        ViewBag.LocationList = new SelectList(
                                        model.Province != null && LocationsByProvince.ContainsKey(model.Province)
                                          ? LocationsByProvince[model.Province]
                                          : new List<string>(),
                                        model.Location
                                    );
        ViewBag.LandUseTypeList = new SelectList(new[] { "Residential", "Rental", "Commercial" }, model.LandUseType);
        ViewBag.SpecialStatusList = new SelectList(new[] { "Senior Citizen", "Disabled" }, model.SpecialStatus);
    }



    // ─── NON-DIGITIZED FILES ────────────────────────────────────────────────────────
    public async Task<IActionResult> NonDigitized()
    {
        var list = await _db.FileRecords
                            .Where(r => !r.IsDigital)
                            .OrderByDescending(r => r.CreatedAt)
                            .ToListAsync();
        return View(list);
    }

}
