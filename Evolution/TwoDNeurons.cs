namespace Evolution;

public class TwoDNeuronsSet {
    public static List<IInputNeuron> Inputs => new() {
        new XPositionNeuron(),
        new YPositionNeuron(),
        new NearestBorder(),
        new LastMovementX(),
        new LastMovementY(),
        new BlockageLeft(),
        new BlockageRight(),
        new BlockageLeftAndRight(),
        new BlockageBack(),
        new BlockageForward(),
    };

    public static List<IOutputNeuron> Outputs => new() {
        new MoveForward(),
        new MoveLeftRight(),
        new MoveBack(),
        new MoveLeftOrRight(),
        new MoveRandom(),
        new MoveEastWest(),
        new MoveNorthSouth(),
    };

}

public abstract class AbstractTwoDNeuron : AbstractNeuron {
}

public abstract class AbstractTwoDInputNeuron : AbstractTwoDNeuron, IInputNeuron {

    public double Value(AbstractCreatureState aState) {
        if (aState == null) {
            throw new Exception($"Attempt to call .Value() with null state");
        }
        var state = (TwoDCreatureState)aState;
        if (state == null) {
            throw new Exception($"State {aState} is not a TwoDCreatureState");
        }
        return TwoDValue(state);
    }

    protected abstract double TwoDValue(TwoDCreatureState state);

}

public abstract class AbstractTwoDOutputNeuron : AbstractTwoDNeuron, IOutputNeuron {
    public bool CanAct(AbstractCreatureState aState, double value) {
        var state = (TwoDCreatureState)aState;
        return TwoDCanAct(state, value);
    }
    
    public void Act(AbstractCreatureState aState, double value) {
        var state = (TwoDCreatureState)aState;
        TwoDAct(state, value);
    }

    protected abstract void TwoDAct(TwoDCreatureState state, double value);
    protected abstract bool TwoDCanAct(TwoDCreatureState state, double value);

}

public class XPositionNeuron : AbstractTwoDInputNeuron {
    public override string Label { get; } = "Xin";
    public override string Tooltip { get; } = "-1=left, +1=right, 0=center";

    protected override double TwoDValue(TwoDCreatureState state) {
        return state.loc.XValue();
    }
}

public class YPositionNeuron : AbstractTwoDInputNeuron {
    public override string Label { get; } = "Yin";
    public override string Tooltip { get; } = "-1=top, +1=bottom, 0=center";

    protected override double TwoDValue(TwoDCreatureState state) {
        return state.loc.YValue();
    }
}

public class NearestBorder : AbstractTwoDInputNeuron {
    public override string Label { get; } = "Border";
    public override string Tooltip { get; } = "0=center, 1=on some border/edge";

    protected override double TwoDValue(TwoDCreatureState state) {
        var y = state.loc.YValue();
        var x = state.loc.XValue();
        return Math.Max(Math.Abs(x), Math.Abs(y));
    }
}

public class LastMovementX : AbstractTwoDInputNeuron {
    public override string Label { get; } = "PastMoveX";
    public override string Tooltip { get; } = "1 or -1 if last movement changed X";

    protected override double TwoDValue(TwoDCreatureState state) {
        var nextLoc = state.loc + state.Forward;
        return nextLoc.x - state.loc.x;
    }
}

public class LastMovementY : AbstractTwoDInputNeuron {
    public override string Label { get; } = "PastMoveY";
    public override string Tooltip { get; } = "1 or -1 if last movement changed Y";

    protected override double TwoDValue(TwoDCreatureState state) {
        var nextLoc = state.loc + state.Forward;
        return nextLoc.y - state.loc.y;
    }
}

public class BlockageLeft : AbstractTwoDInputNeuron {
    public override string Label { get; } = "BlockL";
    public override string Tooltip { get; } = "1 if Left blocked, else 0";

    protected override double TwoDValue(TwoDCreatureState state) {
        return (state.loc + state.Forward.Left).Occupied() ? 1 : 0;
    }
}

public class BlockageRight : AbstractTwoDInputNeuron {
    public override string Label { get; } = "BlockR";
    public override string Tooltip { get; } = "1 if Right blocked, else 0";

    protected override double TwoDValue(TwoDCreatureState state) {
        if (state == null) throw new Exception("state is null");
        if (state.loc == null) throw new Exception("state.loc is null");
        if (state.Forward == null) throw new Exception("state.Forward is null");
        return (state.loc + state.Forward.Right).Occupied() ? 1 : 0;
    }
}

