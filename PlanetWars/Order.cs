using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetWars
{
    public class Order
    {
        public int Player { get; }
        public int Source { get; }
        public int Destination { get; }
        public int Ships { get; }

        private Order(int player, int source, int destination, int ships)
        {
            Player = player;
            Source = source;
            Destination = destination;
            Ships = ships;
        }

        public static Order TryParse(string order, int player, out string error)
        {
            error = "";

            if (player != 1 && player != 2)
            {
                throw new Exception($"Trying to create an order for player {player}");
            }

            string[] components = order.Split(' ');
            if (components.Length != 3)
            {
                error = "Bad order format.  Order = " + order;
                return null;
            }

            int source;
            if (!int.TryParse(components[0], out source))
            {
                error = "Bad order format.  Order = " + order;
                return null;
            }

            int destination;
            if (!int.TryParse(components[1], out destination))
            {
                error = "Bad order format.  Order = " + order;
                return null;
            }

            int ships;
            if (!int.TryParse(components[2], out ships))
            {
                error = "Bad order format.  Order = " + order;
                return null;
            }

            return new Order(player, source, destination, ships);
        }
        
        // Probably don't need this?
        public override string ToString()
        {
            return $"{Source} {Destination} {Ships}";
        }
    }
}
