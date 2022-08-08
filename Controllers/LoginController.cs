using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using SchoolProgramme.CommonCS;
using SchoolProgramme.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using DNTCaptcha.Core;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;
using System.Data;
using Newtonsoft.Json;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SchoolProgramme.Controllers
{
    public class LoginController : Controller
    {
        private readonly SPDBContext _context;
        private readonly ILogger<LoginController> _logger;
        private readonly IWebHostEnvironment webHostEnvironment;

        private readonly IDNTCaptchaValidatorService _validatorService;

        private readonly DNTCaptchaOptions _captchaOptions;

        public LoginController(ILogger<LoginController> logger, SPDBContext context, IWebHostEnvironment webHostEnvironment, IDNTCaptchaValidatorService validatorService, IOptions<DNTCaptchaOptions> options)
        {
            _logger = logger;
            _context = context;
            this.webHostEnvironment = webHostEnvironment;
            _validatorService = validatorService;
            _captchaOptions = options == null ? throw new ArgumentNullException(nameof(options)) : options.Value;
        }


        // GET: Login
        public ActionResult Login()
        {

            return View();
        }

        //// POST: Login/User Check
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(MstUser userDto)
        {
            try
            {

                var user = Authenticate(userDto.UserName, userDto.Password);
                if (user == null)
                {
                    ViewBag.Message = "You have entered an invalid username or password";
                    return View();
                }
                else
                {

                    if (ModelState.IsValid)
                    {
                        if (!_validatorService.HasRequestValidCaptchaEntry(Language.English, DisplayMode.ShowDigits))
                        {
                            // this.ModelState.AddModelError(DNTCaptchaTagHelper.CaptchaInputName, "Please Enter Valid Captcha.");
                            this.ModelState.AddModelError(_captchaOptions.CaptchaComponent.CaptchaInputName, "Please Enter Valid Captcha.");
                            return View("Login");
                        }
                    }
               
                    HttpContext.Session.SetString("UserName", user.UserName);
                    HttpContext.Session.SetString("FullName", user.FullName);
                    HttpContext.Session.SetString("UserEmailID", user.UserEmailID);
                    HttpContext.Session.SetString("UserID", user.UserID.ToString());
                    HttpContext.Session.SetString("RoleID", user.RoleID.ToString());
                    HttpContext.Session.SetString("RoleIDM", user.RoleID.ToString());
                   

                    MstUser ust = _context.MstUsers.Where(p => p.UserID == user.UserID).FirstOrDefault();
                    if(ust.NoofLogin == null)
                    {
                        ust.NoofLogin = 1;
                    }
                    else 
                    { 
                    ust.NoofLogin = ust.NoofLogin + 1;
                    }
                    ust.LastLogin = System.DateTime.Now;
                    _context.Update(ust);
                    _context.SaveChanges();


                    if (Convert.ToInt32(HttpContext.Session.GetString("UserTypeId")) == 1)
                    {
                        HttpContext.Session.SetString("MenuId", "74");
                        return RedirectToAction("Dashboard", "Dashboard");
                    }
                    else
                    {

                        HttpContext.Session.SetString("MenuId", "74");
                        return RedirectToAction("Dashboard", "Dashboard");

                        //int cntpolicy = _context.MstUsers.Where(p => p.SUID == Convert.ToInt32(HttpContext.Session.GetString("Suid")) && p.PoliciesChecked == true).Count();
                        //if (cntpolicy > 0)
                        //{
                        //    if (Convert.ToInt32(HttpContext.Session.GetString("UserTypeId")) == 1)
                        //    {
                                
                        //    }
                        //    else if (Convert.ToInt32(HttpContext.Session.GetString("Suid")) == 6020)
                        //    {
                        //        return RedirectToAction("ResourcesSchoolsGallery", "Gallery");
                        //    }
                        //    else
                        //    {
                        //        return RedirectToAction("InnerSchoolDashboard", "InnerSchoolDashboard");
                        //    }
                        //}
                        //else
                        //{
                        //    return RedirectToAction("ViewPoliciesCheck", "Policies");
                        //}
                    }
                   

                    ///-----------------old sum-------------------------------------//
                    //if (Convert.ToBoolean(HttpContext.Session.GetString("PoliciesChecked")) == true)
                    //{
                    //    if (Convert.ToInt32(HttpContext.Session.GetString("UserTypeId")) == 1)
                    //    {
                    //        return RedirectToAction("InnerSchoolListing", "InnerSchoolListing");
                    //    }
                    //    else
                    //    {
                    //        return RedirectToAction("InnerSchoolDashboard", "InnerSchoolDashboard");
                    //    }
                    //}
                    //else
                    //{
                    //    return RedirectToAction("ViewPoliciesCheck", "Policies");
                    //}


                }

            }
            catch (Exception ex)
            {
                return View(ex.ToString());
            }
        }


        public MstUser Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            var user = _context.MstUsers.SingleOrDefault(x => x.UserName == username && x.IsDeleted == 0);

            // check if username exists
            if (user == null)
                return null;

            //check if password is correctpassword
            var passwordHashed = CommonFN.CreatePasswordHash(password);
            if (user.Password != passwordHashed)
                return null;
            //var CHKpassword = _context.MstUser.SingleOrDefault(x => x.Password == passwordHashed);
            //if (CHKpassword == null)
            //    return null;

            // authentication successful
            return user;
        }


        public ActionResult Logout()
        {
            HttpContext.Session.Clear();
            HttpContext.Session.SetString("UserName", "");
            HttpContext.Session.SetString("FirstName", "");
            HttpContext.Session.SetString("LastName", "");
            HttpContext.Session.SetString("UserEmailID", "");
            HttpContext.Session.SetString("UserID", "");
            HttpContext.Session.SetString("UserTypeId", "");
            HttpContext.Session.SetString("RoleID", "");
            HttpContext.Session.SetString("Suid", "");
            HttpContext.Session.SetString("RoleIDM", "");
            HttpContext.Session.SetString("stepno", "");

            return RedirectToAction("Login", "Login");
        }

        #region Pravar
        public ActionResult Registration()
        {
            try
            {
                ViewBag.State = _context.MstStates.OrderBy(p=>p.StateName).ToList();
                ViewBag.SchoolType = _context.MstSchoolTypes.ToList();
                ViewBag.SchoolCurriculum = _context.MstCurriculumTypes.ToList();
                if (TempData["Username"] != null && TempData["Password"] != null)
                {
                    ViewBag.Username = TempData["Username"];
                    ViewBag.Password = TempData["Password"];
                }
                return View();
            }
            catch (Exception ex)
            {
                return View();
            }
        }
        [HttpPost]
        public ActionResult Registration(tblRegistrationForm model)
        {
            try
            {

                if (!_validatorService.HasRequestValidCaptchaEntry(Language.English, DisplayMode.ShowDigits))
                {
                    // this.ModelState.AddModelError(DNTCaptchaTagHelper.CaptchaInputName, "Please Enter Valid Captcha.");
                    this.ModelState.AddModelError(_captchaOptions.CaptchaComponent.CaptchaInputName, "Please Enter Valid Captcha.");
                    return View("Registration");
                }

                model.CreatedOn = DateTime.Now;
                model.IsVerified = false;
                model.IsDeleted = false;
                _context.tblRegistrationForms.Add(model);
                _context.SaveChanges();



                //---------------------------------Email common FN----------------------------//
                string body = "Dear " + model.SchoolName + "<br>Welcome on board!<br>Thank you for successfully registering for Fairtrade India Schools Programme. " +
                    "Fairtrade India Team will review your registration. Once approved by Fairtrade India team you will receive your credentials on email provided during the registration process." +
                    "<br>Congratulations on joining the global Fairtrade Movement." +
                    "</br>";


                string emailto = model.SchoolEmail;
                string subject = "Welcome on board!";
                string friendlyname = _context.EmailConfigurations.Where(x => x.Key == "EmailFriendlyName" && !x.IsDeleted).Select(p => p.Value).FirstOrDefault();
                string from = _context.EmailConfigurations.Where(x => x.Key == "EmailFrom" && !x.IsDeleted).Select(p => p.Value).FirstOrDefault();
                string username = _context.EmailConfigurations.Where(x => x.Key == "UserName" && !x.IsDeleted).Select(p => p.Value).FirstOrDefault();
                string password = _context.EmailConfigurations.Where(x => x.Key == "Password" && !x.IsDeleted).Select(p => p.Value).FirstOrDefault();
                string smtpclienthost = _context.EmailConfigurations.Where(x => x.Key == "SmtpClientHost" && !x.IsDeleted).Select(p => p.Value).FirstOrDefault();
                CommonFN.CommonEmailFN(body, emailto, subject, friendlyname, from, username, password, smtpclienthost);

                //----------------------------------------------------------------------//


                //---------------------------------Alert Email common FN----------------------------//
                 body = "";
                 emailto = _context.EmailConfigurations.Where(x => x.Key == "AlertRecieverEmailID" && !x.IsDeleted).Select(p => p.Value).FirstOrDefault();
                 subject = "Notification : One new School Registered. (" + model.SchoolName + ")";
                 friendlyname = _context.EmailConfigurations.Where(x => x.Key == "EmailFriendlyName" && !x.IsDeleted).Select(p => p.Value).FirstOrDefault();
                 from = _context.EmailConfigurations.Where(x => x.Key == "EmailFrom" && !x.IsDeleted).Select(p => p.Value).FirstOrDefault();
                 username = _context.EmailConfigurations.Where(x => x.Key == "UserName" && !x.IsDeleted).Select(p => p.Value).FirstOrDefault();
                 password = _context.EmailConfigurations.Where(x => x.Key == "Password" && !x.IsDeleted).Select(p => p.Value).FirstOrDefault();
                 smtpclienthost = _context.EmailConfigurations.Where(x => x.Key == "SmtpClientHost" && !x.IsDeleted).Select(p => p.Value).FirstOrDefault();
                 CommonFN.CommonEmailFN(body, emailto, subject, friendlyname, from, username, password, smtpclienthost);

                //----------------------------------------------------------------------//



                HttpContext.Session.SetString("WLSCName", model.SchoolName);
                return RedirectToAction("WelcomeMSG");
            }
            catch (Exception ex)
            {
                return View();
            }

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

        public ActionResult Profile()
        {
            try
            {
                int id = Convert.ToInt32(HttpContext.Session.GetString("Suid"));

                tblRegistrationForm model = _context.tblRegistrationForms.Where(m => m.SUID == id).FirstOrDefault();
                ViewBag.Area = _context.tblPincodeLatLongs.Where(m => m.Pincode == model.Pincode).ToList();
                ViewBag.State = _context.MstStates.ToList();
                ViewBag.District = _context.MstDistricts.Where(m => m.StateID == model.StateID).ToList();
                ViewBag.SchoolType = _context.MstSchoolTypes.ToList();
                ViewBag.SchoolCurriculum = _context.MstCurriculumTypes.ToList();
                ViewBag.Language = _context.MstLanguageTeachings.ToList();
                ViewBag.Club = _context.MstSchoolClubs.ToList();
                ViewBag.StartingGrade = _context.MstSchoolGrades.ToList();
                ViewBag.SchoolImages = _context.tblRegistrationForms.Where(m => m.SUID == id).Select(p => p.SchoolImage).FirstOrDefault();

                ViewBag.Suid = id;
                var meetingDetails = _context.Stage1STP2MeetingDetails.Where(x => x.SUID == id).ToList();
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<Stage1STP2MeetingDetail, Stage2DateOfMeeting>()
                    .ForMember(c => c.ID, c => c.MapFrom(m => m.ID))
                    .ForMember(c => c.MeetingDate, c => c.MapFrom(m => m.MeetingDate))
                    .ForMember(c => c.StartingTime, c => c.MapFrom(m => m.StartingTime))
                    .ForMember(c => c.EndTime, c => c.MapFrom(m => m.EndTime))
                    .ForMember(c => c.NumberOfPeople, c => c.MapFrom(m => m.NumberOfPeople))
                    .ForMember(c => c.NoofStudent, c => c.MapFrom(m => m.NoofStudent))
                    .ForMember(c => c.NoofTeacher, c => c.MapFrom(m => m.NoofTeacher))
                    .ForMember(c => c.NoofOther, c => c.MapFrom(m => m.NoofOther))
                    .ForMember(c => c.PurposeId, c => c.MapFrom(m => m.PurposeID))
                    .ForMember(c => c.PurposeName, c => c.MapFrom(m => GetPurposeName(m.PurposeID)))
                    .ForMember(c => c.hdnMeetingUpload, c => c.MapFrom(m => m.MOMPath))
                    .ForMember(c => c.hdnPictureUpload, c => c.MapFrom(m => m.PicturePath))
                    .ForMember(d => d.Sno, o => o.MapFrom(s => s.MeetingNo));
                });
                var MeetingMap = Mapper.Map<List<Stage2DateOfMeeting>>(meetingDetails);

                return View(model);
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult Profile(tblRegistrationForm model, IFormCollection form)
        {
            try
            {
                model.UpdatedBy = model.UpdatedBy;
                model.UpdatedOn = DateTime.Now;
                model.IsDeleted = false;
                model.IsValidate = true;
                model.SchoolImage = _context.tblRegistrationForms.Where(p=>p.SUID == model.SUID).Select(p=>p.SchoolImage).FirstOrDefault();

                if (model.RegistrationDate == null)
                {
                    model.IsValidate = false;
                }

                if (model.SchoolName == null)
                {
                    model.IsValidate = false;
                }
                if (model.SchoolCode == null)
                {
                    model.IsValidate = false;
                }
                if (model.PrincipalName == null)
                {
                    model.IsValidate = false;
                }
                if (model.SchoolEmail == null)
                {
                    model.IsValidate = false;
                }
                if (model.SchoolEmail == null)
                {
                    model.IsValidate = false;
                }
                if (model.PhoneNoSchool == null)
                {
                    model.IsValidate = false;
                }
                if (model.Address == null)
                {
                    model.IsValidate = false;
                }
                if (model.StateID == null)
                {
                    model.IsValidate = false;
                }
                if (model.DistrictID == null)
                {
                    model.IsValidate = false;
                }
                if (model.Pincode == null)
                {
                    model.IsValidate = false;
                }
                if (model.Area == null)
                {
                    model.IsValidate = false;
                }
                if (model.AboutofSchool == null)
                {
                    model.IsValidate = false;
                }
                if (model.TotalNoofStudent == null)
                {
                    model.IsValidate = false;
                }
                if (model.TotalNoofTeachingStaff == null)
                {
                    model.IsValidate = false;
                }
                if (model.TotalNoofTeachingStaff == null)
                {
                    model.IsValidate = false;
                }
                if (model.NoofStudentMale == null)
                {
                    model.IsValidate = false;
                }
                if (model.NoofStudentFemale == null)
                {
                    model.IsValidate = false;
                }
                if (model.NoofTeachingStaffMale == null)
                {
                    model.IsValidate = false;
                }
                if (model.NoofTeachingStaffFemale == null)
                {
                    model.IsValidate = false;
                }
                if (model.NoofNonTeachingStaffMale == null)
                {
                    model.IsValidate = false;
                }
                if (model.NoofNonTeachingStaffFemale == null)
                {
                    model.IsValidate = false;
                }
                if (model.TypeOfSchool == null)
                {
                    model.IsValidate = false;
                }
                if (model.TypeOfCurriculumFollowed == null)
                {
                    model.IsValidate = false;
                }
                if (model.StartingGradeTo == null)
                {
                    model.IsValidate = false;
                }
                if (model.StartingGradeUp == null)
                {
                    model.IsValidate = false;
                }
                if (model.LanguageTeaching == null)
                {
                    model.IsValidate = false;
                }
                if (model.SchoolExistingClubs == null)
                {
                    model.IsValidate = false;
                }
                _context.tblRegistrationForms.Update(model);
                _context.SaveChanges();

                if (_context.tblSchoolRegistrationSocialMedia.Where(m => m.SUID == model.SUID && m.SocialMediaID == 1).Count() > 0)
                {
                    tblSchoolRegistrationSocialMedium schoolfacebook = _context.tblSchoolRegistrationSocialMedia.Where(m => m.SUID == model.SUID && m.SocialMediaID == 1).FirstOrDefault();
                    schoolfacebook.SocialMediaUrl = form["facebooklink"];
                    _context.tblSchoolRegistrationSocialMedia.Update(schoolfacebook);
                }
                else
                {
                    tblSchoolRegistrationSocialMedium schoolfacebook = new tblSchoolRegistrationSocialMedium();
                    schoolfacebook.SocialMediaUrl = form["facebooklink"];
                    schoolfacebook.SocialMediaID = 1;
                    schoolfacebook.SUID = model.SUID;
                    _context.tblSchoolRegistrationSocialMedia.Add(schoolfacebook);
                }
                if (_context.tblSchoolRegistrationSocialMedia.Where(m => m.SUID == model.SUID && m.SocialMediaID == 2).Count() > 0)
                {
                    tblSchoolRegistrationSocialMedium schoolinsta = _context.tblSchoolRegistrationSocialMedia.Where(m => m.SUID == model.SUID && m.SocialMediaID == 1).FirstOrDefault();
                    schoolinsta.SocialMediaUrl = form["instagramlink"];
                    _context.tblSchoolRegistrationSocialMedia.Update(schoolinsta);
                }
                else
                {
                    tblSchoolRegistrationSocialMedium schoolinsta = new tblSchoolRegistrationSocialMedium();
                    schoolinsta.SocialMediaUrl = form["instagramlink"];
                    schoolinsta.SocialMediaID = 2;
                    schoolinsta.SUID = model.SUID;
                    _context.tblSchoolRegistrationSocialMedia.Add(schoolinsta);
                }
                if (_context.tblSchoolRegistrationSocialMedia.Where(m => m.SUID == model.SUID && m.SocialMediaID == 3).Count() > 0)
                {
                    tblSchoolRegistrationSocialMedium schooltwit = _context.tblSchoolRegistrationSocialMedia.Where(m => m.SUID == model.SUID && m.SocialMediaID == 1).FirstOrDefault();
                    schooltwit.SocialMediaUrl = form["twitterlink"];
                    _context.tblSchoolRegistrationSocialMedia.Update(schooltwit);
                }
                else
                {
                    tblSchoolRegistrationSocialMedium schooltwit = new tblSchoolRegistrationSocialMedium();
                    schooltwit.SocialMediaUrl = form["twitterlink"];
                    schooltwit.SocialMediaID = 3;
                    schooltwit.SUID = model.SUID;
                    _context.tblSchoolRegistrationSocialMedia.Add(schooltwit);
                }
                _context.SaveChanges();
                return RedirectToAction("Profile", new { id = model.SUID});
            }
            catch (Exception ex)
            {
                return View();
            }
        }
        public JsonResult GetDistrict(int stateid)
        {
            try
            {
                return Json(_context.MstDistricts.Where(m => m.StateID == stateid).OrderBy(p=>p.DistrictName).ToList());
            }
            catch (Exception ex)
            {
                return Json("0");
            }
        }
        public JsonResult GetSchoolDetail(string udise)
        {
            try
            {
                return Json(_context.MstSchoolCodes.Where(m => m.SchoolCode == udise).ToList());
            }
            catch (Exception ex)
            {
                return Json("0");
            }
        }
        public JsonResult GetArea(int pincode)
        {
            try
            {
                return Json(_context.tblPincodeLatLongs.Where(m => m.Pincode == pincode).ToList());
            }
            catch (Exception ex)
            {
                return Json("0");
            }
        }
        public JsonResult GetSocialHandles(int suid)
        {
            try
            {
                return Json(_context.tblSchoolRegistrationSocialMedia.Where(m => m.SUID == suid).ToList());
            }
            catch (Exception ex)
            {
                return Json("0");
            }
        }

        [HttpPost]
        public bool IsAvailable(string Cntxtt, string FLAG)
        {
            var RegUserNAme = (from u in _context.MstUsers where u.UserName.ToUpper() == "" select new { Cntxtt }).FirstOrDefault();

            if (FLAG == "SN")
            {
                RegUserNAme = (from u in _context.tblRegistrationForms
                               where u.SchoolName.ToUpper() == Cntxtt.ToUpper()
                               select new { Cntxtt }).FirstOrDefault();
            }
            else if (FLAG == "SE")
            {
                RegUserNAme = (from u in _context.tblRegistrationForms
                               where u.SchoolEmail.ToUpper() == Cntxtt.ToUpper()
                               select new { Cntxtt }).FirstOrDefault();
            }
            else if (FLAG == "SC")
            {
                RegUserNAme = (from u in _context.tblRegistrationForms
                               where u.SchoolCode.ToUpper() == Cntxtt.ToUpper()
                               select new { Cntxtt }).FirstOrDefault();
            }

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

        public ActionResult WelcomeMSG(string ScName)
        {
            ViewBag.ScName = ScName;
            return View();
        }

        #region Column Visibility
        public ActionResult ColumnVisibility()
        {
            ViewBag.Table = new SelectList(_context.MstTables.ToList(), "TableId", "TableName");
           // ViewBag.SUID = _context.MstUsers.Where(m => m.SUID == Convert.ToUInt32(HttpContext.Session.GetString("UserID"))).FirstOrDefault().SUID;
            return View();
        }
        [HttpPost]
        public ActionResult ColumnVisibilityData(int id, int suid)
        {
            SqlParameter[] s = new SqlParameter[] {
            new SqlParameter("Filter1",id),
            new SqlParameter("Filter2",suid),
            };
            DataTable dt = CommonFN.Procedure_Query_ToDataTable(_context, "USP_GetTableColumnName", CommandType.StoredProcedure, s);
            return Json(JsonConvert.SerializeObject(dt));
        }
        [HttpPost]
        public ActionResult ColumnVisibilitySave(ColumnTable model)
        {
            var suid = model.mstTableColumnRights.FirstOrDefault().SUID;

            List<MstTableColumnRight> dbmodel = _context.MstTableColumnRights.Where(m => m.SUID == suid).ToList();
            if (dbmodel != null && dbmodel.Count > 0)
            {
                _context.MstTableColumnRights.RemoveRange(dbmodel);
                _context.SaveChanges();
            }


            _context.MstTableColumnRights.AddRange(model.mstTableColumnRights);
            _context.SaveChanges();

            return Json("1");
        }

        public class ColumnTable
        {
            public List<MstTableColumnRight> mstTableColumnRights { get; set; } = new List<MstTableColumnRight>();
        }
        #endregion


        #endregion

        [HttpPost]
        public ActionResult UploadSchoolPicture(IFormFile MyUploaderSC)
        {
            _logger.LogInformation($"LoginController/SchoolImageLogo");
            try
            {
                if (MyUploaderSC != null)
                {
                    string fileName = string.Empty; string filePath = string.Empty;
                    string fname = "";

                    var ext = MyUploaderSC.ContentType.Split('/')[1];
                    fname = NewGuid() + "." + ext;
                    fileName = fname;
                    fname = Path.Combine("wwwroot/images/SchoolImageLogo", fname);
                    if (System.IO.File.Exists(fname))
                    {
                        System.IO.File.Delete(fname);
                    }
                    //string filePath = Path.Combine(uploadsFolder, MyUploader.FileName);
                    using (var fileStream = new FileStream(fname, FileMode.Create))
                    {
                        MyUploaderSC.CopyTo(fileStream);
                    }
                    tblRegistrationForm tblres = _context.tblRegistrationForms.Where(p => p.SUID == Convert.ToInt32(HttpContext.Session.GetString("Suid"))).SingleOrDefault();
                    tblres.SchoolImage = fileName;
                    _context.Update(tblres);
                    _context.SaveChanges();

                    // Returns message that successfully uploaded  
                    return Json(new { part = 1, FileName = fileName });
                }
                else
                {
                    return Json("No files selected.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Message-" + ex.Message + " StackTrace-" + ex.StackTrace + " DatetimeStamp-" + DateTime.Now);
                throw;
            }
        }
        public static string NewGuid()
        {
            return Guid.NewGuid().ToString();
        }
        #region  ****** Forget Password
        public ActionResult ForgetPassword_SendEmail(string EmailID)
        {
            try
            {
                var reslut = _context.MstUsers.Where(x => x.UserName == EmailID).ToList();
                if (reslut.Count() > 0)
                {
                    string pwd = CommonFN.Generate_RandomString(8, false);

                    string urlpth = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}" + "/Login/PasswordChangeForcely";
                    string strurl = "u%d=" + reslut[0].UserID + "&e%m=" + reslut[0].UserEmailID + "&m%o=" + reslut[0].Mobile + "&p%d=" + reslut[0].Password;
                    MstUser _User = _context.MstUsers.Where(x => x.UserName == EmailID).FirstOrDefault();
                    _User.Password = CommonFN.CreatePasswordHash(pwd);
                    _User.pwdLinkExpTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    _context.SaveChanges();
                    string ppp = CommonFN.Encrypt(strurl);
                    strurl = HttpUtility.UrlEncode(ppp);
                    urlpth = urlpth + "?" + strurl;

                    string body = "Dear " + reslut[0].UserName + ",<br><br> You are request to changed the password <b>Plan International School</b>. Please click on URL to change your password below- <br><br> Plan International School Portal URL :  " + urlpth + "";
                    CommonFN.Email_Send("rchjh6150@gmail.com", "Plan International School", reslut[0].UserEmailID, "ChangePassword", body, "", "", "");
                    return Json("1");
                }
                else
                {
                    return View("Login");

                }
            }
            catch (Exception ex)
            {

                return Json(ex.Message);
            }
        }
        //[AllowAnonymous]
        [HttpGet]
        public ActionResult PasswordChangeForcely()
        {
            try
            {
                string[] urls = ($"{this.Request.HttpContext.Request.QueryString}").Split('?');
                if (urls.Length > 1)
                {
                    string url = HttpUtility.UrlDecode(urls[1]);
                    string Strs = CommonFN.Decrypt(url.Replace("?", ""));
                    string[] pram = Strs.Split('&');
                    TempData["u_d"] = pram[0].Substring(4, pram[0].Length - 4);
                    TempData["e_m"] = pram[1].Substring(4, pram[1].Length - 4);
                    TempData["m_o"] = pram[2].Substring(4, pram[2].Length - 4);
                    TempData["p_d"] = pram[3].Substring(4, pram[3].Length - 4);
                    int re = 1;
                    //DB_Layer.Users.Forgot_CheckRequest(Convert.ToString(TempData["u_d"]), Convert.ToString(TempData["e_m"]), Convert.ToString(TempData["m_o"]), Convert.ToString(TempData["p_d"]));
                    MstUser _mstUser = new MstUser();
                    _mstUser.UserID = Convert.ToInt32(TempData["u_d"]);
                    _mstUser.UserEmailID = Convert.ToString(TempData["e_m"]);
                    _mstUser.Mobile = Convert.ToString(TempData["m_o"]);
                    //_mstUser.Password = Convert.ToString(TempData["p_d"]);

                    if (re > 0)
                    {
                        TempData.Keep();
                        return View(_mstUser);
                    }
                    else
                    {
                        return RedirectToAction("Error", "Login");
                    }
                }
                else
                {
                    return RedirectToAction("Error", "Login");
                }
                // return View();
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpPost]
        public ActionResult PasswordChangeForcely([FromBody] MstUser f)
        {
            if (f.UserID != 0 && f.UserEmailID != null && f.Mobile != null)
            {
                MstUser User = _context.MstUsers.Where(x => x.UserID == f.UserID).FirstOrDefault();
                var chktime = Convert.ToDateTime(User.pwdLinkExpTime).ToLongTimeString();
                TimeSpan d = (DateTime.Now).Subtract(Convert.ToDateTime(chktime));
                if (d.Minutes <= 10)
                {
                    User.Password = CommonFN.CreatePasswordHash(f.Password);
                    _context.SaveChanges();
                    ViewBag.Message = "Password changed successfully";
                    return Json("Password changed successfully");
                }
                else
                {
                    ViewBag.Message = "Link Expired!! Please reset Password Again";
                    return Json("Link Expired!! Please reset Password Again");
                }
            }
            else
            {
                TempData["Msg"] = "Invalid";
                return View();
            }
        }
        #endregion

        public IActionResult UpdatePassword()
        {
            ViewData["Users"] = new SelectList(_context.MstUsers, "UserId", "UserName");
            ViewBag.UserId = Convert.ToInt32(HttpContext.Session.GetString("UserID"));
            ViewBag.RoleId = Convert.ToInt32(HttpContext.Session.GetString("UserRoleID"));
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> UpdatePassword(UpdatePass model)
        {


            string rt = "";
            int id = Convert.ToInt32(model.userId);

            var user = _context.MstUsers.SingleOrDefault(x => x.UserID == id);

            //check if password is correctpassword
            var passwordHashed = CommonFN.CreatePasswordHash(model.OldPassword);
            if (user.Password != passwordHashed)
            { rt = "2"; }
            else
            {
                var mstUser = await _context.MstUsers.SingleOrDefaultAsync(m => m.UserID == id);
                mstUser.Password = CreateUserNameHash(model.password);
                mstUser.UpdatedBy = id;
                mstUser.UpdatedOn = DateTime.Now;
                _context.MstUsers.Update(mstUser);
                await _context.SaveChangesAsync();
                rt = "1";
            }



            return Json(rt);
        }

        public class UpdatePass
        {
            public int? userId { get; set; }
            public String password { get; set; }
            public String OldPassword { get; set; }
        }


        #region Pop Up Committe Member Details

        [HttpGet]
        //public IActionResult GetCommitteMemberDetails()
        //{
        //    _logger.LogInformation($"Login/GetCommitteMemberDetails/");
        //    try
        //    {
        //        var SUID = Convert.ToInt32(HttpContext.Session.GetString("Suid"));
        //        var commiteeDetails = _context.Stage1STP2CommitteeDetails.Where(x => x.SUID == SUID).ToList();
        //        Mapper.Initialize(cfg =>
        //        {
        //            cfg.CreateMap<Stage1STP2CommitteeDetail, CommitteMember>()
        //            .ForMember(c => c.Id, c => c.MapFrom(m => m.ID))
        //            .ForMember(c => c.Name, c => c.MapFrom(m => m.Name))
        //            .ForMember(c => c.CategoryId, c => c.MapFrom(m => m.CategoryId))
        //            .ForMember(c => c.RoleResponsibilities, c => c.MapFrom(m => m.RolesAndResponsibility))
        //            .ForMember(c => c.EmailId, c => c.MapFrom(m => m.EmailId));
        //        });
        //        var CommitteMap = Mapper.Map<List<CommitteMember>>(commiteeDetails);
        //        CommitteMap.ForEach(x => { x.Category = GetCategoryName(x.CategoryId); });
        //        return PartialView("_CommitteGridViewDetails", CommitteMap);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Message-" + ex.Message + " StackTrace-" + ex.StackTrace + " DatetimeStamp-" + DateTime.Now);
        //        throw;
        //    }
        //}


        [HttpGet]
        //public IActionResult AddCommitteDetails()
        //{
        //    _logger.LogInformation($"Stage1STP2/AddCommitteDetails");
        //    try
        //    {
        //        CommitteMember committeMember = new CommitteMember();
        //        var category = _context.CommitteCategories.Where(x => x.IsDeleted == false).ToList();
        //        Mapper.Initialize(cfg =>
        //        {
        //            cfg.CreateMap<CommitteCategory, SelectListModel>()
        //            .ForMember(c => c.id, c => c.MapFrom(m => m.Id))
        //            .ForMember(d => d.value, o => o.MapFrom(s => s.Value));
        //        });
        //        var categoryMap = Mapper.Map<List<SelectListModel>>(category);
        //        committeMember.CategoryList = categoryMap;
        //        return PartialView("_AddCommitteDetails", committeMember);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Message-" + ex.Message + " StackTrace-" + ex.StackTrace + " DatetimeStamp-" + DateTime.Now);
        //        throw;
        //    }
        //}

        [HttpPost]
        public IActionResult PostCommitteMemberDetails(CommitteMember committeMember)
        {
            _logger.LogInformation($"Stage1STP2/PostCommitteMemberDetails/{committeMember}");
            try
            {
                if (ModelState.IsValid)
                {
                    var SUID = Convert.ToInt32(HttpContext.Session.GetString("Suid"));
                    Stage1STP2CommitteeDetail stage1Stp2CommitteeDetail = new Stage1STP2CommitteeDetail();
                    stage1Stp2CommitteeDetail.SUID = SUID;
                    stage1Stp2CommitteeDetail.Name = committeMember.Name;
                    stage1Stp2CommitteeDetail.CategoryId = committeMember.CategoryId;
                    stage1Stp2CommitteeDetail.RolesAndResponsibility = committeMember.RoleResponsibilities;
                    stage1Stp2CommitteeDetail.EmailId = committeMember.EmailId;
                    _context.Stage1STP2CommitteeDetails.Add(stage1Stp2CommitteeDetail);
                    _context.SaveChanges();

                    //===================Add Contact to Admin==========================//

                    tblContactDetail TblContDetail = new tblContactDetail();
                    TblContDetail.SUID = SUID;
                    TblContDetail.Name = committeMember.Name;
                    TblContDetail.CategoryId = committeMember.CategoryId;
                    TblContDetail.RolesAndResponsibility = committeMember.RoleResponsibilities;
                    TblContDetail.EmailId = committeMember.EmailId;
                    TblContDetail.MemberTypeID = 1;
                    _context.Add(TblContDetail);
                    _context.SaveChanges();

                    //=============================================//

                    var commiteeDetails = _context.Stage1STP2CommitteeDetails.Where(x => x.SUID == SUID).ToList();
                    Mapper.Initialize(cfg =>
                    {
                        cfg.CreateMap<Stage1STP2CommitteeDetail, CommitteMember>()
                        .ForMember(c => c.Id, c => c.MapFrom(m => m.ID))
                        .ForMember(c => c.Name, c => c.MapFrom(m => m.Name))
                        .ForMember(c => c.CategoryId, c => c.MapFrom(m => m.CategoryId))
                        .ForMember(c => c.RoleResponsibilities, c => c.MapFrom(m => m.RolesAndResponsibility))
                        .ForMember(c => c.EmailId, c => c.MapFrom(m => m.EmailId));
                    });
                    var CommitteMap = Mapper.Map<List<CommitteMember>>(commiteeDetails);
                    CommitteMap.ForEach(x => { x.Category = GetCategoryName(x.CategoryId); });

                    var CommittePartialView = RenderViewToStringAsync("/Views/Stage1STP2/_CommitteGridViewDetails.cshtml", CommitteMap);
                    return Json(new { part = "1", Msg = "Data Saved Successfully.", htmlData = CommittePartialView });
                }
                else
                {
                    var category = _context.CommitteCategories.Where(x => x.IsDeleted == false).ToList();
                    Mapper.Initialize(cfg =>
                    {
                        cfg.CreateMap<CommitteCategory, SelectListModel>()
                        .ForMember(c => c.id, c => c.MapFrom(m => m.ID))
                        .ForMember(d => d.value, o => o.MapFrom(s => s.Value));
                    });
                    var categoryMap = Mapper.Map<List<SelectListModel>>(category);
                    committeMember.CategoryList = categoryMap;
                    var POAPartialView = RenderViewToStringAsync("/Views/Stage1STP2/_AddCommitteDetails.cshtml", committeMember);
                    return Json(new { part = "2", htmlData = POAPartialView });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Message-" + ex.Message + " StackTrace-" + ex.StackTrace + " DatetimeStamp-" + DateTime.Now);
                throw;
            }
        }

        public string GetCategoryName(int categoryId)
        {
            _logger.LogInformation($"Stage1STP2/GetCategoryName/{categoryId}");
            try
            {
                var result = "";
                result = _context.CommitteCategories.Where(x => x.ID == categoryId).FirstOrDefault().Value;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Message-" + ex.Message + " StackTrace-" + ex.StackTrace + " DatetimeStamp-" + DateTime.Now);
                throw;
            }
        }

        [HttpGet]
        public IActionResult RemoveCommitteMemberDetails(int Id)
        {
            try
            {
                var SUID = Convert.ToInt32(HttpContext.Session.GetString("Suid"));
                var findRecord = _context.Stage1STP2CommitteeDetails.Where(x => x.ID == Id && x.SUID == SUID).FirstOrDefault();
                if (findRecord != null)
                {
                    _context.Stage1STP2CommitteeDetails.Remove(findRecord);
                    _context.SaveChanges();
                }

                var commiteeDetails = _context.Stage1STP2CommitteeDetails.Where(x => x.SUID == SUID).ToList();
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<Stage1STP2CommitteeDetail, CommitteMember>()
                    .ForMember(c => c.Id, c => c.MapFrom(m => m.ID))
                    .ForMember(c => c.Name, c => c.MapFrom(m => m.Name))
                    .ForMember(c => c.CategoryId, c => c.MapFrom(m => m.CategoryId))
                    .ForMember(c => c.RoleResponsibilities, c => c.MapFrom(m => m.RolesAndResponsibility))
                    .ForMember(c => c.EmailId, c => c.MapFrom(m => m.EmailId));
                });
                var CommitteMap = Mapper.Map<List<CommitteMember>>(commiteeDetails);
                CommitteMap.ForEach(x => { x.Category = GetCategoryName(x.CategoryId); });
                return PartialView("_CommitteGridViewDetails", CommitteMap);
            }
            catch (Exception ex)
            {
                _logger.LogError("Message-" + ex.Message + " StackTrace-" + ex.StackTrace + " DatetimeStamp-" + DateTime.Now);
                throw;
            }
        }

        [HttpGet]
        public IActionResult EditCommitteMemberDetails(int Id)
        {
            _logger.LogInformation($"Stage1STP2/EditCommitteMemberDetails/{Id}");
            try
            {
                CommitteMember committeMember = new CommitteMember();
                var findCommitteDetails = _context.Stage1STP2CommitteeDetails.Where(x => x.ID == Id).FirstOrDefault();
                if (findCommitteDetails != null)
                {
                    committeMember.Id = findCommitteDetails.ID;
                    committeMember.Name = findCommitteDetails.Name;
                    committeMember.CategoryId = findCommitteDetails.CategoryId.Value;
                    committeMember.RoleResponsibilities = findCommitteDetails.RolesAndResponsibility;
                    committeMember.EmailId = findCommitteDetails.EmailId;
                }

                var category = _context.CommitteCategories.Where(x => x.IsDeleted == false).ToList();
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<CommitteCategory, SelectListModel>()
                    .ForMember(c => c.id, c => c.MapFrom(m => m.ID))
                    .ForMember(d => d.value, o => o.MapFrom(s => s.Value));
                });
                var categoryMap = Mapper.Map<List<SelectListModel>>(category);
                committeMember.CategoryList = categoryMap;
                return PartialView("_EditCommitteMemberDetails", committeMember);
            }
            catch (Exception ex)
            {
                _logger.LogError("Message-" + ex.Message + " StackTrace-" + ex.StackTrace + " DatetimeStamp-" + DateTime.Now);
                throw;
            }
        }

        [HttpPost]
        public IActionResult EditCommitteMemberDetails(CommitteMember committeMember)
        {
            _logger.LogInformation($"Stage1STP2/EditCommitteMemberDetails/{committeMember}");
            try
            {
                if (ModelState.IsValid)
                {
                    var SUID = Convert.ToInt32(HttpContext.Session.GetString("Suid"));
                    var findCommitteMemberDetails = _context.Stage1STP2CommitteeDetails.Where(x => x.ID == committeMember.Id).FirstOrDefault();

                    if (findCommitteMemberDetails != null)
                    {
                        findCommitteMemberDetails.ID = committeMember.Id;
                        findCommitteMemberDetails.Name = committeMember.Name;
                        findCommitteMemberDetails.CategoryId = committeMember.CategoryId;
                        findCommitteMemberDetails.RolesAndResponsibility = committeMember.RoleResponsibilities;
                        findCommitteMemberDetails.EmailId = committeMember.EmailId;
                        _context.Stage1STP2CommitteeDetails.Update(findCommitteMemberDetails);
                        _context.SaveChanges();
                    }

                    var commiteeDetails = _context.Stage1STP2CommitteeDetails.Where(x => x.SUID == SUID).ToList();
                    Mapper.Initialize(cfg =>
                    {
                        cfg.CreateMap<Stage1STP2CommitteeDetail, CommitteMember>()
                        .ForMember(c => c.Id, c => c.MapFrom(m => m.ID))
                        .ForMember(c => c.Name, c => c.MapFrom(m => m.Name))
                        .ForMember(c => c.CategoryId, c => c.MapFrom(m => m.CategoryId))
                        .ForMember(c => c.RoleResponsibilities, c => c.MapFrom(m => m.RolesAndResponsibility))
                        .ForMember(c => c.EmailId, c => c.MapFrom(m => m.EmailId));
                    });
                    var CommitteMap = Mapper.Map<List<CommitteMember>>(commiteeDetails);
                    CommitteMap.ForEach(x => { x.Category = GetCategoryName(x.CategoryId); });
                    var CommittePartialView = RenderViewToStringAsync("/Views/Stage1STP2/_CommitteGridViewDetails.cshtml", CommitteMap);
                    return Json(new { part = "1", Msg = "Data Updated Successfully.", htmlData = CommittePartialView });
                }
                else
                {
                    var category = _context.CommitteCategories.Where(x => x.IsDeleted == false).ToList();
                    Mapper.Initialize(cfg =>
                    {
                        cfg.CreateMap<CommitteCategory, SelectListModel>()
                        .ForMember(c => c.id, c => c.MapFrom(m => m.ID))
                        .ForMember(d => d.value, o => o.MapFrom(s => s.Value));
                    });
                    var categoryMap = Mapper.Map<List<SelectListModel>>(category);
                    committeMember.CategoryList = categoryMap;
                    var MeetingPartialView = RenderViewToStringAsync("/Views/Stage1STP2/_EditCommitteMemberDetails.cshtml", committeMember);
                    return Json(new { part = "2", htmlData = MeetingPartialView });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Message-" + ex.Message + " StackTrace-" + ex.StackTrace + " DatetimeStamp-" + DateTime.Now);
                throw;
            }
        }

        public async Task<string> RenderViewToStringAsync<TModel>(string viewNamePath, TModel model)
        {
            Controller controller = this;
            if (string.IsNullOrEmpty(viewNamePath))
                viewNamePath = controller.ControllerContext.ActionDescriptor.ActionName;

            controller.ViewData.Model = model;

            using (StringWriter writer = new StringWriter())
            {
                try
                {
                    IViewEngine viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;

                    ViewEngineResult viewResult = null;

                    if (viewNamePath.EndsWith(".cshtml"))
                        viewResult = viewEngine.GetView(viewNamePath, viewNamePath, false);
                    else
                        viewResult = viewEngine.FindView(controller.ControllerContext, viewNamePath, false);

                    if (!viewResult.Success)
                        return $"A view with the name '{viewNamePath}' could not be found";

                    ViewContext viewContext = new ViewContext(
                        controller.ControllerContext,
                        viewResult.View,
                        controller.ViewData,
                        controller.TempData,
                        writer,
                        new HtmlHelperOptions()
                    );

                    await viewResult.View.RenderAsync(viewContext);

                    return writer.GetStringBuilder().ToString();
                }
                catch (Exception exc)
                {
                    return $"Failed - {exc.Message}";
                }
            }
        }
        #endregion
        //#region Pop Up Committe Member Details

        //[HttpGet]
        //public IActionResult GetCommitteMemberDetails()
        //{
        //    _logger.LogInformation($"Login/GetCommitteMemberDetails/");
        //    try
        //    {
        //        var SUID = Convert.ToInt32(HttpContext.Session.GetString("Suid"));
        //        var commiteeDetails = _context.Stage1Stp2committeeDetails.Where(x => x.Suid == SUID).ToList();
        //        Mapper.Initialize(cfg =>
        //        {
        //            cfg.CreateMap<Stage1Stp2committeeDetail, CommitteMember>()
        //            .ForMember(c => c.Id, c => c.MapFrom(m => m.Id))
        //            .ForMember(c => c.Name, c => c.MapFrom(m => m.Name))
        //            .ForMember(c => c.CategoryId, c => c.MapFrom(m => m.CategoryId))
        //            .ForMember(c => c.RoleResponsibilities, c => c.MapFrom(m => m.RolesAndResponsibility))
        //            .ForMember(c => c.EmailId, c => c.MapFrom(m => m.EmailId));
        //        });
        //        var CommitteMap = Mapper.Map<List<CommitteMember>>(commiteeDetails);
        //        CommitteMap.ForEach(x => { x.Category = GetCategoryName(x.CategoryId); });
        //        return PartialView("_CommitteGridViewDetails", CommitteMap);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Message-" + ex.Message + " StackTrace-" + ex.StackTrace + " DatetimeStamp-" + DateTime.Now);
        //        throw;
        //    }
        //}


        //[HttpGet]
        //public IActionResult AddCommitteDetails()
        //{
        //    _logger.LogInformation($"Stage1STP2/AddCommitteDetails");
        //    try
        //    {
        //        CommitteMember committeMember = new CommitteMember();
        //        var category = _context.CommitteCategories.Where(x => x.IsDeleted == false).ToList();
        //        Mapper.Initialize(cfg =>
        //        {
        //            cfg.CreateMap<CommitteCategory, SelectListModel>()
        //            .ForMember(c => c.id, c => c.MapFrom(m => m.Id))
        //            .ForMember(d => d.value, o => o.MapFrom(s => s.Value));
        //        });
        //        var categoryMap = Mapper.Map<List<SelectListModel>>(category);
        //        committeMember.CategoryList = categoryMap;
        //        return PartialView("_AddCommitteDetails", committeMember);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Message-" + ex.Message + " StackTrace-" + ex.StackTrace + " DatetimeStamp-" + DateTime.Now);
        //        throw;
        //    }
        //}

        //[HttpPost]
        //public IActionResult PostCommitteMemberDetails(CommitteMember committeMember)
        //{
        //    _logger.LogInformation($"Stage1STP2/PostCommitteMemberDetails/{committeMember}");
        //    try
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            var SUID = Convert.ToInt32(HttpContext.Session.GetString("Suid"));
        //            Stage1Stp2committeeDetail stage1Stp2CommitteeDetail = new Stage1Stp2committeeDetail();
        //            stage1Stp2CommitteeDetail.Suid = SUID;
        //            stage1Stp2CommitteeDetail.Name = committeMember.Name;
        //            stage1Stp2CommitteeDetail.CategoryId = committeMember.CategoryId;
        //            stage1Stp2CommitteeDetail.RolesAndResponsibility = committeMember.RoleResponsibilities;
        //            stage1Stp2CommitteeDetail.EmailId = committeMember.EmailId;
        //            _context.Stage1Stp2committeeDetails.Add(stage1Stp2CommitteeDetail);
        //            _context.SaveChanges();

        //            //===================Add Contact to Admin==========================//

        //            TblContactDetail TblContDetail = new TblContactDetail();
        //            TblContDetail.Suid = SUID;
        //            TblContDetail.Name = committeMember.Name;
        //            TblContDetail.CategoryId = committeMember.CategoryId;
        //            TblContDetail.RolesAndResponsibility = committeMember.RoleResponsibilities;
        //            TblContDetail.EmailId = committeMember.EmailId;
        //            TblContDetail.MemberTypeId = 1;
        //            _context.Add(TblContDetail);
        //            _context.SaveChanges();

        //            //=============================================//

        //            var commiteeDetails = _context.Stage1Stp2committeeDetails.Where(x => x.Suid == SUID && x.IsDeleted == false).ToList();
        //            Mapper.Initialize(cfg =>
        //            {
        //                cfg.CreateMap<Stage1Stp2committeeDetail, CommitteMember>()
        //                .ForMember(c => c.Id, c => c.MapFrom(m => m.Id))
        //                .ForMember(c => c.Name, c => c.MapFrom(m => m.Name))
        //                .ForMember(c => c.CategoryId, c => c.MapFrom(m => m.CategoryId))
        //                .ForMember(c => c.RoleResponsibilities, c => c.MapFrom(m => m.RolesAndResponsibility))
        //                .ForMember(c => c.EmailId, c => c.MapFrom(m => m.EmailId));
        //            });
        //            var CommitteMap = Mapper.Map<List<CommitteMember>>(commiteeDetails);
        //            CommitteMap.ForEach(x => { x.Category = GetCategoryName(x.CategoryId); });

        //            var CommittePartialView = RenderViewToStringAsync("/Views/Stage1STP2/_CommitteGridViewDetails.cshtml", CommitteMap);
        //            return Json(new { part = "1", msg = "Data Saved Successfully.", htmlData = CommittePartialView });
        //        }
        //        else
        //        {
        //            var category = _context.CommitteCategories.Where(x => x.IsDeleted == false).ToList();
        //            Mapper.Initialize(cfg =>
        //            {
        //                cfg.CreateMap<CommitteCategory, SelectListModel>()
        //                .ForMember(c => c.id, c => c.MapFrom(m => m.Id))
        //                .ForMember(d => d.value, o => o.MapFrom(s => s.Value));
        //            });
        //            var categoryMap = Mapper.Map<List<SelectListModel>>(category);
        //            committeMember.CategoryList = categoryMap;
        //            var POAPartialView = RenderViewToStringAsync("/Views/Stage1STP2/_AddCommitteDetails.cshtml", committeMember);
        //            return Json(new { part = "2", htmlData = POAPartialView });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Message-" + ex.Message + " StackTrace-" + ex.StackTrace + " DatetimeStamp-" + DateTime.Now);
        //        throw;
        //    }
        //}

        //public string GetCategoryName(int categoryId)
        //{
        //    _logger.LogInformation($"Stage1STP2/GetCategoryName/{categoryId}");
        //    try
        //    {
        //        var result = "";
        //        result = _context.CommitteCategories.Where(x => x.Id == categoryId).FirstOrDefault().Value;
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Message-" + ex.Message + " StackTrace-" + ex.StackTrace + " DatetimeStamp-" + DateTime.Now);
        //        throw;
        //    }
        //}

        //[HttpGet]
        //public IActionResult RemoveCommitteMemberDetails(int Id)
        //{
        //    try
        //    {
        //        var SUID = Convert.ToInt32(HttpContext.Session.GetString("Suid"));
        //        var findRecord = _context.Stage1Stp2committeeDetails.Where(x => x.Id == Id && x.Suid == SUID && x.IsDeleted == false).FirstOrDefault();
        //        if (findRecord != null)
        //        {
        //            //_context.Stage1Stp2committeeDetails.Remove(findRecord);
        //            findRecord.IsDeleted = true;
        //            _context.Stage1Stp2committeeDetails.Update(findRecord);
        //            _context.SaveChanges();
        //            //_context.SaveChanges();
        //        }

        //        var commiteeDetails = _context.Stage1Stp2committeeDetails.Where(x => x.Suid == SUID && x.IsDeleted == false).ToList();
        //        Mapper.Initialize(cfg =>
        //        {
        //            cfg.CreateMap<Stage1Stp2committeeDetail, CommitteMember>()
        //            .ForMember(c => c.Id, c => c.MapFrom(m => m.Id))
        //            .ForMember(c => c.Name, c => c.MapFrom(m => m.Name))
        //            .ForMember(c => c.CategoryId, c => c.MapFrom(m => m.CategoryId))
        //            .ForMember(c => c.RoleResponsibilities, c => c.MapFrom(m => m.RolesAndResponsibility))
        //            .ForMember(c => c.EmailId, c => c.MapFrom(m => m.EmailId));
        //        });
        //        var CommitteMap = Mapper.Map<List<CommitteMember>>(commiteeDetails);
        //        CommitteMap.ForEach(x => { x.Category = GetCategoryName(x.CategoryId); });
        //        return PartialView("_CommitteGridViewDetails", CommitteMap);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Message-" + ex.Message + " StackTrace-" + ex.StackTrace + " DatetimeStamp-" + DateTime.Now);
        //        throw;
        //    }
        //}

        //[HttpGet]
        //public IActionResult EditCommitteMemberDetails(int Id)
        //{
        //    _logger.LogInformation($"Stage1STP2/EditCommitteMemberDetails/{Id}");
        //    try
        //    {
        //        CommitteMember committeMember = new CommitteMember();
        //        var findCommitteDetails = _context.Stage1Stp2committeeDetails.Where(x => x.Id == Id && x.IsDeleted == false).FirstOrDefault();
        //        if (findCommitteDetails != null)
        //        {
        //            committeMember.Id = findCommitteDetails.Id;
        //            committeMember.Name = findCommitteDetails.Name;
        //            committeMember.CategoryId = findCommitteDetails.CategoryId.Value;
        //            committeMember.RoleResponsibilities = findCommitteDetails.RolesAndResponsibility;
        //            committeMember.EmailId = findCommitteDetails.EmailId;
        //        }

        //        var category = _context.CommitteCategories.Where(x => x.IsDeleted == false).ToList();
        //        Mapper.Initialize(cfg =>
        //        {
        //            cfg.CreateMap<CommitteCategory, SelectListModel>()
        //            .ForMember(c => c.id, c => c.MapFrom(m => m.Id))
        //            .ForMember(d => d.value, o => o.MapFrom(s => s.Value));
        //        });
        //        var categoryMap = Mapper.Map<List<SelectListModel>>(category);
        //        committeMember.CategoryList = categoryMap;
        //        return PartialView("_EditCommitteMemberDetails", committeMember);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Message-" + ex.Message + " StackTrace-" + ex.StackTrace + " DatetimeStamp-" + DateTime.Now);
        //        throw;
        //    }
        //}

        //[HttpPost]
        //public IActionResult EditCommitteMemberDetails(CommitteMember committeMember)
        //{
        //    _logger.LogInformation($"Stage1STP2/EditCommitteMemberDetails/{committeMember}");
        //    try
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            var SUID = Convert.ToInt32(HttpContext.Session.GetString("Suid"));
        //            var findCommitteMemberDetails = _context.Stage1Stp2committeeDetails.Where(x => x.Id == committeMember.Id && x.IsDeleted == false).FirstOrDefault();

        //            if (findCommitteMemberDetails != null)
        //            {
        //                findCommitteMemberDetails.Id = committeMember.Id;
        //                findCommitteMemberDetails.Name = committeMember.Name;
        //                findCommitteMemberDetails.CategoryId = committeMember.CategoryId;
        //                findCommitteMemberDetails.RolesAndResponsibility = committeMember.RoleResponsibilities;
        //                findCommitteMemberDetails.EmailId = committeMember.EmailId;
        //                _context.Stage1Stp2committeeDetails.Update(findCommitteMemberDetails);
        //                _context.SaveChanges();
        //            }

        //            var commiteeDetails = _context.Stage1Stp2committeeDetails.Where(x => x.Suid == SUID && x.IsDeleted == false).ToList();
        //            Mapper.Initialize(cfg =>
        //            {
        //                cfg.CreateMap<Stage1Stp2committeeDetail, CommitteMember>()
        //                .ForMember(c => c.Id, c => c.MapFrom(m => m.Id))
        //                .ForMember(c => c.Name, c => c.MapFrom(m => m.Name))
        //                .ForMember(c => c.CategoryId, c => c.MapFrom(m => m.CategoryId))
        //                .ForMember(c => c.RoleResponsibilities, c => c.MapFrom(m => m.RolesAndResponsibility))
        //                .ForMember(c => c.EmailId, c => c.MapFrom(m => m.EmailId));
        //            });
        //            var CommitteMap = Mapper.Map<List<CommitteMember>>(commiteeDetails);
        //            CommitteMap.ForEach(x => { x.Category = GetCategoryName(x.CategoryId); });
        //            var CommittePartialView = RenderViewToStringAsync("/Views/Stage1STP2/_CommitteGridViewDetails.cshtml", CommitteMap);
        //            return Json(new { part = "1", msg = "Data Updated Successfully.", htmlData = CommittePartialView });
        //        }
        //        else
        //        {
        //            var category = _context.CommitteCategories.Where(x => x.IsDeleted == false).ToList();
        //            Mapper.Initialize(cfg =>
        //            {
        //                cfg.CreateMap<CommitteCategory, SelectListModel>()
        //                .ForMember(c => c.id, c => c.MapFrom(m => m.Id))
        //                .ForMember(d => d.value, o => o.MapFrom(s => s.Value));
        //            });
        //            var categoryMap = Mapper.Map<List<SelectListModel>>(category);
        //            committeMember.CategoryList = categoryMap;
        //            var MeetingPartialView = RenderViewToStringAsync("/Views/Stage1STP2/_EditCommitteMemberDetails.cshtml", committeMember);
        //            return Json(new { part = "2", htmlData = MeetingPartialView });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Message-" + ex.Message + " StackTrace-" + ex.StackTrace + " DatetimeStamp-" + DateTime.Now);
        //        throw;
        //    }
        //}
        //#endregion

        #region meeting

        [HttpGet]
        public IActionResult GetMeetingDetailsDetails()
        {
            _logger.LogInformation($"Login/GetMeetingDetailsDetails/");
            try
            {
                var SUID = Convert.ToInt32(HttpContext.Session.GetString("Suid"));
                var meetingDetails = _context.Stage1STP2MeetingDetails.Where(x => x.SUID == SUID).ToList();
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<Stage1STP2MeetingDetail, Stage2DateOfMeeting>()
                    .ForMember(c => c.ID, c => c.MapFrom(m => m.ID))
                    .ForMember(c => c.MeetingDate, c => c.MapFrom(m => m.MeetingDate))
                    .ForMember(c => c.StartingTime, c => c.MapFrom(m => m.StartingTime))
                    .ForMember(c => c.EndTime, c => c.MapFrom(m => m.EndTime))
                    .ForMember(c => c.NumberOfPeople, c => c.MapFrom(m => m.NumberOfPeople))
                    .ForMember(c => c.NoofStudent, c => c.MapFrom(m => m.NoofStudent))
                    .ForMember(c => c.NoofTeacher, c => c.MapFrom(m => m.NoofTeacher))
                    .ForMember(c => c.NoofOther, c => c.MapFrom(m => m.NoofOther))
                    .ForMember(c => c.PurposeId, c => c.MapFrom(m => m.PurposeID))
                    .ForMember(c => c.PurposeName, c => c.MapFrom(m => GetPurposeName(m.PurposeID)))
                    .ForMember(c => c.hdnMeetingUpload, c => c.MapFrom(m => m.MOMPath))
                    .ForMember(c => c.hdnPictureUpload, c => c.MapFrom(m => m.PicturePath))
                    .ForMember(d => d.Sno, o => o.MapFrom(s => s.MeetingNo));
                });
                var MeetingMap = Mapper.Map<List<Stage2DateOfMeeting>>(meetingDetails);
                return PartialView("_MeetingGridView", MeetingMap);
            }
            catch (Exception ex)
            {
                _logger.LogError("Message-" + ex.Message + " StackTrace-" + ex.StackTrace + " DatetimeStamp-" + DateTime.Now);
                throw;
            }
        }


        [HttpGet]
        public IActionResult AddMeetingDetails()
        {
            _logger.LogInformation($"Stage1STP2/AddMeetingDetails");
            try
            {
                Stage2DateOfMeeting stage2DateOfMeeting = new Stage2DateOfMeeting();

                var PurposeDetails = _context.MstPurposes.Where(x => x.Isdeleted == false).OrderBy(p => p.Seq).ToList();
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<MstPurpose, SelectListModel>()
                    .ForMember(c => c.id, c => c.MapFrom(m => m.PurposeID))
                    .ForMember(d => d.value, o => o.MapFrom(s => s.Purpose));
                });
                var PurposeMap = Mapper.Map<List<SelectListModel>>(PurposeDetails);
                stage2DateOfMeeting.Purposelst = PurposeMap;

                stage2DateOfMeeting.LSTRegistrationForm = _context.tblRegistrationForms.Where(p => p.SUID == Convert.ToInt32(HttpContext.Session.GetString("Suid"))).ToList();


                return PartialView("_AddMeetingDetails", stage2DateOfMeeting);
            }
            catch (Exception ex)
            {
                _logger.LogError("Message-" + ex.Message + " StackTrace-" + ex.StackTrace + " DatetimeStamp-" + DateTime.Now);
                throw;
            }
        }

        [HttpGet]
        public IActionResult EditMeetingDetails(int? Id)
        {
            _logger.LogInformation($"Stage1STP2/EditMeetingDetails/{Id}");
            try
            {
                Stage2DateOfMeeting stage2DateOfMeeting = new Stage2DateOfMeeting();
                var findMeetingDetails = _context.Stage1STP2MeetingDetails.Where(x => x.ID == Id).FirstOrDefault();
                if (findMeetingDetails != null)
                {
                    stage2DateOfMeeting.ID = findMeetingDetails.ID;
                    stage2DateOfMeeting.Sno = findMeetingDetails.MeetingNo.Value;
                    stage2DateOfMeeting.MeetingDate = findMeetingDetails.MeetingDate;
                    stage2DateOfMeeting.StartingTime = Convert.ToString(findMeetingDetails.StartingTime);
                    stage2DateOfMeeting.EndTime = Convert.ToString(findMeetingDetails.EndTime);
                    stage2DateOfMeeting.NumberOfPeople = findMeetingDetails.NumberOfPeople.Value;

                    stage2DateOfMeeting.NoofOther = findMeetingDetails.NoofOther.Value;
                    stage2DateOfMeeting.NoofStudent = findMeetingDetails.NoofStudent.Value;
                    stage2DateOfMeeting.NoofTeacher = findMeetingDetails.NoofTeacher.Value;
                    stage2DateOfMeeting.PurposeId = findMeetingDetails.PurposeID;

                    stage2DateOfMeeting.hdnMeetingUpload = findMeetingDetails.MOMPath;
                    stage2DateOfMeeting.hdnPictureUpload = findMeetingDetails.PicturePath;
                }
                var PurposeDetails = _context.MstPurposes.Where(x => x.Isdeleted == false).OrderBy(p => p.Seq).ToList();
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<MstPurpose, SelectListModel>()
                    .ForMember(c => c.id, c => c.MapFrom(m => m.PurposeID))
                    .ForMember(d => d.value, o => o.MapFrom(s => s.Purpose));
                });
                var PurposeMap = Mapper.Map<List<SelectListModel>>(PurposeDetails);
                stage2DateOfMeeting.Purposelst = PurposeMap;

                stage2DateOfMeeting.LSTRegistrationForm = _context.tblRegistrationForms.Where(p => p.SUID == Convert.ToInt32(HttpContext.Session.GetString("Suid"))).ToList();

                return PartialView("_EditMeetingDetails", stage2DateOfMeeting);
            }
            catch (Exception ex)
            {
                _logger.LogError("Message-" + ex.Message + " StackTrace-" + ex.StackTrace + " DatetimeStamp-" + DateTime.Now);
                throw;
            }
        }

        [HttpPost]
        public IActionResult EditMeetingDetails(Stage2DateOfMeeting stage2DateOfMeeting)
        {
            _logger.LogInformation($"Stage1STP2/EditMeetingDetails/{stage2DateOfMeeting}");
            try
            {
                if (ModelState.IsValid)
                {
                    var SUID = Convert.ToInt32(HttpContext.Session.GetString("Suid"));
                    string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "Images\\Stage2Upload");
                    var findMeetingDetails = _context.Stage1STP2MeetingDetails.Where(x => x.ID == stage2DateOfMeeting.ID).FirstOrDefault();

                    if (stage2DateOfMeeting.hdnMeetingUpload != findMeetingDetails.MOMPath)
                    {
                        var fileNameWithPath = string.Concat(uploadsFolder, "\\", findMeetingDetails.MOMPath);
                        if (System.IO.File.Exists(fileNameWithPath))
                        {
                            System.IO.File.Delete(fileNameWithPath);
                        }
                    }
                    if (stage2DateOfMeeting.hdnPictureUpload != findMeetingDetails.PicturePath)
                    {
                        var fileNamePicturePath = string.Concat(uploadsFolder, "\\", findMeetingDetails.PicturePath);
                        if (System.IO.File.Exists(fileNamePicturePath))
                        {
                            System.IO.File.Delete(fileNamePicturePath);
                        }
                    }

                    if (findMeetingDetails != null)
                    {
                        findMeetingDetails.ID = stage2DateOfMeeting.ID;
                        findMeetingDetails.MeetingNo = stage2DateOfMeeting.Sno;
                        findMeetingDetails.MeetingDate = stage2DateOfMeeting.MeetingDate;
                        findMeetingDetails.StartingTime = TimeSpan.Parse(stage2DateOfMeeting.StartingTime);
                        findMeetingDetails.EndTime = TimeSpan.Parse(stage2DateOfMeeting.EndTime);
                        findMeetingDetails.NumberOfPeople = stage2DateOfMeeting.NumberOfPeople;

                        findMeetingDetails.NoofStudent = stage2DateOfMeeting.NoofStudent;
                        findMeetingDetails.NoofTeacher = stage2DateOfMeeting.NoofTeacher;
                        findMeetingDetails.NoofOther = stage2DateOfMeeting.NoofOther;
                        findMeetingDetails.PurposeID = stage2DateOfMeeting.PurposeId.Value;

                        findMeetingDetails.MOMPath = stage2DateOfMeeting.hdnMeetingUpload;
                        findMeetingDetails.PicturePath = stage2DateOfMeeting.hdnPictureUpload;
                        _context.Stage1STP2MeetingDetails.Update(findMeetingDetails);
                        _context.SaveChanges();
                    }

                    var meetingDetails = _context.Stage1STP2MeetingDetails.Where(x => x.SUID == SUID).ToList();
                    Mapper.Initialize(cfg =>
                    {
                        cfg.CreateMap<Stage1STP2MeetingDetail, Stage2DateOfMeeting>()
                        .ForMember(c => c.ID, c => c.MapFrom(m => m.ID))
                        .ForMember(c => c.MeetingDate, c => c.MapFrom(m => m.MeetingDate))
                        .ForMember(c => c.StartingTime, c => c.MapFrom(m => m.StartingTime))
                        .ForMember(c => c.EndTime, c => c.MapFrom(m => m.EndTime))
                        .ForMember(c => c.NumberOfPeople, c => c.MapFrom(m => m.NumberOfPeople))
                        .ForMember(c => c.NoofStudent, c => c.MapFrom(m => m.NoofStudent))
                        .ForMember(c => c.NoofTeacher, c => c.MapFrom(m => m.NoofTeacher))
                        .ForMember(c => c.NoofOther, c => c.MapFrom(m => m.NoofOther))
                        .ForMember(c => c.PurposeId, c => c.MapFrom(m => m.PurposeID))
                        .ForMember(c => c.PurposeName, c => c.MapFrom(m => GetPurposeName(m.PurposeID)))
                        .ForMember(c => c.hdnMeetingUpload, c => c.MapFrom(m => m.MOMPath))
                        .ForMember(c => c.hdnPictureUpload, c => c.MapFrom(m => m.PicturePath))
                        .ForMember(d => d.Sno, o => o.MapFrom(s => s.MeetingNo));
                    });
                    var MeetingMap = Mapper.Map<List<Stage2DateOfMeeting>>(meetingDetails);
                    var MeetingPartialView = RenderViewToStringAsync("/Views/Stage1STP2/_MeetingGridView.cshtml", MeetingMap);
                    return Json(new { part = "1", msg = "Data Updated Successfully.", htmlData = MeetingPartialView });
                }
                else
                {
                    var PurposeDetails = _context.MstPurposes.Where(x => x.Isdeleted == false).OrderBy(p => p.Seq).ToList();
                    Mapper.Initialize(cfg =>
                    {
                        cfg.CreateMap<MstPurpose, SelectListModel>()
                        .ForMember(c => c.id, c => c.MapFrom(m => m.PurposeID))
                        .ForMember(d => d.value, o => o.MapFrom(s => s.Purpose));
                    });
                    var PurposeMap = Mapper.Map<List<SelectListModel>>(PurposeDetails);
                    stage2DateOfMeeting.Purposelst = PurposeMap;

                    stage2DateOfMeeting.LSTRegistrationForm = _context.tblRegistrationForms.Where(p => p.SUID == Convert.ToInt32(HttpContext.Session.GetString("Suid"))).ToList();

                    var MeetingPartialView = RenderViewToStringAsync("/Views/Stage1STP2/_EditMeetingDetails.cshtml", stage2DateOfMeeting);
                    return Json(new { part = "2", htmlData = MeetingPartialView });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Message-" + ex.Message + " StackTrace-" + ex.StackTrace + " DatetimeStamp-" + DateTime.Now);
                throw;
            }
        }

        public string GetPurposeName(int? Purposeid)
        {
            _logger.LogInformation($"Stage1STP2/GetGradeName/{Purposeid}");
            try
            {
                var result = "";
                if (Purposeid == null) { }
                else
                { result = _context.MstPurposes.Where(x => x.PurposeID == Purposeid).FirstOrDefault().Purpose; }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Message-" + ex.Message + " StackTrace-" + ex.StackTrace + " DatetimeStamp-" + DateTime.Now);
                throw;
            }
        }


        [HttpPost]
        public IActionResult PostMeetingDetails(Stage2DateOfMeeting stage2DateOfMeeting)
        {
            try
            {
                if (string.IsNullOrEmpty(stage2DateOfMeeting.hdnMeetingUpload) && string.IsNullOrEmpty(stage2DateOfMeeting.hdnPictureUpload))
                {
                    ModelState.AddModelError("UploadMOMRequired", "Please select file");
                    ModelState.AddModelError("UploadPictureRequired", "Please select file");
                }
                else
                {
                    ModelState.Remove("UploadMOMRequired");
                    ModelState.Remove("UploadPictureRequired");
                }

                if (string.IsNullOrEmpty(stage2DateOfMeeting.hdnMeetingUpload))
                    ModelState.AddModelError("UploadMOMRequired", "Please select file");
                else
                    ModelState.Remove("UploadMOMRequired");

                if (string.IsNullOrEmpty(stage2DateOfMeeting.hdnPictureUpload))
                    ModelState.AddModelError("UploadPictureRequired", "Please select file");
                else
                    ModelState.Remove("UploadPictureRequired");

                if (ModelState.IsValid)
                {
                    var meetingNo = 0;
                    var SUID = Convert.ToInt32(HttpContext.Session.GetString("Suid"));
                    var record = _context.Stage1STP2MeetingDetails.Where(x => x.SUID == SUID).ToList();
                    meetingNo = record.Count + 1;

                    Stage1STP2MeetingDetail stage1Stp2MeetingDetail = new Stage1STP2MeetingDetail();
                    stage1Stp2MeetingDetail.MeetingDate = stage2DateOfMeeting.MeetingDate;
                    stage1Stp2MeetingDetail.SUID = SUID;
                    stage1Stp2MeetingDetail.MeetingNo = meetingNo;
                    stage1Stp2MeetingDetail.StartingTime = TimeSpan.Parse(stage2DateOfMeeting.StartingTime);
                    stage1Stp2MeetingDetail.EndTime = TimeSpan.Parse(stage2DateOfMeeting.EndTime);
                    stage1Stp2MeetingDetail.NumberOfPeople = stage2DateOfMeeting.NumberOfPeople;

                    stage1Stp2MeetingDetail.NoofOther = stage2DateOfMeeting.NoofOther;
                    stage1Stp2MeetingDetail.NoofStudent = stage2DateOfMeeting.NoofStudent;
                    stage1Stp2MeetingDetail.NoofTeacher = stage2DateOfMeeting.NoofTeacher;
                    stage1Stp2MeetingDetail.PurposeID = stage2DateOfMeeting.PurposeId;


                    stage1Stp2MeetingDetail.MOMPath = stage2DateOfMeeting.hdnMeetingUpload;
                    stage1Stp2MeetingDetail.PicturePath = stage2DateOfMeeting.hdnPictureUpload;
                    _context.Stage1STP2MeetingDetails.Add(stage1Stp2MeetingDetail);
                    _context.SaveChanges();

                    var meetingDetails = _context.Stage1STP2MeetingDetails.Where(x => x.SUID == SUID).ToList();
                    Mapper.Initialize(cfg =>
                    {
                        cfg.CreateMap<Stage1STP2MeetingDetail, Stage2DateOfMeeting>()
                        .ForMember(c => c.ID, c => c.MapFrom(m => m.ID))
                        .ForMember(c => c.MeetingDate, c => c.MapFrom(m => m.MeetingDate))
                        .ForMember(c => c.StartingTime, c => c.MapFrom(m => m.StartingTime))
                        .ForMember(c => c.EndTime, c => c.MapFrom(m => m.EndTime))
                        .ForMember(c => c.NumberOfPeople, c => c.MapFrom(m => m.NumberOfPeople))
                        .ForMember(c => c.NoofStudent, c => c.MapFrom(m => m.NoofStudent))
                        .ForMember(c => c.NoofTeacher, c => c.MapFrom(m => m.NoofTeacher))
                        .ForMember(c => c.NoofOther, c => c.MapFrom(m => m.NoofOther))
                        .ForMember(c => c.PurposeId, c => c.MapFrom(m => m.PurposeID))
                        .ForMember(c => c.PurposeName, c => c.MapFrom(m => GetPurposeName(m.PurposeID)))
                        .ForMember(c => c.hdnMeetingUpload, c => c.MapFrom(m => m.MOMPath))
                        .ForMember(c => c.hdnPictureUpload, c => c.MapFrom(m => m.PicturePath))
                        .ForMember(d => d.Sno, o => o.MapFrom(s => s.MeetingNo));
                    });
                    var MeetingMap = Mapper.Map<List<Stage2DateOfMeeting>>(meetingDetails);
                    var MeetingPartialView = RenderViewToStringAsync("/Views/Stage1STP2/_MeetingGridView.cshtml", MeetingMap);
                    return Json(new { part = "1", msg = "Data Saved Successfully.", htmlData = MeetingPartialView });
                }
                else
                {
                    var PurposeDetails = _context.MstPurposes.Where(x => x.Isdeleted == false).OrderBy(p => p.Seq).ToList();
                    Mapper.Initialize(cfg =>
                    {
                        cfg.CreateMap<MstPurpose, SelectListModel>()
                        .ForMember(c => c.id, c => c.MapFrom(m => m.PurposeID))
                        .ForMember(d => d.value, o => o.MapFrom(s => s.Purpose));
                    });
                    var PurposeMap = Mapper.Map<List<SelectListModel>>(PurposeDetails);
                    stage2DateOfMeeting.Purposelst = PurposeMap;

                    var MeetingPartialView = RenderViewToStringAsync("/Views/Stage1STP2/_AddMeetingDetails.cshtml", stage2DateOfMeeting);
                    return Json(new { part = "2", htmlData = MeetingPartialView });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Message-" + ex.Message + " StackTrace-" + ex.StackTrace + " DatetimeStamp-" + DateTime.Now);
                throw;
            }
        }

       
        [HttpPost]
        public ActionResult CheckPreDate(DateTime DateMeeting)
        {
            int cnt = _context.Stage1STP2MeetingDetails.Where(p => p.MeetingDate > DateMeeting).Count();
            if (cnt > 0)
            {
                return Json("1");
            }
            else
            {
                return Json("0");
            }
        }

        #endregion
    }

}