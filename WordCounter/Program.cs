using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SarcasmDetection
{
    class Program
    {
        static void Main(string[] args)
        {

            List<Pattern> trainingPatterns = new List<Pattern>();

            if (Directory.Exists("training"))
            {
                DirectoryInfo articles = new DirectoryInfo("training");
                DirectoryInfo training_not_labeled = new DirectoryInfo("training-not-labeled");

                FileInfo[] files = articles.GetFiles();
                foreach (FileInfo f in files)
                {
                    Console.WriteLine("Parsing file " + f.Name + " ...");
                    FileInfo notlabeled = new FileInfo(Path.Combine(training_not_labeled.FullName, f.Name));

                    myCountingClass cc = new myCountingClass();
                    myCountingClass.setFileLoc(notlabeled.FullName);
                    myCountingClass.count();

                    Dictionary<string, int> countworddic = myCountingClass.getDictionary();
                    int sum = 0;
                    foreach (KeyValuePair<string, int> kv in countworddic)
                    {
                        sum += kv.Value;
                    }
                    float average = (float)sum / countworddic.Count;
                    int newthreshold = (int)Math.Round(average * 2);
                    //Console.WriteLine("Average is: " + newthreshold);
                    //Console.WriteLine("Sum: " + sum + " count: " + countworddic.Count);

                    myCountingClass.setThreshold(newthreshold);
                    

                    //Dictionary<string, int> al = myCountingClass.ReasonableParser(File.ReadAllText(f.FullName));
                    Dictionary<string, int> al = myCountingClass.TrainingParser(f);
                    myCountingClass.setDictionaryOfHighFContentW();
                    Dictionary<string, int> freqdic = myCountingClass.getContentFreqDictionary();
                    List<Pattern> patterns = myCountingClass.getAllPatterns(al, freqdic);
                    
                    foreach (Pattern p in patterns)
                    {
                        //Console.WriteLine(p.level + " ===>  " + p.pattern);
                        trainingPatterns.Add(p);
                    }

                    //Pattern a = new Pattern("[HFW] [CW] [CW] [HFW] [CW] [CW] [CW] [CW] [HFW]  [CW]", 0);
                    //Pattern b = new Pattern(" [HFW]  [CW] [CW] [CW] [HFW]  [CW] [CW] [CW] [CW] [HFW] [CW] ", 0);
                    //Console.WriteLine("Similarity: " + myCountingClass.comparePatterns(a, b));
                    Console.WriteLine("Training from " + f.Name + " done!\n");
                    myCountingClass.reset();


                }
            }
            else Console.WriteLine("Dir not found!");
            //Console.WriteLine("Done!");

            int TP = 0, TN = 0, FP = 0, FN = 0;
            List<Pattern> validPatterns = new List<Pattern>();
            if (Directory.Exists("articles"))
            {
                DirectoryInfo articles = new DirectoryInfo("articles");
                DirectoryInfo articleslabeled = new DirectoryInfo("articles-labeled");

                FileInfo[] files = articles.GetFiles();
                foreach (FileInfo f in files)
                {
                    Console.WriteLine("Parsing file " + f.Name + " ...\n");
                    FileInfo artlabeled = new FileInfo(Path.Combine(articleslabeled.FullName, f.Name));

                    myCountingClass cc = new myCountingClass();
                    myCountingClass.setFileLoc(f.FullName);
                    myCountingClass.count();

                    Dictionary<string, int> countworddic = myCountingClass.getDictionary();
                    int sum = 0;
                    foreach (KeyValuePair<string, int> kv in countworddic)
                    {
                        sum += kv.Value;
                    }
                    float average = (float)sum / countworddic.Count;
                    int newthreshold = (int)Math.Round(average * 2);
                    //Console.WriteLine("Average is: " + newthreshold);
                    //Console.WriteLine("Sum: " + sum + " count: " + countworddic.Count);

                    myCountingClass.setThreshold(newthreshold);
                    

                    Dictionary<string, int> al = myCountingClass.ReasonableParser(myCountingClass.allText);
                    Dictionary<string, int> al2 = myCountingClass.TrainingParser(artlabeled);
                    myCountingClass.setDictionaryOfHighFContentW();

                    List<Pattern> patterns = myCountingClass.getAllPatterns(al, myCountingClass.getContentFreqDictionary());
                    foreach (Pattern p in patterns)
                    {
                        
                        float highest = 0;
                        int sarcasm_level=0;
                        foreach (Pattern t in trainingPatterns)
                        {
                            float compval = myCountingClass.comparePatterns(p,t);
                            if (compval > highest)
                            {
                                highest = compval;
                                sarcasm_level = t.level;
                            }
                        }
                        p.level = sarcasm_level;
                        Console.Write(p.level + " ===> "+p.sentence);
                        //Console.Write(" ===>  " + p.pattern);
                        Console.WriteLine();
                    }
                    //Console.WriteLine(patterns.Count + " " +al2.Count);
                    Console.WriteLine();
                    int index = 0;
                    foreach (KeyValuePair<string, int> kv in al2)
                    {
                        int predicted = kv.Value;
                        int correct = patterns[index].level;

                        if (predicted == 1 && correct == 1) TP++;
                        else if (predicted == 0 && correct == 0) TN++;
                        else if (predicted == 1 && correct == 0) FP++;
                        else if (predicted == 0 && correct == 1) FN++;

                        Console.WriteLine(predicted + " ==> " + correct);
                        index++;
                    }


                    //Pattern a = new Pattern("[HFW] [CW] [CW] [HFW] [CW] [CW] [CW] [CW] [HFW]  [CW]", 0);
                    //Pattern b = new Pattern(" [HFW]  [CW] [CW] [CW] [HFW]  [CW] [CW] [CW] [CW] [HFW] [CW] ", 0);
                    //Console.WriteLine("Similarity: " + myCountingClass.comparePatterns(a, b));
                    Console.WriteLine("\nFile " + f.Name + " labeled!\n\n");
                    myCountingClass.reset();


                }
            }
            else Console.WriteLine("Dir not found!");
            //Console.WriteLine("Done!");

            Console.WriteLine("\n\nTP: {0}\tTN: {1}\nFP: {2}\tFN: {3}\n\n", TP, TN, FP, FN);
            float pr = (float)TP / (TP + FP);
            float rec = (float)TP / (TP + FN);
            float acc = (float)(TP + TN) / (TP + TN + FP + FN);
            float f1 = (float)2 * (pr * rec) / (pr + rec);

            Console.WriteLine("Precision: {0}", pr);
            Console.WriteLine("Recall: {0}", rec);
            Console.WriteLine("Accuracy: {0}", acc);
            Console.WriteLine("F1-score: {0}", f1);

            Console.ReadLine();
        }
    }
}
