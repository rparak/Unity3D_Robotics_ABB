/****************************************************************************
MIT License
Copyright(c) 2020 Roman Parak
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*****************************************************************************
Author   : Roman Parak
Email    : Roman.Parak @outlook.com
Github   : https://github.com/rparak
File Name: abb_data_processing.cs
****************************************************************************/

// System
using System;
using System.Threading;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Xml;
using System.Globalization;
using System.Diagnostics;
// Unity
using UnityEngine;
using Debug = UnityEngine.Debug;

public class abb_data_processing : MonoBehaviour
{
    public static class GlobalVariables_Main_Control
    {
        public static bool connect, disconnect;
    }

    public static class ABB_Stream_Data_XML
    {
        // IP Port Number and IP Address
        public static string ip_address;
        //  The target of reading the data: jointtarget / robtarget
        public static string xml_target = "";
        // Comunication Speed (ms)
        public static int time_step;
        // Joint Space:
        //  Orientation {J1 .. J6} (°)
        public static double[] J_Orientation = new double[6];
        // Class thread information (is alive or not)
        public static bool is_alive = false;
    }

    public static class ABB_Stream_Data_JSON
    {
        // IP Port Number and IP Address
        public static string ip_address;
        //  The target of reading the data: jointtarget / robtarget
        public static string json_target = "";
        // Comunication Speed (ms)
        public static int time_step;
        // Cartesian Space:
        //  Position {X, Y, Z} (mm)
        public static double[] C_Position = new double[3];
        //  Orientation {Quaternion} (-):
        public static double[] C_Orientation = new double[4];
        // Class thread information (is alive or not)
        public static bool is_alive = false;
    }

    // Class Stream {ABB Robot Web Services - XML}
    private ABB_Stream_XML ABB_Stream_Robot_XML;
    // Start Stream {ABB Robot Web Services - JSON}
    private ABB_Stream_JSON ABB_Stream_Robot_JSON;

    // Other variables
    private int main_abb_state = 0;

    // Start is called before the first frame update
    void Start()
    {
        // Initialization {Robot Web Services ABB - XML}
        //  Stream Data:
        ABB_Stream_Data_XML.ip_address = "127.0.0.1";
        //  The target of reading the data: jointtarget / robtarget
        ABB_Stream_Data_XML.xml_target = "jointtarget";
        //  Communication speed (ms)
        ABB_Stream_Data_XML.time_step = 2;
        // Initialization {Robot Web Services ABB - JSON}
        //  Stream Data:
        ABB_Stream_Data_JSON.ip_address = "127.0.0.1";
        //  The target of reading the data: jointtarget / robtarget
        ABB_Stream_Data_JSON.json_target = "robtarget";
        //  Communication speed (ms)
        ABB_Stream_Data_JSON.time_step = 200;

        // Start Stream {ABB Robot Web Services - XML}
        ABB_Stream_Robot_XML = new ABB_Stream_XML();
        // Start Stream {ABB Robot Web Services - JSON}
        ABB_Stream_Robot_JSON = new ABB_Stream_JSON();

    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        switch (main_abb_state)
        {
            case 0:
                {
                    // ------------------------ Wait State {Disconnect State} ------------------------//
                    if (GlobalVariables_Main_Control.connect == true)
                    {
                        //Start Stream { ABB Robot Web Services - XML}
                        ABB_Stream_Robot_XML.Start();
                        //Start Stream { ABB Robot Web Services - JSON}
                        ABB_Stream_Robot_JSON.Start();

                        // go to connect state
                        main_abb_state = 1;
                    }
                }
                break;
            case 1:
                {
                    // ------------------------ Data Processing State {Connect State} ------------------------//
                    if (GlobalVariables_Main_Control.disconnect == true)
                    {
                        // Stop threading block {ABB Robot Web Services - XML}
                        if (ABB_Stream_Data_XML.is_alive == true)
                        {
                            ABB_Stream_Robot_XML.Stop();
                        }

                        // Stop threading block {ABB Robot Web Services - JSON}
                        if (ABB_Stream_Data_JSON.is_alive == true)
                        {
                            ABB_Stream_Robot_JSON.Stop();
                        }

                        if (ABB_Stream_Data_XML.is_alive == false && ABB_Stream_Data_JSON.is_alive == false)
                        {
                            // go to initialization state {wait state -> disconnect state}
                            main_abb_state = 0;
                        }
                    }
                }
                break;
        }
    }

