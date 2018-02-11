// 
// This file is a part of MSM (Multi Server Manager)
// Copyright (C) 2016-2018 Michiel Hazelhof (michiel@hazelhof.nl)
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
using Quartz.Simpl;

namespace MSM.Service
{
    public class Cron
    {
        internal Cron()
        {
            Events.ShutDownFired += ShutDown;
            Run();
        }
        private async void Run()
        {
            NameValueCollection properties = new NameValueCollection
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
        internal async void ShutDown()
        {
            await Scheduler.Shutdown(false);
        }
        internal IScheduler Scheduler;

        // Prevent concurrent _scheduler operations so we don't crash
        private readonly SemaphoreSlim _schedulerSemaphore = new SemaphoreSlim(1, 1);

        // 0/5 means 0, 5, 10, .. 1/5 means 1, 6, 11 .. 2/5 means 2, 7, 12
        // http://quartz-scheduler.org/documentation/quartz-2.2.x/tutorials/crontrigger
        public async void CreateJob<T>(String second, String minute, String hour, String dayOfMonth = "*", Enumerations.CronMonth month = Enumerations.CronMonth.All, Enumerations.CronDayOfTheWeek dayOfweek = Enumerations.CronDayOfTheWeek.All, String year = "*", Boolean immediatlyFireAfterMisfire = true) where T : IJob
        {
            if (Scheduler.IsShutdown) return;

            await _schedulerSemaphore.WaitAsync();

            String name = typeof(T).Namespace + typeof(T).Name;
            Task<Boolean> hasJob = HasJob(name, false);
            hasJob.Wait();
            if (hasJob.Result)
            {
                _schedulerSemaphore.Release();
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
                    Logging.LogErrorItem(exception);
                }
            }
            catch (Exception exception)
            {
                Logging.LogErrorItem(exception);
            }
            finally
            {
                _schedulerSemaphore.Release(1);
            }
        }
        public async void CreateJob<T>(Int32 intervalHours, Int32 intervalMinutes, Int32 intervalSeconds, Boolean fireImmediately) where T : IJob
        {
            if (Scheduler.IsShutdown) return;

            await _schedulerSemaphore.WaitAsync();

            String name = typeof(T).Namespace + typeof(T).Name;
            Task<Boolean> hasJob = HasJob(name, false);
            hasJob.Wait();
            if (hasJob.Result)
            {
                _schedulerSemaphore.Release();
                return;
            }

            DateTimeOffset startFireingAfter = DateBuilder.NextGivenSecondDate(null, 2);
            if (!fireImmediately)
            {
                startFireingAfter = DateBuilder.FutureDate(intervalSeconds + (intervalMinutes * 60) + (intervalHours * 3600), IntervalUnit.Second);
            }
            try
            {
                await Scheduler.ScheduleJob(JobBuilder.Create<T>().WithIdentity(name).Build(), TriggerBuilder.Create().WithIdentity(name).WithSimpleSchedule(x => x.RepeatForever().WithIntervalInSeconds(intervalSeconds + (intervalMinutes * 60) + (intervalHours * 3600)).WithMisfireHandlingInstructionFireNow()).StartAt(startFireingAfter).Build());
            }
            catch (Exception exception)
            {
                Logging.LogErrorItem(exception);
            }
            finally
            {
                _schedulerSemaphore.Release();
            }
        }
        public async void TriggerJob<T>()
        {
            if (Scheduler.IsShutdown) return;

            await _schedulerSemaphore.WaitAsync();

            String name = typeof(T).Namespace + typeof(T).Name;
            Task<Boolean> hasJob = HasJob(name, false);
            hasJob.Wait();
            if (!hasJob.Result)
            {
                _schedulerSemaphore.Release();
                return;
            }

            try
            {
                await Scheduler.TriggerJob(new JobKey(name));
            }
            catch (Exception exception)
            {
                Logging.LogErrorItem(exception);
            }
            finally
            {
                _schedulerSemaphore.Release();
            }
        }
        public async void RemoveJob<T>()
        {
            if (Scheduler.IsShutdown) return;

            await _schedulerSemaphore.WaitAsync();

            try
            {
                String name = typeof(T).Namespace + typeof(T).Name;
                Task<Boolean> hasJob = HasJob(name, false);
                hasJob.Wait();
                if (!hasJob.Result) return;

                await Scheduler.DeleteJob(new JobKey(name));
            }
            catch { }
            finally
            {
                _schedulerSemaphore.Release();
            }
        }
        public Boolean IsRunning<T>()
        {
            if (Scheduler.IsShutdown) return false;

            _schedulerSemaphore.WaitAsync();

            try
            {
                String name = typeof(T).Namespace + typeof(T).Name;
                Task<Boolean> hasJob = HasJob(name, false);
                hasJob.Wait();
                if (!hasJob.Result) return false;

                if (Scheduler.GetCurrentlyExecutingJobs().Result.Any(job => String.Equals(job.Trigger.Key.Name, name)))
                {
                    return true;
                }
            }
            catch { }
            finally
            {
                _schedulerSemaphore.Release();
            }

            return false;
        }
        public Task<Boolean> HasJob<T>()
        {
            return HasJob(typeof(T).Namespace + typeof(T).Name);
        }
        private async Task<Boolean> HasJob(String triggerIdentity, Boolean performLock = true)
        {
            if (Scheduler.IsShutdown) return false;

            if (performLock)
            {
                await _schedulerSemaphore.WaitAsync();
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
            catch {}
            finally
            {
                if (performLock)
                {
                    _schedulerSemaphore.Release();
                }
            }

            return false;
        }
    }

    public class ExceptionOccuredJobListener : IJobListener
    {
        public String Name => "ExceptionOccuredJobListener";

        public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.CompletedTask;
        }
        public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.CompletedTask;
        }
        public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = new CancellationToken())
        {
            if (jobException != null)
            {
                Logging.LogErrorItem(jobException.GetBaseException());
            }
            return Task.CompletedTask;
        }
    }
}