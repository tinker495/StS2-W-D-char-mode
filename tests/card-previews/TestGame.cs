// Fixture for the formal tests requested by wjdrb on 2026-10-25.
// Supply only a native viewport; never start game services or load user saves.
using MegaCrit.Sts2.Core.Nodes;

public partial class TestGame : NGame
{
    public override void _EnterTree() { }
    public override void _Ready() { }
    public override void _Process(double delta) { }
    public override void _ExitTree() { }
}
