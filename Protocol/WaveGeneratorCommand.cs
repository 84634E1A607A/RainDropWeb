using RainDropWeb.Locale;

namespace RainDropWeb.Protocol;

public abstract class WaveGeneratorCommand : BaseCommand
{
    private readonly bool _isChannel2;
    private readonly string _subFunction;

    protected WaveGeneratorCommand(string subFunction, bool isChannel2)
    {
        _isChannel2 = isChannel2;
        _subFunction = subFunction;
    }

    protected override string Command => base.Command + "02" + _subFunction + (_isChannel2 ? "01" : "00");
}

public enum WaveGeneratorFunction
{
    Direct = 0,
    Sine = 1,
    Square = 2,
    Triangle = 3,
    RampUp = 4,
    RampDown = 5,
    Noise = 6,
    Pulse = 7,
    Trapezium = 8,
    SinePower = 9,
    Custom = 10,
    Play = 31
}

public class SetWaveGeneratorFunctionCommand : WaveGeneratorCommand
{
    private readonly string _function;

    public SetWaveGeneratorFunctionCommand(bool isChannel2, WaveGeneratorFunction function) : base("00", isChannel2)
    {
        _function = ((byte)function).ToString("x2");
    }

    protected override string Command => base.Command + _function + "ff";
}

public class SetWaveGeneratorFrequencyCommand : WaveGeneratorCommand
{
    private readonly string _frequency;

    public SetWaveGeneratorFrequencyCommand(bool isChannel2, float frequency) : base("01", isChannel2)
    {
        if (frequency is < 0 or > 40e6f or float.NaN)
            throw new ArgumentOutOfRangeException(nameof(frequency), Localization.Localize("FREQUENCY_OUT_OF_RANGE"));

        _frequency = ((uint)(frequency * 4294967296 / 40e6)).ToString("x8");
    }

    protected override string Command => base.Command + _frequency + "ff";
}

public class SetWaveGeneratorAmplitudeCommand : WaveGeneratorCommand
{
    private readonly string _amplitude;

    public SetWaveGeneratorAmplitudeCommand(bool isChannel2, float amplitude, int calibration) : base("02", isChannel2)
    {
        _amplitude = ((uint)(amplitude / (calibration * 0.001953125f + 5) * 16383)).ToString("x8");
    }

    protected override string Command => base.Command + _amplitude + "ff";
}

public class SetWaveGeneratorOffsetCommand : WaveGeneratorCommand
{
    private readonly string _offset;

    public SetWaveGeneratorOffsetCommand(bool isChannel2, float offset, float amplitude, int calibrationOffset,
        int calibrationAmplitude) : base("03", isChannel2)
    {
        _offset = ((uint)(
            (amplitude / (1 + calibrationAmplitude * 0.001953125f / 5) + offset + 0.4312f +
             calibrationOffset * 0.001953125f) * 4096 / 22.80208f
        )).ToString("x8");
    }

    protected override string Command => base.Command + _offset;
}

public class SetWaveGeneratorSymmetryCommand : WaveGeneratorCommand
{
    private readonly string _symmetry;

    public SetWaveGeneratorSymmetryCommand(bool isChannel2, float symmetry) : base("04", isChannel2)
    {
        if (symmetry is not (>= 0 and <= 1))
            throw new ArgumentOutOfRangeException(nameof(symmetry), Localization.Localize("SYM_UNFIT"));

        _symmetry = ((byte)(symmetry * 255)).ToString("x2");
    }

    protected override string Command => base.Command + _symmetry;
}

public class SetWaveGeneratorPhaseCommand : WaveGeneratorCommand
{
    private readonly string _phase;

    public SetWaveGeneratorPhaseCommand(bool isChannel2, float phase) : base("05", isChannel2)
    {
        _phase = "00000000"; // TODO
    }

    protected override string Command => base.Command + _phase;
}

public class SetWaveGeneratorEnabledCommand : WaveGeneratorCommand
{
    private readonly string _enabled;

    public SetWaveGeneratorEnabledCommand(bool isChannel2, bool enabled) : base("0b", isChannel2)
    {
        _enabled = enabled ? "01" : "00";
    }

    protected override string Command => base.Command + _enabled;
}