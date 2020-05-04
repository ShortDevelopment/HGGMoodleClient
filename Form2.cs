using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }
        public const string Host = "moodle.hgg-online.de";
        private CookieContainer Cookies { get; set; } = new CookieContainer();
        private string SessionKey;
        private void button1_Click(object sender, EventArgs e)
        {
            client.Encoding = Encoding.UTF8;
            if (Login(textBox1.Text, textBox2.Text))
            {
                foreach (var kurs in GetKurse())
                {
                    textBox3.Text += kurs.FullName + "   " + kurs.Id.ToString() + Environment.NewLine;
                    textBox5.Text += $"======= \"{kurs.FullName}\" =======" + Environment.NewLine;
                    textBox5.Text += GetUpdateSince(kurs, DateTime.Now.AddDays(-7.0)) + Environment.NewLine + Environment.NewLine;
                    textBox5.Text += "==============" + Environment.NewLine;
                    webBrowser1.DocumentStream = new MemoryStream(Encoding.UTF8.GetBytes(GetCourseContents(kurs)));
                    return;
                }
            }
        }
        public WebClient client = new WebClient();
        private bool Login(string username, string password)
        {
            try
            {
                var data = client.DownloadString($"https://{Host}/moodle/blocks/exa2fa/login/");
                //textBox3.Text = data;
                ExtractSessionKey(data);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(data);
                var logintoken = doc.DocumentNode.SelectNodes("//input[@name='logintoken']")[0].GetAttributeValue("value", null);
                var querystring = new NameValueCollection();
                querystring["ajax"] = "true";
                querystring["username"] = username;
                querystring["password"] = password;
                querystring["logintoken"] = logintoken;
                querystring["anchor"] = "";
                querystring["token"] = "";
                data = UploadData($"https://{Host}/moodle/blocks/exa2fa/login/", querystring, true);
                //textBox3.Text = data;
                //textBox3.Text = JsonConvert.DeserializeObject(data).GetType().FullName;
                var obj = (JObject)JsonConvert.DeserializeObject(data);
                if (string.IsNullOrEmpty(obj["error"].ToString()))
                {
                    ExtractSessionKey(UploadData(obj["url"].ToString(), null));
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        private void ExtractSessionKey(string data)
        {
            SessionKey = Regex.Match(data, "\"sesskey\":\"(([a-z]|[0-9])*)\"", RegexOptions.IgnoreCase).Groups[1].Value;
        }
        private List<KursResult.Kurs> GetKurse()
        {
            var data = JsonRPC($"https://{Host}/moodle/lib/ajax/service.php?sesskey={SessionKey}", "[{\"index\":0,\"methodname\":\"core_course_get_enrolled_courses_by_timeline_classification\",\"args\":{\"offset\":0,\"limit\":100,\"classification\":\"all\",\"sort\":\"fullname\"}}]");
            //textBox3.Text = SessionKey + data;
            var obj = JsonConvert.DeserializeObject<List<Result<KursResult>>>(data);
            if(obj[0].Error == false)
            {
                return obj[0].Data.Kurse;                
            }
            else
            {
                throw new Exception();
            }
        }
        private int GetTimestamp(DateTime time)
        {
            return (Int32)(time.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
        private Result<UpdatesResult> GetUpdateSince(KursResult.Kurs kurs, DateTime timestamp)
        {
            var data = JsonRPC($"https://{Host}/moodle/lib/ajax/service.php?sesskey={SessionKey}", "[{\"index\":0,\"methodname\":\"core_course_get_updates_since\",\"args\":{\"courseid\":" + kurs.Id.ToString() + ",\"since\":" + GetTimestamp(timestamp) + "}}]");
            return JsonConvert.DeserializeObject<List<Result<UpdatesResult>>>(data)[0];
        }
        private string GetCourseContents(KursResult.Kurs kurs)
        {
            //var data = JsonRPC($"https://{Host}/moodle/lib/ajax/service.php?sesskey={SessionKey}", "[{\"index\":0,\"methodname\":\"core_course_get_courses\",\"args\":{ \"options\": {\"ids\":[297]} }}]");
            //return data;
            var res = "<html><head><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"><link href=\"https://moodle.hgg-online.de/moodle/theme/yui_combo.php?rollup/3.17.2/yui-moodlesimple-min.css\" rel=\"stylesheet\" type=\"text/css\"><link href=\"https://moodle.hgg-online.de/moodle/theme/styles.php/boost/1588259577_1584654151/all\" rel=\"stylesheet\" type=\"text/css\"></head><body>{0}</body></html>";
            var html = UploadData(kurs.URL, null);
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            return string.Format(res, doc.GetElementbyId("region-main").OuterHtml);
        }
        #region TypeDefinitions
        [JsonObject()]
        private struct Result<T>
        {
            [JsonProperty("error")]
            public bool Error { get; set; }
            [JsonProperty("data")]
            public T Data { get; set; }
        }
        [JsonObject()]
        public struct KursResult
        {
            [JsonProperty("courses")]
            public List<Kurs> Kurse { get; set; }
            public struct Kurs
            {
                [JsonProperty("fullname")]
                public string FullName { get; set; }
                [JsonProperty("courseimage")]
                public string Base64Image { get; set; }
                [JsonProperty("viewurl")]
                public string URL { get; set; }
                [JsonProperty("shortname")]
                public string Name { get; set; }
                [JsonProperty("id")]
                public int Id { get; set; }
            }
        }
        [JsonObject()]
        public struct UpdatesResult
        {
            [JsonProperty("instances")]
            public List<UpdateInstance> Instances { get; set; }
            public struct UpdateInstance
            {
                [JsonProperty("contextlevel")]
                public string Type { get; set; }
                [JsonProperty("id")]
                public int Id { get; set; }
                [JsonProperty("updates")]
                public List<Update> Updates { get; set; }
                [JsonObject()]
                public struct Update
                {
                    [JsonProperty("name")]
                    public string Name { get; set; }
                    [JsonProperty("timeupdated")]
                    public int Time { get; set; }
                    [JsonProperty("itemids")]
                    public List<int> Items { get; set; }
                }
            }
        }
        #endregion
        #region Request Functions
        private string UploadData(string url, NameValueCollection postdata, bool json = false)
        {
            try
            {
                string str = "";
                if (postdata != null)
                    foreach (string key in postdata.Keys)
                    {
                        if (str.Length != 0)
                        {
                            str += "&";
                        }
                        str += key + "=" + postdata.Get(key);
                    }
                HttpWebRequest request = (HttpWebRequest)
                WebRequest.Create(url); request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.Method = "POST";
                request.CookieContainer = Cookies;                
                if (json)
                {
                    request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                    request.Accept = "application/json, text/javascript, */*; q=0.01";
                }                    
                request.Host = Host;
                request.Referer = url;                
                // turn our request string into a byte stream
                byte[] postBytes = Encoding.UTF8.GetBytes(str);

                // this is important - make sure you specify type this way
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = postBytes.Length;
                Stream requestStream = request.GetRequestStream();

                // now send it
                requestStream.Write(postBytes, 0, postBytes.Length);
                requestStream.Close();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                return new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd();
            }
            catch { return null; }
        }
        private string JsonRPC(string url, string body)
        {
            try
            {   
                HttpWebRequest request = (HttpWebRequest)
                WebRequest.Create(url); request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.Method = "POST";
                request.CookieContainer = Cookies;
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                request.Accept = "application/json, text/javascript, */*; q=0.01";
                request.Host = Host;
                request.Referer = url;
                request.ContentType = "application/json";
                // turn our request string into a byte stream
                byte[] postBytes = Encoding.UTF8.GetBytes(body);
                Stream requestStream = request.GetRequestStream();

                // now send it
                requestStream.Write(postBytes, 0, postBytes.Length);
                requestStream.Close();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                return new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd();
            }
            catch { return null; }
        }
        #endregion
    }
}
