package calendarclient;

public class Main
{
    public static void main(String[] args)
    {
        try
        {
           ConsoleInterface consoleInterface = new ConsoleInterface();
           consoleInterface.start();
        }
        catch (Exception exception)
        {
            System.out.println(exception.getMessage());
        }
    }
}