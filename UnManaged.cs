using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Unmanaged
{

    public sealed class WinAPI
    {
        const string dll = "kernel32";

        [DllImport(dll, EntryPoint = "SetDllDirectory", CharSet = CharSet.Ansi, SetLastError = true)]
        internal static extern bool SetDllDirectory([MarshalAs(UnmanagedType.LPStr)]string lpLibFileName);

        [DllImport(dll,EntryPoint = "LoadLibrary" , CharSet = CharSet.Ansi,SetLastError =true)]
        internal static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpLibFileName);

        [DllImport(dll, EntryPoint = "FreeLibrary", CharSet = CharSet.Ansi, SetLastError = true)]
        internal static extern bool FreeLibrary();

        [DllImport(dll, EntryPoint = "GetProcAddress", CharSet = CharSet.Ansi, SetLastError = true)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)]string lpProcName);

        internal static bool IsNotHandle(IntPtr handle) => handle == null || handle == IntPtr.Zero;

    }

    public sealed class DllImport
    {
        Dictionary<string, Delegate> cash;

        public IntPtr Handle
        {
            get;private set;
        }

        public void Load(string name)
        {
            if(cash == null)
                cash = new Dictionary<string, Delegate>();
            cash.Clear();

            Handle = WinAPI.LoadLibrary(name);
            if (WinAPI.IsNotHandle(Handle))
                throw new System.IO.FileNotFoundException(name);
        }

        public void Free() => WinAPI.FreeLibrary();

        public T GetFunc<T>(string name = "") where T : class
        {
            //関数名を型か引数かに合わせる。
            var funcname = string.IsNullOrEmpty(name) ? typeof(T).Name : name;

            if (cash.ContainsKey(funcname)) return cash[funcname] as T;

            //関数を取得
            var funcptr = WinAPI.GetProcAddress(Handle,funcname);

            if (WinAPI.IsNotHandle(Handle)) throw new System.ArgumentNullException(funcname);

            var func = Marshal.GetDelegateForFunctionPointer<T>(funcptr);

            cash.Add(funcname, func as Delegate);

            return func;

        }

    }

}
