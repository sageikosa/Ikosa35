using System;
using System.Collections.Generic;
using System.IO;

namespace Ikosa.Packaging
{
    public static class StreamHelper
    {
        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[32768];
            long TempPos = input.Position;
            while (true)
            {
                int read = input.Read(buffer, 0, buffer.Length);
                if (read <= 0)
                    break;
                output.Write(buffer, 0, read);
            }
            input.Position = TempPos;
        }
    }
}
