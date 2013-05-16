using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
namespace SarcasmDetection
{
    class myCountingClass
    {
        static Dictionary<string, int> wordCount = new Dictionary<string, int>();
        static Dictionary<string, int> wordFreq = new Dictionary<string, int>();
        static string fileLoc;
        public static int threshold;
        public static string allText;

        public static void reset()
        {
            wordCount.Clear();
            wordFreq.Clear();
            wordCount = new Dictionary<string, int>();
            wordFreq = new Dictionary<string, int>();
        }
        public static void setFileLoc(string s){

            fileLoc = s;
        }

        public static void count()
        {
            if (!File.Exists(fileLoc))
            {
                Console.WriteLine("file does NOT exist!");
                return;
            }
            else
            {
                string[] allLines = File.ReadAllLines(fileLoc);
                allText = File.ReadAllText(fileLoc);
                foreach (var line in allLines)
                {
                    string[] words = Regex.Split(line, @"\s");
                    //foreach (string word in line.Split(' '))

                    //foreach (string word in words)
                    char[] delChars = new char[] { ' ', ',', '.', '?', '!', '\n', '\t', '\r', ':', ':', '(', ')', '{', '}', '[', ']', '"' };

                    foreach (string word in line.Split(delChars))
                    {
                        if (!wordCount.Keys.Contains(word.ToLower()))
                        {
                            wordCount.Add(word.ToLower(), 1);
                        }
                        else
                        {
                            //add one to the val
                            wordCount[word.ToLower()] = wordCount[word.ToLower()] + 1;
                        }
                    }
                }

            }
        }

        public static void writeDictionay()
        {
            wordCount = getDictionary();
            if (wordCount != null)
            {
                foreach (var pair in wordCount)
                {
                    Console.WriteLine(pair.Key + "  " + pair.Value);
                }

            }
            else
            {
                Console.WriteLine("the dictionary is empty");
            }
        }

        public static Dictionary<string, int> getDictionary()
        {
            if (wordCount != null)
            {
                if (wordCount.Keys.Contains(""))
                {

                    //Console.WriteLine("removing the space");
                    wordCount.Remove("");
                }
                return wordCount;
            }
            return null;
        }


        public static Dictionary<string,int> ReasonableParser(string sTextToParse)
        {
            Dictionary<string, int> al = new Dictionary<string, int>();

            string sTemp = sTextToParse;
            sTemp = sTemp.Replace(Environment.NewLine, " ");

            char[] arrSplitChars = { '.', '?', '!' };

            string[] splitSentences = sTemp.Split(arrSplitChars, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < splitSentences.Length; i++)
            {
                int pos = sTemp.IndexOf(splitSentences[i].ToString());
                char[] arrChars = sTemp.Trim().ToCharArray();
                char c = arrChars[pos + splitSentences[i].Length];
                al.Add(splitSentences[i].ToString().Trim() + c.ToString(),0);
            }

            return al;
        }

        public static Dictionary<string, int> TrainingParser(FileInfo f)
        {
            Dictionary<string, int> al = new Dictionary<string, int>();
            string[] lines = File.ReadAllLines(f.FullName);
            for (int i = 0; i < lines.Count() - 1; i += 2)
            {
                string sentence = lines[i];
                string slabel = lines[i + 1];
                int label = int.Parse(slabel);
                al.Add(sentence, label);
            }
            return al;
        }

        public static void setThreshold(int newThreshold)
        {
            threshold = newThreshold;
        }



        public static void setDictionaryOfHighFContentW()
        {
            //1 HFW
            //0 content word
            foreach (var pair in getDictionary())
            {
                if (pair.Value > threshold)
                {
                    //1 HFW
                    wordFreq.Add(pair.Key, 1);
                }
                else
                {
                    //0 content word
                    wordFreq.Add(pair.Key, 0);
                }
            }
        }

        public static Dictionary<string, int> getContentFreqDictionary()
        {
            return wordFreq;
        }

