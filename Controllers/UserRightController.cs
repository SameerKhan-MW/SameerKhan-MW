using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolProgramme.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SchoolProgramme.CommonCS;
using System.Data;
using Microsoft.Data.SqlClient;
using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace SchoolProgramme.Controllers
{
    public class UserRightController : Controller
    {
        private readonly SPDBContext _context;

        public UserRightController(SPDBContext context)
        {
            _context = context;
        }


        public IActionResult AssignRightsToRole()
        {
            ViewData["RoleID"] = new SelectList(_context.MstRoles.Where(p => p.IsDeleted == 0), "RoleID", "Role");
            return View();
        }

        //user define dropdown bind//
        public IActionResult renderRightsMenuCMS(int RoleID)
        {
            var MenuList = _context.RoleMenus.Where(c => c.RoleID == RoleID && (c.Menu.MenuType == 4)).Include(c => c.Menu).OrderBy(c=>c.Menu.MenuSequence).ToList();
            return PartialView("_PVUserRightMenuAdmin", MenuList);
        }

        //user define dropdown bind//
        public IActionResult SaveUserRights(bool DSN, bool ADN, bool EDN, bool DLN, int RoleMenuIdN, int MenuParentid, int roleid, string Flag)
        {
            RoleMenu RoleMenuN = (from c in _context.RoleMenus
                                  where c.RoleMenuID == RoleMenuIdN
                                  select c).FirstOrDefault();
            RoleMenuN.Display = DSN;
            RoleMenuN.AddNew = ADN;
            RoleMenuN.Edit = EDN;
            RoleMenuN.IsDeleted = DLN;
            _context.Update(RoleMenuN);
            _context.SaveChanges();


            if (MenuParentid == 0) { }
            else
            {
                RoleMenu RoleMenuNParent = (from c in _context.RoleMenus
                                            where c.MenuID == MenuParentid && c.RoleID == roleid
                                            select c).FirstOrDefault();
                RoleMenuNParent.Display = DSN;
                RoleMenuNParent.AddNew = ADN;
                RoleMenuNParent.Edit = EDN;
                RoleMenuNParent.IsDeleted = DLN;
                _context.Update(RoleMenuNParent);
                _context.SaveChanges();
            }


            if (Flag == "Admin")
            {
                var MenuList = _context.RoleMenus.Where(c => c.RoleID == RoleMenuN.RoleID && (c.Menu.MenuType == 4)).Include(c => c.Menu).ToList();
                return PartialView("_PVUserRightMenuAdmin", MenuList);
            }
            else
            {
                var MenuList = _context.RoleMenus.Where(c => c.RoleID == RoleMenuN.RoleID && (c.Menu.MenuType == 4)).Include(c => c.Menu).ToList();
                return PartialView("_PVUserRightMenuAdmin", MenuList);
                //var MenuList = _context.RoleMenus.Where(c => c.RoleID == RoleMenuN.RoleID).Include(c => c.Menu).ToList();
                //return PartialView("_PVUserRightMenu", MenuList);
            }

        }

        // GET: UserRightController
        public ActionResult Index()
        {
            return View();
        }

        // GET: UserRightController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: UserRightController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UserRightController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: UserRightController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: UserRightController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: UserRightController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: UserRightController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
