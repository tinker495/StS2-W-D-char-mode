// Fixture for the formal tests requested by wjdrb on 2026-10-25.
// Native geometry and card models are real; visual resource loading is excluded.
using MegaCrit.Sts2.Core.Nodes.Cards;

public partial class TestCard : NCard
{
    public override void _EnterTree() { }
    public override void _Ready() { }
    public override void _Process(double delta) { }
    public override void _ExitTree() { }
}
