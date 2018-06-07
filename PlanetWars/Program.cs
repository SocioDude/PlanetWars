using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Mono.Options;
using System.Text;
using System.Threading.Tasks;

/**
 * TODOs:
 *   * Add reasonable error messages if commandline args aren't good.
 *   * Show help message if called for.
 */

namespace PlanetWars
{
    class Program
    {
        static void Main(string[] args)
        {
            string mapPath = "";
            string firstAIPath = "";
            string secondAIPath = "";

            var p = new OptionSet() {
                { "m|map=", "the path to the map.",
                    v => mapPath = v },
                { "ai1|firstAI=",
                    "path to the first AI (should be .exe).",
                    v => firstAIPath = v },
                { "ai2|secondAI=",
                    "path to the first AI (should be .exe).",
                    v => secondAIPath = v }
            };

            try
            {
                p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("planetwars: ");
                Console.WriteLine(e.Message);
                return;
            }

            string[] lines = File.ReadAllLines(mapPath);
            Map map = new Map(lines.ToList());

            List<string> replayData = new List<string>();
            replayData.Add(map.ToString());
            ManagedAI first = new ManagedAI(firstAIPath);

            // There's probably a less stupid language construct like some try-with-resources thing that could make
            // sure we kill these processes, but I don't want to look it up.
            try
            {
                ManagedAI second = new ManagedAI(secondAIPath);

                try
                {
                    while (map.GetWinner() == -1)
                    {
                        List<string> playerOneOrders = null;
                        var task = Task.Run(() => first.GetOrders(map.GetPlayerOneState()));
                        if (task.Wait(TimeSpan.FromSeconds(1)))
                        {
                            playerOneOrders = task.Result;
                        }
                        else
                        {
                            map.PlayerOneError = "Timed out.";
                        }

                        List<string> playerTwoOrders = null;
                        task = Task.Run(() => second.GetOrders(map.GetPlayerTwoState()));
                        if (task.Wait(TimeSpan.FromSeconds(1)))
                        {
                            playerTwoOrders = task.Result;
                        }
                        else
                        {
                            map.PlayerTwoError = "Timed out.";
                        }

                        if (map.PlayerOneError != "" || map.PlayerTwoError != "")
                        {
                            // At least one AI timed out.
                            break;
                        }

                        map.ApplyOrders(playerOneOrders, playerTwoOrders);
                        replayData.Add(map.ToString());
                        replayData.Add("go\n");
                    }

                    replayData.Add(map.ToString());
                    replayData.Add("go\n");
                    replayData.Add("GAME OVER");
                    replayData.Add($"LENGTH: {map.TurnNumber}");
                    replayData.Add($"P1ERROR: {map.PlayerOneError}");
                    replayData.Add($"P2ERROR: {map.PlayerTwoError}");
                    replayData.Add($"WINNER: {map.GetWinner()}");
                    replayData.ForEach(x => Console.WriteLine(x));

                    File.WriteAllLines("replay.txt", replayData);
                }
                catch (Exception e)
                {
                    second.EndProcess();
                    throw e;
                }

                second.EndProcess();
            }
            catch (Exception e)
            {
                first.EndProcess();
                throw e;
            }

            first.EndProcess();
        }
    }
}
