using RainDropWeb.Locale;

namespace RainDropWeb.Protocol;

public abstract class OscilloscopeCommand : BaseCommand
{
    protected override string Command => base.Command + "01";
}

public class SetOscilloscopeChannelStateCommand : OscilloscopeCommand
{
    private readonly bool _channel;
    private readonly bool _enable;

    public SetOscilloscopeChannelStateCommand(bool channel, bool enable)
    {
        _channel = channel;
        _enable = enable;
    }

    protected override string Command
        => base.Command + "07" + (_channel ? "01" : "00") + (_enable ? "01" : "00");
}

public class SetOscilloscopeChannelRangeCommand : OscilloscopeCommand
{
    private readonly bool _25V;
    private readonly bool _channel;

    public SetOscilloscopeChannelRangeCommand(bool channel, bool is25V)
    {
        _channel = channel;
        _25V = is25V;
    }

    protected override string Command
        => base.Command + "04" + (_channel ? "01" : "00") + (_25V ? "01" : "00");
}

public class SetOscilloscopeSamplingFrequencyCommand : OscilloscopeCommand
{
    private const float TickTime = 2.5e-8f;
    private readonly float _frequency;

    public SetOscilloscopeSamplingFrequencyCommand(float frequency)
    {
        _frequency = frequency;
    }

    protected override string Command
        => base.Command + "05" + ((int)(1 / _frequency / TickTime)).ToString("x8");
}

public class SetOscilloscopeBufferSizeCommand : OscilloscopeCommand
{
    private readonly int _size;

    public SetOscilloscopeBufferSizeCommand(int size)
    {
        if (size is not (32 or 64 or 128 or 256 or 512 or 1024 or 2048))
            throw new ArgumentOutOfRangeException(nameof(size), Localization.Localize("SIZE_ERR"));

        _size = size;
    }

    protected override string Command
        => base.Command + "06" + ((int)Math.Log2(_size) - 5).ToString("x2");
}

public class SetOscilloscopeRunningCommand : OscilloscopeCommand
{
    private readonly bool _enabled;

    public SetOscilloscopeRunningCommand(bool enabled)
    {
        _enabled = enabled;
    }

    protected override string Command
        => base.Command + "08" + (_enabled ? "01" : "00");
}

public class GetOscilloscopeStatusCommand : OscilloscopeCommand
{
    public override uint BytesToReceive => 4;
    protected override string Command => base.Command + "09";
}

public class StopGettingOscilloscopeStatusCommand : OscilloscopeCommand
{
    protected override string Command => base.Command + "0a";
}

public class ReadOscilloscopeChannelDataCommand : OscilloscopeCommand
{
    private readonly bool _channel;
    private readonly int _size;

    public ReadOscilloscopeChannelDataCommand(bool channel, int size)
    {
        _channel = channel;
        _size = size;
    }

    public override uint BytesToReceive => (uint)(2 * _size);
    protected override string Command => base.Command + (_channel ? "02" : "01");
}

public abstract class OscilloscopeTriggerCommand : BaseCommand
{
    protected override string Command => base.Command + "04";
}

public class SetOscilloscopeTriggerTimeoutCommand : OscilloscopeTriggerCommand
{
    private readonly bool _forever;

    public SetOscilloscopeTriggerTimeoutCommand(bool forever)
    {
        _forever = forever;
    }

    protected override string Command
        => base.Command + "04" + (_forever ? "00" : "01");
}

public class SetOscilloscopeTriggerSourceCommand : OscilloscopeTriggerCommand
{
    private readonly OscilloscopeTriggerSource _source;

    public SetOscilloscopeTriggerSourceCommand(OscilloscopeTriggerSource source)
    {
        _source = source;
    }

    protected override string Command
        => base.Command + "00" + ((int)_source).ToString("x2");
}

public enum OscilloscopeTriggerSource
{
    None = 0,
    DetectorAnalogInCh1 = 1,
    DetectorAnalogInCh2 = 2,
    AnalogOut1 = 3,
    AnalogOut2 = 4,
    DetectorDigitalIn = 5,
    DigitalOut = 6,
    Manual = 7,
    External1 = 8,
    External2 = 9,
    DigitalIn = 10
}

public class SetOscilloscopeTriggerTypeCommand : OscilloscopeTriggerCommand
{
    private readonly OscilloscopeTriggerType _type;

    public SetOscilloscopeTriggerTypeCommand(OscilloscopeTriggerType type)
    {
        _type = type;
    }

    protected override string Command => throw
        // return base.Command + "??" + ((int)_type).ToString("x2");
        new NotImplementedException();
}

// Not used
public enum OscilloscopeTriggerType
{
    Edge = 0,
    Pulse = 1,
    Transition = 2
}

public class SetOscilloscopeTriggerLevelCommand : OscilloscopeTriggerCommand
{
    private readonly ushort _level;

    public SetOscilloscopeTriggerLevelCommand(float level, bool is25V)
    {
        float range = is25V ? 25 : 5;
        if (level < -range || level > range)
            throw new ArgumentOutOfRangeException(nameof(level),
                 string.Format(Localization.Localize("LEVEL_OUT_OF_RANGE"), level, range));

        _level = (ushort)(2048 + (int)(level / range * 2048));
    }

    protected override string Command
        => base.Command + "03" + _level.ToString("x4");
}

public class SetOscilloscopeTriggerConditionCommand : OscilloscopeTriggerCommand
{
    private readonly OscilloscopeTriggerCondition _condition;

    public SetOscilloscopeTriggerConditionCommand(OscilloscopeTriggerCondition condition)
    {
        _condition = condition;
    }

    protected override string Command
        => base.Command + "02" + ((int)_condition).ToString("x2");
}

public enum OscilloscopeTriggerCondition
{
    Rise = 0,
    Fall = 1,
    Edge = 2
}