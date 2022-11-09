using System.Diagnostics;

namespace Evolution; 

public class NeuralNet {
    List<IInputNeuron> inputNeurons = new();
    List<IOutputNeuron> outputNeurons = new();
    List<InternalNeuron> internalNeurons = new();

    Dictionary<IInputNeuron, List<NeuralConnection>> outgoingConnections = new ();
    Dictionary<IOutputNeuron, double> neuralValues = new();
    Dictionary<IOutputNeuron, int> outputHits = new();
    Dictionary<IOutputNeuron, int> outputHitPositive = new();
    Dictionary<IOutputNeuron, int> outputHitNegative = new();

    public NeuralNet Reproduce() {
        return new NeuralNet() {
            inputNeurons = new List<IInputNeuron>(inputNeurons),
            outputNeurons = new List<IOutputNeuron>(outputNeurons),
            internalNeurons = new List<InternalNeuron>(internalNeurons),
            outgoingConnections = new Dictionary<IInputNeuron, List<NeuralConnection>>(outgoingConnections),
        };
    }
    
    public void ConnectNeurons(NeuralConnection c) {
        MaybeAdd((AbstractNeuron)c.source);
        MaybeAdd((AbstractNeuron)c.dest);
        if (c.source is InternalNeuron internal1 && c.dest is InternalNeuron internal2) {
            if (internal1.rank >= internal2.rank) {
                throw new Exception($"Neuron {c.source} cannot be connected to {c.dest} according to rank");
            }
        }
        if (!outgoingConnections.ContainsKey(c.source)) {
            outgoingConnections[c.source] = new();
        }
        outgoingConnections[c.source].Add(c);
    }

    void MaybeAdd(AbstractNeuron n) {
        if (n is InternalNeuron neuron) {
            if (!internalNeurons.Contains(neuron)) {
                internalNeurons.Add(neuron);
                internalNeurons.Sort((a, b) => a.rank.CompareTo(b.rank));
            }
        } else if (n is IInputNeuron inputNeuron) {
            if (!inputNeurons.Contains(inputNeuron)) { 
                inputNeurons.Add(inputNeuron);
            }
        } else if (n is IOutputNeuron outputNeuron) {
            if (!outputNeurons.Contains(outputNeuron)) {
                outputNeurons.Add(outputNeuron);
            }
        } else {
            throw new Exception($"Neuron is weird: {n}");
        }
    }

    public void Execute(AbstractCreatureState state) {
        if (state == null) {
            throw new Exception("Calling .Execute() with null state");
        }
        neuralValues = new();
        FillInputValues(state);
        FillInternalNeurons(state);
    }

    public IOutputNeuron PickOutputNeuron(AbstractCreatureState state) {
        List<IOutputNeuron> neurons = new();
        List<double> values = new();
        double total = 0;
        foreach (var n in neuralValues) {
            if (!n.Key.CanAct(state, n.Value)) {
                continue;
            }
            neurons.Add(n.Key);
            double v = Math.Abs(n.Value);
            if (double.IsNaN(v)) {
                throw new Exception($"Neuron {n.Key} value return NaN");
            }
            values.Add(v);
            total += Math.Max(0.01, Math.Abs(n.Value));
        }
        if (values.Count == 0) {
            return new NoPossible();
        }
        double pick = state.World.randomGenerator.NextDouble() * total;
        for (int i = 0; i < neurons.Count; i++) {
            pick -= Math.Max(0.01, Math.Abs(values[i]));
            if (pick <= 0) {
                return neurons[i];
            }
        }
        throw new Exception($"Somehow nothing was picked from total={total} count={neurons.Count}");
    }

    public void ExecuteOutput(AbstractCreatureState state, IOutputNeuron outputNeuron) {
        if (outputNeuron is NoPossible) {
            return;
        }
        double v = neuralValues[outputNeuron];
        if (!outputNeuron.CanAct(state, v)) {
            // Really this can happen if two things try to do a conflicting action and another
            // wins
            // throw new Exception($"Output neuron {outputNeuron} cannot act");
            return;
        }
        outputHits.TryGetValue(outputNeuron, out int currentHits);
        outputHits[outputNeuron] = currentHits + 1;
        if (v > 0) {
            outputHitPositive.TryGetValue(outputNeuron, out int currentPos);
            outputHitPositive[outputNeuron] = currentPos + 1;
        } else {
            outputHitNegative.TryGetValue(outputNeuron, out int currentNeg);
            outputHitNegative[outputNeuron] = currentNeg + 1;
        }
        outputNeuron.Act(state, v);
    }
    
