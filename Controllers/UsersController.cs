using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SchoolProgramme.CommonCS;
using SchoolProgramme.Models;

namespace SchoolProgramme.Controllers
{
    public class UsersController : Controller
    {
        private readonly SPDBContext _context;

        public UsersController(SPDBContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            ViewData["StateID"] = new SelectList(_context.LocationStates.Where(m => m.IsDeleted == 0 && m.StateID == 5), "StateID", "StateName");
            var sPDBContext = _context.MstUsers.Include(m => m.Role);
            return View(await sPDBContext.ToListAsync());
        }

        [HttpGet]
        public IActionResult AssignSchool(int? id)
        {
            
            UserCommonModel UserCommon = new UserCommonModel();
            //var user = _context.t
            UserCommon.LSTUDISE_Code = _context.tblUDISE_Codes.Where(m => m.UDISEID == id).Include(p => p.State).Include(p => p.District).Include(p => p.Block).Where(p => p.IsDeleted == 0).ToList();

            UserCommon.LSTUserSchoolMap = _context.tblUserSchoolMappings.Where(p => p.userId == id).ToList();
            ViewData["UseriD"] = id;
            ViewData["StateID"] = new SelectList(_context.LocationStates.Where(m => m.IsDeleted == 0 && m.StateID==5), "StateID", "StateName");
            
            return View(UserCommon);
        }

        //GetDistrict


        public JsonResult GetDistrict(int stateid)
        {
            try
            {
                return Json(_context.LocationDistricts.Where(m => m.StateID == stateid).OrderBy(p => p.District).ToList());
            }
             catch(Exception ex)
            {
                return Json("0");
            }
        }

        //GETBLOCK
        public JsonResult GetBlock(int districtid)
        {
            try
            {
                return Json(_context.LocationBlocks.Where(m =>m.DistrictID == districtid).OrderBy(l =>l.BlockName).ToList());
            }
            catch(Exception ex)
            {
                return Json("0");
            }
        }

        //GETSchoolList
     


