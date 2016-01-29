using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;


namespace DarkNotes
{

    /// <summary>
    /// JavaFinder - Windows, search for all installed versions of java on this system
    /// Author: Shimon Doodkin 
    /// Original Author of java version: petrucio@stackoverflow (828681) http://stackoverflow.com/questions/62289/read-write-to-windows-registry-using-java licensed under a Creative Commons Attribution 3.0 Unported License.
    /// ****************************************************************************
    /// //var found = JavaFinder.findJavas().OrderByDescending(x => x.versionWithBit), new JavaFinder.NaturalStringComparer());
    //      Console.WriteLine(found.Select(x => x.ToString()).Aggregate((acc, item) => acc + "\n" + item));
    //        Console.WriteLine("JavaFinder.findbestjvm() = " + JavaFinder.findbestjvm());
    //        Console.WriteLine("JavaFinder.findbestjava() = " + JavaFinder.findbestjava());

    /// </summary>
    public class JavaFinder
    {

        /// <summary>
        ///  searches in registry and adds to javas a list of paths found under this registry key (rooted at HKEY_LOCAL_MACHINE) </summary>
        /// <param name="wow64">  
        ///               RegistryView.Registry32 to force access to 32-bit registry view,
        ///               or RegistryView.Registry64 to force access to 64-bit registry view </param>
        /// <param name="javas">: result (a list) to where to add the guessed pathes.
        /// ************************************************************************ </param>
        private static void guessPathsFromRegistry(string key, RegistryView wow64, List<findJavas_item> javas)
        {

            try
            {
                using (RegistryKey rk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, wow64).OpenSubKey(key))
                {
                    if (rk == null) return;
                    string[] entries = rk.GetSubKeyNames();
                    //string currentVersion = rk.GetValue("CurrentVersion").ToString();
                    for (int i = 0; entries != null && i < entries.Length; i++)
                    {
                        using (Microsoft.Win32.RegistryKey subkey = rk.OpenSubKey(entries[i]))
                        {
                            string javaHome = (string)subkey.GetValue("JavaHome", "");
                            if (javas.Find(x => x.javahome == javaHome) == null)
                                javas.Add(new findJavas_item(javaHome));

                            string javaHomeJDK = Path.Combine(javaHome, "jre");
                            if (javas.Find(x => x.javahome == javaHomeJDK) == null)
                                javas.Add(new findJavas_item(javaHomeJDK));

                        }
                    }
                }
            }
            catch (Exception t)
            {
                Console.WriteLine(t.ToString());
                Console.Write(t.StackTrace);
            }
        }

        /// <summary>
        ///  holds a single result of a found or a not yet found java </summary>
        public class findJavas_item
        {
            public findJavas_item(string javahome)
            {
                this.javahome = javahome;
            }
            public string javahome;
            public string java;
            public string jvm;
            public string version;
            public string versionWithBit;
            public bool is64bit;
            public bool is32bit;
            /// <returns> Human-readable contents of this JavaInfo instance
            /// *************************************************************************** </returns>
            public override string ToString()
            {
                return this.java + "   Version: " + this.versionWithBit + "   jvm: " + this.jvm;
            }

        }

        /// <summary>
        /// @return: A list of findJavas_item with informations about all javas installed on this machine
        /// Searches and returns results in this order:
        ///   HKEY_LOCAL_MACHINE\SOFTWARE\JavaSoft\Java Runtime Environment (32-bits view)
        ///   HKEY_LOCAL_MACHINE\SOFTWARE\JavaSoft\Java Runtime Environment (64-bits view)
        ///   HKEY_LOCAL_MACHINE\SOFTWARE\JavaSoft\Java Development Kit     (32-bits view)
        ///   HKEY_LOCAL_MACHINE\SOFTWARE\JavaSoft\Java Development Kit     (64-bits view)
        ///   WINDIR\system32
        ///   WINDIR\SysWOW64
        /// ***************************************************************************
        /// it is possible to sort the list by version with linq:
        /// JavaFinder.findJavas().OrderByDescending(x => x.version + " " + (x.is64bit ? "-64bit" : (x.is32bit ? "-32bit" : "-no-bit-info")), new JavaFinder.NaturalStringComparer()).ToArray()
        /// </summary>
        /// 
        public static List<findJavas_item> findJavas()
        {
            List<findJavas_item> javas = new List<findJavas_item>();
            List<string> jvms = new List<string>();
            List<string> javasversion = new List<string>();
            List<bool> javasis64bits = new List<bool>();

            guessPathsFromRegistry("SOFTWARE\\JavaSoft\\Java Runtime Environment", RegistryView.Registry32, javas);
            guessPathsFromRegistry("SOFTWARE\\JavaSoft\\Java Runtime Environment", RegistryView.Registry64, javas);
            guessPathsFromRegistry("SOFTWARE\\JavaSoft\\Java Development Kit", RegistryView.Registry32, javas);
            guessPathsFromRegistry("SOFTWARE\\JavaSoft\\Java Development Kit", RegistryView.Registry64, javas);

            string windir = Environment.GetEnvironmentVariable("WINDIR");
            javas.Add(new findJavas_item(Path.Combine(windir, "system32")));
            javas.Add(new findJavas_item(Path.Combine(windir, "SysWOW64")));

            string sysJavahome = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (javas.Find(x => x.javahome == sysJavahome) == null)
                javas.Add(new findJavas_item(sysJavahome));

            for (int i = javas.Count - 1; i >= 0; i--)
            {

                var ajava = javas[i];

                string javaHome = ajava.javahome;

                string javafile = Path.Combine(javaHome, "Java.exe");
                string jvmfile = Path.Combine(javaHome, "server\\jvm.dll");

                string javafile2 = Path.Combine(javaHome, "bin\\Java.exe");
                string jvmfile2 = Path.Combine(javaHome, "bin\\server\\jvm.dll");


                if (File.Exists(javafile))
                {
                    ajava.java = javafile;
                    if (File.Exists(jvmfile))
                    {
                        ajava.jvm = jvmfile;
                    }
                    else if (File.Exists(jvmfile2))
                    {
                        ajava.jvm = jvmfile2;
                    }
                    else
                        ajava.jvm = "";
                }
                else if (File.Exists(javafile2))
                {
                    ajava.java = javafile2;
                    if (File.Exists(jvmfile2))
                    {
                        ajava.jvm = jvmfile2;
                    }
                    else if (File.Exists(jvmfile))
                    {
                        ajava.jvm = jvmfile;
                    }
                    else
                        ajava.jvm = "";
                }
                else
                {
                    javas.RemoveAt(i);
                    continue;
                }


                string file = ajava.java;
                string versionInfo = RunExternalExe(file, "-version");
                string[] tokens = versionInfo.Split('"');


                ajava.is32bit = versionInfo.ToUpper().Contains("32-BIT");
                ajava.is64bit = versionInfo.ToUpper().Contains("64-BIT");

                if (tokens.Length < 2)
                {
                    ajava.version = versionInfo;
                    ajava.versionWithBit = versionInfo;
                }
                else
                {
                    ajava.version = tokens[1];
                    ajava.versionWithBit = ajava.version + (ajava.is64bit ? "-64bit" : (ajava.is32bit ? "-32bit" : "-0-no-bit-info"));
                }

            }

            return javas;
        }


