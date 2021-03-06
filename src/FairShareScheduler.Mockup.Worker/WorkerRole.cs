// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
using bma.Cloud;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;

namespace FairShareScheduler.Mockup
{
    public class WorkerRole : RoleEntryPoint
    {
        private IWorker worker;

        private Stream DoJob(Guid jobId, Stream input)
        {
            Trace.TraceInformation("Doing the job {0}", jobId);
            var sw = new Stopwatch();
            sw.Start();

            var reader = new StreamReader(input);
            var input_s = reader.ReadToEnd();
            var query = JObject.Parse(input_s);

            JToken value;
            if(query.TryGetValue("failworker", out value) && value.Value<bool>())
            {
                Trace.TraceInformation("Query requests to fail the worker");
                var t = new Thread(() =>
                {
                    throw new Exception("Query requests to fail the worker");
                });
                t.IsBackground = false;
                t.Start();
                t.Join();
            }

            var sleep = query["sleep"].Value<int>();
            Thread.Sleep(sleep);
            var output_s = new JObject(new JProperty("result", query["result"].ToString()));

            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(output_s);
            writer.Flush();
            ms.Position = 0;

            sw.Stop();
            Trace.TraceInformation("Job {0} done ({1})", jobId, sw.Elapsed);

            return ms;
        }


        public override void Run()
        {
            Trace.TraceInformation("FairShareScheduler.Mockup.Worker is running");

            worker.Process(DoJob, TimeSpan.FromSeconds(0.1), TimeSpan.FromSeconds(10.0));
        }

        public override bool OnStart()
        {
            ServicePointManager.DefaultConnectionLimit = 12;

            bool result = base.OnStart();


            try
            {
                var storageAccount = CloudStorageAccount.DevelopmentStorageAccount;
                var schedulerName = "testscenarios";

                if (result)
                {
                    var settings = new WorkerSettings(TimeSpan.FromSeconds(10), 2, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1));
                    worker = Worker.Create(storageAccount, schedulerName, settings);
                    Trace.TraceInformation("FairShareScheduler.Mockup.Worker has been started");
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Exception during OnStart: {0}", ex);
                result = false;
            }

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("FairShareScheduler.Mockup.Worker is stopping");

            base.OnStop();

            Trace.TraceInformation("FairShareScheduler.Mockup.Worker has stopped");
        }

    }
}
