namespace Venture;

public class FileLogger
{
    private readonly string fileName;

    public FileLogger(string fileName)
    {
        this.fileName = fileName;
    }

    public void Clear()
    {
        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }
    }

    public void Log(string message)
    {
        File.AppendAllLines(fileName, [message]);
    }
}
