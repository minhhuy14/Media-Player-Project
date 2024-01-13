using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;

namespace MyMediaProject.Models
{
    public class Media
    {
        public int No { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string Artist { get; set; }
        public string Genre { get; set; }
        public string Length { get; set; }
        public Uri Uri { get; set; } 
        public BitmapImage ImageBitmap { get; set; }
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Media))
                return false;

            Media other = (Media)obj;
            return this.Uri == other.Uri;
        }

        public override int GetHashCode()
        {
            return this.Uri.GetHashCode();
        }
    }
}
