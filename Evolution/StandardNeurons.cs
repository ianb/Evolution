namespace Evolution;

public class StandardNeuronsSet {
    public static List<IInputNeuron> Inputs => new() {
        new AgeNeuron(),
        new RandomNeuron(),
        new Oscillator(),
        new ConstantInput(),
    };

    public static List<IOutputNeuron> Outputs => new() {
        new NullOutput(),
    };

}

public class AgeNeuron : AbstractNeuron, IInputNeuron {
    public override string Label { get; } = "Age";

    public double Value(AbstractCreatureState state) {
        return (double)state.Age / state.World.EpochSteps;
    }
}

public class RandomNeuron : AbstractNeuron, IInputNeuron {
    public override string Label { get; } = "RND";

    public double Value(AbstractCreatureState state) {
        return state.World.randomGenerator.NextDouble() * 2 - 1;
    }
}

public class Oscillator : AbstractNeuron, IInputNeuron {
    public override string Label { get; } = "Osc";

    double eraHertz = 10;
    public double Value(AbstractCreatureState state) {
        var pos = (double)state.Age / state.World.EpochSteps;
        pos *= eraHertz;
        pos *= Math.PI;
        return Math.Sin(pos);
    }
}

public class ConstantInput : AbstractNeuron, IInputNeuron {
    public override string Label { get; } = "Const";

    public double Value(AbstractCreatureState state) {
        return 1.0;
    }
}

public class NullOutput : AbstractNeuron, IOutputNeuron {
    public override string Label { get; } = "null";
    public override double RandomPickWeight { get; } = 0.1;

    public bool CanAct(AbstractCreatureState state, double value) {
        return true;
    }

    public void Act(AbstractCreatureState state, double value) {
    }
}

public class NoPossible : NullOutput {
    public override double RandomPickWeight { get; } = 0;
}

public class InternalNeuron : AbstractNeuron, IOutputNeuron, IInputNeuron {
    public override string Label => $"N{rank+1}";

    public int rank;
    
    public bool CanAct(AbstractCreatureState state, double value) {
        return false;
    }

    public void Act(AbstractCreatureState state, double value) {
        state.NeuronValues[this] = value;
    }

    public double Value(AbstractCreatureState state) {
        double result = 0;
        state.NeuronValues.TryGetValue(this, out result);
        return result;
    }

    public override string ToString() {
        return $"InternalNeuron(rank={rank})";
    }
}

// FIXME: should have SetResponsivenessNeuron