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
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;
using MSM.Data;
using MSM.Extends;
using MSM.Functions;
using Quartz;

namespace MSM.Service
{
    public static class UpdateCheck
    {
        public static void StartUpdateCronJob() => Cron.CreateJob<UpdateCheckJob>("UpdateJob", 12, 0, 0, true);
        public static void TriggerUpdateCheckJob() => Cron.TriggerJob<UpdateCheckJob>("UpdateJob");
        public static void StopUpdateCheck() => Cron.RemoveJob<UpdateCheckJob>("UpdateJob");
        public static Boolean HasUpdateCheck()
        {
            Task<Boolean> task = Cron.HasJob<UpdateCheckJob>("UpdateJob");
            task.Wait();
            return task.Result;
        }

        internal static void CheckForUpdates()
        {
            try
            {
                using WebClientOptimized webClient = new(10, false);
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

                    UI.ShowWarning(Variables.MainForm, "Could not perform update check!", "Update check", MessageBoxIcon.Asterisk);
                    return;
                }
                    
                ReleaseInformation resultDeserialized = Statics.NewtonsoftJsonSerializer.Deserialize<ReleaseInformation>(result);
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(FileOperations.GetCurrentExecutable());
                Version currentVersion = Version.Parse(fileVersionInfo.FileVersion);
                String architecturePostFix = "-x86";
                if (Environment.Is64BitProcess)
                {
                    architecturePostFix = "-x64";
                }
                Asset asset = resultDeserialized.assets.FirstOrDefault(x => x.browser_download_url.EndsWith(architecturePostFix + ".zip", StringComparison.Ordinal));

#if !DEBUG
                if(resultDeserialized.tag_name < currentVersion)
                {
                    UI.ShowMessage(Variables.MainForm, "An older version (" + resultDeserialized.tag_name + ") is currently the latest release, please consider downgrading", "Update check", MessageBoxIcon.Asterisk);
                }
#endif

                if (resultDeserialized.tag_name <= currentVersion || asset == null) return;

                String updateDirectory = Path.Combine(FileOperations.GetRunningDirectory(), "update");
                if (Directory.Exists(updateDirectory))
                {
                    while (FileOperations.DeleteDirectory(updateDirectory, true).Any())
                    {
                        Thread.Sleep(500);
                    }
                }

                if (UI.AskQuestion(Variables.MainForm, "A new version (" + resultDeserialized.tag_name + ") is available, would you like to upgrade?", "Upgrade available", MessageBoxButtons.YesNo, MessageBoxDefaultButton.Button1, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                if (!FileOperations.CreateDirectory(updateDirectory)) return;

                MemoryStream stream = new(webClient.DownloadData(asset.browser_download_url));
                FastZip fastZip = new();
                fastZip.ExtractZip(stream, updateDirectory, FastZip.Overwrite.Always, null, null, null, true, true);

                ProcessStartInfo procInfo = new(Path.Combine(updateDirectory, Path.GetFileName(FileOperations.GetCurrentExecutable())))
                {
                    WorkingDirectory = updateDirectory,
                    Arguments = "--update",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Maximized,
                    UseShellExecute = false
                };
                Process.Start(procInfo);

                // Prevent closing our updated version......
                Statics.InformationObjectManager?.Dispose();

                Events.ShutDown();
                Environment.Exit(1001);
            }
            catch (Exception exception)
            {
                Logger.Log(Enumerations.LogTarget.General, Enumerations.LogLevel.Error, "Could not check for updates", exception);
            }
        }
    }

#pragma warning disable IDE1006 // Naming Styles
    // ReSharper disable InconsistentNaming
    public class ReleaseInformation
    {
        public Version tag_name { get; set; }
        public Asset[] assets { get; set; }
    }
    public class Asset
    {
        public String browser_download_url { get; set; }
    }
    // ReSharper restore InconsistentNaming
#pragma warning restore IDE1006 // Naming Styles

    [DisallowConcurrentExecution, ResetTimerAfterRunCompletes]
    internal class UpdateCheckJob : IJob
    {
        public virtual Task Execute(IJobExecutionContext context)
        {
            UpdateCheck.CheckForUpdates();
            return Task.CompletedTask;
        }
    }
}