    void FillInternalNeurons(AbstractCreatureState state) {
        foreach (var internalNeuron in internalNeurons) {
            if (!neuralValues.ContainsKey(internalNeuron)) {
                continue;
            }
            double v = neuralValues[internalNeuron];
            if (outgoingConnections.ContainsKey(internalNeuron)) {
                foreach (var c in outgoingConnections[internalNeuron]) {
                    double prevValue = 0;
                    neuralValues.TryGetValue(c.dest, out prevValue);
                    double myValue = NeuronTransformOutput(v) * c.weight;
                    if (double.IsNaN(myValue)) {
                        throw new Exception($"Neuron {internalNeuron} transformed {v} to {myValue} creating NaN");
                    }
                    neuralValues[c.dest] = prevValue + myValue;
                }
            }
        }
    }

    void FillInputValues(AbstractCreatureState state) {
        foreach (var input in inputNeurons) {
            if (!outgoingConnections.ContainsKey(input)) {
                // FIXME: this shouldn't happen
                continue;
            }
            var v = input.Value(state);
            foreach (var c in outgoingConnections[input]) {
                double prevValue = 0;
                neuralValues.TryGetValue(c.dest, out prevValue);
                double myValue = NeuronTransformOutput(v) * c.weight;
                if (double.IsNaN(myValue)) {
                    throw new Exception($"Neuron {input} created value {myValue} from state {state}");
                }
                neuralValues[c.dest] = prevValue + myValue;
            }
        }
    }

    double NeuronTransformOutput(double input) {
        return Math.Tanh(input);
    }

    string _styles = @"
      node [fontname=helvetica]
      edge [fontname=helvetica]
    ";
    
    public string GraphvizVisualization() {
        List<string> lines = new();
        int totalHits = outputHits.Select(x => x.Value).Sum();
        foreach (var item in outgoingConnections) {
            foreach (var conn in item.Value) {
                outputHits.TryGetValue(conn.dest, out int hits);
                outputHitPositive.TryGetValue(conn.dest, out int pos);
                outputHitNegative.TryGetValue(conn.dest, out int neg);
                double hitPortion = (double)hits / totalHits;
                if (double.IsNaN(hitPortion)) {
                    hitPortion = 0.33;
                }
                double posneg = ((double)pos / (pos + neg) - 0.5) * 2;
                // Then map it to red/blue
                posneg *= 0.16;
                if (posneg < 0) {
                    posneg += 1;
                }
                string destColor = $"green";
                if (!double.IsNaN(posneg)) {
                    destColor = $"\"{posneg:F3} 0.8 0.5\"";
                }
                string color = conn.weight < 0 ? "[color=red]" : "[color=blue]";
                double weight = Math.Sqrt(Math.Abs(conn.weight));
                string srcTooltip = ((AbstractNeuron)conn.source).Tooltip;
                if (!string.IsNullOrEmpty(srcTooltip)) {
                    srcTooltip = $"[tooltip=\"{srcTooltip}\"]";
                }
                string destTooltip = ((AbstractNeuron)conn.dest).Tooltip;
                if (!string.IsNullOrEmpty(destTooltip)) {
                    destTooltip = $"[tooltip=\"{destTooltip}\"]";
                }
                lines.Add(
                    $"  {{ {((AbstractNeuron)conn.source).Label} {srcTooltip} }} -> " +
                    $"{{ {((AbstractNeuron)conn.dest).Label} [color={destColor}] [penwidth={hitPortion*3:F3}] {destTooltip} }}" + 
                    $"{color} [penwidth={weight*4:F3}] [headlabel=\"   {conn.weight:F1}   \"] [arrowhead=empty]"
                    );
            }
        }
        string body = string.Join("\n", lines);
        return $"digraph {{{_styles}\n{body}\n}}";
    }

}

public class NeuralConnection {
    public IInputNeuron source;
    public IOutputNeuron dest;
    public double weight;

    public NeuralConnection(IInputNeuron source, IOutputNeuron dest, double weight) {
        this.source = source;
        this.dest = dest;
        this.weight = weight;
    }
}
