using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCTicketing
{
    public interface IValidatorLocation
    {
        string GetLocation();
    }

    public class BusLocation : IValidatorLocation
    {
        /// <summary>
        /// Sample location, it could use the bus position
        /// </summary>
        private string _location;

        public BusLocation(string location)
        {
            _location = location;
        }

        public string GetLocation()
        {
            return _location;
        }
    }
}
