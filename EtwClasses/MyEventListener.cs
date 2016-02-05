
namespace SimpleEventSource
{
    using System;
    using System.Diagnostics.Tracing;
    using System.Text;

    public class MyEventListener : EventListener
    {
        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            StringBuilder builder = new StringBuilder(50);
            builder.AppendFormat("Event {0}. Payload:", eventData.EventId);
            foreach (object arg in eventData.Payload)
            {
                builder.Append(arg);
            }
            Console.WriteLine(builder.ToString());
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            Console.WriteLine("New event source created.");
        }
    }
}
