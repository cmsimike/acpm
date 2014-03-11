using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
namespace acpm
{
    public partial class Form1 : Form
    {
        delegate void SetVisibleCallback(Control control, bool visible);
        delegate void SetTextCallback(Control control, string text);
        public Form1()
        {
            InitializeComponent();
            Thread oThread = new Thread(new ThreadStart(this.pullLatestPackages));
            oThread.Start();
        }

        public void pullLatestPackages()
        {
            List<Package> packages = new List<Package>();

            for (var i = 0; i < 40; i++ )
            {
                Package p = new Package();
                p.Name = "Display name " + i;
                p.DownloadUrl = "http://www.example.com/whatever" + i + ".zip";
                p.PackageName = "displayname" + i;
                p.Version = 1;
                packages.Add(p);
            }

            if(packages.Count > 0)
            {
                this.dataGridView1.DataSource = packages;
                // Let's figure out which columns we should hide!
                this.dataGridView1.Columns[0].Visible = false;
                this.dataGridView1.Columns[1].Visible = false;
                this.dataGridView1.Columns[2].Visible = false;

                this.dataGridView1.Columns[3].ReadOnly = true;
                this.dataGridView1.Columns[3].FillWeight = 200;

                this.dataGridView1.Columns[4].ReadOnly = true;
                this.dataGridView1.Columns[4].FillWeight = 50;

                this.setVisible(this.label2, false);
                this.setVisible(this.panel1, true);
            }
            else
            {
                this.setText(this.label2, "Sorry, no packages found.\nBug your favorite developer to add theirs!");
            }
        }

        private void setVisible(Control control, bool visible)
        {
            if(control.InvokeRequired)
            {
                SetVisibleCallback d = new SetVisibleCallback(setVisible);
                control.Invoke(d, new object[] { control, visible });
            }
            else 
            {
                control.Visible = visible;
            }
        }

        private void setText(Control control, string text)
        {
            if(control.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(setText);
                control.Invoke(d, new object[] { control, text });
            }
            else
            {
                control.Text = text;
            }
        }

        private void dgv_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            e.PaintParts &= ~DataGridViewPaintParts.Focus;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.dataGridView1.SelectedRows.Count > 0)
            {
                MessageBox.Show("selected " + this.dataGridView1.SelectedRows[0].Index + " index");
            }
            else
            {
                MessageBox.Show("Please make sure you've selected a package to install!");
            }
        }
    }
}
