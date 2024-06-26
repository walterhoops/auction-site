﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using silkroadmvc.Data;
using silkroadmvc.Data.Services;
using silkroadmvc.Models;

namespace silkroadmvc.Controllers
{
    public class AuctionsController : Controller
    {
        private readonly IAuctionsService _auctionsService;
        private readonly IBidsService _bidsService;
        private readonly ICommentsService _commentsService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AuctionsController(IAuctionsService auctionsService, IWebHostEnvironment webHostEnvironment, IBidsService bidsService, ICommentsService commentsService)
        {
            _auctionsService = auctionsService;
            _webHostEnvironment = webHostEnvironment;
            _bidsService = bidsService;
            _commentsService = commentsService;
        }

        // GET: Auctions
        public async Task<IActionResult> Index(int? pageNumber, string searchString)
        {
            var applicationDbContext = _auctionsService.GetAll();
            int pageSize = 3;
            if (!string.IsNullOrEmpty(searchString))
            {
                applicationDbContext = applicationDbContext.Where(a => a.Title.Contains(searchString));
                return View(await PaginatedList<Auction>.CreateAsync(applicationDbContext.Where(a => a.IsSold == false).AsNoTracking(), pageNumber ?? 1, pageSize));
            }
            return View(await PaginatedList<Auction>.CreateAsync(applicationDbContext.Where(a => a.IsSold == false).AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: My Auctions - Show auctions created by the user

        public async Task<IActionResult> MyAuctions(int? pageNumber)
        {
            var applicationDbContext = _auctionsService.GetAll();
            int pageSize = 3;

            return View("Index", await PaginatedList<Auction>.CreateAsync(applicationDbContext.Where(a => a.IdentityUserId == User.FindFirstValue(ClaimTypes.NameIdentifier)).AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Bids/MyBids
        public async Task<IActionResult> MyBids(int? pageNumber)
        {
            var applicationDbContext = _bidsService.GetAll();
            int pageSize = 3;

            return View(await PaginatedList<Bid>.CreateAsync(applicationDbContext.Where(a => a.IdentityUserId == User.FindFirstValue(ClaimTypes.NameIdentifier)).AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Auctions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auction = await _auctionsService.GetById(id);

            if (auction == null)
            {
                return NotFound();
            }

            return View(auction);
        }

        // GET: Auctions/Create
        public IActionResult Create()
        {

            return View();
        }

        // POST: Auctions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AuctionVM auction)
        {
            if (auction.Image != null)
            {
                string fileName = auction.Image.FileName;
                string filePath = Path.Combine("/Images", fileName);
                using (var fileStream = new FileStream(Path.Combine(_webHostEnvironment.WebRootPath, "Images", fileName), FileMode.Create))
                {
                    auction.Image.CopyTo(fileStream);
                }
                var listObject = new Auction
                {
                    Title = auction.Title,
                    Description = auction.Description,
                    Price = auction.Price,
                    IdentityUserId = auction.IdentityUserId,
                    ImagePath = filePath,
                };
                await _auctionsService.Add(listObject);
                return RedirectToAction("Index");
            }
            return View(auction);
        }

        // Add Bids
        [HttpPost]
        public async Task<ActionResult> AddBid([Bind("Id, Price, AuctionId, IdentityUserId")] Bid bid)
        {
            if (ModelState.IsValid)
            {
                await _bidsService.Add(bid);
            }
            var auction = await _auctionsService.GetById(bid.AuctionId);
            auction.Price = bid.Price;
            await _auctionsService.SaveChanges();
            return View("Details", auction);
        }

        // Close Bidding
        public async Task<ActionResult> CloseBidding(int id)
        {
            var auction = await _auctionsService.GetById(id);
            auction.IsSold = true;
            await _auctionsService.SaveChanges();
            return View("Details", auction);
        }

        // POST: Auctions/AddComment
        [HttpPost]
        public async Task<ActionResult> AddComment([Bind("Id, Content, AuctionId, IdentityUserId")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                await _commentsService.Add(comment);
            }
            var auction = await _auctionsService.GetById(comment.AuctionId);
            return View("Details", auction);
        }
        //// GET: Auctions/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null || _context.Auctions == null)
        //    {
        //        return NotFound();
        //    }

        //    var auction = await _context.Auctions.FindAsync(id);
        //    if (auction == null)
        //    {
        //        return NotFound();
        //    }
        //    ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", auction.IdentityUserId);
        //    return View(auction);
        //}

        //// POST: Auctions/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Price,ImagePath,IsSold,IdentityUserId")] Auction auction)
        //{
        //    if (id != auction.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(auction);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!AuctionExists(auction.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", auction.IdentityUserId);
        //    return View(auction);
        //}

        //// GET: Auctions/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null || _context.Auctions == null)
        //    {
        //        return NotFound();
        //    }

        //    var auction = await _context.Auctions
        //        .Include(a => a.User)
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (auction == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(auction);
        //}

        //// POST: Auctions/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    if (_context.Auctions == null)
        //    {
        //        return Problem("Entity set 'ApplicationDbContext.Auctions'  is null.");
        //    }
        //    var auction = await _context.Auctions.FindAsync(id);
        //    if (auction != null)
        //    {
        //        _context.Auctions.Remove(auction);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        //private bool AuctionExists(int id)
        //{
        //  return (_context.Auctions?.Any(e => e.Id == id)).GetValueOrDefault();
        //}
    }
}
