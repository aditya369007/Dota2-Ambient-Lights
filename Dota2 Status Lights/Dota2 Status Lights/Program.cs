﻿using System;
using System.Diagnostics;
using System.IO;
using Dota2GSI;
using Microsoft.Win32;
using System.Threading;
using System.IO.Ports;

namespace Dota2GSI_Example_program
{
    class Program
    {
        static SerialPort SP;
        static GameStateListener _gsl;

        static void Main(string[] args)
        {
            SP = new SerialPort();
            SP.PortName = "com3";
            SP.BaudRate = 9600;
            SP.ReadTimeout = 500;
            SP.Open();
            if (args == null) Console.WriteLine();

            CreateGsifile();

            Process[] pname = Process.GetProcessesByName("Dota2");
            if (pname.Length == 0)
            {
                Console.WriteLine("Dota 2 is not running. Please start Dota 2.");
                Console.ReadLine();
                Environment.Exit(0);
            }

            _gsl = new GameStateListener(4000);
            _gsl.NewGameState += OnNewGameState;


            if (!_gsl.Start())
            {
                Console.WriteLine("GameStateListener could not start. Try running this program as Administrator. Exiting.");
                Console.ReadLine();
                Environment.Exit(0);
            }
            Console.WriteLine("Listening for game integration calls...");

            Console.WriteLine("Press ESC to quit");
            do
            {
                while (!Console.KeyAvailable)
                {
                    Thread.Sleep(1000);
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }
        static void OnNewGameState(GameState gs)
        {
            Console.Clear();
            Console.WriteLine("Press ESC to quit");
            Console.WriteLine("Current Dota version: " + gs.Provider.Version);
            Console.WriteLine("Current time as displayed by the clock (in seconds): " + gs.Map.ClockTime);
            Console.WriteLine("Your steam name: " + gs.Player.Name);
            Console.WriteLine("hero ID: " + gs.Hero.ID);
            Console.WriteLine("Health: " + gs.Hero.Health);
            for (int i = 0; i < gs.Abilities.Count; i++)
            {
                Console.WriteLine("Ability {0} = {1}", i, gs.Abilities[i].Name);
            }
            Console.WriteLine("First slot inventory: " + gs.Items.GetInventoryAt(0).Name);
            Console.WriteLine("Second slot inventory: " + gs.Items.GetInventoryAt(1).Name);
            Console.WriteLine("Third slot inventory: " + gs.Items.GetInventoryAt(2).Name);
            Console.WriteLine("Fourth slot inventory: " + gs.Items.GetInventoryAt(3).Name);
            Console.WriteLine("Fifth slot inventory: " + gs.Items.GetInventoryAt(4).Name);
            Console.WriteLine("Sixth slot inventory: " + gs.Items.GetInventoryAt(5).Name);

            if (gs.Hero.HealthPercent >= 90 && gs.Hero.HealthPercent <= 100)
            {
                Console.WriteLine("Healthy");
                SP.WriteLine("1");
            }
            if (gs.Hero.HealthPercent >= 80 && gs.Hero.HealthPercent < 90)
            {
                Console.WriteLine("Doing Good");
                SP.WriteLine("2");
            }
            if (gs.Hero.HealthPercent >= 70 && gs.Hero.HealthPercent < 80)
            {
                Console.WriteLine("Nice");
                SP.WriteLine("3");
            }
            if (gs.Hero.HealthPercent >= 60 && gs.Hero.HealthPercent < 70)
            {
                Console.WriteLine("Kinda want to take care");
                SP.WriteLine("4");
            }
            if (gs.Hero.HealthPercent >= 50 && gs.Hero.HealthPercent < 60)
            {
                Console.WriteLine("You should be worried");
                SP.WriteLine("5");
            }
            if (gs.Hero.HealthPercent >= 40 && gs.Hero.HealthPercent < 50)
            {
                Console.WriteLine("You must be worried");
                SP.WriteLine("6");
            }
            if (gs.Hero.HealthPercent >= 30 && gs.Hero.HealthPercent < 40)
            {
                Console.WriteLine("Heal up buddy!");
                SP.WriteLine("7");
            }
            if (gs.Hero.HealthPercent >= 20 && gs.Hero.HealthPercent < 30)
            {
                Console.WriteLine("Go for cover now");
                SP.WriteLine("8");
            }
            if (gs.Hero.HealthPercent > 1 && gs.Hero.HealthPercent < 20)
            {
                Console.WriteLine("Almost dead");
                SP.WriteLine("9");
            }
            if (gs.Hero.IsAlive == false)
            {
                Console.WriteLine("Hero Dead");
                SP.WriteLine("0");
            }

            if (gs.Hero.IsMagicImmune == true)
            {
                SP.WriteLine("B");
            }

            if (gs.Hero.HasDebuff == true)
            {
                SP.WriteLine("C");
            }



            //else
            //1 Console.WriteLine("You DO NOT have a blink dagger");
        }

        private static void CreateGsifile()
        {
            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");

            if (regKey != null)
            {
                string gsifolder = regKey.GetValue("SteamPath") +
                                   @"\steamapps\common\dota 2 beta\game\dota\cfg\gamestate_integration";
                Directory.CreateDirectory(gsifolder);
                string gsifile = gsifolder + @"\gamestate_integration_testGSI.cfg";
                if (File.Exists(gsifile))
                    return;

                string[] contentofgsifile =
                {
                    "\"Dota 2 Integration Configuration\"",
                    "{",
                    "    \"uri\"           \"http://localhost:4000\"",
                    "    \"timeout\"       \"5.0\"",
                    "    \"buffer\"        \"0.1\"",
                    "    \"throttle\"      \"0.1\"",
                    "    \"heartbeat\"     \"30.0\"",
                    "    \"data\"",
                    "    {",
                    "        \"provider\"      \"1\"",
                    "        \"map\"           \"1\"",
                    "        \"player\"        \"1\"",
                    "        \"hero\"          \"1\"",
                    "        \"abilities\"     \"1\"",
                    "        \"items\"         \"1\"",
                    "    }",
                    "}",

                };

                File.WriteAllLines(gsifile, contentofgsifile);
            }
            else
            {
                Console.WriteLine("Registry key for steam not found, cannot create Gamestate Integration file");
                Console.ReadLine();
                Environment.Exit(0);
            }
        }
    }
}