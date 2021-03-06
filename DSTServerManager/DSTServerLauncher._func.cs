﻿using DSTServerManager.DataHelper;
using DSTServerManager.Saves;
using DSTServerManager.Servers;
using Renci.SshNet;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DSTServerManager
{
    public partial class DSTServerLauncher : Window
    {
        #region [远程Connect]----------------------------------------------------------------------------------------------------

        /// <summary>
        /// 创建新的远程连接及相应页面
        /// </summary>
        /// <param name="location"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        internal void CreatNewConnect(string location, string username, string password)
        {
            RefreshList();
            TabItem connectTab = System.Windows.Markup.XamlReader.Parse(m_TabItemXaml) as TabItem;
            connectTab.MouseDoubleClick += ConnectTab_MouseDoubleClick;
            TabControl_ServerLog.Items.Add(connectTab);

            ServerConnect serverConnect = new ServerConnect(location, username, password);
            serverConnect.CreatTabWindow(TabControl_ServerLog, connectTab);

            //在后台线程开始执行
            BackgroundWorker connectWorker = new BackgroundWorker();
            connectWorker.DoWork += ConnectWorker_DoWork;
            connectWorker.RunWorkerCompleted += ConnectWorker_RunWorkerCompleted;
            connectWorker.RunWorkerAsync(new object[] { serverConnect });
        }

        private void ConnectWorker_DoWork(object sender, DoWorkEventArgs passValue)
        {
            object[] argument = (object[])passValue.Argument;

            ServerConnect serverConnect = argument[0] as ServerConnect;
            SftpClient client = serverConnect.GetSftpClient;

            serverConnect.StartConnect();
            m_ServerConnect.Add(serverConnect);

            if (SavesManager.GetSavesFolder(client).Count == 0) SavesManager.CreatSavesFolder(client);
            UI.SaveFolders_Cloud = SavesManager.GetSavesFolder(client);

            passValue.Result = new object[] { serverConnect };
        }
        private void ConnectWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs passValue)
        {
            object[] argument = (object[])passValue.Result;

            ServerConnect serverConnect = argument[0] as ServerConnect;

            ComboBox_CloudServer_SavesFolder.SelectedIndex = 0;

            int indexConn = dataGrid_CloudServer_Connections.SelectedIndex;
            //控制DataGrid的颜色
            DataGridRow dataRow = (DataGridRow)dataGrid_CloudServer_Connections.ItemContainerGenerator.ContainerFromIndex(indexConn);
            if (serverConnect.AllConnected) dataRow.Background = new SolidColorBrush(Colors.LightGreen);
            else dataRow.Background = new SolidColorBrush(Colors.OrangeRed);
        }

        private void ConnectTab_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            foreach (var connect in m_ServerConnect)
            {
                if (!connect.ServerTab.Equals(sender as TabItem)) continue;

                textBox_Servers_Tab_Log.Text = sender.ToString();
                TabControl_ServerLog.Items.Remove(sender as TabItem);
            }
        }

        #endregion ----------------------------------------------------------------------------------------------------

        #region [本地Process]----------------------------------------------------------------------------------------------------

        /// <summary>
        /// 创建本地Process
        /// </summary>
        /// <param name="serverExe"></param>
        /// <param name="parameter"></param>
        /// <param name="session"></param>
        internal void CreatNewProcess(string serverExe, string parameter, bool isShell, string session)
        {
            if (!File.Exists(serverExe)) return;

            RefreshList();
            TabItem processTab = System.Windows.Markup.XamlReader.Parse(m_TabItemXaml) as TabItem;
            processTab.MouseDoubleClick += ProcessTab_MouseDoubleClick;
            TabControl_ServerLog.Items.Add(processTab);

            ServerProcess process = new ServerProcess(isShell, session);
            process.CreatTabWindow(TabControl_ServerLog, processTab);

            //在后台线程开始执行
            BackgroundWorker processWorker = new BackgroundWorker();
            processWorker.DoWork += ProcessWorker_DoWork;
            processWorker.RunWorkerCompleted += ProcessWorker_RunWorkerCompleted;
            processWorker.RunWorkerAsync(new object[] { process, serverExe, parameter });
        }

        private void ProcessTab_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            foreach (var servers in m_ServerProcess)
            {
                if (!servers.ServerTab.Equals(sender as TabItem)) continue;

                textBox_Servers_Tab_Log.Text = sender.ToString();
                servers.SendCommand("c_shutdown()");
            }
        }

        private void ProcessWorker_DoWork(object sender, DoWorkEventArgs passValue)
        {
            object[] argument = (object[])passValue.Argument;

            ServerProcess process = argument[0] as ServerProcess;
            string serverExe = argument[1] as string;
            string parameter = argument[2] as string;

            process.StartProcess(serverExe, parameter);
            m_ServerProcess.Add(process);
        }

        private void ProcessWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        #endregion ----------------------------------------------------------------------------------------------------

        #region [远程Screens]----------------------------------------------------------------------------------------------------

        /// <summary>
        /// 创建远程Screens
        /// </summary>
        /// <param name="serverExe"></param>
        /// <param name="parameter"></param>
        /// <param name="session"></param>
        internal void CreatNewScreens(string location, string username, string password, string command)
        {
            RefreshList();
            TabItem screensTab = System.Windows.Markup.XamlReader.Parse(m_TabItemXaml) as TabItem;
            screensTab.MouseDoubleClick += ScreensTab_MouseDoubleClick;
            TabControl_ServerLog.Items.Add(screensTab);

            ServerScreens serverScreens = new ServerScreens(location, username, password);
            serverScreens.CreatTabWindow(TabControl_ServerLog, screensTab);

            //在后台线程开始执行
            BackgroundWorker screensWorker = new BackgroundWorker();
            screensWorker.DoWork += ScreensWorker_DoWork;
            screensWorker.RunWorkerCompleted += ScreensWorker_RunWorkerCompleted;
            screensWorker.RunWorkerAsync(new object[] { serverScreens, command });
        }

        private void ScreensWorker_DoWork(object sender, DoWorkEventArgs passValue)
        {
            object[] argument = (object[])passValue.Argument;

            ServerScreens serverScreens = argument[0] as ServerScreens;
            string command = argument[1] as string;
            SftpClient client = serverScreens.GetSftpClient;

            serverScreens.StartScreens();
            m_ServerScreens.Add(serverScreens);

            serverScreens.SendCommand(command);
        }

        private void ScreensWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void ScreensTab_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            foreach (var screens in m_ServerScreens)
            {
                if (!screens.ServerTab.Equals(sender as TabItem)) continue;

                textBox_Servers_Tab_Log.Text = sender.ToString();
                TabControl_ServerLog.Items.Remove(sender as TabItem);
            }
        }

        #endregion ----------------------------------------------------------------------------------------------------

        /// <summary>
        /// 发送控制命令
        /// </summary>
        /// <param name="command"></param>
        internal void SendCommand(string command)
        {
            foreach (var servers in m_ServerProcess)
            {
                if (!servers.ServerTab.Equals(TabControl_ServerLog.SelectedItem)) continue;

                servers.SendCommand(command);
            }

            foreach (var connect in m_ServerConnect)
            {
                if (!connect.AllConnected) continue;
                if (!connect.ServerTab.Equals(TabControl_ServerLog.SelectedItem)) continue;

                connect.SendCommand(command);
            }

            foreach (var screens in m_ServerScreens)
            {
                if (!screens.ServerTab.Equals(TabControl_ServerLog.SelectedItem)) continue;

                screens.SendCommand(command);
            }
        }

        /// <summary>
        /// 状态列表检查
        /// </summary>
        internal void RefreshList()
        {
            for (int i = 0; i < m_ServerProcess.Count; i++)
            {
                if (m_ServerProcess[i].IsProcessActive == true) continue;

                m_ServerProcess[i] = null;
                m_ServerProcess.Remove(m_ServerProcess[i]);
            }
        }

        private void TabControl_ServerLog_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var screens in m_ServerScreens)
            {
                if (!screens.ServerTab.Equals(TabControl_ServerLog.SelectedItem))
                {
                    screens.IsLogOutput = false;
                    continue;
                }

                screens.IsLogOutput = true;
                screens.DisplayData();
            }
        }

        /// <summary>
        /// 更新指定存档路径下的集群服务器列表
        /// </summary>
        /// <param name="keyword">筛选关键词</param>
        /// <param name="saveFolder">存档文件夹名</param>
        /// <param name="clusterList">Listbox控件</param>
        /// <param name="client">远程服务器连接</param>
        private void RefreshClusterData(string saveFolder, string keyword, ref ListBox clusterList, SftpClient client)
        {
            //获取集群文件夹名称并更新显示
            ObservableCollection<string> clusterFolder = null;
            if (client == null) clusterFolder = SavesManager.GetClusterFolder(saveFolder, keyword);
            else clusterFolder = SavesManager.GetClusterFolder(saveFolder, keyword, client);

            clusterList.Items.Clear();
            foreach (var item in clusterFolder)
                if (!clusterList.Items.Contains(item)) clusterList.Items.Add(item);
        }

        /// <summary>
        /// 更新集群下所有服务器信息
        /// </summary>
        /// <param name="clusterInfo">当前选定的集群信息</param>
        /// <param name="bindData">界面绑定的集群</param>
        private void RefreshServersData(ClusterInfo clusterInfo, ref UserInterfaceData bindData)
        {
            //将当前选定的集群信息赋值给界面绑定的类实例
            ExtendHelper.CopyAllProperties(clusterInfo.ClusterSetting, bindData);

            //更新当前选定的集群服务器DataGrid信息
            bindData.ClusterServersTable.Clear();
            ExtendHelper.CopyAllProperties(clusterInfo, bindData);
        }
    }
}
