using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Evolution;

public class Creature {
    protected static int counter = 0;
    protected int id;
    
    public Creature() {
        counter++;
        id = counter;
    }
    public Genes Genes = new();

    Color _color = Color.Black;

    public void SetColor(Color c) {
        _color = c;
    }

    public Color GetColor(AbstractWorld w) {
        if (_color == Color.Black) {
            var rgba = new Rgba32(
                (byte)(w.randomGenerator.Next() * 255),
                (byte)(w.randomGenerator.Next() * 255),
                (byte)(w.randomGenerator.Next() * 255));
            _color = new Color(rgba);
        }
        return _color;
    }
    
    public override string ToString() {
        return $"[Creature {id}]";
    }
}

public abstract class AbstractCreatureState {
    public int Age => World.EpochTime;
    public Creature Creature { get; protected set; }
    public virtual AbstractWorld World { get; protected set; }
    public Dictionary<AbstractNeuron, double> NeuronValues = new();

    protected AbstractCreatureState(Creature creature, AbstractWorld world) {
        this.Creature = creature;
        this.World = world;
    }

}
