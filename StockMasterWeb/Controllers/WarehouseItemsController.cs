using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using StockMasterWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockMasterWeb.Controllers
{
    public class WarehouseItemsController : Controller
    {
        private readonly StockMasterContext _context;

        public WarehouseItemsController(StockMasterContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IActionResult> Index(string? search, int? lowStock)
        {
            var items = _context.WarehouseItems
                .Include(w => w.Material)
                .Include(w => w.Product)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                items = items.Where(w =>
                    w.Product != null && w.Product.Name.Contains(search) ||
                    w.Material != null && w.Material.Name.Contains(search));
            }

            if (lowStock == 1)
            {
                items = items.Where(w => w.Quantity < 10);
            }

            return View(await items.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return BadRequest();

            var warehouseItem = await _context.WarehouseItems
                .Include(w => w.Material)
                .Include(w => w.Product)
                .FirstOrDefaultAsync(m => m.Id == id.Value);

            return warehouseItem == null ? NotFound() : View(warehouseItem);
        }

        public IActionResult Create()
        {
            ViewData["MaterialId"] = new SelectList(_context.Materials, "Id", "Name");
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProductId,MaterialId,Quantity")] WarehouseItem warehouseItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(warehouseItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["MaterialId"] = new SelectList(_context.Materials, "Id", "Name", warehouseItem.MaterialId);
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", warehouseItem.ProductId);
            return View(warehouseItem);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return BadRequest();

            var warehouseItem = await _context.WarehouseItems.FindAsync(id.Value);
            if (warehouseItem == null) return NotFound();

            ViewData["MaterialId"] = new SelectList(_context.Materials, "Id", "Name", warehouseItem.MaterialId);
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", warehouseItem.ProductId);
            return View(warehouseItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProductId,MaterialId,Quantity")] WarehouseItem warehouseItem)
        {
            if (id != warehouseItem.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(warehouseItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WarehouseItemExists(warehouseItem.Id)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["MaterialId"] = new SelectList(_context.Materials, "Id", "Name", warehouseItem.MaterialId);
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", warehouseItem.ProductId);
            return View(warehouseItem);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();

            var warehouseItem = await _context.WarehouseItems
                .Include(w => w.Material)
                .Include(w => w.Product)
                .FirstOrDefaultAsync(m => m.Id == id.Value);

            return warehouseItem == null ? NotFound() : View(warehouseItem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var warehouseItem = await _context.WarehouseItems.FindAsync(id);
            if (warehouseItem != null)
            {
                _context.WarehouseItems.Remove(warehouseItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ExportToExcel()
        {
            var report = _context.WarehouseItems
                .Include(w => w.Product)
                .Include(w => w.Material)
                .Select(w => new WarehouseReportViewModel
                {
                    ProductName = w.Product.Name,
                    MaterialName = w.Material.Name,
                    Quantity = w.Quantity
                }).ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Склад");

            ws.Cells[1, 1].Value = "Продукт";
            ws.Cells[1, 2].Value = "Материал";
            ws.Cells[1, 3].Value = "Остаток";
            ws.Row(1).Style.Font.Bold = true;

            int row = 2;
            foreach (var item in report)
            {
                ws.Cells[row, 1].Value = item.ProductName;
                ws.Cells[row, 2].Value = item.MaterialName;
                ws.Cells[row, 3].Value = item.Quantity;
                row++;
            }

            ws.Cells.AutoFitColumns();

            var file = package.GetAsByteArray();
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Склад.xlsx");
        }


        private bool WarehouseItemExists(int id) =>
            _context.WarehouseItems.Any(e => e.Id == id);
    }
}
