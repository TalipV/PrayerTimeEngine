using PrayerTimeEngine.Core.Domain.MosquePrayerTimes;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.MosquePrayerTimes.Services;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.MosquePrayerTimes;

public class MosqueReferenceParserTests
{
    // text of the Mawaqit app's "Share" button
    private const string MAWAQIT_SHARE_TEXT = """
        Mosque name - As-Sunnah Moschee Frechen - Frechen

        • Website: http://mawaqit.net/en/assunnah-frechen

        • Email: info@example.org

        📍 Location:
        https://www.google.com/maps/search/?api=1&query=50.9117847,6.8234782

        Jum'a: 13:15

        Open this mosque in MAWAQIT app:
        https://mawaqit.net/en/id/11504

        Powered by MAWAQIT
        https://mawaqit.net
        """;

    [Fact]
    public void Parse_MawaqitShareText_NumericID()
    {
        MosqueReference? result = MosqueReferenceParser.Parse(MAWAQIT_SHARE_TEXT);

        result.Should().Be(new MosqueReference(EMosquePrayerTimeProviderType.Mawaqit, "id/11504"));
    }

    [Fact]
    public void Parse_MawaqitShareTextInOtherLanguage_NumericID()
    {
        // only the links are used, the (translated) labels of the share text do not matter
        const string Input = """
            Nom de la mosquée - As-Sunnah Moschee Frechen - Frechen

            • Site web: http://mawaqit.net/fr/assunnah-frechen

            Ouvrir cette mosquée dans l'application MAWAQIT :
            https://mawaqit.net/fr/id/11504
            """;

        MosqueReference? result = MosqueReferenceParser.Parse(Input);

        result.Should().Be(new MosqueReference(EMosquePrayerTimeProviderType.Mawaqit, "id/11504"));
    }

    [Fact]
    public void Parse_MawaqitShareTextWithForeignWebsite_NumericID()
    {
        string input = MAWAQIT_SHARE_TEXT.Replace("http://mawaqit.net/en/assunnah-frechen", "https://www.example.org");

        MosqueReference? result = MosqueReferenceParser.Parse(input);

        result.Should().Be(new MosqueReference(EMosquePrayerTimeProviderType.Mawaqit, "id/11504"));
    }

    [Fact]
    public void Parse_MawaqitShareTextPastedIntoSingleLineField_NumericID()
    {
        string input = MAWAQIT_SHARE_TEXT.ReplaceLineEndings(" ");

        MosqueReference? result = MosqueReferenceParser.Parse(input);

        result.Should().Be(new MosqueReference(EMosquePrayerTimeProviderType.Mawaqit, "id/11504"));
    }

    [Theory]
    [InlineData("https://mawaqit.net/de/assunnah-frechen")]
    [InlineData("http://mawaqit.net/en/assunnah-frechen")]
    [InlineData("mawaqit.net/fr/assunnah-frechen")]
    [InlineData("https://mawaqit.net/de/assunnah-frechen?view=desktop")]
    [InlineData("https://mawaqit.net/de/assunnah-frechen#times")]
    [InlineData("https://mawaqit.net/de/assunnah-frechen/")]
    [InlineData("https://mawaqit.net/de/m/assunnah-frechen")]
    [InlineData("https://mawaqit.net/de/w/assunnah-frechen")]
    [InlineData("https://www.mawaqit.net/pt-br/assunnah-frechen")]
    [InlineData("  Schau mal: https://mawaqit.net/de/assunnah-frechen  ")]
    public void Parse_MawaqitBrowserLink_Slug(string input)
    {
        MosqueReference? result = MosqueReferenceParser.Parse(input);

        result.Should().Be(new MosqueReference(EMosquePrayerTimeProviderType.Mawaqit, "assunnah-frechen"));
    }

    [Theory]
    [InlineData("https://mawaqit.net/en/id/11504")]
    [InlineData("https://mawaqit.net/id/11504")]
    [InlineData("id/11504")]
    public void Parse_MawaqitNumericID_IDPath(string input)
    {
        MosqueReference? result = MosqueReferenceParser.Parse(input);

        result.Should().Be(new MosqueReference(EMosquePrayerTimeProviderType.Mawaqit, "id/11504"));
    }

    [Theory]
    [InlineData("https://mymosq.com/mosque/1239")]
    [InlineData("https://www.mymosq.com/mosque/1239")]
    [InlineData("mymosq.com/mosque/1239")]
    [InlineData("https://mymosq.com/mosque/1239?lang=de")]
    [InlineData("https://mymosq.com/mosque/1239/")]
    public void Parse_MyMosqBrowserLink_ID(string input)
    {
        MosqueReference? result = MosqueReferenceParser.Parse(input);

        result.Should().Be(new MosqueReference(EMosquePrayerTimeProviderType.MyMosq, "1239"));
    }

    [Theory]
    [InlineData("assunnah-frechen")]
    [InlineData(" 1239 ")]
    public void Parse_PlainIdentifier_UnknownProvider(string input)
    {
        MosqueReference? result = MosqueReferenceParser.Parse(input);

        result.Should().Be(new MosqueReference(EMosquePrayerTimeProviderType.None, input.Trim()));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Verkaufe Fahrrad, 50 €, Abholung in Köln")]
    [InlineData("https://mawaqit.net")]
    [InlineData("https://www.example.org/de/assunnah-frechen")]
    public void Parse_NoMosque_Null(string? input)
    {
        MosqueReference? result = MosqueReferenceParser.Parse(input);

        result.Should().BeNull();
    }
}
