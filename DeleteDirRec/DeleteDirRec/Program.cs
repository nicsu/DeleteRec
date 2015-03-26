using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace DeleteDirRec {
    class Program {
        static void Main(string[] args) {
            DeleteFilesAndDirs(@"\\?\" + args[0]);
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct WIN32_FIND_DATA {
            public uint dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr FindFirstFile(string lpFileName, out 
                                WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern bool FindNextFile(IntPtr hFindFile, out 
                                WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FindClose(IntPtr hFindFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteFile(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RemoveDirectory(string lpDirectoryName);

        private static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        private const int FILE_ATTRIBUTE_DIRECTORY = 0x10;


        // Assume dirName passed in is already prefixed with \\?\
        [SuppressMessage("ReSharper", "InvertIf")]
        public static void DeleteFilesAndDirs(string dirName) {
            WIN32_FIND_DATA findData;
            var findHandle = FindFirstFile(dirName + @"\*", out findData);
            var success = false;

            if (findHandle != INVALID_HANDLE_VALUE) {
                do {
                    string currentFileName = findData.cFileName;
                    // if this is a directory, recurse
                    if (((int)findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) != 0) {

                        if (currentFileName != "." && currentFileName != "..") {
                            DeleteFilesAndDirs(dirName + @"\" + currentFileName);
                        }
                    }
                    // it's a file, delete it
                    else {
                        string file = dirName + @"\" + currentFileName;
                        success = DeleteFile(file);
                        if (!success) {
                            Console.WriteLine("Cannot delete file {0}", file);
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                        }
                    }
                }
                while (FindNextFile(findHandle, out findData));
            }

            // close the find handle
            FindClose(findHandle);

            // the dir is empty so we can remove it
            success = RemoveDirectory(dirName);
            if (!success) {
                Console.WriteLine("Cannot remove dir {0}", dirName);
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
    }
}
