using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Xml;
using System.Net;
using System.IO;


namespace Org.Irduco.UpdateManager
{
    public class BlockedGamesHelper
    {
        private string updateURL;

        public string UpdateURL
        {
            get
            {
                return updateURL;
            }
        }

        public BlockedGamesHelper(string updateURL)
        {
            this.updateURL = updateURL;
        }

        public List<GameInfo> GetBlockedGameList()
        {
            WebRequest request;
            StreamReader reader = null;

            Schema.BlockedGames games = new Schema.BlockedGames();            
            
            try
            {
                request = HttpWebRequest.Create(updateURL);
                WebResponse response = request.GetResponse();
                reader = new StreamReader(response.GetResponseStream());
                games.ReadXml(reader);

                List<GameInfo> gameInfo = GetGameListFromData(games);

                return gameInfo;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static List<GameInfo> GetGameListFromData(Schema.BlockedGames games)
        {
            List<GameInfo> gameInfo = new List<GameInfo>();
            for (int i = 0; i < games.Game.Count; i++)
            {
                gameInfo.Add(new GameInfo(games.Game[i].discId, games.Game[i].titleId, games.Game[i].name));
            }
            return gameInfo;
        }

        public void SaveBlockedGameList(string path, List<GameInfo> blockedGamesList)
        {
            Schema.BlockedGames blockedGames = new Schema.BlockedGames();
            for (int i = 0; i < blockedGamesList.Count; i++)
            {
                Schema.BlockedGames.GameRow row = blockedGames.Game.NewGameRow();
                row.discId = blockedGamesList[i].DiscId;
                row.titleId = blockedGamesList[i].TitleId;
                row.name = blockedGamesList[i].Name;
                blockedGames.Game.AddGameRow(row);
            }

            blockedGames.WriteXml(path);
        }

        public List<GameInfo> ReadBlockedGameList(string path)
        {
            List<GameInfo> gameInfo = new List<GameInfo>();
            Schema.BlockedGames games = new Schema.BlockedGames();
            FileStream stream = new FileStream(path,FileMode.Open, FileAccess.Read);
            games.ReadXml(stream);
            return GetGameListFromData(games);
        }

    }
}
