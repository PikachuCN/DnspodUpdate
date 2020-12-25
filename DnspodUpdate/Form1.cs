using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Serialization.Json;
namespace DnspodUpdate
{
    public partial class Form1 : Form
    {
        Config config = new Config();
        string AesKey = "";

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            InputBoxResult txt = InputBox.Show("请输入要添加的域名");

            if (txt.ReturnCode == DialogResult.OK && txt.Text != string.Empty)
            {
                if (txt.Text.Split('.').Length == 3)
                {
                    listBox1.Items.Add(txt.Text);
                    config.DOMAIN.Add(txt.Text);
                    SaveConfig();
                }
                else
                {
                    MessageBox.Show("请输入正确的域名\n需要包含完整地址\n不需要协议(如http)");
                }
            }


        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ReadConfig();
            textBox1.Text = config.Token;
            textBox2.Text = config.TokenId;
            textBox3.Text = config.Speed;
            foreach (var item in config.DOMAIN)
            {
                listBox1.Items.Add(item);
            }
            Action action = () =>
            {
                int speed = 0;
                while (true)
                {

                    this.BeginInvoke(new Action(() =>
                    {
                        toolStripStatusLabel1.Text = "现在时间:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        toolStripStatusLabel2.Text = "IP地址:" + config.Ip + " 区域:" + config.Addr;
                    }));

                    try
                    {
                        speed++;
                        if (speed == int.Parse(this.textBox3.Text))
                        {
                            GetChangeIp();
                            SaveConfig();
                            speed = 0;
                        }


                    }
                    catch
                    {


                    }
                    System.Threading.Thread.Sleep(1000);
                }
            };
            action.BeginInvoke(null, null);
            GetChangeIp();

        }


        private void GetChangeIp()
        {
            EasyHttpClient ehc = new EasyHttpClient();
            var re = ehc.Get("https://service-reqw3pvy-1252879367.gz.apigw.tencentcs.com/release/ipAddr");
            DataContractJsonSerializer deseralizer = new DataContractJsonSerializer(typeof(ips));
            ips ip = (ips)deseralizer.ReadObject(new MemoryStream(re.ResponseByte));// //反序列化ReadObject
            string NowIp = ip.ip;
            if (NowIp != config.Ip)
            {
                config.Ip = NowIp;
                updateIp();
            }
            config.Addr = ip.loc;
        }
        private void updateIp()
        {

            foreach (var item in config.DOMAIN)
            {
                string sub_dom = item.split("\\.")[0];
                string dom = item.split("\\.")[1] + "." + item.split("\\.")[2];
                string dom_id = "";
                string old_ip = "";
                string records = "";
                EasyHttpClient ehc = new EasyHttpClient();
                var re = ehc.Post("https://dnsapi.cn/Record.List", "", "login_token=" + config.TokenId + "," + config.Token + "&format=json&domain=" + dom + "&sub_domain=" + sub_dom + "&record_type=A");
                if (re.html.GetVal("code\":\"", "\"") == "1")
                {
                    records = re.html.GetVal("records\"", "type");
                    dom_id = records.GetVal("id\":\"", "\"");
                    old_ip = records.GetVal("value\":\"", "\"");
                    if (old_ip != config.Ip)
                    {
                        var re2 = ehc.Post("https://dnsapi.cn/Record.Ddns", "", "login_token=" + config.TokenId + "," + config.Token + "&format=json&domain=" + dom + "&record_type=A&record_line=默认&sub_domain=" + sub_dom + "&value=" + config.Ip + "&record_id=" + dom_id);
                        this.Text = "DNSPOD 动态域名更新程序 - " + DateTime.Now.ToString("HH:mm") + " - DnsPod更新成功";
                    }
                    else
                    {
                        this.Text = "DNSPOD 动态域名更新程序 - " + DateTime.Now.ToString("HH:mm") + " - IP无需更新";
                    }

                }
            }





        }

        private void SaveConfig()
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter binFormat = new BinaryFormatter();
            binFormat.Serialize(ms, config);
            byte[] SerializeByte = ms.ToArray();
            FileStream fs = new FileStream(@".\config.bin", FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            GZipStream compressedStream = new System.IO.Compression.GZipStream(fs, CompressionMode.Compress, true);
            compressedStream.Write(SerializeByte, 0, SerializeByte.Length);
            compressedStream.Flush();
            compressedStream.Close();
            fs.Close();
        }
        private void ReadConfig()
        {
            MemoryStream ms = new MemoryStream();

            byte[] SerializeByte = ms.ToArray();
            FileStream fs = new FileStream(@".\config.bin", FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);
            GZipStream compressedStream = new System.IO.Compression.GZipStream(fs, CompressionMode.Decompress, true);
            compressedStream.CopyTo(ms);
            ms.Position = 0;
            BinaryFormatter binFormat = new BinaryFormatter();
            if (ms.Length != 0)
                config = (Config)binFormat.Deserialize(ms);
            fs.Close();
            ms.Close();
            compressedStream.Close();

        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                config.DOMAIN.Remove(listBox1.SelectedItem.ToString());
                listBox1.Items.Remove(listBox1.SelectedItem.ToString());
                SaveConfig();
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.WindowState == System.Windows.Forms.FormWindowState.Minimized)
            {

                this.Show();
                this.WindowState = System.Windows.Forms.FormWindowState.Normal;
            }
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                this.notifyIcon1.Visible = true;
            }
        }

        private void Form1_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("请问是关闭还是最小化?\n点击【是】最小化窗体\n点击【否】退出程序！", "询问", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveConfig();
            MessageBox.Show("配置保存成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            updateIp();
        }
    }
}
