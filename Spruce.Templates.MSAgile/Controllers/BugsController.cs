﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Text;
using System.Linq.Dynamic;
using System.IO;
using System.ServiceModel.Syndication;
using Spruce.Core.Controllers;
using Spruce.Core;

namespace Spruce.Templates.MSAgile
{
	public class BugsController : SpruceControllerBase
    {
		public override ActionResult Index(string id, string sortBy, bool? desc,int? page,int? pageSize,
			string title,string assignedTo,string startDate,string endDate,string status)
		{
			SetBugView("Index");
			UpdateUserFilterOptions("bugs:default");

			ListData data = FilterAndPage<BugSummary>(GetBugFilterOptions(), id, sortBy, desc, page, pageSize);

			return View(data);
		}

		public ActionResult Heatmap(string id, string sortBy, bool? desc, int? page, int? pageSize,
			string title, string assignedTo, string startDate, string endDate, string status)
		{
			SetBugView("Heatmap");
			UpdateUserFilterOptions("bugs:heatmap");

			ListData data = FilterAndPage<BugSummary>(GetBugHeatmapFilterOptions(), id, sortBy, desc, page, pageSize);

			return View(data);
		}


		[HttpGet]
		public ActionResult New(string id)
		{
			BugManager manager = new BugManager();
			BugSummary summary = (BugSummary)manager.NewItem();

			if (!string.IsNullOrWhiteSpace(id))
				summary.Title = id;

			MSAgileEditData<BugSummary> data = new MSAgileEditData<BugSummary>();
			data.PageTitle = "New bug";
			data.States = summary.ValidStates;
			data.Reasons = summary.ValidReasons;
			data.Priorities = summary.ValidPriorities;
			data.Severities = summary.ValidSeverities;
			data.WorkItem = summary;

			return View("Edit", data);
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult New(BugSummary item)
		{
			BugManager manager = new BugManager();

			try
			{
				item.CreatedBy = UserContext.Current.Name;
				item.IsNew = true;
				manager.Save(item); // item.Id is updated

				// Save the files once it's saved (as we can't add an AttachmentsCollection as it has no constructors)
				if (Request.Files.Count > 0)
				{
					try
					{
						// Save to the App_Data folder.
						List<Attachment> attachments = new List<Attachment>();
						string filename1 = SaveFile("uploadFile1", item.Id);
						string filename2 = SaveFile("uploadFile2", item.Id);
						string filename3 = SaveFile("uploadFile3", item.Id);

						if (!string.IsNullOrEmpty(filename1))
						{
							attachments.Add(new Attachment(filename1, Request.Form["uploadFile1Comments"]));
						}
						if (!string.IsNullOrEmpty(filename2))
						{
							attachments.Add(new Attachment(filename2, Request.Form["uploadFile2Comments"]));
						}
						if (!string.IsNullOrEmpty(filename3))
						{
							attachments.Add(new Attachment(filename3, Request.Form["uploadFile3Comments"]));
						}

						manager.SaveAttachments(item.Id, attachments);
					}
					catch (IOException e)
					{
						TempData["Error"] = e.Message;
						return RedirectToAction("Edit", new { id = item.Id });
					}
				}

				return RedirectToAction("Index");
			}
			catch (SaveException e)
			{
				TempData["Error"] = e.Message;

				// Get the original back, to populate the valid reasons.
				QueryManager queryManager = new QueryManager();
				BugSummary summary = queryManager.ItemById<BugSummary>(item.Id);
				summary.IsNew = false;

				// Repopulate from the POST'd data
				summary.Title = item.Title;
				summary.State = item.State;
				summary.Reason = item.Reason;
				summary.Priority = item.Priority;
				summary.Severity = item.Severity;
				summary.Description = item.Description;
				summary.AssignedTo = item.AssignedTo;
				summary.AreaId = item.AreaId;
				summary.AreaPath = item.AreaPath;
				summary.IterationId = item.IterationId;
				summary.IterationPath = item.IterationPath;

				MSAgileEditData<BugSummary> data = new MSAgileEditData<BugSummary>();
				data.WorkItem = summary;
				data.PageTitle = "Bug " + item.Id;
				data.States = summary.ValidStates;
				data.Reasons = summary.ValidReasons;
				data.Priorities = summary.ValidPriorities;
				data.Severities = summary.ValidSeverities;

				return View(data);
			}
		}

		[HttpGet]
		public ActionResult Edit(int id,string fromUrl)
		{
			QueryManager manager = new QueryManager();
			BugSummary item = manager.ItemById<BugSummary>(id);
			item.IsNew = false;

			MSAgileEditData<BugSummary> data = new MSAgileEditData<BugSummary>();
			data.WorkItem = item;
			data.PageTitle = "Bug " + id;
			data.FromUrl = fromUrl;
			data.States = item.ValidStates;
			data.Reasons = item.ValidReasons;
			data.Priorities = item.ValidPriorities;
			data.Severities = item.ValidSeverities;

			return View(data);
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Edit(BugSummary item,string fromUrl)
		{
			BugManager manager = new BugManager();

			try
			{
				manager.Save(item);

				// Save the files once it's saved (as we can't add an AttachmentsCollection as it has no constructors)
				if (Request.Files.Count > 0)
				{
					try
					{
						// Save to the App_Data folder.
						List<Attachment> attachments = new List<Attachment>();
						string filename1 = SaveFile("uploadFile1", item.Id);
						string filename2 = SaveFile("uploadFile2", item.Id);
						string filename3 = SaveFile("uploadFile3", item.Id);

						if (!string.IsNullOrEmpty(filename1))
						{
							attachments.Add(new Attachment(filename1, Request.Form["uploadFile1Comments"]));
						}
						if (!string.IsNullOrEmpty(filename2))
						{
							attachments.Add(new Attachment(filename2, Request.Form["uploadFile2Comments"]));
						}
						if (!string.IsNullOrEmpty(filename3))
						{
							attachments.Add(new Attachment(filename3, Request.Form["uploadFile3Comments"]));
						}

						manager.SaveAttachments(item.Id, attachments);
					}
					catch (IOException e)
					{
						TempData["Error"] = e.Message;
						return RedirectToAction("Edit", new { id = item.Id });
					}
				}

				if (string.IsNullOrEmpty(fromUrl))
					return RedirectToAction("View", new { id = item.Id });
				else
					return Redirect(fromUrl);
			}
			catch (SaveException e)
			{
				TempData["Error"] = e.Message;

				// Get the original back, to populate the valid reasons.
				QueryManager queryManager = new QueryManager();
				BugSummary summary = queryManager.ItemById<BugSummary>(item.Id);
				summary.IsNew = false;

				// Repopulate from the POST'd data
				summary.Title = item.Title;
				summary.State = item.State;
				summary.Reason = item.Reason;
				summary.Priority = item.Priority;
				summary.Severity = item.Severity;
				summary.Description = item.Description;
				summary.AssignedTo = item.AssignedTo;
				summary.AreaId = item.AreaId;
				summary.AreaPath = item.AreaPath;
				summary.IterationId = item.IterationId;
				summary.IterationPath = item.IterationPath;

				MSAgileEditData<BugSummary> data = new MSAgileEditData<BugSummary>();
				data.WorkItem = summary;
				data.PageTitle = "Bug " + item.Id;
				data.FromUrl = fromUrl;
				data.States = summary.ValidStates;
				data.Reasons = summary.ValidReasons;
				data.Priorities = summary.ValidPriorities;
				data.Severities = summary.ValidSeverities;

				return View(data);
			}
		}

		public ActionResult Excel()
		{
			ListData data = FilterAndPage<BugSummary>(GetBugFilterOptions(), "", "CreatedDate", true, 1, 10000);
			return Excel(data.WorkItems);
		}

		public ActionResult Rss(string projectName, string areaPath,string iterationPath,string filter)
		{
			ListData data = FilterAndPage<BugSummary>(new FilterOptions(), projectName, "CreatedDate", true, 1, 10000);
			return Rss(data.WorkItems, "Bugs", projectName, areaPath, iterationPath, filter);
		}

		public ActionResult GetReasonsForState(string state)
		{
			QueryManager manager = new QueryManager();
			return Json(manager.GetAllowedValuesForState<BugSummary>(state, "Reason"), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Sets the user settings for either heatmap or a normal list.
		/// </summary>
		/// <param name="actionName"></param>
		private void SetBugView(string actionName)
		{
			UserContext.Current.Settings.BugView = actionName;
		}

		private FilterOptions GetBugFilterOptions()
		{
			return UserContext.Current.Settings.GetFilterOptionsForProject(UserContext.Current.CurrentProject.Name)
				.GetByKey("bugs:default");
		}

		private FilterOptions GetBugHeatmapFilterOptions()
		{
			return UserContext.Current.Settings.GetFilterOptionsForProject(UserContext.Current.CurrentProject.Name)
				.GetByKey("bugs:heatmap");
		}

		protected override WorkItemManager GetManager()
		{
			return new BugManager();
		}

		protected override IEnumerable<WorkItemSummary> Page(Pager pager, IEnumerable<WorkItemSummary> items, int currentPage)
		{
			string action = ControllerContext.Controller.ValueProvider.GetValue("action").RawValue.ToString().ToLower();
			if (action == "heatmap")
			{
				List<BugSummary> list = new List<BugSummary>();
				foreach (BugSummary summary in items)
				{
					list.Add(summary);
				}

				list = pager.Page<BugSummary>(list, currentPage, "Priority asc, Severity asc").ToList();

				List<WorkItemSummary> results = new List<WorkItemSummary>();
				foreach (BugSummary summary in list)
				{
					results.Add(summary);
				}

				return results;
			}

			return base.Page(pager, items, currentPage);
		}
    }
}
