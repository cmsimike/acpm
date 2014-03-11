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
        public string PackageName {get; set;}
        public int Version {get; set;}
        public string DownloadUrl {get; set;}
        public string Name { get; set; }

        public string Status {
            get {
                return "NEW!";
            }
        }
    }
}
