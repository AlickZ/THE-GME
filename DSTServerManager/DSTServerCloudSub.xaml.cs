﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Renci.SshNet;
using System.Windows.Shapes;
using System.Data;

namespace DSTServerManager
{
    /// <summary>
    /// DSTServerCloudSub.xaml 的交互逻辑
    /// </summary>
    public partial class DSTServerCloudSub : Window
    {
        public delegate void PassValuesHandler(object sender, PassValuesEventArgs e);
        public event PassValuesHandler PassValuesEvent;

        private DataRow m_CurrentRow = null;
        private bool m_NewRow = false;

        /// <summary>
        /// 子窗口构造函数
        /// </summary>
        /// <param name="rowImport"></param>
        /// <param name="newRow"></param>
        /// <param name="newIndex"></param>
        public DSTServerCloudSub(DataRow rowImport, bool newRow, int newIndex)
        {
            InitializeComponent();
            if (newRow) rowImport.ItemArray = new object[4] { newIndex, "127.0.0.1", "anonymous", "" };

            m_CurrentRow = rowImport;
            m_NewRow = newRow;

            if (rowImport[1] != null) textBox_Addr.Text = rowImport[1].ToString();
            if (rowImport[2] != null) textBox_User.Text = rowImport[2].ToString();
            if (rowImport[3] != null) textBox_Pass.Text = rowImport[3].ToString();
        }

        private void button_SubWin_ConnectionCheck_Click(object sender, RoutedEventArgs e)
        {
            label_Logs.Text = string.Empty;
            SftpClient sftpclient = new SftpClient(textBox_Addr.Text, textBox_User.Text, textBox_Pass.Text);
            try
            {
                sftpclient.Connect();
                button_SubWin_ConnectionSaved.IsEnabled = true;

                label_Logs.Text += "连接测试成功!";
            }
            catch (Exception ex)
            {
                button_SubWin_ConnectionSaved.IsEnabled = false;
                label_Logs.Text += ex.ToString();
            }
            finally
            {
                label_Logs.Text += "\r\n";
                sftpclient = null;
            }

            //List<string> saveFolders_Cloud = SavesManager.GetSavesFolder(sftpclient);
            //foreach (var item in saveFolders_Cloud)
            //    comboBox_SavesFolder_Cloud.Items.Add(item);
            //comboBox_SavesFolder_Cloud.SelectedIndex = 0;
        }

        private void button_SubWin_ConnectionSaved_Click(object sender, RoutedEventArgs e)
        {
            if (textBox_Addr.Text != "") m_CurrentRow[1] = textBox_Addr.Text;
            if (textBox_User.Text != "") m_CurrentRow[2] = textBox_User.Text;
            if (textBox_Pass.Text != "") m_CurrentRow[3] = textBox_Pass.Text;

            PassValuesEventArgs args = new PassValuesEventArgs(m_CurrentRow, m_NewRow);

            if (PassValuesEvent == null) Close();
            PassValuesEvent(this, args);
            Close();
        }
    }

    /// <summary>
    /// 容纳参数传递事件的附加信息
    /// </summary>
    public class PassValuesEventArgs : EventArgs
    {
        private readonly DataRow m_DataRow;
        private readonly bool m_NewRow;

        public PassValuesEventArgs(DataRow dataRow, bool newRow)
        {
            m_NewRow = newRow;
            m_DataRow = dataRow;
        }
        public DataRow GetRow { get { return m_DataRow; } }
        public bool IsNewRow { get { return m_NewRow; } }
    }
}