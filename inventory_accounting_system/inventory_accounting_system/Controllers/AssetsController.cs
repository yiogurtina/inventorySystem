﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using inventory_accounting_system.Data;
using inventory_accounting_system.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using inventory_accounting_system.Services;
using Microsoft.AspNetCore.Http;

namespace inventory_accounting_system.Controllers
{
    public class AssetsController : Controller
    {
        #region Dependency Injection

        private readonly ApplicationDbContext _context;
        static Random generator = new Random();
        private readonly IHostingEnvironment _appEnvironment;
        private readonly FileUploadService _fileUploadService;

        public AssetsController(ApplicationDbContext context, IHostingEnvironment appEnvironment,
            FileUploadService fileUploadService)
        {
            _context = context;
            _appEnvironment = appEnvironment;
            _fileUploadService = fileUploadService;
        }

        #endregion

        #region Index

        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Assets
                .Include(a => a.Category)
                .Include(a => a.Employee)
                .Include(a => a.Office)
                .Include(a => a.Storage)
                .Include(a => a.Supplier);
            return View(await applicationDbContext.ToListAsync());
        }

        #endregion

        #region Details

        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asset = await _context.Assets
                .Include(a => a.Category)
                .Include(a => a.Employee)
                .Include(a => a.Office)
                .Include(a => a.Storage)
                .Include(a => a.Supplier)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (asset == null)
            {
                return NotFound();
            }

            return View(asset);
        }

        #endregion

        #region Create

        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["EmployeeId"] = new SelectList(_context.Users, "Id", "Login");
            ViewData["OfficeId"] = new SelectList(_context.Offices, "Id", "Title");
            ViewData["StorageId"] = new SelectList(_context.Storages, "Id", "Title");
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,CategoryId,InventNumber,InventPrefix,Date,OfficeId,StorageId,SupplierId,EmployeeId,Id,Image")] Asset asset, string serialNum)
        {
            var categoryPrefix = _context.Categories
                .Where(c => c.Id == asset.CategoryId)
                .Select(c => c.Prefix)
                .FirstOrDefaultAsync();

            if (ModelState.IsValid)
            {
                asset.InventNumber = categoryPrefix.Result + generator.Next(0, 1000000).ToString("D6") + asset.InventPrefix;
                asset.SerialNum = serialNum;
                if (asset.Image != null)
                {
                    UploadPhoto(asset);
                }
                else
                {
                    asset.ImagePath = "images/default-image.jpg";
                }

                _context.Add(asset);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", asset.CategoryId);
            ViewData["EmployeeId"] = new SelectList(_context.Users, "Id", "Login", asset.EmployeeId);
            ViewData["OfficeId"] = new SelectList(_context.Offices, "Id", "Title", asset.OfficeId);
            ViewData["StorageId"] = new SelectList(_context.Storages, "Id", "Title", asset.StorageId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", asset.SupplierId);
            return View(asset);
        }

        #endregion
        
        #region Edit

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asset = await _context.Assets.SingleOrDefaultAsync(m => m.Id == id);
            if (asset == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", asset.CategoryId);
            ViewData["EmployeeId"] = new SelectList(_context.Users, "Id", "Login", asset.EmployeeId);
            ViewData["OfficeId"] = new SelectList(_context.Offices, "Id", "Title", asset.OfficeId);
            ViewData["StorageId"] = new SelectList(_context.Storages, "Id", "Title", asset.StorageId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", asset.SupplierId);
            return View(asset);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Name,CategoryId,InventNumber,Date,OfficeId,StorageId,SupplierId,EmployeeId,Image,Id")] Asset asset, string serialNum, string currentPath)
        {
            if (id != asset.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    asset.SerialNum = serialNum;
                    if (asset.Image != null)
                    {
                        UploadPhoto(asset);
                    }
                    else
                    {
                        asset.ImagePath = currentPath;
                    }

                    _context.Update(asset);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AssetExists(asset.Id))
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", asset.CategoryId);
            ViewData["EmployeeId"] = new SelectList(_context.Users, "Id", "Login", asset.EmployeeId);
            ViewData["OfficeId"] = new SelectList(_context.Offices, "Id", "Title", asset.OfficeId);
            ViewData["StorageId"] = new SelectList(_context.Storages, "Id", "Title", asset.StorageId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", asset.SupplierId);
            return View(asset);
        }
        #endregion

        #region Delete

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asset = await _context.Assets
                .Include(a => a.Category)
                .Include(a => a.Employee)
                .Include(a => a.Office)
                .Include(a => a.Storage)
                .Include(a => a.Supplier)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (asset == null)
            {
                return NotFound();
            }

            return View(asset);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var asset = await _context.Assets.SingleOrDefaultAsync(m => m.Id == id);
            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region AssetExists

        private bool AssetExists(string id)
        {
            return _context.Assets.Any(e => e.Id == id);
        }

        #endregion

        private void UploadPhoto(Asset asset)
        {
            var path = Path.Combine(_appEnvironment.WebRootPath, $"images\\{asset.Name}\\image");
            _fileUploadService.Upload(path, asset.Image.FileName, asset.Image);
            asset.ImagePath = $"images/{asset.Name}/image/{asset.Image.FileName}";
        }
    }
}
