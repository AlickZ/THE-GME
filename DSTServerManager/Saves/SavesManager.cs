﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Renci.SshNet;
using System.Threading.Tasks;

namespace DSTServerManager.Saves
{
    /// <summary>
    /// 游戏存档信息
    /// 远程Linux服务器需要传入Sftp连接
    /// </summary>
    static class SavesManager
    {
        //默认路径
        static string m_DefaultPath_Local = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Klei";
        static string m_DefaultPath_Cloud = @"/root/.klei";

        //集群默认配置文件名
        static string m_ClusterIniName = "cluster.ini";
        static string m_ServerIniName = "server.ini";

        /// <summary>
        /// 获取存档文件夹名称
        /// </summary>
        /// <returns></returns>
        static internal List<string> GetSavesFolder()
        {
            return GetFolder(m_DefaultPath_Local);
        }
        static internal List<string> GetSavesFolder(SftpClient client)
        {
            List<string> folder = new List<string>();
            foreach (var item in client.ListDirectory(m_DefaultPath_Cloud))
                if (item.Name != "." && item.Name != "..") folder.Add(item.Name);
            return folder;
        }

        /// <summary>
        /// 获取服务器集群文件夹名列表
        /// </summary>
        /// <param name="saveName"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        static internal List<string> GetClusterFolder(string saveName, string keyword)
        {
            List<string> cluster = new List<string>();
            foreach (var item in GetFolder($"{m_DefaultPath_Local}\\{saveName}"))
                if (item.Contains(keyword)) cluster.Add(item);
            return cluster;
        }
        static internal List<string> GetClusterFolder(string saveName, string keyword, SftpClient client)
        {
            List<string> cluster = new List<string>();
            foreach (var item in client.ListDirectory($"{m_DefaultPath_Cloud}/{saveName}"))
                if (item.Name.Contains(keyword)) cluster.Add(item.Name);
            return cluster;
        }

        /// <summary>
        /// 获取服务器集群信息列表
        /// </summary>
        /// <param name="saveName">存档文件夹名称</param>
        /// <param name="keyword"></param>
        /// <param name="serverTableColumns">服务器信息Table的列数</param>
        /// <returns></returns>
        static internal List<ClusterInfo> GetClusterInfo(string saveName, string keyword)
        {
            List<ClusterInfo> clusterList = new List<ClusterInfo>();
            List<string> clusterFolder = GetClusterFolder(saveName, keyword);

            foreach (var clusterName in clusterFolder)
            {
                ClusterInfo cluster = new ClusterInfo();
                cluster.ClusterFolder = clusterName;
                cluster.ClusterSetting.ReadFromFile($"{m_DefaultPath_Local}\\{saveName}\\{clusterName}\\{m_ClusterIniName}");
                cluster.ClusterServers = GetServerInfo(saveName, clusterName);
                clusterList.Add(cluster);
            }
            return clusterList;
        }
        static internal List<ClusterInfo> GetClusterInfo(string saveName, string keyword, SftpClient client)
        {
            List<ClusterInfo> clusterList = new List<ClusterInfo>();
            List<string> clusterFolder = GetClusterFolder(saveName, keyword, client);

            foreach (var clusterName in clusterFolder)
            {
                ClusterInfo cluster = new ClusterInfo();
                cluster.ClusterFolder = clusterName;
                cluster.ClusterSetting.ReadFromSSH($"{m_DefaultPath_Cloud}/{saveName}/{clusterName}/{m_ClusterIniName}", client);
                cluster.ClusterServers = GetServerInfo(saveName, clusterName, client);
                clusterList.Add(cluster);
            }
            return clusterList;
        }

        /// <summary>
        /// 保存集群信息
        /// </summary>
        /// <param name="saveName"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        static internal bool SetClusterInfo(string saveName, ClusterInfo current)
        {
            current.ClusterSetting.WriteToFile($"{m_DefaultPath_Local}\\{saveName}\\{current.ClusterFolder}\\{m_ClusterIniName}");
            return true;
        }
        static internal bool SetClusterInfo(string saveName, ClusterInfo current, SftpClient client)
        {
            current.ClusterSetting.WriteToSSH($"{m_DefaultPath_Cloud}/{saveName}/{current.ClusterFolder}/{m_ClusterIniName}",client);
            return true;
        }

