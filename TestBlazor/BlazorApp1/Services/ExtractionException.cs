namespace BackendLibrary
{
    /// <summary>
    /// Custom exception for errors that occur during data extraction processes.
    /// </summary>
    public class ExtractionException : Exception
    {
        public ExtractionException() { }

        public ExtractionException(string message) : base(message) { }

        public ExtractionException(string message, Exception innerException) : base(message, innerException) { }
    }
}