
namespace SimpleEventSource
{
    using System;
    using System.Runtime.InteropServices;

    public class NativeMethods
    {
        /// <summary>
        /// Special const. Defined in wmistr.h
        /// </summary>
        public const uint WnodeFlagTracedGuid = 0x20000;

        /// <summary>
        /// Special const. Value for EnableTraceParameters.Version
        /// </summary>
        public const uint ENABLE_TRACE_PARAMETERS_VERSION = 1;

        /// <summary>
        /// ETW callback which is called when the trace is changed.
        /// </summary>
        /// <param name="sourceId">Provider guid.</param>
        /// <param name="isEnabled">True if the trace is enabled.</param>
        /// <param name="level">Level enabled.</param>
        /// <param name="matchAnyKeywords">Keyword filter settings.</param>
        /// <param name="matchAllKeywords">All keyword filter settings.</param>
        /// <param name="filterData">Filter descriptor pointer.</param>
        /// <param name="callbackContext">Callback context pointer.</param>
        public unsafe delegate void EtwEnableCallback(
            [In] ref Guid sourceId,
            [In] int isEnabled,
            [In] byte level,
            [In] long matchAnyKeywords,
            [In] long matchAllKeywords,
            [In] void* filterData,
            [In] void* callbackContext);

        /// <summary>
        /// See http://msdn.microsoft.com/en-us/library/aa363685.aspx for documentation. Callback when each buffer is processed.
        ///  </summary>
        /// <param name="logfile">Pointer to an EVENT_TRACE_LOGFILE structure that contains information about the buffer.</param>
        /// <returns>To continue processing events, return TRUE. Otherwise, return FALSE.
        ///  Returning FALSE will terminate the ProcessTrace function. TRUE == 1. FALSE == 0.</returns>
        public delegate uint EventTraceBufferCallback(
            [In] ref EventTraceLogfile logfile);

        /// <summary>
        /// See http://msdn.microsoft.com/en-us/library/aa363743.aspx for documentation.
        /// 
        /// Callback for processing each event.
        /// </summary>
        /// <param name="eventRecord">Pointer to an EVENT_RECORD structure that contains the event information.</param>
        public delegate void EventRecordCallback(
            [In] ref EVENT_RECORD eventRecord);

        /// <summary>
        /// See http://msdn.microsoft.com/en-us/library/aa363721.aspx for documentation.
        /// 
        /// Callback to process traces from class TraceEvent calls.
        /// </summary>
        /// <param name="buffer">Pointer to an EVENT_TRACE structure that contains the event information.</param>
        public delegate void EventTraceEventCallback(
            [In] ref EventTrace buffer);

        /// <summary>
        /// Starts the traces session.
        /// </summary>
        /// <param name="sessionHandle">Out parameter. Handle to use when talking to this sesison.</param>
        /// <param name="sessionName">Session name.</param>
        /// <param name="properties">Session control properties.</param>
        /// <returns></returns>
        [DllImport("advapi32.dll", EntryPoint = "StartTraceW", CharSet = CharSet.Unicode)]
        public static extern uint StartTrace(
            [Out] out Int64 sessionHandle,
            [In] string sessionName,
            [In, Out] ref EventTraceProperties properties);

        /// <summary>
        /// Stops the tracing session.
        /// </summary>
        /// <param name="sessionHandle">Session handle.</param>
        /// <param name="sessionName">Session name.</param>
        /// <param name="properties">Properties.</param>
        /// <returns></returns>
        [DllImport("advapi32.dll", EntryPoint = "StopTraceW", CharSet = CharSet.Unicode)]
        public static extern uint StopTrace(
            [In] Int64 sessionHandle,
            [In] string sessionName,
            [Out] out EventTraceProperties properties);

        /// <summary>
        /// Flushes etl session into the file.
        /// </summary>
        /// <param name="sessionHandle">Session handle.</param>
        /// <param name="sessionName">Session name.</param>
        /// <param name="properties">Control properties.</param>
        /// <returns></returns>
        [DllImport("advapi32.dll", EntryPoint = "FlushTraceW", CharSet = CharSet.Unicode)]
        public static extern uint FlushTrace(
            [In] Int64 sessionHandle,
            [In] string sessionName,
            [In, Out] ref EventTraceProperties properties);

