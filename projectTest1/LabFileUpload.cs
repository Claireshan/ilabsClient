using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Forms;

namespace projectTest1
{
    class LabFileUpload
    {
        public void LoadCurrentFile(string path)
        {
            try
            {
                XDocument labDoc = XDocument.Load(path);

                string deviceName = (from dev in labDoc.Descendants("Setting")
                                     where (string)dev.Attribute("Name") == "Device"
                                     select (string)dev.Attribute("Value").Value).FirstOrDefault();

                string hostAddress = (from dev in labDoc.Descendants("Setting")
                                      where (string)dev.Attribute("Name") == "Lab Url"
                                      select (string)dev.Attribute("Value").Value).FirstOrDefault();

                List<Channel> channels = (from channel in labDoc.Descendants("Setting")
                                          where (string)channel.Attribute("Type").Value == "Channel"
                                          select new Channel
                                          {
                                              Name = channel.Attribute("Name").Value,
                                              Url = channel.Attribute("Value").Value,
                                              DevicePath = channel.Attribute("DevicePath").Value
                                          }
                               ).ToList<Channel>();
                string val = string.Format("Device Name: {0}\n", deviceName);
                foreach (Channel item in channels)
                {
                    val += "Channel Name: " + item.Name + ", Url: " + item.Url + "\n";
                }
                val += "Lab Url: " + hostAddress;

                MessageBox.Show(val);

                //CreateChannels(channels, hostAddress);
                //CreateScopeTask("Dev1", channels);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        //channel type 

        public class Channel
        {
            public string Name { get; set; }
            public string Url { get; set; }
            public string DevicePath { get; set; }
        }
    }
}
