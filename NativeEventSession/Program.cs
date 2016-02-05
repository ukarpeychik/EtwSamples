namespace NativeEventSession
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using SimpleEventSource;

    class Program
    {
        static void Main(string[] args)
        {
            string sessionName = "myEtlSession";
            Guid providerGuid = Guid.NewGuid();
            NativeMethods.EventTraceProperties properties = Program.GetProperties(
                sessionName,
                providerGuid,
              Utils.GetFilePath("MyEtlFile_%d.etl"));
            long sessionHandle = 0;
            long eventRegisterHandle = 0;
            try
            {
                Utils.RunWithErrorCodeCheck(() => NativeMethods.StartTrace(out sessionHandle, sessionName, ref properties));
                // This call associates a specific trace provider with an etl session.
                NativeMethods.EnableTraceParameters enableParameters = new NativeMethods.EnableTraceParameters();
                enableParameters.Version = NativeMethods.ENABLE_TRACE_PARAMETERS_VERSION;
                enableParameters.SourceId = providerGuid;
                enableParameters.ControlFlags = 0;

                unsafe
                {
                    Utils.RunWithErrorCodeCheck(() => NativeMethods.EventRegister(ref providerGuid, null, null, ref eventRegisterHandle));
                    Utils.RunWithErrorCodeCheck(() => NativeMethods.EnableTraceEx2(
                        sessionHandle,
                        ref providerGuid,
                        (uint) NativeMethods.EnableTraceControlCode.Enable,
                        (byte) NativeMethods.TraceLevel.Verbose,
                        0,
                        0,
                        1000,
                        ref enableParameters));
                    string myData = "Hello world";
                    int dataLength = Encoding.ASCII.GetByteCount(myData) + sizeof(byte);
                    byte[] buffer = new byte[dataLength];
                    Encoding.ASCII.GetBytes(myData, 0, myData.Length, buffer, 0);
                    fixed (byte* nativeBuffer = buffer)
                    {
                        NativeMethods.EventDefinition eventDefinition = new NativeMethods.EventDefinition();
                        NativeMethods.EventData* eventData = stackalloc NativeMethods.EventData[1];
                        eventData[0].DataPointer = (IntPtr)nativeBuffer;
                        eventData[0].Size = dataLength;
                        Utils.RunWithErrorCodeCheck(() => NativeMethods.EventWrite(
                            eventRegisterHandle,
                            ref eventDefinition,
                            1,
                            eventData));
                    }
                }
            }
            finally
            {
                Utils.RunWithErrorCodeCheck(() => NativeMethods.EventUnregister(eventRegisterHandle));
                Utils.RunWithErrorCodeCheck(() => NativeMethods.ControlTrace(
                    0, // sessionHandle
                    sessionName,
                    ref properties,
                    (uint) NativeMethods.ControlTraceCode.Stop));

            }
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
            properties.etp.maximumFileSize = 10 * 1024;
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