        public static void writeContFreqDictionary()
        {

            if (wordCount != null)
            {
                foreach (var pair in wordFreq)
                {
                    Console.WriteLine(pair.Key + "  " + pair.Value);
                }

            }
            else
            {
                Console.WriteLine("the dictionary 2 is empty");
            }
        }

        public static List<Pattern> getAllPatterns(Dictionary<string, int> sentences, Dictionary<string, int> words)
        {
            List<Pattern> al = new List<Pattern>();
            //char[] delChars = new char[] { ' ', ',', '.', '?', '!', '\n', '\t', '\r', ':', ':', '(', ')', '{', '}', '[', ']', '"' };
            foreach (string s in sentences.Keys)
            {
                Regex regex = new Regex(@"[^\w\s_]+");
                string cleanText = regex.Replace(s.ToLower(), " [HFW] ");

                foreach (KeyValuePair<string, int> kv in words)
                {
                    if (kv.Value == 1) cleanText = cleanText.Replace(kv.Key.ToLower(), " [HFW] ");
                }
                //regex = new Regex(@"[^(\[HFW\])_]+");
                //\w+(?=\s)|(?<=\s)\w+
                //regex = new Regex(@"(?<!\[|F|\[F)\w+(?!W|\]|W\]\])");
                regex = new Regex(@"\w+(?=\s)|(?<=\s)\w+");
                cleanText = regex.Replace(cleanText, "[CW]");
                Pattern p = new Pattern(cleanText, sentences[s]);
                p.sentence = s;
                al.Add(p);
            }

            return al;
        }

        public static int countWords(string s)
        {
            MatchCollection collection = Regex.Matches(s, @"[\S]+");
            return collection.Count;
        }
        public static int countHFW(string s)
        {
            return Regex.Matches(s, @"\[HFW\]").Count;
        }

        public static float comparePatterns(Pattern a, Pattern b)
        {
            if (countHFW(a.pattern) != countHFW(b.pattern)) return 0;

            int[] avars = new int[countHFW(a.pattern) + 1];
            int[] bvars = new int[countHFW(b.pattern) + 1];

            string[] a_tags = a.pattern.Split(' ');
            string[] b_tags = b.pattern.Split(' ');
            int current = 0;
            int index = 0;
            for (int i=0; i<a_tags.Count();i++)
            {
                if (a_tags[i].Equals(""))
                {
                    if (i == a_tags.Count() - 1)
                    {
                        avars[index] = current;
                        current = 0;
                    }
                    continue;
                }
                if (a_tags[i].Equals("[HFW]"))
                {
                    avars[index] = current;
                    current = 0;
                    index++;
                }
                else
                {
                    current++;
                }
                if (i == a_tags.Count() - 1)
                {
                    avars[index] = current;
                    current = 0;
                }
            }

            index = 0;
            current = 0;
            for (int i = 0; i < b_tags.Count(); i++)
            {

                if (b_tags[i].Equals(""))
                {
                    if (i == b_tags.Count() - 1)
                    {
                        bvars[index] = current;
                        current = 0;
                    }
                    continue;
                }
                if (b_tags[i].Equals("[HFW]"))
                {
                    bvars[index] = current;
                    current = 0;
                    index++;
                }
                else
                {
                    current++;
                }
                if (i == b_tags.Count() - 1)
                {
                    bvars[index] = current;
                    current = 0;
                }
            }

            int count = 0;
            for (int i = 0; i < countHFW(a.pattern) + 1; i++)
            {
                int bigger = avars[i] >= bvars[i] ? avars[i] : bvars[i];
                int smaller = avars[i] < bvars[i] ? avars[i] : bvars[i];
                if (bigger == 0)
                {
                    count++;
                    continue;
                }
                int diff = bigger - smaller;
                if ((float)diff <= (40f/100f)*(float)bigger) count++;
            }
            float similarity = (float)count / (countHFW(a.pattern) + 1);
            return similarity;
        }

    }
}
