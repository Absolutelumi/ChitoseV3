namespace KitsuSharp.Models
{
    public class Category
    {
        public string Title => title;
        public string Description => description;
        public int TotalMediaCount => totalMediaCount;
        public string Slug => slug;
        public bool Nsfw => nsfw;
        public int ChildCount => childCount;
        public ImageSet Image => image;

        #region Json Fields

#pragma warning disable 0649

        internal string title;

        internal string description;

        internal int totalMediaCount;

        internal string slug;

        internal bool nsfw;

        internal int childCount;

        internal ImageSet image;

#pragma warning restore 0649

        #endregion
    }
}
