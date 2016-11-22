using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ServerUI.Data
{
    class DataService
    {
        async Task<object> GetLastIP()
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile IpFile = await storageFolder.GetFileAsync("IpFile.txt");
            return FileIO.ReadTextAsync(IpFile);
        }
        async void SetLastIP(object val)
        {
            if (val != null)
            {
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                StorageFile IpFile = await storageFolder.CreateFileAsync("IpFile.txt", CreationCollisionOption.OpenIfExists);
                await Windows.Storage.FileIO.WriteTextAsync(IpFile, val.ToString());
            }
            else {throw new ArgumentNullException(); }
        }
    }
    public class Inital
    {
        public static int slider_val = 400;
        public static string slider_header = "Value = Fuck off";
    }
    public class ControlValue
    {
        public int value {get; set;}
        public ControlValue(int val) { value = val; }
    }
}
