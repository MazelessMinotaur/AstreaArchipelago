using MonoMod.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AstreaArchipelago.src.Archipelago
{
    public class APLastConnectInfo
    {
        public string host_name;
        public string slot_name;
        public string password;

        public bool Valid
        {
            get => host_name != "" && slot_name != "";
        }

        public static APLastConnectInfo LoadFromFile(string path)
        {
            if (File.Exists(path))
            {
                var reader = File.OpenText(path);
                var content = reader.ReadToEnd();
                reader.Close();
                return JsonConvert.DeserializeObject<APLastConnectInfo>(content);
            }
            return null;
        }

        public void WriteToFile(string path)
        {
            //var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
            //Platform.IO.File.WriteAllBytes(path, bytes);
        }
    }
}
