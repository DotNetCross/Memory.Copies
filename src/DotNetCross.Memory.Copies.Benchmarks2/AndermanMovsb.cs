using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace DotNetCross.Memory.Copies.Benchmarks2
{
    //Used this code only for testing. This code works only on 64bit
    public static class AndermanMovsb
    {
        private const uint PAGE_READWRITE = 0x04;
        private const uint PAGE_EXECUTE = 0x10;
        private const uint PAGE_EXECUTE_READWRITE = 0x40;
        private const uint MEM_COMMIT = 0x1000;
        private const uint MEM_RELEASE = 0x8000;

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool VirtualProtect(IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, out uint lpflOldProtect);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr VirtualAlloc(IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool VirtualFree(IntPtr lpAddress, IntPtr dwSize, uint dwFreeType);

        public delegate int MyMoveSb(ulong RCX, ulong RDX, ulong R8);

        public static MyMoveSb movsb;
        public static MyMoveSb pinvoke;

        static AndermanMovsb()
        {
            //RAX = 0000008377040000 RBX = 0000000000000005 RCX = 0000000000000004 RDX = 0000000000000005 RSI = 0000008300018350 RDI = 0000000000000004 R8  = 0000000000000006 R9  = 000007F9C2460F84 R10 = 0000000000000000 R11 = 0000000000000000 R12 = 00000083754BDF70 R13 = 0000000000000004 R14 = 0000000000000006 R15 = 000000837555A4D0 RIP = 0000008377040000 RSP = 00000083754BDDF8 RBP = 00000083754BDEA0 EFL = 00000246 
            /*
                0:  49 89 fb                mov    r11,rdi
                3:  49 89 f2                mov    r10,rsi
                6:  48 89 ce                mov    rsi,rcx
                9:  48 89 d7                mov    rdi,rdx
                c:  4c 89 c1                mov    rcx,r8
                f:  f3 a4                   rep movs BYTE PTR es:[rdi],BYTE PTR ds:[rsi]
                11: 4c 89 d6                mov    rsi,r10
                14: 4c 89 df                mov    rdi,r11
                17: c3                      ret
              */
            byte[] assemblyCode = { 0x49, 0x89, 0xFB, 0x49, 0x89, 0xF2, 0x48, 0x89, 0xCE, 0x48, 0x89, 0xD7, 0x4C, 0x89, 0xC1, 0xF3, 0xA4, 0x4C, 0x89, 0xD6, 0x4C, 0x89, 0xDF, 0xC3 };
            byte[] assemblyCode2 = { 0xC3 };


            // We need to push the code bytes into a native buffer

            var bufPtr = IntPtr.Zero;

            try
            {
                // Put the sourcecode in a native buffer
                bufPtr = VirtualAlloc(IntPtr.Zero, (IntPtr)4096, MEM_COMMIT, PAGE_EXECUTE_READWRITE);

                Marshal.Copy(assemblyCode, 0, bufPtr, assemblyCode.Length);
                Marshal.Copy(assemblyCode2, 0, bufPtr+64, assemblyCode2.Length);
                
                uint oldProtection;
                var result = VirtualProtect(bufPtr, (IntPtr)assemblyCode.Length, PAGE_EXECUTE, out oldProtection);

                if (!result)
                {
                    throw new Win32Exception();
                }
                movsb = Marshal.GetDelegateForFunctionPointer<MyMoveSb>(bufPtr);
                pinvoke = Marshal.GetDelegateForFunctionPointer<MyMoveSb>(bufPtr+64);
                bufPtr = IntPtr.Zero;
            }
            finally
            {
                // Free the native buffer
                if (bufPtr != IntPtr.Zero)
                {
                    var result = VirtualFree(bufPtr, IntPtr.Zero, MEM_RELEASE);
                    if (!result)
                    {
                        throw new Win32Exception();
                    }
                }
            }
        }


        public static unsafe void Memmove(byte[] src, int srcOffset, byte[] dst, int dstOffset, int count)
        {
            if (count == 0) return;
            if (src == null || dst == null) throw new ArgumentNullException(nameof(src));
            if (srcOffset + count > src.Length) throw new ArgumentException(nameof(src));
            if (count < 0 || srcOffset < 0 || dstOffset < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (dstOffset + count > dst.Length) throw new ArgumentException(nameof(dst));

            fixed (byte* pSrcOrigin = &src[srcOffset])
            fixed (byte* pDstOrigin = &dst[dstOffset])
            {
                var pSrc = pSrcOrigin;
                var pDst = pDstOrigin;
                var result1 = movsb((ulong)pSrc, (ulong)pDst,(ulong)count);
            }
        }
        public static unsafe void Pinvoke(byte[] src, int srcOffset, byte[] dst, int dstOffset, int count)
        {
            if (count == 0) return;
            if (src == null || dst == null) throw new ArgumentNullException(nameof(src));
            if (srcOffset + count > src.Length) throw new ArgumentException(nameof(src));
            if (count < 0 || srcOffset < 0 || dstOffset < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (dstOffset + count > dst.Length) throw new ArgumentException(nameof(dst));

            fixed (byte* pSrcOrigin = &src[srcOffset])
            fixed (byte* pDstOrigin = &dst[dstOffset])
            {
                var pSrc = pSrcOrigin;
                var pDst = pDstOrigin;
                var result1 = pinvoke((ulong)pSrc, (ulong)pDst, (ulong)count);
            }
        }
    }
}