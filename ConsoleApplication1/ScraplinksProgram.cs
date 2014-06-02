using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using IndiaSearch;

namespace ConsoleApplication4
{

    public struct LinkItem
    {
        string href;
        string text;

        public string Href
        {
            get { return href; }
            set { href = value; }
        }

        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        public override string ToString()
        {
            return Href + "+" + Text;
        }
    }

    static class LinkFinder
    {
        public static List<LinkItem> Find(string file)
        {
            List<LinkItem> list = new List<LinkItem>();

            // 1.
            // Find all matches in file.
            MatchCollection M1 = Regex.Matches(file, @"(<a.*?>.*?</a>)", RegexOptions.Singleline);


            // 2.
            // Loop over each match.
            foreach (Match m in M1)
            {
                String value = m.Groups[1].Value;
                String link;
                LinkItem item = new LinkItem();

                // 3.
                // Get href attribute.
                Match m2 = Regex.Match(value, @"href=\""(.*?)\""", RegexOptions.Singleline);

                if (m2.Success)
                {
                    link = m2.Groups[1].Value;
                    if (link.StartsWith("#") == true || link.Contains("(") || link.Contains("%"))
                    {
                        continue;
                    }
                    else if (System.Text.RegularExpressions.Regex.IsMatch(link, "[A-Z][a-z]*:"))
                    {
                        continue;
                    }
                    else if (link.StartsWith("/wiki") == true)
                    {
                        item.Href = "http://en.wikipedia.org" + link;

                        // 4.
                        //Extracting link title

                        Match m3 = Regex.Match(value, @"title=\""(.*?)\""", RegexOptions.Singleline);
                        if (m3.Success)
                        {
                            item.Text = m3.Groups[1].Value;
                        }
                        list.Add(item);
                    }
                }

            }

            return list;
        }
    }


    class linkAccumulator
    {
        public void acuumulate(String[] CountryNames)
        {
            StreamReader fr = null;
            StreamWriter fw = null;
            foreach (var country in CountryNames)
            {
                try
                {
                    DirectoryInfo dir = new DirectoryInfo(country);
                    FileInfo[] finfo = dir.GetFiles();
                    foreach (var file in finfo)
                    {
                        Dictionary<string, titleHits> hitList = new Dictionary<string, titleHits>();
                        try
                        {
                            fr = new StreamReader(file.FullName);
                            String fileContent = fr.ReadToEnd();
                            List<LinkItem> links = LinkFinder.Find(fileContent);

                            foreach (var link in links)
                            {
                                if (hitList.ContainsKey(link.Text)==true)
                                {
                                    hitList[link.Text].Hits++;
                                }
                                else
                                {
                                    titleHits temp = new titleHits();
                                    temp.Link = link.Href;
                                    temp.Hits = 1;
                                    hitList.Add(link.Text, temp);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            continue;
                        }
                        finally
                        {
                            fr.Close();
                        }
                        try
                        {
                            fw = new StreamWriter(country + "Links.csv");
                            foreach (var item in hitList)
                            {
                                fw.WriteLine(item.Value.Hits + "+" + item.Value.Link + "+" + item.Key);
                            }   
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            continue;
                        }
                        finally
                        {
                            if (fw != null)
                            {
                                fw.Close();
                            }
                        }
                    }
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0}",country);
                    Console.WriteLine(e.Message);
                    continue;
                }
            }
           
        }
    }
    class Program
    {




        void show(string str)
        {
            Console.WriteLine(str);
        }

        static void Main()
        {
            // Scrape links from wikipedia.org

            // 1.
            //WebClient wC = new WebClient();

            String[,] country = new String[3, 2];
            country[0, 0] = "india";
            Console.WriteLine("Downloading india...");
            //country[0, 1] = wC.DownloadString("http://en.wikipedia.org/wiki/India");
            Console.WriteLine("Downloading india... Done");
            country[1, 0] = "usa";
            Console.WriteLine("Downloading usa");
            //country[1, 1] = wC.DownloadString("http://en.wikipedia.org/wiki/United_States");
            Console.WriteLine("Downloading usa... Done");
            country[2, 0] = "japan";
            Console.WriteLine("Downloading japan");
            //country[2, 1] = wC.DownloadString("http://en.wikipedia.org/wiki/Japan");
            Console.WriteLine("Downloading japan... Done");
            linkAccumulator la = new linkAccumulator();
            String[] countrynames = { "india", "japan", "usa" };
            la.acuumulate(countrynames);
            //System.IO.StreamWriter file = null;
            //System.IO.StreamWriter sourcefile = null;
            //for (var index = 0; index < 3; index++)
            //{
            //    int numOfLinks = 0;
            //    String source = null;
            //    String filename;
            //    try
            //    {
            //        filename = country[index, 0] + "Missing.csv";
            //        if (File.Exists(filename))
            //        {
            //            File.Delete(filename);
            //        }
            //        file = new System.IO.StreamWriter(filename);
            //        List<LinkItem> linkist = new List<LinkItem>();
            //        linkist = LinkFinder.Find(country[index, 1]);
            //        Dictionary<String, String> linkDict = new Dictionary<string, string>();

            //        foreach (var li in linkist)
            //        {
                        
            //            try
            //            {
            //                if (!linkDict.ContainsKey(li.Text))
            //                {
            //                    linkDict.Add(li.Text, li.Href);
            //                }
            //            }
            //            catch
            //            {
            //                continue;
            //            }
            //        }
            //        foreach (var li in linkDict)
            //        {

                       
            //            try
            //            {
            //                file.WriteLine(li.Key + "+" + li.Value);
            //                //if (!Directory.Exists(country[index, 0]))
            //                //{
            //                //    Directory.CreateDirectory(country[index, 0]);
            //                //}
            //                //filename = country[index, 0] + "/" + li.Key + ".html";
            //                //if (File.Exists(filename))
            //                //{
            //                //    continue;
            //                //}
            //                //sourcefile = new System.IO.StreamWriter(filename,true);
            //                //source = wC.DownloadString(li.Value);
            //                //sourcefile.WriteLine(source);
            //                //Console.WriteLine(country[index, 0] + " " + numOfLinks + " done");
            //            }
            //            catch (Exception ioe)
            //            {
            //                file.WriteLine(li.Key + "+" + li.Value);
            //                Console.WriteLine(ioe.Message);
            //                numOfLinks++;
            //                continue;
            //            }
                        
            //            finally
            //            {
            //                if (sourcefile != null)
            //                {
            //                    sourcefile.Close();
            //                }
            //            }
            //        }
            //    }
            //    catch (IOException e)
            //    {
            //        Console.WriteLine(e.Message);
            //    }
            //    finally
            //    {
            //        if (file != null)
            //        {
            //            file.Close();
            //        }


            //    }
            //    Console.WriteLine("{0} links retrieved from {1}", numOfLinks, country[index, 0]);
            //}
            //ExecuteSearch es = new ExecuteSearch();*/
            //Prepareresults pr = new Prepareresults();
            //pr.makePreparedresults();
            //Console.ReadLine();
           
        }
    }
}