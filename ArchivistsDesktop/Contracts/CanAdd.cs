namespace ArchivistsDesktop.Contracts;

public class CanAdd
{
    public bool IsAccepted = false;

    public void Accept()
    {
        IsAccepted = true;
    }
}