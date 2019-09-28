using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Uplift.DataAccess.Data;
using Uplift.DataAccess.Data.Repository.IRepository;
using Uplift.Models;
using Uplift.Utility;

namespace Uplift.Areas.Admin.Controllers
{
    [Authorize]
    [Area("Admin")]
    public class WebImageController : Controller
    {
        private readonly ApplicationDbContext _db;
        public WebImageController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id)
        {
            WebImages image = new WebImages();
            if (id == null)
            {
                return View(image);
            }
            else
            {
                image = _db.WebImages.SingleOrDefault(i => i.Id == id);
                if (image == null)
                {
                    return NotFound();
                }
                return View(image);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(int id, WebImages image)
        {
            var files = HttpContext.Request.Form.Files;
            if (files.Count > 0)
            {
                byte[] p1 = null;
                using (var fs1 = files[0].OpenReadStream())
                {
                    using (var ms1 = new MemoryStream())
                    {
                        fs1.CopyTo(ms1);
                        p1 = ms1.ToArray();
                    }
                }
                image.Picture = p1;
                ModelState.ClearValidationState("Picture");
                ModelState.MarkFieldValid("Picture");
            }
            if (ModelState.IsValid)
            {
                if (image.Id == 0)
                {
                    _db.WebImages.Add(image);
                }
                else
                {
                    var imageFromDb = _db.WebImages.Where(c => c.Id == id).FirstOrDefault();
                    imageFromDb.Name = image.Name;
                    if (files.Count > 0)
                    {
                        imageFromDb.Picture = image.Picture;
                    }
                }
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(image);
        }

        #region API calls

        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _db.WebImages.ToList() });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _db.WebImages.Find(id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            else
            {
                _db.WebImages.Remove(objFromDb);
                _db.SaveChanges();
                return Json(new { success = true, message = "Delete successful" });
            }
        }
        #endregion
    }
}