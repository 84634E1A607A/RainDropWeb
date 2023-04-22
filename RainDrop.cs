using RainDropWeb.Driver;
using RainDropWeb.Protocol;
using static System.String;

namespace RainDropWeb;

public class RainDrop
{
    private readonly Ftdi _ftdi = new();

    // This shall not be null when a device is opened.
    private byte[] _calibrationArray = null!;
    private readonly bool[] _oscilloscopeChannelIs25V = { false, false };
    private int _oscilloscopeChannelDataPoints = 2048;

    public string CurrentDevice { get; private set; } = Empty;

    public IEnumerable<string> GetDevices()
    {
        var error = _ftdi.GetNumberOfDevices(out var devicesCount);
        if (error != Ftdi.FtStatus.FtOk) throw new Exception($"Error querying number of devices: {error.ToString()}");

        if (devicesCount == 0) return Array.Empty<string>();

        var devStatus = new Ftdi.FtDeviceInfoNode[devicesCount];
        for (var i = 0; i < devicesCount; ++i) devStatus[i] = new Ftdi.FtDeviceInfoNode();
        error = _ftdi.GetDeviceList(devStatus);
        if (error != Ftdi.FtStatus.FtOk) throw new Exception($"Error when getting devices list: {error.ToString()}");

        return devStatus.Select(s => s.SerialNumber);
    }

    public void ConnectToDevice(string serial)
    {
        if (_ftdi.IsOpen) throw new InvalidOperationException("A device is already open.");

        _ftdi.OpenBySerialNumber(serial);
        _ftdi.SetBitMode(0x00, 0x40);
        var error = _ftdi.SetTimeouts(1500, 500);
        // _ftdi.SetBaudRate(3000000);

        if (error != Ftdi.FtStatus.FtOk)
            throw new Exception(error.ToString());

        CurrentDevice = serial;

        try
        {
            _calibrationArray = SendCommand(new GetCalibrationCommand())!;
        }
        catch
        {
            DisconnectFromDevice();
            throw;
        }
    }

    public void DisconnectFromDevice()
    {
        if (!_ftdi.IsOpen) return;

        _ftdi.Close();
        CurrentDevice = Empty;
    }

    public void SetOscilloscopeChannelState(bool channel, bool enable)
    {
        SendCommand(new SetOscilloscopeChannelStateCommand(channel, enable));
    }

    public void SetOscilloscopeChannelRange(bool channel, int range)
    {
        if (range is not (5 or 25))
            throw new ArgumentOutOfRangeException(nameof(range), "Range must be 5 or 25.");

        _oscilloscopeChannelIs25V[channel ? 1 : 0] = range is 25;
        SendCommand(new SetOscilloscopeChannelRangeCommand(channel, range is 25));
    }

    public void SetOscilloscopeSamplingFrequency(float frequency)
    {
        if (frequency is < 1f or > 40e6f)
            throw new ArgumentOutOfRangeException(nameof(frequency), "Frequency must be between 1 and 40M Hz.");

        SendCommand(new SetOscilloscopeSamplingFrequencyCommand(frequency));
    }

    public void SetOscilloscopeTrigger(bool autoTimeout, OscilloscopeTriggerSource source,
        float level, OscilloscopeTriggerCondition condition)
    {
        SendCommand(new SetOscilloscopeTriggerTimeoutCommand(!autoTimeout));
        SendCommand(new SetOscilloscopeTriggerSourceCommand(source));
        SendCommand(new SetOscilloscopeTriggerLevelCommand(level, source switch
        {
            OscilloscopeTriggerSource.DetectorAnalogInCh1 => _oscilloscopeChannelIs25V[0],
            OscilloscopeTriggerSource.DetectorAnalogInCh2 => _oscilloscopeChannelIs25V[1],
            _ => false
        }));
        SendCommand(new SetOscilloscopeTriggerConditionCommand(condition));
    }

    public void SetOscilloscopeDataPointsCount(int dataPoints)
    {
        if (dataPoints is not (32 or 64 or 128 or 256 or 512 or 1024 or 2048))
            throw new ArgumentOutOfRangeException(nameof(dataPoints),
                "Data points must be 32, 64, 128, 256, 512, 1024 or 2048.");

        _oscilloscopeChannelDataPoints = dataPoints;
        SendCommand(new SetOscilloscopeBufferSizeCommand(dataPoints));
    }

    public void SetOscilloscopeRunning(bool running)
    {
        SendCommand(new SetOscilloscopeRunningCommand(running));
    }

    public int GetOscilloscopeStatus()
    {
        var status = SendCommand(new GetOscilloscopeStatusCommand())!;
        SendCommand(new StopGettingOscilloscopeStatusCommand());
        return status[0];
    }

    public float[] ReadOscilloscopeData(bool channel)
    {
        var data = SendCommand(new ReadOscilloscopeChannelDataCommand(channel, _oscilloscopeChannelDataPoints))!;
        var decoded = new float[_oscilloscopeChannelDataPoints];
        for (var i = 0; i < _oscilloscopeChannelDataPoints; ++i)
            decoded[i] = (_oscilloscopeChannelIs25V[channel ? 1 : 0] ? 25 : 5) *
                         ((data[i << 1] * 0x100 + data[(i << 1) + 1] - 0x800 + _calibrationArray[6]) / (float)0x1000) * 2;

        return decoded;
    }

    private byte[]? SendCommand(BaseCommand command)
    {
        if (!_ftdi.IsOpen) throw new InvalidOperationException("No device is open.");

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