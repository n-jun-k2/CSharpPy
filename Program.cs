using System;
using System.Diagnostics;

namespace CsharpPy
{
    class Program
    {
        static void Main(string[] args)
        {
            var anaconda_home = Environment.GetEnvironmentVariable("ANACONDA_HOME");

            Unmanaged.WinAPI.SetDllDirectory(anaconda_home);

            var pyApi = PythonAPI.instance;

            Console.WriteLine(
                $" {pyApi.GetCopyright()} \n" +
                $" {pyApi.GetVersion()} \n" +
                $"Home: {pyApi.GetPythonHome()} \n" +
                $"Path : {pyApi.GetPath()} \n" +
                $"ProgramPath : {pyApi.GetProgramFullPath()} \n" + 
                $"Platform : {pyApi.GetPlatform()}");

            var sep = ";";
            var path = "";
            path += anaconda_home + sep;
            path += anaconda_home + "\\python37.zip" + sep; 
            path += anaconda_home + "\\DLLs" + sep;
            path += anaconda_home + "\\lib" + sep;
            path += anaconda_home + "\\lib\\site-packages" + sep;
            path += anaconda_home + "\\lib\\site-packages\\win32" + sep;
            path += anaconda_home + "\\lib\\site-packages\\win32\\lib" + sep;
            path += anaconda_home + "\\lib\\site-packages\\Pythonwin" + sep;
            path += Environment.CurrentDirectory + sep;

            pyApi.SetPath(path);
            pyApi.SetPythonHome(anaconda_home);

            Console.WriteLine(
                $"Home: {pyApi.GetPythonHome()} \n" +
                $"Path : {pyApi.GetPath()} \n" +
                $"ProgramPath : {pyApi.GetProgramFullPath()} \n");

            Console.WriteLine($"Is Initlized {pyApi.IsInitlized}");

            pyApi.Initilize(1);

            Console.WriteLine($"Is Initlized {pyApi.IsInitlized}");

            var Qt5 = PythonAPI.PyObject.Import("PyQt5.QtWidgets");
            {
                var QApplication = Qt5.GetAttr("QApplication");
                {
                    var SetLibraryPath = QApplication.GetAttr("addLibraryPath");
                    var setArgs = PythonAPI.PyObject.Tuple(1);
                    setArgs.TupleSetItem(0, PythonAPI.PyObject.FromString(anaconda_home));

                    SetLibraryPath.Call(setArgs);
                }
                {
                    var SetLibraryPath = QApplication.GetAttr("addLibraryPath");
                    var setArgs = PythonAPI.PyObject.Tuple(1);
                    setArgs.TupleSetItem(0, PythonAPI.PyObject.FromString(anaconda_home + "\\Library\\plugins"));

                    SetLibraryPath.Call(setArgs);
                }
            }

            //実行したいスクリプト
            var importSample = PythonAPI.PyObject.Import("sample");
            {
                //引数なし、戻り値なしの関数呼び出し。
                var func = importSample.GetAttr("func");
                var pyFuncArgs = PythonAPI.PyObject.Tuple(0);
                func.Call(pyFuncArgs);
            }
            {
                //引数あり、戻り値ありの関数の呼び出し。
                var func2 = importSample.GetAttr("func2");
                var pyFuncArgs = PythonAPI.PyObject.Tuple(1);
                pyFuncArgs.TupleSetItem(0, PythonAPI.PyObject.FromDouble(1.3));
                var callReturn = func2.Call(pyFuncArgs);
                Console.WriteLine($"func2 return {(double)callReturn}");
            }
            {
                //クラスの生成
                var typeClass = importSample.GetAttr("SampleClass");
                var initArgs = PythonAPI.PyObject.Tuple(0);
                var instance = typeClass.Call(initArgs);

            }
            {
                //継承したクラスの生成
                var typeClass = importSample.GetAttr("SampleClass2");
                var initArgs = PythonAPI.PyObject.Tuple(0);
                var instance = typeClass.Call(initArgs);
            }

            var importMLP = PythonAPI.PyObject.Import("mlp");
            {
                var ministFunc = importMLP.GetAttr("ministFunc");
                var pyFuncArgs = PythonAPI.PyObject.Tuple(0);
                ministFunc.Call(pyFuncArgs);
            }
        }
    }
}
