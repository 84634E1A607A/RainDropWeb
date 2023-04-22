using static System.String;

namespace RainDropWeb;

using Protocol;
using Driver;

public class RainDrop
{
    private readonly Ftdi _ftdi = new();
    
    // This shall not be null when a device is opened.
    private byte[] _calibrationArray = null!;

    public string CurrentDevice { get; private set; } = Empty;

    public IEnumerable<string> GetDevices()
    {
        var error = _ftdi.GetNumberOfDevices(out var devicesCount);
        if (error != Ftdi.FtStatus.FtOk)
        {
            throw new Exception($"Error querying number of devices: {error.ToString()}");
        }

        if (devicesCount == 0)
        {
            return Array.Empty<string>();
        }

        var devStatus = new Ftdi.FtDeviceInfoNode[devicesCount];
        for (var i = 0; i < devicesCount; ++i) devStatus[i] = new Ftdi.FtDeviceInfoNode();
        error = _ftdi.GetDeviceList(devStatus);
        if (error != Ftdi.FtStatus.FtOk)
        {
            throw new Exception($"Error when getting devices list: {error.ToString()}");
        }

        return devStatus.Select(s => s.SerialNumber);
    }

    public void ConnectToDevice(string serial)
    {
        if (_ftdi.IsOpen)
        {
            throw new InvalidOperationException("A device is already open.");
        }

        _ftdi.OpenBySerialNumber(serial);
        _ftdi.SetBitMode(0x00, 0x40);
        var error = _ftdi.SetTimeouts(1000, 500);
        // _ftdi.SetBaudRate(3000000);

        if (error != Ftdi.FtStatus.FtOk)
            throw new Exception(error.ToString());

        CurrentDevice = serial;

        _calibrationArray = SendCommand(new GetCalibrationCommand())!;
    }

    public void DisconnectFromDevice()
    {
        if (!_ftdi.IsOpen)
        {
            return;
        }

        _ftdi.Close();
        CurrentDevice = Empty;
    }

    private byte[]? SendCommand(BaseCommand command)
    {
        if (!_ftdi.IsOpen)
        {
            throw new InvalidOperationException("No device is open.");
        }

        _ftdi.Purge(Ftdi.FtPurge.FtPurgeRx | Ftdi.FtPurge.FtPurgeTx);

        var bytes = (byte[])command;
        var error = _ftdi.Write(bytes, bytes.Length, out var bytesWritten);

        if (error != Ftdi.FtStatus.FtOk)
            throw new Exception(error.ToString());

        if (bytesWritten != bytes.Length)
            throw new Exception("Written data length is not expected.");

        if (command.BytesToReceive == 0)
            return null;

        error = _ftdi.Read(out byte[] receivedData, command.BytesToReceive, out var bytesReceived);

        if (error != Ftdi.FtStatus.FtOk)
            throw new Exception(error.ToString());

        if (bytesReceived != command.BytesToReceive)
            throw new Exception("Received data length is not expected.");

        return receivedData;
    }
}