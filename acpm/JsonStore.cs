using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace acpm
{
    class JsonStore
    {
        private string jsonStoraName = @"acpm.db";
        public void packageInstalled(string packageName, int version)
        {
            Dictionary<string, int> values = JsonConvert.DeserializeObject<Dictionary<string, int>>(this.getJson());
            values[packageName] = version;
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

        private void saveJson(Dictionary<string, int> dictJason)
        {
            string json = JsonConvert.SerializeObject(dictJason);
            System.IO.File.WriteAllText(jsonStoraName, json);
        }

        private string getJson()
        {
            try
            {
                System.IO.StreamReader myFile = new System.IO.StreamReader(jsonStoraName);
                string json = myFile.ReadToEnd();
                myFile.Close();
                return json;
            }   
            catch(Exception e)
            {
                return "{}";
            }
        }
    }
}
