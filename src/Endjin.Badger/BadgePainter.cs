using SixLabors.Fonts;

using System.Globalization;
using System.Net;

namespace Endjin.Badger;

public static class BadgePainter
{
    public static string DrawSVG(string subject, string status, string statusColor, Style style = Style.Flat)
    {
        var template = style switch
        {
            Style.Flat => Resources.flat,
            Style.FlatSquare => Resources.flatSquare,
            Style.Plastic => Resources.plastic,
            _ => Resources.flat,
        };

        // This takes the fonts size in points, which are bigger than pixels. 1 points is 1/72 inches,
        // whereas in web coordinate systems, 1 pixel is 1/96 inches. So we need to multiple by 0.75
        // to provide the font size in pixels.
        Font font = new(SystemFonts.Get("Verdana"), 11 * 0.75f, FontStyle.Regular);
        var textOptions = new TextOptions(font)
        {
            Dpi = 96
        };

        var subjectWidth = TextMeasurer.MeasureSize(WebUtility.HtmlDecode(subject), textOptions).Width + 8; //34.25086
        var statusWidth = TextMeasurer.MeasureSize(WebUtility.HtmlDecode(status), textOptions).Width + 8; // 42.3075

        var result = string.Format(
            CultureInfo.InvariantCulture,
            template,
            subjectWidth + statusWidth, //width
            subjectWidth,
            statusWidth,
            subjectWidth / 2,
            subjectWidth + (statusWidth / 2),
            subject,
            status,
            statusColor);

        return result;
    }
}