        /// <summary>
        /// 获取集群下服务器文件夹名列表
        /// </summary>
        /// <param name="saveName"></param>
        /// <param name="clusterName"></param>
        /// <returns></returns>
        static internal List<string> GetServerFolder(string saveName, string clusterName)
        {
            List<string> server = new List<string>();
            foreach (var item in GetFolder($"{m_DefaultPath_Local}\\{saveName}\\{clusterName}"))
                server.Add(item);
            return server;
        }
        static internal List<string> GetServerFolder(string saveName, string clusterName, SftpClient client)
        {
            List<string> server = new List<string>();
            foreach (var item in client.ListDirectory($"{m_DefaultPath_Cloud}/{saveName}/{clusterName}"))
                if (item.Name != "." && item.Name != ".." && item.IsDirectory) server.Add(item.Name);
            return server;
        }

        /// <summary>
        /// 获取服务器信息列表
        /// </summary>
        /// <param name="saveName"></param>
        /// <param name="clusterName"></param>
        /// <returns></returns>
        static internal List<ServerInfo> GetServerInfo(string saveName, string clusterName)
        {
            List<ServerInfo> serverList = new List<ServerInfo>();
            List<string> serverFolder = GetServerFolder(saveName, clusterName);

            foreach (var serverName in serverFolder)
            {
                ServerInfo server = new ServerInfo();
                server.Folder = serverName;
                server.Setting.ReadFromFile($"{m_DefaultPath_Local}\\{saveName}\\{clusterName}\\{serverName}\\{m_ServerIniName}");
                if (GetFolder($"{m_DefaultPath_Local}\\{saveName}\\{clusterName}\\{serverName}\\save\\session").Count > 0)
                    server.Session = GetFolder($"{m_DefaultPath_Local}\\{saveName}\\{clusterName}\\{serverName}\\save\\session")[0];
                server.Level.ReadFromFile($"{m_DefaultPath_Local}\\{saveName}\\{clusterName}\\{serverName}\\leveldataoverride.lua");

                serverList.Add(server);
            }
            return serverList;
        }
        static internal List<ServerInfo> GetServerInfo(string saveName, string clusterName, SftpClient client)
        {
            List<ServerInfo> serverList = new List<ServerInfo>();
            List<string> serverFolder = GetServerFolder(saveName, clusterName, client);

            foreach (var serverName in serverFolder)
            {
                ServerInfo server = new ServerInfo();
                server.Folder = serverName;
                server.Setting.ReadFromSSH($"{m_DefaultPath_Cloud}/{saveName}/{clusterName}/{serverName}/{m_ServerIniName}", client);
                if (GetFolder($"{m_DefaultPath_Cloud}/{saveName}/{clusterName}/{serverName}/save/session").Count > 0)
                    server.Session = GetFolder($"{m_DefaultPath_Cloud}/{saveName}/{clusterName}/{serverName}/save/session")[0];
                server.Level.ReadFromFile($"{m_DefaultPath_Cloud}/{saveName}/{clusterName}/{serverName}/leveldataoverride.lua");

                serverList.Add(server);
            }
            return serverList;
        }

        /// <summary>
        /// 保存服务器信息
        /// </summary>
        /// <param name="saveName"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        static internal bool SetServerInfo(string saveName, string clusterName, ServerInfo current)
        {
            current.Setting.WriteToFile($"{m_DefaultPath_Local}\\{saveName}\\{clusterName}\\{current.Folder}\\{m_ServerIniName}");
            return true;
        }

        /// <summary>
        /// 获取子目录
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns></returns>
        static private List<string> GetFolder(string path)
        {
            List<string> folderList = new List<string>();
            if (!Directory.Exists(path))
                return folderList;

            DirectoryInfo directory = new DirectoryInfo(path);
            DirectoryInfo[] dirs = directory.GetDirectories();
            for (int i = 0; i < dirs.Length; i++)
            {
                if (!folderList.Contains(dirs[i].Name))
                    folderList.Add(dirs[i].Name);
            }
            return folderList;
        }
    }
}