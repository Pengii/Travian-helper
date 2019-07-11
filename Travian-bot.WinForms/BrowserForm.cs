// Copyright © 2010-2015 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp.MinimalExample.WinForms.Controls;
using CefSharp.MinimalExample.WinForms.Properties;
using CefSharp.WinForms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CefSharp.MinimalExample.WinForms
{
    public partial class BrowserForm : Form
    {
        private readonly ChromiumWebBrowser _browser;
        private string _subDomain = "unl4";
        private bool _started = false;
        private bool _autoBuild = Settings.Default.AutoBuild;
        private bool _loggedIn = false;
        private bool _dataGathered = false;
        private int _maximumLevel = 30;
        private int _serverSpeed = 80000;

        public BrowserForm()
        {
            InitializeComponent();

            Text = "Travian bot";

            textBoxUser.Text = Settings.Default.Username;
            textBoxPw.Text = Settings.Default.Password;
            autoBuild.Checked = Settings.Default.AutoBuild;

            //WindowState = FormWindowState.Maximized;

            textBoxLogs.Text += TraceLine("---------- Travian bot v0.1a ----------");

            _browser = new ChromiumWebBrowser("https://" + _subDomain + ".ttwars.com/dorf1.php")
            {
                Dock = DockStyle.Fill,
            };
            
            LifespanHandler lifeSpan = new LifespanHandler();
            _browser.LifeSpanHandler = lifeSpan;
            lifeSpan.PopupRequest += LifeSpanPopupRequest;

            toolStripContainer.ContentPanel.Controls.Add(_browser);

            _browser.IsBrowserInitializedChanged += OnIsBrowserInitializedChanged;
            _browser.LoadingStateChanged += OnLoadingStateChanged;
            _browser.ConsoleMessage += OnBrowserConsoleMessage;
            _browser.StatusMessage += OnBrowserStatusMessage;
            _browser.TitleChanged += OnBrowserTitleChanged;
            _browser.AddressChanged += OnBrowserAddressChanged;

            textBoxLogs.TextChanged += OnTextChanged;

            var bitness = Environment.Is64BitProcess ? "x64" : "x86";
            var version =
                $"Chromium: {Cef.ChromiumVersion}, CEF: {Cef.CefVersion}, CefSharp: {Cef.CefSharpVersion}, Environment: {bitness}";
            DisplayOutput(version);
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            textBoxLogs.SelectionStart = textBoxLogs.Text.Length;
            textBoxLogs.ScrollToCaret();
        }

        public sealed override string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        private void LifeSpanPopupRequest(string obj)
        {
            this.PopupRequestHandler(obj);
        }

        private void PopupRequestHandler(string url)
        {
            //open pop up in local browser form
            _browser.Load(url);
        }

        private void OnIsBrowserInitializedChanged(object sender, IsBrowserInitializedChangedEventArgs e)
        {
            if(e.IsBrowserInitialized)
            {
                var b = ((ChromiumWebBrowser)sender);

                this.InvokeOnUiThreadIfRequired(() => b.Focus());
            }
        }

        private void OnBrowserConsoleMessage(object sender, ConsoleMessageEventArgs args)
        {
            DisplayOutput(string.Format("Line: {0}, Source: {1}, Message: {2}", args.Line, args.Source, args.Message));
        }

        private void OnBrowserStatusMessage(object sender, StatusMessageEventArgs args)
        {
            //this.InvokeOnUiThreadIfRequired(() => linkLabelLogs.Text = args.Value);
        }

        private void OnLoadingStateChanged(object sender, LoadingStateChangedEventArgs args)
        {
            //Wait for the page to finish loading

            if (args.IsLoading == false)
            {
                StartAsync();
            }

            //SetCanGoBack(args.CanGoBack);
            //SetCanGoForward(args.CanGoForward);

            //this.InvokeOnUiThreadIfRequired(() => SetIsLoading(!args.CanReload));
        }

        protected void StartAsync()
        {
            _browser.ExecuteScriptAsync("document.forms[\"login\"].elements[\"user\"].value=\"Saisama\";");
            _browser.ExecuteScriptAsync("document.forms[\"login\"].elements[\"pw\"].value=\"a72b84zx\";");
            _browser.ExecuteScriptAsync("document.forms[\"login\"].submit();");

            if (_autoBuild)
            {
                var upgrade = new Upgrade(_browser, _subDomain);

                var file = File.OpenText(@"resources.json").ReadToEnd();
                var rsc = JsonConvert.DeserializeObject<List<ResourceField>>(file);

                int index = 0;
                while (rsc[index].ResLevel < _maximumLevel)
                {
                    if (index < 18)
                    {
                        upgrade.UpgradeResource();
                        if (rsc[index].ResLevel == _maximumLevel - 1)
                        {
                            upgrade.UpgradeResource();
                            index++;
                        }
                    }
                }
                textBoxLogs.Text += TraceLine("All Maximum Level reached!");
            }
            else
            {
                textBoxLogs.Text += TraceLine("Auto build is disabled! Started to creating farm list...");
                _browser.Load("https://" + _subDomain+ ".ttwars.com/build.php?tt=99&id=39");
                _browser.ExecuteScriptAsync("document.getElementsByClassName(\"openedClosedSwitch switchClosed\")[0].click();\n");
                _browser.ExecuteScriptAsync("document.getElementById(\"raidListMarkAll31\").click();\n");
                _browser.ExecuteScriptAsync("var aTags = document.getElementsByTagName(\"div\");\r\n                var searchText = \"Start raid\";\r\n                var found;\r\n\r\n                for (var i = 0; i < aTags.length; i++)\r\n                {\r\n                    if (aTags[i].textContent == searchText)\r\n                    {\r\n                        found = aTags[i];\r\n                        break;\r\n                    }\r\n                }\r\n                found.click(); ");
            }

        }

        public async void CollectData()
        {
            string html = await _browser.GetSourceAsync();

            textBoxLogs.Text += TraceLine("Getting source...");

            var parseHtml = new ParseHtml(TraceLine, html); // Extracts some useful data

            List<ResourceField> res = parseHtml.ResInfo();

            foreach (var element in res)
            {
                textBoxLogs.Text +=
                    TraceLine(element.Type + " - Index: " + element.Index + " - Level: " + element.ResLevel);
            }

            string strResourceJson = JsonConvert.SerializeObject(res, Formatting.Indented);
            var settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;

            string fileName = @"resources.json";
            if (File.Exists(fileName))
                File.Delete(fileName);
            StreamWriter sw = new StreamWriter(@"resources.json");
            sw.Write(strResourceJson);
            sw.Dispose();
        }



        private void OnBrowserTitleChanged(object sender, TitleChangedEventArgs args)
        {
            this.InvokeOnUiThreadIfRequired(() => Text = args.Title);
        }

        private void OnBrowserAddressChanged(object sender, AddressChangedEventArgs args)
        {
            this.InvokeOnUiThreadIfRequired(() => textBoxLogs.Text += TraceLine("Loading " + args.Address));
        }

        // Adds time prefix to a message
        private static string TraceLine(object log)
        {
            return $"\n[{DateTime.Now:HH:mm:ss}]    {log}" + Environment.NewLine;
        }

        private void SetCanGoBack(bool canGoBack)
        {
            //this.InvokeOnUiThreadIfRequired(() => backButton.Enabled = canGoBack);
        }
        
        private void SetCanGoForward(bool canGoForward)
        {
            //this.InvokeOnUiThreadIfRequired(() => forwardButton.Enabled = canGoForward);
        }

        private void SetIsLoading(bool isLoading)
        {
            //goButton.Text = isLoading ?
            //    "Stop" :
            //    "Go";
            //goButton.Image = isLoading ?
            //    Properties.Resources.nav_plain_red :
            //    Properties.Resources.nav_plain_green;

            //HandleToolStripLayout();
        }

        public void DisplayOutput(string output)
        {
            //this.InvokeOnUiThreadIfRequired(() => outputLabel.Text = output);
        }

        private void HandleToolStripLayout(object sender, LayoutEventArgs e)
        {
            HandleToolStripLayout();
        }

        private void HandleToolStripLayout()
        {
            
        }

        private void ExitMenuItemClick(object sender, EventArgs e)
        {
            _browser.Dispose();
            Cef.Shutdown();
            Close();
        }

        private void GoButtonClick(object sender, EventArgs e)
        {
            //LoadUrl(urlTextBox.Text);
        }

        private void BackButtonClick(object sender, EventArgs e)
        {
            _browser.Back();
        }

        private void ForwardButtonClick(object sender, EventArgs e)
        {
            _browser.Forward();
        }

        private void UrlTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            //if (e.KeyCode != Keys.Enter)
            //{
            //    return;
            //}

            //LoadUrl(urlTextBox.Text);
        }

        private void LoadUrl(string url)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
            {
                _browser.Load(url);
            }
        }

        private void ShowDevToolsMenuItemClick(object sender, EventArgs e)
        {
            _browser.ShowDevTools();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            Settings.Default["Username"] = textBoxUser.Text;
            Settings.Default["Password"] = textBoxPw.Text;
            Settings.Default["AutoBuild"] = autoBuild.Checked;

            Settings.Default.Save();
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            //var login = new Login(_browser);
            //login.LoginUser();




            //Task task = new Task(StartAsync);
            //task.Start();

            //textBoxLogs.Text += TraceLine("Collecting data...");
            //CollectData();
            //_dataGathered = true;

            _started = true;
        }
    }
}
