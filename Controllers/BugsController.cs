﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Spruce.Models;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Text;

namespace Spruce.Controllers
{
    public class BugsController : Controller
    {
		public ActionResult Index()
		{
			Session["ListLink"] = "All";
			return View("Index", WorkItemManager.AllBugs().ToList());
		}

		public ActionResult View(int id)
		{
			WorkItemSummary item = WorkItemManager.ItemById(id);
			return View(item);
		}

		public ActionResult Active()
		{
			Session["ListLink"] = "Active";
			return View("Index", WorkItemManager.AllActiveBugs().ToList());
		}

		public ActionResult Closed()
		{
			Session["ListLink"] = "Closed";
			return View("Index", WorkItemManager.AllClosedBugs().ToList());
		}

		public ActionResult Resolve(int id)
		{
			WorkItemManager.Resolve(id);
			return RedirectToAction("View", new { id = id });
		}

		public ActionResult Close(int id)
		{
			WorkItemManager.Close(id);
			return RedirectToAction("View", new { id = id });
		}

		[HttpGet]
		public ActionResult New(string id)
		{
			WorkItemSummary item = WorkItemManager.NewBug();

			if (!string.IsNullOrWhiteSpace(id))
				item.Title = id;

			ViewData["PageName"] = "New bug";
			ViewData["States"] = item.ValidStates;
			ViewData["Priorities"] = item.ValidPriorities;
			ViewData["Areas"] = SpruceContext.Current.CurrentProject.Areas;
			ViewData["Iterations"] = SpruceContext.Current.CurrentProject.Iterations;
			ViewData["Users"] = SpruceContext.Current.Users;

			return View("Edit", item);
		}

		[HttpPost]
		public ActionResult New(WorkItemSummary item)
		{
			item.CreatedBy = SpruceContext.Current.CurrentUser;
			item.IsNew = true;
			WorkItemManager.SaveBug(item);

			return RedirectToAction("Index");
		}

		[HttpGet]
		public ActionResult Edit(int id)
		{
			WorkItemSummary item = WorkItemManager.ItemById(id);
			item.IsNew = false;

			ViewData["PageName"] = "Bug " + id;
			ViewData["States"] = item.ValidStates;
			ViewData["Priorities"] = item.ValidPriorities;
			ViewData["Areas"] = SpruceContext.Current.CurrentProject.Areas;
			ViewData["Iterations"] = SpruceContext.Current.CurrentProject.Iterations;
			ViewData["Users"] = SpruceContext.Current.Users;

			return View(item);
		}

		[HttpPost]
		public ActionResult Edit(WorkItemSummary item)
		{
			WorkItemManager.SaveExisting(item);
			return RedirectToAction("View", new { id = item.Id });
		}
    }
}
