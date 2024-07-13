using DocFinanceBusinessLib.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DocFinance_WindowApplication.Forms.QbOnline
{
    public partial class QuickBookOnlineSettngs : Form
    {

        public QuickBookOnlineSettngs()
        {
            InitializeComponent();
        }

        public string ReceivedString { get; set; }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void DocFinanceFilePath(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFolderPath = folderBrowserDialog.SelectedPath;
                    textBox1.Text = selectedFolderPath;
                }
            }
        }
        private void IncomingFilePath(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFolderPath = folderBrowserDialog.SelectedPath;
                    textBox2.Text = selectedFolderPath;
                }
            }
        }

        private void QbOnlineSettngs_Load(object sender, EventArgs e)
        {
            string filePath = "Files\\QuickBooksOnlineSettings.json";
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "[]");
            }
            string existingData = File.ReadAllText(filePath);
            List<dynamic> locationsList = JsonConvert.DeserializeObject<List<dynamic>>(existingData);
            if (locationsList.Count > 0)
            {
                var existingLocation = locationsList.FirstOrDefault(loc => loc.name == ReceivedString);

                textBox1.Text = existingLocation?.data.QbOnlineDocFinance ?? string.Empty;
                textBox2.Text = existingLocation?.data.QbOnlineIncomingFile ?? string.Empty;
                textBox3.Text = existingLocation?.data.TokenFilePath ?? string.Empty;
                textBox4.Text = existingLocation?.data.ClientId ?? string.Empty;
                textBox5.Text = existingLocation?.data.ClientSecret ?? string.Empty;
                textBox6.Text = existingLocation?.data.Scope ?? string.Empty;
                if (existingLocation.data.Auomation == true)
                {
                    checkBox1.Checked = true;
                }
                else
                {
                    checkBox1.Checked = false;
                }
            }
        }

        private void TokenFileSelection(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.CheckFileExists = true;
            openFileDialog.AddExtension = true;
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "JSON files (*.json)|*.json";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                this.textBox3.Text = openFileDialog.FileName;

            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "" || textBox2.Text == "" || textBox3.Text == "" || textBox4.Text == "" || textBox5.Text == "" || textBox6.Text == "")
            {
                ErrorLog.WriteWarnLog("Enter file path!!");
                MessageBox.Show("Please enter paths first.");
            }
            else
            {
                bool automation;
                string filePath = "Files\\QuickBooksOnlineSettings.json";
                string globalPath = "C:\\DocFinance\\Files\\QuickBooksOnlineSettings.json";
                if (checkBox1.Checked)
                {
                    automation = true;
                }
                else
                {
                    automation = false;
                }
                try
                {
                    if (!File.Exists(filePath))
                    {
                        File.WriteAllText(filePath, "[]");
                    }

                    string existingData = File.ReadAllText(filePath);
                    List<dynamic> locationsList = JsonConvert.DeserializeObject<List<dynamic>>(existingData);

                    if (locationsList.Count > 0)
                    {
                        var existingLocation = locationsList.FirstOrDefault(loc => loc.name == ReceivedString);

                        if (existingLocation != null)
                        {
                            existingLocation.data.QbOnlineDocFinance = textBox1.Text;
                            existingLocation.data.QbOnlineIncomingFile = textBox2.Text;
                            existingLocation.data.TokenFilePath = textBox3.Text;
                            existingLocation.data.ClientId = textBox4.Text;
                            existingLocation.data.ClientSecret = textBox5.Text;
                            existingLocation.data.Scope = textBox6.Text;
                            existingLocation.data.Auomation = automation;
                        }
                        else
                        {
                            var newLocation = new
                            {
                                name = ReceivedString,
                                data = new
                                {
                                    QbOnlineDocFinance = textBox1.Text,
                                    QbOnlineIncomingFile = textBox2.Text,
                                    TokenFilePath = textBox3.Text,
                                    ClientId = textBox4.Text,
                                    ClientSecret = textBox5.Text,
                                    Scope = textBox6.Text,
                                    Automation = automation

                                }
                            };

                            locationsList.Add(newLocation);
                        }
                    }
                    else
                    {

                        var newLocation = new
                        {
                            name = ReceivedString,
                            data = new
                            {
                                QbOnlineDocFinance = textBox1.Text,
                                QbOnlineIncomingFile = textBox2.Text,
                                TokenFilePath = textBox3.Text,
                                ClientId = textBox4.Text,
                                ClientSecret = textBox5.Text,
                                Scope = textBox6.Text,
                                Automatio = automation
                            }
                        };

                        locationsList.Add(newLocation);

                    }
                    string updatedJson = JsonConvert.SerializeObject(locationsList);
                    File.WriteAllText(filePath, updatedJson);
                    if (!Directory.Exists(Path.GetDirectoryName(globalPath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(globalPath));
                    }
                    File.WriteAllText(globalPath, updatedJson);
                    MessageBox.Show("Paths saved successfully!");
                    ErrorLog.WriteInfoLog("Required path is saved successfully.");
                    this.Close();


                }
                catch (Exception ex)
                {
                    ErrorLog.WriteErrorLog($"Oops! Something went wrong: {ex.Message}");
                }
            }
        }

        private void ClientId(object sender, EventArgs e)
        {

        }

        private void ClientSecret(object sender, EventArgs e)
        {

        }

        private async void Scope(object sender, EventArgs e)
        {

        }
        private async void testConnectionBtn_Click(object sender, EventArgs e)
        {
            string clientId = "";
            string clientSecret = "";
            string scope = "";
            string filePath = "Files\\QuickBooksOnlineSettings.json";
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "[]");
            }
            string existingData = File.ReadAllText(filePath);
            List<dynamic> locationsList = JsonConvert.DeserializeObject<List<dynamic>>(existingData);
            if (locationsList.Count > 0)
            {
                var existingLocation = locationsList.FirstOrDefault(loc => loc.name == ReceivedString);
                clientId = existingLocation.data.ClientId;
                clientSecret = existingLocation.data.ClientSecret;
                scope = existingLocation.data.Scope;
            }

            using (var client = new HttpClient())

                {

                    var parameters = new Dictionary<string, string>

                     {
                         
                             { "clientId", clientId },

                             { "clientSecret", clientSecret },

                             { "scope", scope }

                      };

                    var content = new FormUrlEncodedContent(parameters);


                    var response = await client.PostAsync("https://docfinance.ginilytics.org:5003/Authorization/QuickBooks", content);

                    if (response.IsSuccessStatusCode)

                    {

                        // Request successful

                        var responseContent = await response.Content.ReadAsStringAsync();

                        Console.WriteLine(responseContent); 

                    }

                    else

                    {

                        // Handle the error

                        Console.WriteLine("Request failed");

                    }

                }

            }
        }
}