        // usage
        //string output = RunExternalExe("example.exe");

        public static string RunExternalExe(string filename, string arguments = null)
        {
            var process = new Process();

            process.StartInfo.FileName = filename;
            if (!string.IsNullOrEmpty(arguments))
            {
                process.StartInfo.Arguments = arguments;
            }

            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.UseShellExecute = false;

            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;

            var stdOutput = new StringBuilder();

            //process.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
            //process.ErrorDataReceived += (s, e) => Console.WriteLine(e.Data);

            process.ErrorDataReceived += (sender, args) => stdOutput.Append(args.Data);
            process.OutputDataReceived += (sender, args) => stdOutput.Append(args.Data);

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }
            catch (Exception e)
            {
                throw new Exception("OS error while executing " + ("'" + filename + ((string.IsNullOrEmpty(arguments)) ? string.Empty : " " + arguments) + "'") + ": " + e.Message, e);
            }

            return stdOutput.ToString();
            /*
            if (process.ExitCode == 0)
            {
               
            }
            else
            {
                var message = new StringBuilder();

                if (!string.IsNullOrEmpty(stdError))
                {
                    message.AppendLine(stdError);
                }

                if (stdOutput.Length != 0)
                {
                    message.AppendLine("Std output:");
                    message.AppendLine(stdOutput.ToString());
                }

                throw new Exception(("'" + filename + ((string.IsNullOrEmpty(arguments)) ? string.Empty : " " + arguments) + "'") + " finished with exit code = " + process.ExitCode + ": " + message);
            }
            */
        }



        /// <summary>
        /// @return: The path to a java.exe that has the same bitness as the OS
        /// (or null if no matching java is found)
        /// ***************************************************************************
        /// </summary>
        public static string findbestjava()
        {

            var found = findJavas().OrderByDescending(x => x.versionWithBit, new NaturalStringComparer()).ToArray();
            if (found.Length > 0)
            {
                return found[0].java;
            }

            throw new Exception(" oracle java not found. please install java");
            //return null;
        }

        public static string findbestjvm()
        {
            var found = findJavas().FindAll(x => x.jvm != "" && Environment.Is64BitProcess ? x.is64bit : x.is32bit).OrderByDescending(x => x.version, new NaturalStringComparer()).ToArray();
            if (found.Length > 0)
            {
                return found[0].jvm;
            }
            throw new Exception(" oracle java " + (Environment.Is64BitProcess ? "64 bit" : "32 bit") + " not found. please install java " + (Environment.Is64BitProcess ? "64 bit" : "32 bit"));
            //return null;
        }

        /// <summary>
        /// sort text by distinguishing between text and numbers
        /// ***************************************************************************
        /// new[] {"a4","a3","a2","a10","b5","b4","b400","1","C1d","c1d2"}.OrderBy(x => x, new NaturalStringComparer()).Dump();
        /// Results:
        /// 1
        /// a2
        /// a3
        /// a4
        /// a10
        /// b4
        /// b5
        /// b400
        /// C1d
        /// c1d2
        /// ***************************************************************************
        /// from http://stackoverflow.com/a/11624488/466363
        /// </summary>

        public class NaturalStringComparer : IComparer<string>
        {
            private static readonly Regex _re = new Regex(@"(?<=\D)(?=\d)|(?<=\d)(?=\D)", RegexOptions.Compiled);

            public int Compare(string x, string y)
            {
                x = x.ToLower();
                y = y.ToLower();
                if (string.Compare(x, 0, y, 0, Math.Min(x.Length, y.Length)) == 0)
                {
                    if (x.Length == y.Length) return 0;
                    return x.Length < y.Length ? -1 : 1;
                }
                var a = _re.Split(x);
                var b = _re.Split(y);
                int i = 0;
                while (true)
                {
                    int r = PartCompare(a[i], b[i]);
                    if (r != 0) return r;
                    ++i;
                }
            }

            private static int PartCompare(string x, string y)
            {
                int a, b;
                if (int.TryParse(x, out a) && int.TryParse(y, out b))
                    return a.CompareTo(b);
                return x.CompareTo(y);
            }
        }
    }

}
