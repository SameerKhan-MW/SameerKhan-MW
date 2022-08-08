using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolProgramme.Models;

namespace SchoolProgramme.Controllers
{
    public class WebUserRegistrationsController : Controller
    {
        private readonly SPDBContext _context;

        public WebUserRegistrationsController(SPDBContext context)
        {
            _context = context;
        }

        // GET: WebUserRegistrations
        public async Task<IActionResult> Index()
        {
            var sPDBContext = _context.tblwebUserRegistrations.Include(t => t.Block).Include(t => t.Class).Include(t => t.Designation).Include(t => t.District).Include(t => t.Gender).Include(t => t.State).Include(t => t.UDISE);
            return View(await sPDBContext.ToListAsync());
        }

        // GET: WebUserRegistrations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblwebUserRegistration = await _context.tblwebUserRegistrations
                .Include(t => t.Block)
                .Include(t => t.Class)
                .Include(t => t.Designation)
                .Include(t => t.District)
                .Include(t => t.Gender)
                .Include(t => t.State)
                .Include(t => t.UDISE)
                .FirstOrDefaultAsync(m => m.UserRegistrationID == id);
            if (tblwebUserRegistration == null)
            {
                return NotFound();
            }

            return View(tblwebUserRegistration);
        }

        // GET: WebUserRegistrations/Create
        public IActionResult Create()
        {
           // ViewData["BlockId"] = new SelectList(_context.LocationBlocks, "BlockID", "BlockName");
            ViewData["ClassId"] = new SelectList(_context.MstClasses.Where(m => m.IsDeleted == 0), "ClassID", "Class");
       
            //ViewData["DesignationId"] = new SelectList(_context.MstRoles.Where(m=>m.RoleFor== "District"), "RoleID", "Role");
            //ViewData["DistrictId"] = new SelectList(_context.LocationDistricts, "DistrictID", "District");
            ViewData["GenderId"] = new SelectList(_context.MstGenders.Where(m => m.IsDeleted == 0), "GenderID", "Gender");
            ViewData["SessionYear"] = new SelectList(_context.mst_SessionYears.Where(m=>m.IsDeleted==0), "Value", "SessionYear");   
            ViewData["StateId"] = new SelectList(_context.LocationStates.Where(m => m.StateID == 5 && m.IsDeleted == 0), "StateID", "StateName");
           // ViewData["UDISEID"] = new SelectList(_context.tblUDISE_Codes, "UDISEID", "UDISE");
            return View();
        }

