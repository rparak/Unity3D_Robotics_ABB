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

// ------------------------------------------------------------------------------------------------------------------------//
// ----------------------------------------------------- LIBRARIES --------------------------------------------------------//
// ------------------------------------------------------------------------------------------------------------------------//

// -------------------- System -------------------- //
using System;
using System.Globalization;
using System.Net;
using System.IO;
using System.Xml;
using System.Net.Http;
using System.Threading;
// -------------------- Unity -------------------- //
using UnityEngine;

// -------------------- Class {Global Variable} -> Main Control -------------------- //
public static class GlobalVariables_Main_Control
{
    // -------------------- Bool -------------------- //
    public static bool abb_irb_dt_enable_rws_xml, abb_irb_dt_enable_rws_json;
    public static bool connect, disconnect;
    // -------------------- String -------------------- //
    public static string[] abb_irb_rws_xml_config = new string[2];
    public static string[] abb_irb_rws_json_config = new string[2];
}

// -------------------- Class {Global Variable} -> Robot Web Services(RWS) Client -------------------- //
public static class GlobalVariables_RWS_client
{
    // -------------------- Float -------------------- //
    public static float[] robotBaseRotLink_irb_joint = { 0f, 0f, 0f, 0f, 0f, 0f };
    // -------------------- String -------------------- //
    public static string[] robotBaseRotLink_irb_cartes = new string[7] { "0.0", "0.0", "0.0", "0.0", "0.0", "0.0", "0.0" };
}

public class abb_data_processing : MonoBehaviour
{
    // -------------------- Thread -------------------- //
    private Thread rws_read_Thread_xml, rws_read_Thread_json;
    // -------------------- Vector3 -------------------- //
    Vector3 set_position_ABB_IRB120;
    // -------------------- CookieContainer -------------------- //
    static CookieContainer c_cookie = new CookieContainer();
    // -------------------- Network Credential -------------------- //
    static NetworkCredential n_credential = new NetworkCredential("Default User", "robotics");
    // -------------------- Stream -------------------- //
    Stream xml_irb_joint;
    // -------------------- XmlNode -------------------- //
    XmlNode joint1, joint2, joint3, joint4, joint5, joint6;
    // -------------------- Int -------------------- //
    private int main_abb_state = 0;

    // ------------------------------------------------------------------------------------------------------------------------//
    // ------------------------------------------------ INITIALIZATION {START} ------------------------------------------------//
    // ------------------------------------------------------------------------------------------------------------------------//
    void Start()
    {
        // ------------------------ Initialization { IRB Digital Twin {Control Robot} - RWS{Robot Web Services) XML } ------------------------//
        // Robot IP Address
        GlobalVariables_Main_Control.abb_irb_rws_xml_config[0] = "127.0.0.1";
        // Robot XML Target
        GlobalVariables_Main_Control.abb_irb_rws_xml_config[1] = "jointtarget";
        // Control -> Start {Read OPCUA data}
        GlobalVariables_Main_Control.abb_irb_dt_enable_rws_xml = true;
        // ------------------------ Initialization { IRB Digital Twin {Control Robot} - RWS{Robot Web Services) JSON } ------------------------//
        // Robot IP Address
        GlobalVariables_Main_Control.abb_irb_rws_json_config[0] = "127.0.0.1";
        // Robot JSON Target
        GlobalVariables_Main_Control.abb_irb_rws_json_config[1] = "/rw/rapid/tasks/T_ROB1/motion?resource=robtarget&json=1";
        // Control -> Start {Read OPCUA data}
        GlobalVariables_Main_Control.abb_irb_dt_enable_rws_json = true;
        
    }

