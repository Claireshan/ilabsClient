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
        static string CFGFILE_PATH = Path.Combine(CFGFOLDER_PATH, "lab.xml");   // Path for config.txt file   
        static string labReport_path = Path.Combine(CFGFOLDER_PATH, "labreport.xml"); //path for labreport
        static string labzipfile = Path.Combine(CFGFOLDER_PATH, "labzipfile.xml");
        string labTimeFile = Path.Combine(CFGFOLDER_PATH, "labTime.txt");
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
        
        public Form1()
        {
            InitializeComponent();            
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
                  MessageBox.Show("Lab already exits please close it");

           }      
                      
        }

        //Adding the xmlLabFile to appData files
        private void appDataXmlfile()
        {           
                var openDialog = new OpenFileDialog()
                {//"xml files (*.xml)|*.xml"
                    InitialDirectory = "HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\ComDlg32",
                    Filter = "Lab Files (*.zip)|*.zip",
                    FilterIndex = 2,
                    RestoreDirectory = true
                };

                if (openDialog.ShowDialog() == DialogResult.OK)
                {                   
                    //ToDo                    
                    bool isValidLabFile = false;
                    try
                    {
                        using (ZipArchive zip = ZipFile.Open(openDialog.FileName, ZipArchiveMode.Read))
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
                                zip.ExtractToDirectory(CFGFOLDER_PATH);                         
                               
                            }                           
                        }
                    }

                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

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
                labCircuit.Image = Image.FromFile(Path.Combine(CFGFOLDER_PATH, "images","ON_OFF.png"));
                CreateChannels(channels, hostAddress);
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
                sw.BackColor = System.Drawing.Color.Silver;
                sw.OffColor = System.Drawing.Color.Blue;
                sw.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
                sw.Size = new System.Drawing.Size(58, 77);
                sw.SwitchStyle = NationalInstruments.UI.SwitchStyle.PushButton3D;
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
            if (Directory.Exists(Path.Combine(CFGFOLDER_PATH, "images")))
            {
                try
                {
                    val = val.Remove(val.Length - 1);
                    textBox1.Visible = false;
                    labCircuit.Image = Image.FromFile(Path.Combine(CFGFOLDER_PATH, "images", val + ".png"));  
                }
                catch (Exception ex)
                {
                    labCircuit.Image = null;
                    textBox1.Visible = true;
                   // MessageBox.Show(ex.Message);
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
                       
            if (File.Exists(CFGFILE_PATH))
            {
                string datetime = (from dev in XDocument.Load(CFGFILE_PATH).Descendants("Setting")
                                   where (string)dev.Attribute("Name") == "DateTime"
                                   select (string)dev.Attribute("Value").Value).FirstOrDefault();
               
               DateTime scheduletime = DateTime.Parse(datetime);
               DateTime duration = scheduletime.AddMinutes(30);

                try
                {
                //MessageBox.Show(myResponse);
                if (int.Parse(scheduletime.ToShortDateString())==int.Parse(servertime.ToShortDateString()))
                {
                    if (int.Parse(servertime.ToShortTimeString()) < int.Parse(duration.ToShortTimeString()))
                    {
                        timer1.Enabled = true;
                        timer1.Start();
                        LoadCurrentFile(CFGFILE_PATH);
                    }
                    else
                    {
                        MessageBox.Show("please reschedule someelse is using the lab");
                    }
                    
                }
                else if (int.Parse(scheduletime.ToShortDateString()) > int.Parse(servertime.ToShortDateString()))
                {
                    MessageBox.Show("not yet time");
                }
                else
                {
                    MessageBox.Show("please reschedule");
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

        // generatinng the lab report
        private void generatingLabReport()
        {
            //taking the graph screenshot
            using (Bitmap step1 = new Bitmap(waveformGraph1.ClientSize.Width, waveformGraph1.ClientSize.Height))
            {
                waveformGraph1.DrawToBitmap(step1, waveformGraph1.ClientRectangle);
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
            // Build the document
            xdoc = new XDocument(
               new XDeclaration("1.0", "utf-8", "yes"), new XElement("root", frequencies, amplitude, labimages));
            
            
            if (Directory.Exists(CFGFOLDER_PATH))
            {                
                
                if (!File.Exists(labReport_path))
                {
                    xdoc.Save(labReport_path);
                    
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
                    string dest= "labreports.xml";
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
            if (counter == 5) //elapsed five times
            {
                counter = 0;
                File.WriteAllText(labTimeFile, new_time.ToString());
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
            deleteXmlFile();

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
                resume();
                ConnectDataSockets();
                resumeToolStripMenuItem.Enabled = false;
                
            }
            else if (fiinishstate == true)
            {
                ConnectDataSockets();
            }
            else
            {
                resume();
                LoadCurrentFile(CFGFILE_PATH);
                resumeToolStripMenuItem.Enabled = false;
            }            
            
        }
        //resuming from a pause
        private void resume()
        {
            if (File.Exists(labTimeFile))
            {
                string remainingtime = File.ReadAllText(labTimeFile);
                if (int.Parse(remainingtime) != 0)
                {
                    time = int.Parse(remainingtime);
                    timer1.Start();                    
                }
                else
                {
                    MessageBox.Show("No remaining time");
                }
                
            }
            else
            {
                MessageBox.Show("No lab was ran");
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
        
    }
}
