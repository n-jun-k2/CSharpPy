using System;
using System.Text;
using System.Runtime.InteropServices;

namespace Unmanaged
{
    public  static class Ptr
    {
        public static int PtrLength(IntPtr ptr)
        {
            int len = 0;
            while (Marshal.ReadByte(ptr, len) == 0) len++;
            return len;
        }

        public static string ToStringUTF8(IntPtr ptr, int length)
        {
            var buffer = new Byte[length];
            Marshal.Copy(ptr,buffer,0,length);
            return Encoding.UTF8.GetString(buffer);
        }

    }
}
