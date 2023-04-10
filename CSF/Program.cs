using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSF
{
    internal class Program
    {
        public class CSFInfo
        {
            public int TypeID { get; set; }
            public int Version { get; set; }

            public List<CSFText> Texts { get; set; }
        }
        public class CSFText
        {
            public string Unknown { get; set; }
            public int TypeID { get; set; }
            public int Num { get; set; }
            public string TextID { get; set; }
            public int Type { get; set; }
            public string Text { get; set; }
        }

        static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                var pathdir = Path.GetDirectoryName(args[0]);
                var filename = Path.GetFileName(args[0]);
                var extension = Path.GetExtension(args[0]);
                var withoutextension = Path.GetFileNameWithoutExtension(args[0]);

                if(extension == ".json")
                {
                    CSFInfo cSFInfo = new CSFInfo();
                    cSFInfo = JsonConvert.DeserializeObject<CSFInfo>(File.ReadAllText(args[0]));

                    using(var fs = new FileStream(pathdir + $"\\new_{withoutextension}.csf", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        fs.Write(cSFInfo.TypeID);
                        fs.Write(cSFInfo.Version);
                        fs.Write(cSFInfo.Texts.Count);
                        fs.Write(cSFInfo.Texts.Count);
                        fs.Skip(8);
                        foreach(var text in cSFInfo.Texts)
                        {
                            if(text.Unknown != null)
                            {
                                fs.Write(text.Unknown.Length);
                                fs.Write(Encoding.UTF8.GetBytes(text.Unknown));
                            }
                            fs.Write(text.TypeID);
                            fs.Write(text.Num);
                            fs.Write(text.TextID.Length);
                            fs.Write(Encoding.UTF8.GetBytes(text.TextID));
                            fs.Write(text.Type);
                            fs.Write(text.Text.Length);
                            var txt = Encoding.Unicode.GetBytes(text.Text);
                            for (int j = 0; j < txt.Length; j++)
                            {
                                txt[j] ^= 0xFF;
                            }
                            fs.Write(txt);
                        }
                    
                    }
                }
                else if(extension == ".csf")
                {
                    using (var fs = new FileStream(args[0], FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        CSFInfo cSFInfo = new CSFInfo();
                        int type = fs.ReadInt32();
                        cSFInfo.TypeID = type;
                        int version = fs.ReadInt32(); //magic 4 byte version 4 byte
                        cSFInfo.Version = version;
                        int count = fs.ReadInt32();
                        count = fs.ReadInt32();
                        fs.Skip(8);

                        List<CSFText> list = new List<CSFText>();

                        for(int i = 0; i < count; i++)
                        {
                            CSFText text = new CSFText();
                            int ms = fs.ReadInt32();
                            if(ms != 1279413280)
                            {
                                string unk = fs.ReadString(ms);
                                text.Unknown = unk;
                                ms = fs.ReadInt32();
                            }
                            text.TypeID = ms;
                            int num = fs.ReadInt32();
                            text.Num = num;
                            int cnt = fs.ReadInt32();
                            string idtxt = fs.ReadString(cnt);
                            text.TextID = idtxt;
                            ms = fs.ReadInt32();
                            text.Type = ms;
                            cnt = fs.ReadInt32();
                            byte[] txt = fs.ReadBytes(cnt * 2);

                            for(int j = 0; j < txt.Length; j++)
                            {
                                txt[j] ^= 0xFF;
                            }

                            string uni = Encoding.Unicode.GetString(txt);
                            text.Text = uni;
                            list.Add(text);
                        }

                        
                        using(var wt = new StreamWriter(pathdir + $"\\{withoutextension}.json"))
                        {
                            cSFInfo.Texts = list;
                            wt.WriteLine(JsonConvert.SerializeObject(cSFInfo, Formatting.Indented));
                        }
                    }
                }
            }
        }
    }
}
