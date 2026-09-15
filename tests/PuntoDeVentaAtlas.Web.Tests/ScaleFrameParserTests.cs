using PuntoDeVentaAtlas.Web.Integrations;
using Xunit;

namespace PuntoDeVentaAtlas.Web.Tests;

public sealed class ScaleFrameParserTests
{
    [Theory]
    [InlineData("10.00 KG\r",10)]
    [InlineData("ST,GS,  1.275kg\r\n",1.275)]
    [InlineData(" 750 g\r",.750)]
    [InlineData("NET +2,350 kg\r",2.350)]
    [InlineData("5 lb\r",2.26796185)]
    public void ParsesCommonScaleFrames(string frame,decimal expected){Assert.True(ScaleFrameParser.TryParse(frame,out var kilograms));Assert.Equal(expected,kilograms);}

    [Theory]
    [InlineData("")]
    [InlineData("OVERLOAD")]
    [InlineData("ERROR\r")]
    public void RejectsFramesWithoutWeight(string frame)=>Assert.False(ScaleFrameParser.TryParse(frame,out _));
}