    // ------------------------------------------------------------------------------------------------------------------------ //
    // ------------------------------------------------ MAIN FUNCTION {Cyclic} ------------------------------------------------ //
    // ------------------------------------------------------------------------------------------------------------------------ //
    private void Update()
    {
        switch (main_abb_state)
        {
            case 0:
                {
                    // ------------------------ Wait State {Disconnect State} ------------------------//
                    if (GlobalVariables_Main_Control.connect == true)
                    {
                        // Control -> Start {Read RWS XML data}
                        GlobalVariables_Main_Control.abb_irb_dt_enable_rws_xml = true;
                        // Control -> Start {Read RWS JSON data}
                        GlobalVariables_Main_Control.abb_irb_dt_enable_rws_json = true;

                        // ABB IRB 120 Control -> Start {RWS XML}
                        // ------------------------ Threading Block { RWS Read Data {XML} } ------------------------//
                        rws_read_Thread_xml = new Thread(() => RWS_Service_read_xml_thread_function("http://" + GlobalVariables_Main_Control.abb_irb_rws_xml_config[0], GlobalVariables_Main_Control.abb_irb_rws_xml_config[1]));
                        rws_read_Thread_xml.IsBackground = true;
                        rws_read_Thread_xml.Start();

                        // ABB IRB 120 Control -> Start {RWS JSON}
                        // ------------------------ Threading Block { RWS Read Data {JSON} } ------------------------//
                        rws_read_Thread_json = new Thread(() => RWS_Service_read_json_thread_function("http://" + GlobalVariables_Main_Control.abb_irb_rws_json_config[0], GlobalVariables_Main_Control.abb_irb_rws_json_config[1]));
                        rws_read_Thread_json.IsBackground = true;
                        rws_read_Thread_json.Start();

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
                        // Control -> Stop {Read RWS XML data}
                        GlobalVariables_Main_Control.abb_irb_dt_enable_rws_xml = true;
                        // Control -> Stop {Read RWS JSON data}
                        GlobalVariables_Main_Control.abb_irb_dt_enable_rws_json = true;

                        // Abort threading block {RWS XML -> read data}
                        if (rws_read_Thread_xml.IsAlive == true)
                        {
                            rws_read_Thread_xml.Abort();
                        }
                        // Abort threading block {RWS JSON -> read data}
                        if (rws_read_Thread_json.IsAlive == true)
                        {
                            rws_read_Thread_json.Abort();
                        }
                        if (rws_read_Thread_xml.IsAlive == false && rws_read_Thread_json.IsAlive == false)
                        {
                            // go to initialization state {wait state -> disconnect state}
                            main_abb_state = 0;
                        }
                    }
                }
                break;
        }
    }

    // ------------------------------------------------------------------------------------------------------------------------//
    // -------------------------------------------------------- FUNCTIONS -----------------------------------------------------//
    // ------------------------------------------------------------------------------------------------------------------------//

    // -------------------- Abort Threading Blocks -------------------- //
    void OnApplicationQuit()
    {
        try
        {
            // Stop - threading while
            // XML
            GlobalVariables_Main_Control.abb_irb_dt_enable_rws_xml  = false;
            // JSON
            GlobalVariables_Main_Control.abb_irb_dt_enable_rws_json = false;

            // Abort threading block {RWS XML -> read data}
            if (rws_read_Thread_xml.IsAlive == true)
            {
                rws_read_Thread_xml.Abort();
            }
            // Abort threading block {RWS JSON -> read data}
            if (rws_read_Thread_json.IsAlive == true)
            {
                rws_read_Thread_json.Abort();
            }
        }
        catch (Exception e)
        {
            // Destroy all
            Destroy(this);
        }
        finally
        {
            // Destroy all
            Destroy(this);
        }
    }

    // ------------------------ Threading Block { RWS - Robot Web Services (READ) -> XML } ------------------------//
    void RWS_Service_read_xml_thread_function(string ip_adr, string xml_target)
    {
        while (GlobalVariables_Main_Control.abb_irb_dt_enable_rws_xml)
        {
            // get the system resource
            Stream xml_joint = get_system_resource(ip_adr, xml_target);
            // display the system resource
            display_data(xml_joint);
        }
    }

    // ------------------------ RWS aux. function { Get System Resource } ------------------------//
    Stream get_system_resource(string host, string xml_target)
    {
        // ip address + xml address + target {joint, cartesian}
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(host + "/rw/rapid/tasks/T_ROB1/motion?resource=" + xml_target));
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

