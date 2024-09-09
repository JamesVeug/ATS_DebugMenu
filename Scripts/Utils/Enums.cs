public enum ShowAll
{
    Any,    // Don't care. Show regardless
    Only,   // Only this
    Except, // Anything except this
}
	
public static class ShowAllExtensions
{
    public static bool Accepts(this ShowAll type, bool value)
    {
        switch (type)
        {
            case ShowAll.Any:
                return true;
            case ShowAll.Only:
                return value;
            case ShowAll.Except:
                return !value;
        }

        return false;
    }
}