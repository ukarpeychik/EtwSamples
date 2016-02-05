namespace ManagedEventSourceAndListener
{
    using System.Diagnostics.Tracing;
    using SimpleEventSource;

    internal class Program
    {
        private static void Main(string[] args)
        {
            using (MyEventListener myListener = new MyEventListener())
            using (MyEventSource source = new MyEventSource())
            {
                myListener.EnableEvents(source, EventLevel.Verbose);
                if (source.ConstructionException != null)
                {
                    throw source.ConstructionException;
                }

                source.String("Hello world");
                source.Int(10);
                source.Bool(true);
            }
        }
    }
}
