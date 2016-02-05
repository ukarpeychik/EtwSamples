namespace EtwSamples
{
    using System;
    using System.Diagnostics.Tracing;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Diagnostics.Tracing.Parsers;
    using Microsoft.Diagnostics.Tracing.Session;
    using SimpleEventSource;

    class Program
    {
        private static readonly Guid providerGuid = Guid.NewGuid();
        private static void Main(string[] args)
        {
            /*using (MyEventListener myListener = new MyEventListener())
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
            }*/


            /*string sessionName = "MySession";
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
            }*/

            string sessionName = "myEtlSession";
            NativeMethods.EventTraceProperties properties = Program.GetProperties(
                sessionName,
                Program.providerGuid,
                @"C:\Users\ulka\Documents\MyEtlFile_%d.etl");
                //Path.Combine(Assembly.GetExecutingAssembly().Location, "MyEtlFile1_%d.etl"));
            long sessionHandle = 0;
            long eventRegisterHandle = 0;
            try
            {
                uint errorCode = NativeMethods.StartTrace(out sessionHandle, sessionName, ref properties);
                // This call associates a specific trace provider with an etl session.
                NativeMethods.EnableTraceParameters enableParameters = new NativeMethods.EnableTraceParameters();
                enableParameters.Version = NativeMethods.ENABLE_TRACE_PARAMETERS_VERSION;
                enableParameters.SourceId = Program.providerGuid;
                enableParameters.ControlFlags = 0;

                unsafe
                {
                    Guid providerGuid = Program.providerGuid;
                    errorCode = NativeMethods.EventRegister(ref providerGuid, null, null, ref eventRegisterHandle);
                    errorCode = NativeMethods.EnableTraceEx2(
                        sessionHandle,
                        ref providerGuid,
                        (uint) NativeMethods.EnableTraceControlCode.Enable,
                        (byte) NativeMethods.TraceLevel.Verbose,
                        0,
                        0,
                        1000,
                        ref enableParameters);
                    string myData = "Hello world";
                    int dataLength = Encoding.ASCII.GetByteCount(myData) + sizeof (byte);
                    byte[] buffer = new byte[dataLength];
                    Encoding.ASCII.GetBytes(myData, 0, myData.Length, buffer, 0);
                    fixed (byte* nativeBuffer = buffer)
                    {
                        NativeMethods.EventDefinition eventDefinition = new NativeMethods.EventDefinition();
                        NativeMethods.EventData* eventData = stackalloc NativeMethods.EventData[1];
                        eventData[0].DataPointer = (IntPtr) nativeBuffer;
                        eventData[0].Size = dataLength;
                        errorCode = NativeMethods.EventWrite(
                            eventRegisterHandle,
                            ref eventDefinition,
                            1,
                            eventData);
                    }
                }
            }
            finally
            {
                uint errorCode = NativeMethods.EventUnregister(eventRegisterHandle);
                errorCode = NativeMethods.ControlTrace(
                    0, // sessionHandle
                    sessionName,
                    ref properties,
                    (uint) NativeMethods.ControlTraceCode.Stop);

            }

            NativeMethods.EventTraceLogfile eventTraceLogfile = new NativeMethods.EventTraceLogfile();
            eventTraceLogfile.LogFileName = @"C:\Users\ulka\Documents\MyEtlFile_1.etl";
            eventTraceLogfile.LogFileMode = NativeMethods.ProcessTraceMode.PROCESS_TRACE_MODE_EVENT_RECORD;
            eventTraceLogfile.BufferCallback = BufferCallback;
            eventTraceLogfile.EventCallback.EventRecord = EventRecord;
            long traceHandle = -1;
            try
            {
                traceHandle = NativeMethods.OpenTrace(ref eventTraceLogfile);
                uint hresult = NativeMethods.ProcessTrace(new long[] {traceHandle}, 1, IntPtr.Zero, IntPtr.Zero);
            }
            finally
            {
                NativeMethods.CloseTrace(traceHandle);
            }

        }

        private static void EventRecord(ref NativeMethods.EVENT_RECORD eventRecord)
        {
            if (eventRecord.EventHeader.ProviderId == Program.providerGuid)
            {
                byte[] buffer = new byte[12];
                Marshal.Copy(eventRecord.UserData, buffer, 0, buffer.Length);
                Console.WriteLine(Encoding.ASCII.GetString(buffer));
            }
        }

        private static uint BufferCallback(ref NativeMethods.EventTraceLogfile logfile)
        {
            return 1;
        }

        public static NativeMethods.EventTraceProperties GetProperties(string sessionName, Guid providerGuid, string path)
        {
            NativeMethods.EventTraceProperties properties = new NativeMethods.EventTraceProperties();
            NativeMethods.LogFileMode fileMode = NativeMethods.LogFileMode.EVENT_TRACE_FILE_MODE_NEWFILE;

            properties.etp.wnode.bufferSize = (uint)Marshal.SizeOf(properties);
            properties.etp.wnode.guid = providerGuid;
            properties.etp.wnode.flags = NativeMethods.WnodeFlagTracedGuid;
            properties.etp.wnode.clientContext = 1; // Query performance counter for time stamp
            properties.etp.bufferSize = 64;
            properties.etp.minimumBuffers = 10;
            properties.etp.maximumBuffers = 10;
            properties.etp.maximumFileSize = 10*1024;
            properties.etp.logFileMode =
                (uint)(fileMode |
                       NativeMethods.LogFileMode.EVENT_TRACE_USE_PAGED_MEMORY |
                       NativeMethods.LogFileMode.EVENT_TRACE_USE_LOCAL_SEQUENCE);
            properties.etp.flushTimer = (uint)10;
            properties.etp.enableFlags = 0;
            properties.etp.logFileNameOffset = (uint)Marshal.OffsetOf(typeof(NativeMethods.EventTraceProperties), "logFileName");
            properties.etp.loggerNameOffset = (uint)Marshal.OffsetOf(typeof(NativeMethods.EventTraceProperties), "loggerName");
            properties.logFileName = path;
            properties.loggerName = sessionName;

            return properties;
        }
    }
}
