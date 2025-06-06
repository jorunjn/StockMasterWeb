using System.IO;                            // нужен для File() и Path
using System;                               // базовый
using System.Collections.Generic;           // IEnumerable и коллекции
using System.Linq;                          // LINQ: .Select(), .Sum(), .Any()
using System.Threading.Tasks;               // async/await
using Microsoft.AspNetCore.Mvc;             // Controller, IActionResult, File()
using Microsoft.AspNetCore.Mvc.Rendering;   // SelectList
using Microsoft.EntityFrameworkCore;        // Include()
using StockMasterWeb.Models;                //  модели
using OfficeOpenXml;                        // EPPlus
using OfficeOpenXml.Style;                  // Стили ячеек Excel



namespace StockMasterWeb.Controllers
{
    public class OrdersController : Controller
    {
        private readonly StockMasterContext _context;

        public OrdersController(StockMasterContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? status, int? userId)
        {
            var orders = _context.Orders
                .Include(o => o.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && status != "Все статусы")
                orders = orders.Where(o => o.Status == status);

            if (userId.HasValue)
                orders = orders.Where(o => o.UserId == userId);

            ViewData["Statuses"] = new List<string>
    {
        "Все статусы",
        "В обработке",
        "Готов к выдаче",
        "Доставлен",
        "Отменен"
    };

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username");
            ViewData["SelectedStatus"] = status;
            ViewData["SelectedUserId"] = userId;

            return View(await orders.ToListAsync());
        }



        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username");
            return View();
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,OrderDate,Status")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username", order.UserId);
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username", order.UserId);
            return View(order);
        }

        // POST: Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,OrderDate,Status")] Order order)
        {
            if (id != order.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username", order.UserId);
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Report()
        {
            var report = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .Select(o => new OrderReportViewModel
                {
                    OrderId = o.Id,
                    ClientName = o.User.Username,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    TotalItems = o.OrderItems.Sum(oi => oi.Quantity),
                    TotalAmount = o.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .ToListAsync();

            return View(report);
        }
        public IActionResult ExportToExcel()
        {
            var orders = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .Select(o => new
                {
                    Id = o.Id,
                    Client = o.User.Username,
                    Date = o.OrderDate.ToShortDateString(),
                    Status = o.Status,
                    Items = o.OrderItems.Sum(oi => oi.Quantity),
                    Total = o.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice)
                }).ToList();

            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Отчёт по заказам");

            // Заголовки
            ws.Cells[1, 1].Value = "Номер заказа";
            ws.Cells[1, 2].Value = "Клиент";
            ws.Cells[1, 3].Value = "Дата";
            ws.Cells[1, 4].Value = "Статус";
            ws.Cells[1, 5].Value = "Позиций";
            ws.Cells[1, 6].Value = "Сумма (BYN)";
            ws.Row(1).Style.Font.Bold = true;

            int row = 2;
            foreach (var order in orders)
            {
                ws.Cells[row, 1].Value = order.Id;
                ws.Cells[row, 2].Value = order.Client;
                ws.Cells[row, 3].Value = order.Date;
                ws.Cells[row, 4].Value = order.Status;
                ws.Cells[row, 5].Value = order.Items;
                ws.Cells[row, 6].Value = order.Total;
                row++;
            }

            ws.Cells.AutoFitColumns();

            var fileBytes = package.GetAsByteArray();
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "OrderReport.xlsx");
        }



    }
}
