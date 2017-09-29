using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KitsuSharp.Models
{
    public enum AgeRating
    {
        G,
        PG,
        R,
        R18
    }

    public enum ShowType
    {
        ONA, 
        OVA,
        TV,
        movie,
        music,
        special
    }

    public enum SubType
    {
        ONA,
        OVA,
        TV,
        movie,
        music,
        special
    }

    public enum Status
    {
        current,
        finished,
        tba,
        unreleased,
        upcoming
    }

    public enum MangaType
    {
        doujin,
        manga,
        manhua,
        manhwa,
        novel,
        oel,
        oneshot
    }
}
