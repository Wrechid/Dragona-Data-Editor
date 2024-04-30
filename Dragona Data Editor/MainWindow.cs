using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dragona_Data_Editor
{
    public partial class MainWindow : Form
    {
        public static eListCollection eLC;
        //public static eListCollection[] db;
        public static List<eListCollection> es;

        bool cont;

        public MainWindow()
        {
            InitializeComponent();
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            label_Version.Text = "Wrechid Was Here    v" + fileVersionInfo.ProductVersion;
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog eLoad = new FolderBrowserDialog();
            var menuText = (sender as ToolStripMenuItem).Text;
            string cfg = "NA";

            switch (menuText)
            {
                case "Load Client Configs":
                    cfg = "Client";
                    eLoad.SelectedPath = Properties.Settings.Default.ClientDir;
                    break;
                case "Load Server Configs":
                    cfg = "Server";
                    eLoad.SelectedPath = Properties.Settings.Default.ServerDir;
                    break;
                case "Load Other Configs":
                    cfg = "Other";
                    eLoad.SelectedPath = Properties.Settings.Default.OtherDir;
                    break;
            }
            
            if (eLoad.ShowDialog() == DialogResult.OK)
            {
                if (cfg == "Client" & Properties.Settings.Default.ClientDir != eLoad.SelectedPath)
                {
                    Properties.Settings.Default.ClientDir = eLoad.SelectedPath;
                    Properties.Settings.Default.Save();
                }
                if (cfg == "Server" & Properties.Settings.Default.ServerDir != eLoad.SelectedPath)
                {
                    Properties.Settings.Default.ServerDir = eLoad.SelectedPath;
                    Properties.Settings.Default.Save();
                }
                if (cfg == "Other" & Properties.Settings.Default.OtherDir != eLoad.SelectedPath)
                {
                    Properties.Settings.Default.OtherDir = eLoad.SelectedPath;
                    Properties.Settings.Default.Save();
                }
                _Load(eLoad.SelectedPath, cfg);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (es.Count > 0)
            {
                int t = 0;
                for (int i = 0; i < es.Count; i++)
                {
                    if (es[i].modified)
                    {
                        es[i].Save(es[i].loadedFile);
                        es[i].modified = false;
                        checkBox_modified.Checked = false;
                        t++;
                    }
                }

                MessageBox.Show("Save Complete... " + t + "\n\n : Files Saved...");
            }
        }

        public void _Load(string dir, string opt)
        {
            GC.Collect();

            cont = false;
            comboBox_lists.Items.Clear();
            listBox_items.Rows.Clear();
            dataGridView_item.Rows.Clear();
            checkBox1.Checked = false;
            checkBox_modified.Checked = false;
            label6.Text = "0";
            label7.Text = "0";
            label8.Text = "0";
            label_files_loaded.Text = "0";
            label11.Text = "NA";

            string[] fileArray = Directory.GetFiles(dir, "*.dfs");
            es = new List<eListCollection>();

            for (int i = 0; i < fileArray.Length; i++)
            {
                eLC = new eListCollection();
                eLC.loadedFile = fileArray[i];
                eLC.Lists = eLC.Load(fileArray[i], opt);

                if (eLC.ConfigFile != null)
                {
                    es.Add(eLC);
                    comboBox_lists.Items.Add(eLC.Lists[0].listName);
                }
            }

            for (int i = 0; i < es.Count; i++)
            {
                if (es[i].Lists[0].listName.Contains("Text_"))
                {
                    string val = es[i].Lists[0].listName;
                    val = val.Replace("Text_", "");
                    for (int x = 0; x < es.Count; x++)
                    {
                        if (es[x].Lists[0].listName == val)
                        {
                            es[x].textList = i;
                        }
                    }
                }
            }

            if (comboBox_lists.Items.Count > 0)
            {
                comboBox_lists.SelectedIndex = 0;
                label_files_loaded.Text = es.Count.ToString();
                label11.Text = opt;
                cont = true;
                change_list(null, null);
            }
            else
            {
                MessageBox.Show("No DFS or config files found...");
                cont = true;
            }
        }

        private void change_list(object sender, EventArgs ex)
        {
            if (cont)
            {
                listBox_items.Rows.Clear();
                int s = comboBox_lists.SelectedIndex;
                for (int e = 0; e < es[s].Lists[0].elementValues.Count; e++)
                {
                    if (es[s].textList > -1)
                    {
                        string txtVal = getVal(es[s].textList, es[s].GetValue(0, e, 0));
                        listBox_items.Rows.Add(new object[] { es[s].GetValue(0, e, 0) + "    (" + txtVal + ")" });
                    }
                    else
                        listBox_items.Rows.Add(new object[] { es[s].GetValue(0, e, 0) });
                }
                
                checkBox1.Checked = es[s].b_Unknown;
                label6.Text = es[s].i_Unknown.ToString();
                label7.Text = es[s].i_RecordSize.ToString();
                label8.Text = es[s].i_ColumnCount.ToString();

                checkBox_modified.Checked = es[s].modified;

                selecter_rowscheckbox.Items.Clear();
                for (int opt = 0; opt < es[s].Lists[0].elementFields.Count; opt++)
                {
                    selecter_rowscheckbox.Items.Add(es[s].Lists[0].elementFields[opt]);
                }
                try { selecter_rowscheckbox.SelectedIndex = 0; } catch (Exception) { }
            }
        }

        public string getVal(int l, string id)
        {
            string val = "";
            for (int i = 0; i < es[l].Lists[0].elementValues.Count; i++)
            {
                if (es[l].GetValue(0, i, 0) == id)
                {
                    val = es[l].GetValue(0, i, 1);
                }
            }

            return val;
        }

        private void listBox_items_CellClick(object sender, EventArgs e)
        {
            if (cont & listBox_items.RowCount > 0)
            {
                dataGridView_item.SuspendLayout();
                dataGridView_item.Rows.Clear();

                int s = comboBox_lists.SelectedIndex;
                int element = listBox_items.CurrentCell.RowIndex;

                if (element > -1 && element < es[s].Lists[0].elementValues.Count)
                {
                    Application.DoEvents();
                    for (int f = 0; f < es[s].Lists[0].elementValues[element].Count; f++)
                    {
                        DataGridViewRow row = (DataGridViewRow)dataGridView_item.RowTemplate.Clone();
                        row.CreateCells(dataGridView_item);
                        row.Cells[0].Value = f.ToString();
                        row.Cells[1].Value = es[s].Lists[0].elementFields[f].Replace('_', ' ');
                        row.Cells[2].Value = es[s].Lists[0].GetType(f);
                        row.Cells[3].Value = es[s].GetValue(0, element, f);
                        dataGridView_item.Rows.Add(row);
                    }
                }
                
                dataGridView_item.ResumeLayout();
            }
        }

        private void change_value(object sender, DataGridViewCellEventArgs ea)
        {
            if (cont)
            {
                int s = comboBox_lists.SelectedIndex;
                if (es[s] != null && ea.ColumnIndex == 3 && ea.RowIndex != 0)
                {
                    int e = listBox_items.CurrentCell.RowIndex;
                    string val = Convert.ToString(dataGridView_item.Rows[ea.RowIndex].Cells[3].Value);
                    string type = Convert.ToString(dataGridView_item.Rows[ea.RowIndex].Cells[2].Value);
                    if (type != "int8")
                    {
                        es[s].SetValue(0, e, ea.RowIndex, val);
                    }
                    else
                    {
                        if (Convert.ToInt32(val) < 256)
                            es[s].SetValue(0, e, ea.RowIndex, val);
                        else
                        {
                            MessageBox.Show("Valve cannot be more than 255...");
                            dataGridView_item.Rows[ea.RowIndex].Cells[3].Value = es[s].GetValue(0, e, ea.RowIndex);
                        }
                    }
                    listBox_items.Rows[e].Cells[0].Value = es[s].GetValue(0, e, 0);//ID
                    es[s].modified = true;
                    checkBox_modified.Checked = es[s].modified;
                }
            }
        }

        private void listSameRowsIDValues(object sender, EventArgs ex)
        {
            int list = comboBox_lists.SelectedIndex;
            if (es != null & list != -1)
            {
                dataGridView1.Rows.Clear();
                int nameIndex = -1;
                for (int i = 0; i < es[list].Lists[0].elementValues.Count; i++)
                {
                    if (nameIndex == -1)
                    {
                        for (int f = 0; f < es[list].Lists[0].elementFields.Count; f++)
                        {
                            if (es[list].Lists[0].elementFields[f].ToLower().Equals("name"))
                            {
                                nameIndex = f;
                                break;
                            }

                        }
                    }
                    int e = selecter_rowscheckbox.SelectedIndex;
                    dataGridView1.Rows.Add(new Object[] { es[list].Lists[0].elementFields[e], nameIndex == -1 ? "" : es[list].Lists[0].GetValue(i, nameIndex), es[list].Lists[0].GetValue(i, e), list, i, e });
                }
            }
        }

        private void button15_Click(object sender, EventArgs ex)
        {
            string toestring = textBox23.Text;
            if (toestring.Length <= 0)
            {
                return;
            }

            if (checkBox5.Checked)
            {
                bool isonlyInThisList = checkBox4.Checked;
                bool isCaseSensitive = checkBox_modified.Checked;
                if (isCaseSensitive)
                {
                    toestring = toestring.Trim().ToLower();
                }
                if (isonlyInThisList)
                {
                    int nameIndex = -1;
                    int list = 0;
                    int l = comboBox_lists.SelectedIndex;
                    if (es != null)
                    {
                        dataGridView1.Rows.Clear();

                        if (nameIndex == -1)
                        {
                            for (int f = 0; f < es[l].Lists[list].elementFields.Count; f++)
                            {
                                if (es[l].Lists[list].elementFields[f].ToLower().Equals("name"))
                                {
                                    nameIndex = f;
                                    break;
                                }
                            }
                        }
                        
                        for (int f = 0; f < es[l].Lists[list].elementValues.Count; f++)
                        {
                            Dictionary<int, Object> data = es[l].Lists[list].elementValues[f];
                            for (int i = 0; i < data.Count; i++)
                            {
                                string value = es[l].Lists[list].GetValue(f, i);
                                if (checkBox_modified.Checked)
                                {
                                    if (toestring.Equals(value.Trim().ToLower()))
                                    {
                                        string namex = nameIndex == -1 ? "" : es[l].Lists[list].GetValue(f, nameIndex);
                                        dataGridView1.Rows.Add(new Object[] { es[l].Lists[list].elementFields[i], namex, value, list, f, i });
                                    }
                                }
                                else
                                {
                                    if (value.ToLower().Trim().Contains(toestring.ToLower().Trim()))
                                    {
                                        string namexx = nameIndex == -1 ? "" : es[l].Lists[list].GetValue(f, nameIndex);
                                        dataGridView1.Rows.Add(new Object[] { es[l].Lists[list].elementFields[i], namexx, value, list, f, i });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                toestring = toestring.Trim().ToLower();
                int l = comboBox_lists.SelectedIndex;
                int list = 0;
                if (es != null & list != -1)
                {
                    if (checkBox4.Checked)
                    {
                        dataGridView1.Rows.Clear();

                        string name = selecter_rowscheckbox.Items[selecter_rowscheckbox.SelectedIndex].ToString();
                        int nameIndex = -1;
                        int elementIndex = -1;
                        for (int i = 0; i < es[l].Lists[list].elementValues.Count; i++)
                        {
                            if (nameIndex == -1)
                            {
                                for (int f = 0; f < es[l].Lists[list].elementFields.Count; f++)
                                {
                                    if (es[l].Lists[list].elementFields[f].ToLower().Equals("name"))
                                    {
                                        nameIndex = f;
                                    }
                                    if (es[l].Lists[list].elementFields[f].Equals(name))
                                    {
                                        elementIndex = f;
                                    }
                                    if (nameIndex != -1 && elementIndex != -1)
                                    {
                                        break;
                                    }
                                }
                            }
                            if (elementIndex == -1)
                            {
                                continue;
                            }
                            int e = elementIndex;
                            string value = es[l].Lists[list].GetValue(i, e);
                            if (value.Length == 0)
                            {
                                continue;
                            }
                            if (checkBox_modified.Checked)
                            {
                                if (toestring.Equals(value.Trim().ToLower()))
                                {
                                    string namex = nameIndex == -1 ? "" : es[l].Lists[list].GetValue(i, nameIndex);
                                    dataGridView1.Rows.Add(new Object[] { es[l].Lists[list].elementFields[e], namex, value, list, i, e });
                                }
                            }
                            else
                            {
                                if (value.ToLower().Trim().Contains(toestring.ToLower().Trim()))
                                {
                                    string namexx = nameIndex == -1 ? "" : es[l].Lists[list].GetValue(i, nameIndex);
                                    dataGridView1.Rows.Add(new Object[] { es[l].Lists[list].elementFields[e], namexx, value, list, i, e });
                                }
                            }
                        }
                    }
                    else
                    {
                        dataGridView1.Rows.Clear();
                        for (l = 0; l < es.Count; l++)
                        {
                            string name = selecter_rowscheckbox.Items[selecter_rowscheckbox.SelectedIndex].ToString();
                            int nameIndex = -1;
                            int elementIndex = -1;
                            for (int i = 0; i < es[l].Lists[0].elementValues.Count; i++)
                            {
                                if (nameIndex == -1)
                                {
                                    for (int f = 0; f < es[l].Lists[0].elementFields.Count; f++)
                                    {
                                        if (es[l].Lists[0].elementFields[f].ToLower().Equals("name"))
                                        {
                                            nameIndex = f;
                                        }
                                        if (es[l].Lists[0].elementFields[f].Equals(name))
                                        {
                                            elementIndex = f;
                                        }
                                        if (nameIndex != -1 && elementIndex != -1)
                                        {
                                            break;
                                        }
                                    }
                                }
                                if (elementIndex == -1)
                                {
                                    continue;
                                }
                                int e = elementIndex;
                                string value = es[l].Lists[0].GetValue(i, e);
                                if (value.Length == 0)
                                {
                                    continue;
                                }
                                if (checkBox_modified.Checked)
                                {
                                    if (toestring.Equals(value.Trim().ToLower()))
                                    {
                                        string namex = nameIndex == -1 ? "" : es[l].Lists[0].GetValue(i, nameIndex);
                                        dataGridView1.Rows.Add(new Object[] { es[l].Lists[0].elementFields[e], namex, value, 0, i, e });
                                    }
                                }
                                else
                                {
                                    if (value.ToLower().Trim().Contains(toestring.ToLower().Trim()))
                                    {
                                        string namexx = nameIndex == -1 ? "" : es[l].Lists[0].GetValue(i, nameIndex);
                                        dataGridView1.Rows.Add(new Object[] { es[l].Lists[0].elementFields[e], namexx, value, 0, i, e });
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private int f = 0;
        private void click_search(object sender, EventArgs ea)
        {
            string id = textBox_search.Text;
            if (es != null && id != "")
            {
                btnSearch.Enabled = false;
                if (!checkBox_SearchMatchCase.Checked)
                    id = id.ToLower();
                int l = comboBox_lists.SelectedIndex;
                int e = listBox_items.CurrentCell.RowIndex;
                for (int lf = l; lf < es.Count; lf++)
                {
                    if (e < 0 || e > es[lf].Lists[0].elementValues.Count) { e = 0; }
                    for (int ef = e; ef < es[lf].Lists[0].elementValues.Count; ef++)
                    {
                        if (f < 0 || f > es[lf].Lists[0].elementFields.Count) { f = 0; }
                        for (int ff = f; ff < es[lf].Lists[0].elementFields.Count; ff++)
                        {
                            string value = es[lf].GetValue(0, ef, ff);
                            if (!checkBox_SearchMatchCase.Checked)
                                value = value.ToLower();
                            if (!checkBox_SearchExactMatching.Checked && value.Contains(id))
                            {
                                comboBox_lists.SelectedIndex = lf;
                                listBox_items.CurrentCell = listBox_items.Rows[ef].Cells[0];
                                listBox_items_CellClick(null, null);
                                dataGridView_item.FirstDisplayedScrollingRowIndex = ff;
                                dataGridView_item.CurrentCell = dataGridView_item.Rows[ff].Cells[2];
                                f = ff + 1;
                                btnSearch.Enabled = true;
                                Application.DoEvents();
                                return;
                            }
                            if (checkBox_SearchExactMatching.Checked && value == id)
                            {
                                comboBox_lists.SelectedIndex = lf;
                                listBox_items.CurrentCell = listBox_items.Rows[ef].Cells[0];
                                listBox_items_CellClick(null, null);
                                dataGridView_item.FirstDisplayedScrollingRowIndex = ff;
                                dataGridView_item.CurrentCell = dataGridView_item.Rows[ff].Cells[2];
                                f = ff + 1;
                                btnSearch.Enabled = true;
                                Application.DoEvents();
                                return;
                            }
                            if (lf == es.Count - 1 && ef == es[lf].Lists[0].elementValues.Count - 1 && ff == es[lf].Lists[0].elementFields.Count - 1)
                            {
                                comboBox_lists.SelectedIndex = 0;
                                MessageBox.Show("None found, or end of list...");
                            }
                        }
                        f = 0;
                    }
                }
                btnSearch.Enabled = true;
            }
        }

        private Dictionary<int, Dictionary<int, Object>> resortDic(IDictionary<int, Dictionary<int, Object>> data)
        {
            Dictionary<int, Dictionary<int, Object>> datanew = new Dictionary<int, Dictionary<int, Object>>();
            int i = 0;
            foreach (KeyValuePair<int, Dictionary<int, Object>> entry in data)
            {
                datanew[i] = entry.Value;
                i++;
            }
            return datanew;
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs ex)
        {
            int l = comboBox_lists.SelectedIndex;
            if (l > -1)
            {
                Dictionary<int, Object> itemValues = new Dictionary<int, Object>();
                for (int i = 0; i < es[l].Lists[0].elementFields.Count; i++)
                {
                    Object data = (Object)es[l].getDefaultValue(es[l].Lists[0].elementTypes[i].ToString());
                    itemValues.Add(i, data);
                }
                es[l].AddItem(l, itemValues);
                listBox_items.Rows.Add(new object[] { es[l].GetValue(0, es[l].Lists[0].elementValues.Count - 1, 0) });
                listBox_items.Rows[es[l].Lists[0].elementValues.Count - 1].Selected = true;
                listBox_items.CurrentCell = listBox_items.Rows[es[l].Lists[0].elementValues.Count - 1].Cells[0];

                es[l].modified = true;
                checkBox_modified.Checked = true;
            }
            else
            {
                MessageBox.Show("Please select an item!");
            }
            listBox_items_CellClick(null, null);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (comboBox_lists.SelectedIndex > -1)
            {
                cont = false;
                int l = comboBox_lists.SelectedIndex;
                int xe = listBox_items.CurrentCell.RowIndex;
                DataGridViewSelectedRowCollection selected = listBox_items.SelectedRows;
                for (int x = 0; x < listBox_items.SelectedRows.Count; x++)
                {
                    System.Windows.Forms.Application.DoEvents();
                    int idx = listBox_items.SelectedRows[x].Index;
                    es[l].Lists[0].RemoveItem(l, idx);
                }
                for (int i = selected.Count - 1; i >= 0; i--)
                {
                    listBox_items.Rows.Remove(selected[i]);
                }
                es[l].Lists[0].elementValues = resortDic(es[l].Lists[0].elementValues);
                cont = true;

                es[l].modified = true;
                checkBox_modified.Checked = true;
            }
            else
            {
                MessageBox.Show("Please select an item!");
            }
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs ex)
        {

            if (comboBox_lists.SelectedIndex == -1)
            {
                return;
            }
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog1.Filter = "DFS Export(*.edx) | *.edx";
            int ltc = comboBox_lists.SelectedIndex;
            // Process input if the user clicked OK.
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string pathx = openFileDialog1.FileName;
                Application.DoEvents();
                using (Stream file = File.Open(pathx, FileMode.Open))
                {

                    BinaryFormatter bf = new BinaryFormatter();
                    Export obj = (Export)bf.Deserialize(file);
                    if (obj.ForVersion != es[ltc].Version)
                    {
                        cont = true;
                        comboBox_lists.Enabled = true;
                        listBox_items.Enabled = true;
                        file.Close();
                        if (!(MessageBox.Show("You are about to import " + obj.ForVersion + " on " + es[ltc].Version + "! Do you want to continue?", "Please confirm!", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes))
                        {
                            return;
                        }
                    }
                    comboBox_lists.SelectedIndex = obj.ListId;
                    ltc = obj.ListId;
                    Application.DoEvents();
                    comboBox_lists.Enabled = false;
                    listBox_items.Enabled = false;
                    cont = false;
                    foreach (KeyValuePair<int, object> entry in obj.data)
                    {
                        Application.DoEvents();
                        Dictionary<int, object> data = (Dictionary<int, object>)entry.Value;

                        es[obj.ListId].Lists[0].AddItem(obj.ListId, data);
                    }
                    file.Close();
                }
                cont = true;
                comboBox_lists.Enabled = true;
                listBox_items.Enabled = true;
                change_list(null, null);
                listBox_items.Rows[es[ltc].Lists[0].elementValues.Count - 1].Selected = true;
                listBox_items.CurrentCell = listBox_items.Rows[es[ltc].Lists[0].elementValues.Count - 1].Cells[0];
            }
            cont = true;

            es[ltc].modified = true;
            checkBox_modified.Checked = true;

            MessageBox.Show("Import Complete...");
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (comboBox_lists.SelectedIndex == -1)
            {
                return;
            }

            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "DFS Export (*.edx)|*.edx";
            save.FileName = comboBox_lists.Text + ".edx";
            int l = comboBox_lists.SelectedIndex;
            int xe = listBox_items.CurrentCell.RowIndex;
            if (xe > -1 && save.ShowDialog() == DialogResult.OK && save.FileName != "")
            {
                try
                {
                    cont = false;

                    if (listBox_items.CurrentCell.RowIndex != -1)
                    {
                        Export export = new Export();
                        export.ListId = l;
                        export.type = 0; //Elements data = 0 | Gshop = 1 
                        export.ForVersion = es[l].Version;
                        export.data = new SortedDictionary<int, object>(new ReverseComparer<int>(Comparer<int>.Default));
                        for (int i = 0; i < listBox_items.SelectedRows.Count; i++)
                        {
                            int index = listBox_items.SelectedRows[i].Index;
                            export.data.Add(i, es[l].Lists[0].elementValues[index]);
                        }
                        FileStream fs = new FileStream(save.FileName, FileMode.Create, FileAccess.Write);
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Serialize(fs, export);
                        fs.Close();
                    }

                    cont = true;
                }
                catch { }

                MessageBox.Show("Export Complete...");
            }
            else
            {
                MessageBox.Show("Please select a item!");
            }
        }

        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dataGridView1.CurrentCell != null && eLC != null)
            {
                int rowindex = dataGridView1.CurrentCell.RowIndex;
                int columnindex = dataGridView1.CurrentCell.ColumnIndex;
                int list = int.Parse(dataGridView1.Rows[rowindex].Cells[3].Value.ToString());
                int element = int.Parse(dataGridView1.Rows[rowindex].Cells[4].Value.ToString());
                int row = int.Parse(dataGridView1.Rows[rowindex].Cells[5].Value.ToString());
                comboBox_lists.SelectedIndex = list;
                listBox_items.ClearSelection();
                dataGridView_item.ClearSelection();
                listBox_items.Rows[element].Selected = true;
                listBox_items.CurrentCell = listBox_items.Rows[element].Cells[0];
                listBox_items_CellClick(null, null);
                dataGridView_item.Rows[row].Selected = true;
                dataGridView_item.CurrentCell = dataGridView_item.Rows[row].Cells[0];
                tabControl1.SelectedIndex = 0;
                listBox_items.PerformLayout();
                dataGridView_item.PerformLayout();
            }
        }

        private void copyAllItemNamesToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {

            List<string> values = new List<string>();
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                values.Add(dataGridView1.Rows[i].Cells[1].Value.ToString());
            }
            Clipboard.SetText(string.Join(",", values.ToArray()));

        }

        private void copyAllItemNamesToClipboardToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            List<string> values = new List<string>();
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                values.Add(dataGridView1.Rows[i].Cells[2].Value.ToString());
            }
            Clipboard.SetText(string.Join(",", values.ToArray()));
        }

        public void CopyToClipboard(Export objectToCopy)
        {
            string format = "MyImporting";
            Clipboard.Clear();
            Clipboard.SetData(format, objectToCopy);
        }

        private void copySelectedItemsToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (es != null)
            {
                cont = false;
                List<string> values = new List<string>();
                Export export = new Export();
                export.ListId = -1;
                export.type = 0; //Elements data = 0 | Gshop = 1 
                export.ForVersion = es[0].Version;
                export.data = new SortedDictionary<int, object>(new ReverseComparer<int>(Comparer<int>.Default));


                for (int i = 0; i < dataGridView1.SelectedRows.Count; i++)
                {
                    int index = dataGridView1.SelectedRows[i].Index;
                    int listToCopyFrom = int.Parse(dataGridView1.Rows[index].Cells[3].Value.ToString());
                    int itemToCopyFrom = int.Parse(dataGridView1.Rows[index].Cells[4].Value.ToString());
                    Export exportx = new Export();
                    exportx.ListId = listToCopyFrom;
                    exportx.type = 0; //Elements data = 0 | Gshop = 1 
                    exportx.ForVersion = es[0].Version;
                    exportx.data = new SortedDictionary<int, object>(new ReverseComparer<int>(Comparer<int>.Default));

                    exportx.data.Add(i, es[listToCopyFrom].Lists[0].elementValues[itemToCopyFrom]);
                    
                    export.data.Add(i, exportx);
                }

                CopyToClipboard(export);
                cont = true;
            }
        }

        private void exportCurrentListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (comboBox_lists.SelectedIndex == -1)
            {
                return;
            }

            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "CVS File (*.csv)|*.csv";
            int l = comboBox_lists.SelectedIndex;
            int xe = listBox_items.CurrentCell.RowIndex;
            if (xe > -1 && save.ShowDialog() == DialogResult.OK && save.FileName != "")
            {
                using (var w = new StreamWriter(save.FileName))
                {
                    cont = false;

                    if (listBox_items.CurrentCell.RowIndex != -1)
                    {
                        int c = listBox_items.SelectedRows.Count;

                        string[] head = new string[es[l].Lists[0].elementFields.Count];
                        for (int h = 0; h < head.Length; h++)
                        {
                            head[h] = es[l].Lists[0].elementFields[h];
                        }

                        w.Write(String.Join(",", head) + Environment.NewLine);
                        w.Flush();

                        for (int el = 0; el < es[l].Lists[0].elementValues.Count; el++)
                        {
                            string[] val = new string[head.Length];
                            for (int fl = 0; fl < val.Length; fl++)
                            {
                                val[fl] = es[l].GetValue(0, el, fl);
                            }

                            w.Write(String.Join(",", val) + Environment.NewLine);
                            w.Flush();

                        }
                    }

                    cont = true;

                    w.Close();
                }

                MessageBox.Show("Export Complete...");
            }
            else
            {
                MessageBox.Show("Please select a item!");
            }
        }

        private void exportSelectedToCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (comboBox_lists.SelectedIndex == -1)
            {
                return;
            }

            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "CVS File (*.csv)|*.csv";
            int l = comboBox_lists.SelectedIndex;
            save.FileName = comboBox_lists.Text;
            int xe = listBox_items.CurrentCell.RowIndex;
            if (xe > -1 && save.ShowDialog() == DialogResult.OK && save.FileName != "")
            {
                using (var w = new StreamWriter(save.FileName))
                {
                    cont = false;

                    if (listBox_items.CurrentCell.RowIndex != -1)
                    {
                        int c = listBox_items.SelectedRows.Count;

                        string[] head = new string[es[l].Lists[0].elementFields.Count];
                        for (int h = 0; h < head.Length; h++)
                        {
                            head[h] = es[l].Lists[0].elementFields[h];
                        }

                        w.Write(String.Join(",", head) + Environment.NewLine);
                        w.Flush();

                        for (int i = 0; i < c; i++)
                        {
                            int el = listBox_items.SelectedRows[i].Index;
                            
                            string[] val = new string[head.Length];
                            for (int fl = 0; fl < val.Length; fl++)
                            {
                                val[fl] = es[l].GetValue(0, el, fl).Replace("\0", "");
                            }

                            w.Write(String.Join(",", val) + Environment.NewLine);
                            w.Flush();
                        }
                    }
                    
                    cont = true;

                    w.Close();
                }

                MessageBox.Show("Export Complete...");
            }
            else
            {
                MessageBox.Show("Please select a item!");
            }
        }
    }
}
