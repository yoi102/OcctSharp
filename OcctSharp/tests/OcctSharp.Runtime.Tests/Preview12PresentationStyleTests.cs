namespace OcctSharp.Runtime.Tests;

public sealed class Preview12PresentationStyleTests
{
    [Fact]
    public void PresentationStyleTopologySurvivesSourceDocumentDisposal()
    {
        IReadOnlyList<XdePresentationStyle> styles;
        using (Shape box = ShapeFactory.CreateBox(8, 6, 4))
        using (XdeDocument document = XdeDocument.Create())
        {
            using XdeTransaction transaction = document.BeginTransaction("Preview.12 style ownership");
            XdeLabel part = document.AddShape(box, "Styled Part");
            part.Color = new XdeColor(0.15, 0.45, 0.85, 0.75);
            Assert.True(transaction.Commit());
            styles = part.GetPresentationStyles();
        }

        try
        {
            XdePresentationStyle style = Assert.Single(styles);
            Assert.True(style.IsVisible);
            Assert.Equal(6, style.Shape.FaceCount);
            XdeColor color = Assert.IsType<XdeColor>(style.EffectiveColor);
            Assert.Equal(0.15, color.Red, 6);
            Assert.Equal(0.45, color.Green, 6);
            Assert.Equal(0.85, color.Blue, 6);
            Assert.Equal(0.75, color.Alpha, 6);
        }
        finally
        {
            foreach (XdePresentationStyle style in styles) style.Dispose();
        }
    }
}
