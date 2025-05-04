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
            /// ������Ŀ¼�е������ļ�
            /// </summary>
            /// <param name="filepath">Ҫ������Ŀ¼</param>
            /// <returns>�ļ�������·��</returns>
            IEnumerable<string> TraverseFile(string filepath);
            /// <summary>
            /// ������Ŀ¼������ָ����չ�����ļ�
            /// </summary>
            /// <param name="filepath">Ҫ������Ŀ¼</param>
            /// <param name="searchString">�����ַ�������"*.txt"</param>
            /// <returns>�ļ�������·��</returns>
            IEnumerable<string> TraverseFile(string filepath, string searchString);
        }

        // ʵ���ļ������ӿڵ���
        public class FileTraverser : IFileTraverser
        {
            // ˽�й��죬��ֹ�ⲿʵ����
            private FileTraverser() { }

            // ���� FileTraverser ʵ��
            public static IFileTraverser Create()
            {
                return new FileTraverser();
            }

            // ����ָ��Ŀ¼�������ļ�
            IEnumerable<string> IFileTraverser.TraverseFile(string filepath)
            {
                // �洢������Ŀ¼
                Stack<string> pendingPaths = new Stack<string>();
                pendingPaths.Push(filepath);

                while (pendingPaths.Count > 0)
                {
                    string currentPath = pendingPaths.Pop();
                    string[] matchingFiles = null;
                    string[] subDirectories = null;

                    try
                    {
                        // ��ȡ��ǰĿ¼�������ļ�
                        matchingFiles = Directory.GetFiles(currentPath);
                    }
                    catch (Exception) { }

                    try
                    {
                        // ��ȡ��ǰĿ¼��������Ŀ¼
                        subDirectories = Directory.GetDirectories(currentPath);
                    }
                    catch (Exception) { }

                    if (matchingFiles != null)
                    {
                        // �����ļ�·��
                        foreach (string file in matchingFiles)
                        {
                            yield return file;
                        }
                    }

                    if (subDirectories != null)
                    {
                        // ѹ����Ŀ¼
                        foreach (string subDirectoryPath in subDirectories)
                        {
                            pendingPaths.Push(subDirectoryPath);
                        }
                    }
                }
            }

            // ����ָ��Ŀ¼��ָ����չ���ļ�
            IEnumerable<string> IFileTraverser.TraverseFile(string filepath, string searchString)
            {
                // �洢������Ŀ¼
                var pendingPaths = new Stack<string>();
                pendingPaths.Push(filepath);

                while (pendingPaths.Count > 0)
                {
                    string currentPath = pendingPaths.Pop();
                    string[] matchingFiles = null;
                    string[] subDirectories = null;

                    try
                    {
                        // ��ȡƥ���ļ�
                        matchingFiles = Directory.GetFiles(currentPath, searchString);
                    }
                    catch (Exception) { }

                    try
                    {
                        // ��ȡ��Ŀ¼
                        subDirectories = Directory.GetDirectories(currentPath);
                    }
                    catch (Exception) { }

                    if (matchingFiles != null)
                    {
                        // ����ƥ���ļ�·��
                        foreach (string file in matchingFiles)
                        {
                            yield return file;
                        }
                    }

                    if (subDirectories != null)
                    {
                        // ѹ����Ŀ¼
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