        /// <summary>
        /// Writes a specific event.
        /// </summary>
        /// <param name="regHandle">Registration handle from the EventRegister.</param>
        /// <param name="eventDefinition">Event definition.</param>
        /// <param name="userDataCount">Number of EventData structures in the <paramref name="userData"/> array.</param>
        /// <param name="userData">Array of UserData structures.</param>
        /// <returns>Error code.</returns>
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        public static extern unsafe uint EventWrite(
            [In] long regHandle,
            [In] ref EventDefinition eventDefinition,
            [In] uint userDataCount,
            [In] EventData* userData);

        /// <summary>
        /// ControlTrace API. Used to get or set tracing session options. 
        /// </summary>
        /// <param name="sessionHandle">The tracing session handle, may be zero. If it is,
        /// the session-name should be specified.</param>
        /// <param name="sessionName">The tracing session name, may be null. If it is, the
        /// session-handle should be specified.</param>
        /// <param name="properties">Marshaled across to the P/Invoked API as an
        /// EVENT_TRACE_PROPERTIES structure. The structure must be correctly initialized.</param>
        /// <param name="controlCode">The type of trace-control being issues. </param>
        /// <returns>Win32 return code</returns>
        [DllImport("advapi32.dll", EntryPoint = "ControlTraceW", CharSet = CharSet.Unicode)]
        public static extern uint ControlTrace(
            [In] Int64 sessionHandle,
            [In] string sessionName,
            [In, Out] ref EventTraceProperties properties,
            [In] uint controlCode);

