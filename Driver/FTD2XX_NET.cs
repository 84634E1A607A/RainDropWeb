using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using static System.String;

namespace RainDropWeb.Driver;

/// <summary>
///     Class wrapper for FTD2XX.DLL
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "InvertIf")]
public class Ftdi
{
    #region VARIABLES

    // Create private variables for the device within the class
    private nint _ftHandle = nint.Zero;

    #endregion

    public Ftdi()
    {
        switch (Environment.OSVersion.Platform)
        {
            case PlatformID.Win32S:
            case PlatformID.WinCE:
            case PlatformID.Win32NT:
            case PlatformID.Win32Windows:
            {
                break;
            }

            case PlatformID.Unix:
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "lsmod",
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true
                });

                process?.WaitForExit();
                if (process == null) throw new Exception("lsmod not found");

                if (process.StandardOutput.ReadToEnd().Contains("ftdi_sio"))
                    throw new Exception("Found conflicting ftdi_sio module. Please remove it.");

                break;
            }

            case PlatformID.MacOSX:
            {
                break;
            }

            default:
            {
                throw new PlatformNotSupportedException();
            }
        }
    }

    #region HELPER_METHODS

    //**************************************************************************
    // ErrorHandler
    //**************************************************************************
    /// <summary>
    ///     Method to check ftStatus and ftErrorCondition values for error conditions and throw exceptions accordingly.
    /// </summary>
    private void ErrorHandler(FtStatus ftStatus, FtError ftErrorCondition)
    {
        if (ftStatus != FtStatus.FtOk)
            // Check FT_STATUS values returned from FTD2XX DLL calls
            switch (ftStatus)
            {
                case FtStatus.FtDeviceNotFound:
                {
                    throw new FtException("FTDI device not found.");
                }
                case FtStatus.FtDeviceNotOpened:
                {
                    throw new FtException("FTDI device not opened.");
                }
                case FtStatus.FtDeviceNotOpenedForErase:
                {
                    throw new FtException("FTDI device not opened for erase.");
                }
                case FtStatus.FtDeviceNotOpenedForWrite:
                {
                    throw new FtException("FTDI device not opened for write.");
                }
                case FtStatus.FtFailedToWriteDevice:
                {
                    throw new FtException("Failed to write to FTDI device.");
                }
                case FtStatus.FtInsufficientResources:
                {
                    throw new FtException("Insufficient resources.");
                }
                case FtStatus.FtInvalidArgs:
                {
                    throw new FtException("Invalid arguments for FTD2XX function call.");
                }
                case FtStatus.FtInvalidBaudRate:
                {
                    throw new FtException("Invalid Baud rate for FTDI device.");
                }
                case FtStatus.FtInvalidHandle:
                {
                    throw new FtException("Invalid handle for FTDI device.");
                }
                case FtStatus.FtInvalidParameter:
                {
                    throw new FtException("Invalid parameter for FTD2XX function call.");
                }
                case FtStatus.FtIoError:
                {
                    throw new FtException("FTDI device IO error.");
                }
                case FtStatus.FtOtherError:
                {
                    throw new FtException(
                        "An unexpected error has occurred when trying to communicate with the FTDI device.");
                }
            }

        if (ftErrorCondition != FtError.FtNoError)
            // Check for other error conditions not handled by FTD2XX DLL
            switch (ftErrorCondition)
            {
                case FtError.FtIncorrectDevice:
                {
                    throw new FtException("The current device type does not match the EEPROM structure.");
                }
                case FtError.FtInvalidBitMode:
                {
                    throw new FtException("The requested bit mode is not valid for the current device.");
                }
                case FtError.FtBufferSize:
                {
                    throw new FtException("The supplied buffer is not big enough.");
                }
            }
    }

    #endregion

    #region TYPEDEFS

    /// <summary>
    ///     Type that holds device information for GetDeviceInformation method.
    ///     Used with FT_GetDeviceInfo and FT_GetDeviceInfoDetail in FTD2XX.DLL
    /// </summary>
    public class FtDeviceInfoNode
    {
        /// <summary>
        ///     The device description
        /// </summary>
        [JsonInclude] public string Description = Empty;

        /// <summary>
        ///     Indicates device state.  Can be any combination of the following: FT_FLAGS_OPENED, FT_FLAGS_HI_SPEED
        /// </summary>
        [JsonInclude] public uint Flags;

        /// <summary>
        ///     The device handle.  This value is not used externally and is provided for information only.
        ///     If the device is not open, this value is 0.
        /// </summary>
        public nint FtHandle;

        /// <summary>
        ///     The Vendor ID and Product ID of the device
        /// </summary>
        [JsonInclude] public uint Id;

        /// <summary>
        ///     The physical location identifier of the device
        /// </summary>
        [JsonInclude] public uint LocId;

        /// <summary>
        ///     The device serial number
        /// </summary>
        [JsonInclude] public string SerialNumber = Empty;

        /// <summary>
        ///     Indicates the device type.  Can be one of the following: FT_DEVICE_232R, FT_DEVICE_2232C, FT_DEVICE_BM,
        ///     FT_DEVICE_AM, FT_DEVICE_100AX or FT_DEVICE_UNKNOWN
        /// </summary>
        [JsonInclude] public FtDevice Type;
    }

    #endregion

    #region EXCEPTION_HANDLING

    /// <summary>
    ///     Exceptions thrown by errors within the FTDI class.
    /// </summary>
    [Serializable]
    public class FtException : Exception
    {
        /// <summary>
        /// </summary>
        public FtException()
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="message"></param>
        public FtException(string message) : base(message)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public FtException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected FtException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }

    #endregion

    #region NativeFunctions

    private const string DllName = "libftd2xx";

    // Definitions for FTD2XX functions
    [DllImport(DllName)]
    private static extern FtStatus FT_CreateDeviceInfoList(ref uint numDevices);

    [DllImport(DllName)]
    private static extern FtStatus FT_GetDeviceInfoDetail(uint index, ref uint flags, ref FtDevice chipType,
        ref uint id, ref uint locId, byte[] serialNumber, byte[] description, ref nint ftHandle);

    [DllImport(DllName)]
    private static extern FtStatus FT_Open(uint index, ref nint ftHandle);

    [DllImport(DllName)]
    private static extern FtStatus FT_OpenEx(string devString, uint dwFlags, ref nint ftHandle);

    [DllImport(DllName)]
    private static extern FtStatus FT_OpenExLoc(uint devLoc, uint dwFlags, ref nint ftHandle);

    [DllImport(DllName)]
    private static extern FtStatus FT_Close(nint ftHandle);

    [DllImport(DllName)]
    private static extern FtStatus FT_Read(nint ftHandle, byte[] lpBuffer, uint dwBytesToRead,
        ref uint lpdwBytesReturned);

    [DllImport(DllName)]
    private static extern FtStatus FT_Write(nint ftHandle, byte[] lpBuffer, uint dwBytesToWrite,
        ref uint lpdwBytesWritten);

    [DllImport(DllName)]
    private static extern FtStatus FT_GetQueueStatus(nint ftHandle, ref uint lpdwAmountInRxQueue);

    [DllImport(DllName)]
    private static extern FtStatus FT_GetModemStatus(nint ftHandle, ref uint lpdwModemStatus);

    [DllImport(DllName)]
    private static extern FtStatus FT_GetStatus(nint ftHandle, ref uint lpdwAmountInRxQueue,
        ref uint lpdwAmountInTxQueue, ref uint lpdwEventStatus);

    [DllImport(DllName)]
    private static extern FtStatus FT_SetBaudRate(nint ftHandle, uint dwBaudRate);

    [DllImport(DllName)]
    private static extern FtStatus FT_SetDataCharacteristics(nint ftHandle, byte uWordLength, byte uStopBits,
        byte uParity);

    [DllImport(DllName)]
    private static extern FtStatus FT_SetFlowControl(nint ftHandle, ushort usFlowControl, byte uXon, byte uXOff);

    [DllImport(DllName)]
    private static extern FtStatus FT_SetDtr(nint ftHandle);

    [DllImport(DllName)]
    private static extern FtStatus FT_ClrDtr(nint ftHandle);

    [DllImport(DllName)]
    private static extern FtStatus FT_SetRts(nint ftHandle);

    [DllImport(DllName)]
    private static extern FtStatus FT_ClrRts(nint ftHandle);

    [DllImport(DllName)]
    private static extern FtStatus FT_ResetDevice(nint ftHandle);

    [DllImport(DllName)]
    private static extern FtStatus FT_ResetPort(nint ftHandle);

    [DllImport(DllName)]
    private static extern FtStatus FT_CyclePort(nint ftHandle);

    [DllImport(DllName)]
    private static extern FtStatus FT_Rescan();

    [DllImport(DllName)]
    private static extern FtStatus FT_Reload(ushort wVid, ushort wPid);

    [DllImport(DllName)]
    private static extern FtStatus FT_Purge(nint ftHandle, uint dwMask);

    [DllImport(DllName)]
    private static extern FtStatus FT_SetTimeouts(nint ftHandle, uint dwReadTimeout, uint dwWriteTimeout);

    [DllImport(DllName)]
    private static extern FtStatus FT_SetBreakOn(nint ftHandle);

    [DllImport(DllName)]
    private static extern FtStatus FT_SetBreakOff(nint ftHandle);

    [DllImport(DllName)]
    private static extern FtStatus FT_GetDeviceInfo(nint ftHandle, ref FtDevice pftType, ref uint lpdwId,
        byte[] pcSerialNumber, byte[] pcDescription, nint pvDummy);

    [DllImport(DllName)]
    private static extern FtStatus FT_SetResetPipeRetryCount(nint ftHandle, uint dwCount);

    [DllImport(DllName)]
    private static extern FtStatus FT_StopInTask(nint ftHandle);

    [DllImport(DllName)]
    private static extern FtStatus FT_RestartInTask(nint ftHandle);

    [DllImport(DllName)]
    private static extern FtStatus FT_GetDriverVersion(nint ftHandle, ref uint lpdwDriverVersion);

    [DllImport(DllName)]
    private static extern FtStatus FT_GetLibraryVersion(ref uint lpdwLibraryVersion);

    [DllImport(DllName)]
    private static extern FtStatus FT_SetDeadmanTimeout(nint ftHandle, uint dwDeadmanTimeout);

    [DllImport(DllName)]
    private static extern FtStatus FT_SetChars(nint ftHandle, byte uEventCh, byte uEventChEn, byte uErrorCh,
        byte uErrorChEn);

    [DllImport(DllName)]
    private static extern FtStatus FT_SetEventNotification(nint ftHandle, uint dwEventMask, SafeHandle hEvent);

    [DllImport(DllName)]
    private static extern FtStatus FT_GetComPortNumber(nint ftHandle, ref int dwComPortNumber);

    [DllImport(DllName)]
    private static extern FtStatus FT_SetLatencyTimer(nint ftHandle, byte ucLatency);

    [DllImport(DllName)]
    private static extern FtStatus FT_GetLatencyTimer(nint ftHandle, ref byte ucLatency);

    [DllImport(DllName)]
    private static extern FtStatus FT_SetBitMode(nint ftHandle, byte ucMask, byte ucMode);

    [DllImport(DllName)]
    private static extern FtStatus FT_GetBitMode(nint ftHandle, ref byte ucMode);

    [DllImport(DllName)]
    private static extern FtStatus FT_SetUSBParameters(nint ftHandle, uint dwInTransferSize,
        uint dwOutTransferSize);

    [DllImport(DllName)]
    private static extern FtStatus FT_VendorCmdGet(nint ftHandle, ushort request, byte[] buf, ushort len);

    [DllImport(DllName)]
    private static extern FtStatus FT_VendorCmdSet(nint ftHandle, ushort request, byte[] buf, ushort len);

    [DllImport(DllName)]
    private static extern FtStatus FT_VendorCmdSetX(nint ftHandle, ushort request, byte[] buf, ushort len);

    #endregion

    #region CONSTANT_VALUES

    // Constants for FT_STATUS
    /// <summary>
    ///     Status values for FTDI devices.
    /// </summary>
    public enum FtStatus
    {
        /// <summary>
        ///     Status OK
        /// </summary>
        FtOk = 0,

        /// <summary>
        ///     The device handle is invalid
        /// </summary>
        FtInvalidHandle,

        /// <summary>
        ///     Device not found
        /// </summary>
        FtDeviceNotFound,

        /// <summary>
        ///     Device is not open
        /// </summary>
        FtDeviceNotOpened,

        /// <summary>
        ///     IO error
        /// </summary>
        FtIoError,

        /// <summary>
        ///     Insufficient resources
        /// </summary>
        FtInsufficientResources,

        /// <summary>
        ///     A parameter was invalid
        /// </summary>
        FtInvalidParameter,

        /// <summary>
        ///     The requested baud rate is invalid
        /// </summary>
        FtInvalidBaudRate,

        /// <summary>
        ///     Device not opened for erase
        /// </summary>
        FtDeviceNotOpenedForErase,

        /// <summary>
        ///     Device not opened for write
        /// </summary>
        FtDeviceNotOpenedForWrite,

        /// <summary>
        ///     Failed to write to device
        /// </summary>
        FtFailedToWriteDevice,

        /// <summary>
        ///     Invalid arguments
        /// </summary>
        FtInvalidArgs,

        /// <summary>
        ///     An other error has occurred
        /// </summary>
        FtOtherError
    }

    // Constants for other error states internal to this class library
    /// <summary>
    ///     Error states not supported by FTD2XX DLL.
    /// </summary>
    private enum FtError
    {
        FtNoError = 0,
        FtIncorrectDevice,
        FtInvalidBitMode,
        FtBufferSize
    }

    // Flags for FT_OpenEx
    private const uint FtOpenBySerialNumber = 0x00000001;
    private const uint FtOpenByDescription = 0x00000002;
    private const uint FtOpenByLocation = 0x00000004;

    // Word Lengths
    /// <summary>
    ///     Permitted data bits for FTDI devices
    /// </summary>
    public static class FtDataBits
    {
        /// <summary>
        ///     8 data bits
        /// </summary>
        public const byte FtBits8 = 0x08;

        /// <summary>
        ///     7 data bits
        /// </summary>
        public const byte FtBits7 = 0x07;
    }

    // Stop Bits
    /// <summary>
    ///     Permitted stop bits for FTDI devices
    /// </summary>
    public static class FtStopBits
    {
        /// <summary>
        ///     1 stop bit
        /// </summary>
        public const byte FtStopBits1 = 0x00;

        /// <summary>
        ///     2 stop bits
        /// </summary>
        public const byte FtStopBits2 = 0x02;
    }

    // Parity
    /// <summary>
    ///     Permitted parity values for FTDI devices
    /// </summary>
    public static class FtParity
    {
        /// <summary>
        ///     No parity
        /// </summary>
        public const byte FtParityNone = 0x00;

        /// <summary>
        ///     Odd parity
        /// </summary>
        public const byte FtParityOdd = 0x01;

        /// <summary>
        ///     Even parity
        /// </summary>
        public const byte FtParityEven = 0x02;

        /// <summary>
        ///     Mark parity
        /// </summary>
        public const byte FtParityMark = 0x03;

        /// <summary>
        ///     Space parity
        /// </summary>
        public const byte FtParitySpace = 0x04;
    }

    // Flow Control
    /// <summary>
    ///     Permitted flow control values for FTDI devices
    /// </summary>
    public static class FtFlowControl
    {
        /// <summary>
        ///     No flow control
        /// </summary>
        public const ushort FtFlowNone = 0x0000;

        /// <summary>
        ///     RTS/CTS flow control
        /// </summary>
        public const ushort FtFlowRtsCts = 0x0100;

        /// <summary>
        ///     DTR/DSR flow control
        /// </summary>
        public const ushort FtFlowDtrDsr = 0x0200;

        /// <summary>
        ///     Xon/Xoff flow control
        /// </summary>
        public const ushort FtFlowXOnXOff = 0x0400;
    }

    // Purge Rx and Tx buffers
    /// <summary>
    ///     Purge buffer constant definitions
    /// </summary>
    public static class FtPurge
    {
        /// <summary>
        ///     Purge Rx buffer
        /// </summary>
        public const byte FtPurgeRx = 0x01;

        /// <summary>
        ///     Purge Tx buffer
        /// </summary>
        public const byte FtPurgeTx = 0x02;
    }

    // Modem Status bits
    /// <summary>
    ///     Modem status bit definitions
    /// </summary>
    public static class FtModemStatus
    {
        /// <summary>
        ///     Clear To Send (CTS) modem status
        /// </summary>
        public const byte FtCts = 0x10;

        /// <summary>
        ///     Data Set Ready (DSR) modem status
        /// </summary>
        public const byte FtDsr = 0x20;

        /// <summary>
        ///     Ring Indicator (RI) modem status
        /// </summary>
        public const byte FtRi = 0x40;

        /// <summary>
        ///     Data Carrier Detect (DCD) modem status
        /// </summary>
        public const byte FtDcd = 0x80;
    }

    // Line Status bits
    /// <summary>
    ///     Line status bit definitions
    /// </summary>
    public static class FtLineStatus
    {
        /// <summary>
        ///     Overrun Error (OE) line status
        /// </summary>
        public const byte FtOe = 0x02;

        /// <summary>
        ///     Parity Error (PE) line status
        /// </summary>
        public const byte FtPe = 0x04;

        /// <summary>
        ///     Framing Error (FE) line status
        /// </summary>
        public const byte FtFe = 0x08;

        /// <summary>
        ///     Break Interrupt (BI) line status
        /// </summary>
        public const byte FtBi = 0x10;
    }

    // Events
    /// <summary>
    ///     FTDI device event types that can be monitored
    /// </summary>
    public static class FtEvents
    {
        /// <summary>
        ///     Event on receive character
        /// </summary>
        public const uint FtEventRxChar = 0x00000001;

        /// <summary>
        ///     Event on modem status change
        /// </summary>
        public const uint FtEventModemStatus = 0x00000002;

        /// <summary>
        ///     Event on line status change
        /// </summary>
        public const uint FtEventLineStatus = 0x00000004;
    }

    // Bit modes
    /// <summary>
    ///     Permitted bit mode values for FTDI devices.  For use with SetBitMode
    /// </summary>
    public static class FtBitModes
    {
        /// <summary>
        ///     Reset bit mode
        /// </summary>
        public const byte FtBitModeReset = 0x00;

        /// <summary>
        ///     Asynchronous bit-bang mode
        /// </summary>
        public const byte FtBitModeAsyncBitBang = 0x01;

        /// <summary>
        ///     MPSSE bit mode - only available on FT2232, FT2232H, FT4232H and FT232H
        /// </summary>
        public const byte FtBitModeMpsse = 0x02;

        /// <summary>
        ///     Synchronous bit-bang mode
        /// </summary>
        public const byte FtBitModeSyncBitBang = 0x04;

        /// <summary>
        ///     MCU host bus emulation mode - only available on FT2232, FT2232H, FT4232H and FT232H
        /// </summary>
        public const byte FtBitModeMcuHost = 0x08;

        /// <summary>
        ///     Fast opto-isolated serial mode - only available on FT2232, FT2232H, FT4232H and FT232H
        /// </summary>
        public const byte FtBitModeFastSerial = 0x10;

        /// <summary>
        ///     CBUS bit-bang mode - only available on FT232R and FT232H
        /// </summary>
        public const byte FtBitModeCbusBitBang = 0x20;

        /// <summary>
        ///     Single channel synchronous 245 FIFO mode - only available on FT2232H channel A and FT232H
        /// </summary>
        public const byte FtBitModeSyncFifo = 0x40;
    }

    // Flag values for FT_GetDeviceInfoDetail and FT_GetDeviceInfo
    /// <summary>
    ///     Flags that provide information on the FTDI device state
    /// </summary>
    public class FtFlags
    {
        /// <summary>
        ///     Indicates that the device is open
        /// </summary>
        public const uint FtFlagsOpened = 0x00000001;

        /// <summary>
        ///     Indicates that the device is enumerated as a hi-speed USB device
        /// </summary>
        public const uint FtFlagsHiSpeed = 0x00000002;
    }

    // Valid drive current values for FT2232H, FT4232H and FT232H devices
    /// <summary>
    ///     Valid values for drive current options on FT2232H, FT4232H and FT232H devices.
    /// </summary>
    public class FtDriveCurrent
    {
        /// <summary>
        ///     4mA drive current
        /// </summary>
        public const byte FtDriveCurrent4Ma = 4;

        /// <summary>
        ///     8mA drive current
        /// </summary>
        public const byte FtDriveCurrent8Ma = 8;

        /// <summary>
        ///     12mA drive current
        /// </summary>
        public const byte FtDriveCurrent12Ma = 12;

        /// <summary>
        ///     16mA drive current
        /// </summary>
        public const byte FtDriveCurrent16Ma = 16;
    }

    // Device type identifiers for FT_GetDeviceInfoDetail and FT_GetDeviceInfo
    /// <summary>
    ///     List of FTDI device types
    /// </summary>
    public enum FtDevice
    {
        /// <summary>
        ///     FT232B or FT245B device
        /// </summary>
        FtDeviceBm = 0,

        /// <summary>
        ///     FT8U232AM or FT8U245AM device
        /// </summary>
        FtDeviceAm,

        /// 1
        /// <summary>
        ///     FT8U100AX device
        /// </summary>
        FtDevice100Ax,

        /// <summary>
        ///     Unknown device
        /// </summary>
        FtDeviceUnknown,

        /// <summary>
        ///     FT2232 device
        /// </summary>
        FtDevice2232,

        /// <summary>
        ///     FT232R or FT245R device
        /// </summary>
        FtDevice232R,

        /// 5
        /// <summary>
        ///     FT2232H device
        /// </summary>
        FtDevice2232H,

        /// 6
        /// <summary>
        ///     FT4232H device
        /// </summary>
        FtDevice4232H,

        /// 7
        /// <summary>
        ///     FT232H device
        /// </summary>
        FtDevice232H,

        /// 8
        /// <summary>
        ///     FT X-Series device
        /// </summary>
        FtDeviceXSeries,

        /// 9
        /// <summary>
        ///     FT4222 hi-speed device Mode 0 - 2 interfaces
        /// </summary>
        FtDevice4222H0,

        /// 10
        /// <summary>
        ///     FT4222 hi-speed device Mode 1 or 2 - 4 interfaces
        /// </summary>
        FtDevice4222H12,

        /// 11
        /// <summary>
        ///     FT4222 hi-speed device Mode 3 - 1 interface
        /// </summary>
        FtDevice4222H3,

        /// 12
        /// <summary>
        ///     OTP programmer board for the FT4222.
        /// </summary>
        FtDevice4222Prog,

        /// 13
        /// <summary>
        ///     OTP programmer board for the FT900.
        /// </summary>
        FtDeviceFt900,

        /// 14
        /// <summary>
        ///     OTP programmer board for the FT930.
        /// </summary>
        FtDeviceFt930,

        /// 15
        /// <summary>
        ///     Flash programmer board for the UMFTPD3A.
        /// </summary>
        FtDeviceUmftpd3A,

        /// 16
        /// <summary>
        ///     FT2233HP hi-speed device.
        /// </summary>
        FtDevice2233Hp,

        /// 17
        /// <summary>
        ///     FT4233HP hi-speed device.
        /// </summary>
        FtDevice4233Hp,

        /// 18
        /// <summary>
        ///     FT2233HP hi-speed device.
        /// </summary>
        FtDevice2232Hp,

        /// 19
        /// <summary>
        ///     FT4233HP hi-speed device.
        /// </summary>
        FtDevice4232Hp,

        /// 20
        /// <summary>
        ///     FT233HP hi-speed device.
        /// </summary>
        FtDevice233Hp,

        /// 21
        /// <summary>
        ///     FT232HP hi-speed device.
        /// </summary>
        FtDevice232Hp,

        /// 22
        /// <summary>
        ///     FT2233HA hi-speed device.
        /// </summary>
        FtDevice2232Ha,

        /// 23
        /// <summary>
        ///     FT4233HA hi-speed device.
        /// </summary>
        FtDevice4232Ha

        // 24
    }

    #endregion

    #region DEFAULT_VALUES

    private const uint FtDefaultBaudRate = 9600;
    private const uint FtDefaultDeadmanTimeout = 5000;
    private const int FtComPortNotAssigned = -1;
    private const uint FtDefaultInTransferSize = 0x1000;
    private const uint FtDefaultOutTransferSize = 0x1000;
    private const byte FtDefaultLatency = 16;
    private const uint FtDefaultDeviceId = 0x04036001;

    #endregion

    #region METHOD_DEFINITIONS

    //**************************************************************************
    // GetNumberOfDevices
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Gets the number of FTDI devices available.
    /// </summary>
    /// <returns>FT_STATUS value from FT_CreateDeviceInfoList in FTD2XX.DLL</returns>
    /// <param name="devCount">The number of FTDI devices available.</param>
    public FtStatus GetNumberOfDevices(out uint devCount)
    {
        devCount = 0;

        // Call FT_CreateDeviceInfoList
        var ftStatus = FT_CreateDeviceInfoList(ref devCount);

        return ftStatus;
    }

    //**************************************************************************
    // GetDeviceList
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Gets information on all of the FTDI devices available.
    /// </summary>
    /// <returns>FT_STATUS value from FT_GetDeviceInfoDetail in FTD2XX.DLL</returns>
    /// <param name="deviceList">
    ///     An array of type FT_DEVICE_INFO_NODE to contain the device information for all available
    ///     devices.
    /// </param>
    /// <exception cref="FtException">Thrown when the supplied buffer is not large enough to contain the device info list.</exception>
    public FtStatus GetDeviceList(FtDeviceInfoNode[] deviceList)
    {
        // Check for our required function pointers being set up

        uint devCount = 0;

        // Call FT_CreateDeviceInfoList
        var ftStatus = FT_CreateDeviceInfoList(ref devCount);

        // Allocate the required storage for our list
        var serNum = new byte[16];
        var desc = new byte[64];

        if (devCount > 0)
        {
            // Check the size of the buffer passed in is big enough
            if (deviceList.Length < devCount)
            {
                // Buffer not big enough
                var ftErrorCondition = FtError.FtBufferSize;
                // Throw exception
                ErrorHandler(ftStatus, ftErrorCondition);
            }

            // Instantiate the array elements as FT_DEVICE_INFO_NODE
            for (uint i = 0; i < devCount; i++)
            {
                deviceList[i] = new FtDeviceInfoNode();
                // Call FT_GetDeviceInfoDetail
                ftStatus = FT_GetDeviceInfoDetail(i, ref deviceList[i].Flags, ref deviceList[i].Type,
                    ref deviceList[i].Id, ref deviceList[i].LocId, serNum, desc, ref deviceList[i].FtHandle);
                // Convert byte arrays to strings
                deviceList[i].SerialNumber = Encoding.ASCII.GetString(serNum);
                deviceList[i].Description = Encoding.ASCII.GetString(desc);
                // Trim strings to first occurrence of a null terminator character
                var nullIndex = deviceList[i].SerialNumber.IndexOf('\0');
                if (nullIndex != -1)
                    deviceList[i].SerialNumber = deviceList[i].SerialNumber.Substring(0, nullIndex);
                nullIndex = deviceList[i].Description.IndexOf('\0');
                if (nullIndex != -1)
                    deviceList[i].Description = deviceList[i].Description.Substring(0, nullIndex);
            }
        }

        return ftStatus;
    }

    //**************************************************************************
    // OpenByIndex
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Opens the FTDI device with the specified index.
    /// </summary>
    /// <returns>FT_STATUS value from FT_Open in FTD2XX.DLL</returns>
    /// <param name="index">
    ///     Index of the device to open.
    ///     Note that this cannot be guaranteed to open a specific device.
    /// </param>
    /// <remarks>Initialises the device to 8 data bits, 1 stop bit, no parity, no flow control and 9600 Baud.</remarks>
    public FtStatus OpenByIndex(uint index)
    {
        var ftStatus =
            // Call FT_Open
            FT_Open(index, ref _ftHandle);

        // Appears that the handle value can be non-NULL on a fail, so set it explicitly
        if (ftStatus != FtStatus.FtOk)
            _ftHandle = nint.Zero;

        if (_ftHandle != nint.Zero)
        {
            // Initialise port data characteristics
            var wordLength = FtDataBits.FtBits8;
            var stopBits = FtStopBits.FtStopBits1;
            var parity = FtParity.FtParityNone;
            FT_SetDataCharacteristics(_ftHandle, wordLength, stopBits, parity);
            // Initialise to no flow control
            var flowControl = FtFlowControl.FtFlowNone;
            byte xOn = 0x11;
            byte xOff = 0x13;
            FT_SetFlowControl(_ftHandle, flowControl, xOn, xOff);
            // Initialise Baud rate
            uint baudRate = 9600;
            ftStatus = FT_SetBaudRate(_ftHandle, baudRate);
        }

        return ftStatus;
    }

    //**************************************************************************
    // OpenBySerialNumber
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Opens the FTDI device with the specified serial number.
    /// </summary>
    /// <returns>FT_STATUS value from FT_OpenEx in FTD2XX.DLL</returns>
    /// <param name="serialNumber">Serial number of the device to open.</param>
    /// <remarks>Initialises the device to 8 data bits, 1 stop bit, no parity, no flow control and 9600 Baud.</remarks>
    public FtStatus OpenBySerialNumber(string serialNumber)
    {
        var ftStatus =
            // Call FT_OpenEx
            FT_OpenEx(serialNumber, FtOpenBySerialNumber, ref _ftHandle);

        // Appears that the handle value can be non-NULL on a fail, so set it explicitly
        if (ftStatus != FtStatus.FtOk)
            _ftHandle = nint.Zero;

        if (_ftHandle != nint.Zero)
        {
            // Initialise port data characteristics
            const byte wordLength = FtDataBits.FtBits8;
            const byte stopBits = FtStopBits.FtStopBits1;
            const byte parity = FtParity.FtParityNone;
            FT_SetDataCharacteristics(_ftHandle, wordLength, stopBits, parity);
            // Initialise to no flow control
            const ushort flowControl = FtFlowControl.FtFlowNone;
            const byte xOn = 0x11;
            const byte xOff = 0x13;
            FT_SetFlowControl(_ftHandle, flowControl, xOn, xOff);
            // Initialise Baud rate
            const uint baudRate = 9600;
            ftStatus = FT_SetBaudRate(_ftHandle, baudRate);
        }

        return ftStatus;
    }

    //**************************************************************************
    // OpenByDescription
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Opens the FTDI device with the specified description.
    /// </summary>
    /// <returns>FT_STATUS value from FT_OpenEx in FTD2XX.DLL</returns>
    /// <param name="description">Description of the device to open.</param>
    /// <remarks>Initialises the device to 8 data bits, 1 stop bit, no parity, no flow control and 9600 Baud.</remarks>
    public FtStatus OpenByDescription(string description)
    {
        var ftStatus =
            // Call FT_OpenEx
            FT_OpenEx(description, FtOpenByDescription, ref _ftHandle);

        // Appears that the handle value can be non-NULL on a fail, so set it explicitly
        if (ftStatus != FtStatus.FtOk)
            _ftHandle = nint.Zero;

        if (_ftHandle != nint.Zero)
        {
            // Initialise port data characteristics
            var wordLength = FtDataBits.FtBits8;
            var stopBits = FtStopBits.FtStopBits1;
            var parity = FtParity.FtParityNone;
            FT_SetDataCharacteristics(_ftHandle, wordLength, stopBits, parity);
            // Initialise to no flow control
            var flowControl = FtFlowControl.FtFlowNone;
            byte xOn = 0x11;
            byte xOff = 0x13;
            FT_SetFlowControl(_ftHandle, flowControl, xOn, xOff);
            // Initialise Baud rate
            uint baudRate = 9600;
            ftStatus = FT_SetBaudRate(_ftHandle, baudRate);
        }

        return ftStatus;
    }

    //**************************************************************************
    // OpenByLocation
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Opens the FTDI device at the specified physical location.
    /// </summary>
    /// <returns>FT_STATUS value from FT_OpenEx in FTD2XX.DLL</returns>
    /// <param name="location">Location of the device to open.</param>
    /// <remarks>Initialises the device to 8 data bits, 1 stop bit, no parity, no flow control and 9600 Baud.</remarks>
    public FtStatus OpenByLocation(string location)
    {
        var ftStatus =
            // Call FT_OpenEx
            FT_OpenEx(location, FtOpenByLocation, ref _ftHandle);

        // Appears that the handle value can be non-NULL on a fail, so set it explicitly
        if (ftStatus != FtStatus.FtOk)
            _ftHandle = nint.Zero;

        if (_ftHandle != nint.Zero)
        {
            // Initialise port data characteristics
            var wordLength = FtDataBits.FtBits8;
            var stopBits = FtStopBits.FtStopBits1;
            var parity = FtParity.FtParityNone;
            FT_SetDataCharacteristics(_ftHandle, wordLength, stopBits, parity);
            // Initialise to no flow control
            var flowControl = FtFlowControl.FtFlowNone;
            byte xOn = 0x11;
            byte xOff = 0x13;
            FT_SetFlowControl(_ftHandle, flowControl, xOn, xOff);
            // Initialise Baud rate
            uint baudRate = 9600;
            FT_SetBaudRate(_ftHandle, baudRate);
        }

        return ftStatus;
    }

    //**************************************************************************
    // Close
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Closes the handle to an open FTDI device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_Close in FTD2XX.DLL</returns>
    public FtStatus Close()
    {
        var ftStatus =
            // Call FT_Close
            FT_Close(_ftHandle);

        if (ftStatus == FtStatus.FtOk) _ftHandle = nint.Zero;

        return ftStatus;
    }

    //**************************************************************************
    // Read
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Read data from an open FTDI device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_Read in FTD2XX.DLL</returns>
    /// <param name="dataBuffer">An array of bytes which will be populated with the data read from the device.</param>
    /// <param name="numBytesToRead">The number of bytes requested from the device.</param>
    /// <param name="numBytesRead">The number of bytes actually read.</param>
    public FtStatus Read(out byte[] dataBuffer, uint numBytesToRead, out uint numBytesRead)
    {
        var ftStatus = FtStatus.FtOtherError;
        numBytesRead = 0;
        dataBuffer = new byte[numBytesToRead];

        if (_ftHandle != nint.Zero)
            // Call FT_Read
            ftStatus = FT_Read(_ftHandle, dataBuffer, numBytesToRead, ref numBytesRead);

        return ftStatus;
    }

    // Intellisense comments
    /// <summary>
    ///     Read data from an open FTDI device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_Read in FTD2XX.DLL</returns>
    /// <param name="dataBuffer">A string containing the data read</param>
    /// <param name="numBytesToRead">The number of bytes requested from the device.</param>
    /// <param name="numBytesRead">The number of bytes actually read.</param>
    public FtStatus Read(out string dataBuffer, uint numBytesToRead, out uint numBytesRead)
    {
        var ftStatus = FtStatus.FtOtherError;
        numBytesRead = 0;

        // As dataBuffer is an OUT parameter, needs to be assigned before returning
        dataBuffer = Empty;

        var byteDataBuffer = new byte[numBytesToRead];

        if (_ftHandle != nint.Zero)
        {
            // Call FT_Read
            ftStatus = FT_Read(_ftHandle, byteDataBuffer, numBytesToRead, ref numBytesRead);

            // Convert ASCII byte array back to Unicode string for passing back
            dataBuffer = Encoding.ASCII.GetString(byteDataBuffer);
            // Trim buffer to actual bytes read
            dataBuffer = dataBuffer[..(int)numBytesRead];
        }

        return ftStatus;
    }

    //**************************************************************************
    // Write
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Write data to an open FTDI device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_Write in FTD2XX.DLL</returns>
    /// <param name="dataBuffer">An array of bytes which contains the data to be written to the device.</param>
    /// <param name="numBytesToWrite">The number of bytes to be written to the device.</param>
    /// <param name="numBytesWritten">The number of bytes actually written to the device.</param>
    public FtStatus Write(byte[] dataBuffer, int numBytesToWrite, out uint numBytesWritten)
    {
        var ftStatus = FtStatus.FtOtherError;
        numBytesWritten = 0;

        if (_ftHandle != nint.Zero)
            // Call FT_Write
            ftStatus = FT_Write(_ftHandle, dataBuffer, (uint)numBytesToWrite, ref numBytesWritten);

        return ftStatus;
    }

    // Intellisense comments
    /// <summary>
    ///     Write data to an open FTDI device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_Write in FTD2XX.DLL</returns>
    /// <param name="dataBuffer">An array of bytes which contains the data to be written to the device.</param>
    /// <param name="numBytesToWrite">The number of bytes to be written to the device.</param>
    /// <param name="numBytesWritten">The number of bytes actually written to the device.</param>
    public FtStatus Write(byte[] dataBuffer, uint numBytesToWrite, out uint numBytesWritten)
    {
        var ftStatus = FtStatus.FtOtherError;
        numBytesWritten = 0;

        if (_ftHandle != nint.Zero)
            // Call FT_Write
            ftStatus = FT_Write(_ftHandle, dataBuffer, numBytesToWrite, ref numBytesWritten);

        return ftStatus;
    }

    // Intellisense comments
    /// <summary>
    ///     Write data to an open FTDI device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_Write in FTD2XX.DLL</returns>
    /// <param name="dataBuffer">A  string which contains the data to be written to the device.</param>
    /// <param name="numBytesToWrite">The number of bytes to be written to the device.</param>
    /// <param name="numBytesWritten">The number of bytes actually written to the device.</param>
    public FtStatus Write(string dataBuffer, int numBytesToWrite, out uint numBytesWritten)
    {
        numBytesWritten = 0;


        var ftStatus = FtStatus.FtOtherError;

        // Convert Unicode string to ASCII byte array
        var byteDataBuffer = Encoding.ASCII.GetBytes(dataBuffer);

        if (_ftHandle != nint.Zero)
            // Call FT_Write
            ftStatus = FT_Write(_ftHandle, byteDataBuffer, (uint)numBytesToWrite, ref numBytesWritten);

        return ftStatus;
    }

    // Intellisense comments
    /// <summary>
    ///     Write data to an open FTDI device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_Write in FTD2XX.DLL</returns>
    /// <param name="dataBuffer">A  string which contains the data to be written to the device.</param>
    /// <param name="numBytesToWrite">The number of bytes to be written to the device.</param>
    /// <param name="numBytesWritten">The number of bytes actually written to the device.</param>
    public FtStatus Write(string dataBuffer, uint numBytesToWrite, out uint numBytesWritten)
    {
        var ftStatus = FtStatus.FtOtherError;
        numBytesWritten = 0;

        // Convert Unicode string to ASCII byte array
        var byteDataBuffer = Encoding.ASCII.GetBytes(dataBuffer);

        if (_ftHandle != nint.Zero)
            // Call FT_Write
            ftStatus = FT_Write(_ftHandle, byteDataBuffer, numBytesToWrite, ref numBytesWritten);

        return ftStatus;
    }

    //**************************************************************************
    // ResetDevice
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Reset an open FTDI device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_ResetDevice in FTD2XX.DLL</returns>
    public FtStatus ResetDevice()
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
            // Call FT_ResetDevice
            ftStatus = FT_ResetDevice(_ftHandle);

        return ftStatus;
    }

    //**************************************************************************
    // Purge
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Purge data from the devices transmit and/or receive buffers.
    /// </summary>
    /// <returns>FT_STATUS value from FT_Purge in FTD2XX.DLL</returns>
    /// <param name="purgeMask">
    ///     Specifies which buffer(s) to be purged.  Valid values are any combination of the following
    ///     flags: FT_PURGE_RX, FT_PURGE_TX
    /// </param>
    public FtStatus Purge(uint purgeMask)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
            // Call FT_Purge
            ftStatus = FT_Purge(_ftHandle, purgeMask);

        return ftStatus;
    }

    //**************************************************************************
    // SetEventNotification
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Register for event notification.
    /// </summary>
    /// <returns>FT_STATUS value from FT_SetEventNotification in FTD2XX.DLL</returns>
    /// <remarks>
    ///     After setting event notification, the event can be caught by executing the WaitOne() method of the
    ///     EventWaitHandle.  If multiple event types are being monitored, the event that fired can be determined from the
    ///     GetEventType method.
    /// </remarks>
    /// <param name="eventMask">
    ///     The type of events to signal.  Can be any combination of the following: FT_EVENT_RXCHAR,
    ///     FT_EVENT_MODEM_STATUS, FT_EVENT_LINE_STATUS
    /// </param>
    /// <param name="eventHandle">Handle to the event that will receive the notification</param>
    public FtStatus SetEventNotification(uint eventMask, EventWaitHandle eventHandle)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
            // Call FT_SetSetEventNotification
            ftStatus = FT_SetEventNotification(_ftHandle, eventMask, eventHandle.SafeWaitHandle);

        return ftStatus;
    }

    //**************************************************************************
    // StopInTask
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Stops the driver issuing USB in requests.
    /// </summary>
    /// <returns>FT_STATUS value from FT_StopInTask in FTD2XX.DLL</returns>
    public FtStatus StopInTask()
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
            // Call FT_StopInTask
            ftStatus = FT_StopInTask(_ftHandle);

        return ftStatus;
    }

    //**************************************************************************
    // RestartInTask
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Resumes the driver issuing USB in requests.
    /// </summary>
    /// <returns>FT_STATUS value from FT_RestartInTask in FTD2XX.DLL</returns>
    public FtStatus RestartInTask()
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
            // Call FT_RestartInTask
            ftStatus = FT_RestartInTask(_ftHandle);

        return ftStatus;
    }

    //**************************************************************************
    // ResetPort
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Resets the device port.
    /// </summary>
    /// <returns>FT_STATUS value from FT_ResetPort in FTD2XX.DLL</returns>
    public FtStatus ResetPort()
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
            // Call FT_ResetPort
            ftStatus = FT_ResetPort(_ftHandle);

        return ftStatus;
    }

    //**************************************************************************
    // CyclePort
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Causes the device to be re-enumerated on the USB bus.  This is equivalent to unplugging and replugging the device.
    ///     Also calls FT_Close if FT_CyclePort is successful, so no need to call this separately in the application.
    /// </summary>
    /// <returns>FT_STATUS value from FT_CyclePort in FTD2XX.DLL</returns>
    public FtStatus CyclePort()
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
        {
            // Call FT_CyclePort
            ftStatus = FT_CyclePort(_ftHandle);
            if (ftStatus == FtStatus.FtOk)
            {
                // If successful, call FT_Close
                ftStatus = FT_Close(_ftHandle);
                if (ftStatus == FtStatus.FtOk) _ftHandle = nint.Zero;
            }
        }

        return ftStatus;
    }

    //**************************************************************************
    // Rescan
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Causes the system to check for USB hardware changes.  This is equivalent to clicking on the "Scan for hardware
    ///     changes" button in the Device Manager.
    /// </summary>
    /// <returns>FT_STATUS value from FT_Rescan in FTD2XX.DLL</returns>
    public FtStatus Rescan()
    {
        var ftStatus =
            // Call FT_Rescan
            FT_Rescan();
        return ftStatus;
    }

    //**************************************************************************
    // Reload
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Forces a reload of the driver for devices with a specific VID and PID combination.
    /// </summary>
    /// <returns>FT_STATUS value from FT_Reload in FTD2XX.DLL</returns>
    /// <remarks>
    ///     If the VID and PID parameters are 0, the drivers for USB root hubs will be reloaded, causing all USB devices
    ///     connected to reload their drivers
    /// </remarks>
    /// <param name="vendorId">Vendor ID of the devices to have the driver reloaded</param>
    /// <param name="productId">Product ID of the devices to have the driver reloaded</param>
    public FtStatus Reload(ushort vendorId, ushort productId)
    {
        var ftStatus =
            // Call FT_Reload
            FT_Reload(vendorId, productId);
        return ftStatus;
    }

    //**************************************************************************
    // SetBitMode
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Puts the device in a mode other than the default UART or FIFO mode.
    /// </summary>
    /// <returns>FT_STATUS value from FT_SetBitMode in FTD2XX.DLL</returns>
    /// <param name="mask">
    ///     Sets up which bits are inputs and which are outputs.  A bit value of 0 sets the corresponding pin to an input, a
    ///     bit value of 1 sets the corresponding pin to an output.
    ///     In the case of CBUS Bit Bang, the upper nibble of this value controls which pins are inputs and outputs, while the
    ///     lower nibble controls which of the outputs are high and low.
    /// </param>
    /// <param name="bitMode">
    ///     For FT232H devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG, FT_BIT_MODE_MPSSE,
    ///     FT_BIT_MODE_SYNC_BITBANG, FT_BIT_MODE_CBUS_BITBANG, FT_BIT_MODE_MCU_HOST, FT_BIT_MODE_FAST_SERIAL,
    ///     FT_BIT_MODE_SYNC_FIFO.
    ///     For FT2232H devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG, FT_BIT_MODE_MPSSE,
    ///     FT_BIT_MODE_SYNC_BITBANG, FT_BIT_MODE_MCU_HOST, FT_BIT_MODE_FAST_SERIAL, FT_BIT_MODE_SYNC_FIFO.
    ///     For FT4232H devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG, FT_BIT_MODE_MPSSE,
    ///     FT_BIT_MODE_SYNC_BITBANG.
    ///     For FT232R devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG, FT_BIT_MODE_SYNC_BITBANG,
    ///     FT_BIT_MODE_CBUS_BITBANG.
    ///     For FT245R devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG, FT_BIT_MODE_SYNC_BITBANG.
    ///     For FT2232 devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG, FT_BIT_MODE_MPSSE,
    ///     FT_BIT_MODE_SYNC_BITBANG, FT_BIT_MODE_MCU_HOST, FT_BIT_MODE_FAST_SERIAL.
    ///     For FT232B and FT245B devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG.
    /// </param>
    /// <exception cref="FtException">Thrown when the current device does not support the requested bit mode.</exception>
    public FtStatus SetBitMode(byte mask, byte bitMode)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
        {
            // Set Bit Mode does not apply to FT8U232AM, FT8U245AM or FT8U100AX devices
            GetDeviceType(out var deviceType);
            FtError ftErrorCondition;
            if (deviceType == FtDevice.FtDeviceAm)
            {
                // Throw an exception
                ftErrorCondition = FtError.FtInvalidBitMode;
                ErrorHandler(ftStatus, ftErrorCondition);
            }
            else if (deviceType == FtDevice.FtDevice100Ax)
            {
                // Throw an exception
                ftErrorCondition = FtError.FtInvalidBitMode;
                ErrorHandler(ftStatus, ftErrorCondition);
            }
            else if (deviceType == FtDevice.FtDeviceBm && bitMode != FtBitModes.FtBitModeReset)
            {
                if ((bitMode & FtBitModes.FtBitModeAsyncBitBang) == 0)
                {
                    // Throw an exception
                    ftErrorCondition = FtError.FtInvalidBitMode;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }
            }
            else if (deviceType == FtDevice.FtDevice2232 && bitMode != FtBitModes.FtBitModeReset)
            {
                if ((bitMode & (FtBitModes.FtBitModeAsyncBitBang | FtBitModes.FtBitModeMpsse |
                                FtBitModes.FtBitModeSyncBitBang | FtBitModes.FtBitModeMcuHost |
                                FtBitModes.FtBitModeFastSerial)) == 0)
                {
                    // Throw an exception
                    ftErrorCondition = FtError.FtInvalidBitMode;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }

                if ((bitMode == FtBitModes.FtBitModeMpsse) & (InterfaceIdentifier != "A"))
                {
                    // MPSSE mode is only available on channel A
                    // Throw an exception
                    ftErrorCondition = FtError.FtInvalidBitMode;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }
            }
            else if (deviceType == FtDevice.FtDevice232R && bitMode != FtBitModes.FtBitModeReset)
            {
                if ((bitMode & (FtBitModes.FtBitModeAsyncBitBang | FtBitModes.FtBitModeSyncBitBang |
                                FtBitModes.FtBitModeCbusBitBang)) == 0)
                {
                    // Throw an exception
                    ftErrorCondition = FtError.FtInvalidBitMode;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }
            }
            else if (deviceType is FtDevice.FtDevice2232H or FtDevice.FtDevice2232Hp or FtDevice.FtDevice2233Hp
                         or FtDevice.FtDevice2232Ha
                     && bitMode != FtBitModes.FtBitModeReset)
            {
                if ((bitMode & (FtBitModes.FtBitModeAsyncBitBang | FtBitModes.FtBitModeMpsse |
                                FtBitModes.FtBitModeSyncBitBang | FtBitModes.FtBitModeMcuHost |
                                FtBitModes.FtBitModeFastSerial | FtBitModes.FtBitModeSyncFifo)) ==
                    0)
                {
                    // Throw an exception
                    ftErrorCondition = FtError.FtInvalidBitMode;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }

                if (((bitMode == FtBitModes.FtBitModeMcuHost) |
                     (bitMode == FtBitModes.FtBitModeSyncFifo)) & (InterfaceIdentifier != "A"))
                {
                    // MCU Host Emulation and Single channel synchronous 245 FIFO mode is only available on channel A
                    // Throw an exception
                    ftErrorCondition = FtError.FtInvalidBitMode;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }
            }
            else if (deviceType is FtDevice.FtDevice4232H or FtDevice.FtDevice4232Hp or FtDevice.FtDevice4233Hp
                         or FtDevice.FtDevice4232Ha
                     && bitMode != FtBitModes.FtBitModeReset)
            {
                if ((bitMode & (FtBitModes.FtBitModeAsyncBitBang | FtBitModes.FtBitModeMpsse |
                                FtBitModes.FtBitModeSyncBitBang)) == 0)
                {
                    // Throw an exception
                    ftErrorCondition = FtError.FtInvalidBitMode;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }

                if ((bitMode == FtBitModes.FtBitModeMpsse) &
                    (InterfaceIdentifier != "A") & (InterfaceIdentifier != "B"))
                {
                    // MPSSE mode is only available on channel A and B
                    // Throw an exception
                    ftErrorCondition = FtError.FtInvalidBitMode;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }
            }
            else if (deviceType is FtDevice.FtDevice232H or FtDevice.FtDevice232Hp or FtDevice.FtDevice233Hp
                     && bitMode != FtBitModes.FtBitModeReset)
            {
                // FT232H supports all current bit modes!
                if (bitMode > FtBitModes.FtBitModeSyncFifo)
                {
                    // Throw an exception
                    ftErrorCondition = FtError.FtInvalidBitMode;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }
            }

            // Requested bit mode is supported
            // Note FT_BIT_MODES.FT_BIT_MODE_RESET falls through to here - no bits set so cannot check for AND
            // Call FT_SetBitMode
            ftStatus = FT_SetBitMode(_ftHandle, mask, bitMode);
        }

        return ftStatus;
    }

    //**************************************************************************
    // GetPinStates
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Gets the instantaneous state of the device IO pins.
    /// </summary>
    /// <returns>FT_STATUS value from FT_GetBitMode in FTD2XX.DLL</returns>
    /// <param name="bitMode">A bitmap value containing the instantaneous state of the device IO pins</param>
    public FtStatus GetPinStates(ref byte bitMode)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
            // Call FT_GetBitMode
            ftStatus = FT_GetBitMode(_ftHandle, ref bitMode);

        return ftStatus;
    }

    //**************************************************************************
    // GetDeviceType
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Gets the chip type of the current device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_GetDeviceInfo in FTD2XX.DLL</returns>
    /// <param name="deviceType">The FTDI chip type of the current device.</param>
    public FtStatus GetDeviceType(out FtDevice deviceType)
    {
        var ftStatus = FtStatus.FtOtherError;

        uint deviceId = 0;
        var serNum = new byte[16];
        var desc = new byte[64];

        deviceType = FtDevice.FtDeviceUnknown;

        if (_ftHandle != nint.Zero)
            // Call FT_GetDeviceInfo
            ftStatus = FT_GetDeviceInfo(_ftHandle, ref deviceType, ref deviceId, serNum, desc, nint.Zero);

        return ftStatus;
    }

    //**************************************************************************
    // GetDeviceID
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Gets the Vendor ID and Product ID of the current device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_GetDeviceInfo in FTD2XX.DLL</returns>
    /// <param name="deviceId">The device ID (Vendor ID and Product ID) of the current device.</param>
    public FtStatus GetDeviceId(ref uint deviceId)
    {
        var ftStatus = FtStatus.FtOtherError;

        var deviceType = FtDevice.FtDeviceUnknown;
        var serNum = new byte[16];
        var desc = new byte[64];

        if (_ftHandle != nint.Zero)
            // Call FT_GetDeviceInfo
            ftStatus = FT_GetDeviceInfo(_ftHandle, ref deviceType, ref deviceId, serNum, desc, nint.Zero);


        return ftStatus;
    }

    //**************************************************************************
    // GetDescription
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Gets the description of the current device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_GetDeviceInfo in FTD2XX.DLL</returns>
    /// <param name="description">The description of the current device.</param>
    public FtStatus GetDescription(out string description)
    {
        var ftStatus = FtStatus.FtOtherError;

        description = Empty;

        uint deviceId = 0;
        var deviceType = FtDevice.FtDeviceUnknown;
        var serNum = new byte[16];
        var desc = new byte[64];

        if (_ftHandle != nint.Zero)
        {
            // Call FT_GetDeviceInfo
            ftStatus = FT_GetDeviceInfo(_ftHandle, ref deviceType, ref deviceId, serNum, desc, nint.Zero);
            description = Encoding.ASCII.GetString(desc);
            description = description.Substring(0, description.IndexOf('\0'));
        }


        return ftStatus;
    }

    //**************************************************************************
    // GetSerialNumber
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Gets the serial number of the current device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_GetDeviceInfo in FTD2XX.DLL</returns>
    /// <param name="serialNumber">The serial number of the current device.</param>
    public FtStatus GetSerialNumber(out string serialNumber)
    {
        var ftStatus = FtStatus.FtOtherError;

        serialNumber = Empty;

        uint deviceId = 0;
        var deviceType = FtDevice.FtDeviceUnknown;
        var serNum = new byte[16];
        var desc = new byte[64];

        if (_ftHandle != nint.Zero)
        {
            // Call FT_GetDeviceInfo
            ftStatus = FT_GetDeviceInfo(_ftHandle, ref deviceType, ref deviceId, serNum, desc, nint.Zero);
            serialNumber = Encoding.ASCII.GetString(serNum);
            serialNumber = serialNumber.Substring(0, serialNumber.IndexOf('\0'));
        }

        return ftStatus;
    }

    //**************************************************************************
    // GetRxBytesAvailable
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Gets the number of bytes available in the receive buffer.
    /// </summary>
    /// <returns>FT_STATUS value from FT_GetQueueStatus in FTD2XX.DLL</returns>
    /// <param name="rxQueue">The number of bytes available to be read.</param>
    public FtStatus GetRxBytesAvailable(ref uint rxQueue)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
            // Call FT_GetQueueStatus
            ftStatus = FT_GetQueueStatus(_ftHandle, ref rxQueue);

        return ftStatus;
    }

    //**************************************************************************
    // GetTxBytesWaiting
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Gets the number of bytes waiting in the transmit buffer.
    /// </summary>
    /// <returns>FT_STATUS value from FT_GetStatus in FTD2XX.DLL</returns>
    /// <param name="txQueue">The number of bytes waiting to be sent.</param>
    public FtStatus GetTxBytesWaiting(ref uint txQueue)
    {
        var ftStatus = FtStatus.FtOtherError;

        uint rxQueue = 0;
        uint eventStatus = 0;

        if (_ftHandle != nint.Zero)
            // Call FT_GetStatus
            ftStatus = FT_GetStatus(_ftHandle, ref rxQueue, ref txQueue, ref eventStatus);


        return ftStatus;
    }

    //**************************************************************************
    // GetEventType
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Gets the event type after an event has fired.  Can be used to distinguish which event has been triggered when
    ///     waiting on multiple event types.
    /// </summary>
    /// <returns>FT_STATUS value from FT_GetStatus in FTD2XX.DLL</returns>
    /// <param name="eventType">The type of event that has occurred.</param>
    public FtStatus GetEventType(ref uint eventType)
    {
        var ftStatus = FtStatus.FtOtherError;

        uint rxQueue = 0;
        uint txQueue = 0;

        if (_ftHandle != nint.Zero)
            // Call FT_GetStatus
            ftStatus = FT_GetStatus(_ftHandle, ref rxQueue, ref txQueue, ref eventType);

        return ftStatus;
    }

    //**************************************************************************
    // GetModemStatus
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Gets the current modem status.
    /// </summary>
    /// <returns>FT_STATUS value from FT_GetModemStatus in FTD2XX.DLL</returns>
    /// <param name="modemStatus">A bit map representation of the current modem status.</param>
    public FtStatus GetModemStatus(out byte modemStatus)
    {
        var ftStatus = FtStatus.FtOtherError;

        uint modemLineStatus = 0;

        if (_ftHandle != nint.Zero)
            // Call FT_GetModemStatus
            ftStatus = FT_GetModemStatus(_ftHandle, ref modemLineStatus);

        modemStatus = Convert.ToByte(modemLineStatus & 0x000000FF);

        return ftStatus;
    }

    //**************************************************************************
    // GetLineStatus
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Gets the current line status.
    /// </summary>
    /// <returns>FT_STATUS value from FT_GetModemStatus in FTD2XX.DLL</returns>
    /// <param name="lineStatus">A bit map representation of the current line status.</param>
    public FtStatus GetLineStatus(out byte lineStatus)
    {
        var ftStatus = FtStatus.FtOtherError;

        uint modemLineStatus = 0;

        if (_ftHandle != nint.Zero)
            // Call FT_GetModemStatus
            ftStatus = FT_GetModemStatus(_ftHandle, ref modemLineStatus);

        lineStatus = Convert.ToByte((modemLineStatus >> 8) & 0x000000FF);


        return ftStatus;
    }

    //**************************************************************************
    // SetBaudRate
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Sets the current Baud rate.
    /// </summary>
    /// <returns>FT_STATUS value from FT_SetBaudRate in FTD2XX.DLL</returns>
    /// <param name="baudRate">The desired Baud rate for the device.</param>
    public FtStatus SetBaudRate(uint baudRate)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
            // Call FT_SetBaudRate
            ftStatus = FT_SetBaudRate(_ftHandle, baudRate);

        return ftStatus;
    }

    //**************************************************************************
    // SetDataCharacteristics
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Sets the data bits, stop bits and parity for the device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_SetDataCharacteristics in FTD2XX.DLL</returns>
    /// <param name="dataBits">
    ///     The number of data bits for UART data.  Valid values are FT_DATA_BITS.FT_DATA_7 or
    ///     FT_DATA_BITS.FT_BITS_8
    /// </param>
    /// <param name="stopBits">
    ///     The number of stop bits for UART data.  Valid values are FT_STOP_BITS.FT_STOP_BITS_1 or
    ///     FT_STOP_BITS.FT_STOP_BITS_2
    /// </param>
    /// <param name="parity">
    ///     The parity of the UART data.  Valid values are FT_PARITY.FT_PARITY_NONE, FT_PARITY.FT_PARITY_ODD,
    ///     FT_PARITY.FT_PARITY_EVEN, FT_PARITY.FT_PARITY_MARK or FT_PARITY.FT_PARITY_SPACE
    /// </param>
    public FtStatus SetDataCharacteristics(byte dataBits, byte stopBits, byte parity)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
            // Call FT_SetDataCharacteristics
            ftStatus = FT_SetDataCharacteristics(_ftHandle, dataBits, stopBits, parity);

        return ftStatus;
    }

    //**************************************************************************
    // SetFlowControl
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Sets the flow control type.
    /// </summary>
    /// <returns>FT_STATUS value from FT_SetFlowControl in FTD2XX.DLL</returns>
    /// <param name="flowControl">
    ///     The type of flow control for the UART.  Valid values are FT_FLOW_CONTROL.FT_FLOW_NONE,
    ///     FT_FLOW_CONTROL.FT_FLOW_RTS_CTS, FT_FLOW_CONTROL.FT_FLOW_DTR_DSR or FT_FLOW_CONTROL.FT_FLOW_XON_XOFF
    /// </param>
    /// <param name="xOn">The Xon character for Xon/Xoff flow control.  Ignored if not using Xon/XOff flow control.</param>
    /// <param name="xOff">The Xoff character for Xon/Xoff flow control.  Ignored if not using Xon/XOff flow control.</param>
    public FtStatus SetFlowControl(ushort flowControl, byte xOn, byte xOff)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
            // Call FT_SetFlowControl
            ftStatus = FT_SetFlowControl(_ftHandle, flowControl, xOn, xOff);

        return ftStatus;
    }

    //**************************************************************************
    // SetRTS
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Asserts or de-asserts the Request To Send (RTS) line.
    /// </summary>
    /// <returns>FT_STATUS value from FT_SetRts or FT_ClrRts in FTD2XX.DLL</returns>
    /// <param name="enable">If true, asserts RTS.  If false, de-asserts RTS</param>
    public FtStatus SetRts(bool enable)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
        {
            // Call FT_SetRts
            ftStatus = enable ? FT_SetRts(_ftHandle) :
                // Call FT_ClrRts
                FT_ClrRts(_ftHandle);
        }

        return ftStatus;
    }

    //**************************************************************************
    // SetDTR
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Asserts or de-asserts the Data Terminal Ready (DTR) line.
    /// </summary>
    /// <returns>FT_STATUS value from FT_SetDtr or FT_ClrDtr in FTD2XX.DLL</returns>
    /// <param name="enable">If true, asserts DTR.  If false, de-asserts DTR.</param>
    public FtStatus SetDtr(bool enable)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
        {
            // Call FT_SetDtr
            ftStatus = enable ? FT_SetDtr(_ftHandle) :
                // Call FT_ClrDtr
                FT_ClrDtr(_ftHandle);
        }

        return ftStatus;
    }

    //**************************************************************************
    // SetTimeouts
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Sets the read and write timeout values.
    /// </summary>
    /// <returns>FT_STATUS value from FT_SetTimeouts in FTD2XX.DLL</returns>
    /// <param name="readTimeout">Read timeout value in ms.  A value of 0 indicates an infinite timeout.</param>
    /// <param name="writeTimeout">Write timeout value in ms.  A value of 0 indicates an infinite timeout.</param>
    public FtStatus SetTimeouts(uint readTimeout, uint writeTimeout)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
            // Call FT_SetTimeouts
            ftStatus = FT_SetTimeouts(_ftHandle, readTimeout, writeTimeout);

        return ftStatus;
    }

    //**************************************************************************
    // SetBreak
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Sets or clears the break state.
    /// </summary>
    /// <returns>FT_STATUS value from FT_SetBreakOn or FT_SetBreakOff in FTD2XX.DLL</returns>
    /// <param name="enable">If true, sets break on.  If false, sets break off.</param>
    public FtStatus SetBreak(bool enable)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
        {
            // Call FT_SetBreakOn
            ftStatus = enable ? FT_SetBreakOn(_ftHandle) :
                // Call FT_SetBreakOff
                FT_SetBreakOff(_ftHandle);
        }

        return ftStatus;
    }

    //**************************************************************************
    // SetResetPipeRetryCount
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Gets or sets the reset pipe retry count.  Default value is 50.
    /// </summary>
    /// <returns>FT_STATUS value from FT_SetResetPipeRetryCount in FTD2XX.DLL</returns>
    /// <param name="resetPipeRetryCount">
    ///     The reset pipe retry count.
    ///     Electrically noisy environments may benefit from a larger value.
    /// </param>
    public FtStatus SetResetPipeRetryCount(uint resetPipeRetryCount)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
            // Call FT_SetResetPipeRetryCount
            ftStatus = FT_SetResetPipeRetryCount(_ftHandle, resetPipeRetryCount);

        return ftStatus;
    }

    //**************************************************************************
    // GetDriverVersion
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Gets the current FTDIBUS.SYS driver version number.
    /// </summary>
    /// <returns>FT_STATUS value from FT_GetDriverVersion in FTD2XX.DLL</returns>
    /// <param name="driverVersion">The current driver version number.</param>
    public FtStatus GetDriverVersion(ref uint driverVersion)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
            // Call FT_GetDriverVersion
            ftStatus = FT_GetDriverVersion(_ftHandle, ref driverVersion);

        return ftStatus;
    }

    //**************************************************************************
    // GetLibraryVersion
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Gets the current FTD2XX.DLL driver version number.
    /// </summary>
    /// <returns>FT_STATUS value from FT_GetLibraryVersion in FTD2XX.DLL</returns>
    /// <param name="libraryVersion">The current library version.</param>
    public FtStatus GetLibraryVersion(ref uint libraryVersion)
    {
        var ftStatus =
            // Call FT_GetLibraryVersion
            FT_GetLibraryVersion(ref libraryVersion);

        return ftStatus;
    }

    //**************************************************************************
    // SetDeadmanTimeout
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Sets the USB deadman timeout value.  Default is 5000ms.
    /// </summary>
    /// <returns>FT_STATUS value from FT_SetDeadmanTimeout in FTD2XX.DLL</returns>
    /// <param name="deadmanTimeout">The deadman timeout value in ms.  Default is 5000ms.</param>
    public FtStatus SetDeadmanTimeout(uint deadmanTimeout)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
            // Call FT_SetDeadmanTimeout
            ftStatus = FT_SetDeadmanTimeout(_ftHandle, deadmanTimeout);

        return ftStatus;
    }

    //**************************************************************************
    // SetLatency
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Sets the value of the latency timer.  Default value is 16ms.
    /// </summary>
    /// <returns>FT_STATUS value from FT_SetLatencyTimer in FTD2XX.DLL</returns>
    /// <param name="latency">
    ///     The latency timer value in ms.
    ///     Valid values are 2ms - 255ms for FT232BM, FT245BM and FT2232 devices.
    ///     Valid values are 0ms - 255ms for other devices.
    /// </param>
    public FtStatus SetLatency(byte latency)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
        {
            // Set Bit Mode does not apply to FT8U232AM, FT8U245AM or FT8U100AX devices
            GetDeviceType(out var deviceType);
            if (deviceType is FtDevice.FtDeviceBm or FtDevice.FtDevice2232)
                // Do not allow latency of 1ms or 0ms for older devices
                // since this can cause problems/lock up due to buffering mechanism
                if (latency < 2)
                    latency = 2;

            // Call FT_SetLatencyTimer
            ftStatus = FT_SetLatencyTimer(_ftHandle, latency);
        }

        return ftStatus;
    }

    //**************************************************************************
    // GetLatency
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Gets the value of the latency timer.  Default value is 16ms.
    /// </summary>
    /// <returns>FT_STATUS value from FT_GetLatencyTimer in FTD2XX.DLL</returns>
    /// <param name="latency">The latency timer value in ms.</param>
    public FtStatus GetLatency(ref byte latency)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
            // Call FT_GetLatencyTimer
            ftStatus = FT_GetLatencyTimer(_ftHandle, ref latency);

        return ftStatus;
    }

    //**************************************************************************
    // SetUSBTransferSizes
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Sets the USB IN and OUT transfer sizes.
    /// </summary>
    /// <returns>FT_STATUS value from FT_SetUSBParameters in FTD2XX.DLL</returns>
    /// <param name="inTransferSize">The USB IN transfer size in bytes.</param>
    public FtStatus InTransferSize(uint inTransferSize)
        // Only support IN transfer sizes at the moment
        //public uint InTransferSize(uint InTransferSize, uint OutTransferSize)
    {
        var ftStatus = FtStatus.FtOtherError;

        var outTransferSize = inTransferSize;

        if (_ftHandle != nint.Zero)
            // Call FT_SetUSBParameters
            ftStatus = FT_SetUSBParameters(_ftHandle, inTransferSize, outTransferSize);

        return ftStatus;
    }

    //**************************************************************************
    // SetCharacters
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Sets an event character, an error character and enables or disables them.
    /// </summary>
    /// <returns>FT_STATUS value from FT_SetChars in FTD2XX.DLL</returns>
    /// <param name="eventChar">A character that will be trigger an IN to the host when this character is received.</param>
    /// <param name="eventCharEnable">Determines if the EventChar is enabled or disabled.</param>
    /// <param name="errorChar">A character that will be inserted into the data stream to indicate that an error has occurred.</param>
    /// <param name="errorCharEnable">Determines if the ErrorChar is enabled or disabled.</param>
    public FtStatus SetCharacters(byte eventChar, bool eventCharEnable, byte errorChar, bool errorCharEnable)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
            // Call FT_SetChars
            ftStatus = FT_SetChars(_ftHandle, eventChar, Convert.ToByte(eventCharEnable), errorChar,
                Convert.ToByte(errorCharEnable));

        return ftStatus;
    }

    //**************************************************************************
    // GetCOMPort
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Gets the corresponding COM port number for the current device.  If no COM port is exposed, an empty string is
    ///     returned.
    /// </summary>
    /// <returns>FT_STATUS value from FT_GetComPortNumber in FTD2XX.DLL</returns>
    /// <param name="comPortName">
    ///     The COM port name corresponding to the current device.  If no COM port is installed, an empty
    ///     string is passed back.
    /// </param>
    public FtStatus GetComPort(out string comPortName)
    {
        var ftStatus = FtStatus.FtOtherError;

        // As ComPortName is an OUT parameter, has to be assigned before returning
        comPortName = Empty;

        var comPortNumber = -1;
        if (_ftHandle != nint.Zero)
            // Call FT_GetComPortNumber
            ftStatus = FT_GetComPortNumber(_ftHandle, ref comPortNumber);

        if (comPortNumber == -1)
            // If no COM port installed, return an empty string
            comPortName = Empty;
        else
            // If installed, return full COM string
            // This can then be passed to an instance of the SerialPort class to assign the port number.
            comPortName = "COM" + comPortNumber;

        return ftStatus;
    }

    //**************************************************************************
    // VendorCmdGet
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Get data from the FT4222 using the vendor command interface.
    /// </summary>
    /// <returns>FT_STATUS value from FT_VendorCmdSet in FTD2XX.DLL</returns>
    public FtStatus VendorCmdGet(ushort request, byte[] buf, ushort len)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
            // Call FT_VendorCmdGet
            ftStatus = FT_VendorCmdGet(_ftHandle, request, buf, len);

        return ftStatus;
    }

    //**************************************************************************
    // VendorCmdSet
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Set data from the FT4222 using the vendor command interface.
    /// </summary>
    /// <returns>FT_STATUS value from FT_VendorCmdSet in FTD2XX.DLL</returns>
    public FtStatus VendorCmdSet(ushort request, byte[] buf, ushort len)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
            // Call FT_VendorCmdSet
            ftStatus = FT_VendorCmdSet(_ftHandle, request, buf, len);

        return ftStatus;
    }

    #endregion

    #region PROPERTY_DEFINITIONS

    //**************************************************************************
    // IsOpen
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Gets the open status of the device.
    /// </summary>
    public bool IsOpen => _ftHandle != nint.Zero;

    //**************************************************************************
    // InterfaceIdentifier
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Gets the interface identifier.
    /// </summary>
    private string InterfaceIdentifier
    {
        get
        {
            var identifier = Empty;
            if (IsOpen)
            {
                GetDeviceType(out var deviceType);
                if (deviceType is FtDevice.FtDevice2232H or FtDevice.FtDevice4232H or FtDevice.FtDevice2233Hp
                    or FtDevice.FtDevice4233Hp or FtDevice.FtDevice2232Hp or FtDevice.FtDevice4232Hp
                    or FtDevice.FtDevice2232Ha or FtDevice.FtDevice4232Ha or FtDevice.FtDevice2232)
                {
                    GetDescription(out var description);
                    identifier = description[^1..];
                    return identifier;
                }
            }

            return identifier;
        }
    }

    #endregion
}