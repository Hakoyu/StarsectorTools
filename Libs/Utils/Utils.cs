﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using Aspose.Zip;
using Aspose.Zip.Rar;
using Aspose.Zip.SevenZip;
using Microsoft.VisualBasic.FileIO;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using StarsectorTools.Windows;
using I18n = StarsectorTools.Langs.Libs.Utils_I18n;

namespace StarsectorTools.Libs.Utils
{
    /// <summary>通用方法</summary>
    public class Utils
    {
        /// <summary>
        /// 检测文件是否存在
        /// </summary>
        /// <param name="file">文件路径</param>
        /// <param name="outputLog">输出日志</param>
        /// <returns>存在为<see langword="true"/>,不存在为<see langword="false"/></returns>
        public static bool FileExists(string file, bool outputLog = true)
        {
            bool isExists = File.Exists(file);
            if (!isExists && outputLog)
                STLog.WriteLine($"{I18n.FileNotFound} {I18n.Path}: {file}", STLogLevel.WARN);
            return isExists;
        }

        /// <summary>
        /// 检测文件夹是否存在
        /// </summary>
        /// <param name="directory">目录路径</param>
        /// <param name="outputLog">输出日志</param>
        /// <returns>存在为<see langword="true"/>,不存在为<see langword="false"/></returns>
        public static bool DirectoryExists(string directory, bool outputLog = true)
        {
            bool exists = Directory.Exists(directory);
            if (!exists && outputLog)
                STLog.WriteLine($"{I18n.DirectoryNotFound} {I18n.Path}: {directory}", STLogLevel.WARN);
            return exists;
        }

        /// <summary>
        /// 格式化Json数据,去除掉注释以及不合规的逗号
        /// </summary>
        /// <param name="jsonData">Json数据</param>
        /// <returns>格式化后的数据</returns>
        public static string JsonParse(string jsonData)
        {
            // 清除json中的注释
            jsonData = Regex.Replace(jsonData, @"(#|//)[\S ]*", "");
            // 清除json中不符合规定的逗号
            jsonData = Regex.Replace(jsonData, @",(?=[\r\n \t]*[\]\}])|(?<=[\}\]]),[ \t]*\r?\Z", "");
            return jsonData;
        }

        /// <summary>
        /// 复制文件夹至目标文件夹
        /// </summary>
        /// <param name="sourceDirectory">原始路径</param>
        /// <param name="destinationDirectory">目标路径</param>
        /// <returns>复制成功为<see langword="true"/>,失败为<see langword="false"/></returns>
        public static bool CopyDirectory(string sourceDirectory, string destinationDirectory)
        {
            try
            {
                FileSystem.CopyDirectory(sourceDirectory, $"{destinationDirectory}\\{Path.GetFileName(sourceDirectory)}", UIOption.OnlyErrorDialogs);
                return true;
            }
            catch (Exception ex)
            {
                STLog.WriteLine(I18n.LoadError, ex);
                return false;
            }
        }

        /// <summary>
        /// 删除文件至回收站
        /// </summary>
        /// <param name="file"></param>
        /// <returns>删除成功为<see langword="true"/>,失败为<see langword="false"/></returns>
        public static bool DeleteFileToRecycleBin(string file)
        {
            try
            {
                FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                return true;
            }
            catch (Exception ex)
            {
                STLog.WriteLine(I18n.LoadError, ex);
                return false;
            }
        }

