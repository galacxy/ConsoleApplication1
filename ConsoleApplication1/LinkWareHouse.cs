﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using System.Text.RegularExpressions;
using System.IO;


/// <summary>
/// Summary description for dataset
/// </summary>
///
namespace IndiaSearch
{
    public class Dataset
    {
        enum datasetStatus
        {
            NEW, INDEXED
        }
        string link;
        double directHits;
        double indirectHits;
        string linkname;

        public string Linkname
        {
            get { return linkname; }
            set { linkname = value; }
        }

        
        datasetStatus status;

        private datasetStatus Status
        {
            get { return status; }
            set { status = value; }
        }

        public string Link
        {
            get { return link; }
            set { link = value; }
        }


        public double DirectHits
        {
            get { return directHits; }
            set { directHits = value; }
        }

        public double IndirectHits
        {
            get { return indirectHits; }
            set { indirectHits = value; }
        }



        public Dataset()
        {
            Link = "http:\\";
            Linkname = "";
            status = datasetStatus.NEW;
            DirectHits = 0.0;
            IndirectHits = 0.0;
        }
        public void AddDirectHit(int count)
        {
            DirectHits += (count) + DirectHits;
        }

        public void AddInDirectHit(int count)
        {
            DirectHits += (count / 2) + DirectHits;
        }
    }

    class SearchProcessing
    {
        public string retrievePageSource(string PageTitle)
        {
            string source = "empty";
            System.IO.StreamReader myFile = null;
            try
            {
                // Read the file as one string.

                myFile = new System.IO.StreamReader(PageTitle);
                source = myFile.ReadToEnd();
            }
            catch (IOException ie)
            {
                Console.WriteLine(ie.Message);
            }
            finally
            {
                if (myFile != null)
                {
                    myFile.Close();
                }
            }
            return source;
        }

        public int findMatches(string needle, string haystack)
        {
            int count = 0;
            MatchCollection M1 = Regex.Matches(haystack, @"(" + needle + ")", RegexOptions.Singleline);
            count = M1.Count;
            return count;
        }

        public int findAllMatches(string term, string foldername)
        {
            int count = 0;
            try
            {
                DirectoryInfo Dinfo = new DirectoryInfo(foldername);//Assuming Test is your Folder
                FileInfo[] Files = Dinfo.GetFiles("*.html"); //Getting Text files
                String PageSource = "";
                foreach (FileInfo file in Files)
                {
                    PageSource = retrievePageSource(foldername + "\\" + file.Name);
                    count += findMatches(term, PageSource);
                }
            }
            catch (IOException ie)
            {
                Console.WriteLine(ie.Message);
            }
            return count;
        }

    }

    class StoredResults
    {
        Dictionary<int, Dataset> DataResults;
        IndexLinks IndexedLinks;
        public StoredResults()
        {
            DataResults = new Dictionary<int, Dataset>();
            IndexedLinks = new IndexLinks();
            IndexedLinks.makeIndexFile();
            //LoadResults();
        }
        public List<String> ReadResults()
        {
            System.IO.StreamReader file = null;
            List<String> Results = new List<String>();
            try
            {
                String singleResult = null;
                int i = 0;
                file = new System.IO.StreamReader("indiaResults.csv");
                while ((singleResult = file.ReadLine()) != null)
                {
                    Results.Add(singleResult + "," + i);
                    i++;
                }
            }
            catch (IOException ie)
            {
                Console.WriteLine(ie.Message);
            }
            catch (ArgumentException ae)
            {
                Console.WriteLine(ae.Message);
            }
            finally
            {
                if (file != null)
                {
                    file.Close();
                }
            }
            return Results;
        }

        public void LoadResults()
        {
            List<String> Results = ReadResults();
            String[] ResultParameters;
            Dataset singleDataSet;
            char[] seperator = { ',' };
            foreach (String Result in Results)
            {
                ResultParameters = Result.Split(seperator);
                singleDataSet = new Dataset();
                singleDataSet.Link = ResultParameters[0];
                singleDataSet.DirectHits = Convert.ToDouble(ResultParameters[1]);
                singleDataSet.IndirectHits = Convert.ToDouble(ResultParameters[2]);
                DataResults.Add(Int32.Parse(ResultParameters[3]), singleDataSet);
            }
        }
    }

    class IndexLinks
    {
        public int countLinks(String Link, String PageSource)
        {
            int count = 0;
            MatchCollection M1 = Regex.Matches(PageSource, @"(" + Link + ")", RegexOptions.Singleline);
            count = M1.Count;
            return count;
        }

