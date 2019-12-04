using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace CsharpPy
{ 

    public sealed class PythonAPI:IDisposable
    {
        static Mutex mutex =new Mutex();
        static PythonAPI api;
        public static PythonAPI instance
        {
            get
            {
                mutex.WaitOne();
                if (api == null)
                    api = new PythonAPI();
                mutex.ReleaseMutex();
                return api;
            }
        }

        Dictionary<string, Delegate> imports;

        public Unmanaged.DllImport Dll { get; private set; } = new Unmanaged.DllImport();
        public static readonly int PyTrue = 1;
        public static readonly int PyFalse = 0;

        private PythonAPI(string path = "python37")
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException();

            Dll.Load(path);
        }


        public void SetPath(string path) => Dll.GetFunc<Py_SetPath>()(path);
        public void SetPythonHome(string path) => Dll.GetFunc<Py_SetPythonHome>()(path);
        public void Initilize(int value = 1) => Dll.GetFunc<Py_InitializeEx>()(value);
        public string GetPythonHome() => Marshal.PtrToStringUni(Dll.GetFunc<Py_GetPythonHome>()());
        public string GetProgramFullPath() => Marshal.PtrToStringUni(Dll.GetFunc<Py_GetProgramFullPath>()());
        public string GetVersion() => Marshal.PtrToStringAnsi(Dll.GetFunc<Py_GetVersion>()());
        public string GetCopyright() => Marshal.PtrToStringAnsi(Dll.GetFunc<Py_GetCopyright>()());
        public string GetPath() => Marshal.PtrToStringAnsi(Dll.GetFunc<Py_GetPath>()());
        public string GetPlatform() => Marshal.PtrToStringAnsi(Dll.GetFunc<Py_GetPlatform>()());
        public string GetCompiler() => Marshal.PtrToStringAnsi(Dll.GetFunc<Py_GetCompiler>()());
        public bool IsInitlized { get => Dll.GetFunc<Py_IsInitialized>()() == PyTrue; }

        public sealed class PyObject : IDisposable
        {
            IntPtr handle;

            private PyObject(IntPtr ptr)
            {
                if (ptr == null || ptr == IntPtr.Zero) 
                    throw new ArgumentNullException();
                handle = ptr;
            }
            static public PyObject Import(string name)=> new PyObject( instance.Dll.GetFunc<PyImport_ImportModule>()(name));
            static public PyObject Tuple(int size) =>new PyObject(instance.Dll.GetFunc<PyTuple_New>()(size));
            static public PyObject List(int size) => new PyObject(instance.Dll.GetFunc<PyList_New>()(size));
            static public PyObject FromPath(string name) => new PyObject(instance.Dll.GetFunc<PyUnicode_DecodeFSDefault>()(name));
            static public PyObject FromString(string name) => new PyObject(instance.Dll.GetFunc<PyUnicode_FromString>()(name));
            static public PyObject FromDouble(double value) => new PyObject(instance.Dll.GetFunc<PyFloat_FromDouble>()(value));
            static public PyObject FromLong(long value) => new PyObject(instance.Dll.GetFunc<PyLong_FromLong>()(value));
            
            public List<string> dir()
            {
                var dirs = new List<string>();

                var pyApi = PythonAPI.instance;
                var _dir = pyApi.Dll.GetFunc<PyObject_Dir>()(handle);
                var size = (long)pyApi.Dll.GetFunc<PyList_Size>()(_dir);
                var getItem = pyApi.Dll.GetFunc<PyList_GetItem>();
                var toUTF8 = pyApi.Dll.GetFunc<PyUnicode_AsUTF8>();
                for(int i = 0; i < size; i++)
                {
                    var item = getItem(_dir,i);
#if UNITY_EDITOR
                    var ptr = toUTF8(item);
                    var len = Unmanaged.Ptr.PtrLength(ptr);
                    dirs.Add(Unmanaged.Ptr.ToStringUTF8(ptr,len));
#else
                    dirs.Add(Marshal.PtrToStringUTF8(toUTF8(item)));
#endif
                }
                return dirs;
            }
            public PyObject GetAttr(string name) => new PyObject(instance.Dll.GetFunc<PyObject_GetAttrString>()(handle, name));
            public PyObject Call(PyObject args)=>new PyObject(instance.Dll.GetFunc<PyObject_CallObject>()(handle,args.handle));

            public PyObject ListGetItem(int index)=> new PyObject(instance.Dll.GetFunc<PyList_GetItem>()(handle, index));
            public void ListSetItem(int index, PyObject value)=> instance.Dll.GetFunc<PyList_SetItem>()(handle, index, value.handle);
            public void ListInsert(int pos, PyObject item) => instance.Dll.GetFunc<PyList_Insert>()(handle, pos, item.handle);
            public void ListAppend(PyObject item) =>instance.Dll.GetFunc<PyList_Append>()(handle, item.handle);
            public long ListSize { get => (long)instance.Dll.GetFunc<PyList_Size>()(handle); }

            public PyObject TupleGetItem(int pos) => new PyObject(instance.Dll.GetFunc<PyTuple_GetItem>()(handle, pos));
            public void TupleSetItem(int pos, PyObject item) => instance.Dll.GetFunc<PyTuple_SetItem>()(handle, pos, item.handle);
            public void TupleResize(int newsize) => instance.Dll.GetFunc<_PyTuple_Resize>()(handle, newsize);
            public long TupleSize { get => (long)instance.Dll.GetFunc<PyTuple_Size>()(handle); }

            public static implicit operator long(PyObject obj)=>instance.Dll.GetFunc<PyLong_AsLong>()(obj.handle);
            public static implicit operator double(PyObject obj)=>instance.Dll.GetFunc<PyFloat_AsDouble>()(obj.handle);
            public static implicit operator string(PyObject obj)
            {
#if UNITY_EDITOR
                var ptr = instance.Dll.GetFunc<PyUnicode_AsUTF8>()(obj.handle);
                var len = Unmanaged.Ptr.PtrLength(ptr);
                return Unmanaged.Ptr.ToStringUTF8(ptr, len);
#else
                return Marshal.PtrToStringUTF8(instance.Dll.GetFunc<PyUnicode_AsUTF8>()(obj.handle));
#endif
            }

#region IDisposable Support
            private bool disposedValue = false; // 重複する呼び出しを検出するには

            void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                        instance.Dll.GetFunc<Py_DECREF>()(handle);
                    }

                    // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                    // TODO: 大きなフィールドを null に設定します。

                    disposedValue = true;
                }
            }

            // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
            // ~PyObject()
            // {
            //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            //   Dispose(false);
            // }

            // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
            public void Dispose()
            {
                // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
                Dispose(true);
                // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
                // GC.SuppressFinalize(this);
            }
