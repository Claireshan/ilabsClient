using NationalInstruments.Net;
using NationalInstruments.Analysis;
using NationalInstruments.Analysis.Conversion;
using NationalInstruments.Analysis.Dsp;
using NationalInstruments.Analysis.Dsp.Filters;
using NationalInstruments.Analysis.Math;
using NationalInstruments.Analysis.Monitoring;
using NationalInstruments.Analysis.SignalGeneration;
using NationalInstruments.Analysis.SpectralMeasurements;
using NationalInstruments;
using NationalInstruments.UI;
using NationalInstruments.UI.WindowsForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Http;
using System.Drawing.Imaging;
using System.IO;
using System.Xml.Linq;
using System.IO.Compression;
using System.Net;
using System.Globalization;

namespace projectTest1
{
    public partial class Form1 : Form
    {
        static string APPDATA_PATH = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); // AppData folder
        static string CFGFOLDER_PATH = Path.Combine(APPDATA_PATH, "Configs");     // Path for program config folder
        string labzipfile = "";
        string labfolder_path ="";
        string CFGFILE_PATH = "";   // Path for config.txt file         
        string labTimeFile = Path.Combine(CFGFOLDER_PATH, "labTime.txt");
        static string labReport_path = Path.Combine(CFGFOLDER_PATH, "labreport.txt"); // AppData folder
        string lab;
        List<string> allknob1values = new List<string>();
        List<string> allknob2values = new List<string>();
        string deviceName;
        string hostAddress;
        string datetime { get; set; }
        List<Channel> channels;
        private int numberOfChannels;
        string data1 = null;
        XDocument xdoc = new XDocument();
        List<Switch> switches;
        private int numberOfSwitches;   
        int time = 5000;
        int counter = 0;
        bool pausestate = false;
        bool fiinishstate = false;
        string xmltotxt = null;
        string encryptrpt = null;
        public string Key = "Nsi2k19!";
        
        public Form1()
        {
            InitializeComponent();
            ExistingLabList.SelectedItem = null;
            ExistingLabList.SelectedText = "--select Lab--";
            comboBox1.SelectedItem=null;
            comboBox1.SelectedText = "sine";
            existingLabs();
        }

        private void knob1_AfterChangeValue(object sender, AfterChangeNumericValueEventArgs e)
        {
            var knob2value = knob2.Value.ToString();
            var knob1value = knob1.Value.ToString();            
            ChangeWave(knob2value, knob1value);
        }

        private void knob2_AfterChangeValue(object sender, AfterChangeNumericValueEventArgs e)
        {
             var knob2value = knob2.Value.ToString();
             var knob1value = knob1.Value.ToString();
             ChangeWave(knob2value, knob1value);
        }
        private void knob2_MouseUp(object sender, MouseEventArgs e)
        {
            var knob2value = knob2.Value.ToString();
            var knob1value = knob1.Value.ToString();
            allknob2values.Add(knob2value);
        }

        private void knob1_MouseUp(object sender, MouseEventArgs e)
        {
            var knob2value = knob2.Value.ToString();
            var knob1value = knob1.Value.ToString();
            allknob1values.Add(knob1value);
            ChangeWave(knob2value, knob1value);
        }
        private string getSwitchState(string toggleState)
        {
            if (toggleState == "True")
                return "on";
            else
                return "off";
        }

