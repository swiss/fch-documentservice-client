namespace Swiss.FCh.DocumentService.Client;

[Serializable]
public class DocumentServiceException : Exception
{
    public DocumentServiceException()
    {
    }

    public DocumentServiceException(string message) : base(message)
    {
    }

    public DocumentServiceException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
