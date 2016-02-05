namespace SimpleEventSource
{
    using System;
    using System.Diagnostics.Tracing;

    public class MyEventSource : EventSource
    {
        public void String(string val)
        {
            this.WriteEvent(1, val);
        }

        public void Int(int val)
        {
            this.WriteEvent(2, val);
        }

        [Event(eventId: 3)]
        public void Bool(bool val)
        {
            this.WriteEvent(3, val);
        }
    }
}
