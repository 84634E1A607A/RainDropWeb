namespace RainDropWeb.Protocol;

public class BaseCommand
{
    public virtual uint BytesToReceive => 0;

    protected virtual string Command => "aaff";

    public static implicit operator string(BaseCommand command)
    {
        return command.Command;
    }

    public static implicit operator byte[](BaseCommand command)
    {
        return command.Command.ToByteArray();
    }
}