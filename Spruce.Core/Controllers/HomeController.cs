﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Text;
using Spruce.Core.Search;

namespace Spruce.Core.Controllers
{
	public class HomeController : SpruceControllerBase<WorkItemSummary>
	{
		public ActionResult Index()
		{
			return View();
		}

		public ActionResult Search(string q)
		{
			IList<WorkItemSummary> summaries = new List<WorkItemSummary>();
			ListData<WorkItemSummary> data = new ListData<WorkItemSummary>();

			if (!string.IsNullOrEmpty(q))
			{
				SearchManager manager = new SearchManager();

				if (manager.IsId(q))
				{
					// For single work item ids (that exist), redirect straight to their view page
					int id = int.Parse(q);
					QueryManager queryManager = new QueryManager();
					WorkItemSummary summary = queryManager.ItemById(id);

					if (summary != null)
					{
						return Redirect(SpruceSettings.SiteUrl + "/" + summary.Controller + "/View/" + id);
					}
				}
				else
				{
					summaries = manager.Search(q).ToList();
					data.WorkItems = summaries;

					ViewData["search"] = q;
				}
			}

			return View(data);
		}

		public ActionResult ChangeProject(string project, string area, string iteration, string fromUrl)
		{
			if (!string.IsNullOrEmpty(project) && project != UserContext.Current.CurrentProject.Name)
			{
				UserContext.Current.ChangeCurrentProject(project);
			}
			else
			{
				if (!string.IsNullOrEmpty(area))
				{
					AreaSummary summary = UserContext.Current.CurrentProject.Areas.FirstOrDefault(a => a.Path == area);
					UserContext.Current.Settings.AreaName = summary.Name;
					UserContext.Current.Settings.AreaPath = summary.Path;
				}

				if (!string.IsNullOrEmpty(iteration))
				{
					IterationSummary summary = UserContext.Current.CurrentProject.Iterations.FirstOrDefault(i => i.Path == iteration);
					UserContext.Current.Settings.IterationName = summary.Name;
					UserContext.Current.Settings.IterationPath = summary.Path;
				}
			}

			// Redirect, however if the fromUrl has the bugs/tasks filter querystrings do not redirect
			// as this will cause those filter settings to be persisted onto the settings of project we're changing to.
			if (!string.IsNullOrEmpty(fromUrl))
			{
				Uri uri = new Uri(fromUrl);
				return Redirect(uri.AbsolutePath);
			}
			else
				return RedirectToAction("Index");
		}

		public ActionResult StoredQueries()
		{
			ListData<WorkItemSummary> data = new ListData<WorkItemSummary>();
			ViewData["CurrentQueryId"] = Guid.Empty;

			return View(data);
		}

		public ActionResult StoredQuery(Guid id, string sortBy, bool? desc, int? page, int? pageSize)
		{
			QueryManager manager = new QueryManager();

			ListData<WorkItemSummary> data = new ListData<WorkItemSummary>();
			data.WorkItems = manager.StoredQuery(id);
			PageList(data, false, sortBy, desc, page, pageSize);
			ViewData["CurrentQueryId"] = id;

			return View("StoredQueries", data);
		}

		/// <summary>
		/// Returns a string containing Javascript 'constants' for the site.
		/// </summary>
		public ActionResult GlobalJsVars()
		{
			UrlHelper helper = new UrlHelper(HttpContext.Request.RequestContext);

			StringBuilder builder = new StringBuilder();
			builder.AppendLine(string.Format("var SPRUCE_SCRIPTPATH = '{0}';", helper.Content("~/Assets/Scripts/")));
			builder.AppendLine(string.Format("var SPRUCE_CSSPATH = '{0}';", helper.Content("~/Assets/Css/")));
			builder.AppendLine(string.Format("var SPRUCE_IMAGEPATH = '{0}';", helper.Content("~/Assets/Images/")));

			return Content(builder.ToString(), "text/javascript");
		}
	}
}
