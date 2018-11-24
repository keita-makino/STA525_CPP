using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BTC.Controllers
{

    public class BTCController : Controller
    {
        CoinCheck.CoinCheck client = new CoinCheck.CoinCheck("GC0BLtAyNwTGfT1g", "GjvJPfn6HLLxl8J9iE7FsqiBUA01NO1T");

        // GET: BTC
        public ActionResult Index()
        {
            var cxt = new Models.DBC();
            var trades = cxt.TradeRecords.ToList();
            return View(trades);
        }

        // GET: BTC/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: BTC/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: BTC/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: BTC/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: BTC/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: BTC/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: BTC/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
