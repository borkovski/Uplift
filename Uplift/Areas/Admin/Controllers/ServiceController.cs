using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Uplift.DataAccess.Data.Repository.IRepository;
using Uplift.Models.ViewModels;

namespace Uplift.Areas.Admin.Controllers
{
    [Authorize]
    [Area("Admin")]
    public class ServiceController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        [BindProperty]
        public ServiceVM ServiceVM { get; set; }
        public ServiceController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Upsert(int? id)
        {
            ServiceVM = new ServiceVM()
            {
                Service = new Models.Service(),
                CategoryList = _unitOfWork.Category.GetCategoryListForDropDown(),
                FrequencyList = _unitOfWork.Frequency.GetFrequencyListForDropDown()
            };
            if(id != null)
            {
                ServiceVM.Service = _unitOfWork.Service.Get(id.GetValueOrDefault());
            }
            return View(ServiceVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert()
        {
            if (ModelState.IsValid)
            {
                string webRootPath = _hostEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                if(ServiceVM.Service.Id == 0)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(webRootPath, @"images\services");
                    var extension = Path.GetExtension(files[0].FileName);
                    using(var fileStream = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }
                    ServiceVM.Service.ImageUrl = @"\images\services\" + fileName + extension;
                    _unitOfWork.Service.Add(ServiceVM.Service);
                }
                else
                {
                    var serviceFromDb =_unitOfWork.Service.Get(ServiceVM.Service.Id);
                    if(files.Count() > 0)
                    {
                        string fileName = Guid.NewGuid().ToString();
                        var uploads = Path.Combine(webRootPath, @"images\services");
                        var extension_new = Path.GetExtension(files[0].FileName);
                        var imagePath = Path.Combine(webRootPath, serviceFromDb.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                        using (var fileStream = new FileStream(Path.Combine(uploads, fileName + extension_new), FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);
                        }
                        ServiceVM.Service.ImageUrl = @"\images\services\" + fileName + extension_new;
                    }
                    else
                    {
                        ServiceVM.Service.ImageUrl = serviceFromDb.ImageUrl;
                    }
                    _unitOfWork.Service.Update(ServiceVM.Service);
                }
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ServiceVM.CategoryList = _unitOfWork.Category.GetCategoryListForDropDown();
                ServiceVM.FrequencyList = _unitOfWork.Frequency.GetFrequencyListForDropDown();
                return View(ServiceVM);
            }
        }
        #region API calls
        public IActionResult GetAll()
        {
            return Json(new { data = _unitOfWork.Service.GetAll(includeProperties:"Category,Frequency") });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _unitOfWork.Service.Get(id);
            string webRootPath = _hostEnvironment.WebRootPath;
            var imagePath = Path.Combine(webRootPath, objFromDb.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            else
            {
                _unitOfWork.Service.Remove(objFromDb);
                _unitOfWork.Save();
                return Json(new { success = true, message = "Delete successful" });
            }
        }
        #endregion
    }
}