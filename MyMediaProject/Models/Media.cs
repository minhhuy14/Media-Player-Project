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
        public int Length { get; set; }
    }
}
