using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace RemoveUnusedResources
{
    class Program
    {
        /// <summary>
        /// XAML and CS files
        /// </summary>
        static readonly List<string> Files = new List<string>();

        /// <summary>
        /// RESX files
        /// </summary>
        static readonly List<string> ResourceFiles = new List<string>();

        /// <summary>
        /// String to be skipped, mostly from Telerik controls
        /// </summary>
        private static readonly string[] Whitelist = new[] { "ResourceFlowDirection", "ResourceLanguage", "MBNoText", "MBYesText", "MBCancelText", "MBOkText", "ListBoxEmptyContent", "ListPullToRefresh", "ListPullToRefreshLoading", "ListPullToRefreshTime", "ListReleaseToRefresh" };

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: RemoveUnusedResources.exe project_dir_path");
                return;
            }

            var path = args[0];

            Console.WriteLine("Scanning " + path);

            DirSearch(path); //get all the XAML, CS and RESX files

            foreach (var file in ResourceFiles)
            {
                Console.WriteLine("Processing " + file);

                var doc = XDocument.Load(file);

                var resources = doc.Descendants("data").Select(l => l.Attribute("name").Value).ToList();
                var used = new HashSet<string>();

                foreach (var f in Files)
                {
                    var content = File.ReadAllText(f);
                    foreach (var resource in resources)
                    {
                        if (Whitelist.Contains(resource))
                        {
                            used.Add(resource);
                            continue;
                        }

                        if (f.EndsWith(".xaml"))
                        {
                            //{Binding LocalizedResources.PaymentSelection, Source={StaticResource LocalizedStrings}
                            if (content.Contains(string.Format("LocalizedResources.{0}", resource)))
                            {
                                used.Add(resource);
                            }
                        }
                        else if (f.EndsWith(".cs"))
                        {
                            //AppResources.PaymentSelection
                            if (content.Contains(string.Format("Resources.{0}", resource)))
                            {
                                used.Add(resource);
                            }
                        }
                    }
                }

                var unusedCount = resources.Count - used.Count;
                if (unusedCount == 0)
                {
                    Console.WriteLine("No unused strings found");
                }
                else
                {
                    Console.WriteLine("Unused strings: " + unusedCount);

                    doc.Descendants("data").Where(l => !used.Contains(l.Attribute("name").Value)).Remove();

                    Console.WriteLine("Saving " + file);
                    File.WriteAllText(file, doc.ToString());
                }

                Console.WriteLine();
            }
        }

        private static void DirSearch(string sDir)
        {
            foreach (string d in Directory.GetDirectories(sDir))
            {
                foreach (string f in Directory.GetFiles(d))
                {
                    if (f.EndsWith(".xaml") || f.EndsWith(".cs"))
                    {
                        Files.Add(f);
                    }
                    if (f.EndsWith(".resx"))
                    {
                        ResourceFiles.Add(f);
                    }
                }
                DirSearch(d);
            }
        }
    }
}
