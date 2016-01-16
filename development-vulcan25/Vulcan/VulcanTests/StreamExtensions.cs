using System;
using System.IO;
using System.Linq;

namespace VulcanTests
{
    public static class StreamExtensions
    {
        public static void CopyTo(this Stream source, Stream destination)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }

            System.Diagnostics.Debug.Assert(source.CanRead, "src.CanRead");
            System.Diagnostics.Debug.Assert(destination.CanWrite, "destination.CanWrite");

            int readCount;
            var buffer = new byte[8192];
            while ((readCount = source.Read(buffer, 0, buffer.Length)) != 0)
            {
                destination.Write(buffer, 0, readCount);
            }
        }

        public static bool IsEqualTo(this Stream source, Stream destination)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }

            System.Diagnostics.Debug.Assert(source.CanRead, "src.CanRead");
            System.Diagnostics.Debug.Assert(destination.CanRead, "dest.CanWrite");

            if (source.Length != destination.Length)
            {
                return false;
            }

            int srcReadCount = -1;
            int destReadCount = -1;
            var srcBuffer = new byte[8192];
            var destBuffer = new byte[8192];
            while (srcReadCount == destReadCount && srcReadCount != 0)
            {
                srcReadCount = source.Read(srcBuffer, 0, srcBuffer.Length);
                destReadCount = destination.Read(destBuffer, 0, destBuffer.Length);
                if (!srcBuffer.SequenceEqual(destBuffer))
                {
                    return false;
                }
            }

            return srcReadCount == destReadCount;
        }
    }
}
