using System;
using System.Collections.Generic;
using System.IO;

namespace VortexSecOps
{
    namespace FileTraversalService
    {
        public interface IFileTraverser
        {
            /// <summary>
            /// 遍历该目录中的所有文件
            /// </summary>
            /// <param name="filepath">要遍历的目录</param>
            /// <returns>文件的完整路径</returns>
            IEnumerable<string> TraverseFile(string filepath);
            /// <summary>
            /// 遍历该目录中所有指定扩展名的文件
            /// </summary>
            /// <param name="filepath">要遍历的目录</param>
            /// <param name="searchString">搜索字符串，如"*.txt"</param>
            /// <returns>文件的完整路径</returns>
            IEnumerable<string> TraverseFile(string filepath, string searchString);
        }

        // 实现文件遍历接口的类
        public class FileTraverser : IFileTraverser
        {
            // 私有构造，防止外部实例化
            private FileTraverser() { }

            // 创建 FileTraverser 实例
            public static IFileTraverser Create()
            {
                return new FileTraverser();
            }

            // 遍历指定目录下所有文件
            IEnumerable<string> IFileTraverser.TraverseFile(string filepath)
            {
                // 存储待遍历目录
                Stack<string> pendingPaths = new Stack<string>();
                pendingPaths.Push(filepath);

                while (pendingPaths.Count > 0)
                {
                    string currentPath = pendingPaths.Pop();
                    string[] matchingFiles = null;
                    string[] subDirectories = null;

                    try
                    {
                        // 获取当前目录下所有文件
                        matchingFiles = Directory.GetFiles(currentPath);
                    }
                    catch (Exception) { }

                    try
                    {
                        // 获取当前目录下所有子目录
                        subDirectories = Directory.GetDirectories(currentPath);
                    }
                    catch (Exception) { }

                    if (matchingFiles != null)
                    {
                        // 返回文件路径
                        foreach (string file in matchingFiles)
                        {
                            yield return file;
                        }
                    }

                    if (subDirectories != null)
                    {
                        // 压入子目录
                        foreach (string subDirectoryPath in subDirectories)
                        {
                            pendingPaths.Push(subDirectoryPath);
                        }
                    }
                }
            }

            // 遍历指定目录下指定扩展名文件
            IEnumerable<string> IFileTraverser.TraverseFile(string filepath, string searchString)
            {
                // 存储待遍历目录
                var pendingPaths = new Stack<string>();
                pendingPaths.Push(filepath);

                while (pendingPaths.Count > 0)
                {
                    string currentPath = pendingPaths.Pop();
                    string[] matchingFiles = null;
                    string[] subDirectories = null;

                    try
                    {
                        // 获取匹配文件
                        matchingFiles = Directory.GetFiles(currentPath, searchString);
                    }
                    catch (Exception) { }

                    try
                    {
                        // 获取子目录
                        subDirectories = Directory.GetDirectories(currentPath);
                    }
                    catch (Exception) { }

                    if (matchingFiles != null)
                    {
                        // 返回匹配文件路径
                        foreach (string file in matchingFiles)
                        {
                            yield return file;
                        }
                    }

                    if (subDirectories != null)
                    {
                        // 压入子目录
                        foreach (string subDirectoryPath in subDirectories)
                        {
                            pendingPaths.Push(subDirectoryPath);
                        }
                    }
                }
            }
        }
    }
}