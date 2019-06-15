using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Text.RegularExpressions;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            //COUNT THESE KEYWORDS - MAKE SURE ASIAN IS IN POST!!!!!
            //anxiety, asian, asian american, mental health, suicide, depression, depressed, mental illness, killmyself, sentiment (pos,neg, or neutral), 300 posts
            int limit = 5;  //change for each search limit
            string requestid = "303afe78c7b49dfbaaf04518016908ce";
            for (int i = 0; i < limit; i++)
            {
                string html = string.Empty;
                string network = "reddit";    //switch between twitter and reddit
                string keyword = "asianamericanmentalhealth";
                string url = @"https://api.social-searcher.com/v2/search?q=asian&q=" + keyword + "&key=111032c25f6ad6615b8b5fdfdce68260&limit=20&requestid=" + requestid + "&page=" + i + "&network=" + network;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.GZip;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    html = reader.ReadToEnd();
                    Uri.EscapeDataString(html);
                }
                JObject temp = JObject.Parse(html);
                JArray tempArray = (JArray)temp["posts"];
                StreamReader sr = new StreamReader("/Users/jacobbyerline/Desktop/GraduateProjectData.txt");
                string currentFile = sr.ReadToEnd();
                sr.Close();

                //New code for reading counts for sentiment
                /*StreamReader sr2 = new StreamReader("/Users/dylan/Desktop/GraduateProjectCount.txt");
                int malePosCount = Convert.ToInt32(sr2.ReadLine());
                int maleNegCount = Convert.ToInt32(sr2.ReadLine());
                int maleNeuCount = Convert.ToInt32(sr2.ReadLine());
                int femalePosCount = Convert.ToInt32(sr2.ReadLine());
                int femaleNegCount = Convert.ToInt32(sr2.ReadLine());
                int femaleNeuCount = Convert.ToInt32(sr2.ReadLine());
                sr2.Close();*/

                StringBuilder sb = new StringBuilder();
                sb.Append(currentFile);
                for (int k = 0; k < tempArray.Count(); k++)
                {
                    try
                    {
                        JObject index = (JObject)tempArray[k];
                        string name = index["user"]["name"].ToString();
                        string sadMessage = string.Empty;

                        //New sentiment variable
                        string sentiment = index["sentiment"].ToString();
                        if (network == "reddit") sadMessage = index["text"].ToString();
                        else if (network == "twitter")
                        {
                            string twitterPostURL = index["url"].ToString();
                            using (WebClient client = new WebClient())
                            {
                                string testString = client.DownloadString(twitterPostURL);
                                char key = '>';
                                int count = 0;
                                int startIndex = 0;
                                for (int j = 0; count < 22; j++)
                                {
                                    if (testString[j] == key) count++;
                                    startIndex = j;
                                }
                                for (int j = startIndex + 1; testString[j] != '<'; j++)
                                {
                                    sadMessage = sadMessage + testString[j];
                                }
                            }
                        }
                        string html1 = string.Empty;
                        string genderPass = "NoNeHwBYJaYoyRKJKS";
                        string url1 = @"https://gender-api.com/get?key=" + genderPass + "&name=+" + name;
                        HttpWebRequest request1 = (HttpWebRequest)WebRequest.Create(url1);
                        request1.AutomaticDecompression = DecompressionMethods.GZip;
                        using (HttpWebResponse response1 = (HttpWebResponse)request1.GetResponse())
                        using (Stream stream1 = response1.GetResponseStream())
                        using (StreamReader reader1 = new StreamReader(stream1))
                        {
                            html1 = reader1.ReadToEnd();
                            Uri.EscapeDataString(html1);
                        }
                        JObject temp1 = JObject.Parse(html1);
                        string gender = temp1["gender"].ToString();
                        string accuracyString = temp1["accuracy"].ToString();
                        int accuracy = Convert.ToInt32(accuracyString);
                        if (gender.ToString() != "unknown" && name != "null" && accuracy >= 0)    //if accuracy > 80
                        {
                            string newSadData = sadMessage.Replace("&#10;", "  ");
                            newSadData = newSadData.Replace("&#39;", "' ");
                            newSadData = newSadData.Replace("&quot;", "\" ");
                            newSadData = newSadData.Replace("&amp;", "& ");
                            newSadData = newSadData.Replace('%', ' ');

                            bool isNew = true;
                            string tempString = sb.ToString();
                            List<string> tempList = tempString.Split('\n').ToList();
                            for (int j = 0; j < tempList.Count; j++)
                            {
                                if (newSadData == tempList[j])
                                {
                                    isNew = false;
                                    break;
                                }
                            }

                            if (isNew)
                            {
                                string newData = "Name: " + name + "\n" + "Gender: " + gender + "\n" + "Accuracy: " + accuracy + "\n" + "Sentiment: " + sentiment + "\n" + newSadData + "\n\n";

                                //Checks if new data is positive negative or neutral and which gender it is
                                if (gender == "male")
                                {
                                   // if (sentiment == "positive") malePosCount++;
                                   // else if (sentiment == "negative") maleNegCount++;
                                   // else maleNeuCount++;
                                }
                                else if (gender == "female")
                                {
                                   // if (sentiment == "positive") femalePosCount++;
                                   // else if (sentiment == "negative") femaleNegCount++;
                                   // else femaleNeuCount++;
                                }
                                sb.Append(newData);
                            }
                            else Console.WriteLine("This was a retweet");
                        }
                        else
                        {
                            Console.WriteLine("No gender found for this entry.\n\n");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Oops you got an error!");
                    }
                }
                StreamWriter sw = new StreamWriter("/Users/jacobbyerline/Desktop/GraduateProjectData.txt");
                currentFile = sb.ToString();
                sw.WriteLine(currentFile);
                sw.Close();

                //Writes to new file to store count data
               /* StreamWriter sw2 = new StreamWriter("/Users/dylan/Desktop/GraduateProjectCount.txt");
                sw2.WriteLine(malePosCount + "\n" + maleNegCount + "\n" + maleNeuCount + "\n" + femalePosCount + "\n" + femaleNegCount + "\n" + femaleNeuCount);
                sw2.WriteLine("Male Positive Sentiment: " + malePosCount);
                sw2.WriteLine("Male Negative Sentiment: " + maleNegCount);
                sw2.WriteLine("Male Neutral Sentiment: " + maleNeuCount);
                sw2.WriteLine("Female Positive Sentiment: " + femalePosCount);
                sw2.WriteLine("Female Negative Sentiment: " + femaleNegCount);
                sw2.WriteLine("Female Neutral Sentiment: " + femaleNeuCount);
                sw2.WriteLine("Total Positive Sentiment: " + (malePosCount + femalePosCount));
                sw2.WriteLine("Total Negative Sentiment: " + (maleNegCount + femaleNegCount));
                sw2.WriteLine("Total Neutral Sentiment: " + (maleNeuCount + femaleNeuCount));
                sw2.WriteLine("Total men: " + (malePosCount + maleNegCount + maleNeuCount));
                sw2.WriteLine("Total women: " + (femalePosCount + femaleNegCount + femaleNeuCount));
                sw2.WriteLine("Total tests: " + (malePosCount + maleNegCount + maleNeuCount + femalePosCount + femaleNegCount + femaleNeuCount));
                sw2.Close();
                */
            }
        }
    }
}