        // GET: Users/Create
        public IActionResult Create()
        {
            ViewBag.Role = _context.MstRoles.OrderBy(p => p.RoleID).ToList();
            ViewBag.UserType = _context.tblUserTypes.OrderBy(p => p.UserType).ToList();
            ViewBag.StateID = _context.LocationStates.Where(m=>m.IsDeleted==0 && m.StateID==5).OrderBy(p => p.StateName).ToList();
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MstUser mstUsers,string LocationDetailselectedItems,string SUID)
        {
            var UserID = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
            if (Convert.ToString(HttpContext.Session.GetString("UserID")) == "")
            {
                return RedirectToAction("Login", "Logout");
            }
            if (ModelState.IsValid)
            {
                if (mstUsers.RoleID == 1 || mstUsers.RoleID == 2 || mstUsers.RoleID == 3)
                {
                    if (LocationDetailselectedItems != "hdd" && LocationDetailselectedItems != "[]")
                    {
                        mstUsers.Password = CreateUserNameHash(mstUsers.Password);
                        mstUsers.CreatedBy = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
                        mstUsers.CreatedOn = DateTime.Now;
                       // mstUsers.UserTypeID = 1;
                        _context.Add(mstUsers);
                        _context.SaveChanges();

                        if (mstUsers.RoleID == 6)
                        {

                        }
                        else
                        {
                            List<treenodes> LocationDetailItem = JsonConvert.DeserializeObject<List<treenodes>>(LocationDetailselectedItems);
                            if (mstUsers.RoleID == 1)
                            {
                                List<UserState> lstLocationdetail = new List<UserState>();
                                foreach (var item1 in LocationDetailItem)
                                {
                                    lstLocationdetail.Add(new UserState
                                    {
                                        UserID = mstUsers.UserID,
                                        StateID = Convert.ToInt32(item1.id),
                                        CreatedBy = Convert.ToInt32(HttpContext.Session.GetString("UserID")),
                                        CreatedOn = DateTime.Now,
                                    });
                                }
                                _context.AddRange(lstLocationdetail);
                            }
                            else if (mstUsers.RoleID == 2)
                            {
                                List<UserDistrict> lstLocationdetail = new List<UserDistrict>();
                                foreach (var item1 in LocationDetailItem)
                                {
                                    lstLocationdetail.Add(new UserDistrict
                                    {
                                        UserID = mstUsers.UserID,
                                        DistrictID = Convert.ToInt32(item1.id),
                                        CreatedBy = Convert.ToInt32(HttpContext.Session.GetString("UserID")),
                                        CreatedOn = DateTime.Now,
                                    });
                                }
                                _context.AddRange(lstLocationdetail);
                            }
                            else if (mstUsers.RoleID == 3)
                            {
                                List<UserBlock> lstLocationdetail = new List<UserBlock>();
                                foreach (var item1 in LocationDetailItem)
                                {
                                    lstLocationdetail.Add(new UserBlock
                                    {
                                        UserID = mstUsers.UserID,
                                        BlockID = Convert.ToInt32(item1.id),
                                        CreatedBy = Convert.ToInt32(HttpContext.Session.GetString("UserID")),
                                        CreatedOn = DateTime.Now,
                                    });
                                }
                                //_context.UserState.Where(m => m.UserID == mstUsers.UserID
                                //                       .ToList().ForEach(p => _context.UserState.Remove(p));
                                _context.AddRange(lstLocationdetail);
                            }
                            _context.SaveChanges();
                        }

                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {

                    }

                }
                else if(mstUsers.RoleID == 4 || mstUsers.RoleID == 5)
                {
                    if (!string.IsNullOrEmpty(SUID))
                    {
                        if(SUID!="0")
                        {
                            mstUsers.Password = CreateUserNameHash(mstUsers.Password);
                            mstUsers.CreatedBy = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
                            mstUsers.CreatedOn = DateTime.Now;
                            _context.Add(mstUsers);
                            _context.SaveChanges();

                            List<tblUserSchoolMapping> tblUserSchoolMapping = new List<tblUserSchoolMapping>();

                            tblUserSchoolMapping.Add(new tblUserSchoolMapping
                            {
                                userId = mstUsers.UserID,
                                UdiseID = Convert.ToInt32(SUID)
                            });
                            _context.AddRange(tblUserSchoolMapping);
                            _context.SaveChanges();

                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            ViewBag.Message = "Please Select School";
                            ViewBag.RollNo = 1;
                        }
                    
                    }
                    else
                    {

                    }
                }     
                else if(mstUsers.RoleID == 0)
                {
                    ViewBag.Message = "Please Select Role";
                    ViewBag.RollNo = 2;
                }
                else
                {
                   
                    mstUsers.Password = CreateUserNameHash(mstUsers.Password);
                    mstUsers.CreatedBy = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
                    mstUsers.CreatedOn = DateTime.Now;
                    _context.Add(mstUsers);
                    _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }              
            }

            ViewBag.Role = _context.MstRoles.OrderBy(p => p.RoleID).ToList();
            ViewBag.UserType = _context.tblUserTypes.OrderBy(p => p.UserType).ToList();
            ViewBag.StateID = _context.LocationStates.Where(m => m.IsDeleted == 0 && m.StateID == 5).OrderBy(p => p.StateName).ToList();
            return View();
        }

        // GET: Users/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mstUsers =  _context.MstUsers.SingleOrDefault(m => m.UserID == id);
            if (mstUsers == null)
            {
                return NotFound();
            }
            
            ViewBag.Role = _context.MstRoles.OrderBy(p => p.Role).ToList();
            ViewBag.StateID = _context.LocationStates.OrderBy(p => p.StateName).ToList();
            fillUserLOCATION(mstUsers.UserID, mstUsers.RoleID);
            return View(mstUsers);
        }

       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id ,[Bind("FullName,UserEmailID,Mobile")] MstUser mstUser)
        {           
            if (ModelState.IsValid)
            {

                var User =  _context.MstUsers.Where(m => m.UserID == id).FirstOrDefault();
                
                if (User == null)
                {
                    return NotFound();
                }
                else
                {

                    User.UserEmailID = mstUser.UserEmailID;
                    User.FullName = mstUser.FullName;
                    User.Mobile = mstUser.Mobile;
                    User.UpdatedBy = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
                    User.UpdatedOn = DateTime.Now;
                    
                    _context.Update(User);
                    _context.SaveChanges();
              
                }
                return RedirectToAction(nameof(Index));
            }
            
            return View(mstUser);
        }

