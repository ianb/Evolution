namespace Evolution;

public class Genes {
    public NeuralNet NeuralNet { get; protected set; } = new();

    public void CreateRandomGenes(Ecosystem e) {
        var internalNeurons = e.World.AvailableInternalNeurons;
        var inputs = e.World.AvailableInputNeurons.Concat(internalNeurons).ToList();
        var outputs = e.World.AvailableOutputNeurons;
        for (int i = 0; i < e.NumberOfConnections; i++) {
            var input = e.World.RandomChoice(inputs);
            var availOutput = new List<IOutputNeuron>(outputs);
            if (input is InternalNeuron internalNeuron) {
                foreach (var n in internalNeurons) {
                    if (n.rank > internalNeuron.rank) {
                        availOutput.Add(n);
                    }
                }
            } else {
                availOutput = availOutput.Concat(internalNeurons).ToList();
            }
            var output = PickWeighted(e, availOutput);
            var weight = e.World.randomGenerator.NextDouble() * 8 - 4;
            var c = new NeuralConnection(input, output, weight);
            NeuralNet.ConnectNeurons(c);
        }
    }

    IOutputNeuron PickWeighted(Ecosystem e, List<IOutputNeuron> neurons) {
        var absNeurons = neurons.ConvertAll(x => (AbstractNeuron)x);
        double total = absNeurons.Select(x => x.RandomPickWeight).Sum();
        double pick = e.World.randomGenerator.NextDouble() * total;
        double place = 0;
        foreach (var n in absNeurons) {
            place += n.RandomPickWeight;
            if (place >= pick) {
                return (IOutputNeuron)n;
            }
        }
        throw new Exception($"Somehow didn't pick a neuron");
    }

    public Genes Reproduce() {
        return new Genes() {
            NeuralNet = NeuralNet.Reproduce(),
        };
    }
}