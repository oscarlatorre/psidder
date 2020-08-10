using System;

namespace PSidder
{
    [Serializable]
    public class FileIsNotUserProfileDiskException : Exception
    {
        public FileIsNotUserProfileDiskException() { }
        public FileIsNotUserProfileDiskException(string message) : base(message) { }
        public FileIsNotUserProfileDiskException(string message, Exception inner) : base(message, inner) { }
        protected FileIsNotUserProfileDiskException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
