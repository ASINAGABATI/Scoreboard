using System.IO;
using System.Text.Json;

namespace Scoreboard
{
    class Config
    {
        public DateTime today { get; set; }     // 最終起動日時

        public string leftTeamName { get; set; }
        public int leftTeamNameFontSize { get; set; }
        public string rightTeamName { get; set; }
        public int rightTeamNameFontSize { get; set; }

        public int periodSets { get; set; }   // [回]
        public int periodTime { get; set; }   // [分]
        public int periodInterval { get; set; } // [分]

        public string wav_path { get; set; }
    }

    internal class Match
    {
        internal class Team
        {
            public string name;
            public int sizeFont;

            public Team()
            {
                name = "";
                sizeFont = 34;
            }
        }
        private const string path_json = "FB_Match.json";

        public Team left = null;
        public Team right = null;
        public int periodSets = 3;  // [回]
        public int periodTime = 3;  // [min]
        public int periodInterval = 1; // [min]
        public string? wav_path = "";

        public Match()
        {
            left = new Team();
            right = new Team();

            string jsonString = null;
            try
            {
                jsonString = File.ReadAllText(path_json);
            }
            catch (Exception) { }
            if (jsonString == null)
            {
                left.name = "LeftTeam";
                left.sizeFont = 62;
                right.name = "RightTeam";
                right.sizeFont = 62;

                periodSets = 3;
                periodTime = 3;
                periodInterval = 1;

                wav_path = null;
            }
            else
            {
                var rdata = JsonSerializer.Deserialize<Config>(jsonString);

                left.name = rdata.leftTeamName;
                left.sizeFont = rdata.leftTeamNameFontSize;
                right.name = rdata.rightTeamName;
                right.sizeFont = rdata.rightTeamNameFontSize;

                periodSets = rdata.periodSets;
                periodTime = rdata.periodTime;
                periodInterval = rdata.periodInterval;

                if (File.Exists(rdata.wav_path))
                {
                    wav_path = rdata.wav_path;
                }
                else
                {
                    wav_path = null;
                }
            }
        }

        public void update()
        {
            Config conf = new Config();

            conf.leftTeamName = left.name;
            conf.leftTeamNameFontSize = left.sizeFont;
            conf.rightTeamName = right.name;
            conf.rightTeamNameFontSize = right.sizeFont;
            conf.periodSets = periodSets;
            conf.periodTime = periodTime;
            conf.periodInterval = periodInterval;
            conf.wav_path = wav_path;

            var json_str = JsonSerializer.Serialize(conf, new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(path_json, json_str);
        }
    }
}
