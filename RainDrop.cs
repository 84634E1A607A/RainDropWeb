using RainDropWeb.Driver;
using RainDropWeb.Locale;
using RainDropWeb.Protocol;
using static System.String;

namespace RainDropWeb;

public class RainDrop
{
    public enum DeviceStatus
    {
        Ready = 0,
        Config = 4,
        Prefill = 5,
        Armed = 1,
        Wait = 7,
        Triggered = 3,
        Running = 3,
        Done = 2
    }

    private readonly Mutex _commandMutex = new();
    private readonly Ftdi _ftdi = new();
    private bool _isAdjustingSupplyVoltage;

    // Local copy of device status
    private readonly bool[] _oscilloscopeChannelEnabled = { false, false };
    private readonly bool[] _oscilloscopeChannelIs25V = { false, false };
    private readonly bool[] _supplyChannelEnabled = { false, false };
    

    /// <summary>
    ///     AnalogOutOffset A[0]B[1]; AnalogOutAmplitude A[2]B[3];
    ///     PowerSupplyOffset A[4]B[5];
    ///     Analog In [5V / 25V] [offset / amplitude] [A / B] [6-13].
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This shall not be null when a device is opened.
    ///     </para>
    /// </remarks>
    private byte[] _calibrationArray = null!;

    private int _oscilloscopeChannelDataPoints = 2048;

    public string CurrentDevice { get; private set; } = Empty;

    public bool OscilloscopeRunning { get; private set; }

    public IEnumerable<string> GetDevices()
    {
        var error = _ftdi.GetNumberOfDevices(out var devicesCount);
        if (error != Ftdi.FtStatus.FtOk) throw new Exception(Localization.Localize("DEVICE_QUERY_ERR") + $"{error.ToString()}");//$"Error querying number of devices: {error.ToString()}");

        if (devicesCount == 0) return Array.Empty<string>();

        var devStatus = new Ftdi.FtDeviceInfoNode[devicesCount];
        for (var i = 0; i < devicesCount; ++i) devStatus[i] = new Ftdi.FtDeviceInfoNode();
        error = _ftdi.GetDeviceList(devStatus);
        if (error != Ftdi.FtStatus.FtOk) throw new Exception(Localization.Localize("DEVICE_LIST_ERR") + $"{error.ToString()}");//$"Error when getting devices list: {error.ToString()}");

        return devStatus.Select(s => s.SerialNumber);
    }

    public void ConnectToDevice(string serial)
    {
        if (_ftdi.IsOpen) throw new InvalidOperationException(Localization.Localize("DEVICE_ALREADY_OPEN"));

        try
        {
            _ftdi.OpenBySerialNumber(serial);
            _ftdi.SetBitMode(0x00, 0x40);
            var error = _ftdi.SetTimeouts(1500, 500);
            _ftdi.SetBaudRate(1152000);

            if (error != Ftdi.FtStatus.FtOk)
                throw new Exception(error.ToString());

            CurrentDevice = serial;
            _calibrationArray = SendCommand(new GetCalibrationCommand())![1..15];
            InitializeDevice();
        }
        catch
        {
            DisconnectFromDevice();
            throw;
        }
    }

    private void InitializeDevice()
    {
        SetSupplyEnabled(false, false);
        SetSupplyEnabled(true, false);
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
        _oscilloscopeChannelEnabled[channel ? 1 : 0] = enable;
    }

    public void SetOscilloscopeChannelRange(bool channel, int range)
    {
        if (range is not (5 or 25))
            throw new ArgumentOutOfRangeException(nameof(range), Localization.Localize("OSC_RANGE_ERR"));

        _oscilloscopeChannelIs25V[channel ? 1 : 0] = range is 25;
        SendCommand(new SetOscilloscopeChannelRangeCommand(channel, range is 25));
    }

    public void SetOscilloscopeSamplingFrequency(float frequency)
    {
        if (frequency is < 1f or > 40e6f)
            throw new ArgumentOutOfRangeException(nameof(frequency), Localization.Localize("FREQUENCY_OUT_OF_RANGE"));

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
                Localization.Localize("OSC_DATA_RANGE_ERR"));

