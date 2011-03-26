﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Spruce.Core.Search;

namespace Spruce.Core.Controllers
{
	public class SearchController : ControllerBase
    {
        public ActionResult Index(string id)
        {
			IList<WorkItemSummary> summaries = new List<WorkItemSummary>();

			if (!string.IsNullOrEmpty(id))
			{
				SearchManager manager = new SearchManager();
				summaries = manager.Search(id);
				ViewData["search"] = id;

				if (manager.IsWorkItemId(id) && summaries.Count == 1)
				{
					// For single work item ids (that exist), redirect straight to the bug page
					return RedirectToAction("View", "Bugs", new { id = int.Parse(id) });
				}
			}

			return View(summaries);
        }
    }
}