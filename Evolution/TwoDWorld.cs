using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Evolution;

public class Location {
    public Location(int x, int y, TwoDWorld world) {
        this.x = x;
        this.y = y;
        this.world = world;
    }
    public readonly int x;
    public readonly int y;
    public readonly TwoDWorld world;

    public double XValue() {
        var w = (double)world.Width;
        return (w / x) * 2 - 1;
    }

    public double YValue() {
        var h = (double)world.Height;
        return (h / y) * 2 - 1;
    }

    public override string ToString() {
        return $"<Location {x}, {y}>";
    }

    public bool Occupied() {
        return world.Occupied(this);
    }
    
    public override bool Equals(Object? obj)
    {
        //Check for null and compare run-time types.
        if ((obj == null) || this.GetType() != obj.GetType())
        {
            return false;
        }
        Location other = (Location)obj;
        return x == other.x && y == other.y && world == other.world;
    }

    public override int GetHashCode()
    {
        return (x << 2) ^ y;
    }
    
}

public class TwoDCreatureState : AbstractCreatureState {
    public Location loc;
    public Location lastLoc;
    public TwoDWorld TwoDWorld => (TwoDWorld)World;

    public TwoDCreatureState(Creature creature, AbstractWorld world, Location loc, Location? lastLoc = null) : base(creature, world) {
        this.loc = loc;
        this.lastLoc = lastLoc ?? loc;
    }
    
    static int nullLocationIndex = 0;

    public override string ToString() {
        return $"[TwoD state at {loc.x},{loc.y} facing {Forward}]";
    }
    
    public AbstractDirection Forward {
        get {
            if (loc.Equals(lastLoc)) {
                // Trying to be deterministic but not biased to a direction
                nullLocationIndex = (nullLocationIndex + 1) % 4;
                return AbstractDirection.Directions[nullLocationIndex];
            }
            if (loc.x == lastLoc.x) {
                return loc.y < lastLoc.y ? VerticalDirection.South : VerticalDirection.North;
            } else {
                return loc.x < lastLoc.x ? HorizontalDirection.East : HorizontalDirection.West;
            }
        }
    }

    public void MoveTo(Location dest) {
        if (loc.Equals(dest)) {
            return;
        }
        if (dest.Occupied()) {
            throw new Exception($"Trying to move from {loc} to {dest} but it is occupied");
        }
        TwoDWorld.Move(loc, dest);
        lastLoc = loc;
        loc = dest;
    }

}

public abstract class AbstractDirection {
    protected int sign;
    public double rotation;

    public abstract Location AddTo(Location loc);
    public abstract AbstractDirection Left { get; }
    public abstract AbstractDirection Right { get; }
    public abstract AbstractDirection Back { get; }
    
    public static Location operator +(AbstractDirection d, Location loc) => d.AddTo(loc);
    public static Location operator +(Location loc, AbstractDirection d) => d.AddTo(loc);

    public static AbstractDirection[] Directions = new AbstractDirection[]
        { VerticalDirection.North, VerticalDirection.South, HorizontalDirection.East, HorizontalDirection.West };
}

public class HorizontalDirection : AbstractDirection {
    public static HorizontalDirection East = new HorizontalDirection { sign = 1, rotation = 0 };
    public static HorizontalDirection West = new HorizontalDirection { sign = -1, rotation = Math.PI };

    public override Location AddTo(Location loc) {
        var newX = Math.Clamp(loc.x + sign, 0, loc.world.Width - 1);
        return new Location(newX, loc.y, loc.world);
    }

    public override AbstractDirection Left => this == East ? VerticalDirection.South : VerticalDirection.North;
    public override AbstractDirection Right => this == East ? VerticalDirection.North : VerticalDirection.South;
    public override AbstractDirection Back => this == East ? West : East;

    public override string ToString() {
        return this == East ? "East" : "West";
    }
}

public class VerticalDirection : AbstractDirection {
    public static VerticalDirection North = new VerticalDirection { sign = -1, rotation = Math.PI / 2 };
    public static VerticalDirection South = new VerticalDirection { sign = 1, rotation = 3 * Math.PI / 2 };
    public override Location AddTo(Location loc) {
        var newY = Math.Clamp(loc.y + sign, 0, loc.world.Height - 1);
        return new Location(loc.x, newY, loc.world);
    }

