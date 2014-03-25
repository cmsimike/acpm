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
using Newtonsoft.Json;
using System.Net;
using System.Runtime.CompilerServices;
using System.IO;
using System.IO.Compression;
using Microsoft.Win32;
using System.Security.Cryptography;

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
            List<Package> packages = this.getPackagesAcmpr();
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
                this.dataGridView1.Columns[5].Visible = false;

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
                Thread thread = new Thread(() => this.downloadPackage(selectedPackage));
                thread.Start();
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

        private List<Package> getPackagesAcmpr()
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    string s = client.DownloadString(@"http://www.acpm.io/package/repository.json");
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
            }
            this.refreshDataGrid();
        }

        // I am going to believe this works like Java's method syncronization.
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void installPackage(Package thePackage, string fileToUnzip)
        {
            try
            {
                string tmpDir = this.GetTemporaryDirectory();
                string computedDownloadedHash = null;
                // need to check hash here
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(fileToUnzip))
                    {
                        computedDownloadedHash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                    }
                }

                if (thePackage.fileHash == computedDownloadedHash)
                {
                    ZipFile.ExtractToDirectory(fileToUnzip, tmpDir);
                    this.DirectoryCopy(tmpDir, this.getACPath(), true);

                    JsonStore store = new JsonStore();

                    store.packageInstalled(thePackage);
                    thePackage.setComplete();
                    return;
                }                 
            }
            catch(Exception)
            {
               
            }
            thePackage.setErrored();
        }

        private string GetTemporaryDirectory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }

        private string getACPath()
        {
            string directory = "";
            RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            using (RegistryKey key = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 244210"))
            {
                if (key != null)
                {
                    directory = (string)key.GetValue("InstallLocation");
                }
            }
            if(string.IsNullOrEmpty(directory) && Directory.Exists(@"C:\Program Files (x86)\Steam\SteamApps\common\assettocorsa"))
            {
                directory = @"C:\Program Files (x86)\Steam\SteamApps\common\assettocorsa";
            }
            else if(string.IsNullOrEmpty(directory) && Directory.Exists(@"C:\Program Files\Steam\SteamApps\common\assettocorsa"))
            {
                directory = @"C:\Program Files\Steam\SteamApps\common\assettocorsa";
            }

            return directory;
        }

        // from http://msdn.microsoft.com/en-us/library/bb762914.aspx
        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}
