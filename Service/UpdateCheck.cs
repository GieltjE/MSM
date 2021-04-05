// 
// This file is a part of MSM (Multi Server Manager)
// Copyright (C) 2016-2021 Michiel Hazelhof (michiel@hazelhof.nl)
// 
// MSM is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// MSM is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using MSM.Data;
using MSM.Extends;
using MSM.Functions;
using Quartz;

namespace MSM.Service
{
    public static class UpdateCheck
    {
        public static void StartUpdateCheck()
        {
            Cron.CreateJob<UpdateCheckJob>(12, 0, 0, true);
        }
        public static void StopUpdateCheck()
        {
            Cron.RemoveJob<UpdateCheckJob>();
        }

        internal static void CheckForUpdates()
        {
            try
            {
                using WebClientOptimized webClient = new(10, false, false);
                String result;
                try
                {
                    result = webClient.DownloadString("https://api.github.com/repos/GieltjE/MSM/releases/latest");
                }
                catch (WebException exception)
                {
                    if (exception.Status == WebExceptionStatus.ProtocolError)
                    {
                        if (exception.Response is HttpWebResponse response)
                        {
                            if ((Int32)response.StatusCode == 404)
                            {
                                return;
                            }
                        }
                    }

                    UI.ShowMessage(Variables.MainForm, "Could not perform update check!", "Update check", MessageBoxIcon.Asterisk);
                    return;
                }
                    
                ReleaseInformation resultDeserialized = Statics.NewtonsoftJsonSerializer.Deserialize<ReleaseInformation>(result);
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(FileOperations.GetCurrentExecutable());
                Version currentVersion = Version.Parse(fileVersionInfo.FileVersion);

                if (resultDeserialized.tag_name > currentVersion)
                {
                    UI.ShowMessage(Variables.MainForm, "A new version (" + resultDeserialized.tag_name + ") is available!", "Update check", MessageBoxIcon.Asterisk);
                }
#if !DEBUG
                    else if(resultDeserialized.tag_name < currentVersion)
                    {
                        UI.ShowMessage(Variables.MainForm, "An older version (" + resultDeserialized.tag_name + ") is currently the latest release, please consider downgrading", "Update check", MessageBoxIcon.Asterisk);
                    }
#endif
            }
            catch (Exception exception)
            {
                Logging.LogErrorItem(exception);
            }
        }
    }

#pragma warning disable IDE1006 // Naming Styles
    // ReSharper disable InconsistentNaming
    public class Author
    {
        public String login { get; set; }
        public Int32 id { get; set; }
        public String avatar_url { get; set; }
        public String gravatar_id { get; set; }
        public String url { get; set; }
        public String html_url { get; set; }
        public String followers_url { get; set; }
        public String following_url { get; set; }
        public String gists_url { get; set; }
        public String starred_url { get; set; }
        public String subscriptions_url { get; set; }
        public String organizations_url { get; set; }
        public String repos_url { get; set; }
        public String events_url { get; set; }
        public String received_events_url { get; set; }
        public String type { get; set; }
        public Boolean site_admin { get; set; }
    }
    public class ReleaseInformation
    {
        public String url { get; set; }
        public String assets_url { get; set; }
        public String upload_url { get; set; }
        public String html_url { get; set; }
        public Int32 id { get; set; }
        public Version tag_name { get; set; }
        public String target_commitish { get; set; }
        public String name { get; set; }
        public Boolean draft { get; set; }
        public Author author { get; set; }
        public Boolean prerelease { get; set; }
        public DateTime created_at { get; set; }
        public DateTime published_at { get; set; }
        public Object[] assets { get; set; }
        public String tarball_url { get; set; }
        public String zipball_url { get; set; }
        public String body { get; set; }
    }
    // ReSharper restore InconsistentNaming
#pragma warning restore IDE1006 // Naming Styles

    [DisallowConcurrentExecution]
    internal class UpdateCheckJob : IJob
    {
        public virtual Task Execute(IJobExecutionContext context)
        {
            UpdateCheck.CheckForUpdates();
            return Task.CompletedTask;
        }
    }
}