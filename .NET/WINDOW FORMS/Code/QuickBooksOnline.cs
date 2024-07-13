using DocFinanceBusinessLib.BusinessLogic;
using DocFinanceBusinessLib.Models;
using Intuit.Ipp.Core;
using Intuit.Ipp.Data;
using Intuit.Ipp.OAuth2PlatformClient;
using Intuit.Ipp.QueryFilter;
using Intuit.Ipp.Security;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Formatting = Newtonsoft.Json.Formatting;

namespace DocFinance_WindowApplication
{
    public partial class QuickBooksOnline : Form
    {
        public QuickBooksOnline()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
        }
        bool flag = false;
        public string ReceivedString { get; set; }
        string filePath = "Files\\QuickBooksOnlineSettings.json";
        public string Token = "";
        string tokenFilePath;
        dynamic tokenDetails;
        string clientId;
        string clientSecret;
        public string redirectUrl = "https://localhost:7175/Authorization/CallQuickbooks";
        public string environment = "sandbox";
        public string realmId = "";
        public string fullFileName = "";
        private void Home_Load(object sender, EventArgs e)
        {

            try
            {
                string existingData = File.ReadAllText(filePath);
                List<dynamic> locationsList = JsonConvert.DeserializeObject<List<dynamic>>(existingData);
                dynamic existingLocation = locationsList.FirstOrDefault(loc => loc.name == ReceivedString);
                if (existingLocation != null)
                {
                    clientId = existingLocation.data.ClientId;
                    clientSecret = existingLocation.data.ClientSecret;
                    fileLocation.Text = existingLocation.data.QbOnlineDocFinance;
                    tokenFilePath = existingLocation.data.TokenFilePath;
                    this.tokenDetails = File.ReadAllText(tokenFilePath);
                    var token = JsonConvert.DeserializeObject<dynamic>(this.tokenDetails);

                    Token = token.access_token;
                    realmId = token.realmId;
                }
                else
                {
                    Console.WriteLine("Error: Location not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

        }
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            int centerX = (Screen.PrimaryScreen.WorkingArea.Width - panel1.Width) / 2;
            int centerY = (Screen.PrimaryScreen.WorkingArea.Height - panel1.Height) / 2;

            panel1.Location = new System.Drawing.Point(centerX, centerY);
        }

        private void incomingButton_Click(object sender, EventArgs e)
        {

            if (IncomingComboBox.SelectedIndex != -1)
            {
                incomingButton.Text = "Running...";

                progressBar1.Visible = true;
                progressBar1.Value = 10;
                if (IncomingComboBox.SelectedIndex != -1)
                {


                    string existingData = File.ReadAllText(filePath);
                    List<dynamic> locationsList = JsonConvert.DeserializeObject<List<dynamic>>(existingData);
                    var existingLocation = locationsList.FirstOrDefault(loc => loc.name == ReceivedString);
                    progressBar1.Value = 20;
                    if (existingLocation.data.TokenFilePath == "" || existingLocation.data.QbOnlineDocFinance == "" || existingLocation.data.QbOnlineIncomingFile == "")
                    {
                        MessageBox.Show("Please check the files path in the settings there is any missing path!");
                        progressBar1.Value = 0;
                        incomingButton.BackColor = Color.FromArgb(30, 150, 80);
                        incomingButton.ForeColor = Color.White;
                        incomingButton.Text = "Run";
                        return;
                    }

                    string QbwPath = "";
                    if (IncomingComboBox.Text == "Charts of Accounts")
                    {
                        progressBar1.Value = 30;
                        this.QBOChartsOfAccount();
                        incomingButton.Text = "Run";
                        label2.Visible = true;
                        label3.Visible = true;
                        label4.Visible = true;
                        label4.Text = fullFileName;
                        IncomingFilePath.Text = existingLocation.data.QbOnlineIncomingFile;
                        IncomingFilePath.Visible = true;
                       
                    }
                    else if (IncomingComboBox.Text == "Due Dates")
                    {
                        progressBar1.Value = 30;
                        this.QBODueDates();
                        incomingButton.Text = "Run";
                        label2.Visible = true;
                        label3.Visible = true;
                        label4.Visible = true;
                        label4.Text = fullFileName;
                        IncomingFilePath.Text = existingLocation.data.QbOnlineIncomingFile;
                        IncomingFilePath.Visible = true;

                    }

                    else if (IncomingComboBox.Text == "")
                    {
                        progressBar1.Value = 0;
                        incomingButton.BackColor = Color.Gray;
                        incomingButton.ForeColor = Color.White;
                        incomingButton.Text = "Run";
                        MessageBox.Show("Please select any category!");
                        incomingButton.Text = "Run";
                    }
                    else
                    {
                        progressBar1.Value = 0;
                        incomingButton.BackColor = Color.Gray;
                        incomingButton.ForeColor = Color.White;
                        incomingButton.Text = "Run";
                        MessageBox.Show("Somethings went Wrong !");
                        incomingButton.Text = "Run";
                    }


                }
                else
                {
                    progressBar1.Value = 0;
                    incomingButton.BackColor = Color.Gray;
                    incomingButton.ForeColor = Color.White;
                    incomingButton.Text = "Run";
                    MessageBox.Show("Please select any category.");
                }
            }
            else
            {
                MessageBox.Show("Please select Category First !");
            }
        }

        private void outgoingButton_Click(object sender, EventArgs e)
        {

        }

        private void IncomingComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IncomingComboBox.SelectedIndex != -1)
            {
                incomingButton.BackColor = Color.FromArgb(30, 150, 80);
            }
        }

