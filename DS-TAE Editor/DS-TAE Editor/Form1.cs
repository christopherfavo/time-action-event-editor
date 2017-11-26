using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace DS_TAE_Editor
{
    public partial class Form1 : Form
    {
        public TAE.Tae tae;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void textDrag(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            textBox1.Text = files[0];
        }

        private void OpenBtn_Click(object sender, EventArgs e)
        {
            Open();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResetState();

            int i = 0;
            int max = 0;

            foreach (TAE.EventStruct Event in tae.data.animDatas[listBox1.SelectedIndex].events)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = i});
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = Event.eventType.ToString() });
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = Event.parameters.Count.ToString() });
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = Event.startTime.ToString() });
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = Event.endTime.ToString() });

                row.Cells[0].ReadOnly = true;
                row.Cells[2].ReadOnly = true;

                for (int j = 0; j < Event.parameters.Count; j++)
                {
                    if (Event.parameters.Count > dataGridView1.ColumnCount - 5)
                    {
                        DataGridViewColumn column = new DataGridViewColumn();
                        column.Name = "Column" + max + 5;
                        column.HeaderText = "Parameter #" + (max + 1);
                        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        column.CellTemplate = new DataGridViewTextBoxCell();

                        dataGridView1.Columns.Add(column);
                        
                        max++;
                    }

                    row.Cells.Add(new DataGridViewTextBoxCell());
                    if (Helper.ok)
                    {
                        bool found = false;
                        int index = 0;
                        
                        while (!found && index < Helper.helpers.Count)
                        {
                            if (Helper.helpers[index].id == Event.eventType)
                            {                                
                                switch (Helper.helpers[index].parameterTypes[j])
                                {
                                    case "byte":
                                    case "ubyte":
                                        row.Cells[j + 5].Value = ToByte(Event.parameters[j]).ToString();
                                        break;

                                    case "short":
                                        row.Cells[j + 5].Value = BitConverter.ToInt16(Event.parameters[j], 0).ToString();
                                        break;

                                    case "ushort":
                                        row.Cells[j + 5].Value = BitConverter.ToUInt16(Event.parameters[j], 0).ToString();
                                        break;

                                    case "int":
                                        row.Cells[j + 5].Value = BitConverter.ToInt32(Event.parameters[j], 0).ToString();
                                        break;
                                    case "uint":
                                        row.Cells[j + 5].Value = BitConverter.ToUInt32(Event.parameters[j], 0).ToString();
                                        break;

                                    case "float":
                                        row.Cells[j + 5].Value = BitConverter.ToSingle(Event.parameters[j], 0).ToString();
                                        break;

                                    case "long":
                                        row.Cells[j + 5].Value = BitConverter.ToInt64(Event.parameters[j], 0).ToString();
                                        break;

                                    case "ulong":
                                        row.Cells[j + 5].Value = BitConverter.ToUInt64(Event.parameters[j], 0).ToString();
                                        break;

                                    case "double":
                                        row.Cells[j + 5].Value = BitConverter.ToDouble(Event.parameters[j], 0).ToString();
                                        break;

                                    default:
                                        break;
                                }
                                
                                found = true;
                            }
                            else
                            {
                                index++;
                            }
                        }
                    }
                    else
                    {
                        row.Cells[j + 5].Value = BitConverter.ToInt32(Event.parameters[j], 0).ToString();
                    }
                }

                dataGridView1.Rows.Add(row);
                i++;
            }
        }

        private byte ToByte(byte[] parameter)
        {
            return parameter[0];
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index >= 0)
            {
                string output = listBox1.Items[e.Index].ToString();
                float olength = e.Graphics.MeasureString(output, e.Font).Width;
                float pos = listBox1.Width - 25 - olength;

                SolidBrush brushBack = new SolidBrush(e.BackColor);
                e.Graphics.FillRectangle(brushBack, e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);


                SolidBrush brush = new SolidBrush(e.ForeColor);
                e.Graphics.DrawString(output, e.Font, brush, pos, e.Bounds.Top);
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            int eventIndex = (int)dataGridView1.Rows[e.RowIndex].Cells[0].Value;

            TAE.EventStruct Event = tae.data.animDatas[listBox1.SelectedIndex].events[eventIndex];
                        
            if (e.ColumnIndex == 1)
            {
                int paramCount = 0;

                if (Helper.ok)
                {
                    bool found = false;
                    int index = 0;
                    uint value = 0;

                    if (!uint.TryParse((string)dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value, out value))
                    {
                        MessageBox.Show("Invalid value for this type. Type of this cell:\n\nuint");
                        dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Event.eventType.ToString();
                        return;
                    }
                    else
                    {
                        while (!found && index < Helper.helpers.Count)
                        {
                            if (Helper.helpers[index].id == value)
                            {
                                paramCount = Helper.helpers[index].parameterTypes.Count;
                                found = true;
                            }
                            else
                            {
                                index++;
                            }
                        }
                    }
                }
                else
                {
                    int value = 0;
                    if (!int.TryParse((string)dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value, out value))
                    {
                        MessageBox.Show("Invalid value for this type. Type of this cell:\n\nint");
                        dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Event.eventType.ToString();
                        return;
                    }
                    else
                    {
                        switch (value)
                        {
                            case 32:
                            case 33:
                            case 65:
                            case 66:
                            case 67:
                            case 101:
                            case 110:
                            case 145:
                            case 224:
                            case 225:
                            case 226:
                            case 229:
                            case 231:
                            case 232:
                            case 301:
                            case 302:
                            case 308:
                            case 401:
                                paramCount = 1;
                                break;

                            case 5:
                            case 64:
                            case 112:
                            case 121:
                            case 128:
                            case 193:
                            case 233:
                            case 304:
                                paramCount = 2;
                                break;

                            case 0:
                            case 1:
                            case 96:
                            case 100:
                            case 104:
                            case 109:
                            case 114:
                            case 115:
                            case 116:
                            case 118:
                            case 144:
                            case 228:
                            case 236:
                            case 307:
                                paramCount = 3;
                                break;

                            case 2:
                            case 16:
                            case 24:
                            case 130:
                            case 300:
                                paramCount = 4;
                                break;

                            case 120:
                                paramCount = 6;
                                break;

                            case 8:
                                paramCount = 12;
                                break;

                            default:
                                paramCount = 0;
                                break;
                        }
                    }
                }

                dataGridView1.Rows[e.RowIndex].Cells[2].Value = paramCount;

                while (paramCount + 5 > dataGridView1.ColumnCount)
                {
                    DataGridViewColumn column = new DataGridViewColumn();
                    column.Name = "Column" + (dataGridView1.ColumnCount - 4);
                    column.HeaderText = "Parameter #" + (dataGridView1.ColumnCount - 4);
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    column.CellTemplate = new DataGridViewTextBoxCell();

                    dataGridView1.Columns.Add(column);
                }

                /*
                for (int i = 0; i < paramCount; i++)
                {
                    dataGridView1.Rows[e.RowIndex].Cells[i + 5].Value = "0";                   
                }
                */

                for (int i = 0; i < dataGridView1.ColumnCount - 5; i++)
                {
                    if (i < paramCount) dataGridView1.Rows[e.RowIndex].Cells[i + 5].Value = "0";
                    else dataGridView1.Rows[e.RowIndex].Cells[i + 5] = new DataGridViewTextBoxCell();
                }

                Event.parameters = new List<byte[]> { };
                
                for (int i = 0; i < paramCount; i++)
                {
                    byte[] byteA = BitConverter.GetBytes(int.Parse((string)dataGridView1.Rows[e.RowIndex].Cells[i + 5].Value));
                    Event.parameters.Add(byteA);
                }
                
            }

            DataGridViewCellCollection cells = dataGridView1.Rows[e.RowIndex].Cells;

            string invalid = "Invalid value for this type. Type of this cell:\n\n";

            switch (e.ColumnIndex)
            {
                case 0:
                    break;

                case 1:
                    if (!uint.TryParse((string)cells[1].Value, out Event.eventType))
                    {
                        MessageBox.Show(invalid + "uint");
                        dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Event.eventType;
                    }
                    break;

                case 2:
                    break;

                case 3:
                    float value1 = 0;
                    if (!float.TryParse((string)cells[3].Value, out value1))
                    {
                        MessageBox.Show(invalid + "float");
                        dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Event.startTime;
                    }
                    else
                    {
                        Event.startTime = value1;

                        bool l = false;
                        uint i = 0;

                        while (!l && i < tae.data.animDatas[listBox1.SelectedIndex].timeConstantsCount)
                        {
                            if (tae.data.animDatas[listBox1.SelectedIndex].timeConstants[(int)i] == Event.startTime)
                            {
                                l = true;
                                Event.startTimeOffset = i * 4;
                            }

                            i++;
                        }

                        if (!l)
                        {
                            tae.data.animDatas[listBox1.SelectedIndex].timeConstants.Add(Event.startTime);
                            tae.data.animDatas[listBox1.SelectedIndex].timeConstantsCount++;
                            Event.startTimeOffset = (tae.data.animDatas[listBox1.SelectedIndex].timeConstantsCount - 1) * 4;
                        }

                        CheckForUnreferencedConstant();
                    }

                    break;

                case 4:
                    value1 = 0;
                    if (!float.TryParse((string)cells[4].Value, out value1))
                    {
                        MessageBox.Show(invalid + "float");
                        dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = Event.endTime;
                    }
                    else
                    {
                        Event.endTime = value1;

                        bool l = false;
                        uint i = 0;

                        while (!l && i < tae.data.animDatas[listBox1.SelectedIndex].timeConstantsCount)
                        {
                            if (tae.data.animDatas[listBox1.SelectedIndex].timeConstants[(int)i] == Event.endTime)
                            {
                                l = true;
                                Event.endTimeOffset = i * 4;
                            }

                            i++;
                        }

                        if (!l)
                        {
                            tae.data.animDatas[listBox1.SelectedIndex].timeConstants.Add(Event.endTime);
                            tae.data.animDatas[listBox1.SelectedIndex].timeConstantsCount++;
                            Event.endTimeOffset = (tae.data.animDatas[listBox1.SelectedIndex].timeConstantsCount - 1) * 4;
                        }

                        CheckForUnreferencedConstant();
                    }
                    break;

                default:
                    if (e.ColumnIndex - 5 < Event.parameters.Count)
                    {
                        if (Helper.ok)
                        {
                            bool found = false;
                            int index = 0;

                            while (!found && index < Helper.helpers.Count)
                            {
                                if (Helper.helpers[index].id == Event.eventType)
                                {
                                    switch (Helper.helpers[index].parameterTypes[e.ColumnIndex - 5])
                                    {
                                        case "byte":
                                            sbyte sbValue = 0;
                                            if (!sbyte.TryParse((string)cells[e.ColumnIndex].Value, out sbValue))
                                            {
                                                MessageBox.Show(invalid + "byte");
                                                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = ToByte(Event.parameters[e.ColumnIndex - 5]);
                                            }
                                            else Event.parameters[e.ColumnIndex - 5] = BitConverter.GetBytes(sbValue);
                                            break;

                                        case "ubyte":
                                            byte bValue = 0;
                                            if (!byte.TryParse((string)cells[e.ColumnIndex].Value, out bValue))
                                            {
                                                MessageBox.Show(invalid + "byte");
                                                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = ToByte(Event.parameters[e.ColumnIndex - 5]);
                                            }
                                            else Event.parameters[e.ColumnIndex - 5] = BitConverter.GetBytes(bValue);
                                            break;

                                        case "short":
                                            short sValue = 0;
                                            if (!short.TryParse((string)cells[e.ColumnIndex].Value, out sValue))
                                            {
                                                MessageBox.Show(invalid + "short");
                                                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = BitConverter.ToInt16(Event.parameters[e.ColumnIndex - 5], 0);
                                            }
                                            else Event.parameters[e.ColumnIndex - 5] = BitConverter.GetBytes(sValue);
                                            break;

                                        case "ushort":
                                            ushort usValue = 0;
                                            if (!ushort.TryParse((string)cells[e.ColumnIndex].Value, out usValue))
                                            {
                                                MessageBox.Show(invalid + "ushort");
                                                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = BitConverter.ToUInt16(Event.parameters[e.ColumnIndex - 5],0);
                                            }
                                            else Event.parameters[e.ColumnIndex - 5] = BitConverter.GetBytes(usValue);
                                            break;

                                        case "int":
                                            int iValue = 0;
                                            if (!int.TryParse((string)cells[e.ColumnIndex].Value, out iValue))
                                            {
                                                MessageBox.Show(invalid + "int");
                                                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = BitConverter.ToInt32(Event.parameters[e.ColumnIndex - 5], 0);
                                            }
                                            else Event.parameters[e.ColumnIndex - 5] = BitConverter.GetBytes(iValue);
                                            break;

                                        case "uint":
                                            uint uiValue = 0;
                                            if (!uint.TryParse((string)cells[e.ColumnIndex].Value, out uiValue))
                                            {
                                                MessageBox.Show(invalid + "uint");
                                                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = BitConverter.ToUInt32(Event.parameters[e.ColumnIndex - 5], 0);
                                            }
                                            else Event.parameters[e.ColumnIndex - 5] = BitConverter.GetBytes(uiValue);
                                            break;

                                        case "float":
                                            float fValue = 0;
                                            if (!float.TryParse((string)cells[e.ColumnIndex].Value, out fValue))
                                            {
                                                MessageBox.Show(invalid + "float");
                                                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = BitConverter.ToSingle(Event.parameters[e.ColumnIndex - 5], 0);
                                            }
                                            else Event.parameters[e.ColumnIndex - 5] = BitConverter.GetBytes(fValue);
                                            break;

                                        case "long":
                                            long lValue = 0;
                                            if (!long.TryParse((string)cells[e.ColumnIndex].Value, out lValue))
                                            {
                                                MessageBox.Show(invalid + "long");
                                                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = BitConverter.ToInt64(Event.parameters[e.ColumnIndex - 5], 0);
                                            }
                                            else Event.parameters[e.ColumnIndex - 5] = BitConverter.GetBytes(lValue);
                                            break;

                                        case "ulong":
                                            ulong ulValue = 0;
                                            if (!ulong.TryParse((string)cells[e.ColumnIndex].Value, out ulValue))
                                            {
                                                MessageBox.Show(invalid + "ulong");
                                                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = BitConverter.ToUInt64(Event.parameters[e.ColumnIndex - 5], 0);
                                            }
                                            else Event.parameters[e.ColumnIndex - 5] = BitConverter.GetBytes(ulValue);
                                            break;

                                        case "double":
                                            double dValue = 0;
                                            if (!double.TryParse((string)cells[e.ColumnIndex].Value, out dValue))
                                            {
                                                MessageBox.Show(invalid + "double");
                                                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = BitConverter.ToDouble(Event.parameters[e.ColumnIndex - 5], 0);
                                            }
                                            else Event.parameters[e.ColumnIndex - 5] = BitConverter.GetBytes(dValue);
                                            break;

                                        default:
                                            break;
                                    }

                                    found = true;
                                }
                                else
                                {
                                    index++;
                                }
                            }
                        }
                        else
                        {
                            int value = 0;
                            if (!int.TryParse((string)cells[e.ColumnIndex].Value, out value))
                            {
                                MessageBox.Show(invalid + "int");
                                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = BitConverter.ToInt32(Event.parameters[e.ColumnIndex - 5], 0).ToString();
                            }
                            else Event.parameters[e.ColumnIndex - 5] = BitConverter.GetBytes(value);
                        }
                    }
                    break;
            }

            tae.data.animDatas[listBox1.SelectedIndex].events[eventIndex] = Event;
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            SaveBtn.Enabled = false;

            if (tae != null)
            {
                if (!File.Exists(textBox1.Text + ".bak"))
                {
                    File.Move(textBox1.Text, textBox1.Text + ".bak");
                }

                TAE.WriteTae(tae, textBox1.Text);
            }

            SaveBtn.Enabled = true;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            textBox1_DragDrop(sender, e);
            Open();
        }

        private void Open()
        {
            tae = new TAE.Tae();
            listBox1.Items.Clear();
            ResetState();

            if (File.Exists(textBox1.Text))
            {
                tae = TAE.ReadTae(textBox1.Text);

                if (tae.err != null) MessageBox.Show(tae.err);
                else
                {
                    foreach (TAE.IdStruct id in tae.data.ids)
                    {
                        listBox1.Items.Add(id.id.ToString());
                    }
                }
            }
        }

        private void ResetState()
        {
            dataGridView1.Rows.Clear();

            for (int j = dataGridView1.ColumnCount - 1; j > 4; j--)
            {
                dataGridView1.Columns.Remove(dataGridView1.Columns[j]);
            }
        }

        private void AddEventBtn_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count != 0 && listBox1.SelectedIndex != -1)
            {
                if (dataGridView1.Rows.Count == 0)
                {
                    tae.data.animDatas[listBox1.SelectedIndex].timeConstants.Add(0);
                    tae.data.animDatas[listBox1.SelectedIndex].timeConstantsCount++;
                }

                TAE.AddEvent(tae.data.animDatas[listBox1.SelectedIndex]);
                listBox1_SelectedIndexChanged(sender, e);
                dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.Rows.Count - 1;
                                
            }
        }

        private void DelEventBtn_Click(object sender, EventArgs e)
        {

            if (dataGridView1.RowCount != 0)
            {
                tae.data.animDatas[listBox1.SelectedIndex].events.Remove(tae.data.animDatas[listBox1.SelectedIndex].events[(int)dataGridView1.Rows[dataGridView1.SelectedCells[0].RowIndex].Cells[0].Value]);
                tae.data.animDatas[listBox1.SelectedIndex].eventCount--;

                dataGridView1.Rows.Remove(dataGridView1.Rows[dataGridView1.SelectedCells[0].RowIndex]);

                CheckForUnreferencedConstant();
                CheckForUnreferencedConstant();
            }

        }

        private void CheckForUnreferencedConstant()
        {
            TAE.AnimDataStruct animData = tae.data.animDatas[listBox1.SelectedIndex];
            List<int> references = new List<int> (new int [(int)animData.timeConstantsCount]);
            
            foreach (TAE.EventStruct Event in animData.events)
            {
                references[(int)Event.startTimeOffset / 4]++;
                references[(int)Event.endTimeOffset / 4]++;
            }

            bool l = false;
            int i = 0;

            while (!l && i < animData.timeConstantsCount)
            {
                if (references[i] == 0)
                {
                    animData.timeConstants.Remove(animData.timeConstants[i]);
                    animData.timeConstantsCount--;

                    l = true;
                }
                else
                {
                    i++;
                }
            }

            if (l)
            {
                for (int j = 0; j < animData.eventCount; j++)
                {
                    if (animData.events[j].startTimeOffset > i * 4)
                    {
                        animData.events[j].startTimeOffset -= 4;
                    }

                    if (animData.events[j].endTimeOffset > i * 4)
                    {
                        animData.events[j].endTimeOffset -= 4;
                    }
                }
            }

            tae.data.animDatas[listBox1.SelectedIndex] = animData;
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                for (int i = 0; i < listBox1.Items.Count; i++)
                {
                    if ((string)listBox1.Items[i] == textBox2.Text)
                    {
                        listBox1.SelectedIndex = i;
                    }
                }
            }
        }

        private void textBox2_Enter(object sender, EventArgs e)
        {
            if (textBox2.Text == "Search bar")
            {
                textBox2.Text = "";
                textBox2.ForeColor = Color.Black;
            }
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            if (textBox2.Text == "")
            {
                textBox2.Text = "Search bar";
                textBox2.ForeColor = Color.Silver;
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tae != null && saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filename = saveFileDialog1.FileName;

                if (filename.Substring(filename.Length - 4) != ".tae") filename += ".tae";

                TAE.WriteTae(tae, filename);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveBtn_Click(sender, e);
        }

        private void closeTAEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tae = null;

            dataGridView1.Rows.Clear();

            listBox1.Items.Clear();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void AddAnim(int newId)
        {
            TAE.AnimDataStruct animData = new TAE.AnimDataStruct();
            TAE.IdStruct id = new TAE.IdStruct();
            TAE.GroupStruct group = new TAE.GroupStruct();

            animData.animFile = new TAE.AnimFileStruct();
            animData.animFile.name = new TAE.NameStruct();

            List<byte> name = new List<byte> (Encoding.Unicode.GetBytes(Path.GetFileName(textBox1.Text).Split('.')[0] + "_" + newId.ToString().PadLeft(4, '0') + ".hkxwin"));
            name.Add(0x00);
            name.Add(0x00);
                        
            animData.animFile.name.name = Encoding.UTF8.GetString(name.ToArray());
            animData.events = new List<TAE.EventStruct> { };
            animData.timeConstants = new List<float> { };

            id.id = (uint)newId;

            group.firstId = id.id;
            group.lastId = id.id;

            tae.data.animDatas.Add(animData);
            tae.header.dataCount++;

            tae.data.ids.Add(id);
            tae.header.idCount++;

            tae.data.groups.Add(group);
            tae.data.groupCount++;

            listBox1.Items.Add(newId);
        }

        private void NewIdBtn_Click(object sender, EventArgs e)
        {
            if (tae != null)
            {
                int id = -1;
                if (int.TryParse(textBox2.Text, out id))
                {
                    bool exists = false;
                    int i = 0;

                    while(!exists && i < listBox1.Items.Count)
                    {
                        exists = (string)listBox1.Items[i] == id.ToString();
                        i++;
                    }

                    if (!exists)
                    {
                        AddAnim(id);
                        listBox1.SelectedIndex = listBox1.Items.Count - 1;
                    }
                    else MessageBox.Show("Animation ID " + id + " already exists in the current file.");
                }
                else
                {
                    MessageBox.Show("\"" + textBox2.Text + "\" is not a valid number.");
                }
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            string couldnt = "Could not load \"helper.txt\".\n\nReason:\n";
            string def = "\n\nUsing default values.";

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "helper.txt"))
            {
                Helper.ReadHelper();

                if (!Helper.ok)
                {
                    MessageBox.Show(couldnt + "Either it is not formatted correctly, or some data values are invalid." + def);
                }
                else
                {
                    helperInfo.Text = "Helper Info from \"helper.txt\" has loaded successfully!\nClick \"Help - > Helper Info\" to access event descriptions.";
                }
            }
            else
            {
                MessageBox.Show(couldnt + "The file does not exist." + def);
            }
        }

        private void helpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (Helper.ok)
            {
                Form2 helperForm = new Form2();
                helperForm.Show();
            }
            else
            {
                MessageBox.Show("Helper Info cannot be shown, since \"helper.txt\" is corrupt or missing.\n\nIf the problem was resolved, restart the editor to try and load the file.");
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Time Action Event (TAE) Editor v0.1 (2017.11.26)\n\nDeveloped by Christopher Favorido a.k.a /u/RavagerChris37");
        }

        private void tutorialVideoToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (MessageBox.Show("This operation will open the video in your default browser.\n\nContinue?","Tutorial video",MessageBoxButtons.OKCancel) == DialogResult.OK) System.Diagnostics.Process.Start("https://www.youtube.com/watch?v=ZjWAG71t1R8");
        }

        private void allowNewIDAdditionexperimentalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            allowNewIDAdditionexperimentalToolStripMenuItem.Checked = !allowNewIDAdditionexperimentalToolStripMenuItem.Checked;
            NewIdBtn.Enabled = allowNewIDAdditionexperimentalToolStripMenuItem.Checked;
        }
    }
}