        public void makeIndexFile()
        {
            System.IO.StreamWriter fileW = null;
            System.IO.StreamReader fileR = null;
            String[] Countries = { "japan" };
            SearchProcessing LinksIndexing = new SearchProcessing();
            Dictionary<String, Dataset> RetrievedResult = new Dictionary<String, Dataset>();
            foreach (String Country in Countries)
            {
                string filename = Country + "Results.csv";
                try
                {
                    fileW = new System.IO.StreamWriter(filename);
                    try
                    {

                        fileR = new System.IO.StreamReader(Country + ".csv");
                        char[] seperator = { '+' };
                        String linkline = null;
                        String link = null;
                        String linkname = null;
                        int index = 0;
                        while ((linkline = fileR.ReadLine()) != null)
                        {
                            int linkHit = 0;
                            link = linkline.Split(seperator)[0];
                            linkname = linkline.Split(seperator)[1];
                            link = link.Replace("http://en.wikipedia.org", "");
                            linkHit = LinksIndexing.findAllMatches(link, Country);
                            if (RetrievedResult.ContainsKey(link))
                            {
                                RetrievedResult[link].AddDirectHit(linkHit);
                            }
                            else
                            {
                                Dataset ds = new Dataset();
                                ds.Linkname = linkname;
                                ds.Link = link;
                                ds.DirectHits = linkHit;
                                RetrievedResult.Add(link, ds);
                                //fileW.WriteLine(index + "," + link + "," + linkHit + ",");
                                //Console.WriteLine(index + "," + link + "," + linkHit + ",");
                                index++;
                            }
                            Console.WriteLine(link + "+" + linkHit);
                        }
                        foreach (KeyValuePair<String, Dataset> ds in RetrievedResult)
                        {
                            fileW.WriteLine(ds.Value.DirectHits + "+" + ds.Value.Linkname + "+" + ds.Value.Link);
                        }

                    }
                    catch (IOException ie)
                    {
                        Console.WriteLine(ie.Message);
                    }
                    finally
                    {
                        if (fileR != null)
                        {
                            fileR.Close();
                        }
                    }
                }
                catch (IOException ie)
                {
                    Console.WriteLine(ie.Message);
                }
                finally
                {
                    if (fileW != null)
                    {
                        fileW.Close();
                    }
                }
            }
        }
    }

    public class ExecuteSearch
    {
        
            StoredResults SResults;
            public ExecuteSearch()
            {
                SResults = new StoredResults();
            }
       
    }

    public class titleHits
    {
        Int64 hits;

        public Int64 Hits
        {
            get { return hits; }
            set { hits = value; }
        }
        String link;

        public String Link
        {
            get { return link; }
            set { link = value; }
        }
        public titleHits()
        {
            Hits = 0;
            Link = "";
        }
    }


    public class Prepareresults
    {
        
        public void makePreparedresults()
        {
            System.IO.StreamReader fr1 = null;
            System.IO.StreamReader fr2 = null;
            System.IO.StreamWriter fw1 = null;
            String[] countries = {"india","usa","japan"};            
            Dictionary<String, titleHits> PreparedData = new Dictionary<string, titleHits>();
            foreach(var country in countries)
            {
                try
                {
                    fr1 = new StreamReader(country + "Results.csv");
                    String fileline;

                    while ((fileline = fr1.ReadLine()) != null)
                    {
                        String[] explodedFileLine = fileline.Split('+');
                        if (PreparedData.ContainsKey(explodedFileLine[2]) == true)
                        {
                            PreparedData[explodedFileLine[2]].Hits += Convert.ToInt64(explodedFileLine[0]);
                            Console.WriteLine(explodedFileLine[2] + " " + PreparedData[explodedFileLine[2]].Hits);
                        }
                        else
                        {
                            titleHits temp = new titleHits();
                            temp.Hits = Convert.ToInt64(explodedFileLine[0]);
                            temp.Link = explodedFileLine[1];
                            PreparedData.Add(explodedFileLine[2], temp);
                        }
                    }
                }
                catch
                {
                    continue;
                }
                finally
                {
                    if (fr1 != null)
                    {
                        fr1.Close();
                    }
                }

            }
            try
            {
                fw1 = new StreamWriter("preparedResults.csv");
                foreach (var item in PreparedData)
                {
                    fw1.WriteLine(item.Value.Hits + "+" + item.Value.Link + "+" + item.Key);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                if(fw1!=null)
                {
                    fw1.Close();
                }
            }
        }
    }
}