        private void OutgoingComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private async void QBOChartsOfAccount()
        {

            progressBar1.Value = 50;
            var res = await GetTokenDetails(tokenDetails);

            if (flag == true)
            {
                Token = res.access_token;
            }
            OAuth2RequestValidator oauthValidator = new OAuth2RequestValidator(Token);

            ServiceContext serviceContext = new ServiceContext(realmId, IntuitServicesType.QBO, oauthValidator);
            serviceContext.IppConfiguration.MinorVersion.Qbo = "23";
            serviceContext.IppConfiguration.BaseUrl.Qbo = "https://sandbox-quickbooks.api.intuit.com/";
            progressBar1.Value = 60;
            QueryService<Account> querySvc1 = new QueryService<Account>(serviceContext);
            List<Account> accounts = querySvc1.ExecuteIdsQuery("SELECT * FROM Account").ToList();

            progressBar1.Value = 65;
            QueryService<Intuit.Ipp.Data.Vendor> querySvc2 = new QueryService<Intuit.Ipp.Data.Vendor>(serviceContext);
            List<Intuit.Ipp.Data.Vendor> vendors = querySvc2.ExecuteIdsQuery("SELECT * FROM Vendor").ToList();
            progressBar1.Value = 75;
            QueryService<Intuit.Ipp.Data.Customer> querySvc3 = new QueryService<Intuit.Ipp.Data.Customer>(serviceContext);
            List<Intuit.Ipp.Data.Customer> customers = querySvc3.ExecuteIdsQuery("SELECT * FROM Customer").ToList();
            progressBar1.Value = 80;
            QueryService<CompanyInfo> querySvc4 = new QueryService<CompanyInfo>(serviceContext);
            CompanyInfo companyInfo = querySvc4.ExecuteIdsQuery("SELECT * FROM CompanyInfo").FirstOrDefault();

            progressBar1.Value = 90;

            string jsonAccounts = JsonConvert.SerializeObject(accounts, new JsonSerializerSettings());
            string jsonVendors = JsonConvert.SerializeObject(vendors, new JsonSerializerSettings());
            string jsonCustomers = JsonConvert.SerializeObject(customers, new JsonSerializerSettings());
            string jsonCompanyInfo = JsonConvert.SerializeObject(companyInfo, new JsonSerializerSettings());
            QBOChartsofAccounts<string> QBOChartsofaccount = new QBOChartsofAccounts<string>();

            string result = QBOChartsofaccount.GetDocFinanceStandardSerializedstring(jsonAccounts, jsonVendors, jsonCustomers, jsonCompanyInfo, false);

            string existingData = File.ReadAllText(filePath);
            List<dynamic> locationsList = JsonConvert.DeserializeObject<List<dynamic>>(existingData);
            dynamic existingLocation = locationsList.FirstOrDefault(loc => loc.name == ReceivedString);

            string formattedResult = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(result), Newtonsoft.Json.Formatting.Indented);
            string path = existingLocation.data.QbOnlineIncomingFile;
            fullFileName = "QBO-ChartsOfAccounts" + DateTime.Now.GetDateTimeFormats()[35].Replace(":", "_").Replace(" ", "") + ".json";
            System.IO.File.WriteAllText(path + "\\" + fullFileName, formattedResult);
            progressBar1.Value = 100;
            FilePreview.Text = formattedResult;
            progressBar1.Visible = false;
            MessageBox.Show("File saved.");
        }

    



            // Instantiate object
            // environment is “sandbox” or “production”


        
        public async Task<TokenModel> GetTokenDetails(string obj)
        {
            TokenModel tokenModel = JsonConvert.DeserializeObject<TokenModel>(obj);
            DateTime currentTime = DateTime.Now;
            DateTime expiryTime = tokenModel.current_time.AddSeconds(3550);
            if (currentTime >= expiryTime)
            {
                var res = await UpdateToken(tokenModel);
                TokenModel result = JsonConvert.DeserializeObject<TokenModel>(res.Json.ToString());
                return result;
                //    return updatedToken.Result.AccessToken.ToString();
            }
            else
            {
                return tokenModel;
            }
        }

        public async Task<TokenResponse> UpdateToken(TokenModel tokenModel)
        {
            try
            {
                string refreshToken = tokenModel.refresh_token;
                OAuth2Client oauthClient = new OAuth2Client(clientId, clientSecret, redirectUrl, environment);
                var tokenResp = await oauthClient.RefreshTokenAsync(refreshToken);
                CultureInfo culture = new CultureInfo("en-US");
                tokenResp.Json.Add("current_time", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss", culture));
                tokenResp.Json.Add("realmId", realmId);
                string tokenString = tokenResp.Json.ToString();
                File.WriteAllText(tokenFilePath, tokenString);
                flag = true;
                return tokenResp;
            }
            catch (Exception ex)
            {
                return null;
            }
         }
        private async void QBODueDates()
         {
            progressBar1.Value = 50;
            // Replace with your actual access token
            //string realmId = "4620816365354145020"; // Replace with your QuickBooks Company ID

            //// API endpoint to fetch invoices
            //string apiUrl = $"https://sandbox-quickbooks.api.intuit.com/v3/company/{realmId}/query?query=SELECT * FROM Invoice";

            //// Create the HTTP request
            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl);
            //request.Method = "GET";
            //request.Headers.Add("Authorization", "Bearer " + Token);

            //try
            //{
            //    // Get the response
            //    using (WebResponse response = request.GetResponse())
            //    {
            //        using (Stream stream = response.GetResponseStream())
            //        {
            //            using (StreamReader reader = new StreamReader(stream))
            //            {
            //                string xmlResponse = reader.ReadToEnd();

            //                // Convert XML to JSON
            //                string jsonResponse = ConvertXmlToJson(xmlResponse);

            //                // Save JSON data to a file
            //                File.WriteAllText("invoices.json", jsonResponse);
            //                Console.WriteLine("Invoice data saved to invoices.json");

            //                File.WriteAllText("C:\\Users\\GL-Jitender\\Desktop\\Data\\Inco\\DueDates.json", jsonResponse);
            //                MessageBox.Show("Invoice data saved to invoices.json");
            //            }
            //        }
            //    }
            //}
            //catch (WebException ex)
            //{
            //    // Handle any exceptions
            //    Console.WriteLine("Error: " + ex.Message);
            //}
            var res = await GetTokenDetails(tokenDetails);

            if (flag == true)
            {
                Token = res.access_token;
            }
            OAuth2RequestValidator oauthValidator = new OAuth2RequestValidator(Token);

            ServiceContext serviceContext = new ServiceContext(realmId, IntuitServicesType.QBO, oauthValidator);
            serviceContext.IppConfiguration.MinorVersion.Qbo = "23";
            serviceContext.IppConfiguration.BaseUrl.Qbo = "https://sandbox-quickbooks.api.intuit.com/"; //This is sandbox Url. Change to Prod Url if you are using production
            progressBar1.Value = 60;
            QueryService<Invoice> querySvc = new QueryService<Invoice>(serviceContext);
            QueryService<CompanyInfo> companyquerySvc = new QueryService<CompanyInfo>(serviceContext);
            //Invoice companyInfo = querySvc.ExecuteIdsQuery("select * from Invoice ").FirstOrDefault();
            progressBar1.Value = 70;

            List<Invoice> invoiceInfo = querySvc.ExecuteIdsQuery("SELECT * FROM Invoice").ToList();
            CompanyInfo companyInfo = companyquerySvc.ExecuteIdsQuery("select * from CompanyInfo").FirstOrDefault();
            //string output = JsonConvert.SerializeObject(invoiceInfo, new JsonSerializerSettings());
            //File.WriteAllText("C:\\Users\\GL-Jitender\\Desktop\\Data\\Inco\\DueDatesonline .json", output);
            progressBar1.Value = 80;

            string invoiceJson = JsonConvert.SerializeObject(invoiceInfo, new JsonSerializerSettings());
            string companyJson = JsonConvert.SerializeObject(companyInfo, new JsonSerializerSettings());
            
            QBOInvoiceLogic<string> qBOInvoiceLogic = new QBOInvoiceLogic<string>();
            string SerializedJson = qBOInvoiceLogic.GetDocFinanceStandardSerializedstring(invoiceJson, companyJson, false);
            string formattedResult = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(SerializedJson), Formatting.Indented);
            string existingData = File.ReadAllText(filePath);
            progressBar1.Value = 90;

            List<dynamic> locationsList = JsonConvert.DeserializeObject<List<dynamic>>(existingData);
            dynamic existingLocation = locationsList.FirstOrDefault(loc => loc.name == ReceivedString);
            string path = existingLocation.data.QbOnlineIncomingFile;
            fullFileName = "QBO-DueDates" + DateTime.Now.GetDateTimeFormats()[35].Replace(":", "_").Replace(" ", "") + ".json";
            System.IO.File.WriteAllText(path + "\\" + fullFileName, formattedResult);
            FilePreview.Text = formattedResult;
            progressBar1.Value = 100;
            progressBar1.Visible = false;
            MessageBox.Show("File saved.");




        }
        static string ConvertXmlToJson(string xml)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            string json = JsonConvert.SerializeXmlNode(xmlDoc, Newtonsoft.Json.Formatting.Indented);
            return json;
        }



        public async void SelectCompany()
        {
            List<Company> companyList = await GetCompanyListAsync();

            foreach (var company in companyList)
            {
                Console.WriteLine($"{company.CompanyName} - {company.Id}");
            }

            // Step 3: User Selection
            Console.Write("Enter the Company ID you want to interact with: ");
            string selectedCompanyId = Console.ReadLine();

            // Step 4: API Call with Selected Company ID
            Company selectedCompany = await GetCompanyByIdAsync(selectedCompanyId);

        }
        static async Task<List<Company>> GetCompanyListAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                // Replace with your actual QuickBooks Online API endpoint for getting company list
                string apiUrlForCompanyList = "https://quickbooks.api.url/v3/company";


                HttpResponseMessage response = await client.GetAsync(apiUrlForCompanyList);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Company>>(jsonResponse);
                }
                else
                {
                    throw new Exception($"Error getting company list. Status Code: {response.StatusCode}");
                }
            }
        }

        static async Task<Company> GetCompanyByIdAsync(string companyId)
        {
            using (HttpClient client = new HttpClient())
            {
                string apiUrl = $"https://api.quickbooks.com/v3/company/{companyId}";
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Company>(jsonResponse);
                }
                else
                {
                    throw new Exception($"Error getting company by ID. Status Code: {response.StatusCode}");
                }
            }
        }

        private void FilePreview_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

