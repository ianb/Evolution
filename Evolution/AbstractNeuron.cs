namespace Evolution; 

public abstract class AbstractNeuron {
    public abstract string Label { get; }
    public virtual string Tooltip { get; } = "";
    public virtual double RandomPickWeight { get; } = 1.0;
}

public interface IInputNeuron {
    public double Value(AbstractCreatureState state);
}

public interface IOutputNeuron {
    public bool CanAct(AbstractCreatureState state, double value);
    public void Act(AbstractCreatureState state, double value);
}
