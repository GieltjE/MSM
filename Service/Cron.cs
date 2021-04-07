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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MSM.Data;
using MSM.Functions;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Quartz.Impl.Triggers;
using Quartz.Simpl;
using Quartz.Util;

namespace MSM.Service
{
    public static class Cron
    {
        static Cron()
        {
            Events.ShutDownFired += ShutDown;
            Run();
        }
        private static async void Run()
        {
            NameValueCollection properties = new()
            {
                ["quartz.threadPool.type"] = typeof(DefaultThreadPool).AssemblyQualifiedName,
                ["quartz.jobStore.type"] = typeof(RAMJobStore).AssemblyQualifiedName,
                ["quartz.jobStore.misfireThreshold"] = "1000",
                ["quartz.serializer.type"] = "binary"
            };

            Scheduler = await new StdSchedulerFactory(properties).GetScheduler();
            Scheduler.ListenerManager.AddJobListener(new ExceptionOccuredJobListener(), GroupMatcher<JobKey>.AnyGroup());
            await Scheduler.Start();
        }
        internal static async void ShutDown()
        {
            await Scheduler.Shutdown(false);
        }
        internal static IScheduler Scheduler;

        // Prevent concurrent _scheduler operations so we don't crash
        private static readonly SemaphoreSlim SchedulerSemaphore = new(1, 1);

