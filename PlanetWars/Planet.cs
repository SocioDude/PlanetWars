using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetWars
{
    public class Planet
    {
        public int Identifier { get; }
        public double X { get; }
        public double Y { get; }
        public int Owner { get; set; }
        public int Ships { get; set; }
        public int Growth { get; }

        public Planet(string planet, int identifier)
        {
            Identifier = identifier;

            string[] components = planet.Split(' ');
            if (components.Length != 6 || components[0] != "P")
            {
                throw new Exception("Bad planet line format.  Line = " + planet);
            }

            double x;
            if (!double.TryParse(components[1], out x))
            {
                throw new Exception("Bad planet line format.  Line = " + planet);
            }
            X = x;

            double y;
            if (!double.TryParse(components[2], out y))
            {
                throw new Exception("Bad planet line format.  Line = " + planet);
            }
            Y = y;

            int owner;
            if (!int.TryParse(components[3], out owner))
            {
                throw new Exception("Bad planet line format.  Line = " + planet);
            }
            Owner = owner;

            int ships;
            if (!int.TryParse(components[4], out ships))
            {
                throw new Exception("Bad planet line format.  Line = " + planet);
            }
            Ships = ships;

            int growth;
            if (!int.TryParse(components[5], out growth))
            {
                throw new Exception("Bad planet line format.  Line = " + planet);
            }
            Growth = growth;
        }

        public string ToPlayerTwoString()
        {
            int transformedOwner = Owner == 0 ? 0 : Owner == 1 ? 2 : 1;
            return $"P {X} {Y} {transformedOwner} {Ships} {Growth}";
        }

        public override string ToString()
        {
            return $"P {X} {Y} {Owner} {Ships} {Growth}";
        }
    }
}