        _oscilloscopeChannelDataPoints = dataPoints;
        SendCommand(new SetOscilloscopeBufferSizeCommand(dataPoints));
    }

    public void SetOscilloscopeRunning(bool running)
    {
        SendCommand(new SetOscilloscopeRunningCommand(running));
        OscilloscopeRunning = running;
    }

    public DeviceStatus GetDeviceStatus()
    {
        var status = SendCommand(new GetOscilloscopeStatusCommand())!;
        SendCommand(new StopGettingOscilloscopeStatusCommand());
        return (DeviceStatus)status[3];
    }

    public (float[]? A, float[]? B) ReadOscilloscopeData()
    {
        if (!(_oscilloscopeChannelEnabled[0] || _oscilloscopeChannelEnabled[1]))
            throw new InvalidOperationException(Localization.Localize("CHANNEL_UNAVAILABLE"));

        return (_oscilloscopeChannelEnabled[0] ? ReadOscilloscopeData(false) : null,
            _oscilloscopeChannelEnabled[1] ? ReadOscilloscopeData(true) : null);
    }

    private float[] ReadOscilloscopeData(bool channel)
    {
        var is25V = _oscilloscopeChannelIs25V[channel ? 1 : 0];
        int calibrationOffset;
        float calibratedMaxAmplitude;

        if (!is25V)
        {
            calibrationOffset = -123 + (channel ? _calibrationArray[7] : _calibrationArray[6]);
            var calibrationAmplitude = channel ? _calibrationArray[9] : _calibrationArray[8];
            calibratedMaxAmplitude = (float)(1 / (0.00048828125 * calibrationAmplitude + 0.19));
        }
        else
        {
            calibrationOffset = -123 + (channel ? _calibrationArray[11] : _calibrationArray[10]);
            var calibrationAmplitude = channel ? _calibrationArray[13] : _calibrationArray[12];
            calibratedMaxAmplitude = (float)((channel ? -1 : 1) / (0.0001220703125 * calibrationAmplitude + 0.036));
        }

        var data = SendCommand(new ReadOscilloscopeChannelDataCommand(channel, _oscilloscopeChannelDataPoints))!;
        var decoded = new float[_oscilloscopeChannelDataPoints];
        for (var i = 0; i < _oscilloscopeChannelDataPoints; ++i)
            decoded[i] = calibratedMaxAmplitude *
                         ((data[i << 1] * 0x100 + data[(i << 1) + 1] - 0x800 + calibrationOffset) / (float)0x800);

        return decoded;
    }

    public void SetSupplyEnabled(bool isNegative, bool enabled)
    {
        SendCommand(new SetSupplyEnabledCommand(isNegative, enabled));
        _supplyChannelEnabled[isNegative ? 1 : 0] = enabled;
    }

    public void SetSupplyVoltage(bool isNegative, float value)
    {
        if (_isAdjustingSupplyVoltage)
            throw new InvalidOperationException(Localization.Localize("SUPPLY_ADJUSTING"));

        _isAdjustingSupplyVoltage = true;
        try
        {
            var previouslyEnabled = _supplyChannelEnabled[isNegative ? 1 : 0];
            if (previouslyEnabled)
            {
                SetSupplyEnabled(isNegative, false);
                Task.Delay(200).Wait();
            }

            SendCommand(new SetSupplyVoltageCommand(isNegative, value, _calibrationArray[isNegative ? 5 : 4]));

            if (previouslyEnabled)
            {
                Task.Delay(200).Wait();
                SetSupplyEnabled(isNegative, true);
            }
        }
        finally
        {
            _isAdjustingSupplyVoltage = false;
        }
    }

    public void SetWaveGeneratorFunction(bool isChannel2, WaveGeneratorFunction function)
    {
        SendCommand(new SetWaveGeneratorFunctionCommand(isChannel2, function));
    }

    public void SetWaveGeneratorFrequency(bool isChannel2, float frequency)
    {
        SendCommand(new SetWaveGeneratorFrequencyCommand(isChannel2, frequency));
    }

    public void SetWaveGeneratorOffsetAndAmplitude(bool isChannel2, float offset, float amplitude)
    {
        SendCommand(new SetWaveGeneratorAmplitudeCommand(isChannel2, amplitude, _calibrationArray[isChannel2 ? 3 : 1]));
        SendCommand(new SetWaveGeneratorOffsetCommand(isChannel2, offset, amplitude,
            _calibrationArray[isChannel2 ? 2 : 0], _calibrationArray[isChannel2 ? 3 : 1]));
    }

    public void SetWaveGeneratorSymmetry(bool isChannel2, float symmetry)
    {
        SendCommand(new SetWaveGeneratorSymmetryCommand(isChannel2, symmetry));
    }

    public void SetWaveGeneratorPhase(bool isChannel2, float phase)
    {
        SendCommand(new SetWaveGeneratorPhaseCommand(isChannel2, phase));
    }

    public void SetWaveGeneratorEnabled(bool isChannel2, bool enabled)
    {
        SendCommand(new SetWaveGeneratorEnabledCommand(isChannel2, enabled));
    }

    private byte[]? SendCommand(BaseCommand command)
    {
        // Allow only one command at a time.
        _commandMutex.WaitOne();

        try
        {
            if (!_ftdi.IsOpen) throw new InvalidOperationException(Localization.Localize("DEVICE_NOT_OPEN"));

            _ftdi.Purge(Ftdi.FtPurge.FtPurgeRx | Ftdi.FtPurge.FtPurgeTx);

            var bytes = (byte[])command;
            var error = _ftdi.Write(bytes, bytes.Length, out var bytesWritten);

            if (error != Ftdi.FtStatus.FtOk)
                throw new Exception(error.ToString());

            if (bytesWritten != bytes.Length)
                throw new Exception(Localization.Localize("UNEXPECTED_WRITTEN_DATA_LENGTH"));

            if (command.BytesToReceive == 0)
                return null;

            error = _ftdi.Read(out byte[] receivedData, command.BytesToReceive, out var bytesReceived);

            if (error != Ftdi.FtStatus.FtOk)
                throw new Exception(error.ToString());

            if (bytesReceived != command.BytesToReceive)
                throw new Exception(Localization.Localize("UNEXPECTED_RECEIVED_DATA_LENGTH"));

            return receivedData;
        }
        finally
        {
            _commandMutex.ReleaseMutex();
        }
    }
}