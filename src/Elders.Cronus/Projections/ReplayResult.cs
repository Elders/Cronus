namespace Elders.Cronus.Projections
{
    public struct ReplayResult
    {
        public ReplayResult(string error = null, bool isTimeOut = false, bool canRetry = false)
        {
            Error = error;
            IsTimeout = isTimeOut;
            ShouldRetry = canRetry;
        }

        public string Error { get; private set; }

        public bool IsSuccess { get { return string.IsNullOrEmpty(Error) && IsTimeout == false && ShouldRetry == false; } }

        public bool IsTimeout { get; private set; }

        public bool ShouldRetry { get; private set; }

        public static ReplayResult Timeout(string error)
        {
            return new ReplayResult(error, true);
        }

        public static ReplayResult RetryLater(string message)
        {
            return new ReplayResult(message, isTimeOut: false, canRetry: true);
        }
    }
}
