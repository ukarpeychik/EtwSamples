namespace ManagedEventSession
{
    using System;
    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Diagnostics.Tracing.Parsers;
    using Microsoft.Diagnostics.Tracing.Session;
    using SimpleEventSource;

    internal class Program
    {
        private static void Main(string[] args)
        {
            string sessionName = "mySession";
            using (MyEventSource source = new MyEventSource())
            using (TraceEventSession session = new TraceEventSession(sessionName, null)) // the null second parameter means 'real time session'
            using (ETWTraceEventSource eventSource = new ETWTraceEventSource(sessionName, TraceEventSourceType.Session))
            {
                DynamicTraceEventParser parser = new DynamicTraceEventParser(eventSource);
                parser.All += delegate(TraceEvent data)
                {
                    Console.WriteLine("Event name:{0}. Payload:{1}.", data.EventName, data.PayloadValue(0));
                };

                session.EnableProvider(source.Guid);
                source.String("Hello world");
                source.Int(123);
                eventSource.Process();
            }
        }
    }
}
