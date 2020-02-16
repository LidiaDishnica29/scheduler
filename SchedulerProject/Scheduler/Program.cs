using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Scheduler
{
    class Program
    {
        static CancellationTokenSource m_ctSource;
        static int numHours = DateTime.Now.Hour;
        static int numMins = DateTime.Now.Minute;

        static void Main(string[] args)
        {
            Console.WriteLine("Type S for Seconds, M  for minutes, H for hours, D for days, or any other letter to stop");
            string type = Console.ReadLine();
            Console.WriteLine("Choose the start hour: (if you press an invalid letter, the default  will be 1)");
            string hou = Console.ReadLine();
            Console.WriteLine("Choose the start minute: (if you press an invalid letter, the default  will be 1)");
            string minute = Console.ReadLine();
            int h = 1;
            if (!String.IsNullOrWhiteSpace(hou)) { int.TryParse(hou, out h); }

            int m = 1;
            if (!String.IsNullOrWhiteSpace(minute)) { int.TryParse(minute, out m); }
            switch (type)
            {
                case "S":
                    // For Interval in Seconds 
                    Console.WriteLine("Choose the repetitive second: (if you press an invalid letter, the default  will be 1)");
                    string sec = Console.ReadLine();

                    int s = 1;
                    if (!String.IsNullOrWhiteSpace(sec)) { int.TryParse(sec, out s); }
                    MyScheduler.IntervalInSeconds(h, m, s,
                    () =>
                    {
                        Interval();
                    });
                    MyScheduler.IntervalInMinutes(h, m, 1,
                  () =>
                  {
                      Interval();
                  });
                    break;

                case "M":
                    Console.WriteLine("Choose the repetitive minute: (if you press an invalid letter, the default will be 1)");
                    string mn = Console.ReadLine();

                    int mt = 1;
                    if (!String.IsNullOrWhiteSpace(mn)) { int.TryParse(mn, out mt); }
                    MyScheduler.IntervalInMinutes(h, m, mt,
                    () =>
                    {
                        Interval();
                    });
                    break;

                case "H":
                    Console.WriteLine("Choose the repetitive hour: (if you press an invalid letter, the hour will be 1)");
                    string hr = Console.ReadLine();

                    int hrs = 1;
                    if (!String.IsNullOrWhiteSpace(hr)) { int.TryParse(hr, out hrs); }
                    MyScheduler.IntervalInHours(h, m, hrs,
                    () =>
                    {
                        Interval();
                    });
                    break;

                case "D":
                    Console.WriteLine("Choose the repetitive day: (if you press an invalid letter, the default will be 1)");
                    string day = Console.ReadLine();

                    int dd = 1;
                    if (!String.IsNullOrWhiteSpace(day)) { int.TryParse(day, out dd); }
                    MyScheduler.IntervalInDays(h, m, dd,
                    () =>
                    {
                        Interval();
                    });
                    break;
                default:
                    return;


            }
                    Console.ReadLine();
        }

        private static void runCodeAt(DateTime date, Scheduler scheduler)
        {
            m_ctSource = new CancellationTokenSource();

            var dateNow = DateTime.Now;
            TimeSpan ts;
            if (date > dateNow)
                ts = date - dateNow;
            else
            {
                date = getNextDate(date, scheduler);
                ts = date - dateNow;
            }
            Task.Delay(ts).ContinueWith((x) =>
            {
                methodToCall(date);
                runCodeAt(getNextDate(date, scheduler), scheduler);

            }, m_ctSource.Token);
        }

        private static DateTime getNextDate(DateTime date, Scheduler scheduler)
        {
            switch (scheduler)
            {
                case Scheduler.EveryMinutes:
                    return date.AddMinutes(1);
                case Scheduler.EveryHour:
                    return date.AddHours(1);
                case Scheduler.EveryHalfDay:
                    return date.AddHours(12);
                case Scheduler.EveryDay:
                    return date.AddDays(1);
                case Scheduler.EveryWeek:
                    return date.AddDays(7);
                case Scheduler.EveryMonth:
                    return date.AddMonths(1);
                case Scheduler.EveryYear:
                    return date.AddYears(1);
                default:
                    throw new Exception("Error");
            }

        }
        private static Scheduler getScheduler()
        {

            return Scheduler.EveryMinutes;
        }

        private static void methodToCall(DateTime time)
        {
            //setup next call
            var nextTimeToCall = getNextDate(time, getScheduler());
            var strText = string.Format("Job called at {0}. The other job is in : {1}", time, nextTimeToCall);
            Console.WriteLine(strText);
        }

        private static void Interval()
        {
            int hour = (int)numHours;
            int minutes = (int)numMins;
            var dateNow = DateTime.Now;
            var date = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, hour, minutes, 0);
            string directory = Directory.GetCurrentDirectory();
            directory = directory + @"\\XMLFile1.xml";
            List<Job> jobs = new List<Job>();

            XmlDocument xml = new XmlDocument();
            xml.Load(directory);

            XmlNodeList xnList = xml.SelectNodes("/jobs/job");

            foreach (XmlNode xn in xnList)
            {
                Job job = new Job();
                job.Id = Convert.ToInt32(xn["id"].InnerText);
                job.Description = xn["description"].InnerText;
                jobs.Add(job);
            }

            foreach (Job item in jobs)
                Console.WriteLine(item.Description);

            //get nex date the code need to run
            var nextDateValue = getNextDate(date, getScheduler());
            runCodeAt(nextDateValue, getScheduler());
        }
    }

}

