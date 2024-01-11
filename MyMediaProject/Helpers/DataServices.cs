using MyMediaProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMediaProject.Helpers
{
    public class DataServices
    {
        private string _primaryPath;
        private string _secondaryPath;

        public DataServices() 
        {
            _primaryPath = "";
            _secondaryPath = "";
        }
        
        public async Task<List<Media>> LoadHome()
        {
            return null;
        }

        public async Task<bool> SaveHome()
        {
            return true;
        }
    }
}

