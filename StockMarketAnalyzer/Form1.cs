using Newtonsoft.Json;
using OfficeOpenXml;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace StockMarketAnalyzer
{
    public partial class Form1 : Form
    {
        public bool checkedTriggerActive = false;
        private bool mouseDown;
        private Point lastLocation;
        private List<string> originalItems;
        int lineNumber;
        public Form1()
        {
            InitializeComponent();
        }

        private void label2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Form1.CheckForIllegalCrossThreadCalls = false;
            pictureBox1.ImageLocation = Application.StartupPath + "/loading.gif";
            string path = Application.StartupPath + "/config.xml";
            string readedXML = System.IO.File.ReadAllText(path);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(readedXML);
            XmlNodeList shareList = xmlDoc.SelectNodes("config/shares/share");
            XmlNodeList usedShareList = xmlDoc.SelectNodes("config/usedShares/share");
            foreach (XmlNode share in shareList)
            {
                shareListbox.Items.Add(share.InnerText);
            }
            foreach (XmlNode share in usedShareList)
            {
                shareListbox.SetItemChecked(shareListbox.Items.IndexOf(share.InnerText), true);

            }

            originalItems = shareListbox.Items.Cast<string>().ToList();
            checkedTriggerActive = true;
        }

        private void bunifuTextbox1_MouseEnter(object sender, EventArgs e)
        {
            bunifuTextbox1.text = "";
        }

        private void shareListbox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (checkedTriggerActive == true)
            {
            if (shareListbox.GetItemCheckState(shareListbox.SelectedIndex) == CheckState.Unchecked)
            {
                    string path = Application.StartupPath + "/config.xml";
                    string readedXML = System.IO.File.ReadAllText(path);
                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(path);
                    XmlNode nodes = xmlDoc.SelectSingleNode("config/usedShares");
                    var newElement = xmlDoc.CreateElement("share");
                    newElement.InnerText = shareListbox.SelectedItem.ToString();
                    bool flag = false;
                    foreach (XmlNode item in nodes)
                    {
                        if (item.InnerText == shareListbox.SelectedItem.ToString())
                        {
                            flag = true;
                        }
                    }
                    if (flag == false)
                    {
                        nodes.AppendChild(newElement);
                    }           
                    xmlDoc.Save(path);

                }
                else
            {
                    string path = Application.StartupPath + "/config.xml";
                    string readedXML = System.IO.File.ReadAllText(path);
                    var xmlDoc = new XmlDocument();
                    Console.WriteLine(shareListbox.SelectedItem.ToString());
                    xmlDoc.Load(path);
                    XmlNodeList usedShareList = xmlDoc.SelectNodes("config/usedShares/share");
                    foreach (XmlNode node in usedShareList)
                    {
                        Console.WriteLine(node.InnerText);
                        if (node.InnerText == shareListbox.SelectedItem.ToString())
                        {
                            node.ParentNode.RemoveChild(node);
                        }
                    }
                    xmlDoc.Save(path);
                }
                originalItems = shareListbox.Items.Cast<string>().ToList();
            }
        }

        private void bunifuGradientPanel1_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
            lastLocation = e.Location;
        }

        private void bunifuGradientPanel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                this.Location = new Point(
                    (this.Location.X - lastLocation.X) + e.X, (this.Location.Y - lastLocation.Y) + e.Y);

                this.Update();
            }
        }

        private void bunifuGradientPanel1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        public void clearAllChecks()
        {
            for (int i = 0; i < shareListbox.Items.Count; i++)
            {
                shareListbox.SetItemChecked(i, false);
            }
        }

        private void bunifuTextbox1_KeyDown(object sender, EventArgs e)
        {
            checkedTriggerActive = false;
            string path = Application.StartupPath + "/config.xml";
            string readedXML = System.IO.File.ReadAllText(path);
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            XmlNodeList shareList = xmlDoc.SelectNodes("config/shares/share");
            XmlNodeList usedShareList = xmlDoc.SelectNodes("config/usedShares/share");

            List<string> dataSource = new List<string>();
           
            foreach (XmlNode share in shareList)
            {
                dataSource.Add(share.InnerText);
            }
            var filtered = dataSource.Cast<string>().ToList().Where(param => param.ToUpper().Contains(bunifuTextbox1.text.ToUpper())).ToList();
            shareListbox.DataSource = null;
            shareListbox.DataSource = filtered;

            foreach (XmlNode node in usedShareList)
            {
                try
                {
                    shareListbox.SetItemChecked(shareListbox.Items.IndexOf(node.InnerText), true);
                }
                catch (Exception)
                {

                }
            }


            if (String.IsNullOrWhiteSpace(bunifuTextbox1.text))
            {
                shareListbox.DataSource = originalItems;
            }


            checkedTriggerActive = true;
        }

        

        private void startButton_Click(object sender, EventArgs e)
        {
            pictureBox1.Visible = true;
            startButton.Visible = false;
            checkedTriggerActive = false;
            char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            string path = Application.StartupPath + "/config.xml";
            string readedXML = System.IO.File.ReadAllText(path);
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            XmlNodeList shareList = xmlDoc.SelectNodes("config/shares/share");
            XmlNodeList usedShareList = xmlDoc.SelectNodes("config/usedShares/share");

            List<string> dataSource = new List<string>();
            foreach (XmlNode share in usedShareList)
            {
                dataSource.Add(share.InnerText);
            }
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            var file = new FileInfo(Application.StartupPath + "/table.xlsx");
            using (var package = new ExcelPackage(file))
            {
                var sheet = package.Workbook.Worksheets["shares"];
                int counter = 1;
                foreach (XmlNode share in usedShareList)
                {
                    if (dataSource.Contains(sheet.Cells[alphabet[counter] + "1"].Value) == false)
                    {
                        sheet.Cells[alphabet[counter] + "1"].Value = share.InnerText;
                    }
                   
                    counter++;
                }
                lineNumber = 2;
                try
                {
                    while (sheet.Cells["A" + Convert.ToString(lineNumber)].Value != null)
                    {
                        lineNumber++;
                    }
                }
                catch (Exception)
                {

                }

                sheet.Cells["A" + lineNumber].Value = DateTime.Now.ToString("dd/MM/yyyy");
                try
                {
                    package.Save();
                }
                catch (Exception err)
                {
                    MessageBox.Show("Excel arkaplanda açık olduğundan dolayı işlem tamamlanamadı !");
                    pictureBox1.Visible = false;
                    startButton.Visible = true;
                }
                
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void bunifuTextbox1_OnTextChange(object sender, EventArgs e)
        {
            if (bunifuTextbox1.text.IndexOf(Environment.NewLine) > -1)
            {
                bunifuTextbox1.text = "";
                string path = Application.StartupPath + "/config.xml";
                string readedXML = System.IO.File.ReadAllText(path);
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(path);
                XmlNodeList shareList = xmlDoc.SelectNodes("config/shares/share");
                XmlNodeList usedShareList = xmlDoc.SelectNodes("config/usedShares/share");
                List<string> dataSource = new List<string>();
                foreach (XmlNode share in shareList)
                {
                    dataSource.Add(share.InnerText);
                }
                shareListbox.DataSource = null;
                shareListbox.DataSource = dataSource;

                foreach (XmlNode node in usedShareList)
                {
                    try
                    {
                        shareListbox.SetItemChecked(shareListbox.Items.IndexOf(node.InnerText), true);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            string path = Application.StartupPath + "/config.xml";
            string readedXML = System.IO.File.ReadAllText(path);
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            XmlNodeList shareList = xmlDoc.SelectNodes("config/shares/share");
            XmlNodeList usedShareList = xmlDoc.SelectNodes("config/usedShares/share");
            var file = new FileInfo(Application.StartupPath + "/table.xlsx");
            using (var package = new ExcelPackage(file))
            {
                var sheet = package.Workbook.Worksheets["shares"];
                int counter = 1;
                try
                {
                    while (sheet.Cells[alphabet[counter] + "1"].Value != null)
                    {
                        var url = "https://scanner.tradingview.com/turkey/scan";
                        string myJson = "{ \"symbols\":{ \"tickers\":[\"BIST:" + sheet.Cells[alphabet[counter] + "1"].Value  + "\"],\"query\":{ \"types\":[]} },\"columns\":[\"Pivot.M.Classic.Middle\"]}";

                        var client = new RestClient("https://scanner.tradingview.com");
                        var request = new RestRequest("turkey/scan");
                        request.AddJsonBody(myJson);
                        var response = client.Post(request);
                        var content = response.Content;
                        dynamic m = JsonConvert.DeserializeObject<dynamic>(content);
                        if (m.data.Count> 0)
                        {
                            sheet.Cells[alphabet[counter] + lineNumber.ToString()].Value = m.data[0].d[0].ToString("#.##");
                        }else
                        {
                            sheet.Cells[alphabet[counter] + lineNumber.ToString()].Value = "Veri yok";
                        }
                       
                        counter++;
                    }
                }
                catch (Exception err)
                {
                    Console.WriteLine("{0} Hata.", err);
                }


                try
                {
                    package.Save();
                }
                catch (Exception err)
                {
                    MessageBox.Show("Excel arkaplanda açık olduğundan dolayı işlem tamamlanamadı !");
                    pictureBox1.Visible = false;
                    startButton.Visible = true;
                }
            }
            pictureBox1.Visible = false;
            startButton.Text = "Veriler getirildi";
            startButton.Visible = true;
            countdown.Enabled = true;
        }
    }
}