        /// <summary>
        /// 删除文件夹至回收站
        /// </summary>
        /// <param name="directory">文件夹</param>
        /// <returns>删除成功为<see langword="true"/>,失败为<see langword="false"/></returns>
        public static bool DeleteDirToRecycleBin(string directory)
        {
            try
            {
                FileSystem.DeleteDirectory(directory, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                return true;
            }
            catch (Exception ex)
            {
                STLog.WriteLine(I18n.LoadError, ex);
                return false;
            }
        }

        /// <summary>
        /// 检测颜色是否为亮色调
        /// </summary>
        /// <param name="color">颜色</param>
        /// <returns>是为<see langword="true"/>,不是为<see langword="false"/></returns>
        public static bool IsLightColor(Color color)
        {
            return (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255 > 0.5;
        }

        /// <summary>
        /// 使用系统默认打开方式打开链接,文件或文件夹
        /// </summary>
        /// <param name="link">链接</param>
        /// <returns>打开成功为<see langword="true"/>,失败为<see langword="false"/></returns>
        public static bool OpenLink(string link)
        {
            try
            {
                Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
                return true;
            }
            catch (Exception ex)
            {
                STLog.WriteLine(I18n.LinkError, ex);
                return false;
            }
        }

        /// <summary>
        /// <para>压缩文件夹至Zip文件并输出到目录</para>
        /// <para>若不输入压缩文件名,则以原始目录的文件夹名称来命名</para>
        /// </summary>
        /// <param name="sourceDirectory">原始目录</param>
        /// <param name="destinationDirectory">输出目录</param>
        /// <param name="archiveName">压缩文件名</param>
        /// <returns>压缩成功为<see langword="true"/>,失败为<see langword="false"/></returns>
        public static bool ArchiveDirToDir(string sourceDirectory, string destinationDirectory, string? archiveName = null)
        {
            if (!DirectoryExists(sourceDirectory))
                return false;
            try
            {
                using (var archive = ZipArchive.Create())
                {
                    archive.AddAllFromDirectory(sourceDirectory);
                    if (archiveName is null)
                        archive.SaveTo($"{destinationDirectory}\\{Path.GetFileName(sourceDirectory)}.zip", CompressionType.Deflate);
                    else
                        archive.SaveTo($"{destinationDirectory}\\{archiveName}.zip", CompressionType.Deflate);
                }
                return true;
            }
            catch (Exception ex)
            {
                STLog.WriteLine($"{I18n.ZipFileError} {I18n.Path}: {sourceDirectory}", ex);
                return false;
            }
        }

        /// <summary>
        /// <para>解压压缩文件至目录</para>
        /// <para>支持: <see langword="Zip"/> <see langword="Rar"/> <see langword="7z"/></para>
        /// </summary>
        /// <param name="sourceFile">原始文件</param>
        /// <param name="destinationDirectory">输出目录</param>
        /// <returns>解压成功为<see langword="true"/>,失败为<see langword="false"/></returns>
        public static bool UnArchiveFileToDir(string sourceFile, string destinationDirectory)
        {
            if (!FileExists(sourceFile))
                return false;
            //读取压缩文件头,以判断压缩文件类型
            using StreamReader sr = new(sourceFile);
            string head = $"{sr.Read()}{sr.Read()}";
            sr.Close();
            if (!DirectoryExists(destinationDirectory, false))
                Directory.CreateDirectory(destinationDirectory);
            try
            {
                if (head == "8075")//Zip文件
                {
                    using (var archive = new Archive(sourceFile, new() { Encoding = Encoding.UTF8 }))
                    {
                        archive.ExtractToDirectory(destinationDirectory);
                    }
                }
                else if (head == "8297")//Rar文件
                {
                    using (var archive = new RarArchive(sourceFile))
                    {
                        archive.ExtractToDirectory(destinationDirectory);
                    }
                }
                else if (head == "55122")//7z文件
                {
                    using (var archive = new SevenZipArchive(sourceFile))
                    {
                        archive.ExtractToDirectory(destinationDirectory);
                    }
                }
                else
                    throw new();
            }
            catch (Exception ex)
            {
                STLog.WriteLine($"{I18n.ZipFileError}  {I18n.Path}: {sourceFile}", ex);
                if (DirectoryExists(destinationDirectory, false))
                    Directory.Delete(destinationDirectory);
                return false;
            }
            return true;
        }

        private static Panuon.WPF.UI.MessageBoxIcon GetMessageBoxIcon(STMessageBoxIcon icon) =>
            icon switch
            {
                STMessageBoxIcon.None => Panuon.WPF.UI.MessageBoxIcon.None,
                STMessageBoxIcon.Info => Panuon.WPF.UI.MessageBoxIcon.Info,
                STMessageBoxIcon.Warning => Panuon.WPF.UI.MessageBoxIcon.Warning,
                STMessageBoxIcon.Error => Panuon.WPF.UI.MessageBoxIcon.Error,
                STMessageBoxIcon.Success => Panuon.WPF.UI.MessageBoxIcon.Success,
                STMessageBoxIcon.Question => Panuon.WPF.UI.MessageBoxIcon.Question,
                _ => Panuon.WPF.UI.MessageBoxIcon.None
            };

        /// <summary>
        /// 弹出消息窗口
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="icon">显示的图标</param>
        /// <returns>按钮结果: <see cref="MessageBoxResult"/></returns>
        public static MessageBoxResult ShowMessageBox(string message,
                                                      STMessageBoxIcon icon = STMessageBoxIcon.Info,
                                                      bool setBlurEffect = true)
        {
            return ShowMessageBox(message, " ", icon: icon, setBlurEffect: setBlurEffect);
        }

        /// <summary>
        /// 弹出消息窗口
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="button">显示的按钮</param>
        /// <param name="icon">显示的图标</param>
        /// <returns>按钮结果: <see cref="MessageBoxResult"/></returns>
        public static MessageBoxResult ShowMessageBox(string message,
                                                      MessageBoxButton button,
                                                      STMessageBoxIcon icon,
                                                      bool setBlurEffect = true)
        {
            return ShowMessageBox(message, " ", button, icon, setBlurEffect: setBlurEffect);
        }

        /// <summary>
        /// 弹出消息窗口
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="caption">标头</param>
        /// <param name="button">显示的按钮</param>
        /// <param name="image">显示的图片</param>
        /// <param name="result">默认按钮结果</param>
        /// <param name="options">窗口设置</param>
        /// <returns>按钮结果: <see cref="MessageBoxResult"/></returns>
        public static MessageBoxResult ShowMessageBox(string message,
                                                      string caption,
                                                      MessageBoxButton button = MessageBoxButton.OK,
                                                      STMessageBoxIcon icon = STMessageBoxIcon.Info,
                                                      bool setBlurEffect = true)
        {
            if (setBlurEffect)
            {
                SetMainWindowBlurEffect();
                var outResult = Panuon.WPF.UI.MessageBoxX.Show(message, caption, button, GetMessageBoxIcon(icon));
                //var outResult = MessageBox.Show(message, caption, button, icon, result);
                RemoveMainWindowBlurEffect();
                return outResult;
            }
            else
            {
                return Panuon.WPF.UI.MessageBoxX.Show(message, caption, button, GetMessageBoxIcon(icon));
            }
        }

        /// <summary>
        /// 为主窗口设置模糊效果,用于聚焦弹窗
        /// </summary>
        public static void SetMainWindowBlurEffect() => ((MainWindow)Application.Current.MainWindow).SetBlurEffect();

        /// <summary>
        /// 取消主窗口的模糊效果
        /// </summary>
        public static void RemoveMainWindowBlurEffect() => ((MainWindow)Application.Current.MainWindow).RemoveBlurEffect();
    }

    /// <summary>弹窗图案</summary>
    public enum STMessageBoxIcon
    {
        /// <summary>无</summary>
        None,

        /// <summary>信息</summary>
        Info,

        /// <summary>警告</summary>
        Warning,

        /// <summary>错误</summary>
        Error,

        /// <summary>成功</summary>
        Success,

        /// <summary>问题</summary>
        Question
    }
}