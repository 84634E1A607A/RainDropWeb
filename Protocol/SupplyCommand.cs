namespace RainDropWeb.Protocol;

public abstract class SupplyCommand : BaseCommand
{
    protected override string Command => base.Command + "03";
}

public class SetSupplyVoltageCommand : SupplyCommand
{
    private readonly bool _isNegativeChannel;
    private readonly short _voltage;

    public SetSupplyVoltageCommand(bool isNegativeChannel, float voltage, int calibrationOffset)
    {
        if (isNegativeChannel && voltage is not (<= 0 and >= -5))
            throw new ArgumentOutOfRangeException(nameof(voltage),
                "Negative channel voltage should be between -5 and 0");

        if (!isNegativeChannel && voltage is not (>= 0 and <= 5))
            throw new ArgumentOutOfRangeException(nameof(voltage),
                "Positive channel voltage should be between 0 and 5");

        _isNegativeChannel = isNegativeChannel;

        if (!isNegativeChannel)
            _voltage = (short)((5.4f - calibrationOffset * 0.001953125f - voltage) * 4096 / 12.175f);
        else _voltage = (short)((5.26f + calibrationOffset * 0.001953125f + voltage) * 4096 / 12.175f);
    }

    protected override string Command
        => base.Command + (_isNegativeChannel ? "02" : "01") + "0000" + _voltage.ToString("x4");
}

public class SetSupplyEnabledCommand : SupplyCommand
{
    private readonly bool _enable;
    private readonly bool _isNegativeChannel;

    public SetSupplyEnabledCommand(bool isNegativeChannel, bool enable)
    {
        _isNegativeChannel = isNegativeChannel;
        _enable = enable;
    }

    protected override string Command =>
        base.Command + "00" + ((_isNegativeChannel ? 2 : 0) + (_enable ? 1 : 0)).ToString("x2");
}