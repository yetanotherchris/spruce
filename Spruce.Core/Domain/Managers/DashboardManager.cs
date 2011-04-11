﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.VersionControl.Client;
using System.Collections;

namespace Spruce.Core
{
	public class DashboardManager
	{
		public static DashboardSummary GetSummary()
		{
			IList<WorkItemSummary> allbugs = WorkItemManager.AllBugs();
			IList<WorkItemSummary> allTasks = WorkItemManager.AllTasks();

			DashboardSummary summary = new DashboardSummary();
			summary.RecentCheckins = RecentCheckins();
			summary.ActiveBugs = WorkItemManager.AllActiveBugs().Count;
			summary.ActiveTasks = allTasks.Count;
			summary.BugCount = allbugs.Count;
			summary.TaskCount = allTasks.Count;
			summary.MyActiveBugCount = allbugs.Where(b => b.State == "Active").ToList().Count;

			summary.MyActiveBugs = allbugs.Where(b => b.State == "Active" && b.CreatedBy == SpruceContext.Current.CurrentUser)
				.OrderByDescending(b => b.CreatedDate).Take(5).ToList();

			summary.MyActiveTasks = allTasks.Where(b => b.State == "Active" && b.CreatedBy == SpruceContext.Current.CurrentUser)
				.OrderByDescending(b => b.CreatedDate).Take(5).ToList();

			return summary;
		}

		public static List<ChangesetSummary> RecentCheckins()
		{
			string path = SpruceContext.Current.CurrentProject.Path;
			var checkins = SpruceContext.Current.VersionControlServer.QueryHistory(
							path,
							VersionSpec.Latest,
							0,
							RecursionType.Full,
							null,
							new DateVersionSpec(DateTime.Now.AddDays(-7)), // version from
							VersionSpec.Latest, // version to
							int.MaxValue, // maxcount
							true,
							true,
							true);

			List<ChangesetSummary> list = new List<ChangesetSummary>();
			foreach (Changeset item in checkins)
			{
				ChangesetSummary summary = new ChangesetSummary()
				{
					Id = item.ChangesetId,
					Date = item.CreationDate,
					Message = item.Comment,
					User = item.Committer
				};

				foreach (Change change in item.Changes)
				{
					summary.Files.Add(change.Item.ArtifactUri.ToString());
				}

				list.Add(summary);
			}

			return list;
		}
	}
}