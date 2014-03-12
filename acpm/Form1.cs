﻿using System;
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
using Newtonsoft.Json;
using System.Net;
using System.Runtime.CompilerServices;
using System.IO;

namespace acpm
{
    public partial class Form1 : Form
    {
        delegate void SetVisibleCallback(Control control, bool visible);
        delegate void SetTextCallback(Control control, string text);
        delegate void RefreshDataGridCallback();

        public Form1()
        {
            InitializeComponent();
            Thread oThread = new Thread(new ThreadStart(this.pullLatestPackages));
            oThread.Start();
        }

        public void pullLatestPackages()
        {
            List<Package> packages = this.getPackagesLocal();
            if(packages == null)
            {
                this.setText(this.label2, "There was a problem loading the repository.\nPlease restart the app to try again.");
            }
            else if(packages.Count > 0)
            {
                this.dataGridView1.DataSource = packages;
                // Let's figure out which columns we should hide!
                this.dataGridView1.Columns[0].Visible = false;
                this.dataGridView1.Columns[1].Visible = false;
                this.dataGridView1.Columns[2].Visible = false;

                this.dataGridView1.Columns[3].ReadOnly = true;
                this.dataGridView1.Columns[3].FillWeight = 200;
                this.dataGridView1.Columns[3].HeaderText = "Package Name";

                this.dataGridView1.Columns[4].ReadOnly = true;
                this.dataGridView1.Columns[4].FillWeight = 50;
                this.dataGridView1.Columns[4].HeaderText = "Status";

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
                SetTextCallback d = new SetTextCallback(this.setText);
                control.Invoke(d, new object[] { control, text });
            }
            else
            {
                control.Text = text;
            }
        }
        
        private void refreshDataGrid()
        {
            if(this.dataGridView1.InvokeRequired)
            {
                RefreshDataGridCallback d = new RefreshDataGridCallback(this.refreshDataGrid);
                this.dataGridView1.Invoke(d);
            }
            else
            {
                this.dataGridView1.Refresh();
            }
        }
        private void dataGridView_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            e.PaintParts &= ~DataGridViewPaintParts.Focus;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.dataGridView1.SelectedRows.Count > 0)
            {
                Package selectedPackage = this.dataGridView1.SelectedRows[0].DataBoundItem as Package;
                if(!selectedPackage.canInstall())
                {
                    MessageBox.Show("Sorry, you can't install this package right now.");
                    return;
                }
                System.Console.WriteLine("before thread");
                Thread thread = new Thread(() => this.downloadPackage(selectedPackage));
                thread.Start();
                System.Console.WriteLine("after thread");
            }
            else
            {
                MessageBox.Show(@"Please select a package to install!");
            }
        }

        private List<Package> jsonToPackages(string json)
        {
            List<Package> packages = JsonConvert.DeserializeObject<List<Package>>(json);

            return packages;
        }

        private List<Package> getPackagesLocal()
        {
            System.IO.StreamReader myFile = new System.IO.StreamReader(@"packages.json");
            string json = myFile.ReadToEnd();
            myFile.Close();
            return this.jsonToPackages(json);
        }

        private List<Package> getPackagesGithub()
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    string s = client.DownloadString(@"https://raw.github.com/cmsimike/acpmr/master/repository.json");
                    return this.jsonToPackages(s);
                }
                catch(WebException)
                {
                    return null;
                }
            }
        }

        private void downloadPackage(Package thePackage)
        {
            thePackage.setDownloading();
            this.refreshDataGrid();
            try
            {
                string tmpFile = Path.GetTempFileName();
                WebClient Client = new WebClient();

                Client.DownloadFile(thePackage.downloadUrl, tmpFile);
                this.installPackage(thePackage, tmpFile);
            }
            catch(Exception)
            {
                thePackage.setErrored();
                this.refreshDataGrid();
            }
        }

        // I am going to believe this works like Java's method syncronization.
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void installPackage(Package thePackage, string fileToUnzip)
        {
            //TODO unzip right here
            JsonStore store = new JsonStore();

            store.packageInstalled(thePackage);
            thePackage.setComplete();
            this.refreshDataGrid();
        }
    }
}