    void OnApplicationQuit()
    {
        try
        {
            // Destroy Stream {ABB Robot Web Services - XML}
            ABB_Stream_Robot_XML.Destroy();
            //Start Stream { ABB Robot Web Services - JSON}
            ABB_Stream_Robot_JSON.Destroy();

            Destroy(this);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    class ABB_Stream_XML
    {
        // Initialization of Class variables
        //  Thread
        private Thread robot_thread = null;
        private bool exit_thread = false;
        // Robot Web Services (RWS): XML Communication
        private CookieContainer c_cookie = new CookieContainer();
        private NetworkCredential n_credential = new NetworkCredential("Default User", "robotics");

        public void ABB_Stream_Thread_XML()
        {
            try
            {
                // Initialization timer
                var t = new Stopwatch();

                while (exit_thread == false)
                {
                    // t_{0}: Timer start.
                    t.Start();

                    // Get the system resource
                    Stream source_data = Get_System_Resource(ABB_Stream_Data_XML.ip_address, ABB_Stream_Data_XML.xml_target);
                    // Current data streaming from the source page
                    Stream_Data(source_data);

                    // t_{1}: Timer stop.
                    t.Stop();

                    // Recalculate the time: t = t_{1} - t_{0} -> Elapsed Time in milliseconds
                    if (t.ElapsedMilliseconds < ABB_Stream_Data_XML.time_step)
                    {
                        Thread.Sleep(ABB_Stream_Data_XML.time_step - (int)t.ElapsedMilliseconds);
                    }

                    // Reset (Restart) timer.
                    t.Restart();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        Stream Get_System_Resource(string host, string target)
        {
            // http:// + ip address + xml address + target {joint, cartesian}
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri("http://" + host + "/rw/rapid/tasks/T_ROB1/motion?resource=" + target));
            // Login: Default User; Password: robotics
            request.Credentials = n_credential;
            // don't use proxy, it's aussumed that the RC/VC is reachable without going via proxy 
            request.Proxy = null;
            request.Method = "GET";
            // re-use http session between requests 
            request.CookieContainer = c_cookie;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            return response.GetResponseStream();
        }

        void Stream_Data(Stream source_data)
        {
            // Xml Node: Initialization Document
            XmlDocument xml_doc = new XmlDocument();
            // Load XML data
            xml_doc.Load(source_data);

            // Create an XmlNamespaceManager for resolving namespaces.
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml_doc.NameTable);

            nsmgr.AddNamespace("ns", "http://www.w3.org/1999/xhtml");

            // -------------------- Read State {Joint (1 - 6)} -------------------- //
            XmlNodeList xml_node = xml_doc.SelectNodes("//ns:li[@class='rapid-jointtarget']", nsmgr);

            // Joint (1 - 6) -> Read RWS XML
            ABB_Stream_Data_XML.J_Orientation[0] = double.Parse(xml_node[0].SelectSingleNode("ns:span[@class='j1']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat);
            ABB_Stream_Data_XML.J_Orientation[1] = double.Parse(xml_node[0].SelectSingleNode("ns:span[@class='j2']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat);
            ABB_Stream_Data_XML.J_Orientation[2] = double.Parse(xml_node[0].SelectSingleNode("ns:span[@class='j3']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat);
            ABB_Stream_Data_XML.J_Orientation[3] = double.Parse(xml_node[0].SelectSingleNode("ns:span[@class='j4']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat);
            ABB_Stream_Data_XML.J_Orientation[4] = double.Parse(xml_node[0].SelectSingleNode("ns:span[@class='j5']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat);
            ABB_Stream_Data_XML.J_Orientation[5] = double.Parse(xml_node[0].SelectSingleNode("ns:span[@class='j6']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat);
        }

        public void Start()
        {
            exit_thread = false;
            // Start a thread to stream ABB Robot
            robot_thread = new Thread(new ThreadStart(ABB_Stream_Thread_XML));
            robot_thread.IsBackground = true;
            robot_thread.Start();
            // Thread is active
            ABB_Stream_Data_XML.is_alive = true;
        }
        public void Stop()
        {
            exit_thread = true;
            // Stop a thread
            Thread.Sleep(100);
            ABB_Stream_Data_XML.is_alive = robot_thread.IsAlive;
            robot_thread.Abort();
        }
        public void Destroy()
        {
            // Stop a thread (Robot Web Services communication)
            Stop();
            Thread.Sleep(100);
        }
    }

    class ABB_Stream_JSON
    {
        // Initialization of Class variables
        //  Thread
        private Thread robot_thread = null;
        private bool exit_thread = false;

        async void ABB_Stream_Thread_JSON()
        {
            var handler = new HttpClientHandler { Credentials = new NetworkCredential("Default User", "robotics") };
            // disable the proxy, the controller is connected on same subnet as the PC 
            handler.Proxy = null;
            handler.UseProxy = false;

            try
            {
                // Send a request continue when complete
                using (HttpClient client = new HttpClient(handler))
                {
                    // Initialization timer
                    var t = new Stopwatch();

                    while (exit_thread == false)
                    {
                        // t_{0}: Timer start.
                        t.Start();

                        // Current data streaming from the source page
                        using (HttpResponseMessage response = await client.GetAsync("http://" + ABB_Stream_Data_JSON.ip_address + "/rw/rapid/tasks/T_ROB1/motion?resource=" + ABB_Stream_Data_JSON.json_target + "&json=1"))
                        {
                            using (HttpContent content = response.Content)
                            {
                                try
                                {
                                    // Check that response was successful or throw exception
                                    response.EnsureSuccessStatusCode();
                                    // Get HTTP response from completed task.
                                    string result = await content.ReadAsStringAsync();
                                    // Deserialize the returned json string
                                    dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject(result);

                                    // Display controller name, version and version name
                                    var service = obj._embedded._state[0];

                                    // TCP {X, Y, Z} -> Read RWS JSON
                                    ABB_Stream_Data_JSON.C_Position[0] = (double)service.x;
                                    ABB_Stream_Data_JSON.C_Position[1] = (double)service.y;
                                    ABB_Stream_Data_JSON.C_Position[2] = (double)service.z;
                                    // Quaternion {q1 .. q4} -> Read RWS JSON
                                    ABB_Stream_Data_JSON.C_Orientation[0] = (double)service.q1;
                                    ABB_Stream_Data_JSON.C_Orientation[1] = (double)service.q2;
                                    ABB_Stream_Data_JSON.C_Orientation[2] = (double)service.q3;
                                    ABB_Stream_Data_JSON.C_Orientation[3] = (double)service.q4;

                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.Message);
                                }
                            }
                        }

                        // t_{1}: Timer stop.
                        t.Stop();

                        // Recalculate the time: t = t_{1} - t_{0} -> Elapsed Time in milliseconds
                        if (t.ElapsedMilliseconds < ABB_Stream_Data_JSON.time_step)
                        {
                            Thread.Sleep(ABB_Stream_Data_JSON.time_step - (int)t.ElapsedMilliseconds);
                        }

                        // Reset (Restart) timer.
                        t.Restart();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Communication Problem: {0}", e);
            }
        }

        public void Start()
        {
            exit_thread = false;
            // Start a thread to stream ABB Robot
            robot_thread = new Thread(new ThreadStart(ABB_Stream_Thread_JSON));
            robot_thread.IsBackground = true;
            robot_thread.Start();
            // Thread is active
            ABB_Stream_Data_JSON.is_alive = true;
        }
        public void Stop()
        {
            exit_thread = true;
            // Stop a thread
            Thread.Sleep(100);
            ABB_Stream_Data_JSON.is_alive = robot_thread.IsAlive;
            robot_thread.Abort();
        }
        public void Destroy()
        {
            // Stop a thread (Robot Web Services communication)
            Stop();
            Thread.Sleep(100);
        }
    }
}
