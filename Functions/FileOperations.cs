// 
// This file is a part of MSM (Multi Server Manager)
// Copyright (C) 2016-2021 Michiel Hazelhof (michiel@hazelhof.nl)
// 
// MSM is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// MSM is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using MSM.Data;
using MSM.Extends;

namespace MSM.Functions
{
    public static class FileOperations
    {
        private static String _runningDirectory;
        public static String GetRunningDirectory()
        {
            if (_runningDirectory != null) return _runningDirectory;
            try
            {
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            }
            catch (Exception)
            {
                Environment.Exit(-1);
            }
            _runningDirectory = Path.GetDirectoryName(Application.ExecutablePath);
            // Just to be safe from slow updates.....
            return Path.GetDirectoryName(Application.ExecutablePath);
        }
        public static String GetCurrentExecutable(Boolean removeVisualStudioHost = true, Boolean fileOnly = false)
        {
            String result = Process.GetCurrentProcess().MainModule.FileName;
            if (removeVisualStudioHost)
            {
                result = result.Replace(".vshost.", ".");
            }
            if (fileOnly)
            {
                result = Path.GetFileName(result);
            }
            return result;
        }

        public static Boolean DeleteFile(String fullDirectoryPathAndFileName)
        {
            if (!File.Exists(fullDirectoryPathAndFileName)) return true;
            try
            {
                File.SetAttributes(fullDirectoryPathAndFileName, FileAttributes.Normal);
                File.Delete(fullDirectoryPathAndFileName);

                Int32 attempt = 0;
                retry:
                if (!File.Exists(fullDirectoryPathAndFileName)) return true;
                if (attempt >= 2) return false;
                attempt++;
                Thread.Sleep(25);
                goto retry;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static List<String> DeleteDirectory(String fullDirectoryPath, Boolean recursive)
        {
            List<String> failed = new();
            if (!Directory.Exists(fullDirectoryPath)) return failed;
            try
            {
                foreach (String file in Directory.GetFiles(fullDirectoryPath))
                {
                    File.Delete(file);
                }

                foreach (String directory in Directory.GetDirectories(fullDirectoryPath))
                {
                    DeleteDirectory(directory, recursive);
                }

                Directory.Delete(fullDirectoryPath, recursive);
                Int32 attempt = 0;
                retry:
                if (!Directory.Exists(fullDirectoryPath)) return new List<String>();
                if (attempt < 2)
                {
                    attempt++;
                    Thread.Sleep(25);
                    goto retry;
                }
                failed.Add(fullDirectoryPath);

                return failed;
            }
            catch (Exception)
            {
                failed.Add(fullDirectoryPath);
            }
            return failed;
        }
        public static Boolean MoveFile(String fullDirectoryPathAndFileName, String fullDirectoryPathAndFileNameNew)
        {
            try
            {
                if (File.Exists(fullDirectoryPathAndFileNameNew) && File.Exists(fullDirectoryPathAndFileName))
                {
                    if (!DeleteFile(fullDirectoryPathAndFileNameNew))
                    {
                        return false;
                    }
                }
                if (File.Exists(fullDirectoryPathAndFileName))
                {
                    FileInfo origin = new(fullDirectoryPathAndFileName);
                    DateTime creationTime = origin.CreationTime, lastWriteTime = origin.LastWriteTime, lastAccessTime = origin.LastAccessTime;
                    FileAttributes attributes = File.GetAttributes(fullDirectoryPathAndFileName);

                    File.Move(fullDirectoryPathAndFileName, fullDirectoryPathAndFileNameNew);

                    FileInfo destination = new(fullDirectoryPathAndFileNameNew);
                    if (destination.IsReadOnly)
                    {
                        destination.IsReadOnly = false;
                        destination.CreationTime = creationTime;
                        destination.LastWriteTime = lastWriteTime;
                        destination.LastAccessTime = lastAccessTime;
                        destination.IsReadOnly = true;
                    }
                    else
                    {
                        destination.CreationTime = creationTime;
                        destination.LastWriteTime = lastWriteTime;
                        destination.LastAccessTime = lastAccessTime;
                    }

                    File.SetAttributes(fullDirectoryPathAndFileNameNew, attributes);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static Boolean CopyFile(String fullDirectoryPathAndFileName, String fullDirectoryPathAndFileNameNew, Boolean overWrite)
        {
            if (String.Equals(fullDirectoryPathAndFileName, fullDirectoryPathAndFileNameNew, StringComparison.OrdinalIgnoreCase)) return true;

            try
            {
                if (File.Exists(fullDirectoryPathAndFileNameNew))
                {
                    FileAttributes attributes = File.GetAttributes(fullDirectoryPathAndFileNameNew);
                    if (attributes.HasFlag(FileAttributes.Directory))
                    {
                        FileInfo fileInfo = new(fullDirectoryPathAndFileName);
                        fullDirectoryPathAndFileNameNew += @"\" + fileInfo.Name;
                    }
                    else
                    {
                        if (!DeleteFile(fullDirectoryPathAndFileNameNew))
                        {
                            return false;
                        }
                    }
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch {}

            try
            {
                if (!File.Exists(fullDirectoryPathAndFileName))
                {
                    return false;
                }

                String fullPath = Path.GetDirectoryName(fullDirectoryPathAndFileNameNew);
                if (fullPath != null)
                {
                    if (!Directory.Exists(fullPath))
                    {
                        CreateDirectory(fullPath);
                    }
                }

                if (File.Exists(fullDirectoryPathAndFileName))
                {
                    File.Copy(fullDirectoryPathAndFileName, fullDirectoryPathAndFileNameNew, overWrite);

                    File.SetCreationTime(fullDirectoryPathAndFileNameNew, File.GetCreationTime(fullDirectoryPathAndFileName));
                    File.SetLastAccessTime(fullDirectoryPathAndFileNameNew, File.GetLastAccessTime(fullDirectoryPathAndFileName));
                    File.SetLastWriteTime(fullDirectoryPathAndFileNameNew, File.GetLastWriteTime(fullDirectoryPathAndFileName));
                    File.SetAttributes(fullDirectoryPathAndFileName, File.GetAttributes(fullDirectoryPathAndFileName));
                }
                return File.Exists(fullDirectoryPathAndFileNameNew);
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static Boolean CreateFile(String fullDirectoryPathAndFileName)
        {
            if (Path.GetDirectoryName(fullDirectoryPathAndFileName) == null) return false;

            try
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                if (!Directory.Exists(Path.GetDirectoryName(fullDirectoryPathAndFileName)))
                {
                    if (!CreateDirectory(Path.GetDirectoryName(fullDirectoryPathAndFileName)))
                    {
                        return false;
                    }
                }

                using FileStreamOptimized testFile = new(fullDirectoryPathAndFileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
                testFile.Flush(true);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static Boolean CreateDirectory(String directory)
        {
            DirectoryInfo dirInfo = new(directory);
            try
            {
                if (dirInfo.Parent != null && !dirInfo.Exists)
                {
                    CreateDirectory(dirInfo.Parent.FullName);
                }
                if (dirInfo.Exists) return true;
                dirInfo.Create();

                DirectorySecurity directorySecurity = dirInfo.GetAccessControl();
                directorySecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, AccessControlType.Allow));
                dirInfo.SetAccessControl(directorySecurity);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        public static Boolean Unblock(String fileName)
        {
            return NativeMethods.DeleteFile(fileName + ":Zone.Identifier");
        }
        public static Boolean IsFileLocked(String file)
        {
            if (!File.Exists(file))
            {
                return false;
            }

            FileStream stream = null;

            try
            {
                FileInfo testFileInfo = new(file);
                stream = testFileInfo.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                stream.Close();
                stream.Dispose();
            }
            catch (Exception)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
            return false;
        }

        [DebuggerStepThrough, Pure]
        public static Boolean Contains(this String[] input, String toFind, StringComparison comparison = StringComparison.Ordinal)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (Int32 i = 0; i < input.Length; i++)
            {
                if (String.Equals(input[i], toFind, comparison))
                {
                    return true;
                }
            }
            return false;
        }
        public static List<String> ReturnFiles(String localPath, Boolean recurseDirectories, String[] excludedExtensions = null, String[] excludedFiles = null, String[] includedExtensions = null, String[] includedFiles = null, Boolean includeBasePath = true, String[] excludedDirectories = null, Boolean regex = false, Boolean includePath = true)
        {
            if (!Directory.Exists(localPath)) return new List<String>();

            if (!localPath.EndsWith(@"\"))
            {
                localPath += @"\";
            }

            List<String> list = new();
            DirectoryInfo directoryInfo = new(localPath);

            IEnumerable<FileInfo> result = directoryInfo.EnumerateFiles("*", recurseDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            if (excludedDirectories != null)
            {
                for (Int32 i = 0; i < excludedDirectories.Length; i++)
                {
                    excludedDirectories[i] = excludedDirectories[i].TrimEnd('\\');
                }

                result = result.Where(x => !excludedDirectories.Any(excludedDirectory => String.Equals(x.DirectoryName, excludedDirectory, StringComparison.OrdinalIgnoreCase)));

                String[] full = new String[excludedDirectories.Length];
                for (Int32 i = 0; i < excludedDirectories.Length; i++)
                {
                    full[i] = excludedDirectories[i] + @"\";
                }

                result = result.Where(x => !full.Any(excludedDirectory => x.DirectoryName != null && x.DirectoryName.StartsWith(excludedDirectory, StringComparison.OrdinalIgnoreCase)));
            }

            foreach (FileInfo file in result)
            {
                if (excludedExtensions != null && excludedExtensions.Contains(file.Extension, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }
                if (excludedFiles != null && (excludedFiles.Contains(file.Name, StringComparer.OrdinalIgnoreCase) || (regex && excludedFiles.Any(excludedFile => Regex.IsMatch(file.Name, excludedFile, RegexOptions.IgnoreCase)))))
                {
                    continue;
                }

                if ((includedFiles == null && includedExtensions == null) || (includedFiles != null && includedFiles.Contains(file.Name, StringComparison.OrdinalIgnoreCase)) || (includedExtensions != null && includedExtensions.Contains(file.Extension, StringComparer.OrdinalIgnoreCase)) || (regex && includedFiles != null && includedFiles.Any(includedFile => Regex.IsMatch(file.Name, includedFile, RegexOptions.IgnoreCase))))
                {
                    if (!includePath)
                    {
                        list.Add(file.Name);
                    }
                    else
                    {
                        list.Add(includeBasePath ? file.FullName : file.FullName.Replace(localPath, "", StringComparison.OrdinalIgnoreCase));
                    }
                }
            }
            return list;
        }
    }
}
