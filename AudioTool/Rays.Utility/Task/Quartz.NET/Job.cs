using Quartz;
using Quartz.Impl;
using Quartz.Simpl;
using Quartz.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rays.Utility.Job
{
    public static class Job
    {
        static Job()
        {
            //XMLSchedulingDataProcessor xMLSchedulingDataProcessor = new XMLSchedulingDataProcessor(new SimpleTypeLoadHelper());
            IScheduler scheduler = (new StdSchedulerFactory()).GetScheduler();
            //xMLSchedulingDataProcessor.ProcessFileAndScheduleJobs(@"C:\Work\PictureBook\Rays.Utility\Task\Quartz.NET\quartz_jobs.xml", scheduler);
            scheduler.Start();
        }
        public static void Start()
        {
        }
    }
}