#endregion

        }

        public sealed class GIL : IDisposable
        {
            IntPtr handle;

            public GIL() => handle = instance.Dll.GetFunc<PyGILState_Ensure>()();

#region IDisposable Support
            private bool disposedValue = false; // 重複する呼び出しを検出するには

            void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                        instance.Dll.GetFunc<PyGILState_Release>()(handle);
                    }

                    // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                    // TODO: 大きなフィールドを null に設定します。

                    disposedValue = true;
                }
            }

            // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
            // ~GIL()
            // {
            //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            //   Dispose(false);
            // }

            // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
            public void Dispose()
            {
                // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
                Dispose(true);
                // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
                // GC.SuppressFinalize(this);
            }
#endregion

        }

        delegate void Py_InitializeEx(int value);
        delegate void Py_SetPath([MarshalAs(UnmanagedType.LPWStr)]string path);
        delegate void Py_SetPythonHome([MarshalAs(UnmanagedType.LPWStr)] string path);
        delegate void PyGILState_Release(IntPtr handle);
        delegate void Py_DECREF(IntPtr item);
        delegate void Py_INCREF(IntPtr item);
        delegate int PyList_SetItem(IntPtr list, int pos, IntPtr item);
        delegate int PyList_Insert(IntPtr list, int pos, IntPtr item);
        delegate int _PyTuple_Resize(IntPtr tuple, int size);
        delegate int PyList_Append(IntPtr list, IntPtr item);
        delegate int Py_IsInitialized();
        delegate IntPtr PyGILState_Ensure();
        delegate IntPtr PyUnicode_DecodeFSDefault([MarshalAs(UnmanagedType.LPStr)]string str);
        delegate IntPtr PyUnicode_DecodeLocale([MarshalAs(UnmanagedType.LPStr)]string str, [MarshalAs(UnmanagedType.LPStr)]string err);
        delegate IntPtr PyLong_FromLong(long value);
        delegate IntPtr PyFloat_FromDouble(double value);
        delegate IntPtr PyTuple_GetItem(IntPtr tuple, int pos);
        delegate IntPtr PyTuple_Size(IntPtr tuple);
        delegate IntPtr PyTuple_SetItem(IntPtr tuple, int pos, IntPtr item);
        delegate IntPtr PyTuple_New(int size);
        delegate IntPtr PyList_GetItem(IntPtr name, int pos);
        delegate IntPtr PyList_Size(IntPtr list);
        delegate IntPtr PyList_New(int size);
        delegate IntPtr PyObject_CallObject(IntPtr callable, IntPtr args);
        delegate IntPtr PyUnicode_FromString([MarshalAs(UnmanagedType.LPStr)] string name);
        delegate IntPtr PyObject_GetAttrString(IntPtr module,[MarshalAs(UnmanagedType.LPStr)]string name);
        delegate IntPtr PyObject_Dir(IntPtr name);
        delegate IntPtr PyImport_ImportModule([MarshalAs(UnmanagedType.LPStr)]string name);
        delegate IntPtr Py_GetProgramFullPath();
        delegate IntPtr Py_GetPythonHome();
        delegate IntPtr Py_GetVersion();
        delegate IntPtr Py_GetPlatform();
        delegate IntPtr Py_GetCopyright();
        delegate IntPtr Py_GetPath();
        delegate IntPtr Py_GetCompiler();
        delegate IntPtr PyUnicode_AsUTF8(IntPtr name);
        delegate long PyLong_AsLong(IntPtr item);
        delegate double PyFloat_AsDouble(IntPtr item);

#region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~PythonAPI()
        // {
        //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        //   Dispose(false);
        // }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }
#endregion

    }
}
