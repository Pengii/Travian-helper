using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CefSharp.MinimalExample.WinForms.Controls;
using CefSharp.WinForms;

namespace CefSharp.MinimalExample.WinForms
{
    public class ParseHtml
    {
        private readonly Func<string, string> _traceLane;
        private readonly string _html;

        public ParseHtml(Func<string, string> traceLane, string html)
        {
            _traceLane = traceLane;
            _html = html;
        }

        public List<IResource> ResInfo()
        {
            List<IResource> resources = new List<IResource>();

            var woodCutterLevels = Regex.Matches(_html, $@".*Woodcutter level (\S)");
            var clayPitLevels = Regex.Matches(_html, $@".*Clay Pit level (\S)");
            var ironMineLevels = Regex.Matches(_html, $@".*Iron Mine level (\S)");
            var cropLandLevels = Regex.Matches(_html, $@".*Cropland level (\S)");

            if (woodCutterLevels.Count > 0 && clayPitLevels.Count > 0 && ironMineLevels.Count > 0 && cropLandLevels.Count > 0)
            {
                var indexer = 0;
                var index = 0;
                foreach (Match m in woodCutterLevels)
                {
                    if (indexer == 0)
                        index = 1;
                    else if (indexer == 1)
                        index = 3;
                    else if (indexer == 2)
                        index = 14;
                    else if (indexer == 3)
                        index = 17;

                    resources.Add(new Wood(index, int.Parse(m.Groups[1].ToString())));
                    indexer++;
                }

                indexer = 0;
                foreach (Match m in clayPitLevels)
                {
                    if (indexer == 0)
                        index = 5;
                    else if (indexer == 1)
                        index = 6;
                    else if (indexer == 2)
                        index = 16;
                    else if (indexer == 3)
                        index = 18;
                    resources.Add(new Clay(index, int.Parse(m.Groups[1].ToString())));
                    indexer++;
                }

                indexer = 0;
                foreach (Match m in ironMineLevels)
                {
                    if (indexer == 0)
                        index = 4;
                    else if (indexer == 1)
                        index = 7;
                    else if (indexer == 2)
                        index = 10;
                    else if (indexer == 3)
                        index = 11;
                    resources.Add(new Iron(index, int.Parse(m.Groups[1].ToString())));
                    indexer++;
                }

                indexer = 0;
                foreach (Match m in cropLandLevels)
                {
                    if (indexer == 0)
                        index = 2;
                    else if (indexer == 1)
                        index = 8;
                    else if (indexer == 2)
                        index = 9;
                    else if (indexer == 3)
                        index = 12;
                    else if (indexer == 4)
                        index = 13;
                    else if (indexer == 5)
                        index = 15;
                    resources.Add(new Crop(index, int.Parse(m.Groups[1].ToString())));
                    indexer++;
                }
            }
            return resources;
        }
    }
}