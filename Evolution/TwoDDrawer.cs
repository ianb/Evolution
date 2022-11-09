using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace Evolution; 
using SixLabors.ImageSharp.PixelFormats;
using P = SixLabors.ImageSharp.Drawing.Processing;
using D = SixLabors.ImageSharp.Drawing;


public class TwoDDrawer {
    int WorldPositionSize = 10;
    
    public Image<Rgba32> Draw(TwoDWorld w) {
        Image<Rgba32> image = new Image<Rgba32>(w.Width * WorldPositionSize, w.Height * WorldPositionSize);
        foreach (var state in w.CreatureStates) {
            var s = CharacterShape(WorldPositionSize);
            s = s.Rotate((float)state.Forward.rotation);
            s = s.Translate(new PointF(state.loc.x * WorldPositionSize, state.loc.y * WorldPositionSize));
            // s = s.Translate(new PointF(w.randomGenerator.Next() * 100f, w.randomGenerator.Next() * 100));
            image.Mutate(x => x.Fill(state.Creature.GetColor(w), s));
        }
        return image;
    }

    IPath CharacterShape(int size) {
        var mid = size / 2;
        var b = new D.PathBuilder();
        b.AddEllipticalArc(new PointF(mid, mid), mid, mid, 0, 0, 180);
        b.AddLine(new PointF(0, mid), new PointF(mid, size));
        b.AddLine(new PointF(mid, size), new PointF(size, mid));
        b.CloseFigure();
        var path = b.Build();
        return path;
    }
}