public class BlockageLeftAndRight : AbstractTwoDInputNeuron {
    public override string Label { get; } = "BlockLoR";
    public override string Tooltip { get; } = "1 if Right AND Left blocked, else 0";

    protected override double TwoDValue(TwoDCreatureState state) {
        return (state.loc + state.Forward.Right).Occupied() && (state.loc + state.Forward.Left).Occupied() ? 1 : 0;
    }
}

public class BlockageBack : AbstractTwoDInputNeuron {
    public override string Label { get; } = "BlockB";
    public override string Tooltip { get; } = "1 if Back blocked, else 0";

    protected override double TwoDValue(TwoDCreatureState state) {
        return (state.loc + state.Forward.Back).Occupied() ? 1 : 0;
    }
}

public class BlockageForward : AbstractTwoDInputNeuron {
    public override string Label { get; } = "BlockF";
    public override string Tooltip { get; } = "1 if Forward blocked, else 0";

    protected override double TwoDValue(TwoDCreatureState state) {
        return (state.loc + state.Forward).Occupied() ? 1 : 0;
    }
}

public abstract class AbstractMove : AbstractTwoDOutputNeuron {
    protected abstract Location Destination(TwoDCreatureState state, double value);

    protected override bool TwoDCanAct(TwoDCreatureState state, double value) {
        return !Destination(state, value).Occupied();
    }
    
    protected override void TwoDAct(TwoDCreatureState state, double value) {
        var dest = Destination(state, value);
        if (!dest.Occupied()) {
            state.MoveTo(dest);
        }
    }

}

public class MoveForward : AbstractMove {
    public override string Label { get; } = "MoveF";
    public override string Tooltip { get; } = "Move Forward (binary)";

    protected override Location Destination(TwoDCreatureState state, double value) {
        return state.loc + state.Forward;
    }
}

public class MoveLeftRight : AbstractMove {
    public override string Label { get; } = "MoveLR";
    public override string Tooltip { get; } = "Move Right if value>0, else Left";

    protected override Location Destination(TwoDCreatureState state, double value) {
        if (value > 0) {
            return state.loc + state.Forward.Right;
        }
        return state.loc + state.Forward.Left;
    }
}

public class MoveBack : AbstractMove {
    public override string Label { get; } = "MoveB";
    public override string Tooltip { get; } = "Move Back (binary)";

    protected override Location Destination(TwoDCreatureState state, double value) {
        return state.loc + state.Forward.Back;
    }
}

public class MoveLeftOrRight : AbstractMove {
    public override string Label { get; } = "MoveLoR";
    public override string Tooltip { get; } = "Move Left or Right, whichever is unoccupied; if value>0 prefer Right";

    protected override Location Destination(TwoDCreatureState state, double value) {
        if (value > 0) {
            if ((state.loc + state.Forward.Right).Occupied()) {
                return state.loc + state.Forward.Left;
            }
            return state.loc + state.Forward.Right;
        }
        if ((state.loc + state.Forward.Left).Occupied()) {
            return state.loc + state.Forward.Right;
        }
        return state.loc + state.Forward.Left;
    }
}

public class MoveRandom : AbstractMove {
    public override string Label { get; } = "MoveRND";
    public override string Tooltip { get; } = "Move randomly, irrespective of value";

    protected override Location Destination(TwoDCreatureState state, double value) {
        var offset = (int)(state.World.randomGenerator.NextDouble() * 4);
        for (int i = 0; i < 4; i++) {
            var index = (i + offset) % 4;
            var dir = AbstractDirection.Directions[i];
            var loc = state.loc + dir;
            if (!loc.Occupied()) {
                return loc;
            }
        }
        return state.loc;
    }
}

public class MoveEastWest : AbstractMove {
    public override string Label { get; } = "MoveEW";
    public override string Tooltip { get; } = "Move West if value>0, else East";

    protected override Location Destination(TwoDCreatureState state, double value) {
        if (value > 0) {
            return state.loc + HorizontalDirection.West;
        }
        return state.loc + HorizontalDirection.East;
    }
}

public class MoveNorthSouth : AbstractMove {
    public override string Label { get; } = "MoveNS";
    public override string Tooltip { get; } = "Move South if value>0, else North";

    protected override Location Destination(TwoDCreatureState state, double value) {
        if (value > 0) {
            return state.loc + VerticalDirection.South;
        }
        return state.loc + VerticalDirection.North;
    }
}