        /// <summary>
        /// Enable the trace provider in a specific session.
        /// </summary>
        /// <param name="sessionHandle">Handle of the session to enable the trace for.</param>
        /// <param name="providerId">Guid identifying the provider.</param>
        /// <param name="controlCode">Trace control code.</param>
        /// <param name="level">Trace level enabled.</param>
        /// <param name="matchAnyKeyword">Filter settings any keyword control.</param>
        /// <param name="matchAllKeyword">Filter settings all keyword control.</param>
        /// <param name="timeout">Trace control operation timeout.</param>
        /// <param name="enableParameters">Trace enable parameters.</param>
        /// <returns>Error code.</returns>
        [DllImport("advapi32.dll", EntryPoint = "EnableTraceEx2", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe uint EnableTraceEx2(
            [In] long sessionHandle,
            [In] ref Guid providerId,
            [In] uint controlCode,
            [In] byte level,
            [In] ulong matchAnyKeyword,
            [In] ulong matchAllKeyword,
            [In] uint timeout,
            [In] ref EnableTraceParameters enableParameters);

        /// <summary>
        /// Register the trace provider in a session.
        /// </summary>
        /// <param name="providerId">Guid provider.</param>
        /// <param name="enableCallback">Callback to call when the trace is changed.</param>
        /// <param name="callbackContext">Callback context to pass to the callback later.</param>
        /// <param name="registrationHandle">Registration handle to use for writing later.</param>
        /// <returns>Win32 return code</returns>
        [DllImport("advapi32.dll", EntryPoint = "EventRegister", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe uint EventRegister(
            [In] ref Guid providerId,
            [In] EtwEnableCallback enableCallback,
            [In] void* callbackContext,
            [In, Out] ref long registrationHandle);

        /// <summary>
        /// Unregisters a trace provider.
        /// </summary>
        /// <param name="regHandle">Registration handle aquired from EventRegister.</param>
        /// <returns>Win32 return code</returns>
        [DllImport("advapi32.dll", EntryPoint = "EventUnregister", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint EventUnregister(
            [In] long regHandle);


        [DllImport("advapi32.dll", EntryPoint = "OpenTraceW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern long OpenTrace([In] ref EventTraceLogfile logfile);

        [DllImport("advapi32.dll", EntryPoint = "ProcessTrace", CharSet = CharSet.Unicode)]
        public static extern uint ProcessTrace(
            [In] long[] handleArray,
            [In] uint handleCount,
            [In] IntPtr startTime,
            [In] IntPtr endTime);

        [DllImport("advapi32.dll", EntryPoint = "CloseTrace", CharSet = CharSet.Unicode)]
        public static extern uint CloseTrace([In] long traceHandle);

        /// <summary>
        /// The EVENT_TRACE_PROPERTIES structure used for many P/Invoked tracing
        /// API calls.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct EventTraceProperties_Inner
        {
            public WNodeHeader wnode;
            public uint bufferSize;
            public uint minimumBuffers;
            public uint maximumBuffers;
            public uint maximumFileSize;
            public uint logFileMode;
            public uint flushTimer;
            public uint enableFlags;
            public int ageLimit;
            public uint numberOfBuffers;
            public uint freeBuffers;
            public uint eventsLost;
            public uint buffersWritten;
            public uint logBuffersLost;
            public uint realTimeBuffersLost;
            public IntPtr loggerThreadId;
            public uint logFileNameOffset;
            public uint loggerNameOffset;
        }

        /// <summary>
        /// This struct is defined in wmistr.h. It contains a couple unions and
        /// therefore has to be defined as LayoutKind.Explicit.
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public struct WNodeHeader
        {
            [FieldOffset(0)] public uint bufferSize;

            [FieldOffset(4)] public uint providerId;

            [FieldOffset(8)] public uint version;

            [FieldOffset(12)] public uint linkage;

            [FieldOffset(16)] public IntPtr kernelHandle;

            [FieldOffset(24)] public Guid guid;

            [FieldOffset(40)] public uint clientContext;

            [FieldOffset(44)] public uint flags;
        }

        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
        public struct EventTraceProperties
        {
            [FieldOffset(0)]
            public EventTraceProperties_Inner etp;

            /// <summary>buffer for the logger name,
            /// offset above should point to this buffer</summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
            [FieldOffset(120)]
            public string loggerName;

            /// <summary>buffer for the logfile name,
            /// offset above should point to this buffer</summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
            [FieldOffset(2168)]
            public string logFileName;
        }

        /// <summary>
        /// Struct describing definition settings
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public struct EventData
        {
            /// <summary>
            /// Filter pointer.
            /// </summary>
            [FieldOffset(0)]
            private long ptr;

            /// <summary>
            /// Data size.
            /// </summary>
            [FieldOffset(8)]
            public int Size;

            /// <summary>
            /// Filter type.
            /// </summary>
            [FieldOffset(12)]
            public int Reserved;

            /// <summary>
            /// Gets or sets the property to hide pointer-long conversions.
            /// </summary>
            public IntPtr DataPointer
            {
                get
                {
                    return (IntPtr)this.ptr;
                }

                set
                {
                    this.ptr = (long)value;
                }
            }
        }

        /// <summary>
        /// Event descriptor.
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 16)]
        public struct EventDefinition
        {
            /// <summary>
            /// Event id
            /// </summary>
            [FieldOffset(0)]
            public ushort Id;

            /// <summary>
            /// Event verions.
            /// </summary>
            [FieldOffset(2)]
            public byte Version;

            /// <summary>
            /// Event channel.
            /// </summary>
            [FieldOffset(3)]
            public byte Channel;

            /// <summary>
            /// Event level.
            /// </summary>
            [FieldOffset(4)]
            public byte Level;

            /// <summary>
            /// Event opcode.
            /// </summary>
            [FieldOffset(5)]
            public byte Opcode;

            /// <summary>
            /// Event task.
            /// </summary>
            [FieldOffset(6)]
            public ushort Task;

            /// <summary>
            /// Event keywords.
            /// </summary>
            [FieldOffset(8)]
            public long Keyword;
        }

        [Flags]
        public enum LogFileMode : uint
        {
            EVENT_TRACE_FILE_MODE_NONE = 0x00000000,

            EVENT_TRACE_FILE_MODE_SEQUENTIAL = 0x00000001,

            EVENT_TRACE_FILE_MODE_CIRCULAR = 0x00000002,

            EVENT_TRACE_FILE_MODE_APPEND = 0x00000004,

            EVENT_TRACE_FILE_MODE_NEWFILE = 0x00000008,

            EVENT_TRACE_FILE_MODE_PREALLOCATE = 0x00000020,

            EVENT_TRACE_SECURE_MODE = 0X00000080,

            EVENT_TRACE_REAL_TIME_MODE = 0x00000100,

            EVENT_TRACE_BUFFERING_MODE = 0x00000400,

            EVENT_TRACE_PRIVATE_LOGGER_MODE = 0x00000800,

            EVENT_TRACE_USE_KBYTES_FOR_SIZE = 0x00002000,

            EVENT_TRACE_USE_GLOBAL_SEQUENCE = 0x00004000,

            EVENT_TRACE_USE_LOCAL_SEQUENCE = 0x00008000,

            EVENT_TRACE_RELOG_MODE = 0x00010000,

            EVENT_TRACE_PRIVATE_IN_PROC = 0x00020000,

            EVENT_TRACE_USE_PAGED_MEMORY = 0x01000000
        }

        /// <summary>
        /// Trace enable parameters.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct EnableTraceParameters
        {
            /// <summary>
            /// Version. Defined by the API.
            /// </summary>
            public uint Version;

            /// <summary>
            /// Enable property. Defined by the API.
            /// </summary>
            public uint EnableProperty;

            /// <summary>
            /// Trace control flags.
            /// </summary>
            public uint ControlFlags;

            /// <summary>
            /// Log provider id.
            /// </summary>
            public Guid SourceId;

            /// <summary>
            /// Filtering settings.
            /// </summary>
            public unsafe FilterDescriptor* EnableFilterDesc;
        }

        /// <summary>
        /// Struct describing filter settings.
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public struct FilterDescriptor
        {
            /// <summary>
            /// Filter pointer.
            /// </summary>
            [FieldOffset(0)]
            public long Ptr;

            /// <summary>
            /// Data size.
            /// </summary>
            [FieldOffset(8)]
            public int Size;

            /// <summary>
            /// Filter type.
            /// </summary>
            [FieldOffset(12)]
            public int Type;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct EventTraceLogfile       // _EVENT_TRACE_LOGFILEW
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string LogFileName;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string LoggerName;

            public long CurrentTime;

            public uint BuffersRead;

            public ProcessTraceMode LogFileMode;

            public EventTrace CurrentEvent;

            public TraceLogfileHeader LogfileHeader;

            public EventTraceBufferCallback BufferCallback;

            public uint BufferSize;

            public uint Filled;

            public uint EventsLost;

            public EventCallback EventCallback;

            public uint IsKernelTrace;  // TRUE for kernel logfile

            public IntPtr Context;
        }

        /// <summary>
        /// Pointer to the function that ETW calls for each event in the buffer
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        public struct EventCallback
        {
            [FieldOffset(0)]
            public EventTraceEventCallback EventTrace;

            [FieldOffset(0)]
            public EventRecordCallback EventRecord;
        }

        /// <summary>
        /// See http://msdn.microsoft.com/en-us/library/aa363773.aspx for documentation.
        /// 
        /// The EVENT_TRACE structure is used to deliver event information to an event trace consumer.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Size = 0x54)]
        public struct EventTrace            // _EVENT_TRACE
        {
            /// <summary>
            /// An EVENT_TRACE_HEADER structure that contains standard event tracing information.
            /// </summary>
            public EventTraceHeader Header;

            /// <summary>
            /// Instance identifier. Contains valid data when the provider calls the
            /// TraceEventInstance function to generate the event. Otherwise, the value is zero.
            /// </summary>
            public uint InstanceId;

            /// <summary>
            /// Instance identifier for a parent event. Contains valid data when the provider
            /// calls the TraceEventInstance function to generate the event. Otherwise, the value is zero.
            /// </summary>
            public uint ParentInstanceId;

            /// <summary>
            /// Class GUID of the parent event. Contains valid data when the provider calls the
            /// TraceEventInstance function to generate the event. Otherwise, the value is zero.
            /// </summary>
            public Guid ParentGuid;

            /// <summary>
            /// Pointer to the beginning of the event-specific data for this event.
            /// </summary>
            public IntPtr MofData; // PVOID

            /// <summary>
            /// Number of bytes to which MofData points.
            /// </summary>
            public uint MofLength;

            /// <summary>
            /// Field is Reserved
            /// </summary>
            public uint ClientContext;
        }

        [StructLayout(LayoutKind.Sequential, Size = 0x30)]
        public struct EventTraceHeader      // _EVENT_TRACE_HEADER
        {
            public ushort Size;

            /// <summary>
            /// Field is Reserved
            /// </summary>
            public ushort FieldTypeFlags;   // holds our MarkerFlags too

            public VersionUnionCore VersionUnion;

            public uint ThreadId;

            public uint ProcessId;

            public long TimeStamp; // LARGE_INTEGER

            public Guid Guid;

            public uint KernelTime;

            public uint UserTime;

            /// <summary>
            /// Union type for version.
            /// </summary>
            [StructLayout(LayoutKind.Explicit)]
            public struct VersionUnionCore
            {
                /// <summary>
                /// Uint representation of the version.
                /// </summary>
                [FieldOffset(0)]
                public uint Version;

                /// <summary>
                /// Version class representation of the version.
                /// </summary>
                [FieldOffset(0)]
                public VersionClassCore VersionClass;
            }

            /// <summary>
            /// Version class representation of the version.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct VersionClassCore
            {
                public EventTraceType Type;

                public TraceLevel Level;

                public ushort Version;
            }
        }

        public enum EventTraceType : byte
        {
            /// <summary>
            /// Informational event. This is the default event type.
            /// </summary>
            EVENT_TRACE_TYPE_INFO = 0x00,

            /// <summary>
            /// Start event. Use to trace the initial state of a multi-step event.
            /// </summary>
            EVENT_TRACE_TYPE_START = 0x01,

            /// <summary>
            /// End event. Use to trace the final state of a multi-step event.
            /// </summary>
            EVENT_TRACE_TYPE_END = 0x02,

            /// <summary>
            /// Data collection start event.
            /// </summary>
            EVENT_TRACE_TYPE_DC_START = 0x03,

            /// <summary>
            /// Data collection end event.
            /// </summary>
            EVENT_TRACE_TYPE_DC_END = 0x04,

            EVENT_TRACE_TYPE_EXTENSION = 0x05,

            EVENT_TRACE_TYPE_REPLY = 0x06,

            EVENT_TRACE_TYPE_DEQUEUE = 0x07,

            /// <summary>
            /// Checkpoint event. Use for an event that is not at the start or end of an activity.
            /// </summary>
            EVENT_TRACE_TYPE_CHECKPOINT = 0x08,
        }

        [StructLayout(LayoutKind.Sequential, Size = 0x110)]
        public struct TraceLogfileHeader      // _TRACE_LOGFILE_HEADER
        {
            /// <summary>
            /// Size of the event tracing session's buffers, in kilobytes.
            /// </summary>
            public uint BufferSize;

            public uint Version;

            /// <summary>
            /// Build number of the operating system.
            /// </summary>
            public uint ProviderVersion;

            /// <summary>
            /// Number of processors on the system.
            /// </summary>
            public uint NumberOfProcessors;

            public long EndTime;

            public uint TimerResolution;

            /// <summary>
            /// Maximum size of the log file, in megabytes.
            /// </summary>
            public uint MaximumFileSize;

            /// <summary>
            /// Current logging mode for the event tracing session.
            /// </summary>
            public LogFileMode LogFileMode;

            /// <summary>
            /// Total number of buffers written by the event tracing session.
            /// </summary>
            public uint BuffersWritten;

            /// <summary>
            /// Field is Reserved
            /// </summary>
            public Guid LogInstanceGuid;

            public IntPtr LoggerName;   

            public IntPtr LogFileName;  

            public TIME_ZONE_INFORMATION TimeZone;

            public long BootTime;

            /// <summary>
            /// Frequency of the high-resolution performance counter, if one exists.
            /// </summary>
            public long PerfFreq;

            /// <summary>
            /// Time at which the event tracing session started, in 100-nanosecond intervals since
            /// midnight, January 1, 1601.
            /// </summary>
            public long StartTime;

            /// <summary>
            /// Specifies the clock type. For details, see the ClientContext member of WNODE_HEADER.
            /// </summary>
            public uint ReservedFlags;

            /// <summary>
            /// Total number of buffers lost during the event tracing session.
            /// </summary>
            public uint BuffersLost;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct TIME_ZONE_INFORMATION : IEquatable<TIME_ZONE_INFORMATION>
        {
            public int Bias;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string StandardName;

            public SYSTEMTIME StandardDate;

            public int StandardBias;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DaylightName;

            public SYSTEMTIME DaylightDate;

            public int DaylightBias;

            public static bool operator ==(TIME_ZONE_INFORMATION v1, TIME_ZONE_INFORMATION v2)
            {
                return v1.Equals(v2);
            }

            /// <summary>
            /// Inequality operator
            /// </summary>
            /// <param name="v1">TIME_ZONE_INFORMATION 1</param>
            /// <param name="v2">TIME_ZONE_INFORMATION 2</param>
            /// <returns>True if v1 and v2 do not represent the same date and time; otherwise, false.</returns>
            public static bool operator !=(TIME_ZONE_INFORMATION v1, TIME_ZONE_INFORMATION v2)
            {
                return !v1.Equals(v2);
            }

            /// <summary>
            /// Returns a value that indicates whether this instance is equal to a specified object.
            /// </summary>
            /// <param name="value">A TIME_ZONE_INFORMATION instance to compare to this instance.</param>
            /// <returns>True if the value parameter equals the value of this instance; otherwise, false. </returns>
            public bool Equals(TIME_ZONE_INFORMATION value)
            {
                return this.Bias == value.Bias
                    && this.DaylightBias == value.DaylightBias
                    && this.DaylightDate == value.DaylightDate
                    && this.StandardBias == value.StandardBias
                    && this.StandardDate == value.StandardDate;
            }

            /// <summary>
            /// Returns a value that indicates whether this instance is equal to a specified object.
            /// </summary>
            /// <param name="obj">An object instance to compare to this instance.</param>
            /// <returns>True if the value parameter equals the value of this instance; otherwise, false. </returns>
            public override bool Equals(object obj)
            {
                if (obj is TIME_ZONE_INFORMATION)
                {
                    return this.Equals((TIME_ZONE_INFORMATION)obj);
                }
                else
                {
                    return false;
                }
            }

            /// <summary>
            /// Returns the hash code for this instance.
            /// </summary>
            /// <returns>Hash code for this instance.</returns>
            public override int GetHashCode()
            {
                return this.Bias
                    ^ this.DaylightBias
                    ^ this.DaylightDate.GetHashCode()
                    ^ this.StandardBias
                    ^ this.StandardDate.GetHashCode();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEMTIME : IEquatable<SYSTEMTIME>
        {
            /// <summary>
            /// The year. The valid values for this member are 1601 through 30827.
            /// </summary>
            public ushort Year;

            public ushort Month;

            public ushort DayOfWeek;

            /// <summary>
            /// The day of the month. The valid values for this member are 1 through 31.
            /// </summary>
            public ushort Day;

            /// <summary>
            /// The hour. The valid values for this member are 0 through 23.
            /// </summary>
            public ushort Hour;

            /// <summary>
            /// The minute. The valid values for this member are 0 through 59.
            /// </summary>
            public ushort Minute;

            /// <summary>
            /// The second. The valid values for this member are 0 through 59.
            /// </summary>
            public ushort Second;

            /// <summary>
            /// The millisecond. The valid values for this member are 0 through 999.
            /// </summary>
            public ushort Milliseconds;

            /// <summary>
            /// Initializes a new instance of the SYSTEMTIME struct
            /// </summary>
            /// <param name="year">Sets the Year member.</param>
            /// <param name="month">Sets the Month member.</param>
            /// <param name="dayOfWeek">Sets the DayOfWeek member.</param>
            /// <param name="day">Sets the Day member.</param>
            /// <param name="hour">Sets the Hour member.</param>
            /// <param name="minute">Sets the Minute member.</param>
            /// <param name="second">Sets the Second member.</param>
            /// <param name="milliseconds">Sets the Milliseconds member.</param>
            public SYSTEMTIME(
                ushort year,
                ushort month,
                ushort dayOfWeek,
                ushort day,
                ushort hour,
                ushort minute,
                ushort second,
                ushort milliseconds)
            {
                this.Year = year;
                this.Month = month;
                this.DayOfWeek = dayOfWeek;
                this.Day = day;
                this.Hour = hour;
                this.Minute = minute;
                this.Second = second;
                this.Milliseconds = milliseconds;
            }

            /// <summary>
            /// Initializes a new instance of the SYSTEMTIME struct
            /// </summary>
            /// <param name="dateTime">DateTime parameter</param>
            public SYSTEMTIME(DateTime dateTime)
                : this(
                    (ushort)dateTime.Year,
                    (ushort)dateTime.Month,
                    (ushort)dateTime.DayOfWeek,
                    (ushort)dateTime.Day,
                    (ushort)dateTime.Hour,
                    (ushort)dateTime.Minute,
                    (ushort)dateTime.Second,
                    (ushort)dateTime.Millisecond)
            {
            }

            /// <summary>
            /// Equality operator=
            /// </summary>
            /// <param name="v1">SYSTEMTIME 1</param>
            /// <param name="v2">SYSTEMTIME 2</param>
            /// <returns>True if v1 and v2 represent the same date and time; otherwise, false.</returns>
            public static bool operator ==(SYSTEMTIME v1, SYSTEMTIME v2)
            {
                return v1.Equals(v2);
            }

            /// <summary>
            /// Inequality operator
            /// </summary>
            /// <param name="v1">SYSTEMTIME 1</param>
            /// <param name="v2">SYSTEMTIME 2</param>
            /// <returns>True if v1 and v2 do not represent the same date and time; otherwise, false.</returns>
            public static bool operator !=(SYSTEMTIME v1, SYSTEMTIME v2)
            {
                return !v1.Equals(v2);
            }

            /// <summary>
            /// Returns a value that indicates whether this instance is equal to a specified object.
            /// </summary>
            /// <param name="value">A SYSTEMTIME instance to compare to this instance.</param>
            /// <returns>True if the value parameter equals the value of this instance; otherwise, false. </returns>
            public bool Equals(SYSTEMTIME value)
            {
                return this.Milliseconds == value.Milliseconds
                    && this.Second == value.Second
                    && this.Minute == value.Minute
                    && this.Hour == value.Hour
                    && this.Day == value.Day
                    && this.DayOfWeek == value.DayOfWeek
                    && this.Month == value.Month
                    && this.Year == value.Year;
            }

            /// <summary>
            /// Returns a value that indicates whether this instance is equal to a specified object.
            /// </summary>
            /// <param name="obj">An object instance to compare to this instance.</param>
            /// <returns>True if the value parameter equals the value of this instance; otherwise, false. </returns>
            public override bool Equals(object obj)
            {
                if (obj is SYSTEMTIME)
                {
                    return this.Equals((SYSTEMTIME)obj);
                }
                else
                {
                    return false;
                }
            }

            /// <summary>
            /// Returns the hash code for this instance.
            /// </summary>
            /// <returns>Hash code for this instance.</returns>
            public override int GetHashCode()
            {
                return (int)((uint)this.Hour << 28
                        | (uint)this.Minute << 20
                        | (uint)this.Second << 12
                        | (ushort)this.Milliseconds);
            }
        }

        public enum ProcessTraceMode : uint
        {
            EVENT_TRACE_FILE_MODE_NONE = 0x00000000,

            EVENT_TRACE_FILE_MODE_SEQUENTIAL = 0x00000001,

            EVENT_TRACE_FILE_MODE_CIRCULAR = 0x00000002,

            EVENT_TRACE_FILE_MODE_APPEND = 0x00000004,

            EVENT_TRACE_FILE_MODE_NEWFILE = 0x00000008,

            EVENT_TRACE_FILE_MODE_PREALLOCATE = 0x00000020,

            EVENT_TRACE_SECURE_MODE = 0X00000080,

            EVENT_TRACE_REAL_TIME_MODE = 0x00000100,

            EVENT_TRACE_BUFFERING_MODE = 0x00000400,

            EVENT_TRACE_PRIVATE_LOGGER_MODE = 0x00000800,

            EVENT_TRACE_USE_KBYTES_FOR_SIZE = 0x00002000,

            EVENT_TRACE_USE_GLOBAL_SEQUENCE = 0x00004000,

            EVENT_TRACE_USE_LOCAL_SEQUENCE = 0x00008000,

            EVENT_TRACE_RELOG_MODE = 0x00010000,

            EVENT_TRACE_PRIVATE_IN_PROC = 0x00020000,

            EVENT_TRACE_USE_PAGED_MEMORY = 0x01000000,

            PROCESS_TRACE_MODE_EVENT_RECORD = 0x10000000,

            PROCESS_TRACE_MODE_RAW_TIMESTAMP = 0x00001000
        }

        /// <summary>
        /// EVENT_RECORD structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct EVENT_RECORD
        {
            /// <summary>Event header</summary>
            public EVENT_HEADER EventHeader;

            /// <summary>Buffer context</summary>
            public ETW_BUFFER_CONTEXT BufferContext;

            /// <summary>Extended data count</summary>
            public ushort ExtendedDataCount;

            /// <summary>User data length</summary>
            public ushort UserDataLength;

            /// <summary>Extended data</summary>
            public IntPtr ExtendedData;

            /// <summary>Event user data</summary>
            public IntPtr UserData;

            /// <summary>User context</summary>
            public IntPtr UserContext;
        }

        /// <summary>ETW_BUFFER_CONTEXT structure</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct ETW_BUFFER_CONTEXT
        {
            /// <summary>Processor number</summary>
            internal byte ProcessorNumber;

            /// <summary>Alignment value</summary>
            internal byte Alignment;

            /// <summary>ETW Logger id</summary>
            internal ushort LoggerId;
        }

        /// <summary>EVENT_HEADER structure</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct EVENT_HEADER
        {
            /// <summary>Size of the struct</summary>
            public ushort Size;

            /// <summary>Header type</summary>
            public ushort HeaderType;

            /// <summary>Header flags</summary>
            public EVENT_HEADER_FLAGS Flags;

            /// <summary>Event property</summary>
            public ushort EventProperty;

            /// <summary>Event thread id</summary>
            public uint ThreadId;

            /// <summary>Event process id</summary>
            public uint ProcessId;

            /// <summary>Event time stamp</summary>
            public ulong TimeStamp;

            /// <summary>Event provider ID</summary>
            public Guid ProviderId;

            /// <summary>Event descriptor</summary>
            public EVENT_DESCRIPTOR EventDescriptor;

            /// <summary>Event kernel time</summary>
            public uint KernelTime;

            /// <summary>Event user time</summary>
            public uint UserTime;

            /// <summary>Event activity ID</summary>
            public Guid ActivityId;
        }

        /// <summary>EVENT_DESCRIPTOR structure</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct EVENT_DESCRIPTOR
        {
            /// <summary>Event ID value.</summary>
            internal ushort Id;

            /// <summary>Event version.</summary>
            internal byte Version;

            /// <summary>Event channel</summary>
            internal byte Channel;

            /// <summary>Event level</summary>
            internal byte Level;

            /// <summary>Event opcode</summary>
            internal byte Opcode;

            /// <summary>Event task</summary>
            internal ushort Task;

            /// <summary>Event keyword</summary>
            internal ulong Keyword;
        }

        /// <summary>EVENT_HEADER_FLAGS Flags</summary>
        [Flags]
        public enum EVENT_HEADER_FLAGS : ushort
        {
            /// <summary>EVENT_HEADER_FLAG_EXTENDED_INFO flag</summary>
            EVENT_HEADER_FLAG_EXTENDED_INFO = 0x0001,

            /// <summary>EVENT_HEADER_FLAG_PRIVATE_SESSION flag</summary>
            EVENT_HEADER_FLAG_PRIVATE_SESSION = 0x0002,

            /// <summary>EVENT_HEADER_FLAG_STRING_ONLY flag</summary>
            EVENT_HEADER_FLAG_STRING_ONLY = 0x0004,

            /// <summary>EVENT_HEADER_FLAG_TRACE_MESSAGE flag</summary>
            EVENT_HEADER_FLAG_TRACE_MESSAGE = 0x0008,

            /// <summary>EVENT_HEADER_FLAG_NO_CPUTIME flag</summary>
            EVENT_HEADER_FLAG_NO_CPUTIME = 0x0010,

            /// <summary>EVENT_HEADER_FLAG_32_BIT_HEADER flag</summary>
            EVENT_HEADER_FLAG_32_BIT_HEADER = 0x0020,

            /// <summary>EVENT_HEADER_FLAG_64_BIT_HEADER flag</summary>
            EVENT_HEADER_FLAG_64_BIT_HEADER = 0x0040,

            /// <summary>EVENT_HEADER_FLAG_CLASSIC_HEADER flag</summary>
            EVENT_HEADER_FLAG_CLASSIC_HEADER = 0x0100,
        }

        /// <summary>
        /// Control codes for ControlTrace calls.
        /// </summary>
        public enum ControlTraceCode : uint
        {
            Query = 0,
            Stop = 1,
            Update = 2,
            Flush = 3
        }

        /// <summary>
        /// Control codes for EnableTraceEx2 calls.
        /// </summary>
        public enum EnableTraceControlCode : uint
        {
            Disable = 0,
            Enable = 1
        }

        /// <summary>
        /// Trace level constants.
        /// </summary>
        public enum TraceLevel : byte
        {
            Critical = 1,
            Error = 2,
            Warning = 3,
            Information = 4,
            Verbose = 5
        }
    }
}
