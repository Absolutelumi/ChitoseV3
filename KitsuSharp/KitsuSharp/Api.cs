﻿using KitsuSharp.Queries;

namespace KitsuSharp
{
    public class Api
    {
        public IAnimeQuery GetAnime => new AnimeQuery();
    }
}
