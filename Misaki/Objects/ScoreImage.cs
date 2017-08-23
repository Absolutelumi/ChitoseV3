using ImageProcessor;
using OsuApi.Model;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Score_Image
{
    public class ScoreImage
    {
        private const int AvatarSize = 128;

        private const int BackgroundHeight = 250;

        private const int BackgroundWidth = 900;

        private const int StrokeWidth = 5;

        private const int MinimumDifficultyWidth = 150;

        private static Brush BlueBrush = new SolidBrush(Color.FromArgb(255, 34, 187, 221));

        private static Brush PinkBrush = new SolidBrush(Color.FromArgb(255, 238, 34, 153));

        private static readonly Pen PinkPen = new Pen(Color.FromArgb(255, 238, 34, 153), StrokeWidth * 2);
        private static readonly Font RankFont = new Font("Calibri", 128, GraphicsUnit.Point);
        private static readonly Font TitleFont = new Font("Calibri", 24, GraphicsUnit.Point);
        private static readonly Brush SilverBrush = new SolidBrush(Color.Silver);

        private static readonly Pen SilverPen = new Pen(Color.Silver, StrokeWidth * 2);
        private static readonly Pen WhitePen = new Pen(Color.White, StrokeWidth * 2);

        static ScoreImage()
        {
            WhitePen.LineJoin = LineJoin.Round;
        }

        public static Bitmap CreateScorePanel(User user, Score score, Beatmap beatmap)
        {
            Bitmap background = GetRoundedBackground(AcquireBackground(beatmap.BeatmapSetId));

            using (var graphics = Graphics.FromImage(background))
            {
                graphics.InterpolationMode = InterpolationMode.High;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                graphics.CompositingQuality = CompositingQuality.HighQuality;

                DrawWhiteOverlay(background, graphics);
                DrawAvatar(graphics, user.UserID);
                DrawTitle(graphics, beatmap.Title, beatmap.Difficulty, beatmap.Stars.ToString("#.##"), beatmap.Beatmapper);
                DrawCreator(graphics, beatmap);
                DrawUsername(user, graphics);
                if (score.Mods != Mods.NM)
                    DrawMods(score, graphics);
                if (score.PP != 0)
                    DrawPP(user, score, graphics);
                DrawRank(score, graphics);
                DrawAcc(score, graphics);
                DrawCombo(score, graphics);
            }
            return background;
        }

        private static Bitmap AcquireAvatar(string userId) => new Bitmap(Misaki.Extensions.GetHttpStream(new Uri($"https://a.ppy.sh/{userId}")));

        private static Bitmap AcquireBackground(string beatmapId) => new Bitmap(Misaki.Extensions.GetHttpStream(new Uri($"https://assets.ppy.sh/beatmaps/{beatmapId}/covers/cover.jpg")));

        private static void DrawAcc(Score score, Graphics graphics)
        {
            string acc = $"{score.Accuracy:0.00%}";
            Font accFont = new Font("Calibri", 60, GraphicsUnit.Point);
            GraphicsPath accPath = new GraphicsPath();
            StringFormat accFormat = new StringFormat();
            accFormat.Alignment = StringAlignment.Far;
            accPath.AddString(acc, accFont.FontFamily, (int)FontStyle.Regular, graphics.DpiY * 60 / 110, new Point(GetByRank(score, graphics), 170), accFormat);
            graphics.DrawPath(WhitePen, accPath);
            graphics.FillPath(PinkBrush, accPath);
        }

        private static void DrawAvatar(Graphics graphics, string userId)
        {
            GraphicsPath squarePath = new GraphicsPath();
            squarePath.AddRectangle(new Rectangle(20, BackgroundHeight - 20 - AvatarSize, AvatarSize, AvatarSize));
            graphics.FillPath(Brushes.White, squarePath);
            using (Bitmap avatar = AcquireAvatar(userId))
            using (GraphicsPath roundedPath = GetRoundedCorners(20, BackgroundHeight - 20 - AvatarSize, AvatarSize, AvatarSize, 10))
            {
                graphics.DrawPath(WhitePen, roundedPath);
                graphics.SetClip(roundedPath);
                graphics.DrawImage(avatar, 20, BackgroundHeight - 20 - AvatarSize, AvatarSize, AvatarSize);
                graphics.ResetClip();
            }
        }

        private static void DrawCombo(Score score, Graphics graphics)
        {
            string combo = score.Combo.ToString() + "x";
            Font comboFont = new Font("Calibri", 60, GraphicsUnit.Point);
            StringFormat comboFormat = new StringFormat();
            GraphicsPath comboPath = new GraphicsPath();
            comboFormat.Alignment = StringAlignment.Far;
            comboPath.AddString(combo, comboFont.FontFamily, (int)FontStyle.Regular, graphics.DpiY * 60 / 110, new Point(GetByRank(score, graphics), 110), comboFormat);
            graphics.DrawPath(WhitePen, comboPath);
            graphics.FillPath(PinkBrush, comboPath);
        }

        private static void DrawCreator(Graphics graphics, Beatmap beatmap)
        {
            string mappedBy = "mapped by " + beatmap.Beatmapper;
            GraphicsPath creatorPath = new GraphicsPath();
            Font creatorFont = new Font("Calibri", 60, GraphicsUnit.Point);
            creatorPath.AddString(mappedBy, creatorFont.FontFamily, (int)FontStyle.Regular, graphics.DpiY * 60 / 180, new Point(15, 50), new StringFormat());
            graphics.DrawPath(WhitePen, creatorPath);
            graphics.FillPath(BlueBrush, creatorPath);
        }

        private static void DrawMods(Score score, Graphics graphics)
        {
            string mods = score.Mods.ToString();
            Font modFont = new Font("Calibri", 48, GraphicsUnit.Point);
            GraphicsPath modPath = new GraphicsPath();
            modPath.AddString(mods, modFont.FontFamily, (int)FontStyle.Regular, graphics.DpiY * 48 / 100, new Point(25 + AvatarSize, 120), new StringFormat());
            graphics.DrawPath(WhitePen, modPath);
            graphics.FillPath(PinkBrush, modPath);
        }

        private static void DrawPP(User user, Score score, Graphics graphics)
        {
            var ppFont = new Font("Calibri", 224, GraphicsUnit.Point);
            Brush ppBrush = new SolidBrush(Color.FromArgb(150, 238, 34, 153));
            graphics.RotateTransform(30);
            graphics.DrawString(score.PP + " pp", ppFont, ppBrush, new Point(50, 10));
        }

        private static void DrawRank(Score score, Graphics graphics)
        {
            string rank = score.Rank.ToString().Contains("H") ? score.Rank.ToString().Replace("H", string.Empty) : score.Rank.ToString();
            rank = rank.Contains("X") ? rank.Replace("X", "S") : rank;
            GraphicsPath rankPath = new GraphicsPath();
            StringFormat rankFormat = new StringFormat();
            rankFormat.Alignment = StringAlignment.Far;
            rankPath.AddString(rank, RankFont.FontFamily, (int)FontStyle.Regular, graphics.DpiY * 128 / 72, new Point(900, 60), rankFormat);
            if (score.Rank == Rank.SH || score.Rank == Rank.SX || score.Rank == Rank.XH)
            {
                var innerPinkPen = new Pen(Color.FromArgb(255, 238, 34, 153), StrokeWidth * 0.75f);
                var outerWhitePen = new Pen(Color.White, StrokeWidth * 2);
                graphics.DrawPath(outerWhitePen, rankPath);
                graphics.DrawPath(innerPinkPen, rankPath);
            }
            else
            {
                graphics.DrawPath(WhitePen, rankPath);
                graphics.FillPath(PinkBrush, rankPath);
            }
            //if (score.Rank == Rank.SH || score.Rank == Rank.SX || score.Rank == Rank.XH)
            //{
            //    graphics.DrawPath(PinkPen, rankPath);
            //    graphics.FillPath(SilverBrush, rankPath);
            //}
            //else
            //{
            //    graphics.DrawPath(WhitePen, rankPath);
            //    graphics.FillPath(PinkBrush, rankPath);
            //}
        }

        private static void DrawUsername(User user, Graphics graphics)
        {
            Font usernameFont = new Font("Calibri", 60, GraphicsUnit.Point);
            var usernameSize = usernameFont.Size;
            GraphicsPath usernamePath = new GraphicsPath();
            usernamePath.AddString(user.Username, usernameFont.FontFamily, (int)FontStyle.Regular, 60, new Point(25 + AvatarSize, 170), new StringFormat());
            graphics.DrawPath(WhitePen, usernamePath);
            graphics.FillPath(BlueBrush, usernamePath);
        }

        private static void DrawWhiteOverlay(Bitmap background, Graphics graphics)
        {
            using (var imageFactory = new ImageFactory())
            using (var roundedPath = GetRoundedCorners(10, 10, BackgroundWidth - 20, BackgroundHeight - 20, 20))
            using (var blurredBackground = imageFactory.Load(background).GaussianBlur(10).Image)
            {
                graphics.SetClip(roundedPath);
                graphics.DrawImage(blurredBackground, 0, 0);
                var whiteBrush = new SolidBrush(Color.FromArgb(89, Color.White));
                graphics.FillPath(whiteBrush, roundedPath);
                graphics.ResetClip();
            }
        }

        private static int GetByRank(Score score, Graphics graphics)
        {
            string rank = score.Rank.ToString().Contains("H") ? score.Rank.ToString().Replace("H", string.Empty) : score.Rank.ToString();
            int rankSize = (int)graphics.MeasureString(rank, RankFont).Width;

            return 900 - rankSize + 20;
        }

        private static Bitmap GetRoundedBackground(Bitmap background)
        {
            Bitmap roundedBackground = new Bitmap(background.Width, background.Height);
            Brush backgroundBrush = new TextureBrush(background);
            GraphicsPath backgroundPath = GetRoundedCorners(0, 0, background.Width, background.Height, 30);
            using (Graphics graphics = Graphics.FromImage(roundedBackground))
            {
                graphics.Clear(Color.Transparent);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.FillPath(backgroundBrush, backgroundPath);
                return roundedBackground;
            }
        }

        private static GraphicsPath GetRoundedCorners(int x, int y, int width, int height, int radius)
        {
            int diameter = radius * 2;
            var graphicsPath = new GraphicsPath();
            graphicsPath.AddArc(x, y, diameter, diameter, 180, 90);
            graphicsPath.AddArc(x + width - diameter, y, diameter, diameter, 270, 90);
            graphicsPath.AddArc(x + width - diameter, y + height - diameter, diameter, diameter, 0, 90);
            graphicsPath.AddArc(x, y + height - diameter, diameter, diameter, 90, 90);
            graphicsPath.CloseFigure();
            return graphicsPath;
        }

        private static void CalculateWidths(int width, ref int titleWidth, ref int difficultyWidth)
        {
            int removalRequired = titleWidth + difficultyWidth - width;
            if (removalRequired > 0)
            {
                if (difficultyWidth - removalRequired < MinimumDifficultyWidth)
                {
                    if (difficultyWidth < MinimumDifficultyWidth)
                    {
                        titleWidth -= removalRequired;
                    }
                    else
                    {
                        titleWidth -= removalRequired - (difficultyWidth - MinimumDifficultyWidth);
                        difficultyWidth = MinimumDifficultyWidth;
                    }
                }
                else
                {
                    difficultyWidth -= removalRequired;
                }
            }
        }

        private static RectangleF GetStringBounds(Graphics graphics, string text, Font font, StringFormat format)
        {
            using (var path = new GraphicsPath())
            {
                path.AddString(text, font.FontFamily, (int)font.Style, graphics.DpiY * font.Size / 72, new PointF(0.0f, 0.0f), format);
                var bounds = path.GetBounds();
                bounds.Width += 25;
                return bounds;
            }
        }

        private static void DrawTitle(Graphics graphics, string title, string difficulty, string stars, string beatmapper)
        {
            using (var titlePath = new GraphicsPath())
            using (var titleFormat = new StringFormat())
            {
                titleFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoClip | StringFormatFlags.NoWrap;
                titleFormat.Trimming = StringTrimming.EllipsisCharacter;
                titleFormat.LineAlignment = StringAlignment.Center;
                int xLocation = 15;
                const int yLocation = 20;
                const int style = (int)FontStyle.Regular;
                const int shift = -10;
                const int maxWidth = BackgroundWidth - StrokeWidth * 4 - 5 * shift;
                float emSize = graphics.DpiY * TitleFont.Size / 72;

                var ending = $"] {stars}★";
                var middle = " [";
                var endingSize = GetStringBounds(graphics, ending, TitleFont, titleFormat);
                int height = (int)endingSize.Height;

                int endingWidth = (int)endingSize.Width;
                int middleWidth = (int)GetStringBounds(graphics, middle, TitleFont, titleFormat).Width;

                int remainingWidth = maxWidth - endingWidth - middleWidth;
                int titleWidth = (int)GetStringBounds(graphics, title, TitleFont, titleFormat).Width;
                int difficultyWidth = (int)GetStringBounds(graphics, difficulty, TitleFont, titleFormat).Width;
                CalculateWidths(remainingWidth, ref titleWidth, ref difficultyWidth);

                titlePath.AddString(title, TitleFont.FontFamily, style, emSize, new Rectangle(xLocation, yLocation, titleWidth, height), titleFormat);
                titlePath.AddString(middle, TitleFont.FontFamily, style, emSize, new Rectangle(xLocation += titleWidth + 2 * shift, yLocation, middleWidth, height), titleFormat);
                titlePath.AddString(difficulty, TitleFont.FontFamily, style, emSize, new Rectangle(xLocation += middleWidth + shift, yLocation, difficultyWidth, height), titleFormat);
                titlePath.AddString(ending, TitleFont.FontFamily, style, emSize, new Rectangle(xLocation += difficultyWidth + 2 * shift, yLocation, endingWidth, height), titleFormat);
                graphics.DrawPath(WhitePen, titlePath);
                graphics.FillPath(BlueBrush, titlePath);
            }
        }
    }
}