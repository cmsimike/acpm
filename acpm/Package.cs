using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace acpm
{
    class Package
    {
        /** Important to note that ordering here is important because I am hiding columns based on position.
         * packageName is 0, version is 1 etc. Add new properties at the end
         */
        private static int STATIC_CLEAR = 0;
        private static int STATUS_DOWNLOADING = 1;
        private static int STATUS_ERROR = 2;
        public string packageName {get; set;}
        public int version {get; set;}
        public string downloadUrl {get; set;}
        public string name { get; set; }
        private int CurrentStatus { get; set; }

        private JsonStore store = new JsonStore();
        public string status {
            get {
                // If the package is in some sort of current status, return that first
                if(this.CurrentStatus == STATUS_DOWNLOADING)
                {
                    return "Installing...";
                }
                else if(this.CurrentStatus == STATUS_ERROR)
                {
                    return "Errored!";
                }
                int installedVersion = this.store.getVersionInstalled(this.packageName);
                if(installedVersion == 0)
                {
                    return "Not installed";
                }
                else if(installedVersion == this.version)
                {
                    return "Up to date";
                }
                else if(installedVersion < this.version)
                {
                    return "Needs update";
                }
                return "Something bad";
            }
        }

        public bool canInstall()
        {
            int installedVersion = this.store.getVersionInstalled(this.packageName);
            return  this.CurrentStatus == 0 && (installedVersion == 0 || installedVersion < this.version);
        }

        public void setDownloading()
        {
            this.CurrentStatus = STATUS_DOWNLOADING;
        }
        public void setErrored()
        {
            this.CurrentStatus = STATUS_ERROR;
        }

        public void setComplete()
        {
            this.CurrentStatus = STATIC_CLEAR;
        }
    }
}
