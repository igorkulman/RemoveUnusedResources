using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace RemoveUnusedResources
{
    class Program
    {
        static readonly List<string> Files = new List<string>();
        static readonly List<string> ResourceFiles = new List<string>();

        private static readonly string[] Whitelist = new[] {"ResourceFlowDirection", "ResourceLanguage", "MBNoText", "MBYesText", "MBCancelText", "MBOkText", "ListBoxEmptyContent", "ListPullToRefresh", "ListPullToRefreshLoading", "ListPullToRefreshTime", "ListReleaseToRefresh" };

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: RemoveUnusedResources.exe project_dir_path");
                return;
            }

            var path = args[0];

            Console.WriteLine("Scanning " + path);

            DirSearch(path);

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
                        if (content.Contains("." + resource) || Whitelist.Contains(resource))
                        {
                            used.Add(resource);
                        }
                    }
                }

                var unused = resources.Where(l => !used.Contains(l)).ToList();
                Console.WriteLine("Unused keys: "+unused.Count);

                doc.Descendants("data").Where(l => unused.Contains(l.Attribute("name").Value)).Remove();

                Console.WriteLine("Saving");
                File.WriteAllText(file,doc.ToString());

                Console.WriteLine();
            }
        }

        private static void DirSearch(string sDir)
        {
            try
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
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
