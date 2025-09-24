using HtmlToXamlDemo;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;

namespace BetterWidgets.Extensions.Xaml
{
    public static class FlowDocumentExtensions
    {
        public static FlowDocument GetDocumentFromHtml(this string htmlContent, FontFamily defaultFont = null, int fontSize = 15)
        {
            string xaml = HtmlToXamlConverter.ConvertHtmlToXaml(htmlContent, true, defaultFont?.Source, fontSize);

            var stringReader = new StringReader(xaml);
            var xmlReader = XmlReader.Create(stringReader);

            var doc = (FlowDocument)XamlReader.Load(xmlReader);

            return doc;
        }

        public static string GetHtmlFromFlowDocument(this FlowDocument document)
        {
            if(document == null) return null;

            var range = new TextRange(document.ContentStart, document.ContentEnd);

            using var stream = new MemoryStream();
            range.Save(stream, DataFormats.Xaml);
            stream.Position = 0;

            string xaml = new StreamReader(stream).ReadToEnd();
            string wrappedXaml = $"<FlowDocument xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">{xaml}</FlowDocument>";

            string html = HtmlFromXamlConverter.ConvertXamlToHtml(wrappedXaml);

            return html;
        }

        public static FlowDocument CloneDocument(this FlowDocument source)
        {
            if(source == null) return null;

            string xaml = XamlWriter.Save(source);
            using var stringReader = new StringReader(xaml);
            using var xmlReader = XmlReader.Create(stringReader);

            return (FlowDocument)XamlReader.Load(xmlReader);
        }
    }
}
