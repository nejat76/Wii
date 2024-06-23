// Copyright 2010 Nejat Dilek  <imruon@gmail.com>
// Licensed under the terms of the GNU GPL, version 2
// http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Xml;
using System.Net;
using System.IO;
using System.Net.Cache;


namespace Org.Irduco.UpdateManager
{
    public class BlockedGamesHelper
    {
        private string updateURL;
        private WebProxy proxy;

        public string UpdateURL
        {
            get
            {
                return updateURL;
            }
        }

        public WebProxy Proxy
        {
            set
            {
                this.proxy = value;
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
                request.Proxy = (proxy != null) ? proxy : request.Proxy;
                HttpRequestCachePolicy policy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
                request.CachePolicy = policy;

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
