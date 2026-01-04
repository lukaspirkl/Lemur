namespace Venture;

public class FileLogger
{
    private readonly string m_FileName;

    public FileLogger(string fileName)
    {
        m_FileName = fileName;
    }

    public void Clear()
    {
        if (File.Exists(m_FileName))
        {
            File.Delete(m_FileName);
        }
    }

    public void Log(string message)
    {
        File.AppendAllLines(m_FileName, [message]);
    }
}
