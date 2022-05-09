namespace Kagami.GenerateHelpImage;

public static class Methods
{
    private const string ChineseFontFamily = "微软雅黑";
    private const string EnglishFontFamily = "JetBrains Mono";

    private const string MethodColor = "DimGray";//= "BlanchedAlmond";
    private const string ArgColor = "Purple";//= "LightGray";
    private const string ClassColor = "Green";
    private const string SummaryColor = "Black";//= "White";
    private const string CommentColor = "Gray";

    public enum Color
    {
        Method, Arg, Class, Summary, Comment
    }

    public static string Run(this string text, Color color)
    {
        return $@"<Run FontFamily=""{color switch
        {
            Color.Method => EnglishFontFamily,
            Color.Arg => ChineseFontFamily,
            Color.Class => EnglishFontFamily,
            Color.Summary => ChineseFontFamily,
            Color.Comment => ChineseFontFamily,
            _ => throw new ArgumentOutOfRangeException(nameof(color), color, null)
        }}"" Foreground=""{color switch
        {
            Color.Method => MethodColor,
            Color.Arg => ArgColor,
            Color.Class => ClassColor,
            Color.Summary => SummaryColor,
            Color.Comment => CommentColor,
            _ => throw new ArgumentOutOfRangeException(nameof(color), color, null)
        }}"" >{ text}</Run> ";
    }
}