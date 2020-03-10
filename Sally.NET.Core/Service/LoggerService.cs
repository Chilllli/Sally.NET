using log4net;
using log4net.Config;
using log4net.Core;
using Sally.NET.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Sally.NET.Service
{
    public static class LoggerService
    {
        private static readonly string commandFile = "./log/command.txt";
        private static readonly string moodFile = "./log/mood.txt";
        private static readonly string levelUpFile = "./log/levelUp.txt";
        public static Logger commandLogger;
        public static Logger moodLogger;
        public static Logger levelUpLogger;

        public static void Initialize()
        {
            commandLogger = new Logger(commandFile);
            moodLogger = new Logger(moodFile);
            levelUpLogger = new Logger(levelUpFile);
        }
    }
}