        // POST: WebUserRegistrations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserRegistrationID,UserLevel,StateId,DistrictId,BlockId,UDISEID,DesignationId,Name,MobileNumber,EmailId,GenderId,SessionYear,ClassId,CreatedOn,WebUserId")] tblwebUserRegistration tblwebUserRegistration)
        {
            if (ModelState.IsValid)
            {
                if(tblwebUserRegistration.ClassId==0)
                {
                    tblwebUserRegistration.ClassId = null;
                }
                if(tblwebUserRegistration.SessionYear == 0)
                {
                    tblwebUserRegistration.SessionYear = null;
                }
                if (tblwebUserRegistration.UDISEID == 0)
                {
                    tblwebUserRegistration.UDISEID = null;
                }
                tblwebUserRegistration.WebUserId = 1;
                tblwebUserRegistration.CreatedOn = DateTime.Now;
                _context.Add(tblwebUserRegistration);
                await _context.SaveChangesAsync();

                var WebUserList = _context.tblwebUserRegistrations.Where(m => m.DesignationId == tblwebUserRegistration.DesignationId && m.DistrictId==tblwebUserRegistration.DistrictId).OrderBy(l => l.Name).Include(t => t.Block).Include(t => t.Class).Include(t => t.Designation).Include(t => t.District).Include(t => t.Gender).Include(t => t.State).Include(t => t.UDISE);

                if (tblwebUserRegistration.UserLevel == "District")
                {
                    return View(await WebUserList.ToListAsync());
                   // return PartialView("/Views/WebUserRegistrations/_PVGetDistrictUserList.cshtml", WebUserList);
                }
                else if (tblwebUserRegistration.UserLevel == "Block")
                {

                }
                else if (tblwebUserRegistration.UserLevel == "School")
                {

                }
                //return RedirectToAction(nameof(Index));
            }

            ViewData["ClassId"] = new SelectList(_context.MstClasses.Where(m => m.IsDeleted == 0), "ClassID", "Class");
            ViewData["GenderId"] = new SelectList(_context.MstGenders.Where(m => m.IsDeleted == 0), "GenderID", "Gender");
            ViewData["SessionYear"] = new SelectList(_context.mst_SessionYears.Where(m => m.IsDeleted == 0), "Value", "SessionYear");
            ViewData["StateId"] = new SelectList(_context.LocationStates.Where(m => m.StateID == 5 && m.IsDeleted == 0), "StateID", "StateName");

            //ViewData["BlockId"] = new SelectList(_context.LocationBlocks, "BlockID", "BlockName", tblwebUserRegistration.BlockId);
            //ViewData["ClassId"] = new SelectList(_context.MstClasses, "ClassID", "ClassID", tblwebUserRegistration.ClassId);
            //ViewData["DesignationId"] = new SelectList(_context.MstRoles, "RoleID", "RoleID", tblwebUserRegistration.DesignationId);
            //ViewData["DistrictId"] = new SelectList(_context.LocationDistricts, "DistrictID", "District", tblwebUserRegistration.DistrictId);
            //ViewData["GenderId"] = new SelectList(_context.MstGenders, "GenderID", "GenderID", tblwebUserRegistration.GenderId);
            //ViewData["SessionYear"] = new SelectList(_context.mst_SessionYears, "SessionYearID", "SessionYearID", tblwebUserRegistration.SessionYear);
            //ViewData["StateId"] = new SelectList(_context.LocationStates, "StateID", "StateName", tblwebUserRegistration.StateId);
            //ViewData["UDISEID"] = new SelectList(_context.tblUDISE_Codes, "UDISEID", "UDISE", tblwebUserRegistration.UDISEID);
            return View(tblwebUserRegistration);
        }