        public static string ChangeWave(string amp, string freq)
        {

            string baseAddress = "http://localhost:9000/";
            double amplitude = double.Parse(amp);
            double frequency = double.Parse(freq);
            HttpClient client = new HttpClient();
            var response = client.GetAsync(baseAddress + "api/values/2/" + frequency.ToString() + "/" + amplitude.ToString()).Result;
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return "ok";
            }
            else
            {
                return "failed";
            }
        }
        //POST for changewave
        //public async void Change_Wave(string amp, string freq)
        //{
        //    string baseAddress = "http://localhost:9000/api/values";
        //    float amplitude = float.Parse(amp);
        //    float frequency = float.Parse(freq);
        //    HttpClient client = new HttpClient();
        //    var ampfreq_values = new Dictionary<string, double>();
        //    //ampfreq_values.Add( ,amplitude);
        //    //ampfreq_values.Add(, frequency);
        //    var content = new FormUrlEncodedContent();
        //    var post_response = await client.PostAsync(baseAddress, content);
        //    var response_string = await post_response.Content.ReadAsStringAsync();
        //    if (post_response.StatusCode == System.Net.HttpStatusCode.OK)
        //    {
        //        // do something
        //    }
        //    else
        //    {
        //        // do something
        //    }
        //}

        public static string ToggleSwitches(string s1)
        {
            string baseaddress = "http://localhost:9000/";
            HttpClient client = new HttpClient();
            var response = client.GetAsync(baseaddress + "api/switch/" + s1 ).Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return "ok";
            }
            else
            {
                return "failed";
            }
            
        }     
        //POST for Switch toggle
        //public async void Toggle_Switches(string s1)
        //{
        //    //making POST
        //    string baseaddress = "http://localhost:9000/api/switch";
        //    HttpClient client = new HttpClient();
        //    var switch_values = new Dictionary<string, string>();

        //    foreach (var sw in this.components.Components.OfType<NationalInstruments.UI.WindowsForms.Switch>())
        //    {
        //        switch_values.Add(sw.Name, GetSwState(sw.Value));
        //    }
        //    var content = new FormUrlEncodedContent(switch_values);          
        //    var post_response = await client.PostAsync(baseaddress, content);
        //    var response_string = await post_response.Content.ReadAsStringAsync();
        //    if (post_response.StatusCode == System.Net.HttpStatusCode.OK)
        //    {
        //       // do something
        //    }
        //    else{
        //       // do something
        //    }
        //}
    

        //Uploading the xml file
        private void uploadLabToolStripMenuItem_Click(object sender, EventArgs e)
        {        
            //file_content = reader.ReadToEnd();
           if (!Directory.Exists(CFGFOLDER_PATH))
           {
                   Directory.CreateDirectory(CFGFOLDER_PATH); // Create the Config File Exmaple folder
                   appDataXmlfile();
           }
           else
           {
                  appDataXmlfile();
           }      
                      
        }

        //Adding the xmlLabFile to appData files
        private void appDataXmlfile()
        {           
                var openDialog = new OpenFileDialog()
                {   
                    Filter = "Lab Files (*.zip)|*.zip",
                 };

                if (openDialog.ShowDialog() == DialogResult.OK)
                {                   
                    //ToDo                    
                    bool isValidLabFile = false;
                    try
                    {
                        using (ZipArchive zip = ZipFile.Open(openDialog.FileName, ZipArchiveMode.Read))
                        {
                            labzipfile = Path.GetFileNameWithoutExtension(openDialog.FileName);
                            labfolder_path = Path.Combine(CFGFOLDER_PATH, labzipfile);
                            CFGFILE_PATH = Path.Combine(labfolder_path, "lab.xml");

                            if (!Directory.Exists(Path.Combine(CFGFOLDER_PATH, Path.GetFileNameWithoutExtension(openDialog.FileName))))
                            {
                                foreach (ZipArchiveEntry entry in zip.Entries)
                                {
                                    if (entry.Name.Equals("lab.xml"))
                                    {
                                        isValidLabFile = true;
                                        break;
                                    }
                                }

                                if (isValidLabFile)
                                {                                                                   
                                    zip.ExtractToDirectory(Path.Combine(CFGFOLDER_PATH, labzipfile));                                  
                                   
                                }     
                            }
                            else
                            {
                                MessageBox.Show("Labfile already exists please just select i from the combo box!");
                            }                                                  
                        }
                    }

                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }           
        }
        
        //adding existing labs to the combbox
        private void existingLabs()
        {
            if (Directory.Exists(CFGFOLDER_PATH))
            {
                string[] existingFiles = Directory.GetDirectories(CFGFOLDER_PATH);
                List<string> labsAvailable = new List<string>();
                foreach (string item in existingFiles)
                {
                    labsAvailable.Add(Path.GetFileName(item));
                }
                ExistingLabList.Items.Clear();
                ExistingLabList.Items.AddRange(labsAvailable.ToArray());
            }
        }
        
        //loading the content from the xml file
        public void LoadCurrentFile(string path)
        {
            try
            {
            XDocument labDoc = XDocument.Load(path);

                deviceName = (from dev in labDoc.Descendants("Setting")
                              where (string)dev.Attribute("Name") == "Device"
                              select (string)dev.Attribute("Value").Value).FirstOrDefault();

                hostAddress = (from dev in labDoc.Descendants("Setting")
                               where (string)dev.Attribute("Name") == "Lab Url"
                               select (string)dev.Attribute("Value").Value).FirstOrDefault();
                
                switches = (from dev in labDoc.Descendants("Setting")
                            where (string)dev.Attribute("Type").Value == "Switch"
                            select new Switch
                            {
                                Name = dev.Attribute("Name").Value,
                                Url = dev.Attribute("Value").Value,
                              //  Index = int.Parse(dev.Attribute("Index").Value)
                            }).ToList<Switch>();

                channels = (from channel in labDoc.Descendants("Setting")
                            where (string)channel.Attribute("Type").Value == "Channel"
                            select new Channel
                            {
                                Name = channel.Attribute("Name").Value,
                                Url = channel.Attribute("Value").Value,
                                //DevicePath = "",//channel.Attribute("DevicePath").Value,
                                Index = int.Parse(channel.Attribute("Index").Value)
                            }
                                         ).ToList<Channel>();

                numberOfChannels = channels.Count();
                numberOfSwitches = switches.Count();
                string val = string.Format("Device Name: {0}\n", deviceName);
                CreateChannels(channels, hostAddress);
                labCircuit.Image = Image.FromFile(Path.Combine(labfolder_path, "images", "ON_OFF" + ".png"));
                label2.Visible = true;
                CreateSwitches(switches, hostAddress);
                ConnectDataSockets();                     

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        //creating the switches
        public void CreateSwitches(List<Switch> labSwitches, string hostAddress)
        {
           //  List<Image> image = Directory.GetFiles(Path.Combine(CFGFOLDER_PATH, "images"));
            foreach (Switch _switch in labSwitches)
            {
                 NationalInstruments.UI.WindowsForms.Switch sw = new NationalInstruments.UI.WindowsForms.Switch();
                 ((System.ComponentModel.ISupportInitialize)(sw)).BeginInit();               
                sw.OffColor = System.Drawing.Color.Blue;
                sw.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
                sw.Size = new System.Drawing.Size(58, 77);
                sw.SwitchStyle = NationalInstruments.UI.SwitchStyle.HorizontalSlide3D;
                sw.TabIndex = 5;
                sw.Value = true;
                sw.Name = _switch.Name;              
                ((System.ComponentModel.ISupportInitialize)(sw)).EndInit();
                flowLayoutPanel1.Controls.Add(sw);
                sw.StateChanged += new NationalInstruments.UI.ActionEventHandler(switches_StateChanged);
                this.components.Add(sw);                
            }          
        }

        string GetSwState(bool st){
            if (st == true)
                return "on";
            else
                return "off";
        }

        void switches_StateChanged(object sender, ActionEventArgs e)
        {      
            var labSwitches = this.components.Components.OfType<NationalInstruments.UI.WindowsForms.Switch>();
            string val="";           
           
            foreach (var sw in labSwitches)
            {

                val += GetSwState(sw.Value) + "_";      

               // ToggleSwitches(switchvalue);
            }
            if (Directory.Exists(Path.Combine(labfolder_path, "images")))
            {
                try
                {
                    val = val.Remove(val.Length - 1);
                    textBox1.Visible = false;
                    labCircuit.Image = Image.FromFile(Path.Combine(labfolder_path, "images", val + ".png"));  
                }
                catch (Exception ex)
                {
                    labCircuit.Image = null;
                    textBox1.Visible = true;
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("No image file exists");
                
            }
           
         }

        //creating channels for data transfer
        public void CreateChannels(List<Channel> channels, string hostAddress)
        {
            // creating the wave forms
            List<Color> colors = new List<Color> { System.Drawing.Color.Green, 
                  System.Drawing.Color.Blue, System.Drawing.Color.Red,
                  System.Drawing.Color.White,System.Drawing.Color.Brown,
                  System.Drawing.Color.DarkCyan,System.Drawing.Color.DarkOrange,System.Drawing.Color.Gainsboro};

            waveformGraph1.Plots.RemoveAt(0);
            for (int i = 0; i < channels.Count; i++)
            {
                WaveformPlot plot = new WaveformPlot();

                plot.LineColor = colors[i];
                plot.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
                plot.XAxis = this.xAxis1;
                plot.YAxis = this.yAxis1;
                this.waveformGraph1.Plots.Add(plot);
            }

            //create dataSockets

            foreach (Channel channel in channels)
            {
                DataSocket dataSocket = new DataSocket(this.components);
                ((System.ComponentModel.ISupportInitialize)(dataSocket)).BeginInit();

                ((System.ComponentModel.ISupportInitialize)(dataSocket)).EndInit();

                dataSocket.Url = "dstp://" + hostAddress + channel.Url;
                dataSocket.AccessMode = NationalInstruments.Net.AccessMode.ReadAutoUpdate;
                dataSocket.DataUpdated += (object sender, DataUpdatedEventArgs e) =>
                                              {
                                                  DataSocket dS = (DataSocket)sender;

                                                  waveformGraph1.Plots[channel.Index].PlotY((double[])e.Data.Value);
                                              };
            }
        }

        //Connectiong to the data sockets
        public void ConnectDataSockets()
        {
            try
            {
                foreach (var dataSocket in this.components.Components.OfType<DataSocket>())
                {
                    if (dataSocket.IsConnected)
                        dataSocket.Disconnect();
                    dataSocket.Connect();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //Disconnecting from the data socket
        public void DisConnectDataSockets()
        {
            try
            {
                foreach (var dataSocket in this.components.Components.OfType<DataSocket>())
                {
                    if (dataSocket.IsConnected)
                        dataSocket.Disconnect();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }            

        private void startLabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string baseAddress = "http://localhost:9000/api/time";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(baseAddress);
            request.Method = "Get";
            request.KeepAlive = true;
            
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string myResponse = "";

            using (System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream()))
            {
                myResponse = sr.ReadToEnd();
            }

            String serverdatetime = myResponse.ToString().Substring(1, myResponse.Length - 2);
            DateTime servertime = DateTime.Parse(serverdatetime, new System.Globalization.CultureInfo("pt-BR"));
            if (ExistingLabList.SelectedIndex > -1)
            {
                lab = ExistingLabList.GetItemText(ExistingLabList.SelectedItem);
                labzipfile = lab;
                labfolder_path = Path.Combine(CFGFOLDER_PATH, labzipfile);
                CFGFILE_PATH = Path.Combine(labfolder_path, "lab.xml");
                
                if (File.Exists(CFGFILE_PATH))
                {
                    string datetime = (from dev in XDocument.Load(CFGFILE_PATH).Descendants("Setting")
                                       where (string)dev.Attribute("Name") == "DateTime"
                                       select (string)dev.Attribute("Value").Value).FirstOrDefault();

                    DateTime scheduletime = DateTime.Parse(datetime);
                    DateTime duration = scheduletime.AddMinutes(30);
                    //MessageBox.Show(servertime.ToString("hh:mm"));
                    //MessageBox.Show(scheduletime.ToString("hh:mm"));
                    //MessageBox.Show(duration.ToString("hh:mm"));

                    try
                    {
                        //  MessageBox.Show(myResponse);
                        if (servertime.ToShortDateString().Equals(scheduletime.ToShortDateString()))
                        {
                            if (servertime.TimeOfDay >= scheduletime.TimeOfDay && servertime.TimeOfDay <= duration.TimeOfDay)
                            {
                                timer1.Enabled = true;
                                timer1.Start();
                                LoadCurrentFile(CFGFILE_PATH);

                            }
                            else if (servertime.TimeOfDay >= scheduletime.TimeOfDay && servertime.TimeOfDay > duration.TimeOfDay)
                            {
                                MessageBox.Show("please reschedule");

                            }
                            else
                            {
                                MessageBox.Show("not yet time");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Please varify date scheduled");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                }

                else
                {
                    MessageBox.Show("Upload lab file");
                }    
            }
            else
            {
                MessageBox.Show("Please select a lab to do!");
            }
          
          }

        // generatinng the lab report
        private void generatingLabReport()
        {
            //taking the graph screenshot
            using (Bitmap step1 = new Bitmap(waveformGraph1.ClientSize.Width, waveformGraph1.ClientSize.Height))
            {
                waveformGraph1.DrawToBitmap(step1, waveformGraph1.ClientRectangle);
               // waveformGraph1.DrawPlotAreaComponents(waveformPlot2);
                using (MemoryStream mem = new MemoryStream())
                {
                    step1.Save(mem, System.Drawing.Imaging.ImageFormat.Bmp);
                    //converting the bytes to a string
                    data1 = Convert.ToBase64String(mem.ToArray());

                }
            }

            XElement frequencies = new XElement("Frequencies",
               from f in allknob1values
               select
               new XElement("Frequency", f)
               );
            XElement amplitude = new XElement("Amplitudes",
                from a in allknob2values
                select
                new XElement("Amplitude", a)
                );
            var i = new List<pictures>() { 
                    new pictures() {ID = 3, image= data1},                      
                    };

            XElement labimages = new XElement("LabImages",
                     from emp in i
                     select
                         new XElement("ID", emp.ID,
                         new XElement("Image", emp.image)));

            //Build the xml document
            xdoc = new XDocument(
               new XDeclaration("1.0", "utf-8", "yes"), new XElement("root", frequencies, amplitude, labimages));

            //convert xml created to text file to encrypt
            //encryption does not tamper with base64 image string
            xmltotxt = xdoc.ToString();
            encryptrpt = AesEncryption.EncryptDataAES(xmltotxt, Key);
            
            
            if (Directory.Exists(CFGFOLDER_PATH))
            {                
                
                if (!File.Exists(labReport_path))
                {                  
                    StreamWriter savereport = new StreamWriter(labReport_path, true);
                    savereport.WriteLine(encryptrpt);
                    savereport.Close();                  
                }
                else
                {
                    MessageBox.Show("Lab already finished");
                }
            }
        }


        //downloading the labreport from appdata
        private void downloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
           // var labreport_file = string.Empty;
            if(Directory.Exists(CFGFOLDER_PATH))
            {
                if(File.Exists(labReport_path))
                {
                    string dest= "labreport.txt";
                    string targetPath = @"C:\Users\ilabsdeveloper\Downloads";
                    string destFile = Path.Combine(targetPath, dest);
                    if (!File.Exists(destFile))
                    {        
                        
                        File.Copy(labReport_path, destFile);                      
                        MessageBox.Show("lab downloaded to Downloads");
                    }
                    else
                    {
                        MessageBox.Show("report already downloaded");
                    }                  
                }
                else
                {
                    MessageBox.Show("No lab was done please finish the lab");
                }
            }
            else
            {
                MessageBox.Show("No lab was uploaded");
            }
        }

        
        private void timer1_Tick(object sender, EventArgs e)
        {            
            int new_time =  time--;
            Time.Text = new_time.ToString();

            if (new_time == 0)
            {
                timer1.Stop();
                DisConnectDataSockets();
                generatingLabReport();
                deleteXmlFile();
            }

            counter++;
            if (counter == 2) //elapsed five times
            {
                counter = 0;
                existingLabs();
                if (Directory.Exists(CFGFOLDER_PATH))
                {
                    string runlab = ExistingLabList.GetItemText(ExistingLabList.SelectedItem); 
                    File.WriteAllText(labTimeFile, new_time.ToString() + "\n" +runlab);
                    //File.WriteAllText(labTimeFile, "\n" + lab);
                }
                else
                {
                    Directory.CreateDirectory(CFGFOLDER_PATH);
                    File.WriteAllText(labTimeFile, new_time.ToString());
                  //  File.WriteAllText(labTimeFile, "\n" + lab);
                }
            }
            
        }
            
        //Finishing the current lab
        private void finishLabToolStripMenuItem_Click(object sender, EventArgs e)
        {
           // timer1.Stop();
            DisConnectDataSockets();           
            generatingLabReport();
            fiinishstate = true;
            resumeToolStripMenuItem.Enabled = true;
        }

        //closing existing lab
        private void closeLabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            DisConnectDataSockets();
            labCircuit.Image = null;
            System.Windows.Forms.Application.ExitThread(); 
            //deleteXmlFile();

        }

        //deleting existing lab file
        private void deleteXmlFile()
        {
            if (Directory.Exists(CFGFOLDER_PATH))
            {
                var dir = new DirectoryInfo(CFGFOLDER_PATH);
                try
                {
                    dir.Delete(true);  
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }                        
              
            }           
        }

        //screenshoting the graphs
        private void screenShotToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            SaveFileDialog dialogue = new SaveFileDialog();
            dialogue.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif|Png Image|*.png|Txt Image|*.txt";
            if (dialogue.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.IO.FileStream fs = (System.IO.FileStream)dialogue.OpenFile();
                if (dialogue.FileName != "")
                {
                    using (Bitmap bmp = new Bitmap(waveformGraph1.ClientSize.Width, waveformGraph1.ClientSize.Height))
                    {
                        waveformGraph1.DrawToBitmap(bmp, waveformGraph1.ClientRectangle);
                        bmp.Save(fs, ImageFormat.Png);
                    }
                }
            }
        }

        //Pausing the lab
        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DisConnectDataSockets();
            pausestate = true;
            resumeToolStripMenuItem.Enabled = true;
        }

        //resuming from a pause
        private void resumeToolStripMenuItem_Click(object sender, EventArgs e)
        {            
            if (pausestate == true)
            {
                ConnectDataSockets();
                resumeToolStripMenuItem.Enabled = false;
                
            }
            else if (fiinishstate == true)
            {
                ConnectDataSockets();
            }
            else
            {
                if (File.Exists(labTimeFile))
                {

                    string remainingtime = File.ReadLines(labTimeFile).First();
                    string labrunning = File.ReadLines(labTimeFile).ElementAtOrDefault(1);
                    ExistingLabList.SelectedText = labrunning;
                    labzipfile = labrunning;
                    labfolder_path = Path.Combine(CFGFOLDER_PATH, labzipfile);
                    CFGFILE_PATH = Path.Combine(labfolder_path, "lab.xml");
                  //  File.AppendAllText(labTimeFile, "\n" + ExistingLabList.SelectedText);
                    if (int.Parse(remainingtime) != 0)
                    {
                        time = int.Parse(remainingtime);
                        timer1.Start();
                        LoadCurrentFile(CFGFILE_PATH);
                    }
                    else
                    {
                        MessageBox.Show("No remaining time");
                        DisConnectDataSockets();
                    }

                }
                else
                {
                    MessageBox.Show("No lab was running");
                }
                resumeToolStripMenuItem.Enabled = false;
            }            
            
        }

        public class Channel
        {
            public string Name { get; set; }
            public string Url { get; set; }
            public string DevicePath { get; set; }
            public int Index { get; set; }
        }

        public class pictures
        {
            public int ID { get; set; }
            public string image { get; set; }
        }
        public class Switch
        {
            public string Name { get; set; }
            public string Url { get; set; }
            public string DevicePath { get; set; }

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string wave_type = comboBox1.GetItemText(comboBox1.SelectedItem);
            if (wave_type == "sine"){
                 
            }
            else if (wave_type == "square")
            {

            }
            else
            {

            }
        }                
        
    }
}
