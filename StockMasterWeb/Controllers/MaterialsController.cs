using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using StockMasterWeb.Models;
using System.Linq;
using System.Threading.Tasks;

namespace StockMasterWeb.Controllers
{
    public class MaterialsController : Controller
    {
        private readonly StockMasterContext _context;

        public MaterialsController(StockMasterContext context)
        {
            _context = context;
        }

        // GET: Materials
        public async Task<IActionResult> Index(int? supplierId, int? stockThreshold)
        {
            var materials = _context.Materials
                .Include(m => m.Supplier)
                .AsQueryable();

            if (supplierId.HasValue)
                materials = materials.Where(m => m.SupplierId == supplierId);

            if (stockThreshold.HasValue)
                materials = materials.Where(m => m.Quantity < stockThreshold.Value);

            ViewData["Suppliers"] = new SelectList(_context.Suppliers, "Id", "Name");
            ViewData["SelectedSupplier"] = supplierId;
            ViewData["StockThreshold"] = stockThreshold;

            return View(await materials.ToListAsync());
        }

        // GET: Materials/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var material = await _context.Materials
                .Include(m => m.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (material == null) return NotFound();

            return View(material);
        }

        // GET: Materials/Create
        public IActionResult Create()
        {
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name");
            return View();
        }

        // POST: Materials/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Quantity,Unit,SupplierId")] Material material)
        {
            if (ModelState.IsValid)
            {
                _context.Add(material);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", material.SupplierId);
            return View(material);
        }

        // GET: Materials/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var material = await _context.Materials.FindAsync(id);
            if (material == null) return NotFound();

            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", material.SupplierId);
            return View(material);
        }

        // POST: Materials/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Quantity,Unit,SupplierId")] Material material)
        {
            if (id != material.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(material);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaterialExists(material.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "Name", material.SupplierId);
            return View(material);
        }

        // GET: Materials/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var material = await _context.Materials
                .Include(m => m.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (material == null) return NotFound();

            return View(material);
        }

        // POST: Materials/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material != null)
            {
                _context.Materials.Remove(material);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // Export to Excel
        public IActionResult ExportToExcel()
        {
            var materials = _context.Materials
                .Include(m => m.Supplier)
                .Select(m => new MaterialReportViewModel
                {
                    Name = m.Name,
                    SupplierName = m.Supplier.Name,
                    Quantity = m.Quantity,
                    Unit = m.Unit
                }).ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Материалы");

            ws.Cells[1, 1].Value = "Материал";
            ws.Cells[1, 2].Value = "Поставщик";
            ws.Cells[1, 3].Value = "Остаток";
            ws.Cells[1, 4].Value = "Ед. изм.";
            ws.Row(1).Style.Font.Bold = true;

            int row = 2;
            foreach (var m in materials)
            {
                ws.Cells[row, 1].Value = m.Name;
                ws.Cells[row, 2].Value = m.SupplierName;
                ws.Cells[row, 3].Value = m.Quantity;
                ws.Cells[row, 4].Value = m.Unit;
                row++;
            }

            ws.Cells.AutoFitColumns();

            var file = package.GetAsByteArray();
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "MaterialsReport.xlsx");
        }

        private bool MaterialExists(int id)
        {
            return _context.Materials.Any(e => e.Id == id);
        }
    }
}
