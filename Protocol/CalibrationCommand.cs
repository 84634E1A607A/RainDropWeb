namespace RainDropWeb.Protocol;

public abstract class CalibrationCommand : BaseCommand
{
    protected override string Command => base.Command + "06";
}

public class GetCalibrationCommand : CalibrationCommand
{
    public override uint BytesToReceive => 17;
    protected override string Command => base.Command + "01";
}