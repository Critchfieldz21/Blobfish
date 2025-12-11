namespace BackendLibrary
{
    /// <summary>
    /// Custom exception when Detect.cs detect method yields no results.
    /// </summary>
    public class DetectionException : Exception
    {
        public DetectionException() { }

        public DetectionException(string message) : base(message) { }

        public DetectionException(string message, Exception innerException) : base(message, innerException) { }
    }
}