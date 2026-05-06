using System;
using System.Collections.Generic;
using System.Text;

namespace Bookstore.Shared.Helpers
{
    public class RankHelper
    {
        public enum LevelRank
        {
            Bronze = 10,
            Silver = 50,
            Gold = 100,
            Platinum = 250,
            Diamond = 500
        }

        public static LevelRank GetRank(int totalPoints)
        {
            return Enum.GetValues(typeof(LevelRank))
                .Cast<LevelRank>()
                .OrderByDescending(r => (int)r)
                .First(r => totalPoints >= (int)r);
        }
    }
}
