using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using HtmlAgilityPack;

namespace Invisible_Man
{
    class ServerInformation
    {
        public string serverName { get; set; }
        public string countryImage { get; set; }
        public string serverAddress { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        
        public ServerInformation(string serverName, string countryImage, string serverAddress, string username, string password)
        {
            this.serverName = serverName;
            this.countryImage = countryImage;
            this.serverAddress = serverAddress;
            this.username = username;
            this.password = password;
        }
    }

    class InvisibleManCore
    {
        // Initialize
        public static int bundleIdentifier = 1;
        public static int index = 0;
        public static bool isSelectServer = false;

        private static string path = Directory.GetCurrentDirectory();

        // Available servers
        public static List<ServerInformation> serverInformations = new List<ServerInformation> 
        {
            new ServerInformation("VPN Account - 1", "United-Kingdom.png", "Account address","username","password"),
            new ServerInformation("VPN Account - 2", "Canada.png", "Account address","username","password"),
            new ServerInformation("VPN Account - 3", "Singapore.png", "Account address","username","password")
        };

        /// <summary>
        /// Save the selected server index in a file
        /// </summary>
        public void SaveServerInFile()
        {
            try
            {
                // Get and encode data
                string information = index.ToString();
                byte[] dataByte = System.Text.Encoding.UTF8.GetBytes(information);
                string encodedData = System.Convert.ToBase64String(dataByte);

                // Save data in file
                string serverInfoPath = path + "\\data";
                
                if (!Directory.Exists(serverInfoPath))
                    Directory.CreateDirectory(serverInfoPath);

                File.WriteAllLines(serverInfoPath + "\\ServerInfo.inm", new string[1] { encodedData });
            }
            catch (Exception)
            {
                // Do nothing!
            }
        }

        /// <summary>
        /// Load the selected server index from the file
        /// </summary>
        /// <returns>Load successfully or not</returns>
        public bool LoadServerFromFile()
        {
            try
            {
                string serverInfoPath = path + "\\data\\ServerInfo.inm";

                if (File.Exists(serverInfoPath))
                {
                    string[] encodedData = File.ReadAllLines(serverInfoPath);

                    if (encodedData.Length != 1) // File is corrupt
                    {
                        return false;
                    }
                    else
                    {
                        // Convert encodedData to byte
                        byte[] dataByte = System.Convert.FromBase64String(encodedData[0]);

                        // Convert dataByte to string
                        int information = Convert.ToInt32(System.Text.Encoding.UTF8.GetString(dataByte));

                        if (information >= serverInformations.Count) // File is corrupt
                        {
                            return false;
                        }
                        else
                        {
                            index = information;
                            return true;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            catch(Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Use Rasdial to connect to VPN account
        /// </summary>
        /// <returns>The result of connecting</returns>
        private string Connect()
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe");

                string rootDrive = Path.GetPathRoot(path);
                processStartInfo.Arguments = "/C " + rootDrive.Split('\\')[0] + " & cd \"" + path + "\\data\" & rasdial \"InvisibleManVPN\" " 
                    + serverInformations[index].username + " " + serverInformations[index].password + " /phonebook:Connection.inm";
                processStartInfo.RedirectStandardError = true;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.UseShellExecute = false;
                processStartInfo.CreateNoWindow = true;

                Process process = Process.Start(processStartInfo);
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (output.Length != 0)
                {
                    stringBuilder.Append(output);
                    return output;
                }
                else
                {
                    return "CommandFailed";
                }
            }
            catch (Exception)
            {
                return "CommandFailed";
            }
        }

        /// <summary>
        /// Run the disconnect instruction and disconnect from VPN account
        /// </summary>
        private void Disconnect()
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe");

                string rootDrive = Path.GetPathRoot(path);
                processStartInfo.Arguments = "/C " + rootDrive.Split('\\')[0] + " & cd \"" + path + "\\data"
                    + "\" & rasdial \"InvisibleManVPN\" /disconnect";
                processStartInfo.RedirectStandardError = true;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.UseShellExecute = false;
                processStartInfo.CreateNoWindow = true;

                Process process = Process.Start(processStartInfo);
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }
            catch (Exception)
            {
                // Do nothing!
            }
        }

        /// <summary>
        /// Set selected server information
        /// </summary>
        /// <param name="serverIndex">The index of selected server</param>
        public void SetServer(int serverIndex)
        {
            index = serverIndex;
            SaveServerInFile();
            isSelectServer = true;
        }

        /// <summary>
        /// Connect to VPN account
        /// </summary>
        /// <returns>Connecting successful or not</returns>
        public bool ConnectVPN()
        {
            // Make a connection file
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("[InvisibleManVPN]");
            stringBuilder.AppendLine("MEDIA=rastapi");
            stringBuilder.AppendLine("Port=VPN2-0");
            stringBuilder.AppendLine("Device=WAN Miniport (IKEv2)");
            stringBuilder.AppendLine("DEVICE=vpn");
            stringBuilder.AppendLine("PhoneNumber=" + serverInformations[index].serverAddress);

            // Save connection in Data folder
            try
            {
                string vpnFolder = path + "\\data";

                if (!Directory.Exists(vpnFolder))
                    Directory.CreateDirectory(vpnFolder);

                File.WriteAllText(vpnFolder + "\\Connection.inm", stringBuilder.ToString());

                // Connect to VPN
                string output = Connect();

                if(output == "CommandFailed")
                {
                    return false;
                }
                else
                {
                    string[] stateLines = output.Split('\n');
                    string state = stateLines[stateLines.Length - 2];

                    if (state == "Command completed successfully.")
                        return true;
                    else
                        return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Disconnect from VPN account
        /// </summary>
        public void DisconnectVPN()
        {
            Disconnect();
        }

        /// <summary>
        /// Check the server to update VPN
        /// </summary>
        public async Task<string> CheckForUpdatesVPN()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                var url = "https://invisiblemanvpn.github.io/index.html";
                HttpClient httpClient = new HttpClient();
                string html = await httpClient.GetStringAsync(url);

                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);
                List<HtmlNode> htmlNodeList = htmlDocument.DocumentNode.SelectNodes("//meta")
                    .Where(node => node.GetAttributeValue("name", "")
                    .Equals("BundleIdentifier")).ToList();
                HtmlAttribute contentHtmlAttribute = htmlNodeList[0].Attributes["content"];
                int bundleIdentifierContent = Convert.ToInt32(contentHtmlAttribute.Value);

                if (bundleIdentifierContent > bundleIdentifier) // If new update available on the site
                    return "NewUpdate";
                else
                    return "AlreadyHave";
            }
            catch(Exception)
            {
                return "FailedToConnect";
            }
        }
    }
}
