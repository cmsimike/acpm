using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace acpm
{
    class JsonStore
    {
        public void packageInstalled(Package package)
        {
            Dictionary<string, int> values = JsonConvert.DeserializeObject<Dictionary<string, int>>(this.getJson());
            values[package.packageName] = package.version;
            this.saveJson(values);
        }

        public int getVersionInstalled(string packageName)
        {
            Dictionary<string, int> values = JsonConvert.DeserializeObject<Dictionary<string, int>>(this.getJson());
            if(values.ContainsKey(packageName))
            {
                return values[packageName];
            }

            return 0;
        }

        // I don't think I need to be syncronized because the only place this gets called is installPackage, which only gets called from the syncronized install package method
        private void saveJson(Dictionary<string, int> dictJason)
        {
            string json = JsonConvert.SerializeObject(dictJason);
            Properties.Settings.Default.InstalledPackages = json;
            Properties.Settings.Default.Save();
        }

        private string getJson()
        {
            string json = Properties.Settings.Default.InstalledPackages;
            return json;
        }
    }
}