    public override AbstractDirection Left => this == North ? HorizontalDirection.East : HorizontalDirection.West;
    public override AbstractDirection Right => this == North ? HorizontalDirection.West : HorizontalDirection.East;
    public override AbstractDirection Back => this == North ? South : North;
    
    public override string ToString() {
        return this == North ? "North" : "South";
    }
}

public class TwoDWorld : AbstractWorld
{
    public int Height = 128;
    public int Width = 128;
    
    List<List<TwoDCreatureState?>> cells = new();
    Dictionary<Creature, Location> creatureLocations = new();

    public override List<IInputNeuron> AvailableInputNeurons { get; }= StandardNeuronsSet.Inputs.Concat(TwoDNeuronsSet.Inputs).ToList();
    List<InternalNeuron> _availInternal = new();

    public override List<InternalNeuron> AvailableInternalNeurons {
        get {
            if (_availInternal.Count == 0) {
                for (int i = 0; i < NumberOfInternalNeurons; i++) {
                    _availInternal.Add(new InternalNeuron() {rank=i});
                }
            }
            return _availInternal;
        }
    }

    public override List<IOutputNeuron> AvailableOutputNeurons { get; } =
        StandardNeuronsSet.Outputs.Concat(TwoDNeuronsSet.Outputs).ToList();

    
    public TwoDWorld()
    {
        for (int i = 0; i < Height; i++)
        {
            List<TwoDCreatureState?> row = new();
            for (int j = 0; j < Width; j++)
            {
                row.Add(null);
            }
            cells.Add(row);
        }
    }

    public void PlaceCreature(Creature c, Location loc) {
        if (cells[loc.y][loc.x] != null) {
            throw new Exception($"Location {loc} already occupied by {cells[loc.y][loc.x]}");
        }
        var state = new TwoDCreatureState(c, this, loc, loc);
        cells[loc.y][loc.x] = state;
        creatureLocations[c] = loc;
    }

    public override void PlaceCreatureRandomly(Creature c) {
        PlaceCreature(c, RandomLocation());
    }

    public override void ShuffleCreatures() {
        List<TwoDCreatureState?> flatStates = new();
        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                flatStates.Add(cells[y][x]);
            }
        }
        var shuffled = Shuffle(flatStates);
        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                var state = shuffled[x * Width + y];
                if (state != null) {
                    state.loc = new Location(x, y, this);
                    creatureLocations[state.Creature] = state.loc;
                }
                cells[y][x] = state;
            }
        }
    }
    
    public Location RandomLocation() {
        int x, y;
        do {
            x = (int)(randomGenerator.NextDouble() * Width);
            y = (int)(randomGenerator.NextDouble() * Height);
        } while (cells[y][x] != null);
        return new Location(x, y, this);
    }

    public Location CreatureLocation(Creature c) {
        if (!creatureLocations.ContainsKey(c)) {
            throw new Exception($"Creature {c} is not in world");
        }
        return creatureLocations[c];
    }

    public override AbstractCreatureState CreatureState(Creature c) {
        var loc = CreatureLocation(c);
        var state = cells[loc.y][loc.x];
        if (state == null) {
            throw new Exception($"Creature {c} at location {loc} should have had state but none is found");
        }
        return state;
    }

    public List<TwoDCreatureState> CreatureStates => creatureLocations.Select(loc => cells[loc.Value.y][loc.Value.x])
        .OfType<TwoDCreatureState>().ToList();
    
    public bool Occupied(Location loc) {
        var c = cells[loc.y][loc.x];
        return c != null;
    }

    public void Move(Location loc, Location dest) {
        var state = cells[loc.y][loc.x];
        if (state == null) {
            throw new Exception($"Trying to move from {loc} to {dest} but there's no state");
        }
        cells[dest.y][dest.x] = state;
        cells[loc.y][loc.x] = null;
        creatureLocations[state.Creature] = dest;
    }
    
    public override void Kill(Creature c) {
        var state = (TwoDCreatureState)CreatureState(c);
        cells[state.loc.y][state.loc.x] = null;
        creatureLocations.Remove(c);
    }

    public override Image<Rgba32> Draw() {
        var drawer = new TwoDDrawer();
        return drawer.Draw(this);
    }
}