        // 0/5 means 0, 5, 10, .. 1/5 means 1, 6, 11 .. 2/5 means 2, 7, 12
        // http://quartz-scheduler.org/documentation/quartz-2.2.x/tutorials/crontrigger
        public static async void CreateJob<T>(String identifier, String second, String minute, String hour, String dayOfMonth = "*", Enumerations.CronMonth month = Enumerations.CronMonth.All, Enumerations.CronDayOfTheWeek dayOfweek = Enumerations.CronDayOfTheWeek.All, String year = "*", Boolean immediatlyFireAfterMisfire = true) where T : IJob
        {
            if (Scheduler.IsShutdown) return;

            await SchedulerSemaphore.WaitAsync();

            String name = GetSafeName<T>(identifier);
            Task<Boolean> hasJob = HasJob(name, false);
            hasJob.Wait();
            if (hasJob.Result)
            {
                SchedulerSemaphore.Release();
                return;
            }

            if (dayOfweek != Enumerations.CronDayOfTheWeek.All)
            {
                dayOfMonth = "?";
            }

            IEnumerable<Enumerations.CronDayOfTheWeek> days = dayOfweek.GetAllFlags();
            String dayOfweek1 = days.Aggregate("", (current, value) => current + "," + Enumerations.CronDayOfTheWeekString[(Int32)value]).TrimStart(',');

            try
            {
                CronScheduleBuilder cronScheduleBuilder = CronScheduleBuilder.CronSchedule(second + " " + minute + " " + hour + " " + dayOfMonth + " " + month.GetAllFlags().Aggregate("", (current, value) => current + "," + Enumerations.CronMonthString[(Int32)value]).TrimStart(',') + " " + dayOfweek1 + " " + year);
                if (immediatlyFireAfterMisfire)
                {
                    cronScheduleBuilder.WithMisfireHandlingInstructionFireAndProceed();
                }
                else
                {
                    cronScheduleBuilder.WithMisfireHandlingInstructionDoNothing();
                }
                try
                {
                    await Scheduler.ScheduleJob(JobBuilder.Create<T>().WithIdentity(name).Build(), TriggerBuilder.Create().WithIdentity(name).ForJob(name).WithSchedule(cronScheduleBuilder).StartNow().Build());
                }
                catch (Exception exception)
                {
                    Logger.Log(Enumerations.LogTarget.General, Enumerations.LogLevel.Fatal, "Cron failure (" + name + ")", exception);
                }
            }
            catch (Exception exception)
            {
                Logger.Log(Enumerations.LogTarget.General, Enumerations.LogLevel.Fatal, "Cron failure (" + name + ")", exception);
            }
            finally
            {
                SchedulerSemaphore.Release(1);
            }
        }
        public static async void CreateJob<T>(String identifier, Int32 intervalHours, Int32 intervalMinutes, Int32 intervalSeconds, Boolean fireImmediately, Boolean refireImmediately = true) where T : IJob
        {
            if (Scheduler.IsShutdown) return;

            await SchedulerSemaphore.WaitAsync();

            String name = GetSafeName<T>(identifier);
            Task<Boolean> hasJob = HasJob(name, false);
            hasJob.Wait();
            if (hasJob.Result)
            {
                SchedulerSemaphore.Release();
                return;
            }

            DateTimeOffset startFireingAfter = DateBuilder.NextGivenSecondDate(null, 2);
            if (!fireImmediately)
            {
                startFireingAfter = DateBuilder.FutureDate(intervalSeconds + (intervalMinutes * 60) + (intervalHours * 3600), IntervalUnit.Second);
            }
            try
            {
                TriggerBuilder trigger = TriggerBuilder.Create().WithIdentity(name);
                if (refireImmediately)
                {
                    trigger.WithSimpleSchedule(x => x.RepeatForever().WithIntervalInSeconds(intervalSeconds + (intervalMinutes * 60) + (intervalHours * 3600)).WithMisfireHandlingInstructionFireNow());
                }
                else
                {
                    trigger.WithSimpleSchedule(x => x.RepeatForever().WithIntervalInSeconds(intervalSeconds + (intervalMinutes * 60) + (intervalHours * 3600)).WithMisfireHandlingInstructionNextWithExistingCount());
                }

                await Scheduler.ScheduleJob(JobBuilder.Create<T>().WithIdentity(name).Build(), trigger.StartAt(startFireingAfter).Build());
            }
            catch (Exception exception)
            {
                Logger.Log(Enumerations.LogTarget.General, Enumerations.LogLevel.Fatal, "Cron failure (" + name + ")", exception);
            }
            finally
            {
                SchedulerSemaphore.Release();
            }
        }
        public static async void TriggerJob<T>(String identifier)
        {
            if (Scheduler.IsShutdown) return;

            await SchedulerSemaphore.WaitAsync();

            String name = GetSafeName<T>(identifier);
            Task<Boolean> hasJob = HasJob(name, false);
            hasJob.Wait();
            if (!hasJob.Result)
            {
                SchedulerSemaphore.Release();
                return;
            }

            try
            {
                await Scheduler.TriggerJob(new JobKey(name));
            }
            catch (Exception exception)
            {
                Logger.Log(Enumerations.LogTarget.General, Enumerations.LogLevel.Fatal, "Cron failure (" + name + ")", exception);
            }
            finally
            {
                SchedulerSemaphore.Release();
            }
        }
        public static async void RemoveJob<T>(String identifier)
        {
            if (Scheduler.IsShutdown) return;

            await SchedulerSemaphore.WaitAsync();

            try
            {
                String name = GetSafeName<T>(identifier);
                Task<Boolean> hasJob = HasJob(name, false);
                hasJob.Wait();
                if (!hasJob.Result) return;

                await Scheduler.DeleteJob(new JobKey(name));
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch {}
            finally
            {
                SchedulerSemaphore.Release();
            }
        }
        public static Boolean IsRunning<T>(String identifier)
        {
            if (Scheduler.IsShutdown) return false;

            SchedulerSemaphore.WaitAsync();

            try
            {
                String name = GetSafeName<T>(identifier);
                Task<Boolean> hasJob = HasJob(name, false);
                hasJob.Wait();
                if (!hasJob.Result) return false;

                if (Scheduler.GetCurrentlyExecutingJobs().Result.Any(job => String.Equals(job.Trigger.Key.Name, name)))
                {
                    return true;
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch { }
            finally
            {
                SchedulerSemaphore.Release();
            }

            return false;
        }
        public static Task<Boolean> HasJob<T>(String identifier)
        {
            return HasJob(GetSafeName<T>(identifier));
        }
        private static async Task<Boolean> HasJob(String triggerIdentity, Boolean performLock = true)
        {
            if (Scheduler.IsShutdown) return false;

            if (performLock)
            {
                await SchedulerSemaphore.WaitAsync();
            }

            try
            {
                Task<IJobDetail> jobDetail = Scheduler.GetJobDetail(new JobKey(triggerIdentity));
                jobDetail.Wait();
                if (jobDetail.Result != null)
                {
                    return true;
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch {}
            finally
            {
                if (performLock)
                {
                    SchedulerSemaphore.Release();
                }
            }

            return false;
        }

        private static String GetSafeName<T>(String identifier)
        {
            return typeof(T).Namespace + "-" + typeof(T).Name + "-" + identifier;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ResetTimerAfterRunCompletes : Attribute {}

    public class ExceptionOccuredJobListener : IJobListener
    {
        public String Name => "ExceptionOccuredJobListener";

        public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = new()) => Task.CompletedTask;
        public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = new()) => Task.CompletedTask;
        public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = new())
        {
            if (ObjectUtils.IsAttributePresent(context.JobDetail.JobType, typeof(ResetTimerAfterRunCompletes)))
            {
                TriggerKey triggerName = new(context.Trigger.Key.Name, ((AbstractTrigger)context.Trigger).Group);

                Task<ITrigger> trigger = context.Scheduler.GetTrigger(triggerName, cancellationToken);
                trigger.Wait(cancellationToken);

                TriggerBuilder newTrigger = trigger.Result.GetTriggerBuilder();
                newTrigger.StartAt(DateBuilder.FutureDate((Int32)((SimpleTriggerImpl)trigger.Result).RepeatInterval.TotalSeconds, IntervalUnit.Second));
                context.Scheduler.RescheduleJob(triggerName, newTrigger.Build(), cancellationToken);
            }

            if (jobException != null)
            {
                Logger.Log(Enumerations.LogTarget.General, Enumerations.LogLevel.Fatal, "Cron failure (" + context.Trigger.Key.Name + ")", jobException);
            }
            return Task.CompletedTask;
        }
    }
}