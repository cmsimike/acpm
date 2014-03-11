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
        public string packageName {get; set;}
        public int version {get; set;}
        public string downloadUrl {get; set;}
        public string name { get; set; }

        public string status {
            get {
                return "Not installed";
            }
        }
    }
}
