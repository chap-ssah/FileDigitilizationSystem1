using FileDigitilizationSystem.Data;
using FileDigitilizationSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

[Authorize(Roles = "RecordsTeam")]
public class RecordsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userMgr;
    private readonly IWebHostEnvironment _env;

    public RecordsController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> um,
        IWebHostEnvironment env)
    {
        _db = db;
        _userMgr = um;
        _env = env;
    }

    // GET: /Records/Dashboard
    public async Task<IActionResult> Dashboard()
    {
        var userId = _userMgr.GetUserId(User);

        var vm = new RecordsDashboardViewModel
        {
            UserName = User.Identity.Name,

            FileRequests = await _db.FileRequests
                                     .Where(fr => !fr.Handled)
                                     .OrderByDescending(fr => fr.CreatedAt)
                                     .ToListAsync(),
            DigitizedRecords = await _db.FileRecords
                                     .Where(r => r.IsDigital)
                                     .OrderByDescending(r => r.CreatedAt)
                                     .ToListAsync(),
            Notifications = await _db.Notifications
                                     .Where(n => n.UserId == userId)
                                     .OrderByDescending(n => n.Timestamp)
                                     .ToListAsync()
        };

        return View(vm);
    }
    /*
    //populate dropdown lists
   private void PopulateLookups(FileRecord rec = null)
    {
        var provinces = _db.FileRecords.Select(f => f.Province).Distinct().ToList();
        var locations = _db.FileRecords.Select(f => f.Location).Distinct().ToList();
        var applicantTypes = _db.FileRecords.Select(f => f.ApplicantType).Distinct().ToList();
        var landUseTypes = _db.FileRecords.Select(f => f.LandUseType).Distinct().ToList();
        var specialStatus = _db.FileRecords.Select(f => f.SpecialStatus).Distinct().ToList();

        ViewBag.ProvinceList = new SelectList(provinces, rec?.Province);
        ViewBag.LocationList = new SelectList(locations, rec?.Location);
        ViewBag.ApplicantTypeList = new SelectList(applicantTypes, rec?.ApplicantType);
        ViewBag.LandUseTypeList = new SelectList(landUseTypes, rec?.LandUseType);
        ViewBag.SpecialStatusList = new SelectList(specialStatus, rec?.SpecialStatus);
    }*/

    private void PopulateLookups(FileRecord rec = null)
    {
        var provinces = new List<string> { "Harare", "Bulawayo", "Manicaland" };
        var locations = new List<string> { "Glen View", "Budiriro", "Borrowdale" };
        var applicantTypes = new List<string> { "Individual", "Developer" };
        var landUseTypes = new List<string> { "Residential", "Commercial", "Agricultural" };
        var specialStatus = new List<string> { "None", "Disabled", "Senior Citizen", "Government Worker", "Other" };

        ViewBag.ProvinceList = new SelectList(provinces, rec?.Province);
        ViewBag.LocationList = new SelectList(locations, rec?.Location);
        ViewBag.ApplicantTypeList = new SelectList(applicantTypes, rec?.ApplicantType);
        ViewBag.LandUseTypeList = new SelectList(landUseTypes, rec?.LandUseType);
        ViewBag.SpecialStatusList = new SelectList(specialStatus, rec?.SpecialStatus);
    }


    // GET: /Records/Create
    // GET: /Records/Create?requestId=42
    public async Task<IActionResult> Create(int? requestId)
    {
        FileRecord model;
        if (requestId.HasValue)
        {
            // pull the request so we can prefill
            var req = await _db.FileRequests.FindAsync(requestId.Value);
            if (req == null) return NotFound();

            model = new FileRecord
            {
                Reference = req.Reference,
                ApplicantName = req.ApplicantName,
                ApplicantType = req.ApplicantType,
                Province = req.Province,
                Location = req.Suburb // or .Location if you renamed it
            };

            ViewBag.RequestId = requestId;
        }
        else
        {
            model = new FileRecord();
        }

        PopulateLookups(model);
        return View(model);
    }

    // POST: /Records/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
    FileRecord model,
    List<IFormFile> scannedFiles,
    int? requestId)
    {
        // EF will set these for us later
        ModelState.Remove(nameof(model.FilePath));
        ModelState.Remove(nameof(model.CreatedById));
        ModelState.Remove(nameof(model.CreatedBy));

        PopulateLookups(model);
        if (!ModelState.IsValid)
            return View(model);

        // 1️⃣ Stamp who created it
        model.CreatedById = _userMgr.GetUserId(User);

        // — FAST FIX for your NOT NULL FilePath column:
        model.FilePath = String.Empty;

        // 2️⃣ Insert the row to get model.Id
        try
        {
            _db.FileRecords.Add(model);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            ModelState.AddModelError("", ex.GetBaseException().Message);
            return View(model);
        }

        // 3️⃣ If there are scans, save them and flip to digital
        if (scannedFiles?.Any() == true)
        {
            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", model.Id.ToString());
            Directory.CreateDirectory(uploadDir);
            foreach (var file in scannedFiles)
            {
                var fn = Path.GetFileName(file.FileName);
                var path = Path.Combine(uploadDir, fn);
                using var fs = new FileStream(path, FileMode.Create);
                await file.CopyToAsync(fs);
            }

            model.FilePath = Path.Combine("uploads", model.Id.ToString(), scannedFiles.First().FileName);
            model.IsDigital = true;
            model.Status = "Completed";
        }
        else
        {
            model.IsDigital = false;
            model.Status = "Pending";
        }

        // 4️⃣ If this was a FileRequest, mark it handled
        if (requestId.HasValue)
        {
            var req = await _db.FileRequests.FindAsync(requestId.Value);
            if (req != null) req.Handled = true;
        }

        // 5️⃣ Persist the updated flags & FilePath
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            ModelState.AddModelError("", ex.GetBaseException().Message);
            return View(model);
        }

        return RedirectToAction(nameof(Dashboard));
    }



    /* // POST: /Records/Digitize/{id}  (optional quick action)
     [HttpPost, ValidateAntiForgeryToken]
     public async Task<IActionResult> Digitize(int id)
     {
         var rec = await _db.FileRecords.FindAsync(id);
         if (rec == null) return NotFound();
         rec.IsDigital = true;
         rec.Status = "Completed";
         await _db.SaveChangesAsync();
         return RedirectToAction(nameof(Dashboard));
     }


     /*public async Task<IActionResult> Digitizee(int id)
     {
         var req = await _db.FileRequests.FindAsync(id);
         if (req == null) return NotFound();

         // build a new FileRecord pre‐populated with the request’s metadata
         var model = new FileRecord
         {
             Reference = req.Reference,
             ApplicantName = req.ApplicantName,
             ApplicantType = req.ApplicantType,
             Province = req.Province,
             Location = req.Suburb,       // or .Location if you renamed it
             // you can set other fields to blank or copy from request as needed
         };

         ViewBag.RequestId = id;
         PopulateLookups(model);
         return View("Create", model); // reuse the Create view
     }

     // POST: /Records/Digitize/5
     [HttpPost, ValidateAntiForgeryToken]
     public async Task<IActionResult> Digitizee(int requestId, FileRecord model, List<IFormFile> scannedFiles)
     {
         // get back the request so we can mark it handled
         var req = await _db.FileRequests.FindAsync(requestId);
         if (req == null) return NotFound();

         PopulateLookups(model);
         if (!ModelState.IsValid)
             return View("Create", model);

         // stamp who & when & digital status
         model.CreatedById = _userMgr.GetUserId(User);
         model.IsDigital = true;
         model.Status = "Completed";

         // persist the new FileRecord
         _db.FileRecords.Add(model);
         await _db.SaveChangesAsync();

         // save scans to disk / set FilePath if you want…
         if (scannedFiles?.Any() == true)
         {
             var uploadDir = Path.Combine(_env.WebRootPath, "uploads", model.Id.ToString());
             Directory.CreateDirectory(uploadDir);
             foreach (var file in scannedFiles)
             {
                 var fn = Path.GetFileName(file.FileName);
                 using var fs = System.IO.File.Create(Path.Combine(uploadDir, fn));
                 await file.CopyToAsync(fs);
             }
         }

         // now mark the request handled
         req.Handled = true;
         await _db.SaveChangesAsync();

         return RedirectToAction(nameof(Dashboard));
     }*/

    // GET: /Records/Search
    [HttpGet]
    public async Task<IActionResult> Search(
    string q,
    string province,
    string location,
    string type)
    {
        // 1) Prefill dropdown lists *with* the selected value
        var lookupRec = new FileRecord
        {
            Province = province,
            Location = location,
            ApplicantType = type
        };
        PopulateLookups(lookupRec);

        var userId = _userMgr.GetUserId(User);

        // 2) Always show pending requests unfiltered
        var pending = await _db.FileRequests
                               .Where(fr => !fr.Handled)
                               .OrderByDescending(fr => fr.CreatedAt)
                               .ToListAsync();

        // 3) Build up your filtered digitized records query
        IQueryable<FileRecord> digitized = _db.FileRecords.Where(r => r.IsDigital);

        if (!string.IsNullOrWhiteSpace(q))
            digitized = digitized.Where(r =>
                r.Reference.Contains(q) ||
                r.ApplicantName.Contains(q) ||
                r.ApplicantId.Contains(q)
            );

        if (!string.IsNullOrWhiteSpace(province))
            digitized = digitized.Where(r => r.Province == province);

        if (!string.IsNullOrWhiteSpace(location))
            digitized = digitized.Where(r => r.Location == location);

        if (!string.IsNullOrWhiteSpace(type))
            digitized = digitized.Where(r => r.ApplicantType == type);

        var completed = await digitized
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        // 4) Pack into VM
        var vm = new RecordsDashboardViewModel
        {
            UserName = User.Identity.Name,
            FileRequests = pending,
            DigitizedRecords = completed,
            Notifications = await _db.Notifications
                                       .Where(n => n.UserId == userId)
                                       .OrderByDescending(n => n.Timestamp)
                                       .ToListAsync(),

            // search metadata
            SearchPerformed = true,
            SearchQuery = q,
            FilterProvince = province,
            FilterLocation = location,
            FilterType = type
        };

        return View("Dashboard", vm);
    }


    }
