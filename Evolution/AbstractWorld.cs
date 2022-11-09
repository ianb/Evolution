using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Evolution;

public abstract class AbstractWorld {
    public Random randomGenerator = new Random(5);
    public T RandomChoice<T>(List<T> choices) {
        return choices[(int)(randomGenerator.NextDouble() * choices.Count)];
    }

    public List<T> Shuffle<T>(List<T> input) {
        var output = new List<T>(input);
        for (int i = output.Count - 1; i >= 1; i--) {
            int j = (int)(randomGenerator.NextDouble() * (i + 1));
            (output[i], output[j]) = (output[j], output[i]);
        }
        return output;
    }
    
    public int EpochTime;
    public int EpochSteps = -1;
    public int NumberOfInternalNeurons = -1;
    public abstract List<IInputNeuron> AvailableInputNeurons { get; }
    public abstract List<InternalNeuron> AvailableInternalNeurons { get; }
    public abstract List<IOutputNeuron> AvailableOutputNeurons { get; }

    public abstract AbstractCreatureState CreatureState(Creature c);

    public abstract void PlaceCreatureRandomly(Creature c);


    public abstract Image<Rgba32> Draw();

    public abstract void Kill(Creature stateCreature);
    public abstract void ShuffleCreatures();
}
