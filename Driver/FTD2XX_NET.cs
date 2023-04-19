using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using static System.String;

#pragma warning disable

namespace RainDropWeb.Driver;

/// <summary>
///     Class wrapper for FTD2XX.DLL
/// </summary>
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
                case FtStatus.FtEepromEraseFailed:
                {
                    throw new FtException("Failed to erase FTDI device EEPROM.");
                }
                case FtStatus.FtEepromNotPresent:
                {
                    throw new FtException("No EEPROM fitted to FTDI device.");
                }
                case FtStatus.FtEepromNotProgrammed:
                {
                    throw new FtException("FTDI device EEPROM not programmed.");
                }
                case FtStatus.FtEepromReadFailed:
                {
                    throw new FtException("Failed to read FTDI device EEPROM.");
                }
                case FtStatus.FtEepromWriteFailed:
                {
                    throw new FtException("Failed to write FTDI device EEPROM.");
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
                case FtError.FtInvalidBitmode:
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
        public string Description = Empty;

        /// <summary>
        ///     Indicates device state.  Can be any combination of the following: FT_FLAGS_OPENED, FT_FLAGS_HISPEED
        /// </summary>
        public uint Flags;

        /// <summary>
        ///     The device handle.  This value is not used externally and is provided for information only.
        ///     If the device is not open, this value is 0.
        /// </summary>
        public nint FtHandle;

        /// <summary>
        ///     The Vendor ID and Product ID of the device
        /// </summary>
        public uint Id;

        /// <summary>
        ///     The physical location identifier of the device
        /// </summary>
        public uint LocId;

        /// <summary>
        ///     The device serial number
        /// </summary>
        public string SerialNumber = Empty;

        /// <summary>
        ///     Indicates the device type.  Can be one of the following: FT_DEVICE_232R, FT_DEVICE_2232C, FT_DEVICE_BM,
        ///     FT_DEVICE_AM, FT_DEVICE_100AX or FT_DEVICE_UNKNOWN
        /// </summary>
        public FtDevice Type;
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
    private static extern FtStatus FT_ReadEE(nint ftHandle, uint dwWordOffset, ref ushort lpwValue);

    [DllImport(DllName)]
    private static extern FtStatus FT_WriteEE(nint ftHandle, uint dwWordOffset, ushort wValue);

    [DllImport(DllName)]
    private static extern FtStatus FT_EraseEE(nint ftHandle);

    [DllImport(DllName)]
    private static extern FtStatus FT_EE_UASize(nint ftHandle, ref uint dwSize);

    [DllImport(DllName)]
    private static extern FtStatus FT_EE_UARead(nint ftHandle, byte[] pucData, int dwDataLen,
        ref uint lpdwDataRead);

    [DllImport(DllName)]
    private static extern FtStatus FT_EE_UAWrite(nint ftHandle, byte[] pucData, int dwDataLen);

    [DllImport(DllName)]
    private static extern FtStatus FT_EE_Read(nint ftHandle, FtProgramData pData);

    [DllImport(DllName)]
    private static extern FtStatus FT_EE_Program(nint ftHandle, FtProgramData pData);

    [DllImport(DllName)]
    private static extern FtStatus FT_EEPROM_Read(nint ftHandle, nint eepromData, uint eepromDataSize,
        byte[] manufacturer, byte[] manufacturerId, byte[] description, byte[] serialNumber);

    [DllImport(DllName)]
    private static extern FtStatus FT_EEPROM_Program(nint ftHandle, nint eepromData, uint eepromDataSize,
        byte[] manufacturer, byte[] manufacturerId, byte[] description, byte[] serialNumber);

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
        ///     Device not poened for write
        /// </summary>
        FtDeviceNotOpenedForWrite,

        /// <summary>
        ///     Failed to write to device
        /// </summary>
        FtFailedToWriteDevice,

        /// <summary>
        ///     Failed to read the device EEPROM
        /// </summary>
        FtEepromReadFailed,

        /// <summary>
        ///     Failed to write the device EEPROM
        /// </summary>
        FtEepromWriteFailed,

        /// <summary>
        ///     Failed to erase the device EEPROM
        /// </summary>
        FtEepromEraseFailed,

        /// <summary>
        ///     An EEPROM is not fitted to the device
        /// </summary>
        FtEepromNotPresent,

        /// <summary>
        ///     Device EEPROM is blank
        /// </summary>
        FtEepromNotProgrammed,

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
        FtInvalidBitmode,
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

    // FT232R CBUS Options
    /// <summary>
    ///     Available functions for the FT232R CBUS pins.  Controlled by FT232R EEPROM settings
    /// </summary>
    public static class FtCbusOptions
    {
        /// <summary>
        ///     FT232R CBUS EEPROM options - Tx Data Enable
        /// </summary>
        public const byte FtCbusTxDEn = 0x00;

        /// <summary>
        ///     FT232R CBUS EEPROM options - Power On
        /// </summary>
        public const byte FtCbusPwrOn = 0x01;

        /// <summary>
        ///     FT232R CBUS EEPROM options - Rx LED
        /// </summary>
        public const byte FtCbusRxLed = 0x02;

        /// <summary>
        ///     FT232R CBUS EEPROM options - Tx LED
        /// </summary>
        public const byte FtCbusTxLed = 0x03;

        /// <summary>
        ///     FT232R CBUS EEPROM options - Tx and Rx LED
        /// </summary>
        public const byte FtCbusTxRxLed = 0x04;

        /// <summary>
        ///     FT232R CBUS EEPROM options - Sleep
        /// </summary>
        public const byte FtCbusSleep = 0x05;

        /// <summary>
        ///     FT232R CBUS EEPROM options - 48MHz clock
        /// </summary>
        public const byte FtCbusClk48 = 0x06;

        /// <summary>
        ///     FT232R CBUS EEPROM options - 24MHz clock
        /// </summary>
        public const byte FtCbusClk24 = 0x07;

        /// <summary>
        ///     FT232R CBUS EEPROM options - 12MHz clock
        /// </summary>
        public const byte FtCbusClk12 = 0x08;

        /// <summary>
        ///     FT232R CBUS EEPROM options - 6MHz clock
        /// </summary>
        public const byte FtCbusClk6 = 0x09;

        /// <summary>
        ///     FT232R CBUS EEPROM options - IO mode
        /// </summary>
        public const byte FtCbusIoMode = 0x0A;

        /// <summary>
        ///     FT232R CBUS EEPROM options - Bit-bang write strobe
        /// </summary>
        public const byte FtCbusBitBangWr = 0x0B;

        /// <summary>
        ///     FT232R CBUS EEPROM options - Bit-bang read strobe
        /// </summary>
        public const byte FtCbusBitBangRd = 0x0C;
    }

    // FT232H CBUS Options
    /// <summary>
    ///     Available functions for the FT232H CBUS pins.  Controlled by FT232H EEPROM settings
    /// </summary>
    public static class Ft232HCbusOptions
    {
        /// <summary>
        ///     FT232H CBUS EEPROM options - Tristate
        /// </summary>
        public const byte FtCbusTristate = 0x00;

        /// <summary>
        ///     FT232H CBUS EEPROM options - Rx LED
        /// </summary>
        public const byte FtCbusRxLed = 0x01;

        /// <summary>
        ///     FT232H CBUS EEPROM options - Tx LED
        /// </summary>
        public const byte FtCbusTxLed = 0x02;

        /// <summary>
        ///     FT232H CBUS EEPROM options - Tx and Rx LED
        /// </summary>
        public const byte FtCbusTxRxLed = 0x03;

        /// <summary>
        ///     FT232H CBUS EEPROM options - Power Enable#
        /// </summary>
        public const byte FtCbusPwrEn = 0x04;

        /// <summary>
        ///     FT232H CBUS EEPROM options - Sleep
        /// </summary>
        public const byte FtCbusSleep = 0x05;

        /// <summary>
        ///     FT232H CBUS EEPROM options - Drive pin to logic 0
        /// </summary>
        public const byte FtCbusDrive0 = 0x06;

        /// <summary>
        ///     FT232H CBUS EEPROM options - Drive pin to logic 1
        /// </summary>
        public const byte FtCbusDrive1 = 0x07;

        /// <summary>
        ///     FT232H CBUS EEPROM options - IO Mode
        /// </summary>
        public const byte FtCbusIoMode = 0x08;

        /// <summary>
        ///     FT232H CBUS EEPROM options - Tx Data Enable
        /// </summary>
        public const byte FtCbusTxdEn = 0x09;

        /// <summary>
        ///     FT232H CBUS EEPROM options - 30MHz clock
        /// </summary>
        public const byte FtCbusClk30 = 0x0A;

        /// <summary>
        ///     FT232H CBUS EEPROM options - 15MHz clock
        /// </summary>
        public const byte FtCbusClk15 = 0x0B;

        /// <summary>
        ///     FT232H CBUS EEPROM options - 7.5MHz clock
        /// </summary>
        public const byte FtCbusClk75 = 0x0C;
    }

    /// <summary>
    ///     Available functions for the X-Series CBUS pins.  Controlled by X-Series EEPROM settings
    /// </summary>
    public class FtXSeriesCbusOptions
    {
        /// <summary>
        ///     FT X-Series CBUS EEPROM options - Tristate
        /// </summary>
        public const byte FtCbusTristate = 0x00;

        /// <summary>
        ///     FT X-Series CBUS EEPROM options - RxLED#
        /// </summary>
        public const byte FtCbusRxLed = 0x01;

        /// <summary>
        ///     FT X-Series CBUS EEPROM options - TxLED#
        /// </summary>
        public const byte FtCbusTxLed = 0x02;

        /// <summary>
        ///     FT X-Series CBUS EEPROM options - TxRxLED#
        /// </summary>
        public const byte FtCbusTxrxLed = 0x03;

        /// <summary>
        ///     FT X-Series CBUS EEPROM options - PwrEn#
        /// </summary>
        public const byte FtCbusPwrEn = 0x04;

        /// <summary>
        ///     FT X-Series CBUS EEPROM options - Sleep#
        /// </summary>
        public const byte FtCbusSleep = 0x05;

        /// <summary>
        ///     FT X-Series CBUS EEPROM options - Drive_0
        /// </summary>
        public const byte FtCbusDrive0 = 0x06;

        /// <summary>
        ///     FT X-Series CBUS EEPROM options - Drive_1
        /// </summary>
        public const byte FtCbusDrive1 = 0x07;

        /// <summary>
        ///     FT X-Series CBUS EEPROM options - GPIO
        /// </summary>
        public const byte FtCbusGpio = 0x08;

        /// <summary>
        ///     FT X-Series CBUS EEPROM options - TxdEn
        /// </summary>
        public const byte FtCbusTxdEn = 0x09;

        /// <summary>
        ///     FT X-Series CBUS EEPROM options - Clk24MHz
        /// </summary>
        public const byte FtCbusClk24MHz = 0x0A;

        /// <summary>
        ///     FT X-Series CBUS EEPROM options - Clk12MHz
        /// </summary>
        public const byte FtCbusClk12MHz = 0x0B;

        /// <summary>
        ///     FT X-Series CBUS EEPROM options - Clk6MHz
        /// </summary>
        public const byte FtCbusClk6MHz = 0x0C;

        /// <summary>
        ///     FT X-Series CBUS EEPROM options - BCD_Charger
        /// </summary>
        public const byte FtCbusBcdCharger = 0x0D;

        /// <summary>
        ///     FT X-Series CBUS EEPROM options - BCD_Charger#
        /// </summary>
        public const byte FtCbusBcdChargerN = 0x0E;

        /// <summary>
        ///     FT X-Series CBUS EEPROM options - I2C_TXE#
        /// </summary>
        public const byte FtCbusI2CTxe = 0x0F;

        /// <summary>
        ///     FT X-Series CBUS EEPROM options - I2C_RXF#
        /// </summary>
        public const byte FtCbusI2CRxf = 0x10;

        /// <summary>
        ///     FT X-Series CBUS EEPROM options - VBUS_Sense
        /// </summary>
        public const byte FtCbusVbusSense = 0x11;

        /// <summary>
        ///     FT X-Series CBUS EEPROM options - BitBang_WR#
        /// </summary>
        public const byte FtCbusBitBangWr = 0x12;

        /// <summary>
        ///     FT X-Series CBUS EEPROM options - BitBang_RD#
        /// </summary>
        public const byte FtCbusBitBangRd = 0x13;

        /// <summary>
        ///     FT X-Series CBUS EEPROM options - Time_Stampe
        /// </summary>
        public const byte FtCbusTimeStamp = 0x14;

        /// <summary>
        ///     FT X-Series CBUS EEPROM options - Keep_Awake#
        /// </summary>
        public const byte FtCbusKeepAwake = 0x15;
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
        public const uint FtFlagsHispeed = 0x00000002;
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

    #region EEPROM_STRUCTURES

    // Data structure can't be changed
#pragma warning disable CS0649
    // Internal structure for reading and writing EEPROM contents
    // NOTE:  NEED Pack=1 for byte alignment!  Without this, data is garbage
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private class FtProgramData
    {
        public byte ACDriveCurrentH; // valid values are 4mA, 8mA, 12mA, 16mA
        public byte ACSchmittInputH; // non-zero if AC pins are Schmitt input
        public byte ACSlowSlewH; // non-zero if AC pins have slow slew
        public byte ADDriveCurrentH; // valid values are 4mA, 8mA, 12mA, 16mA
        public byte ADriveCurrent; // valid values are 4mA, 8mA, 12mA, 16mA
        public byte ADSchmittInputH; // non-zero if AD pins are Schmitt input
        public byte ADSlowSlewH; // non-zero if AD pins have slow slew
        public byte AHDriveCurrent; // valid values are 4mA, 8mA, 12mA, 16mA
        public byte AHSchmittInput; // non-zero if AH pins are Schmitt input
        public byte AHSlowSlew; // non-zero if AH pins have slow slew
        public byte AIsHighCurrent;
        public byte AIsVCP;
        public byte AIsVCP7; // non-zero if interface is to use VCP drivers
        public byte AIsVCP8; // non-zero if interface is to use VCP drivers
        public byte ALDriveCurrent; // valid values are 4mA, 8mA, 12mA, 16mA
        public byte ALSchmittInput; // non-zero if AL pins are Schmitt input
        public byte ALSlowSlew; // non-zero if AL pins have slow slew
        public byte ARIIsTxdEn;
        public byte ASchmittInput; // non-zero if AL pins are Schmitt input
        public byte ASlowSlew; // non-zero if AL pins have slow slew
        public byte BDriveCurrent; // valid values are 4mA, 8mA, 12mA, 16mA
        public byte BHDriveCurrent; // valid values are 4mA, 8mA, 12mA, 16mA
        public byte BHSchmittInput; // non-zero if BH pins are Schmitt input
        public byte BHSlowSlew; // non-zero if BH pins have slow slew
        public byte BIsHighCurrent;

        public byte BIsVCP;
        public byte BIsVCP7; // non-zero if interface is to use VCP drivers
        public byte BIsVCP8; // non-zero if interface is to use VCP drivers
        public byte BLDriveCurrent; // valid values are 4mA, 8mA, 12mA, 16mA
        public byte BLSchmittInput; // non-zero if BL pins are Schmitt input
        public byte BLSlowSlew; // non-zero if BL pins have slow slew
        public byte BRIIsTxdEn;
        public byte BSchmittInput; // non-zero if AH pins are Schmitt input
        public byte BSlowSlew; // non-zero if AH pins have slow slew
        public byte Cbus0; // Cbus Mux control - Ignored for FT245R
        public byte Cbus0H; // Cbus Mux control
        public byte Cbus1; // Cbus Mux control - Ignored for FT245R
        public byte Cbus1H; // Cbus Mux control
        public byte Cbus2; // Cbus Mux control - Ignored for FT245R
        public byte Cbus2H; // Cbus Mux control
        public byte Cbus3; // Cbus Mux control - Ignored for FT245R
        public byte Cbus3H; // Cbus Mux control
        public byte Cbus4; // Cbus Mux control - Ignored for FT245R
        public byte Cbus4H; // Cbus Mux control
        public byte Cbus5H; // Cbus Mux control
        public byte Cbus6H; // Cbus Mux control
        public byte Cbus7H; // Cbus Mux control
        public byte Cbus8H; // Cbus Mux control
        public byte Cbus9H; // Cbus Mux control
        public byte CDriveCurrent; // valid values are 4mA, 8mA, 12mA, 16mA
        public byte CIsVCP8; // non-zero if interface is to use VCP drivers
        public byte CRIIsTxdEn;
        public byte CSchmittInput; // non-zero if BL pins are Schmitt input
        public byte CSlowSlew; // non-zero if BL pins have slow slew
        public byte DDriveCurrent; // valid values are 4mA, 8mA, 12mA, 16mA
        public nint Description;

        public byte DIsVCP8; // non-zero if interface is to use VCP drivers
        public byte DRIIsTxdEn;
        public byte DSchmittInput; // non-zero if BH pins are Schmitt input
        public byte DSlowSlew; // non-zero if BH pins have slow slew
        public byte EndpointSize;
        public byte FT1248CPolH; // FT1248 clock polarity
        public byte FT1248FlowControlH; // FT1248 flow control enable
        public byte FT1248LsbH; // FT1248 data is LSB (1) or MSB (0)
        public byte HighDriveIOs;
        public byte IFAIsFastSer;
        public byte IFAIsFastSer7; // non-zero if interface is Fast serial
        public byte IFAIsFifo;
        public byte IFAIsFifo7; // non-zero if interface is 245 FIFO
        public byte IFAIsFifoTar;
        public byte IFAIsFifoTar7; // non-zero if interface is 245 FIFO CPU target
        public byte IFBIsFastSer;
        public byte IFBIsFastSer7; // non-zero if interface is Fast serial
        public byte IFBIsFifo;
        public byte IFBIsFifo7; // non-zero if interface is 245 FIFO
        public byte IFBIsFifoTar;
        public byte IFBIsFifoTar7; // non-zero if interface is 245 FIFO CPU target
        public byte InvertCTS; // non-zero if invert CTS
        public byte InvertDCD; // non-zero if invert DCD
        public byte InvertDSR; // non-zero if invert DSR
        public byte InvertDTR; // non-zero if invert DTR
        public byte InvertRI; // non-zero if invert RI
        public byte InvertRTS; // non-zero if invert RTS
        public byte InvertRXD; // non-zero if invert RXD
        public byte InvertTXD; // non-zero if invert TXD
        public byte IsFastSerH; // non-zero if interface is Fast serial
        public byte IsFifoH; // non-zero if interface is 245 FIFO
        public byte IsFifoTarH; // non-zero if interface is 245 FIFO CPU target
        public byte IsFT1248H; // non-zero if interface is FT1248
        public byte IsoIn;
        public byte IsoInA;
        public byte IsoInB;
        public byte IsoOut;
        public byte IsoOutA;
        public byte IsoOutB;
        public byte IsVCPH; // non-zero if interface is to use VCP drivers

        public nint Manufacturer;
        public nint ManufacturerID;

        public ushort MaxPower;
        public ushort PnP;

        public byte PowerSaveEnable; // non-zero if using BCBUS7 to save power for self-powered designs
        public byte PowerSaveEnableH; // non-zero if using ACBUS7 to save power for self-powered designs
        public ushort ProductID;
        public byte PullDownEnable;
        public byte PullDownEnable5;

        // FT2232H extensions
        public byte PullDownEnable7;

        // FT4232H extensions
        public byte PullDownEnable8;

        // FT232H extensions
        public byte PullDownEnableH; // non-zero if pull down enabled
        public byte PullDownEnableR;

        public ushort RemoteWakeup;

        // FT232B extensions
        public byte Rev4;

        // FT2232D extensions
        public byte Rev5;

        public byte RIsD2XX; // Default to loading VCP
        public ushort SelfPowered;
        public nint SerialNumber;
        public byte SerNumEnable;
        public byte SerNumEnable5;
        public byte SerNumEnable7;
        public byte SerNumEnable8;
        public byte SerNumEnableH; // non-zero if serial number to be used
        public byte SerNumEnableR;
        public uint Signature1;
        public uint Signature2;

        public ushort USBVersion;
        public ushort USBVersion5;
        public byte USBVersionEnable;
        public byte USBVersionEnable5;

        // FT232R extensions
        public byte UseExtOsc;
        public ushort VendorID;
        public uint Version;
    }
#pragma warning restore CS0649

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct FtEepromHeader
    {
        public uint deviceType; // FTxxxx device type to be programmed

        // Device descriptor options
        public ushort VendorId; // 0x0403
        public ushort ProductId; // 0x6001

        public byte SerNumEnable; // non-zero if serial number to be used

        // Config descriptor options
        public ushort MaxPower; // 0 < MaxPower <= 500
        public byte SelfPowered; // 0 = bus powered, 1 = self powered

        public byte RemoteWakeup; // 0 = not capable, 1 = capable

        // Hardware options
        public byte PullDownEnable; // non-zero if pull down in suspend enabled
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct FtXSeriesData
    {
        public FtEepromHeader common;

        public byte ACSlowSlew; // non-zero if AC bus pins have slow slew
        public byte ACSchmittInput; // non-zero if AC bus pins are Schmitt input
        public byte ACDriveCurrent; // valid values are 4mA, 8mA, 12mA, 16mA
        public byte ADSlowSlew; // non-zero if AD bus pins have slow slew
        public byte ADSchmittInput; // non-zero if AD bus pins are Schmitt input

        public byte ADDriveCurrent; // valid values are 4mA, 8mA, 12mA, 16mA

        // CBUS options
        public byte Cbus0; // Cbus Mux control
        public byte Cbus1; // Cbus Mux control
        public byte Cbus2; // Cbus Mux control
        public byte Cbus3; // Cbus Mux control
        public byte Cbus4; // Cbus Mux control
        public byte Cbus5; // Cbus Mux control

        public byte Cbus6; // Cbus Mux control

        // UART signal options
        public byte InvertTXD; // non-zero if invert TXD
        public byte InvertRXD; // non-zero if invert RXD
        public byte InvertRTS; // non-zero if invert RTS
        public byte InvertCTS; // non-zero if invert CTS
        public byte InvertDTR; // non-zero if invert DTR
        public byte InvertDSR; // non-zero if invert DSR
        public byte InvertDCD; // non-zero if invert DCD

        public byte InvertRI; // non-zero if invert RI

        // Battery Charge Detect options
        public byte BCDEnable; // Enable Battery Charger Detection
        public byte BCDForceCbusPWREN; // asserts the power enable signal on CBUS when charging port detected

        public byte BCDDisableSleep; // forces the device never to go into sleep mode

        // I2C options
        public ushort I2CSlaveAddress; // I2C slave device address
        public uint I2CDeviceId; // I2C device ID

        public byte I2CDisableSchmitt; // Disable I2C Schmitt trigger

        // FT1248 options
        public byte FT1248CPol; // FT1248 clock polarity - clock idle high (1) or clock idle low (0)
        public byte FT1248Lsb; // FT1248 data is LSB (1) or MSB (0)

        public byte FT1248FlowControl; // FT1248 flow control enable

        // Hardware options
        public byte RS485EchoSuppress; // 

        public byte PowerSaveEnable; // 

        // Driver option
        public byte DriverType; // 
    }

    // Base class for EEPROM structures - these elements are common to all devices
    /// <summary>
    ///     Common EEPROM elements for all devices.  Inherited to specific device type EEPROMs.
    /// </summary>
    public class FtEepromData
    {
        /// <summary>
        ///     Device description string
        /// </summary>
        public string Description = "USB-Serial Converter";

        /// <summary>
        ///     Manufacturer name string
        /// </summary>
        public string Manufacturer = "FTDI";

        /// <summary>
        ///     Manufacturer name abbreviation to be used as a prefix for automatically generated serial numbers
        /// </summary>
        public string ManufacturerId = "FT";

        /// <summary>
        ///     Maximum power the device needs
        /// </summary>
        public ushort MaxPower = 0x0090;

        /// <summary>
        ///     Product ID
        /// </summary>
        public ushort ProductId = 0x6001;

        /// <summary>
        ///     Determines if the device can wake the host PC from suspend by toggling the RI line
        /// </summary>
        public bool RemoteWakeup;

        //private bool PnP                    = true;
        /// <summary>
        ///     Indicates if the device has its own power supply (self-powered) or gets power from the USB port (bus-powered)
        /// </summary>
        public bool SelfPowered;

        /// <summary>
        ///     Device serial number string
        /// </summary>
        public string SerialNumber = "";

        //private const uint Signature1     = 0x00000000;
        //private const uint Signature2     = 0xFFFFFFFF;
        //private const uint Version        = 0x00000002;
        /// <summary>
        ///     Vendor ID as supplied by the USB Implementers Forum
        /// </summary>
        public ushort VendorId = 0x0403;
    }

    // EEPROM class for FT232B and FT245B
    /// <summary>
    ///     EEPROM structure specific to FT232B and FT245B devices.
    ///     Inherits from FT_EEPROM_DATA.
    /// </summary>
    public class Ft232BEepromStructure : FtEepromData
    {
        //private bool Rev4                   = true;
        //private bool IsoIn                  = false;
        //private bool IsoOut                 = false;
        /// <summary>
        ///     Determines if IOs are pulled down when the device is in suspend
        /// </summary>
        public bool PullDownEnable;

        /// <summary>
        ///     Determines if the serial number is enabled
        /// </summary>
        public bool SerNumEnable = true;

        /// <summary>
        ///     The USB version number.  Should be either 0x0110 (USB 1.1) or 0x0200 (USB 2.0)
        /// </summary>
        public ushort UsbVersion = 0x0200;

        /// <summary>
        ///     Determines if the USB version number is enabled
        /// </summary>
        public bool UsbVersionEnable = true;
    }

    // EEPROM class for FT2232C, FT2232L and FT2232D
    /// <summary>
    ///     EEPROM structure specific to FT2232 devices.
    ///     Inherits from FT_EEPROM_DATA.
    /// </summary>
    public class Ft2232EepromStructure : FtEepromData
    {
        /// <summary>
        ///     Enables high current IOs on channel A
        /// </summary>
        public bool AIsHighCurrent;

        /// <summary>
        ///     Determines if channel A loads the VCP driver
        /// </summary>
        public bool AIsVcp = true;

        /// <summary>
        ///     Enables high current IOs on channel B
        /// </summary>
        public bool BIsHighCurrent;

        /// <summary>
        ///     Determines if channel B loads the VCP driver
        /// </summary>
        public bool BIsVcp = true;

        /// <summary>
        ///     Determines if channel A is in fast serial mode
        /// </summary>
        public bool IfaIsFastSer;

        /// <summary>
        ///     Determines if channel A is in FIFO mode
        /// </summary>
        public bool IfaIsFifo;

        /// <summary>
        ///     Determines if channel A is in FIFO target mode
        /// </summary>
        public bool IfaIsFifoTar;

        /// <summary>
        ///     Determines if channel B is in fast serial mode
        /// </summary>
        public bool IfbIsFastSer;

        /// <summary>
        ///     Determines if channel B is in FIFO mode
        /// </summary>
        public bool IfbIsFifo;

        /// <summary>
        ///     Determines if channel B is in FIFO target mode
        /// </summary>
        public bool IfbIsFifoTar;

        //private bool Rev5                   = true;
        //private bool IsoInA                 = false;
        //private bool IsoInB                 = false;
        //private bool IsoOutA                = false;
        //private bool IsoOutB                = false;
        /// <summary>
        ///     Determines if IOs are pulled down when the device is in suspend
        /// </summary>
        public bool PullDownEnable;

        /// <summary>
        ///     Determines if the serial number is enabled
        /// </summary>
        public bool SerNumEnable = true;

        /// <summary>
        ///     The USB version number.  Should be either 0x0110 (USB 1.1) or 0x0200 (USB 2.0)
        /// </summary>
        public ushort UsbVersion = 0x0200;

        /// <summary>
        ///     Determines if the USB version number is enabled
        /// </summary>
        public bool UsbVersionEnable = true;
    }

    // EEPROM class for FT232R and FT245R
    /// <summary>
    ///     EEPROM structure specific to FT232R and FT245R devices.
    ///     Inherits from FT_EEPROM_DATA.
    /// </summary>
    public class Ft232REepromStructure : FtEepromData
    {
        /// <summary>
        ///     Sets the function of the CBUS0 pin for FT232R devices.
        ///     Valid values are FT_CBUS_TxdEn, FT_CBUS_PWRON , FT_CBUS_RXLED, FT_CBUS_TXLED,
        ///     FT_CBUS_TXRXLED, FT_CBUS_SLEEP, FT_CBUS_CLK48, FT_CBUS_CLK24, FT_CBUS_CLK12,
        ///     FT_CBUS_CLK6, FT_CBUS_IOMODE, FT_CBUS_BITBANG_WR, FT_CBUS_BITBANG_RD
        /// </summary>
        public byte Cbus0 = FtCbusOptions.FtCbusSleep;

        /// <summary>
        ///     Sets the function of the CBUS1 pin for FT232R devices.
        ///     Valid values are FT_CBUS_TxdEn, FT_CBUS_PWRON , FT_CBUS_RXLED, FT_CBUS_TXLED,
        ///     FT_CBUS_TXRXLED, FT_CBUS_SLEEP, FT_CBUS_CLK48, FT_CBUS_CLK24, FT_CBUS_CLK12,
        ///     FT_CBUS_CLK6, FT_CBUS_IOMODE, FT_CBUS_BITBANG_WR, FT_CBUS_BITBANG_RD
        /// </summary>
        public byte Cbus1 = FtCbusOptions.FtCbusSleep;

        /// <summary>
        ///     Sets the function of the CBUS2 pin for FT232R devices.
        ///     Valid values are FT_CBUS_TxdEn, FT_CBUS_PWRON , FT_CBUS_RXLED, FT_CBUS_TXLED,
        ///     FT_CBUS_TXRXLED, FT_CBUS_SLEEP, FT_CBUS_CLK48, FT_CBUS_CLK24, FT_CBUS_CLK12,
        ///     FT_CBUS_CLK6, FT_CBUS_IOMODE, FT_CBUS_BITBANG_WR, FT_CBUS_BITBANG_RD
        /// </summary>
        public byte Cbus2 = FtCbusOptions.FtCbusSleep;

        /// <summary>
        ///     Sets the function of the CBUS3 pin for FT232R devices.
        ///     Valid values are FT_CBUS_TxdEn, FT_CBUS_PWRON , FT_CBUS_RXLED, FT_CBUS_TXLED,
        ///     FT_CBUS_TXRXLED, FT_CBUS_SLEEP, FT_CBUS_CLK48, FT_CBUS_CLK24, FT_CBUS_CLK12,
        ///     FT_CBUS_CLK6, FT_CBUS_IOMODE, FT_CBUS_BITBANG_WR, FT_CBUS_BITBANG_RD
        /// </summary>
        public byte Cbus3 = FtCbusOptions.FtCbusSleep;

        /// <summary>
        ///     Sets the function of the CBUS4 pin for FT232R devices.
        ///     Valid values are FT_CBUS_TxdEn, FT_CBUS_PWRON , FT_CBUS_RXLED, FT_CBUS_TXLED,
        ///     FT_CBUS_TXRXLED, FT_CBUS_SLEEP, FT_CBUS_CLK48, FT_CBUS_CLK24, FT_CBUS_CLK12,
        ///     FT_CBUS_CLK6
        /// </summary>
        public byte Cbus4 = FtCbusOptions.FtCbusSleep;

        /// <summary>
        ///     Sets the endpoint size.  This should always be set to 64
        /// </summary>
        public byte EndpointSize = 64;

        /// <summary>
        ///     Enables high current IOs
        /// </summary>
        public bool HighDriveIOs;

        /// <summary>
        ///     Inverts the sense of the CTS line
        /// </summary>
        public bool InvertCts;

        /// <summary>
        ///     Inverts the sense of the DCD line
        /// </summary>
        public bool InvertDcd;

        /// <summary>
        ///     Inverts the sense of the DSR line
        /// </summary>
        public bool InvertDsr;

        /// <summary>
        ///     Inverts the sense of the DTR line
        /// </summary>
        public bool InvertDtr;

        /// <summary>
        ///     Inverts the sense of the RI line
        /// </summary>
        public bool InvertRi;

        /// <summary>
        ///     Inverts the sense of the RTS line
        /// </summary>
        public bool InvertRts;

        /// <summary>
        ///     Inverts the sense of the RXD line
        /// </summary>
        public bool InvertRxd;

        /// <summary>
        ///     Inverts the sense of the TXD line
        /// </summary>
        public bool InvertTxd;

        /// <summary>
        ///     Determines if IOs are pulled down when the device is in suspend
        /// </summary>
        public bool PullDownEnable;

        /// <summary>
        ///     Determines if the VCP driver is loaded
        /// </summary>
        public bool RIsD2Xx;

        /// <summary>
        ///     Determines if the serial number is enabled
        /// </summary>
        public bool SerNumEnable = true;

        /// <summary>
        ///     Disables the FT232R internal clock source.
        ///     If the device has external oscillator enabled it must have an external oscillator fitted to function
        /// </summary>
        public bool UseExtOsc;
    }

    // EEPROM class for FT2232H
    /// <summary>
    ///     EEPROM structure specific to FT2232H devices.
    ///     Inherits from FT_EEPROM_DATA.
    /// </summary>
    public class Ft2232HEepromStructure : FtEepromData
    {
        /// <summary>
        ///     Determines the AH pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA,
        ///     FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
        /// </summary>
        public byte AhDriveCurrent = FtDriveCurrent.FtDriveCurrent4Ma;

        /// <summary>
        ///     Determines if the AH pins have a Schmitt input
        /// </summary>
        public bool AhSchmittInput;

        /// <summary>
        ///     Determines if AH pins have a slow slew rate
        /// </summary>
        public bool AhSlowSlew;

        /// <summary>
        ///     Determines if channel A loads the VCP driver
        /// </summary>
        public bool AIsVcp = true;

        /// <summary>
        ///     Determines the AL pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA,
        ///     FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
        /// </summary>
        public byte AlDriveCurrent = FtDriveCurrent.FtDriveCurrent4Ma;

        /// <summary>
        ///     Determines if the AL pins have a Schmitt input
        /// </summary>
        public bool AlSchmittInput;

        /// <summary>
        ///     Determines if AL pins have a slow slew rate
        /// </summary>
        public bool AlSlowSlew;

        /// <summary>
        ///     Determines the BH pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA,
        ///     FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
        /// </summary>
        public byte BhDriveCurrent = FtDriveCurrent.FtDriveCurrent4Ma;

        /// <summary>
        ///     Determines if the BH pins have a Schmitt input
        /// </summary>
        public bool BhSchmittInput;

        /// <summary>
        ///     Determines if BH pins have a slow slew rate
        /// </summary>
        public bool BhSlowSlew;

        /// <summary>
        ///     Determines if channel B loads the VCP driver
        /// </summary>
        public bool BIsVcp = true;

        /// <summary>
        ///     Determines the BL pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA,
        ///     FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
        /// </summary>
        public byte BlDriveCurrent = FtDriveCurrent.FtDriveCurrent4Ma;

        /// <summary>
        ///     Determines if the BL pins have a Schmitt input
        /// </summary>
        public bool BlSchmittInput;

        /// <summary>
        ///     Determines if BL pins have a slow slew rate
        /// </summary>
        public bool BlSlowSlew;

        /// <summary>
        ///     Determines if channel A is in fast serial mode
        /// </summary>
        public bool IfaIsFastSer;

        /// <summary>
        ///     Determines if channel A is in FIFO mode
        /// </summary>
        public bool IfaIsFifo;

        /// <summary>
        ///     Determines if channel A is in FIFO target mode
        /// </summary>
        public bool IfaIsFifoTar;

        /// <summary>
        ///     Determines if channel B is in fast serial mode
        /// </summary>
        public bool IfbIsFastSer;

        /// <summary>
        ///     Determines if channel B is in FIFO mode
        /// </summary>
        public bool IfbIsFifo;

        /// <summary>
        ///     Determines if channel B is in FIFO target mode
        /// </summary>
        public bool IfbIsFifoTar;

        /// <summary>
        ///     For self-powered designs, keeps the FT2232H in low power state until BCBUS7 is high
        /// </summary>
        public bool PowerSaveEnable;

        /// <summary>
        ///     Determines if IOs are pulled down when the device is in suspend
        /// </summary>
        public bool PullDownEnable;

        /// <summary>
        ///     Determines if the serial number is enabled
        /// </summary>
        public bool SerNumEnable = true;
    }

    // EEPROM class for FT4232H
    /// <summary>
    ///     EEPROM structure specific to FT4232H devices.
    ///     Inherits from FT_EEPROM_DATA.
    /// </summary>
    public class Ft4232HEepromStructure : FtEepromData
    {
        /// <summary>
        ///     Determines the A pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA,
        ///     FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
        /// </summary>
        public byte ADriveCurrent = FtDriveCurrent.FtDriveCurrent4Ma;

        /// <summary>
        ///     Determines if channel A loads the VCP driver
        /// </summary>
        public bool AIsVcp = true;

        /// <summary>
        ///     RI of port A acts as RS485 transmit enable (TxdEn)
        /// </summary>
        public bool AriIsTxdEn;

        /// <summary>
        ///     Determines if the A pins have a Schmitt input
        /// </summary>
        public bool ASchmittInput;

        /// <summary>
        ///     Determines if A pins have a slow slew rate
        /// </summary>
        public bool ASlowSlew;

        /// <summary>
        ///     Determines the B pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA,
        ///     FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
        /// </summary>
        public byte BDriveCurrent = FtDriveCurrent.FtDriveCurrent4Ma;

        /// <summary>
        ///     Determines if channel B loads the VCP driver
        /// </summary>
        public bool BIsVcp = true;

        /// <summary>
        ///     RI of port B acts as RS485 transmit enable (TxdEn)
        /// </summary>
        public bool BriIsTxdEn;

        /// <summary>
        ///     Determines if the B pins have a Schmitt input
        /// </summary>
        public bool BSchmittInput;

        /// <summary>
        ///     Determines if B pins have a slow slew rate
        /// </summary>
        public bool BSlowSlew;

        /// <summary>
        ///     Determines the C pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA,
        ///     FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
        /// </summary>
        public byte CDriveCurrent = FtDriveCurrent.FtDriveCurrent4Ma;

        /// <summary>
        ///     Determines if channel C loads the VCP driver
        /// </summary>
        public bool CIsVcp = true;

        /// <summary>
        ///     RI of port C acts as RS485 transmit enable (TxdEn)
        /// </summary>
        public bool CriIsTxdEn;

        /// <summary>
        ///     Determines if the C pins have a Schmitt input
        /// </summary>
        public bool CSchmittInput;

        /// <summary>
        ///     Determines if C pins have a slow slew rate
        /// </summary>
        public bool CSlowSlew;

        /// <summary>
        ///     Determines the D pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA,
        ///     FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
        /// </summary>
        public byte DDriveCurrent = FtDriveCurrent.FtDriveCurrent4Ma;

        /// <summary>
        ///     Determines if channel D loads the VCP driver
        /// </summary>
        public bool DIsVcp = true;

        /// <summary>
        ///     RI of port D acts as RS485 transmit enable (TxdEn)
        /// </summary>
        public bool DriIsTxdEn;

        /// <summary>
        ///     Determines if the D pins have a Schmitt input
        /// </summary>
        public bool DSchmittInput;

        /// <summary>
        ///     Determines if D pins have a slow slew rate
        /// </summary>
        public bool DSlowSlew;

        /// <summary>
        ///     Determines if IOs are pulled down when the device is in suspend
        /// </summary>
        public bool PullDownEnable;

        /// <summary>
        ///     Determines if the serial number is enabled
        /// </summary>
        public bool SerNumEnable = true;
    }

    // EEPROM class for FT232H
    /// <summary>
    ///     EEPROM structure specific to FT232H devices.
    ///     Inherits from FT_EEPROM_DATA.
    /// </summary>
    public class Ft232HEepromStructure : FtEepromData
    {
        /// <summary>
        ///     Determines the AC pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA,
        ///     FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
        /// </summary>
        public byte AcDriveCurrent = FtDriveCurrent.FtDriveCurrent4Ma;

        /// <summary>
        ///     Determines if the AC pins have a Schmitt input
        /// </summary>
        public bool AcSchmittInput;

        /// <summary>
        ///     Determines if AC pins have a slow slew rate
        /// </summary>
        public bool AcSlowSlew;

        /// <summary>
        ///     Determines the AD pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA,
        ///     FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
        /// </summary>
        public byte AdDriveCurrent = FtDriveCurrent.FtDriveCurrent4Ma;

        /// <summary>
        ///     Determines if the AD pins have a Schmitt input
        /// </summary>
        public bool AdSchmittInput;

        /// <summary>
        ///     Determines if AD pins have a slow slew rate
        /// </summary>
        public bool AdSlowSlew;

        /// <summary>
        ///     Sets the function of the CBUS0 pin for FT232H devices.
        ///     Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
        ///     FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_TxdEn, FT_CBUS_CLK30,
        ///     FT_CBUS_CLK15, FT_CBUS_CLK7_5
        /// </summary>
        public byte Cbus0 = Ft232HCbusOptions.FtCbusTristate;

        /// <summary>
        ///     Sets the function of the CBUS1 pin for FT232H devices.
        ///     Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
        ///     FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_TxdEn, FT_CBUS_CLK30,
        ///     FT_CBUS_CLK15, FT_CBUS_CLK7_5
        /// </summary>
        public byte Cbus1 = Ft232HCbusOptions.FtCbusTristate;

        /// <summary>
        ///     Sets the function of the CBUS2 pin for FT232H devices.
        ///     Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
        ///     FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_TxdEn
        /// </summary>
        public byte Cbus2 = Ft232HCbusOptions.FtCbusTristate;

        /// <summary>
        ///     Sets the function of the CBUS3 pin for FT232H devices.
        ///     Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
        ///     FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_TxdEn
        /// </summary>
        public byte Cbus3 = Ft232HCbusOptions.FtCbusTristate;

        /// <summary>
        ///     Sets the function of the CBUS4 pin for FT232H devices.
        ///     Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
        ///     FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_TxdEn
        /// </summary>
        public byte Cbus4 = Ft232HCbusOptions.FtCbusTristate;

        /// <summary>
        ///     Sets the function of the CBUS5 pin for FT232H devices.
        ///     Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
        ///     FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_IOMODE,
        ///     FT_CBUS_TxdEn, FT_CBUS_CLK30, FT_CBUS_CLK15, FT_CBUS_CLK7_5
        /// </summary>
        public byte Cbus5 = Ft232HCbusOptions.FtCbusTristate;

        /// <summary>
        ///     Sets the function of the CBUS6 pin for FT232H devices.
        ///     Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
        ///     FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_IOMODE,
        ///     FT_CBUS_TxdEn, FT_CBUS_CLK30, FT_CBUS_CLK15, FT_CBUS_CLK7_5
        /// </summary>
        public byte Cbus6 = Ft232HCbusOptions.FtCbusTristate;

        /// <summary>
        ///     Sets the function of the CBUS7 pin for FT232H devices.
        ///     Valid values are FT_CBUS_TRISTATE
        /// </summary>
        public byte Cbus7 = Ft232HCbusOptions.FtCbusTristate;

        /// <summary>
        ///     Sets the function of the CBUS8 pin for FT232H devices.
        ///     Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
        ///     FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_IOMODE,
        ///     FT_CBUS_TxdEn, FT_CBUS_CLK30, FT_CBUS_CLK15, FT_CBUS_CLK7_5
        /// </summary>
        public byte Cbus8 = Ft232HCbusOptions.FtCbusTristate;

        /// <summary>
        ///     Sets the function of the CBUS9 pin for FT232H devices.
        ///     Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
        ///     FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_IOMODE,
        ///     FT_CBUS_TxdEn, FT_CBUS_CLK30, FT_CBUS_CLK15, FT_CBUS_CLK7_5
        /// </summary>
        public byte Cbus9 = Ft232HCbusOptions.FtCbusTristate;

        /// <summary>
        ///     Determines FT1248 mode clock polarity
        /// </summary>
        public bool Ft1248CPol;

        /// <summary>
        ///     Determines if FT1248 mode uses flow control
        /// </summary>
        public bool Ft1248FlowControl;

        /// <summary>
        ///     Determines if data is ent MSB (0) or LSB (1) in FT1248 mode
        /// </summary>
        public bool Ft1248Lsb;

        /// <summary>
        ///     Determines if the device is in fast serial mode
        /// </summary>
        public bool IsFastSer;

        /// <summary>
        ///     Determines if the device is in FIFO mode
        /// </summary>
        public bool IsFifo;

        /// <summary>
        ///     Determines if the device is in FIFO target mode
        /// </summary>
        public bool IsFifoTar;

        /// <summary>
        ///     Determines if the device is in FT1248 mode
        /// </summary>
        public bool IsFt1248;

        /// <summary>
        ///     Determines if the VCP driver is loaded
        /// </summary>
        public bool IsVcp = true;

        /// <summary>
        ///     For self-powered designs, keeps the FT232H in low power state until ACBUS7 is high
        /// </summary>
        public bool PowerSaveEnable;

        /// <summary>
        ///     Determines if IOs are pulled down when the device is in suspend
        /// </summary>
        public bool PullDownEnable;

        /// <summary>
        ///     Determines if the serial number is enabled
        /// </summary>
        public bool SerNumEnable = true;
    }

    /// <summary>
    ///     EEPROM structure specific to X-Series devices.
    ///     Inherits from FT_EEPROM_DATA.
    /// </summary>
    public class FtXSeriesEepromStructure : FtEepromData
    {
        /// <summary>
        ///     Determines the AC pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA,
        ///     FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
        /// </summary>
        public byte AcDriveCurrent;

        /// <summary>
        ///     Determines if the AC pins have a Schmitt input
        /// </summary>
        public byte AcSchmittInput;

        /// <summary>
        ///     Determines if AC pins have a slow slew rate
        /// </summary>
        public byte AcSlowSlew;

        /// <summary>
        ///     Determines the AD pins drive current in mA.  Valid values are FT_DRIVE_CURRENT_4MA, FT_DRIVE_CURRENT_8MA,
        ///     FT_DRIVE_CURRENT_12MA or FT_DRIVE_CURRENT_16MA
        /// </summary>
        public byte AdDriveCurrent;

        /// <summary>
        ///     Determines if AD pins have a schmitt input
        /// </summary>
        public byte AdSchmittInput;

        /// <summary>
        ///     Determines if AD pins have a slow slew rate
        /// </summary>
        public byte AdSlowSlew;

        /// <summary>
        ///     Forces the device never to go into sleep mode.
        /// </summary>
        public byte BcdDisableSleep;

        /// <summary>
        ///     Determines whether the Battery Charge Detection option is enabled.
        /// </summary>
        public byte BcdEnable;

        /// <summary>
        ///     Asserts the power enable signal on CBUS when charging port detected.
        /// </summary>
        public byte BcdForceCbusPwrEn;

        /// <summary>
        ///     Sets the function of the CBUS0 pin for FT232H devices.
        ///     Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
        ///     FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_GPIO, FT_CBUS_TxdEn, FT_CBUS_CLK24,
        ///     FT_CBUS_CLK12, FT_CBUS_CLK6, FT_CBUS_BCD_CHARGER, FT_CBUS_BCD_CHARGER_N, FT_CBUS_VBUS_SENSE, FT_CBUS_BITBANG_WR,
        ///     FT_CBUS_BITBANG_RD, FT_CBUS_TIME_STAMP, FT_CBUS_KEEP_AWAKE
        /// </summary>
        public byte Cbus0;

        /// <summary>
        ///     Sets the function of the CBUS1 pin for FT232H devices.
        ///     Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
        ///     FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_GPIO, FT_CBUS_TxdEn, FT_CBUS_CLK24,
        ///     FT_CBUS_CLK12, FT_CBUS_CLK6, FT_CBUS_BCD_CHARGER, FT_CBUS_BCD_CHARGER_N, FT_CBUS_VBUS_SENSE, FT_CBUS_BITBANG_WR,
        ///     FT_CBUS_BITBANG_RD, FT_CBUS_TIME_STAMP, FT_CBUS_KEEP_AWAKE
        /// </summary>
        public byte Cbus1;

        /// <summary>
        ///     Sets the function of the CBUS2 pin for FT232H devices.
        ///     Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
        ///     FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_GPIO, FT_CBUS_TxdEn, FT_CBUS_CLK24,
        ///     FT_CBUS_CLK12, FT_CBUS_CLK6, FT_CBUS_BCD_CHARGER, FT_CBUS_BCD_CHARGER_N, FT_CBUS_VBUS_SENSE, FT_CBUS_BITBANG_WR,
        ///     FT_CBUS_BITBANG_RD, FT_CBUS_TIME_STAMP, FT_CBUS_KEEP_AWAKE
        /// </summary>
        public byte Cbus2;

        /// <summary>
        ///     Sets the function of the CBUS3 pin for FT232H devices.
        ///     Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
        ///     FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_GPIO, FT_CBUS_TxdEn, FT_CBUS_CLK24,
        ///     FT_CBUS_CLK12, FT_CBUS_CLK6, FT_CBUS_BCD_CHARGER, FT_CBUS_BCD_CHARGER_N, FT_CBUS_VBUS_SENSE, FT_CBUS_BITBANG_WR,
        ///     FT_CBUS_BITBANG_RD, FT_CBUS_TIME_STAMP, FT_CBUS_KEEP_AWAKE
        /// </summary>
        public byte Cbus3;

        /// <summary>
        ///     Sets the function of the CBUS4 pin for FT232H devices.
        ///     Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
        ///     FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_TxdEn, FT_CBUS_CLK24,
        ///     FT_CBUS_CLK12, FT_CBUS_CLK6, FT_CBUS_BCD_CHARGER, FT_CBUS_BCD_CHARGER_N, FT_CBUS_VBUS_SENSE, FT_CBUS_BITBANG_WR,
        ///     FT_CBUS_BITBANG_RD, FT_CBUS_TIME_STAMP, FT_CBUS_KEEP_AWAKE
        /// </summary>
        public byte Cbus4;

        /// <summary>
        ///     Sets the function of the CBUS5 pin for FT232H devices.
        ///     Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
        ///     FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_TxdEn, FT_CBUS_CLK24,
        ///     FT_CBUS_CLK12, FT_CBUS_CLK6, FT_CBUS_BCD_CHARGER, FT_CBUS_BCD_CHARGER_N, FT_CBUS_VBUS_SENSE, FT_CBUS_BITBANG_WR,
        ///     FT_CBUS_BITBANG_RD, FT_CBUS_TIME_STAMP, FT_CBUS_KEEP_AWAKE
        /// </summary>
        public byte Cbus5;

        /// <summary>
        ///     Sets the function of the CBUS6 pin for FT232H devices.
        ///     Valid values are FT_CBUS_TRISTATE, FT_CBUS_RXLED, FT_CBUS_TXLED, FT_CBUS_TXRXLED,
        ///     FT_CBUS_PWREN, FT_CBUS_SLEEP, FT_CBUS_DRIVE_0, FT_CBUS_DRIVE_1, FT_CBUS_TxdEn, FT_CBUS_CLK24,
        ///     FT_CBUS_CLK12, FT_CBUS_CLK6, FT_CBUS_BCD_CHARGER, FT_CBUS_BCD_CHARGER_N, FT_CBUS_VBUS_SENSE, FT_CBUS_BITBANG_WR,
        ///     FT_CBUS_BITBANG_RD, FT_CBUS_TIME_STAMP, FT_CBUS_KEEP_AWAKE
        /// </summary>
        public byte Cbus6;

        /// <summary>
        ///     FT1248 clock polarity - clock idle high (1) or clock idle low (0)
        /// </summary>
        public byte Ft1248CPol;

        /// <summary>
        ///     FT1248 flow control enable.
        /// </summary>
        public byte Ft1248FlowControl;

        /// <summary>
        ///     FT1248 data is LSB (1) or MSB (0)
        /// </summary>
        public byte Ft1248Lsb;

        /// <summary>
        ///     I2C device ID
        /// </summary>
        public uint I2CDeviceId;

        /// <summary>
        ///     Disable I2C Schmitt trigger.
        /// </summary>
        public byte I2CDisableSchmitt;

        /// <summary>
        ///     I2C slave device address.
        /// </summary>
        public ushort I2CSlaveAddress;

        /// <summary>
        ///     Inverts the sense of the CTS line
        /// </summary>
        public byte InvertCts;

        /// <summary>
        ///     Inverts the sense of the DCD line
        /// </summary>
        public byte InvertDcd;

        /// <summary>
        ///     Inverts the sense of the DSR line
        /// </summary>
        public byte InvertDsr;

        /// <summary>
        ///     Inverts the sense of the DTR line
        /// </summary>
        public byte InvertDtr;

        /// <summary>
        ///     Inverts the sense of the RI line
        /// </summary>
        public byte InvertRi;

        /// <summary>
        ///     Inverts the sense of the RTS line
        /// </summary>
        public byte InvertRts;

        /// <summary>
        ///     Inverts the sense of the RXD line
        /// </summary>
        public byte InvertRxd;

        /// <summary>
        ///     Inverts the sense of the TXD line
        /// </summary>
        public byte InvertTxd;

        /// <summary>
        ///     Determines whether the VCP driver is loaded.
        /// </summary>
        public byte IsVcp;

        /// <summary>
        ///     Enable Power Save mode.
        /// </summary>
        public byte PowerSaveEnable;

        /// <summary>
        ///     Determines if IOs are pulled down when the device is in suspend
        /// </summary>
        public bool PullDownEnable;

        /// <summary>
        ///     Enable RS485 Echo Suppression
        /// </summary>
        public byte Rs485EchoSuppress;

        /// <summary>
        ///     Determines if the serial number is enabled
        /// </summary>
        public bool SerNumEnable = true;

        /// <summary>
        ///     The USB version number: 0x0200 (USB 2.0)
        /// </summary>
        public ushort UsbVersion = 0x0200;

        /// <summary>
        ///     Determines if the USB version number is enabled
        /// </summary>
        public bool UsbVersionEnable = true;
    }

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
            byte xon = 0x11;
            byte xoff = 0x13;
            FT_SetFlowControl(_ftHandle, flowControl, xon, xoff);
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
            byte xon = 0x11;
            byte xoff = 0x13;
            FT_SetFlowControl(_ftHandle, flowControl, xon, xoff);
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
            byte xon = 0x11;
            byte xoff = 0x13;
            FT_SetFlowControl(_ftHandle, flowControl, xon, xoff);
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
    public FtStatus Read(byte[] dataBuffer, uint numBytesToRead, out uint numBytesRead)
    {
        var ftStatus = FtStatus.FtOtherError;
        numBytesRead = 0;

        // If the buffer is not big enough to receive the amount of data requested, adjust the number of bytes to read
        if (dataBuffer.Length < numBytesToRead) numBytesToRead = (uint)dataBuffer.Length;

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
    /// <param name="purgemask">
    ///     Specifies which buffer(s) to be purged.  Valid values are any combination of the following
    ///     flags: FT_PURGE_RX, FT_PURGE_TX
    /// </param>
    public FtStatus Purge(uint purgemask)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
            // Call FT_Purge
            ftStatus = FT_Purge(_ftHandle, purgemask);

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
    /// <param name="eventmask">
    ///     The type of events to signal.  Can be any combination of the following: FT_EVENT_RXCHAR,
    ///     FT_EVENT_MODEM_STATUS, FT_EVENT_LINE_STATUS
    /// </param>
    /// <param name="eventhandle">Handle to the event that will receive the notification</param>
    public FtStatus SetEventNotification(uint eventmask, EventWaitHandle eventhandle)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
            // Call FT_SetSetEventNotification
            ftStatus = FT_SetEventNotification(_ftHandle, eventmask, eventhandle.SafeWaitHandle);

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
            var deviceType = FtDevice.FtDeviceUnknown;
            // Set Bit Mode does not apply to FT8U232AM, FT8U245AM or FT8U100AX devices
            GetDeviceType(ref deviceType);
            FtError ftErrorCondition;
            if (deviceType == FtDevice.FtDeviceAm)
            {
                // Throw an exception
                ftErrorCondition = FtError.FtInvalidBitmode;
                ErrorHandler(ftStatus, ftErrorCondition);
            }
            else if (deviceType == FtDevice.FtDevice100Ax)
            {
                // Throw an exception
                ftErrorCondition = FtError.FtInvalidBitmode;
                ErrorHandler(ftStatus, ftErrorCondition);
            }
            else if (deviceType == FtDevice.FtDeviceBm && bitMode != FtBitModes.FtBitModeReset)
            {
                if ((bitMode & FtBitModes.FtBitModeAsyncBitBang) == 0)
                {
                    // Throw an exception
                    ftErrorCondition = FtError.FtInvalidBitmode;
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
                    ftErrorCondition = FtError.FtInvalidBitmode;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }

                if ((bitMode == FtBitModes.FtBitModeMpsse) & (InterfaceIdentifier != "A"))
                {
                    // MPSSE mode is only available on channel A
                    // Throw an exception
                    ftErrorCondition = FtError.FtInvalidBitmode;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }
            }
            else if (deviceType == FtDevice.FtDevice232R && bitMode != FtBitModes.FtBitModeReset)
            {
                if ((bitMode & (FtBitModes.FtBitModeAsyncBitBang | FtBitModes.FtBitModeSyncBitBang |
                                FtBitModes.FtBitModeCbusBitBang)) == 0)
                {
                    // Throw an exception
                    ftErrorCondition = FtError.FtInvalidBitmode;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }
            }
            else if ((deviceType == FtDevice.FtDevice2232H
                      || deviceType == FtDevice.FtDevice2232Hp
                      || deviceType == FtDevice.FtDevice2233Hp
                      || deviceType == FtDevice.FtDevice2232Ha)
                     && bitMode != FtBitModes.FtBitModeReset)
            {
                if ((bitMode & (FtBitModes.FtBitModeAsyncBitBang | FtBitModes.FtBitModeMpsse |
                                FtBitModes.FtBitModeSyncBitBang | FtBitModes.FtBitModeMcuHost |
                                FtBitModes.FtBitModeFastSerial | FtBitModes.FtBitModeSyncFifo)) ==
                    0)
                {
                    // Throw an exception
                    ftErrorCondition = FtError.FtInvalidBitmode;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }

                if (((bitMode == FtBitModes.FtBitModeMcuHost) |
                     (bitMode == FtBitModes.FtBitModeSyncFifo)) & (InterfaceIdentifier != "A"))
                {
                    // MCU Host Emulation and Single channel synchronous 245 FIFO mode is only available on channel A
                    // Throw an exception
                    ftErrorCondition = FtError.FtInvalidBitmode;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }
            }
            else if ((deviceType == FtDevice.FtDevice4232H
                      || deviceType == FtDevice.FtDevice4232Hp
                      || deviceType == FtDevice.FtDevice4233Hp
                      || deviceType == FtDevice.FtDevice4232Ha)
                     && bitMode != FtBitModes.FtBitModeReset)
            {
                if ((bitMode & (FtBitModes.FtBitModeAsyncBitBang | FtBitModes.FtBitModeMpsse |
                                FtBitModes.FtBitModeSyncBitBang)) == 0)
                {
                    // Throw an exception
                    ftErrorCondition = FtError.FtInvalidBitmode;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }

                if ((bitMode == FtBitModes.FtBitModeMpsse) &
                    (InterfaceIdentifier != "A") & (InterfaceIdentifier != "B"))
                {
                    // MPSSE mode is only available on channel A and B
                    // Throw an exception
                    ftErrorCondition = FtError.FtInvalidBitmode;
                    ErrorHandler(ftStatus, ftErrorCondition);
                }
            }
            else if ((deviceType == FtDevice.FtDevice232H
                      || deviceType == FtDevice.FtDevice232Hp
                      || deviceType == FtDevice.FtDevice233Hp)
                     && bitMode != FtBitModes.FtBitModeReset)
            {
                // FT232H supports all current bit modes!
                if (bitMode > FtBitModes.FtBitModeSyncFifo)
                {
                    // Throw an exception
                    ftErrorCondition = FtError.FtInvalidBitmode;
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
    // ReadEEPROMLocation
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Reads an individual word value from a specified location in the device's EEPROM.
    /// </summary>
    /// <returns>FT_STATUS value from FT_ReadEE in FTD2XX.DLL</returns>
    /// <param name="address">The EEPROM location to read data from</param>
    /// <param name="eeValue">The WORD value read from the EEPROM location specified in the Address paramter</param>
    public FtStatus ReadEepromLocation(uint address, ref ushort eeValue)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
            // Call FT_ReadEE
            ftStatus = FT_ReadEE(_ftHandle, address, ref eeValue);

        return ftStatus;
    }

    //**************************************************************************
    // WriteEEPROMLocation
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Writes an individual word value to a specified location in the device's EEPROM.
    /// </summary>
    /// <returns>FT_STATUS value from FT_WriteEE in FTD2XX.DLL</returns>
    /// <param name="address">The EEPROM location to read data from</param>
    /// <param name="eeValue">The WORD value to write to the EEPROM location specified by the Address parameter</param>
    public FtStatus WriteEepromLocation(uint address, ushort eeValue)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
            // Call FT_WriteEE
            ftStatus = FT_WriteEE(_ftHandle, address, eeValue);

        return ftStatus;
    }

    //**************************************************************************
    // EraseEEPROM
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Erases the device EEPROM.
    /// </summary>
    /// <returns>FT_STATUS value from FT_EraseEE in FTD2XX.DLL</returns>
    /// <exception cref="FtException">
    ///     Thrown when attempting to erase the EEPROM of a device with an internal EEPROM such as
    ///     an FT232R or FT245R.
    /// </exception>
    public FtStatus EraseEeprom()
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
        {
            var deviceType = FtDevice.FtDeviceUnknown;
            // Check that it is not an FT232R or FT245R that we are trying to erase
            GetDeviceType(ref deviceType);
            if (deviceType == FtDevice.FtDevice232R)
            {
                // If it is a device with an internal EEPROM, throw an exception
                var ftErrorCondition = FtError.FtIncorrectDevice;
                ErrorHandler(ftStatus, ftErrorCondition);
            }

            // Call FT_EraseEE
            ftStatus = FT_EraseEE(_ftHandle);
        }

        return ftStatus;
    }

    //**************************************************************************
    // ReadFT232BEEPROM
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Reads the EEPROM contents of an FT232B or FT245B device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_EE_Read in FTD2XX DLL</returns>
    /// <param name="ee232B">
    ///     An FT232B_EEPROM_STRUCTURE which contains only the relevant information for an FT232B and FT245B
    ///     device.
    /// </param>
    /// <exception cref="FtException">Thrown when the current device does not match the type required by this method.</exception>
    public FtStatus ReadFt232Beeprom(Ft232BEepromStructure ee232B)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
        {
            var deviceType = FtDevice.FtDeviceUnknown;
            // Check that it is an FT232B or FT245B that we are trying to read
            GetDeviceType(ref deviceType);
            if (deviceType != FtDevice.FtDeviceBm)
            {
                // If it is not, throw an exception
                var ftErrorCondition = FtError.FtIncorrectDevice;
                ErrorHandler(ftStatus, ftErrorCondition);
            }

            var eeData = new FtProgramData
            {
                // Set up structure headers
                Signature1 = 0x00000000,
                Signature2 = 0xFFFFFFFF,
                Version = 2,
                // Allocate space from unmanaged heap
                Manufacturer = Marshal.AllocHGlobal(32),
                ManufacturerID = Marshal.AllocHGlobal(16),
                Description = Marshal.AllocHGlobal(64),
                SerialNumber = Marshal.AllocHGlobal(16)
            };

            // Call FT_EE_Read
            ftStatus = FT_EE_Read(_ftHandle, eeData);

            // Retrieve string values
            ee232B.Manufacturer = Marshal.PtrToStringAnsi(eeData.Manufacturer)!;
            ee232B.ManufacturerId = Marshal.PtrToStringAnsi(eeData.ManufacturerID)!;
            ee232B.Description = Marshal.PtrToStringAnsi(eeData.Description)!;
            ee232B.SerialNumber = Marshal.PtrToStringAnsi(eeData.SerialNumber)!;

            // Free unmanaged buffers
            Marshal.FreeHGlobal(eeData.Manufacturer);
            Marshal.FreeHGlobal(eeData.ManufacturerID);
            Marshal.FreeHGlobal(eeData.Description);
            Marshal.FreeHGlobal(eeData.SerialNumber);

            // Map non-string elements to structure to be returned
            // Standard elements
            ee232B.VendorId = eeData.VendorID;
            ee232B.ProductId = eeData.ProductID;
            ee232B.MaxPower = eeData.MaxPower;
            ee232B.SelfPowered = Convert.ToBoolean(eeData.SelfPowered);
            ee232B.RemoteWakeup = Convert.ToBoolean(eeData.RemoteWakeup);
            // B specific fields
            ee232B.PullDownEnable = Convert.ToBoolean(eeData.PullDownEnable);
            ee232B.SerNumEnable = Convert.ToBoolean(eeData.SerNumEnable);
            ee232B.UsbVersionEnable = Convert.ToBoolean(eeData.USBVersionEnable);
            ee232B.UsbVersion = eeData.USBVersion;
        }

        return ftStatus;
    }

    //**************************************************************************
    // ReadFT2232EEPROM
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Reads the EEPROM contents of an FT2232 device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_EE_Read in FTD2XX DLL</returns>
    /// <param name="ee2232">An FT2232_EEPROM_STRUCTURE which contains only the relevant information for an FT2232 device.</param>
    /// <exception cref="FtException">Thrown when the current device does not match the type required by this method.</exception>
    public FtStatus ReadFt2232Eeprom(Ft2232EepromStructure ee2232)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
        {
            var deviceType = FtDevice.FtDeviceUnknown;
            // Check that it is an FT2232 that we are trying to read
            GetDeviceType(ref deviceType);
            if (deviceType != FtDevice.FtDevice2232)
            {
                // If it is not, throw an exception
                var ftErrorCondition = FtError.FtIncorrectDevice;
                ErrorHandler(ftStatus, ftErrorCondition);
            }

            var eeData = new FtProgramData();

            // Set up structure headers
            eeData.Signature1 = 0x00000000;
            eeData.Signature2 = 0xFFFFFFFF;
            eeData.Version = 2;

            // Allocate space from unmanaged heap
            eeData.Manufacturer = Marshal.AllocHGlobal(32);
            eeData.ManufacturerID = Marshal.AllocHGlobal(16);
            eeData.Description = Marshal.AllocHGlobal(64);
            eeData.SerialNumber = Marshal.AllocHGlobal(16);

            // Call FT_EE_Read
            ftStatus = FT_EE_Read(_ftHandle, eeData);

            // Retrieve string values
            ee2232.Manufacturer = Marshal.PtrToStringAnsi(eeData.Manufacturer)!;
            ee2232.ManufacturerId = Marshal.PtrToStringAnsi(eeData.ManufacturerID)!;
            ee2232.Description = Marshal.PtrToStringAnsi(eeData.Description)!;
            ee2232.SerialNumber = Marshal.PtrToStringAnsi(eeData.SerialNumber)!;

            // Free unmanaged buffers
            Marshal.FreeHGlobal(eeData.Manufacturer);
            Marshal.FreeHGlobal(eeData.ManufacturerID);
            Marshal.FreeHGlobal(eeData.Description);
            Marshal.FreeHGlobal(eeData.SerialNumber);

            // Map non-string elements to structure to be returned
            // Standard elements
            ee2232.VendorId = eeData.VendorID;
            ee2232.ProductId = eeData.ProductID;
            ee2232.MaxPower = eeData.MaxPower;
            ee2232.SelfPowered = Convert.ToBoolean(eeData.SelfPowered);
            ee2232.RemoteWakeup = Convert.ToBoolean(eeData.RemoteWakeup);
            // 2232 specific fields
            ee2232.PullDownEnable = Convert.ToBoolean(eeData.PullDownEnable5);
            ee2232.SerNumEnable = Convert.ToBoolean(eeData.SerNumEnable5);
            ee2232.UsbVersionEnable = Convert.ToBoolean(eeData.USBVersionEnable5);
            ee2232.UsbVersion = eeData.USBVersion5;
            ee2232.AIsHighCurrent = Convert.ToBoolean(eeData.AIsHighCurrent);
            ee2232.BIsHighCurrent = Convert.ToBoolean(eeData.BIsHighCurrent);
            ee2232.IfaIsFifo = Convert.ToBoolean(eeData.IFAIsFifo);
            ee2232.IfaIsFifoTar = Convert.ToBoolean(eeData.IFAIsFifoTar);
            ee2232.IfaIsFastSer = Convert.ToBoolean(eeData.IFAIsFastSer);
            ee2232.AIsVcp = Convert.ToBoolean(eeData.AIsVCP);
            ee2232.IfbIsFifo = Convert.ToBoolean(eeData.IFBIsFifo);
            ee2232.IfbIsFifoTar = Convert.ToBoolean(eeData.IFBIsFifoTar);
            ee2232.IfbIsFastSer = Convert.ToBoolean(eeData.IFBIsFastSer);
            ee2232.BIsVcp = Convert.ToBoolean(eeData.BIsVCP);
        }

        return ftStatus;
    }

    //**************************************************************************
    // ReadFT232REEPROM
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Reads the EEPROM contents of an FT232R or FT245R device.
    ///     Calls FT_EE_Read in FTD2XX DLL
    /// </summary>
    /// <returns>An FT232R_EEPROM_STRUCTURE which contains only the relevant information for an FT232R and FT245R device.</returns>
    /// <exception cref="FtException">Thrown when the current device does not match the type required by this method.</exception>
    public FtStatus ReadFt232Reeprom(Ft232REepromStructure ee232R)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
        {
            var deviceType = FtDevice.FtDeviceUnknown;
            // Check that it is an FT232R or FT245R that we are trying to read
            GetDeviceType(ref deviceType);
            if (deviceType != FtDevice.FtDevice232R)
            {
                // If it is not, throw an exception
                var ftErrorCondition = FtError.FtIncorrectDevice;
                ErrorHandler(ftStatus, ftErrorCondition);
            }

            var eeData = new FtProgramData();

            // Set up structure headers
            eeData.Signature1 = 0x00000000;
            eeData.Signature2 = 0xFFFFFFFF;
            eeData.Version = 2;

            // Allocate space from unmanaged heap
            eeData.Manufacturer = Marshal.AllocHGlobal(32);
            eeData.ManufacturerID = Marshal.AllocHGlobal(16);
            eeData.Description = Marshal.AllocHGlobal(64);
            eeData.SerialNumber = Marshal.AllocHGlobal(16);

            // Call FT_EE_Read
            ftStatus = FT_EE_Read(_ftHandle, eeData);

            // Retrieve string values
            ee232R.Manufacturer = Marshal.PtrToStringAnsi(eeData.Manufacturer)!;
            ee232R.ManufacturerId = Marshal.PtrToStringAnsi(eeData.ManufacturerID)!;
            ee232R.Description = Marshal.PtrToStringAnsi(eeData.Description)!;
            ee232R.SerialNumber = Marshal.PtrToStringAnsi(eeData.SerialNumber)!;

            // Free unmanaged buffers
            Marshal.FreeHGlobal(eeData.Manufacturer);
            Marshal.FreeHGlobal(eeData.ManufacturerID);
            Marshal.FreeHGlobal(eeData.Description);
            Marshal.FreeHGlobal(eeData.SerialNumber);

            // Map non-string elements to structure to be returned
            // Standard elements
            ee232R.VendorId = eeData.VendorID;
            ee232R.ProductId = eeData.ProductID;
            ee232R.MaxPower = eeData.MaxPower;
            ee232R.SelfPowered = Convert.ToBoolean(eeData.SelfPowered);
            ee232R.RemoteWakeup = Convert.ToBoolean(eeData.RemoteWakeup);
            // 232R specific fields
            ee232R.UseExtOsc = Convert.ToBoolean(eeData.UseExtOsc);
            ee232R.HighDriveIOs = Convert.ToBoolean(eeData.HighDriveIOs);
            ee232R.EndpointSize = eeData.EndpointSize;
            ee232R.PullDownEnable = Convert.ToBoolean(eeData.PullDownEnableR);
            ee232R.SerNumEnable = Convert.ToBoolean(eeData.SerNumEnableR);
            ee232R.InvertTxd = Convert.ToBoolean(eeData.InvertTXD);
            ee232R.InvertRxd = Convert.ToBoolean(eeData.InvertRXD);
            ee232R.InvertRts = Convert.ToBoolean(eeData.InvertRTS);
            ee232R.InvertCts = Convert.ToBoolean(eeData.InvertCTS);
            ee232R.InvertDtr = Convert.ToBoolean(eeData.InvertDTR);
            ee232R.InvertDsr = Convert.ToBoolean(eeData.InvertDSR);
            ee232R.InvertDcd = Convert.ToBoolean(eeData.InvertDCD);
            ee232R.InvertRi = Convert.ToBoolean(eeData.InvertRI);
            ee232R.Cbus0 = eeData.Cbus0;
            ee232R.Cbus1 = eeData.Cbus1;
            ee232R.Cbus2 = eeData.Cbus2;
            ee232R.Cbus3 = eeData.Cbus3;
            ee232R.Cbus4 = eeData.Cbus4;
            ee232R.RIsD2Xx = Convert.ToBoolean(eeData.RIsD2XX);
        }

        return ftStatus;
    }

    //**************************************************************************
    // ReadFT2232HEEPROM
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Reads the EEPROM contents of an FT2232H device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_EE_Read in FTD2XX DLL</returns>
    /// <param name="ee2232H">An FT2232H_EEPROM_STRUCTURE which contains only the relevant information for an FT2232H device.</param>
    /// <exception cref="FtException">Thrown when the current device does not match the type required by this method.</exception>
    public FtStatus ReadFt2232Heeprom(Ft2232HEepromStructure ee2232H)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
        {
            var deviceType = FtDevice.FtDeviceUnknown;
            // Check that it is an FT2232H that we are trying to read
            GetDeviceType(ref deviceType);
            if (deviceType != FtDevice.FtDevice2232H)
            {
                // If it is not, throw an exception
                var ftErrorCondition = FtError.FtIncorrectDevice;
                ErrorHandler(ftStatus, ftErrorCondition);
            }

            var eeData = new FtProgramData();

            // Set up structure headers
            eeData.Signature1 = 0x00000000;
            eeData.Signature2 = 0xFFFFFFFF;
            eeData.Version = 3;

            // Allocate space from unmanaged heap
            eeData.Manufacturer = Marshal.AllocHGlobal(32);
            eeData.ManufacturerID = Marshal.AllocHGlobal(16);
            eeData.Description = Marshal.AllocHGlobal(64);
            eeData.SerialNumber = Marshal.AllocHGlobal(16);

            // Call FT_EE_Read
            ftStatus = FT_EE_Read(_ftHandle, eeData);

            // Retrieve string values
            ee2232H.Manufacturer = Marshal.PtrToStringAnsi(eeData.Manufacturer)!;
            ee2232H.ManufacturerId = Marshal.PtrToStringAnsi(eeData.ManufacturerID)!;
            ee2232H.Description = Marshal.PtrToStringAnsi(eeData.Description)!;
            ee2232H.SerialNumber = Marshal.PtrToStringAnsi(eeData.SerialNumber)!;

            // Free unmanaged buffers
            Marshal.FreeHGlobal(eeData.Manufacturer);
            Marshal.FreeHGlobal(eeData.ManufacturerID);
            Marshal.FreeHGlobal(eeData.Description);
            Marshal.FreeHGlobal(eeData.SerialNumber);

            // Map non-string elements to structure to be returned
            // Standard elements
            ee2232H.VendorId = eeData.VendorID;
            ee2232H.ProductId = eeData.ProductID;
            ee2232H.MaxPower = eeData.MaxPower;
            ee2232H.SelfPowered = Convert.ToBoolean(eeData.SelfPowered);
            ee2232H.RemoteWakeup = Convert.ToBoolean(eeData.RemoteWakeup);
            // 2232H specific fields
            ee2232H.PullDownEnable = Convert.ToBoolean(eeData.PullDownEnable7);
            ee2232H.SerNumEnable = Convert.ToBoolean(eeData.SerNumEnable7);
            ee2232H.AlSlowSlew = Convert.ToBoolean(eeData.ALSlowSlew);
            ee2232H.AlSchmittInput = Convert.ToBoolean(eeData.ALSchmittInput);
            ee2232H.AlDriveCurrent = eeData.ALDriveCurrent;
            ee2232H.AhSlowSlew = Convert.ToBoolean(eeData.AHSlowSlew);
            ee2232H.AhSchmittInput = Convert.ToBoolean(eeData.AHSchmittInput);
            ee2232H.AhDriveCurrent = eeData.AHDriveCurrent;
            ee2232H.BlSlowSlew = Convert.ToBoolean(eeData.BLSlowSlew);
            ee2232H.BlSchmittInput = Convert.ToBoolean(eeData.BLSchmittInput);
            ee2232H.BlDriveCurrent = eeData.BLDriveCurrent;
            ee2232H.BhSlowSlew = Convert.ToBoolean(eeData.BHSlowSlew);
            ee2232H.BhSchmittInput = Convert.ToBoolean(eeData.BHSchmittInput);
            ee2232H.BhDriveCurrent = eeData.BHDriveCurrent;
            ee2232H.IfaIsFifo = Convert.ToBoolean(eeData.IFAIsFifo7);
            ee2232H.IfaIsFifoTar = Convert.ToBoolean(eeData.IFAIsFifoTar7);
            ee2232H.IfaIsFastSer = Convert.ToBoolean(eeData.IFAIsFastSer7);
            ee2232H.AIsVcp = Convert.ToBoolean(eeData.AIsVCP7);
            ee2232H.IfbIsFifo = Convert.ToBoolean(eeData.IFBIsFifo7);
            ee2232H.IfbIsFifoTar = Convert.ToBoolean(eeData.IFBIsFifoTar7);
            ee2232H.IfbIsFastSer = Convert.ToBoolean(eeData.IFBIsFastSer7);
            ee2232H.BIsVcp = Convert.ToBoolean(eeData.BIsVCP7);
            ee2232H.PowerSaveEnable = Convert.ToBoolean(eeData.PowerSaveEnable);
        }

        return ftStatus;
    }

    //**************************************************************************
    // ReadFT4232HEEPROM
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Reads the EEPROM contents of an FT4232H device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_EE_Read in FTD2XX DLL</returns>
    /// <param name="ee4232H">An FT4232H_EEPROM_STRUCTURE which contains only the relevant information for an FT4232H device.</param>
    /// <exception cref="FtException">Thrown when the current device does not match the type required by this method.</exception>
    public FtStatus ReadFt4232Heeprom(Ft4232HEepromStructure ee4232H)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
        {
            var deviceType = FtDevice.FtDeviceUnknown;
            // Check that it is an FT4232H that we are trying to read
            GetDeviceType(ref deviceType);
            if (deviceType != FtDevice.FtDevice4232H)
            {
                // If it is not, throw an exception
                var ftErrorCondition = FtError.FtIncorrectDevice;
                ErrorHandler(ftStatus, ftErrorCondition);
            }

            var eeData = new FtProgramData();

            // Set up structure headers
            eeData.Signature1 = 0x00000000;
            eeData.Signature2 = 0xFFFFFFFF;
            eeData.Version = 4;

            // Allocate space from unmanaged heap
            eeData.Manufacturer = Marshal.AllocHGlobal(32);
            eeData.ManufacturerID = Marshal.AllocHGlobal(16);
            eeData.Description = Marshal.AllocHGlobal(64);
            eeData.SerialNumber = Marshal.AllocHGlobal(16);

            // Call FT_EE_Read
            ftStatus = FT_EE_Read(_ftHandle, eeData);

            // Retrieve string values
            ee4232H.Manufacturer = Marshal.PtrToStringAnsi(eeData.Manufacturer)!;
            ee4232H.ManufacturerId = Marshal.PtrToStringAnsi(eeData.ManufacturerID)!;
            ee4232H.Description = Marshal.PtrToStringAnsi(eeData.Description)!;
            ee4232H.SerialNumber = Marshal.PtrToStringAnsi(eeData.SerialNumber)!;

            // Free unmanaged buffers
            Marshal.FreeHGlobal(eeData.Manufacturer);
            Marshal.FreeHGlobal(eeData.ManufacturerID);
            Marshal.FreeHGlobal(eeData.Description);
            Marshal.FreeHGlobal(eeData.SerialNumber);

            // Map non-string elements to structure to be returned
            // Standard elements
            ee4232H.VendorId = eeData.VendorID;
            ee4232H.ProductId = eeData.ProductID;
            ee4232H.MaxPower = eeData.MaxPower;
            ee4232H.SelfPowered = Convert.ToBoolean(eeData.SelfPowered);
            ee4232H.RemoteWakeup = Convert.ToBoolean(eeData.RemoteWakeup);
            // 4232H specific fields
            ee4232H.PullDownEnable = Convert.ToBoolean(eeData.PullDownEnable8);
            ee4232H.SerNumEnable = Convert.ToBoolean(eeData.SerNumEnable8);
            ee4232H.ASlowSlew = Convert.ToBoolean(eeData.ASlowSlew);
            ee4232H.ASchmittInput = Convert.ToBoolean(eeData.ASchmittInput);
            ee4232H.ADriveCurrent = eeData.ADriveCurrent;
            ee4232H.BSlowSlew = Convert.ToBoolean(eeData.BSlowSlew);
            ee4232H.BSchmittInput = Convert.ToBoolean(eeData.BSchmittInput);
            ee4232H.BDriveCurrent = eeData.BDriveCurrent;
            ee4232H.CSlowSlew = Convert.ToBoolean(eeData.CSlowSlew);
            ee4232H.CSchmittInput = Convert.ToBoolean(eeData.CSchmittInput);
            ee4232H.CDriveCurrent = eeData.CDriveCurrent;
            ee4232H.DSlowSlew = Convert.ToBoolean(eeData.DSlowSlew);
            ee4232H.DSchmittInput = Convert.ToBoolean(eeData.DSchmittInput);
            ee4232H.DDriveCurrent = eeData.DDriveCurrent;
            ee4232H.AriIsTxdEn = Convert.ToBoolean(eeData.ARIIsTxdEn);
            ee4232H.BriIsTxdEn = Convert.ToBoolean(eeData.BRIIsTxdEn);
            ee4232H.CriIsTxdEn = Convert.ToBoolean(eeData.CRIIsTxdEn);
            ee4232H.DriIsTxdEn = Convert.ToBoolean(eeData.DRIIsTxdEn);
            ee4232H.AIsVcp = Convert.ToBoolean(eeData.AIsVCP8);
            ee4232H.BIsVcp = Convert.ToBoolean(eeData.BIsVCP8);
            ee4232H.CIsVcp = Convert.ToBoolean(eeData.CIsVCP8);
            ee4232H.DIsVcp = Convert.ToBoolean(eeData.DIsVCP8);
        }

        return ftStatus;
    }

    //**************************************************************************
    // ReadFT232HEEPROM
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Reads the EEPROM contents of an FT232H device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_EE_Read in FTD2XX DLL</returns>
    /// <param name="ee232H">An FT232H_EEPROM_STRUCTURE which contains only the relevant information for an FT232H device.</param>
    /// <exception cref="FtException">Thrown when the current device does not match the type required by this method.</exception>
    public FtStatus ReadFt232Heeprom(Ft232HEepromStructure ee232H)
    {
        var ftStatus = FtStatus.FtOtherError;
        var ftErrorCondition = FtError.FtNoError;

        if (_ftHandle != nint.Zero)
        {
            var deviceType = FtDevice.FtDeviceUnknown;
            // Check that it is an FT232H that we are trying to read
            GetDeviceType(ref deviceType);
            if (deviceType != FtDevice.FtDevice232H
                && deviceType != FtDevice.FtDevice232Hp
                && deviceType != FtDevice.FtDevice233Hp)
            {
                // If it is not, throw an exception
                ftErrorCondition = FtError.FtIncorrectDevice;
                ErrorHandler(ftStatus, ftErrorCondition);
            }

            var eeData = new FtProgramData();

            // Set up structure headers
            eeData.Signature1 = 0x00000000;
            eeData.Signature2 = 0xFFFFFFFF;
            eeData.Version = 5;

            // Allocate space from unmanaged heap
            eeData.Manufacturer = Marshal.AllocHGlobal(32);
            eeData.ManufacturerID = Marshal.AllocHGlobal(16);
            eeData.Description = Marshal.AllocHGlobal(64);
            eeData.SerialNumber = Marshal.AllocHGlobal(16);

            // Call FT_EE_Read
            ftStatus = FT_EE_Read(_ftHandle, eeData);

            // Retrieve string values
            ee232H.Manufacturer = Marshal.PtrToStringAnsi(eeData.Manufacturer)!;
            ee232H.ManufacturerId = Marshal.PtrToStringAnsi(eeData.ManufacturerID)!;
            ee232H.Description = Marshal.PtrToStringAnsi(eeData.Description)!;
            ee232H.SerialNumber = Marshal.PtrToStringAnsi(eeData.SerialNumber)!;

            // Free unmanaged buffers
            Marshal.FreeHGlobal(eeData.Manufacturer);
            Marshal.FreeHGlobal(eeData.ManufacturerID);
            Marshal.FreeHGlobal(eeData.Description);
            Marshal.FreeHGlobal(eeData.SerialNumber);

            // Map non-string elements to structure to be returned
            // Standard elements
            ee232H.VendorId = eeData.VendorID;
            ee232H.ProductId = eeData.ProductID;
            ee232H.MaxPower = eeData.MaxPower;
            ee232H.SelfPowered = Convert.ToBoolean(eeData.SelfPowered);
            ee232H.RemoteWakeup = Convert.ToBoolean(eeData.RemoteWakeup);
            // 232H specific fields
            ee232H.PullDownEnable = Convert.ToBoolean(eeData.PullDownEnableH);
            ee232H.SerNumEnable = Convert.ToBoolean(eeData.SerNumEnableH);
            ee232H.AcSlowSlew = Convert.ToBoolean(eeData.ACSlowSlewH);
            ee232H.AcSchmittInput = Convert.ToBoolean(eeData.ACSchmittInputH);
            ee232H.AcDriveCurrent = eeData.ACDriveCurrentH;
            ee232H.AdSlowSlew = Convert.ToBoolean(eeData.ADSlowSlewH);
            ee232H.AdSchmittInput = Convert.ToBoolean(eeData.ADSchmittInputH);
            ee232H.AdDriveCurrent = eeData.ADDriveCurrentH;
            ee232H.Cbus0 = eeData.Cbus0H;
            ee232H.Cbus1 = eeData.Cbus1H;
            ee232H.Cbus2 = eeData.Cbus2H;
            ee232H.Cbus3 = eeData.Cbus3H;
            ee232H.Cbus4 = eeData.Cbus4H;
            ee232H.Cbus5 = eeData.Cbus5H;
            ee232H.Cbus6 = eeData.Cbus6H;
            ee232H.Cbus7 = eeData.Cbus7H;
            ee232H.Cbus8 = eeData.Cbus8H;
            ee232H.Cbus9 = eeData.Cbus9H;
            ee232H.IsFifo = Convert.ToBoolean(eeData.IsFifoH);
            ee232H.IsFifoTar = Convert.ToBoolean(eeData.IsFifoTarH);
            ee232H.IsFastSer = Convert.ToBoolean(eeData.IsFastSerH);
            ee232H.IsFt1248 = Convert.ToBoolean(eeData.IsFT1248H);
            ee232H.Ft1248CPol = Convert.ToBoolean(eeData.FT1248CPolH);
            ee232H.Ft1248Lsb = Convert.ToBoolean(eeData.FT1248LsbH);
            ee232H.Ft1248FlowControl = Convert.ToBoolean(eeData.FT1248FlowControlH);
            ee232H.IsVcp = Convert.ToBoolean(eeData.IsVCPH);
            ee232H.PowerSaveEnable = Convert.ToBoolean(eeData.PowerSaveEnableH);
        }

        return ftStatus;
    }

    //**************************************************************************
    // ReadXSeriesEEPROM
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Reads the EEPROM contents of an X-Series device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_EEPROM_Read in FTD2XX DLL</returns>
    /// <param name="eeX">An FT_XSERIES_EEPROM_STRUCTURE which contains only the relevant information for an X-Series device.</param>
    /// <exception cref="FtException">Thrown when the current device does not match the type required by this method.</exception>
    public FtStatus ReadXSeriesEeprom(FtXSeriesEepromStructure eeX)
    {
        var ftStatus = FtStatus.FtOtherError;
        var ftErrorCondition = FtError.FtNoError;

        if (_ftHandle != nint.Zero)
        {
            var deviceType = FtDevice.FtDeviceUnknown;
            // Check that it is an FT232H that we are trying to read
            GetDeviceType(ref deviceType);
            if (deviceType != FtDevice.FtDeviceXSeries)
            {
                // If it is not, throw an exception
                ftErrorCondition = FtError.FtIncorrectDevice;
                ErrorHandler(ftStatus, ftErrorCondition);
            }

            var eeData = new FtXSeriesData();
            var eeHeader = new FtEepromHeader();

            var manufacturer = new byte[32];
            var manufacturerId = new byte[16];
            var description = new byte[64];
            var serialNumber = new byte[16];

            eeHeader.deviceType = (uint)FtDevice.FtDeviceXSeries;
            eeData.common = eeHeader;

            // Calculate the size of our data structure...
            var size = Marshal.SizeOf(eeData);

            // Allocate space for our pointer...
            var eeDataMarshal = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(eeData, eeDataMarshal, false);

            // Call FT_EEPROM_Read
            ftStatus = FT_EEPROM_Read(_ftHandle, eeDataMarshal, (uint)size, manufacturer, manufacturerId,
                description, serialNumber);

            if (ftStatus == FtStatus.FtOk)
            {
                // Get the data back from the pointer...
                eeData = (FtXSeriesData)Marshal.PtrToStructure(eeDataMarshal, typeof(FtXSeriesData))!;

                // Retrieve string values
                var enc = new UTF8Encoding();
                eeX.Manufacturer = enc.GetString(manufacturer);
                eeX.ManufacturerId = enc.GetString(manufacturerId);
                eeX.Description = enc.GetString(description);
                eeX.SerialNumber = enc.GetString(serialNumber);
                // Map non-string elements to structure to be returned
                // Standard elements
                eeX.VendorId = eeData.common.VendorId;
                eeX.ProductId = eeData.common.ProductId;
                eeX.MaxPower = eeData.common.MaxPower;
                eeX.SelfPowered = Convert.ToBoolean(eeData.common.SelfPowered);
                eeX.RemoteWakeup = Convert.ToBoolean(eeData.common.RemoteWakeup);
                eeX.SerNumEnable = Convert.ToBoolean(eeData.common.SerNumEnable);
                eeX.PullDownEnable = Convert.ToBoolean(eeData.common.PullDownEnable);
                // X-Series specific fields
                // CBUS
                eeX.Cbus0 = eeData.Cbus0;
                eeX.Cbus1 = eeData.Cbus1;
                eeX.Cbus2 = eeData.Cbus2;
                eeX.Cbus3 = eeData.Cbus3;
                eeX.Cbus4 = eeData.Cbus4;
                eeX.Cbus5 = eeData.Cbus5;
                eeX.Cbus6 = eeData.Cbus6;
                // Drive Options
                eeX.AcDriveCurrent = eeData.ACDriveCurrent;
                eeX.AcSchmittInput = eeData.ACSchmittInput;
                eeX.AcSlowSlew = eeData.ACSlowSlew;
                eeX.AdDriveCurrent = eeData.ADDriveCurrent;
                eeX.AdSchmittInput = eeData.ADSchmittInput;
                eeX.AdSlowSlew = eeData.ADSlowSlew;
                // BCD
                eeX.BcdDisableSleep = eeData.BCDDisableSleep;
                eeX.BcdEnable = eeData.BCDEnable;
                eeX.BcdForceCbusPwrEn = eeData.BCDForceCbusPWREN;
                // FT1248
                eeX.Ft1248CPol = eeData.FT1248CPol;
                eeX.Ft1248FlowControl = eeData.FT1248FlowControl;
                eeX.Ft1248Lsb = eeData.FT1248Lsb;
                // I2C
                eeX.I2CDeviceId = eeData.I2CDeviceId;
                eeX.I2CDisableSchmitt = eeData.I2CDisableSchmitt;
                eeX.I2CSlaveAddress = eeData.I2CSlaveAddress;
                // RS232 Signals
                eeX.InvertCts = eeData.InvertCTS;
                eeX.InvertDcd = eeData.InvertDCD;
                eeX.InvertDsr = eeData.InvertDSR;
                eeX.InvertDtr = eeData.InvertDTR;
                eeX.InvertRi = eeData.InvertRI;
                eeX.InvertRts = eeData.InvertRTS;
                eeX.InvertRxd = eeData.InvertRXD;
                eeX.InvertTxd = eeData.InvertTXD;
                // Hardware Options
                eeX.PowerSaveEnable = eeData.PowerSaveEnable;
                eeX.Rs485EchoSuppress = eeData.RS485EchoSuppress;
                // Driver Option
                eeX.IsVcp = eeData.DriverType;
            }
        }

        return ftStatus;
    }

    //**************************************************************************
    // WriteFT232BEEPROM
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Writes the specified values to the EEPROM of an FT232B or FT245B device.
    /// </summary>
    /// <returns>FT_STATUS value from FT_EE_Program in FTD2XX DLL</returns>
    /// <param name="ee232B">The EEPROM settings to be written to the device</param>
    /// <remarks>If the strings are too long, they will be truncated to their maximum permitted lengths</remarks>
    /// <exception cref="FtException">Thrown when the current device does not match the type required by this method.</exception>
    public FtStatus WriteFt232Beeprom(Ft232BEepromStructure ee232B)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
        {
            var deviceType = FtDevice.FtDeviceUnknown;
            // Check that it is an FT232B or FT245B that we are trying to write
            GetDeviceType(ref deviceType);
            if (deviceType != FtDevice.FtDeviceBm)
            {
                // If it is not, throw an exception
                var ftErrorCondition = FtError.FtIncorrectDevice;
                ErrorHandler(ftStatus, ftErrorCondition);
            }

            // Check for VID and PID of 0x0000
            if ((ee232B.VendorId == 0x0000) | (ee232B.ProductId == 0x0000))
                // Do not allow users to program the device with VID or PID of 0x0000
                return FtStatus.FtInvalidParameter;

            var eeData = new FtProgramData();

            // Set up structure headers
            eeData.Signature1 = 0x00000000;
            eeData.Signature2 = 0xFFFFFFFF;
            eeData.Version = 2;

            // Allocate space from unmanaged heap
            eeData.Manufacturer = Marshal.AllocHGlobal(32);
            eeData.ManufacturerID = Marshal.AllocHGlobal(16);
            eeData.Description = Marshal.AllocHGlobal(64);
            eeData.SerialNumber = Marshal.AllocHGlobal(16);

            // Check lengths of strings to make sure that they are within our limits
            // If not, trim them to make them our maximum length
            if (ee232B.Manufacturer.Length > 32)
                ee232B.Manufacturer = ee232B.Manufacturer.Substring(0, 32);
            if (ee232B.ManufacturerId.Length > 16)
                ee232B.ManufacturerId = ee232B.ManufacturerId.Substring(0, 16);
            if (ee232B.Description.Length > 64)
                ee232B.Description = ee232B.Description.Substring(0, 64);
            if (ee232B.SerialNumber.Length > 16)
                ee232B.SerialNumber = ee232B.SerialNumber.Substring(0, 16);

            // Set string values
            eeData.Manufacturer = Marshal.StringToHGlobalAnsi(ee232B.Manufacturer);
            eeData.ManufacturerID = Marshal.StringToHGlobalAnsi(ee232B.ManufacturerId);
            eeData.Description = Marshal.StringToHGlobalAnsi(ee232B.Description);
            eeData.SerialNumber = Marshal.StringToHGlobalAnsi(ee232B.SerialNumber);

            // Map non-string elements to structure
            // Standard elements
            eeData.VendorID = ee232B.VendorId;
            eeData.ProductID = ee232B.ProductId;
            eeData.MaxPower = ee232B.MaxPower;
            eeData.SelfPowered = Convert.ToUInt16(ee232B.SelfPowered);
            eeData.RemoteWakeup = Convert.ToUInt16(ee232B.RemoteWakeup);
            // B specific fields
            eeData.Rev4 = Convert.ToByte(true);
            eeData.PullDownEnable = Convert.ToByte(ee232B.PullDownEnable);
            eeData.SerNumEnable = Convert.ToByte(ee232B.SerNumEnable);
            eeData.USBVersionEnable = Convert.ToByte(ee232B.UsbVersionEnable);
            eeData.USBVersion = ee232B.UsbVersion;

            // Call FT_EE_Program
            ftStatus = FT_EE_Program(_ftHandle, eeData);

            // Free unmanaged buffers
            Marshal.FreeHGlobal(eeData.Manufacturer);
            Marshal.FreeHGlobal(eeData.ManufacturerID);
            Marshal.FreeHGlobal(eeData.Description);
            Marshal.FreeHGlobal(eeData.SerialNumber);
        }

        return ftStatus;
    }

    //**************************************************************************
    // WriteFT2232EEPROM
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Writes the specified values to the EEPROM of an FT2232 device.
    ///     Calls FT_EE_Program in FTD2XX DLL
    /// </summary>
    /// <returns>FT_STATUS value from FT_EE_Program in FTD2XX DLL</returns>
    /// <param name="ee2232">The EEPROM settings to be written to the device</param>
    /// <remarks>If the strings are too long, they will be truncated to their maximum permitted lengths</remarks>
    /// <exception cref="FtException">Thrown when the current device does not match the type required by this method.</exception>
    public FtStatus WriteFt2232Eeprom(Ft2232EepromStructure ee2232)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
        {
            var deviceType = FtDevice.FtDeviceUnknown;
            // Check that it is an FT2232 that we are trying to write
            GetDeviceType(ref deviceType);
            if (deviceType != FtDevice.FtDevice2232)
            {
                // If it is not, throw an exception
                var ftErrorCondition = FtError.FtIncorrectDevice;
                ErrorHandler(ftStatus, ftErrorCondition);
            }

            // Check for VID and PID of 0x0000
            if ((ee2232.VendorId == 0x0000) | (ee2232.ProductId == 0x0000))
                // Do not allow users to program the device with VID or PID of 0x0000
                return FtStatus.FtInvalidParameter;

            var eeData = new FtProgramData();

            // Set up structure headers
            eeData.Signature1 = 0x00000000;
            eeData.Signature2 = 0xFFFFFFFF;
            eeData.Version = 2;

            // Allocate space from unmanaged heap
            eeData.Manufacturer = Marshal.AllocHGlobal(32);
            eeData.ManufacturerID = Marshal.AllocHGlobal(16);
            eeData.Description = Marshal.AllocHGlobal(64);
            eeData.SerialNumber = Marshal.AllocHGlobal(16);

            // Check lengths of strings to make sure that they are within our limits
            // If not, trim them to make them our maximum length
            if (ee2232.Manufacturer.Length > 32)
                ee2232.Manufacturer = ee2232.Manufacturer.Substring(0, 32);
            if (ee2232.ManufacturerId.Length > 16)
                ee2232.ManufacturerId = ee2232.ManufacturerId.Substring(0, 16);
            if (ee2232.Description.Length > 64)
                ee2232.Description = ee2232.Description.Substring(0, 64);
            if (ee2232.SerialNumber.Length > 16)
                ee2232.SerialNumber = ee2232.SerialNumber.Substring(0, 16);

            // Set string values
            eeData.Manufacturer = Marshal.StringToHGlobalAnsi(ee2232.Manufacturer);
            eeData.ManufacturerID = Marshal.StringToHGlobalAnsi(ee2232.ManufacturerId);
            eeData.Description = Marshal.StringToHGlobalAnsi(ee2232.Description);
            eeData.SerialNumber = Marshal.StringToHGlobalAnsi(ee2232.SerialNumber);

            // Map non-string elements to structure
            // Standard elements
            eeData.VendorID = ee2232.VendorId;
            eeData.ProductID = ee2232.ProductId;
            eeData.MaxPower = ee2232.MaxPower;
            eeData.SelfPowered = Convert.ToUInt16(ee2232.SelfPowered);
            eeData.RemoteWakeup = Convert.ToUInt16(ee2232.RemoteWakeup);
            // 2232 specific fields
            eeData.Rev5 = Convert.ToByte(true);
            eeData.PullDownEnable5 = Convert.ToByte(ee2232.PullDownEnable);
            eeData.SerNumEnable5 = Convert.ToByte(ee2232.SerNumEnable);
            eeData.USBVersionEnable5 = Convert.ToByte(ee2232.UsbVersionEnable);
            eeData.USBVersion5 = ee2232.UsbVersion;
            eeData.AIsHighCurrent = Convert.ToByte(ee2232.AIsHighCurrent);
            eeData.BIsHighCurrent = Convert.ToByte(ee2232.BIsHighCurrent);
            eeData.IFAIsFifo = Convert.ToByte(ee2232.IfaIsFifo);
            eeData.IFAIsFifoTar = Convert.ToByte(ee2232.IfaIsFifoTar);
            eeData.IFAIsFastSer = Convert.ToByte(ee2232.IfaIsFastSer);
            eeData.AIsVCP = Convert.ToByte(ee2232.AIsVcp);
            eeData.IFBIsFifo = Convert.ToByte(ee2232.IfbIsFifo);
            eeData.IFBIsFifoTar = Convert.ToByte(ee2232.IfbIsFifoTar);
            eeData.IFBIsFastSer = Convert.ToByte(ee2232.IfbIsFastSer);
            eeData.BIsVCP = Convert.ToByte(ee2232.BIsVcp);

            // Call FT_EE_Program
            ftStatus = FT_EE_Program(_ftHandle, eeData);

            // Free unmanaged buffers
            Marshal.FreeHGlobal(eeData.Manufacturer);
            Marshal.FreeHGlobal(eeData.ManufacturerID);
            Marshal.FreeHGlobal(eeData.Description);
            Marshal.FreeHGlobal(eeData.SerialNumber);
        }

        return ftStatus;
    }

    //**************************************************************************
    // WriteFT232REEPROM
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Writes the specified values to the EEPROM of an FT232R or FT245R device.
    ///     Calls FT_EE_Program in FTD2XX DLL
    /// </summary>
    /// <returns>FT_STATUS value from FT_EE_Program in FTD2XX DLL</returns>
    /// <param name="ee232R">The EEPROM settings to be written to the device</param>
    /// <remarks>If the strings are too long, they will be truncated to their maximum permitted lengths</remarks>
    /// <exception cref="FtException">Thrown when the current device does not match the type required by this method.</exception>
    public FtStatus WriteFt232Reeprom(Ft232REepromStructure ee232R)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
        {
            var deviceType = FtDevice.FtDeviceUnknown;
            // Check that it is an FT232R or FT245R that we are trying to write
            GetDeviceType(ref deviceType);
            if (deviceType != FtDevice.FtDevice232R)
            {
                // If it is not, throw an exception
                var ftErrorCondition = FtError.FtIncorrectDevice;
                ErrorHandler(ftStatus, ftErrorCondition);
            }

            // Check for VID and PID of 0x0000
            if ((ee232R.VendorId == 0x0000) | (ee232R.ProductId == 0x0000))
                // Do not allow users to program the device with VID or PID of 0x0000
                return FtStatus.FtInvalidParameter;

            var eeData = new FtProgramData();

            // Set up structure headers
            eeData.Signature1 = 0x00000000;
            eeData.Signature2 = 0xFFFFFFFF;
            eeData.Version = 2;

            // Allocate space from unmanaged heap
            eeData.Manufacturer = Marshal.AllocHGlobal(32);
            eeData.ManufacturerID = Marshal.AllocHGlobal(16);
            eeData.Description = Marshal.AllocHGlobal(64);
            eeData.SerialNumber = Marshal.AllocHGlobal(16);

            // Check lengths of strings to make sure that they are within our limits
            // If not, trim them to make them our maximum length
            if (ee232R.Manufacturer.Length > 32)
                ee232R.Manufacturer = ee232R.Manufacturer.Substring(0, 32);
            if (ee232R.ManufacturerId.Length > 16)
                ee232R.ManufacturerId = ee232R.ManufacturerId.Substring(0, 16);
            if (ee232R.Description.Length > 64)
                ee232R.Description = ee232R.Description.Substring(0, 64);
            if (ee232R.SerialNumber.Length > 16)
                ee232R.SerialNumber = ee232R.SerialNumber.Substring(0, 16);

            // Set string values
            eeData.Manufacturer = Marshal.StringToHGlobalAnsi(ee232R.Manufacturer);
            eeData.ManufacturerID = Marshal.StringToHGlobalAnsi(ee232R.ManufacturerId);
            eeData.Description = Marshal.StringToHGlobalAnsi(ee232R.Description);
            eeData.SerialNumber = Marshal.StringToHGlobalAnsi(ee232R.SerialNumber);

            // Map non-string elements to structure
            // Standard elements
            eeData.VendorID = ee232R.VendorId;
            eeData.ProductID = ee232R.ProductId;
            eeData.MaxPower = ee232R.MaxPower;
            eeData.SelfPowered = Convert.ToUInt16(ee232R.SelfPowered);
            eeData.RemoteWakeup = Convert.ToUInt16(ee232R.RemoteWakeup);
            // 232R specific fields
            eeData.PullDownEnableR = Convert.ToByte(ee232R.PullDownEnable);
            eeData.SerNumEnableR = Convert.ToByte(ee232R.SerNumEnable);
            eeData.UseExtOsc = Convert.ToByte(ee232R.UseExtOsc);
            eeData.HighDriveIOs = Convert.ToByte(ee232R.HighDriveIOs);
            // Override any endpoint size the user has selected and force 64 bytes
            // Some users have been known to wreck devices by setting 0 here...
            eeData.EndpointSize = 64;
            eeData.PullDownEnableR = Convert.ToByte(ee232R.PullDownEnable);
            eeData.SerNumEnableR = Convert.ToByte(ee232R.SerNumEnable);
            eeData.InvertTXD = Convert.ToByte(ee232R.InvertTxd);
            eeData.InvertRXD = Convert.ToByte(ee232R.InvertRxd);
            eeData.InvertRTS = Convert.ToByte(ee232R.InvertRts);
            eeData.InvertCTS = Convert.ToByte(ee232R.InvertCts);
            eeData.InvertDTR = Convert.ToByte(ee232R.InvertDtr);
            eeData.InvertDSR = Convert.ToByte(ee232R.InvertDsr);
            eeData.InvertDCD = Convert.ToByte(ee232R.InvertDcd);
            eeData.InvertRI = Convert.ToByte(ee232R.InvertRi);
            eeData.Cbus0 = ee232R.Cbus0;
            eeData.Cbus1 = ee232R.Cbus1;
            eeData.Cbus2 = ee232R.Cbus2;
            eeData.Cbus3 = ee232R.Cbus3;
            eeData.Cbus4 = ee232R.Cbus4;
            eeData.RIsD2XX = Convert.ToByte(ee232R.RIsD2Xx);

            // Call FT_EE_Program
            ftStatus = FT_EE_Program(_ftHandle, eeData);

            // Free unmanaged buffers
            Marshal.FreeHGlobal(eeData.Manufacturer);
            Marshal.FreeHGlobal(eeData.ManufacturerID);
            Marshal.FreeHGlobal(eeData.Description);
            Marshal.FreeHGlobal(eeData.SerialNumber);
        }

        return ftStatus;
    }

    //**************************************************************************
    // WriteFT2232HEEPROM
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Writes the specified values to the EEPROM of an FT2232H device.
    ///     Calls FT_EE_Program in FTD2XX DLL
    /// </summary>
    /// <returns>FT_STATUS value from FT_EE_Program in FTD2XX DLL</returns>
    /// <param name="ee2232H">The EEPROM settings to be written to the device</param>
    /// <remarks>If the strings are too long, they will be truncated to their maximum permitted lengths</remarks>
    /// <exception cref="FtException">Thrown when the current device does not match the type required by this method.</exception>
    public FtStatus WriteFt2232Heeprom(Ft2232HEepromStructure ee2232H)
    {
        var ftStatus = FtStatus.FtOtherError;
        var ftErrorCondition = FtError.FtNoError;

        if (_ftHandle != nint.Zero)
        {
            var deviceType = FtDevice.FtDeviceUnknown;
            // Check that it is an FT2232H that we are trying to write
            GetDeviceType(ref deviceType);
            if (deviceType != FtDevice.FtDevice2232H)
            {
                // If it is not, throw an exception
                ftErrorCondition = FtError.FtIncorrectDevice;
                ErrorHandler(ftStatus, ftErrorCondition);
            }

            // Check for VID and PID of 0x0000
            if ((ee2232H.VendorId == 0x0000) | (ee2232H.ProductId == 0x0000))
                // Do not allow users to program the device with VID or PID of 0x0000
                return FtStatus.FtInvalidParameter;

            var eeData = new FtProgramData();

            // Set up structure headers
            eeData.Signature1 = 0x00000000;
            eeData.Signature2 = 0xFFFFFFFF;
            eeData.Version = 3;

            // Allocate space from unmanaged heap
            eeData.Manufacturer = Marshal.AllocHGlobal(32);
            eeData.ManufacturerID = Marshal.AllocHGlobal(16);
            eeData.Description = Marshal.AllocHGlobal(64);
            eeData.SerialNumber = Marshal.AllocHGlobal(16);

            // Check lengths of strings to make sure that they are within our limits
            // If not, trim them to make them our maximum length
            if (ee2232H.Manufacturer.Length > 32)
                ee2232H.Manufacturer = ee2232H.Manufacturer.Substring(0, 32);
            if (ee2232H.ManufacturerId.Length > 16)
                ee2232H.ManufacturerId = ee2232H.ManufacturerId.Substring(0, 16);
            if (ee2232H.Description.Length > 64)
                ee2232H.Description = ee2232H.Description.Substring(0, 64);
            if (ee2232H.SerialNumber.Length > 16)
                ee2232H.SerialNumber = ee2232H.SerialNumber.Substring(0, 16);

            // Set string values
            eeData.Manufacturer = Marshal.StringToHGlobalAnsi(ee2232H.Manufacturer);
            eeData.ManufacturerID = Marshal.StringToHGlobalAnsi(ee2232H.ManufacturerId);
            eeData.Description = Marshal.StringToHGlobalAnsi(ee2232H.Description);
            eeData.SerialNumber = Marshal.StringToHGlobalAnsi(ee2232H.SerialNumber);

            // Map non-string elements to structure
            // Standard elements
            eeData.VendorID = ee2232H.VendorId;
            eeData.ProductID = ee2232H.ProductId;
            eeData.MaxPower = ee2232H.MaxPower;
            eeData.SelfPowered = Convert.ToUInt16(ee2232H.SelfPowered);
            eeData.RemoteWakeup = Convert.ToUInt16(ee2232H.RemoteWakeup);
            // 2232H specific fields
            eeData.PullDownEnable7 = Convert.ToByte(ee2232H.PullDownEnable);
            eeData.SerNumEnable7 = Convert.ToByte(ee2232H.SerNumEnable);
            eeData.ALSlowSlew = Convert.ToByte(ee2232H.AlSlowSlew);
            eeData.ALSchmittInput = Convert.ToByte(ee2232H.AlSchmittInput);
            eeData.ALDriveCurrent = ee2232H.AlDriveCurrent;
            eeData.AHSlowSlew = Convert.ToByte(ee2232H.AhSlowSlew);
            eeData.AHSchmittInput = Convert.ToByte(ee2232H.AhSchmittInput);
            eeData.AHDriveCurrent = ee2232H.AhDriveCurrent;
            eeData.BLSlowSlew = Convert.ToByte(ee2232H.BlSlowSlew);
            eeData.BLSchmittInput = Convert.ToByte(ee2232H.BlSchmittInput);
            eeData.BLDriveCurrent = ee2232H.BlDriveCurrent;
            eeData.BHSlowSlew = Convert.ToByte(ee2232H.BhSlowSlew);
            eeData.BHSchmittInput = Convert.ToByte(ee2232H.BhSchmittInput);
            eeData.BHDriveCurrent = ee2232H.BhDriveCurrent;
            eeData.IFAIsFifo7 = Convert.ToByte(ee2232H.IfaIsFifo);
            eeData.IFAIsFifoTar7 = Convert.ToByte(ee2232H.IfaIsFifoTar);
            eeData.IFAIsFastSer7 = Convert.ToByte(ee2232H.IfaIsFastSer);
            eeData.AIsVCP7 = Convert.ToByte(ee2232H.AIsVcp);
            eeData.IFBIsFifo7 = Convert.ToByte(ee2232H.IfbIsFifo);
            eeData.IFBIsFifoTar7 = Convert.ToByte(ee2232H.IfbIsFifoTar);
            eeData.IFBIsFastSer7 = Convert.ToByte(ee2232H.IfbIsFastSer);
            eeData.BIsVCP7 = Convert.ToByte(ee2232H.BIsVcp);
            eeData.PowerSaveEnable = Convert.ToByte(ee2232H.PowerSaveEnable);

            // Call FT_EE_Program
            ftStatus = FT_EE_Program(_ftHandle, eeData);

            // Free unmanaged buffers
            Marshal.FreeHGlobal(eeData.Manufacturer);
            Marshal.FreeHGlobal(eeData.ManufacturerID);
            Marshal.FreeHGlobal(eeData.Description);
            Marshal.FreeHGlobal(eeData.SerialNumber);
        }

        return ftStatus;
    }

    //**************************************************************************
    // WriteFT4232HEEPROM
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Writes the specified values to the EEPROM of an FT4232H device.
    ///     Calls FT_EE_Program in FTD2XX DLL
    /// </summary>
    /// <returns>FT_STATUS value from FT_EE_Program in FTD2XX DLL</returns>
    /// <param name="ee4232H">The EEPROM settings to be written to the device</param>
    /// <remarks>If the strings are too long, they will be truncated to their maximum permitted lengths</remarks>
    /// <exception cref="FtException">Thrown when the current device does not match the type required by this method.</exception>
    public FtStatus WriteFt4232Heeprom(Ft4232HEepromStructure ee4232H)
    {
        var ftStatus = FtStatus.FtOtherError;
        var ftErrorCondition = FtError.FtNoError;

        if (_ftHandle != nint.Zero)
        {
            var deviceType = FtDevice.FtDeviceUnknown;
            // Check that it is an FT4232H that we are trying to write
            GetDeviceType(ref deviceType);
            if (deviceType != FtDevice.FtDevice4232H)
            {
                // If it is not, throw an exception
                ftErrorCondition = FtError.FtIncorrectDevice;
                ErrorHandler(ftStatus, ftErrorCondition);
            }

            // Check for VID and PID of 0x0000
            if ((ee4232H.VendorId == 0x0000) | (ee4232H.ProductId == 0x0000))
                // Do not allow users to program the device with VID or PID of 0x0000
                return FtStatus.FtInvalidParameter;

            var eeData = new FtProgramData();

            // Set up structure headers
            eeData.Signature1 = 0x00000000;
            eeData.Signature2 = 0xFFFFFFFF;
            eeData.Version = 4;

            // Allocate space from unmanaged heap
            eeData.Manufacturer = Marshal.AllocHGlobal(32);
            eeData.ManufacturerID = Marshal.AllocHGlobal(16);
            eeData.Description = Marshal.AllocHGlobal(64);
            eeData.SerialNumber = Marshal.AllocHGlobal(16);

            // Check lengths of strings to make sure that they are within our limits
            // If not, trim them to make them our maximum length
            if (ee4232H.Manufacturer.Length > 32)
                ee4232H.Manufacturer = ee4232H.Manufacturer.Substring(0, 32);
            if (ee4232H.ManufacturerId.Length > 16)
                ee4232H.ManufacturerId = ee4232H.ManufacturerId.Substring(0, 16);
            if (ee4232H.Description.Length > 64)
                ee4232H.Description = ee4232H.Description.Substring(0, 64);
            if (ee4232H.SerialNumber.Length > 16)
                ee4232H.SerialNumber = ee4232H.SerialNumber.Substring(0, 16);

            // Set string values
            eeData.Manufacturer = Marshal.StringToHGlobalAnsi(ee4232H.Manufacturer);
            eeData.ManufacturerID = Marshal.StringToHGlobalAnsi(ee4232H.ManufacturerId);
            eeData.Description = Marshal.StringToHGlobalAnsi(ee4232H.Description);
            eeData.SerialNumber = Marshal.StringToHGlobalAnsi(ee4232H.SerialNumber);

            // Map non-string elements to structure
            // Standard elements
            eeData.VendorID = ee4232H.VendorId;
            eeData.ProductID = ee4232H.ProductId;
            eeData.MaxPower = ee4232H.MaxPower;
            eeData.SelfPowered = Convert.ToUInt16(ee4232H.SelfPowered);
            eeData.RemoteWakeup = Convert.ToUInt16(ee4232H.RemoteWakeup);
            // 4232H specific fields
            eeData.PullDownEnable8 = Convert.ToByte(ee4232H.PullDownEnable);
            eeData.SerNumEnable8 = Convert.ToByte(ee4232H.SerNumEnable);
            eeData.ASlowSlew = Convert.ToByte(ee4232H.ASlowSlew);
            eeData.ASchmittInput = Convert.ToByte(ee4232H.ASchmittInput);
            eeData.ADriveCurrent = ee4232H.ADriveCurrent;
            eeData.BSlowSlew = Convert.ToByte(ee4232H.BSlowSlew);
            eeData.BSchmittInput = Convert.ToByte(ee4232H.BSchmittInput);
            eeData.BDriveCurrent = ee4232H.BDriveCurrent;
            eeData.CSlowSlew = Convert.ToByte(ee4232H.CSlowSlew);
            eeData.CSchmittInput = Convert.ToByte(ee4232H.CSchmittInput);
            eeData.CDriveCurrent = ee4232H.CDriveCurrent;
            eeData.DSlowSlew = Convert.ToByte(ee4232H.DSlowSlew);
            eeData.DSchmittInput = Convert.ToByte(ee4232H.DSchmittInput);
            eeData.DDriveCurrent = ee4232H.DDriveCurrent;
            eeData.ARIIsTxdEn = Convert.ToByte(ee4232H.AriIsTxdEn);
            eeData.BRIIsTxdEn = Convert.ToByte(ee4232H.BriIsTxdEn);
            eeData.CRIIsTxdEn = Convert.ToByte(ee4232H.CriIsTxdEn);
            eeData.DRIIsTxdEn = Convert.ToByte(ee4232H.DriIsTxdEn);
            eeData.AIsVCP8 = Convert.ToByte(ee4232H.AIsVcp);
            eeData.BIsVCP8 = Convert.ToByte(ee4232H.BIsVcp);
            eeData.CIsVCP8 = Convert.ToByte(ee4232H.CIsVcp);
            eeData.DIsVCP8 = Convert.ToByte(ee4232H.DIsVcp);

            // Call FT_EE_Program
            ftStatus = FT_EE_Program(_ftHandle, eeData);

            // Free unmanaged buffers
            Marshal.FreeHGlobal(eeData.Manufacturer);
            Marshal.FreeHGlobal(eeData.ManufacturerID);
            Marshal.FreeHGlobal(eeData.Description);
            Marshal.FreeHGlobal(eeData.SerialNumber);
        }

        return ftStatus;
    }

    //**************************************************************************
    // WriteFT232HEEPROM
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Writes the specified values to the EEPROM of an FT232H device.
    ///     Calls FT_EE_Program in FTD2XX DLL
    /// </summary>
    /// <returns>FT_STATUS value from FT_EE_Program in FTD2XX DLL</returns>
    /// <param name="ee232H">The EEPROM settings to be written to the device</param>
    /// <remarks>If the strings are too long, they will be truncated to their maximum permitted lengths</remarks>
    /// <exception cref="FtException">Thrown when the current device does not match the type required by this method.</exception>
    public FtStatus WriteFt232Heeprom(Ft232HEepromStructure ee232H)
    {
        var ftStatus = FtStatus.FtOtherError;
        var ftErrorCondition = FtError.FtNoError;

        if (_ftHandle != nint.Zero)
        {
            var deviceType = FtDevice.FtDeviceUnknown;
            // Check that it is an FT232H that we are trying to write
            GetDeviceType(ref deviceType);
            if (deviceType != FtDevice.FtDevice232H)
            {
                // If it is not, throw an exception
                ftErrorCondition = FtError.FtIncorrectDevice;
                ErrorHandler(ftStatus, ftErrorCondition);
            }

            // Check for VID and PID of 0x0000
            if ((ee232H.VendorId == 0x0000) | (ee232H.ProductId == 0x0000))
                // Do not allow users to program the device with VID or PID of 0x0000
                return FtStatus.FtInvalidParameter;

            var eeData = new FtProgramData();

            // Set up structure headers
            eeData.Signature1 = 0x00000000;
            eeData.Signature2 = 0xFFFFFFFF;
            eeData.Version = 5;

            // Allocate space from unmanaged heap
            eeData.Manufacturer = Marshal.AllocHGlobal(32);
            eeData.ManufacturerID = Marshal.AllocHGlobal(16);
            eeData.Description = Marshal.AllocHGlobal(64);
            eeData.SerialNumber = Marshal.AllocHGlobal(16);

            // Check lengths of strings to make sure that they are within our limits
            // If not, trim them to make them our maximum length
            if (ee232H.Manufacturer.Length > 32)
                ee232H.Manufacturer = ee232H.Manufacturer.Substring(0, 32);
            if (ee232H.ManufacturerId.Length > 16)
                ee232H.ManufacturerId = ee232H.ManufacturerId.Substring(0, 16);
            if (ee232H.Description.Length > 64)
                ee232H.Description = ee232H.Description.Substring(0, 64);
            if (ee232H.SerialNumber.Length > 16)
                ee232H.SerialNumber = ee232H.SerialNumber.Substring(0, 16);

            // Set string values
            eeData.Manufacturer = Marshal.StringToHGlobalAnsi(ee232H.Manufacturer);
            eeData.ManufacturerID = Marshal.StringToHGlobalAnsi(ee232H.ManufacturerId);
            eeData.Description = Marshal.StringToHGlobalAnsi(ee232H.Description);
            eeData.SerialNumber = Marshal.StringToHGlobalAnsi(ee232H.SerialNumber);

            // Map non-string elements to structure
            // Standard elements
            eeData.VendorID = ee232H.VendorId;
            eeData.ProductID = ee232H.ProductId;
            eeData.MaxPower = ee232H.MaxPower;
            eeData.SelfPowered = Convert.ToUInt16(ee232H.SelfPowered);
            eeData.RemoteWakeup = Convert.ToUInt16(ee232H.RemoteWakeup);
            // 232H specific fields
            eeData.PullDownEnableH = Convert.ToByte(ee232H.PullDownEnable);
            eeData.SerNumEnableH = Convert.ToByte(ee232H.SerNumEnable);
            eeData.ACSlowSlewH = Convert.ToByte(ee232H.AcSlowSlew);
            eeData.ACSchmittInputH = Convert.ToByte(ee232H.AcSchmittInput);
            eeData.ACDriveCurrentH = Convert.ToByte(ee232H.AcDriveCurrent);
            eeData.ADSlowSlewH = Convert.ToByte(ee232H.AdSlowSlew);
            eeData.ADSchmittInputH = Convert.ToByte(ee232H.AdSchmittInput);
            eeData.ADDriveCurrentH = Convert.ToByte(ee232H.AdDriveCurrent);
            eeData.Cbus0H = Convert.ToByte(ee232H.Cbus0);
            eeData.Cbus1H = Convert.ToByte(ee232H.Cbus1);
            eeData.Cbus2H = Convert.ToByte(ee232H.Cbus2);
            eeData.Cbus3H = Convert.ToByte(ee232H.Cbus3);
            eeData.Cbus4H = Convert.ToByte(ee232H.Cbus4);
            eeData.Cbus5H = Convert.ToByte(ee232H.Cbus5);
            eeData.Cbus6H = Convert.ToByte(ee232H.Cbus6);
            eeData.Cbus7H = Convert.ToByte(ee232H.Cbus7);
            eeData.Cbus8H = Convert.ToByte(ee232H.Cbus8);
            eeData.Cbus9H = Convert.ToByte(ee232H.Cbus9);
            eeData.IsFifoH = Convert.ToByte(ee232H.IsFifo);
            eeData.IsFifoTarH = Convert.ToByte(ee232H.IsFifoTar);
            eeData.IsFastSerH = Convert.ToByte(ee232H.IsFastSer);
            eeData.IsFT1248H = Convert.ToByte(ee232H.IsFt1248);
            eeData.FT1248CPolH = Convert.ToByte(ee232H.Ft1248CPol);
            eeData.FT1248LsbH = Convert.ToByte(ee232H.Ft1248Lsb);
            eeData.FT1248FlowControlH = Convert.ToByte(ee232H.Ft1248FlowControl);
            eeData.IsVCPH = Convert.ToByte(ee232H.IsVcp);
            eeData.PowerSaveEnableH = Convert.ToByte(ee232H.PowerSaveEnable);

            // Call FT_EE_Program
            ftStatus = FT_EE_Program(_ftHandle, eeData);

            // Free unmanaged buffers
            Marshal.FreeHGlobal(eeData.Manufacturer);
            Marshal.FreeHGlobal(eeData.ManufacturerID);
            Marshal.FreeHGlobal(eeData.Description);
            Marshal.FreeHGlobal(eeData.SerialNumber);
        }

        return ftStatus;
    }

    //**************************************************************************
    // WriteXSeriesEEPROM
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Writes the specified values to the EEPROM of an X-Series device.
    ///     Calls FT_EEPROM_Program in FTD2XX DLL
    /// </summary>
    /// <returns>FT_STATUS value from FT_EEPROM_Program in FTD2XX DLL</returns>
    /// <param name="eeX">The EEPROM settings to be written to the device</param>
    /// <remarks>If the strings are too long, they will be truncated to their maximum permitted lengths</remarks>
    /// <exception cref="FtException">Thrown when the current device does not match the type required by this method.</exception>
    public FtStatus WriteXSeriesEeprom(FtXSeriesEepromStructure eeX)
    {
        var ftStatus = FtStatus.FtOtherError;
        var ftErrorCondition = FtError.FtNoError;

        byte[] manufacturer, manufacturerId, description, serialNumber;

        if (_ftHandle != nint.Zero)
        {
            var deviceType = FtDevice.FtDeviceUnknown;
            // Check that it is an FT232H that we are trying to write
            GetDeviceType(ref deviceType);
            if (deviceType != FtDevice.FtDeviceXSeries)
            {
                // If it is not, throw an exception
                ftErrorCondition = FtError.FtIncorrectDevice;
                ErrorHandler(ftStatus, ftErrorCondition);
            }

            // Check for VID and PID of 0x0000
            if ((eeX.VendorId == 0x0000) | (eeX.ProductId == 0x0000))
                // Do not allow users to program the device with VID or PID of 0x0000
                return FtStatus.FtInvalidParameter;

            var eeData = new FtXSeriesData();

            // String manipulation...
            // Allocate space from unmanaged heap
            manufacturer = new byte[32];
            manufacturerId = new byte[16];
            description = new byte[64];
            serialNumber = new byte[16];

            // Check lengths of strings to make sure that they are within our limits
            // If not, trim them to make them our maximum length
            if (eeX.Manufacturer.Length > 32)
                eeX.Manufacturer = eeX.Manufacturer.Substring(0, 32);
            if (eeX.ManufacturerId.Length > 16)
                eeX.ManufacturerId = eeX.ManufacturerId.Substring(0, 16);
            if (eeX.Description.Length > 64)
                eeX.Description = eeX.Description.Substring(0, 64);
            if (eeX.SerialNumber.Length > 16)
                eeX.SerialNumber = eeX.SerialNumber.Substring(0, 16);

            // Set string values
            var encoding = new UTF8Encoding();
            manufacturer = encoding.GetBytes(eeX.Manufacturer);
            manufacturerId = encoding.GetBytes(eeX.ManufacturerId);
            description = encoding.GetBytes(eeX.Description);
            serialNumber = encoding.GetBytes(eeX.SerialNumber);

            // Map non-string elements to structure to be returned
            // Standard elements
            eeData.common.deviceType = (uint)FtDevice.FtDeviceXSeries;
            eeData.common.VendorId = eeX.VendorId;
            eeData.common.ProductId = eeX.ProductId;
            eeData.common.MaxPower = eeX.MaxPower;
            eeData.common.SelfPowered = Convert.ToByte(eeX.SelfPowered);
            eeData.common.RemoteWakeup = Convert.ToByte(eeX.RemoteWakeup);
            eeData.common.SerNumEnable = Convert.ToByte(eeX.SerNumEnable);
            eeData.common.PullDownEnable = Convert.ToByte(eeX.PullDownEnable);
            // X-Series specific fields
            // CBUS
            eeData.Cbus0 = eeX.Cbus0;
            eeData.Cbus1 = eeX.Cbus1;
            eeData.Cbus2 = eeX.Cbus2;
            eeData.Cbus3 = eeX.Cbus3;
            eeData.Cbus4 = eeX.Cbus4;
            eeData.Cbus5 = eeX.Cbus5;
            eeData.Cbus6 = eeX.Cbus6;
            // Drive Options
            eeData.ACDriveCurrent = eeX.AcDriveCurrent;
            eeData.ACSchmittInput = eeX.AcSchmittInput;
            eeData.ACSlowSlew = eeX.AcSlowSlew;
            eeData.ADDriveCurrent = eeX.AdDriveCurrent;
            eeData.ADSchmittInput = eeX.AdSchmittInput;
            eeData.ADSlowSlew = eeX.AdSlowSlew;
            // BCD
            eeData.BCDDisableSleep = eeX.BcdDisableSleep;
            eeData.BCDEnable = eeX.BcdEnable;
            eeData.BCDForceCbusPWREN = eeX.BcdForceCbusPwrEn;
            // FT1248
            eeData.FT1248CPol = eeX.Ft1248CPol;
            eeData.FT1248FlowControl = eeX.Ft1248FlowControl;
            eeData.FT1248Lsb = eeX.Ft1248Lsb;
            // I2C
            eeData.I2CDeviceId = eeX.I2CDeviceId;
            eeData.I2CDisableSchmitt = eeX.I2CDisableSchmitt;
            eeData.I2CSlaveAddress = eeX.I2CSlaveAddress;
            // RS232 Signals
            eeData.InvertCTS = eeX.InvertCts;
            eeData.InvertDCD = eeX.InvertDcd;
            eeData.InvertDSR = eeX.InvertDsr;
            eeData.InvertDTR = eeX.InvertDtr;
            eeData.InvertRI = eeX.InvertRi;
            eeData.InvertRTS = eeX.InvertRts;
            eeData.InvertRXD = eeX.InvertRxd;
            eeData.InvertTXD = eeX.InvertTxd;
            // Hardware Options
            eeData.PowerSaveEnable = eeX.PowerSaveEnable;
            eeData.RS485EchoSuppress = eeX.Rs485EchoSuppress;
            // Driver Option
            eeData.DriverType = eeX.IsVcp;

            // Check the size of the structure...
            var size = Marshal.SizeOf(eeData);
            // Allocate space for our pointer...
            var eeDataMarshal = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(eeData, eeDataMarshal, false);

            ftStatus = FT_EEPROM_Program(_ftHandle, eeDataMarshal, (uint)size, manufacturer, manufacturerId,
                description, serialNumber);
        }

        return ftStatus;
    }

    //**************************************************************************
    // EEReadUserArea
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Reads data from the user area of the device EEPROM.
    /// </summary>
    /// <returns>FT_STATUS from FT_UARead in FTD2XX.DLL</returns>
    /// <param name="userAreaDataBuffer">
    ///     An array of bytes which will be populated with the data read from the device EEPROM
    ///     user area.
    /// </param>
    /// <param name="numBytesRead">The number of bytes actually read from the EEPROM user area.</param>
    public FtStatus EeReadUserArea(byte[] userAreaDataBuffer, ref uint numBytesRead)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
        {
            uint uaSize = 0;
            // Get size of user area to allocate an array of the correct size.
            // The application must also get the UA size for its copy
            ftStatus = FT_EE_UASize(_ftHandle, ref uaSize);

            // Make sure we have enough storage for the whole user area
            if (userAreaDataBuffer.Length >= uaSize)
                // Call FT_EE_UARead
                ftStatus = FT_EE_UARead(_ftHandle, userAreaDataBuffer, userAreaDataBuffer.Length,
                    ref numBytesRead);
        }


        return ftStatus;
    }

    //**************************************************************************
    // EEWriteUserArea
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Writes data to the user area of the device EEPROM.
    /// </summary>
    /// <returns>FT_STATUS value from FT_UAWrite in FTD2XX.DLL</returns>
    /// <param name="userAreaDataBuffer">An array of bytes which will be written to the device EEPROM user area.</param>
    public FtStatus EeWriteUserArea(byte[] userAreaDataBuffer)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
        {
            uint uaSize = 0;
            // Get size of user area to allocate an array of the correct size.
            // The application must also get the UA size for its copy
            ftStatus = FT_EE_UASize(_ftHandle, ref uaSize);

            // Make sure we have enough storage for all the data in the EEPROM
            if (userAreaDataBuffer.Length <= uaSize)
                // Call FT_EE_UAWrite
                ftStatus = FT_EE_UAWrite(_ftHandle, userAreaDataBuffer, userAreaDataBuffer.Length);
        }

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
    public FtStatus GetDeviceType(ref FtDevice deviceType)
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
    /// <param name="modemStatus">A bit map representaion of the current modem status.</param>
    public FtStatus GetModemStatus(ref byte modemStatus)
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
    /// <param name="lineStatus">A bit map representaion of the current line status.</param>
    public FtStatus GetLineStatus(ref byte lineStatus)
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
    /// <param name="xon">The Xon character for Xon/Xoff flow control.  Ignored if not using Xon/XOff flow control.</param>
    /// <param name="xoff">The Xoff character for Xon/Xoff flow control.  Ignored if not using Xon/XOff flow control.</param>
    public FtStatus SetFlowControl(ushort flowControl, byte xon, byte xoff)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero)
            // Call FT_SetFlowControl
            ftStatus = FT_SetFlowControl(_ftHandle, flowControl, xon, xoff);

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
            if (enable)
                // Call FT_SetRts
                ftStatus = FT_SetRts(_ftHandle);
            else
                // Call FT_ClrRts
                ftStatus = FT_ClrRts(_ftHandle);
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
            if (enable)
                // Call FT_SetDtr
                ftStatus = FT_SetDtr(_ftHandle);
            else
                // Call FT_ClrDtr
                ftStatus = FT_ClrDtr(_ftHandle);
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
            if (enable)
                // Call FT_SetBreakOn
                ftStatus = FT_SetBreakOn(_ftHandle);
            else
                // Call FT_SetBreakOff
                ftStatus = FT_SetBreakOff(_ftHandle);
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
    /// <returns>FT_STATUS vlaue from FT_SetResetPipeRetryCount in FTD2XX.DLL</returns>
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
        var ftStatus = FtStatus.FtOtherError;

        // Call FT_GetLibraryVersion
        ftStatus = FT_GetLibraryVersion(ref libraryVersion);

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
            var deviceType = FtDevice.FtDeviceUnknown;
            // Set Bit Mode does not apply to FT8U232AM, FT8U245AM or FT8U100AX devices
            GetDeviceType(ref deviceType);
            if (deviceType == FtDevice.FtDeviceBm || deviceType == FtDevice.FtDevice2232)
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
    /// <param name="eventChar">A character that will be tigger an IN to the host when this character is received.</param>
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
    // GetEEUserAreaSize
    //**************************************************************************
    // Intellisense comments
    /// <summary>
    ///     Gets the size of the EEPROM user area.
    /// </summary>
    /// <returns>FT_STATUS value from FT_EE_UASize in FTD2XX.DLL</returns>
    /// <param name="uaSize">The EEPROM user area size in bytes.</param>
    public FtStatus EeUserAreaSize(ref uint uaSize)
    {
        var ftStatus = FtStatus.FtOtherError;

        if (_ftHandle != nint.Zero) ftStatus = FT_EE_UASize(_ftHandle, ref uaSize);

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

        // As ComPortName is an OUT paremeter, has to be assigned before returning
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
    public bool IsOpen
    {
        get
        {
            if (_ftHandle == nint.Zero)
                return false;
            return true;
        }
    }

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
            string identifier;
            identifier = Empty;
            if (IsOpen)
            {
                var deviceType = FtDevice.FtDeviceBm;
                GetDeviceType(ref deviceType);
                if (deviceType == FtDevice.FtDevice2232H ||
                    deviceType == FtDevice.FtDevice4232H ||
                    deviceType == FtDevice.FtDevice2233Hp ||
                    deviceType == FtDevice.FtDevice4233Hp ||
                    deviceType == FtDevice.FtDevice2232Hp ||
                    deviceType == FtDevice.FtDevice4232Hp ||
                    deviceType == FtDevice.FtDevice2232Ha ||
                    deviceType == FtDevice.FtDevice4232Ha ||
                    deviceType == FtDevice.FtDevice2232)
                {
                    string description;
                    GetDescription(out description);
                    identifier = description.Substring(description.Length - 1);
                    return identifier;
                }
            }

            return identifier;
        }
    }

    #endregion
}