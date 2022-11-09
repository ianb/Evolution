using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Evolution; 

public class Ecosystem {
    public readonly AbstractWorld World;
    public int NumberOfConnections;
    public int NumberOfInternalNeurons;
    public int InitialOrganisms;
    public int EpochSteps;
    public List<Creature> Creatures = new();
    public List<Image<Rgba32>> ImageHistory = new();
    public Dictionary<Type, int> OutputHistograms = new();
    public bool RecordStepImages = true;

    public Ecosystem(AbstractWorld world) {
        World = world;
    }
    
    public void AddRandomCreatures() {
        while (Creatures.Count < InitialOrganisms) {
            AddRandomCreature();
        }
    }

    void AddRandomCreature() {
        Creature c = new Creature();
        c.Genes.CreateRandomGenes(this);
        World.PlaceCreatureRandomly(c);
        Creatures.Add(c);
    }

    public void Init() {
        if (World.NumberOfInternalNeurons == -1) {
            World.NumberOfInternalNeurons = NumberOfInternalNeurons;
        }
        if (World.EpochSteps == -1) {
            World.EpochSteps = EpochSteps;
        }
        World.EpochTime = 0;
        AddRandomCreatures();
    }
    
    public void Execute() {
        Init();
        ExecuteEpoch();
    }

    public void ExecuteEpoch() {
        while (World.EpochTime < EpochSteps) {
            ExecuteStep();
        }
    }

    public void ExecuteStep() {
        var randomCreatures = World.Shuffle(Creatures);
        foreach (var c in randomCreatures) {
            var nn = c.Genes.NeuralNet;
            var state = World.CreatureState(c);
            nn.Execute(state);
            var neuron = nn.PickOutputNeuron(state);
            nn.ExecuteOutput(state, neuron);
            int prev = 0;
            OutputHistograms.TryGetValue(neuron.GetType(), out prev);
            OutputHistograms[neuron.GetType()] = prev + 1;
        }
        World.EpochTime++;
        if (RecordStepImages) {
            ImageHistory.Add(World.Draw());
        }
    }

    public void Cull(Func<AbstractCreatureState, bool> filterFunc) {
        var oldCreatures = new List<Creature>(Creatures);
        foreach (var c in oldCreatures)
        {
            var state = World.CreatureState(c);
            if (!filterFunc(state)) {
                Kill(state.Creature);
            }
        }
    }

    void Kill(Creature c) {
        World.Kill(c);
        Creatures.Remove(c);
    }

    public void ReproducePopulation(double portion = 1.0) {
        var target = (int)(InitialOrganisms * portion);
        var remaining = new List<Creature>(Creatures);
        while (Creatures.Count < target) {
            Creature c = remaining.Count > 0 ? World.RandomChoice(remaining) : World.RandomChoice(Creatures);
            if (remaining.Count > 0) {
                remaining.Remove(c);
            }
            Reproduce(c);
        }
    }

    void Reproduce(Creature c) {
        var child = new Creature() {
            Genes = c.Genes.Reproduce(),
        };
        child.SetColor(c.GetColor(World));
        World.PlaceCreatureRandomly(child);
        Creatures.Add(child);
    }
    
    public string OutputHistogramSummary() {
        var items = OutputHistograms.OrderBy(kvp => -kvp.Value).ToList();
        var lines = new List<string>();
        foreach (var item in items) {
            lines.Add($"{item.Key.ToString(),-25}: {item.Value.ToString(),6}");
        }
        return string.Join("\n", lines);
    }
}
