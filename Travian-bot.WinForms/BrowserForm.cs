// Copyright © 2010-2015 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp.MinimalExample.WinForms.Controls;
using CefSharp.WinForms;

namespace CefSharp.MinimalExample.WinForms
{
    public partial class BrowserForm : Form
    {
        private readonly ChromiumWebBrowser browser;
        private string subDomain = "unl4";

        public BrowserForm()
        {
            InitializeComponent();

            Text = "Travian bot";
            //WindowState = FormWindowState.Maximized;

            textBoxLogs.Text += TraceLine("---------- Travian bot v0.1a ----------");

            browser = new ChromiumWebBrowser("https://" + subDomain + ".ttwars.com/dorf1.php")
            {
                Dock = DockStyle.Fill,
            };
            
            LifespanHandler lifeSpan = new LifespanHandler();
            browser.LifeSpanHandler = lifeSpan;
            lifeSpan.PopupRequest += LifeSpanPopupRequest;

            toolStripContainer.ContentPanel.Controls.Add(browser);

            browser.IsBrowserInitializedChanged += OnIsBrowserInitializedChanged;
            browser.LoadingStateChanged += OnLoadingStateChanged;
            browser.ConsoleMessage += OnBrowserConsoleMessage;
            browser.StatusMessage += OnBrowserStatusMessage;
            browser.TitleChanged += OnBrowserTitleChanged;
            browser.AddressChanged += OnBrowserAddressChanged;

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
            browser.Load(url);
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
            // ++++++++++++++++++++++++++++++++++++++

            // URL den yaz programi. 
            // https://vip3.ttwars.com/build.php?id=2&fastUP=0
            // id degisiyor sadece

            // ++++++++++++++++++++++++++++++++++++++
            //Wait for the page to finish loading

            if (args.IsLoading == false)
            {
                var task = browser.GetSourceAsync();
                task.ContinueWith(taskHtml =>
                {
                    var html = taskHtml.Result;
                    var parseHtml = new ParseHtml(TraceLine, html); // Extracts some useful data

                    var regexLogin = new Regex("[login]");
                    if (regexLogin.IsMatch(html))
                    {
                        textBoxLogs.Text += TraceLine("Logging in...");
                        var login = new Login(browser);
                        login.LoginUser();
                    }

                    List<IResource> res = parseHtml.ResInfo();

                    foreach (var e in res)
                    {
                        textBoxLogs.Text += TraceLine(e.GetType().Name + " - Index: " + e.Index + " - Level: " + e.ResLevel);
                    }
                }, TaskScheduler.Default);

                if (browser.Address.Contains("https://" + subDomain + ".ttwars.com/dorf1.php"))
                {
                    browser.ExecuteScriptAsyncWhenPageLoaded("var element = document.getElementById(\"rx\").children;" +
                                                             "\r\nelement[3].click();");
                }
                else if (browser.Address.Contains("https://" + subDomain + ".ttwars.com/build.php?id="))
                {
                    textBoxLogs.Text += TraceLine("Upgrading!");
                    browser.ExecuteScriptAsyncWhenPageLoaded(
                        @"function getElementsByText(text) {
                    function rec(ele, arr)
                    {
                        if (ele.childNodes.length > 0) 
                            for (var i = 0; i < ele.childNodes.length; i++) 
                                rec(ele.childNodes[i], arr);
                        else if (ele.nodeType == Node.TEXT_NODE && 
                            ele.nodeValue.indexOf(text) != -1) 
                            arr.push(ele.parentNode);
                        return arr;
                    }
                    return rec(document.body, []);
                }"
                        + "\r\nvar item = getElementsByText('Upgrade to level ');"
                        + "\r\nitem[0].click();");
                    textBoxLogs.Text += TraceLine("Upgrading...");
                }
            }

            //SetCanGoBack(args.CanGoBack);
            //SetCanGoForward(args.CanGoForward);

            //this.InvokeOnUiThreadIfRequired(() => SetIsLoading(!args.CanReload));
        }

        private void InvokeMethod(string result)
        {
            if (result != "")
            {
                textBoxLogs.Text += TraceLine("Level: " + result);
            }
            else
            {
                textBoxLogs.Text += TraceLine("Error while getting the level data");
            }

            var numberResult = Regex.Match(result, @"\d+").Value;
            var level = Int32.Parse(numberResult) + 1;
            while (level < 20)
            {
                textBoxLogs.Text += TraceLine("Upgrading Level: " + result);
                
                level++;
            }
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
            //var width = toolStrip1.Width;
            //foreach (ToolStripItem item in toolStrip1.Items)
            //{
            //    if (item != urlTextBox)
            //    {
            //        width -= item.Width - item.Margin.Horizontal;
            //    }
            //}
            //urlTextBox.Width = Math.Max(0, width - urlTextBox.Margin.Horizontal - 18);
        }

        private void ExitMenuItemClick(object sender, EventArgs e)
        {
            browser.Dispose();
            Cef.Shutdown();
            Close();
        }

        private void GoButtonClick(object sender, EventArgs e)
        {
            //LoadUrl(urlTextBox.Text);
        }

        private void BackButtonClick(object sender, EventArgs e)
        {
            browser.Back();
        }

        private void ForwardButtonClick(object sender, EventArgs e)
        {
            browser.Forward();
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
                browser.Load(url);
            }
        }

        private void ShowDevToolsMenuItemClick(object sender, EventArgs e)
        {
            browser.ShowDevTools();
        }
    }
}