    // ------------------------ RWS XML function { Read Data } ------------------------//
    void display_data(Stream xmldata)
    {
        // XmlNode -> Initialization Document
        XmlDocument doc = new XmlDocument();
        // Load XML data
        doc.Load(xmldata);

        // Create an XmlNamespaceManager for resolving namespaces.
        XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);

        nsmgr.AddNamespace("ns", "http://www.w3.org/1999/xhtml");

        XmlNodeList optionNodes = doc.SelectNodes("//ns:li[@class='rapid-jointtarget']", nsmgr);

        // -------------------- Read State {Joint (1 - 6)} -------------------- //
        foreach (XmlNode optNode in optionNodes)
        {
            // Joint (1 - 6) -> Read RWS XML
            // optNode.SelectSingleNode("ns:span[@class='j1']", nsmgr).InnerText.ToString()

            // Joint(1 - 6) -> Write { Digital Twin, OPCUa }
            GlobalVariables_RWS_client.robotBaseRotLink_irb_joint[0] = float.Parse(optNode.SelectSingleNode("ns:span[@class='j1']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat);
            GlobalVariables_RWS_client.robotBaseRotLink_irb_joint[1] = float.Parse(optNode.SelectSingleNode("ns:span[@class='j2']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat);
            GlobalVariables_RWS_client.robotBaseRotLink_irb_joint[2] = float.Parse(optNode.SelectSingleNode("ns:span[@class='j3']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat);
            GlobalVariables_RWS_client.robotBaseRotLink_irb_joint[3] = float.Parse(optNode.SelectSingleNode("ns:span[@class='j4']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat);
            GlobalVariables_RWS_client.robotBaseRotLink_irb_joint[4] = float.Parse(optNode.SelectSingleNode("ns:span[@class='j5']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat);
            GlobalVariables_RWS_client.robotBaseRotLink_irb_joint[5] = float.Parse(optNode.SelectSingleNode("ns:span[@class='j6']", nsmgr).InnerText.ToString(), CultureInfo.InvariantCulture.NumberFormat);

            // Thread Sleep {2 ms}
            Thread.Sleep(2);
        }
    }

    // ------------------------ Threading Block { RWS - Robot Web Services (READ) -> JSON } ------------------------//
    async void RWS_Service_read_json_thread_function(string ip_adr, string json_target)
    {
        var handler = new HttpClientHandler { Credentials = new NetworkCredential("Default User", "robotics") };
        // disable the proxy, the controller is connected on same subnet as the PC 
        handler.Proxy = null;
        handler.UseProxy = false;

        // Send a request continue when complete
        using (HttpClient client = new HttpClient(handler))
        {
            // while - reading {data Joint, Cartesian}
            while (GlobalVariables_Main_Control.abb_irb_dt_enable_rws_json)
            {
                using (HttpResponseMessage response = await client.GetAsync(ip_adr + json_target))
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
                            GlobalVariables_RWS_client.robotBaseRotLink_irb_cartes[0] = Convert.ToString(service.x);
                            GlobalVariables_RWS_client.robotBaseRotLink_irb_cartes[1] = Convert.ToString(service.y);
                            GlobalVariables_RWS_client.robotBaseRotLink_irb_cartes[2] = Convert.ToString(service.z);
                            // Quaternion {q1 .. q4} -> Read RWS JSON
                            GlobalVariables_RWS_client.robotBaseRotLink_irb_cartes[3] = Convert.ToString(service.q1);
                            GlobalVariables_RWS_client.robotBaseRotLink_irb_cartes[4] = Convert.ToString(service.q2);
                            GlobalVariables_RWS_client.robotBaseRotLink_irb_cartes[5] = Convert.ToString(service.q3);
                            GlobalVariables_RWS_client.robotBaseRotLink_irb_cartes[6] = Convert.ToString(service.q4);

                            // Thread Sleep {200 ms}
                            Thread.Sleep(200);
                        }
                        catch (Exception e)
                        {
                            Debug.Log(e.Message);
                        }
                        finally
                        {
                            content.Dispose();
                        }
                    }
                }
            }
        }
    }
}
