﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using resources = CTRFramework.Properties.Resources;

namespace CTRFramework.Shared
{
    public class Meta
    {
        public static int SectorSize = 0x800;

        #region Paths/filenames
        public static string BasePath = AppDomain.CurrentDomain.BaseDirectory;
        public static string UserPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CTRViewer");
        public static string SettingsFile = Path.Combine(UserPath, "settings.xml");
        public const string JsonPath = "versions.json";
        public const string XmlPath = "versions.xml";
        public const string HowlPath = "howlnames.txt";
        public const string CseqPath = "cseq.json";
        public const string SmplPath = "samplenames.txt";
        public const string BankPath = "banknames.txt";
        public const string ModelsPath = "models";
        public const string BigFileName = "bigfile.big";
        #endregion

        static JObject json;

        public static void Load()
        {
            json = JObject.Parse(Helpers.GetTextFromResource(JsonPath));
        }

        public List<string> LoadList(string fn)
        {
            string[] s = File.ReadAllLines(fn);
            return new List<string>(s);
        }

        public static string Detect(string file, string list, int fc)
        {
            if (json == null) Load();

            Console.Write("Calculating MD5... ");

            string md5 = Helpers.CalculateMD5(file);
            string res = "";

            Console.WriteLine(md5);

            JToken j = json[list][md5];
            // string tag = "ntsc";

            if (j != null)
            {
                res = j.Value<string>("list");

                Console.WriteLine(
                    String.Format("Detected file from {0}",
                    j.Value<string>("comment")
                    ));
            }
            else
            {
                Console.WriteLine("Unknown file.");

                j = json["big_nums"][fc.ToString()];

                if (j != null)
                {
                    res = j.ToString();

                    Console.WriteLine(
                        String.Format("{0} files in BIG. Assume: {1}",
                        fc, res
                        ));
                }
            }

            if (res == "")
                File.WriteAllText("unknown_md5.txt", md5);

            Console.WriteLine("list tag = {0}", res);
            return res;
        }

        public static Dictionary<int, string> GetBigList(string resource)
        {
            string path = Application.dataPath + "/ctr-tools/CTRFramework/Data/"+ resource;
            return Meta.LoadNumberedList(path); //Path.Combine(Meta.DataPath, fn));
        }

        public static Dictionary<int, string> LoadNumberedList(string resource)
        {
            string[] lines = File.ReadAllLines(resource);

            Dictionary<int, string> names = new Dictionary<int, string>();

            foreach (string l in lines)
            {
                string line = l.Split('#')[0];

                if (line.Trim() != "")
                {
                    string[] bb = line.Trim().Replace(" ", "").Split('=');

                    int x = -1;
                    Int32.TryParse(bb[0], out x);

                    if (x == -1)
                    {
                        Console.WriteLine("List parsing error at: {0}", line);
                        continue;
                    }

                    if (names.ContainsKey(x))
                    {
                        Helpers.Panic("Meta", PanicType.Error, $"duplicate entry {x}");
                        continue;
                    }

                    names.Add(x, bb[1]);
                }
            }

            return names;
        }

        public static Dictionary<string, string> LoadTagList(string resource)
        {
            string[] lines = Helpers.GetLinesFromResource(resource);

            Dictionary<string, string> names = new Dictionary<string, string>();

            foreach (string l in lines)
            {
                string line = l.Split('#')[0];

                if (line.Trim() != "")
                {
                    string[] bb = line.Trim().Replace(" ", "").Split('=');

                    if (names.ContainsKey(bb[0]))
                    {
                        Helpers.Panic("Meta", PanicType.Error, $"duplicate entry {bb[0]}");
                        continue;
                    }

                    names.Add(bb[0].Trim(), bb[1].Trim());
                }
            }

            return names;
        }

        public static string GetVersion() => $"CTRFramework {resources.Version} ({resources.BuildDate.Split(',')[0]})";

        public static string GetSignature() => resources.signature;


        //public static JArray levels;
        public static JObject midi;

        public static bool LoadMidiJson()
        {
            try
            {
                JObject json = JObject.Parse(Helpers.GetTextFromResource(Meta.CseqPath));

                //levels = (JArray)json["levels"];
                midi = (JObject)json["midi"];

                return true;
            }
            catch (Exception ex)
            {
                Helpers.Panic("Meta", PanicType.Error, $"Failed to load meta instruments: {ex.Message}");
                return false;
            }
        }
        /*
        public static string GetLevelTitle(string lev)
        {
            foreach (JToken j in levels)
                if (j["name"].ToString() == lev)
                    return j["title"].ToString();

            return "!" + lev + "!";
        }
        */
        public static MetaInst GetMetaInst(string track, string inst, int x)
        {
            if (midi == null)
                Meta.LoadMidiJson();

            try
            {
                //really?
                if (midi != null)
                {
                    if (midi[track] != null)
                        if (midi[track][inst] != null)
                            if (midi[track][inst][x] != null)
                            {
                                return JsonConvert.DeserializeObject<MetaInst>(midi[track][inst][x].ToString());
                            }
                }
            }
            catch
            {
                Helpers.Panic("Meta", PanicType.Error, $"Failed to load meta instrument: {track} {inst} {x}");
            }

            return new MetaInst();
        }

        public static List<string> GetPatchList()
        {
            var list = new List<string>();

            foreach (KeyValuePair<string, JToken> j in midi)
                list.Add(j.Key);

            return list;
        }

        public static string GetMetaInstText(string name)
        {
            return midi[name].ToString();
        }

        public static int GetBankIndex(string track)
        {
            try
            {
                //really?
                if (midi != null)
                {
                    if (midi[track] != null)
                    {
                        if (midi[track]["bank"] != null)
                        {
                            return midi[track]["bank"].ToObject<int>();
                        }
                    }
                }
            }
            catch
            {
            }

            return 0;
        }
    }
    public struct MetaInst
    {
        public int Midi { get; set; }
        public int Pitch { get; set; }
        public int Key { get; set; }
        public string Title { get; set; }
    }
}