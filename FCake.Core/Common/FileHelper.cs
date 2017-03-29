using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace FCake.Core.Common
{
    public class FileHelper
    {
        public const string FOLDER = "TempFiles";

        const uint ERR_PROCESS_CANNOT_ACCESS_FILE = 0x80070020;

        public static string BasePath { get { return AppDomain.CurrentDomain.BaseDirectory + FOLDER; } }

        public static long GetFileLength(string fileName)
        {
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(fileName);
            return fileInfo.Length;
        }

        public static byte[] ReadToEnd(Stream stream)
        {
            long originalPosition = stream.Position;
            stream.Position = 0;

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                stream.Position = originalPosition;
            }
        }



        public static void ReadStreamToString(System.IO.Stream input, out string str, Encoding encode)
        {
            input.Position = 0;
            System.IO.StreamReader readStream = new System.IO.StreamReader(input, encode);
            str = readStream.ReadToEnd();
        }

        public static string ReadFileToString(string filName, Encoding encode)
        {
            if (!File.Exists(filName))
                return string.Empty;

            FileStream fs = null;
            string str;

            int times = 0;
            while (true)
            {
                try
                {
                    fs = new FileStream(filName, FileMode.Open, FileAccess.Read);
                    ReadStreamToString(fs, out str, encode);
                    fs.Close();
                    return str;
                }
                catch (IOException e)
                {
                    uint rst = (uint)System.Runtime.InteropServices.Marshal.GetHRForException(e);
                    if (rst == ERR_PROCESS_CANNOT_ACCESS_FILE)
                    {
                        if (times > 10)
                        {
                            ////Maybe another program has some trouble with file
                            ////We must exit now
                            throw e;
                        }

                        System.Threading.Thread.Sleep(200);
                        times++;
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
        }

        public static MemoryStream ReadFileToStream(string filName)
        {
            byte[] bytes = new byte[32768];
            int read = 0;
            int offset = 0;
            FileStream fs = null;

            int times = 0;
            while (true)
            {
                try
                {
                    MemoryStream mem = new MemoryStream();
                    fs = new FileStream(filName, FileMode.Open, FileAccess.Read);
                    mem.Position = 0;

                    while ((read = fs.Read(bytes, 0, bytes.Length)) > 0)
                    {
                        offset += read;
                        mem.Write(bytes, 0, read);
                    }

                    fs.Close();
                    return mem;
                }
                catch (IOException e)
                {
                    uint rst = (uint)System.Runtime.InteropServices.Marshal.GetHRForException(e);
                    if (rst == ERR_PROCESS_CANNOT_ACCESS_FILE)
                    {
                        if (times > 10)
                        {
                            throw e;
                        }

                        System.Threading.Thread.Sleep(200);
                        times++;
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
        }

        public static void WriteStream(string fileName, MemoryStream inputStream)
        {
            FileStream fs = null;

            int times = 0;
            while (true)
            {
                try
                {
                    if (System.IO.File.Exists(fileName))
                    {
                        System.IO.File.Delete(fileName);
                    }

                    fs = new FileStream(fileName, FileMode.CreateNew);
                    inputStream.WriteTo(fs);
                    fs.Close();
                    return;
                }
                catch (IOException e)
                {
                    uint rst = (uint)System.Runtime.InteropServices.Marshal.GetHRForException(e);
                    if (rst == ERR_PROCESS_CANNOT_ACCESS_FILE)
                    {
                        if (times > 10)
                        {
                            throw e;
                        }

                        System.Threading.Thread.Sleep(200);
                        times++;
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
        }

        public static void WriteLine(string fileName, string str)
        {
            int times = 0;

            while (true)
            {
                try
                {
                    using (FileStream fs = new FileStream(fileName, FileMode.Append))
                    {
                        using (StreamWriter w = new StreamWriter(fs, Encoding.UTF8))
                        {
                            w.WriteLine(str);
                        }
                    }

                    return;
                }
                catch (IOException e)
                {
                    uint rst = (uint)System.Runtime.InteropServices.Marshal.GetHRForException(e);
                    if (rst == ERR_PROCESS_CANNOT_ACCESS_FILE)
                    {
                        if (times > 10)
                        {
                            throw e;
                        }

                        System.Threading.Thread.Sleep(200);
                        times++;
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
        }

        public static void WriteString(string filName, string str, Encoding encode)
        {
            TextWriter writer = null;
            FileStream fs = null;

            int times = 0;
            while (true)
            {
                try
                {
                    if (System.IO.File.Exists(filName))
                    {
                        System.IO.File.Delete(filName);
                    }

                    fs = new FileStream(filName, FileMode.CreateNew);
                    writer = new StreamWriter(fs, encode);
                    writer.Write(str);
                    writer.Close();
                    fs.Close();
                    return;
                }
                catch (IOException e)
                {
                    uint rst = (uint)System.Runtime.InteropServices.Marshal.GetHRForException(e);
                    if (rst == ERR_PROCESS_CANNOT_ACCESS_FILE)
                    {
                        if (times > 10)
                        {
                            ////Maybe another program has some trouble with file
                            ////We must exit now
                            throw e;
                        }

                        System.Threading.Thread.Sleep(200);
                        times++;
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
        }

        public static void SaveLog(string message)
        {
            SaveLog(message, null);
        }

        public static void SaveLog(string message, string fileName)
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FOLDER);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            string file = string.Concat(dir, DateTime.Now.ToString("yyyy-MM-dd"), ".log");
            if (!string.IsNullOrEmpty(fileName))
                file = dir + fileName;
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    StringBuilder line = new StringBuilder();
                    line.AppendFormat("LogTime:{0}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    line.AppendFormat("Process:{0}\r\n", System.Diagnostics.Process.GetCurrentProcess().ProcessName);
                    line.AppendFormat("Message:{0}\r\n", message);
                    WriteLine(file, line.ToString());
                }
                else
                {
                    WriteLine(file, message);
                }
            }
            catch
            {
            }
        }

        public static void DeleteFile(string path, string fileName, bool recursive)
        {
            if (path[path.Length - 1] != '\\')
            {
                path += '\\';
            }

            if (!recursive)
            {
                System.IO.File.Delete(path + fileName);
            }
            else
            {
                System.IO.File.Delete(path + fileName);

                string[] subFolders = System.IO.Directory.GetDirectories(path);

                foreach (string folder in subFolders)
                {
                    DeleteFile(folder, fileName, recursive);
                }
            }
        }
    }
}
