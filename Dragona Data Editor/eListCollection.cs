using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Dragona_Data_Editor
{
    [Serializable]
    public class eListCollection 
    {
        public short Version;
        public short Signature;
        public bool b_Unknown;
        public int i_Unknown;
        public int i_RecordSize;
        public int i_ColumnCount;
        public int ConversationListIndex;
        public string ConfigFile;
        public string loadedFile;
        public eList[] Lists;
        public SortedList<int, string> pList;
        public SortedList<int, int> iList;
        public string strPATH;
        public bool other_path;
        public int path_timestamp;
        public bool modified;
        public int textList;
        
        public eListCollection(string fileName, string opt)
        {
            Lists = Load(fileName, opt);
        }

        public eListCollection()
        {
        }
        
        public void setLists(eList[] _Lists)
        {
            Lists = _Lists;
        }
        public void RemoveItem(int ListIndex, int ElementIndex)
        {
            Lists[ListIndex].RemoveItem(ListIndex, ElementIndex);
        }
        public void AddItem(int ListIndex, Dictionary<int,object> ItemValues)
        {
            Lists[ListIndex].AddItem(ListIndex, ItemValues);
        }
        public string GetOffset(int ListIndex)
        {
            return BitConverter.ToString(Lists[ListIndex].listOffset);
        }
        public void SetOffset(int ListIndex, string Offset)
        {
            if (Offset != "")
            {
                // Convert from Hex to byte[]
                string[] hex = Offset.Split('-');
                Lists[ListIndex].listOffset = new byte[hex.Length];
                for (int i = 0; i < hex.Length; i++)
                {
                    Lists[ListIndex].listOffset[i] = Convert.ToByte(hex[i], 16);
                }
            }
            else
            {
                Lists[ListIndex].listOffset = new byte[0];
            }
        }
        public string GetValue(int ListIndex, int ElementIndex, int FieldIndex)
        {
            return Lists[ListIndex].GetValue(ElementIndex, FieldIndex);
        }
        public void SetValue(int ListIndex, int ElementIndex, int FieldIndex, string Value)
        {
            Lists[ListIndex].SetValue(ElementIndex, FieldIndex, Value);
        }
        public string GetType(int ListIndex, int FieldIndex)
        {
            return Lists[ListIndex].GetType(FieldIndex);
        }
        public object readValue(BinaryReader br, string type)
        {
            if (type == "int8")
            {
                return br.ReadByte();
            }
            if (type == "uint8")
            {
                return br.ReadSByte();
            }
            if (type == "int16")
            {
                return br.ReadInt16();
            }
            if (type == "uint16")
            {
                return br.ReadUInt16();
            }
            if (type == "int32")
            {
                return br.ReadInt32();
            }
            if (type == "uint32")
            {
                return br.ReadUInt32();
            }
            if (type == "int64")
            {
                return br.ReadInt64();
            }
            if (type == "uint64")
            {
                return br.ReadUInt64();
            }
            if (type == "float")
            {
                return br.ReadSingle();
            }
            if (type == "double")
            {
                return br.ReadDouble();
            }
            if (type.Contains("byte:"))
            {
                return br.ReadBytes(Convert.ToInt32(type.Substring(5)));
            }
            if (type.Contains("wstring:"))
            {
                return br.ReadBytes(Convert.ToInt32(type.Substring(8)));
            }
            if (type.Contains("string:"))
            {
                return br.ReadBytes(Convert.ToInt32(type.Substring(7)));
            }
            return null;
        }
        public object getDefaultValue(string type)
        {
            if (type == "int8")
            {
                return (byte)0;
            }
            if (type == "uint8")
            {
                return (sbyte)0;
            }
            if (type == "int16")
            {
                return (short)0;
            }
            if (type == "uint16")
            {
                return (ushort)0;
            }
            if (type == "int32")
            {
                return (int)0;
            }
            if (type == "uint32")
            {
                return (uint)0;
            }
            if (type == "int64")
            {
                return (long)0;
            }
            if (type == "uint64")
            {
                return (ulong)0;
            }
            if (type == "float")
            {
                return (float)0;
            }
            if (type == "double")
            {
                return (double)0;
            }
            if (type.Contains("byte:"))
            {
                string[] tmp = type.Split(':');
                return new byte[int.Parse(tmp[1])];
            }
            if (type.Contains("wstring:"))
            {
                string[] tmp = type.Split(':');
                return new byte[int.Parse(tmp[1])];
            }
            if (type.Contains("string:"))
            {
                string[] tmp = type.Split(':');
                return new byte[int.Parse(tmp[1])];
            }
            return null;
        }
        private void writeValue(BinaryWriter bw, object value, string type)
        {
            if (type == "int8")
            {
                bw.Write((byte)value);
                return;
            }
            if (type == "uint8")
            {
                bw.Write((sbyte)value);
                return;
            }
            if (type == "int16")
            {
                bw.Write((short)value);
                return;
            }
            if (type == "uint16")
            {
                bw.Write((ushort)value);
                return;
            }
            if (type == "int32")
            {
                bw.Write((int)value);
                return;
            }
            if (type == "uint32")
            {
                bw.Write((uint)value);
                return;
            }
            if (type == "int64")
            {
                bw.Write((long)value);
                return;
            }
            if (type == "uint64")
            {
                bw.Write((ulong)value);
                return;
            }
            if (type == "float")
            {
                bw.Write((float)value);
                return;
            }
            if (type == "double")
            {
                bw.Write((double)value);
                return;
            }
            if (type.Contains("byte:"))
            {
                bw.Write((byte[])value);
                return;
            }
            if (type.Contains("wstring:"))
            {
                bw.Write((byte[])value);
                return;
            }
            if (type.Contains("string:"))
            {
                bw.Write((byte[])value);
                return;
            }
        }
        private eList[] loadConfiguration(string file)
        {
            StreamReader sr = new StreamReader(file);
            eList[] Li = new eList[Convert.ToInt32(sr.ReadLine())];
            try
            {
                ConversationListIndex = Convert.ToInt32(sr.ReadLine());
            }
            catch (Exception)
            {
                ConversationListIndex = 58;
            }
            string line;
            for (int i = 0; i < Li.Length; i++)
            {
                Application.DoEvents();

                while ((line = sr.ReadLine()) == "")
                {
                }
                Li[i] = new eList();
                try
                {
                    Li[i].listName = line.Split('-')[1].TrimStart();
                }
                catch
                {
                    Li[i].listName = line;
                }
                Li[i].listOffset = null;
                string offset = sr.ReadLine();
                if (offset != "AUTO")
                {
                    Li[i].listOffset = new byte[Convert.ToInt32(offset)];
                }
                string[] a = sr.ReadLine().Split(';');
                string[] b = sr.ReadLine().Split(';');
                Dictionary<int, string> aa = new Dictionary<int, string>();
                Dictionary<int, string> ab = new Dictionary<int, string>();
                for (int x = 0; x < a.Length; x++)
                {
                    aa.Add(x, a[x].ToString());
                }
                for (int xx = 0; xx < b.Length; xx++)
                {
                    ab.Add(xx, b[xx].ToString());
                }
                Li[i].elementFields = aa;
                Li[i].elementTypes = ab;
            }
            sr.Close();

            return Li;
        }
        
        public eList[] Load(string elFile, string option)
        {
            eList[] Li = new eList[0];
            
            try
            {
                // open the element file
                using (BinaryReader br = new BinaryReader(File.Open(elFile, FileMode.Open)))
                {
                    Version = br.ReadInt16();
                    Signature = br.ReadInt16();
                    // check if a corresponding configuration file exists
                    string elFileName = Path.GetFileNameWithoutExtension(elFile);
                    string[] configFiles = Directory.GetFiles(Application.StartupPath + "\\cfg\\" + option + "\\", elFileName + ".cfg");
                    if (configFiles.Length > 0)
                    {
                        // configure an eList array with the configuration file
                        ConfigFile = configFiles[0];
                        Li = loadConfiguration(ConfigFile);
                        // read the element file

                        for (int l = 0; l < Li.Length; l++)
                        {
                            Application.DoEvents();

                            // read offset
                            if (Li[l].listOffset != null)
                            {
                                if (Li[l].listOffset.Length > 0)
                                {
                                    Li[l].listOffset = br.ReadBytes(Li[l].listOffset.Length);
                                }
                            }
                            b_Unknown = br.ReadBoolean();
                            i_Unknown = br.ReadInt32();
                            i_RecordSize = br.ReadInt32();
                            i_ColumnCount = br.ReadInt32();
                            
                            // read lists
                            Li[l].elementValues = new Dictionary<int, Dictionary<int, object>>();
                            int count = br.ReadInt32();

                            // go through all elements of a list
                            for (int e = 0; e < count; e++)
                            {
                                Li[l].elementValues[e] = new Dictionary<int, object>();
                                // go through all fields of an element
                                for (int f = 0; f < Li[l].elementTypes.Count; f++)
                                {
                                    Li[l].elementValues[e][f] = readValue(br, Li[l].elementTypes[f]);

                                }
                            }

                            modified = false;
                            textList = -1;
                        }
                    }

                    Application.DoEvents();
                }
            }
            catch
            {
                MessageBox.Show("LOADING ERROR!\n\nThis error usually occurs if incorrect configuration file...");
            }

            return Li;
        }

        public bool Save(string elFile)
        {
            if (File.Exists(elFile))
            {
                File.Delete(elFile);
            }

            using (BinaryWriter bw = new BinaryWriter(File.Open(elFile, FileMode.Create)))
            {

                bw.Write(Version);
                bw.Write(Signature);

                // go through all lists
                for (int list = 0; list < Lists.Length; list++)
                {
                    Application.DoEvents();

                    if (Lists[list].listOffset.Length > 0)
                    {
                        bw.Write(Lists[list].listOffset);
                    }

                    bw.Write(b_Unknown);
                    bw.Write(i_Unknown);
                    bw.Write(i_RecordSize);
                    bw.Write(i_ColumnCount);

                    bw.Write(Lists[list].elementValues.Count);

                    // go through all elements of a list
                    for (int item = 0; item < Lists[list].elementValues.Count; item++)
                    {
                        // go through all fields of an element
                        for (int row = 0; row < Lists[list].elementValues[item].Count; row++)
                        {
                            writeValue(bw, Lists[list].elementValues[item][row], Lists[list].elementTypes[row]);
                        }
                    }
                }
            }
            return true;
        }

        private Hashtable loadRules(string file)
        {
            using (var sr = new StreamReader(file))
            {
                Hashtable result = new Hashtable();
                string key = "";
                string value = "";
                string line;
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    Application.DoEvents();

                    if (line != "" && !line.StartsWith("#"))
                    {
                        if (line.Contains("|"))
                        {
                            key = line.Split(new char[] { '|' })[0];
                            value = line.Split(new char[] { '|' })[1];
                        }
                        else
                        {
                            key = line;
                            value = "";
                        }
                        result.Add(key, value);
                    }
                }

                if (!result.ContainsKey("SETCONVERSATIONLISTINDEX"))
                    result.Add("SETCONVERSATIONLISTINDEX", 58);

                return result;
            }
        }

        public void Export(string RulesFile, string TargetFile)
        {
            // Load the rules
            Hashtable rules = loadRules(RulesFile);


            if (File.Exists(TargetFile))
            {
                File.Delete(TargetFile);
            }

            using (BinaryWriter bw = new BinaryWriter((Stream)File.Open(TargetFile, FileMode.Create)))
            {

                if (rules.ContainsKey("SETVERSION"))
                {
                    bw.Write(Convert.ToInt16((string)rules["SETVERSION"]));
                }
                else
                {
                    MessageBox.Show("Rules file is missing parameter\n\nSETVERSION:", "Export Failed");
                    return;
                }

                if (rules.ContainsKey("SETSIGNATURE"))
                {
                    bw.Write(Convert.ToInt16((string)rules["SETSIGNATURE"]));
                }
                else
                {
                    MessageBox.Show("Rules file is missing parameter\n\nSETSIGNATURE:", "Export Failed");
                    return;
                }
                // go through all lists
                for (int l = 0; l < Lists.Length; l++)
                {
                    if (Convert.ToInt16((string)rules["SETCONVERSATIONLISTINDEX"]) == l)
                    {
                        for (int e = 0; e < Lists[ConversationListIndex].elementValues.Count; e++)
                        {
                            // go through all fields of an element
                            for (int f = 0; f < Lists[ConversationListIndex].elementValues[e].Count; f++)
                            {
                                Application.DoEvents();

                                writeValue(bw, Lists[ConversationListIndex].elementValues[e][f], Lists[ConversationListIndex].elementTypes[f]);
                            }
                        }
                    }
                    if (l != ConversationListIndex)
                    {
                        if (!rules.ContainsKey("REMOVELIST:" + l))
                        {

                            if (rules.ContainsKey("REPLACEOFFSET:" + l))
                            {
                                // Convert from Hex to byte[]
                                string[] hex = ((string)rules["REPLACEOFFSET:" + l]).Split(new char[] { '-', ' ' });
                                if (hex.Length > 1)
                                {
                                    byte[] b = new byte[hex.Length];
                                    for (int i = 0; i < hex.Length; i++)
                                    {
                                        b[i] = Convert.ToByte(hex[i], 16);
                                    }
                                    if (b.Length > 0)
                                    {
                                        bw.Write(b);
                                    }
                                }
                            }
                            else
                            {
                                if (Lists[l].listOffset.Length > 0)
                                {
                                    bw.Write(Lists[l].listOffset);
                                }
                            }

                            bw.Write(Lists[l].elementValues.Count);

                            // go through all elements of a list
                            for (int e = 0; e < Lists[l].elementValues.Count; e++)
                            {
                                // go through all fields of an element
                                for (int f = 0; f < Lists[l].elementValues[e].Count; f++)
                                {
                                    Application.DoEvents();

                                    if (!rules.ContainsKey("REMOVEVALUE:" + l + ":" + f))
                                    {
                                        writeValue(bw, Lists[l].elementValues[e][f], Lists[l].elementTypes[f]);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