        // GET: WebUserRegistrations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblwebUserRegistration = await _context.tblwebUserRegistrations.FindAsync(id);
            if (tblwebUserRegistration == null)
            {
                return NotFound();
            }
            ViewData["BlockId"] = new SelectList(_context.LocationBlocks, "BlockID", "BlockName", tblwebUserRegistration.BlockId);
            ViewData["ClassId"] = new SelectList(_context.MstClasses, "ClassID", "ClassID", tblwebUserRegistration.ClassId);
            ViewData["DesignationId"] = new SelectList(_context.MstRoles, "RoleID", "RoleID", tblwebUserRegistration.DesignationId);
            ViewData["DistrictId"] = new SelectList(_context.LocationDistricts, "DistrictID", "District", tblwebUserRegistration.DistrictId);
            ViewData["GenderId"] = new SelectList(_context.MstGenders, "GenderID", "GenderID", tblwebUserRegistration.GenderId);
            ViewData["SessionYear"] = new SelectList(_context.mst_SessionYears, "SessionYearID", "SessionYearID", tblwebUserRegistration.SessionYear);
            ViewData["StateId"] = new SelectList(_context.LocationStates, "StateID", "StateName", tblwebUserRegistration.StateId);
            ViewData["UDISEID"] = new SelectList(_context.tblUDISE_Codes, "UDISEID", "UDISE", tblwebUserRegistration.UDISEID);
            return View(tblwebUserRegistration);
        }

        // POST: WebUserRegistrations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserRegistrationID,UserLevel,StateId,DistrictId,BlockId,UDISEID,DesignationId,Name,MobileNumber,EmailId,GenderId,SessionYear,ClassId")] tblwebUserRegistration tblwebUserRegistration)
        {
            if (id != tblwebUserRegistration.UserRegistrationID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tblwebUserRegistration);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!tblwebUserRegistrationExists(tblwebUserRegistration.UserRegistrationID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BlockId"] = new SelectList(_context.LocationBlocks, "BlockID", "BlockName", tblwebUserRegistration.BlockId);
            ViewData["ClassId"] = new SelectList(_context.MstClasses, "ClassID", "ClassID", tblwebUserRegistration.ClassId);
            ViewData["DesignationId"] = new SelectList(_context.MstRoles, "RoleID", "RoleID", tblwebUserRegistration.DesignationId);
            ViewData["DistrictId"] = new SelectList(_context.LocationDistricts, "DistrictID", "District", tblwebUserRegistration.DistrictId);
            ViewData["GenderId"] = new SelectList(_context.MstGenders, "GenderID", "GenderID", tblwebUserRegistration.GenderId);
            ViewData["SessionYear"] = new SelectList(_context.mst_SessionYears, "SessionYearID", "SessionYearID", tblwebUserRegistration.SessionYear);
            ViewData["StateId"] = new SelectList(_context.LocationStates, "StateID", "StateName", tblwebUserRegistration.StateId);
            ViewData["UDISEID"] = new SelectList(_context.tblUDISE_Codes, "UDISEID", "UDISE", tblwebUserRegistration.UDISEID);
            return View(tblwebUserRegistration);
        }

        // GET: WebUserRegistrations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tblwebUserRegistration = await _context.tblwebUserRegistrations
                .Include(t => t.Block)
                .Include(t => t.Class)
                .Include(t => t.Designation)
                .Include(t => t.District)
                .Include(t => t.Gender)
                .Include(t => t.State)
                .Include(t => t.UDISE)
                .FirstOrDefaultAsync(m => m.UserRegistrationID == id);
            if (tblwebUserRegistration == null)
            {
                return NotFound();
            }

            return View(tblwebUserRegistration);
        }

        // POST: WebUserRegistrations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tblwebUserRegistration = await _context.tblwebUserRegistrations.FindAsync(id);
            _context.tblwebUserRegistrations.Remove(tblwebUserRegistration);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool tblwebUserRegistrationExists(int id)
        {
            return _context.tblwebUserRegistrations.Any(e => e.UserRegistrationID == id);
        }

        public JsonResult GetDistrict(int stateid)
        {
            try
            {
                return Json(_context.LocationDistricts.Where(m => m.StateID == stateid).OrderBy(p => p.District).ToList());
            }
            catch (Exception ex)
            {
                return Json("0");
            }
        }

        //GETBLOCK
        public JsonResult GetBlock(int districtid)
        {
            try
            {
                return Json(_context.LocationBlocks.Where(m => m.DistrictID == districtid).OrderBy(l => l.BlockName).ToList());
            }
            catch (Exception ex)
            {
                return Json("0");
            }
        }
        //GETUDISE_Codes
        public JsonResult UDISECodes(int blockID)
        {
            try
            {
                return Json(_context.tblUDISE_Codes.Where(m => m.BlockID == blockID).OrderBy(l => l.UDISEID).ToList());
            }
            catch (Exception ex)
            {
                return Json("0");
            }
        }

        public JsonResult GetDesgnation(string userlevelid)
        {
            try
            {
                return Json(_context.MstRoles.Where(m => m.RoleFor == userlevelid).OrderBy(l => l.Role).ToList());
            }
            catch (Exception ex)
            {
                return Json("0");
            }
        }    
        public JsonResult UDISEGridShow(int UDISEID)
        {
            try
            {
                return Json(_context.tblUDISE_Codes.Where(m => m.UDISEID == UDISEID && m.IsDeleted==0).OrderBy(l => l.SchoolName).ToList());
            }
            catch (Exception ex)
            {
                return Json("0");
            }
        }
        public IActionResult SaveWebUser(string Userlevel, int StateId, int DistrictId, int BlockId, int UDISEID, int DesginationID, string Name, int GenderId, string MobileNumber, string EmailId, int SessionYear, int ClassId)
        {
            try
            {
                tblwebUserRegistration webUserRegistration = new tblwebUserRegistration();

                webUserRegistration.UserLevel = Userlevel;
                webUserRegistration.StateId = StateId;
                webUserRegistration.DistrictId = DistrictId;
                if (BlockId == 0)
                {
                    webUserRegistration.BlockId = null;
                }
                else
                {
                    webUserRegistration.BlockId = BlockId;
                }
                if (UDISEID == 0)
                {
                    webUserRegistration.UDISEID = null;
                }
                else
                {
                    webUserRegistration.UDISEID = UDISEID;
                }               
                if (DesginationID == 0)
                {
                    webUserRegistration.DesignationId = null;
                }
                else
                {
                    webUserRegistration.DesignationId = DesginationID;
                }
                if (SessionYear == 0)
                {
                    webUserRegistration.SessionYear = null;
                }
                else
                {
                    webUserRegistration.SessionYear = SessionYear;
                }
                if (ClassId == 0)
                {
                    webUserRegistration.ClassId = null;
                }
                else
                {
                    webUserRegistration.ClassId = ClassId;
                }
                if (GenderId == 0)
                {
                    webUserRegistration.GenderId = null;
                }
                else
                {
                    webUserRegistration.GenderId = GenderId;
                }

                webUserRegistration.Name = Name;        
                webUserRegistration.MobileNumber = MobileNumber;
                webUserRegistration.EmailId = EmailId;            
                webUserRegistration.WebUserId = 1;
                webUserRegistration.CreatedOn = DateTime.Now;

                _context.Add(webUserRegistration);
                _context.SaveChanges();

                WebUserRegistrationCollection WURC = new WebUserRegistrationCollection();

                if (webUserRegistration.UserLevel == "District")
                {
                    var WebUserList = _context.tblwebUserRegistrations.Where(m => m.DesignationId == webUserRegistration.DesignationId && m.DistrictId == webUserRegistration.DistrictId).OrderBy(l => l.Name).Include(t => t.Block).Include(t => t.Class).Include(t => t.Designation).Include(t => t.District).Include(t => t.Gender).Include(t => t.State).Include(t => t.UDISE).ToList();

                    return PartialView("/Views/WebUserRegistrations/_PVGetDistrictUserList.cshtml", WebUserList);
                }
                else if (webUserRegistration.UserLevel == "Block")
                {
                    WURC.WURList = _context.tblwebUserRegistrations.Where(m => m.DesignationId == webUserRegistration.DesignationId && m.DistrictId == webUserRegistration.DistrictId && m.BlockId==webUserRegistration.BlockId).OrderBy(l => l.Name).Include(t => t.Block).Include(t => t.Class).Include(t => t.Designation).Include(t => t.District).Include(t => t.Gender).Include(t => t.State).Include(t => t.UDISE).ToList();
                    WURC.BSCList = _context.tblBlockSchoolsCounts.ToList();

                    return PartialView("/Views/WebUserRegistrations/_PVGetBlockUserList.cshtml", WURC);
                }
                else if (webUserRegistration.UserLevel == "School")
                {
                    WURC.WURList = _context.tblwebUserRegistrations.Where(m => m.DesignationId == webUserRegistration.DesignationId && m.DistrictId == webUserRegistration.DistrictId && m.BlockId == webUserRegistration.BlockId && m.UDISEID == webUserRegistration.UDISEID).OrderBy(l => l.Name).Include(t => t.Block).Include(t => t.Class).Include(t => t.Designation).Include(t => t.District).Include(t => t.Gender).Include(t => t.State).Include(t => t.UDISE).ToList();

                    WURC.HWMList = _context.tblHWMs.ToList();

                    return PartialView("/Views/WebUserRegistrations/_PVGetSchoolUserList.cshtml", WURC);
                }
                return View();
            }
            catch (Exception ex)
            {
                return Json("0");
            }
        }
        public IActionResult WebRegList(string UserLevel,int DesginationID,int DistrictID,int BlockID,int UDISEID)
        {
            try
            {
                WebUserRegistrationCollection WURC = new WebUserRegistrationCollection();


                if (UserLevel == "District")
                {
                    var WebUserList = _context.tblwebUserRegistrations.Where(m => m.DesignationId == DesginationID && m.DistrictId == DistrictID).OrderBy(l => l.Name).Include(t => t.Block).Include(t => t.Class).Include(t => t.Designation).Include(t => t.District).Include(t => t.Gender).Include(t => t.State).Include(t => t.UDISE).ToList();

                    return PartialView("/Views/WebUserRegistrations/_PVGetDistrictUserList.cshtml", WebUserList);
                }
                else if(UserLevel == "Block")
                {
                    WURC.WURList = _context.tblwebUserRegistrations.Where(m => m.DesignationId == DesginationID && m.BlockId == BlockID).OrderBy(l => l.Name).Include(t => t.Block).Include(t => t.Class).Include(t => t.Designation).Include(t => t.District).Include(t => t.Gender).Include(t => t.State).Include(t => t.UDISE).ToList();

                    WURC.BSCList = _context.tblBlockSchoolsCounts.ToList();

                    return PartialView("/Views/WebUserRegistrations/_PVGetBlockUserList.cshtml", WURC);
                }
                else if (UserLevel == "School")
                {
                    WURC.WURList = _context.tblwebUserRegistrations.Where(m => m.DesignationId == DesginationID && m.UDISEID == UDISEID).OrderBy(l => l.Name).Include(t => t.Block).Include(t => t.Class).Include(t => t.Designation).Include(t => t.District).Include(t => t.Gender).Include(t => t.State).Include(t => t.UDISE).ToList();

                    WURC.HWMList = _context.tblHWMs.ToList();

                    return PartialView("/Views/WebUserRegistrations/_PVGetSchoolUserList.cshtml", WURC);
                }
                else
                {

                }

                return View();
            }
            catch (Exception ex)
            {
                return Json("0");
            }
        }


        public JsonResult SaveBlockQues(int UserRegistrationID, int StateId, int DistrictId, int BlockId, int NoHigherPrimarySchool, int NoSecondarySchool, int NoHigherSecondarySchool, int NoHigherPrimaryStudent, int NoSecondaryStudent, int NoHigherSecondaryStudent, int SessionYear)
        {
            try
            {
                tblBlockSchoolsCount BlockSchoolsCount = new tblBlockSchoolsCount();

                BlockSchoolsCount.UserRegistrationID = UserRegistrationID;
                BlockSchoolsCount.StateId = StateId;
                BlockSchoolsCount.DistrictId = DistrictId;
                BlockSchoolsCount.BlockId = BlockId;
                BlockSchoolsCount.NoHigherPrimarySchool = NoHigherPrimarySchool;
                BlockSchoolsCount.NoSecondarySchool = NoSecondarySchool;
                BlockSchoolsCount.NoHigherSecondarySchool = NoHigherSecondarySchool;
                BlockSchoolsCount.NoHigherPrimaryStudent = NoHigherPrimaryStudent;
                BlockSchoolsCount.NoSecondaryStudent = NoSecondaryStudent;
                BlockSchoolsCount.NoHigherSecondaryStudent = NoHigherSecondaryStudent;

                if (SessionYear == 0)
                {
                    BlockSchoolsCount.SessionYear = null;
                }
                else
                {
                    BlockSchoolsCount.SessionYear = SessionYear;
                }

                BlockSchoolsCount.CreatedOn = DateTime.Now;

                _context.Add(BlockSchoolsCount);
                _context.SaveChanges();

                return Json(BlockSchoolsCount);
            }
            catch (Exception ex)
            {
                return Json("0");
            }
        }



        public IActionResult BlockQuesDatashow(int UserRegWebID)
        {
            try
            {
                WebUserRegistrationCollection WURC = new WebUserRegistrationCollection();

                WURC.BSCList = _context.tblBlockSchoolsCounts.Where(m=>m.UserRegistrationID==UserRegWebID).ToList();

                return PartialView("/Views/WebUserRegistrations/_PVGetBlockUserDataList.cshtml", WURC);

            }
            catch (Exception ex)
            {
                return Json("0");
            }          
        }

    }
}
