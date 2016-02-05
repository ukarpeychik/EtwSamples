namespace ManagedDecode
{
    using System;
    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Diagnostics.Tracing.Parsers;
    using Microsoft.Diagnostics.Tracing.Session;
    using SimpleEventSource;

    class Program
    {
        static void Main(string[] args)
        {
            string sessionName = "mySession";
            using (MyEventSource source = new MyEventSource())
            using (TraceEventSession session = new TraceEventSession(sessionName, Utils.GetFilePath("MyEtlFile_1.etl"))) // the null second parameter means 'real time session'
            {
                session.EnableProvider(source.Guid);
                source.String("Hello world");
                source.Int(123);
            }

            using (ETWTraceEventSource eventSource = new ETWTraceEventSource(Utils.GetFilePath("MyEtlFile_1.etl"), TraceEventSourceType.FileOnly))
            {
                DynamicTraceEventParser parser = new DynamicTraceEventParser(eventSource);
                parser.All += delegate (TraceEvent data)
                {
                    Console.WriteLine("Event name:{0}. Payload:{1}.", data.EventName, data.PayloadValue(0));
                };

                eventSource.Process();
            }
        }
    }
}