        public async Task<IActionResult> UpdateRole(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mstUser = await _context.MstUsers.SingleOrDefaultAsync(m => m.UserID == id);
            ViewBag.Role = _context.MstRoles.OrderBy(p => p.Role).ToList();
            ViewBag.UserRoleID = mstUser.RoleID;
            ViewBag.StateID = _context.LocationStates.OrderBy(p => p.StateName).ToList();
            return View(mstUser);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateRole(int id, MstUser mstUser, string LocationDetailselectedItems, int SUID)
        {

            if (ModelState.IsValid)
            {
                var User = _context.MstUsers.Where(m => m.UserID == id).FirstOrDefault();
                if (mstUser.RoleID == 1 || mstUser.RoleID == 2 || mstUser.RoleID == 3)
                {
                    if (LocationDetailselectedItems != "hdd" && LocationDetailselectedItems != "[]")
                    {
                        // remove location start
                        int roleIdUser = (int) _context.MstUsers.Where(p => p.UserID == mstUser.UserID).FirstOrDefault().RoleID;
                        //int roleIdUser = _context.MstUsers.Where(p => p.UserID == mstUser.UserID).ToList();
                        if (roleIdUser == 1 || roleIdUser == 2 || roleIdUser == 3 || roleIdUser == 4 || roleIdUser == 5)
                        {
                            if (roleIdUser == 1)
                            {
                                var roleUsers = _context.UserStates.Where(p => p.UserID == mstUser.UserID).ToList();
                                if (roleUsers.Count > 0)
                                    _context.RemoveRange(roleUsers);
                            }
                            else if (roleIdUser == 2)
                            {
                                var roleUsers = _context.UserDistricts.Where(p => p.UserID == mstUser.UserID).ToList();
                                if (roleUsers.Count > 0)
                                    _context.RemoveRange(roleUsers);
                            }
                            else if (roleIdUser == 3)
                            {
                                var roleUsers = _context.UserBlocks.Where(p => p.UserID == mstUser.UserID).ToList();
                                if (roleUsers.Count > 0)
                                    _context.RemoveRange(roleUsers);
                            }
                            else if (roleIdUser == 4)
                            {
                                var roleUsers = _context.tblUserSchoolMappings.Where(p => p.userId == mstUser.UserID).ToList();
                                if (roleUsers.Count > 0)
                                    _context.RemoveRange(roleUsers);
                            }

                            else if (roleIdUser == 5)
                            {
                                var roleUsers = _context.tblUserSchoolMappings.Where(p => p.userId == mstUser.UserID).ToList();
                                if (roleUsers.Count > 0)
                                    _context.RemoveRange(roleUsers);
                            }
                        }

                        User.UpdatedBy = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
                        User.UpdatedOn = DateTime.Now;
                        User.RoleID = mstUser.RoleID;
                        _context.MstUsers.Update(User);

                        List<treenodes> LocationDetailItem = JsonConvert.DeserializeObject<List<treenodes>>(LocationDetailselectedItems);
                        if (mstUser.RoleID == 1)
                        {
                            List<UserState> lstLocationdetail = new List<UserState>();
                            foreach (var item1 in LocationDetailItem)
                            {
                                lstLocationdetail.Add(new UserState
                                {

                                    UserID = mstUser.UserID,
                                    StateID = Convert.ToInt32(item1.id),
                                    IsDeleted = 0,
                                    CreatedBy = Convert.ToInt32(HttpContext.Session.GetString("UserID")),
                                    CreatedOn = DateTime.Now,
                                });
                            }
                            _context.AddRange(lstLocationdetail);
                        }
                        else if (mstUser.RoleID == 2)
                        {
                            List<UserDistrict> lstLocationdetail = new List<UserDistrict>();
                            foreach (var item1 in LocationDetailItem)
                            {
                                lstLocationdetail.Add(new UserDistrict
                                {
                                    UserID = mstUser.UserID,
                                    DistrictID = Convert.ToInt32(item1.id),
                                    IsDeleted = 0,
                                    CreatedBy = Convert.ToInt32(HttpContext.Session.GetString("UserID")),
                                    CreatedOn = DateTime.Now,
                                });
                            }
                            _context.AddRange(lstLocationdetail);
                        }
                        else if (mstUser.RoleID == 3)
                        {
                            List<UserBlock> lstLocationdetail = new List<UserBlock>();
                            foreach (var item1 in LocationDetailItem)
                            {
                                lstLocationdetail.Add(new UserBlock
                                {
                                    UserID = mstUser.UserID,
                                    BlockID = Convert.ToInt32(item1.id),
                                    IsDeleted = 0,
                                    CreatedBy = Convert.ToInt32(HttpContext.Session.GetString("UserID")),
                                    CreatedOn = DateTime.Now,
                                });
                            }
                            _context.AddRange(lstLocationdetail);
                        }

                        _context.SaveChanges();

                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ViewBag.Role = mstUser.RoleID;
                        ViewData["RoleId"] = new SelectList(_context.MstRoles.Where(p => p.IsDeleted == 0), "RoleID", "Role1");
                        ViewBag.Message = "Please Select The Location";
                        return View(mstUser);
                    }
                }
                else
                {
                    // remove location start
                    int roleIdUser = (int)_context.MstUsers.Where(p => p.UserID == mstUser.UserID).FirstOrDefault().RoleID;
                    if (roleIdUser == 1 || roleIdUser == 2 || roleIdUser == 3 || roleIdUser == 4 || roleIdUser == 5)
                    {
                        if (roleIdUser == 1)
                        {
                            var roleUsers = _context.UserStates.Where(p => p.UserID == mstUser.UserID).ToList();
                            if (roleUsers.Count > 0)
                                _context.RemoveRange(roleUsers);
                        }
                        else if (roleIdUser == 2)
                        {
                            var roleUsers = _context.UserDistricts.Where(p => p.UserID == mstUser.UserID).ToList();
                            if (roleUsers.Count > 0)
                                _context.RemoveRange(roleUsers);
                        }
                        else if (roleIdUser == 3)
                        {
                            var roleUsers = _context.UserBlocks.Where(p => p.UserID == mstUser.UserID).ToList();
                            if (roleUsers.Count > 0)
                                _context.RemoveRange(roleUsers);
                        }

                        else if (roleIdUser == 4)
                        {
                            var roleUsers = _context.tblUserSchoolMappings.Where(p => p.userId == mstUser.UserID).ToList();
                            if (roleUsers.Count > 0)
                                _context.RemoveRange(roleUsers);
                        }

                        else if (roleIdUser == 5)
                        {
                            var roleUsers = _context.tblUserSchoolMappings.Where(p => p.userId == mstUser.UserID).ToList();
                            if (roleUsers.Count > 0)
                                _context.RemoveRange(roleUsers);
                        }
                    }              
                    // remove location end

                    User.UpdatedBy = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
                    User.UpdatedOn = DateTime.Now;
                    User.RoleID = mstUser.RoleID;
                    _context.MstUsers.Update(User);
                    _context.SaveChanges();

                    if (mstUser.RoleID == 4 && SUID != 0)
                    {
                        tblUserSchoolMapping tblUserSchoolMapping = new tblUserSchoolMapping();
                        tblUserSchoolMapping.userId = User.UserID ;
                        tblUserSchoolMapping.UdiseID = SUID;                      
                        _context.Add(tblUserSchoolMapping);
                        _context.SaveChanges();
                    }
                    else if (mstUser.RoleID == 5 && SUID != 0)
                    {
                        tblUserSchoolMapping tblUserSchoolMapping = new tblUserSchoolMapping();
                        tblUserSchoolMapping.userId = User.UserID;
                        tblUserSchoolMapping.UdiseID = SUID;
                        _context.Add(tblUserSchoolMapping);
                        _context.SaveChanges();
                    }
           
                    return RedirectToAction(nameof(Index));
                }
            }
            ViewBag.Role = mstUser.RoleID;
            ViewData["RoleId"] = new SelectList(_context.MstUsers.Where(p => p.IsDeleted == 0), "RoleID", "Role1");
            return View(mstUser);
        }


        public JsonResult GetUserLocation(int UserId, int RoleId)
        {
            if (RoleId == 1) // StateLevel
            {
                var listState = _context.UserStates.Where(p => p.UserID == UserId).Select(p => p.StateID).ToList();
               // var result = _context.LocationStates.Where(p => listState.Contains(p.StateID)).Select(p=>p.StateName).ToList();
                var result = _context.LocationStates.Where(p => listState.Contains(p.StateID)).ToList();
                //var result = _context.UserStates.Where(p => p.UserID == UserId).Select(p => p.StateID).ToList();
                return Json(result);
            }
            else if (RoleId == 2) // DistrictLevel
            {
                var listDistrict = _context.UserDistricts.Where(p => p.UserID == UserId).Select(p => p.DistrictID).ToList();
                var result = _context.LocationDistricts.Where(p => listDistrict.Contains(p.DistrictID)).Include(p => p.State).ToList();
                return Json(result);
            }
            else if (RoleId == 3)// BlockLevel
            {
                var listBlock = _context.UserBlocks.Where(p => p.UserID == UserId).Select(p => p.BlockID).ToList();
                var result = _context.LocationBlocks.Where(p => listBlock.Contains(p.BlockID)).Include(p => p.State).Include(p => p.District).ToList();
                return Json(result);
            }
            else if (RoleId == 4)// Principle
            {
                var listSCM = _context.tblUserSchoolMappings.Where(p => p.userId == UserId).Select(p => p.UdiseID).ToList();
                var result = _context.tblUDISE_Codes.Where(p => listSCM.Contains(p.UDISEID)).Include(p => p.State).Include(p => p.District).Include(p => p.Block).ToList();
                return Json(result);
            }
            else if (RoleId == 5)// Teacher
            {
                var listSCM = _context.tblUserSchoolMappings.Where(p => p.userId == UserId).Select(p => p.UdiseID).ToList();
                var result = _context.tblUDISE_Codes.Where(p => listSCM.Contains(p.UDISEID)).Include(p => p.State).Include(p => p.District).Include(p => p.Block).ToList();
                return Json(result);
            }
            else
            {
                return Json("");
            }
        }


        private bool MstUserExists(int UserName)
        {
            return _context.MstUsers.Any(e => e.UserID == UserName);
        }
        public string fillUserLOCATIONEdit(int RoleID, int id)
        {
            List<treenodes> Locnodes = new List<treenodes>();
            IQueryable<LocationState> distinctState = null;

            var statelist = from c in _context.LocationStates
                            select c;

            var districtlist = from c in _context.LocationDistricts
                               select c;

            var blocklist = from c in _context.LocationBlocks
                            select c;
            if (RoleID == 2)
            {
                statelist = from c in _context.LocationStates
                            join cn in _context.UserStates on c.StateID equals cn.StateID
                            where (cn.UserID == id)
                            select c;
            }
            else if (RoleID == 3)
            {
                var stateDetails = from c in _context.LocationStates
                                   join cn in _context.LocationDistricts on c.StateID equals cn.StateID
                                   join ud in _context.UserDistricts on cn.DistrictID equals ud.DistrictID
                                   where (ud.UserID == id)
                                   select c;
                distinctState = stateDetails.GroupBy(x => x.StateID).Select(y => new LocationState() { StateID = y.Key });
                districtlist = _context.LocationDistricts.Join(distinctState, Dist => Dist.StateID, st => st.StateID, (Dist, st) => Dist);

                //districtlist = _context.MstDistrict.Join(statelist, Dist => Dist.StateId, st => st.StateId, (Dist, st) => Dist);
            }
            else
            {
                statelist = from c in _context.LocationStates
                            join cn in _context.LocationDistricts on c.StateID equals cn.StateID
                            join bd in _context.LocationBlocks on cn.DistrictID equals bd.DistrictID
                            join ud in _context.UserBlocks on bd.BlockID equals ud.BlockID
                            where (ud.UserID == id)
                            select c;
                districtlist = _context.LocationDistricts.Join(statelist, Dist => Dist.StateID, st => st.StateID, (Dist, st) => Dist);
                blocklist = _context.LocationBlocks.Join(districtlist, blck => blck.DistrictID, dist => dist.DistrictID, (blck, dist) => blck);

            }
            string rjson = JsonConvert.SerializeObject(Locnodes);
            ViewBag.LocationDetailJson = JsonConvert.SerializeObject(Locnodes);
            return rjson;
        }

        public static string CreateUserNameHash(string UserName)
        {
            int Password_saltArraySize = 16;
            string saltAndPwd = String.Concat(UserName, Password_saltArraySize.ToString());
            HashAlgorithm hashAlgorithm = SHA512.Create();
            List<byte> pass = new List<byte>(Encoding.Unicode.GetBytes(saltAndPwd));
            string hashedPwd = Convert.ToBase64String(hashAlgorithm.ComputeHash(pass.ToArray()));
            hashedPwd = String.Concat(hashedPwd, Password_saltArraySize.ToString());
            return hashedPwd;
        }

        [HttpPost]
        public string fillUserLOCATIONFLT(int RoleID, int id, string LocationItems)
        {
            List<treenodes> LocationDetailItem = JsonConvert.DeserializeObject<List<treenodes>>(LocationItems);

            List<treenodes> Locnodes = new List<treenodes>();

            DataTable dt = ToDataTable<treenodes>(LocationDetailItem);
            dt.Columns.Remove("parent");
            dt.Columns.Remove("parentMain2");
            dt.Columns.Remove("parentMain1");
            dt.Columns.Remove("parentMain");
            dt.Columns.Remove("text");
            DataSet ds = GetMstState(dt, _context);

            if (RoleID == 1) //state
            {
                foreach (DataRow dtRow in ds.Tables[0].Rows)
                {
                    Locnodes.Add(new treenodes { id = dtRow["StateID"].ToString(), parent = "#", text = dtRow["StateName"].ToString() });
                }
                var AlredyExistLoc = _context.UserStates.Where(s => s.UserID == id)
                 .Select(s => new SelectListItem
                 {
                     Value = s.StateID.ToString(),
                     Text = ""
                 });
                ViewBag.SelLocationDetailJson = JsonConvert.SerializeObject(new SelectList(AlredyExistLoc, "Value", "Text", id));
            }
            else if (RoleID == 2) //District
            {
                foreach (DataRow dtRow in ds.Tables[0].Rows)
                {
                    Locnodes.Add(new treenodes { id = "S" + dtRow["StateID"].ToString(), parent = "#", text = dtRow["StateName"].ToString() });
                }
                //Loop and add the Parent Nodes.
                foreach (DataRow dtRow in ds.Tables[1].Rows)
                {
                    Locnodes.Add(new treenodes { id = dtRow["StateID"].ToString() + "-" + dtRow["DistrictID"].ToString(), parent = "S" + dtRow["StateID"].ToString(), text = dtRow["District"].ToString() });
                }
                var AlredyExistLoc = _context.UserDistricts.Where(s => s.UserID == id)
                   .Select(s => new SelectListItem
                   {
                       Value = _context.LocationDistricts.Where(p => p.DistrictID == s.DistrictID).Select(p => p.StateID).FirstOrDefault().ToString() + "-" + s.DistrictID.ToString(),
                       Text = ""
                   });
                ViewBag.SelLocationDetailJson = JsonConvert.SerializeObject(new SelectList(AlredyExistLoc, "Value", "Text", id));
            }
            else if (RoleID == 3) //Block
            {
                foreach (DataRow dtRow in ds.Tables[0].Rows)
                {
                    Locnodes.Add(new treenodes { id = "S" + dtRow["StateID"].ToString(), parent = "#", text = dtRow["StateName"].ToString() });
                }
                //Loop and add the Parent Nodes.
                foreach (DataRow dtRow in ds.Tables[1].Rows)
                {
                    Locnodes.Add(new treenodes { id = "D" + dtRow["DistrictID"].ToString(), parent = "S" + dtRow["StateID"].ToString(), text = dtRow["District"].ToString() });
                }
                foreach (DataRow dtRow in ds.Tables[2].Rows)
                {
                    /*Locnodes.Add(new TreeViewNode { id = dtRow["StateID"].ToString() + "-" + dtRow["DistrictID"].ToString() + "-" + dtRow["BlockID"].ToString(), parent = "D" + dtRow["DistrictID"].ToString(), text = dtRow["BlockName"].ToString() })*/
                    ;

                    Locnodes.Add(new treenodes { id = "B" + "-" + dtRow["DistrictID"].ToString() + "-" + dtRow["BlockID"].ToString(), parent = "D" + dtRow["DistrictID"].ToString(), text = dtRow["BlockName"].ToString() });
                }
                var AlredyExistLoc = _context.UserBlocks.Where(s => s.UserID == id)
                   .Select(s => new SelectListItem
                   {
                       Value = _context.LocationDistricts.Where(p => p.DistrictID == _context.LocationBlocks.Where(b => b.BlockID == s.BlockID).Select(b => b.DistrictID).FirstOrDefault()).Select(p => p.StateID).FirstOrDefault().ToString() + "-" + _context.LocationBlocks.Where(p => p.BlockID == s.BlockID).Select(p => p.DistrictID).FirstOrDefault().ToString() + "-" + s.BlockID.ToString(),
                       Text = ""
                   });
                ViewBag.SelLocationDetailJson = JsonConvert.SerializeObject(new SelectList(AlredyExistLoc, "Value", "Text", id));
            }
            string rjson = JsonConvert.SerializeObject(Locnodes);
            ViewBag.LocationDetailJson = JsonConvert.SerializeObject(Locnodes);
            return rjson;
        }

        [HttpPost]
        public string fillUserLOCATION(int id,int? Roleid)
        {
            List<treenodes> loctreenodes = new List<treenodes>();
            var statelist = from c in _context.LocationStates.Where(m => m.IsDeleted==0 && m.StateID==5)
                            select c;
            foreach(LocationState state in statelist)
            {
                loctreenodes.Add(new treenodes { id = state.StateID.ToString(), parent = "#", text = state.StateName });
            }
            var AlredyExistLoc = _context.UserStates.Where(s => s.UserID == id)
              .Select(s => new SelectListItem
              {
                  Value = s.StateID.ToString(),
                  Text = ""
              });
            ViewBag.SelLocationDetailJson = JsonConvert.SerializeObject(new SelectList(AlredyExistLoc, "Value", "Text", id));

            string rjson = JsonConvert.SerializeObject(loctreenodes);
            ViewBag.LocationDetailJson = JsonConvert.SerializeObject(loctreenodes);
            return rjson;
        }
        

        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties by using reflection   
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names  
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {

                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }

            return dataTable;
        }
        public static DataSet GetMstState(DataTable treenodes, SPDBContext db)
        {
            try
            {
                DataSet ds = new DataSet();
                SqlParameter[] s = new SqlParameter[] {
                   new SqlParameter("loc",treenodes),
                };
                ds = CommonCS.CommonFN.Procedure_Query_ToDataSet(db, "Get_Statelist", CommandType.StoredProcedure, s);
                return ds;
            }
            catch (Exception ex)
            {

                return new DataSet();
            }
        }
        private static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        
        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {


                    if (pro.Name == column.ColumnName)
                    {
                        if (dr[column.ColumnName] == DBNull.Value)
                        {
                            dr[column.ColumnName] = DBNull.Value;
                        }
                        else
                        {
                            pro.SetValue(obj, dr[column.ColumnName], null);
                        }

                    }
                    else
                        continue;
                }
            }
            return obj;
        }
        [HttpPost]
        public bool IsUserAvailable(string UserName)
        {
            var RegUserNAme = (from u in _context.MstUsers
                               where u.UserName.ToUpper() == UserName.ToUpper()
                               select new { UserName }).FirstOrDefault();
            bool status;
            if (RegUserNAme != null)
            {
                //Already registered  
                status = false;
            }
            else
            {
                //Available to use  
                status = true;
            }

            return status;
        }

        [HttpPost]
        public bool IsUsercontectAvailable(string Contact)
        {
            var RegUserMobileno = (from u in _context.MstUsers
                               where u.Mobile == Contact
                                   select new { Contact }).FirstOrDefault();
            bool status;
            if (RegUserMobileno != null)
            {
                //Already registered  
                status = false;
            }
            else
            {
                //Available to use  
                status = true;
            }

            return status;
        }

        [HttpPost]
        public bool IsUserEmailAvailable(string UEmail)
        {

            var RegUserEmail = (from u in _context.MstUsers
                                where u.UserEmailID.ToUpper() == UEmail.ToUpper()
                                select new { UEmail }).FirstOrDefault();

            bool status;
            if (RegUserEmail != null)
            {
                //Already registered  
                status = false;
            }
            else
            {
                //Available to use  
                status = true;
            }

            return status;
        }

        [HttpPost]
        public IActionResult FillGroupContent(int UserID, int Block)
        {
            try
            {
                UserCommonModel UserCommon = new UserCommonModel();

                UserCommon.LSTUDISE_Code = _context.tblUDISE_Codes.Where(m =>m.BlockID == Block).Include(p => p.State).Include(p => p.District).Include(p => p.Block).Where(p => p.IsDeleted == 0).ToList();

                UserCommon.LSTUserSchoolMap = _context.tblUserSchoolMappings.Where(p => p.userId == UserID).ToList();
                return PartialView("/Views/Users/_PVGetForGRPContactList.cshtml", UserCommon);
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index));
            }

        }     

        public class suidlist
        {
            public int suid { get; set; }
        }

        [HttpPost]
        public IActionResult LinkMember(string MemselectedItems, int userid)
        {
            List<suidlist> suidlist = JsonConvert.DeserializeObject<List<suidlist>>(MemselectedItems);

            List<tblUserSchoolMapping> lstPart = new List<tblUserSchoolMapping>();

            foreach (var item1 in suidlist)
            {

                lstPart.Add(new tblUserSchoolMapping
                {
                    userId = userid,
                    UdiseID = item1.suid

                });
            }

          // _context.tblUserSchoolMappings.Where(m => m.userId == userid).ToList().ForEach(p => _context.tblUserSchoolMappings.Remove(p));

            _context.AddRange(lstPart);
            _context.SaveChanges();
            return Json(JsonConvert.SerializeObject(userid));
        }
  
        public JsonResult GetAssignSchoolShow(int UserId)
        {
            SqlParameter[] s = new SqlParameter[] {
            new SqlParameter("UserId",UserId),
            };
            DataTable dt = CommonFN.Procedure_Query_ToDataTable(_context, "GetAssignSchoolShow", CommandType.StoredProcedure, s);
            return Json(JsonConvert.SerializeObject(dt));
        }


        public JsonResult UserAllMappingDelete(int MappingID)
        {
           //_context.tblUserSchoolMappings.Where(m => m.userId == UserId).ToList().ForEach(p => _context.tblUserSchoolMappings.Remove(p));

           // return Json(JsonConvert.SerializeObject(UserId));
            SqlParameter[] s = new SqlParameter[] {
            new SqlParameter("UserId",MappingID),
            };
            DataTable dt = CommonFN.Procedure_Query_ToDataTable(_context, "USP_UserAllMappingDelete", CommandType.StoredProcedure, s);
            return Json(JsonConvert.SerializeObject(dt));
        }

        public IActionResult UserMappingDelete(int MappingID)
        {
            SqlParameter[] s = new SqlParameter[] {
            new SqlParameter("MappingID",MappingID),
            };
            DataTable dt = CommonFN.Procedure_Query_ToDataTable(_context, "USP_UserMappingDelete", CommandType.StoredProcedure, s);
            return Json(JsonConvert.SerializeObject(dt));


           // var tblUserSchoolMappings = _context.tblUserSchoolMappings.FirstOrDefaultAsync(m => m.MappingId == MappingID);
           // _context.tblUserSchoolMappings.Remove(tblUserSchoolMappings);
           // _context.SaveChanges();           
           //// _context.tblUserSchoolMappings.Where(m => m.MappingId == MappingID).ToList().ForEach(p => _context.tblUserSchoolMappings.Remove(p));
           // return Json(JsonConvert.SerializeObject(MappingID));
        }
        public IActionResult BackAssignList(int UserID)
        {          
            return Json(JsonConvert.SerializeObject(UserID));
        }
    }
}
