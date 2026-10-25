// Human requester: wjdrb, 2026-10-25, explicitly approved formal tests for
// referenced card types, upgrade states, self-reference and viewport overflow,
// plus a local verification command. See README.md for the agreed contracts.
using Godot;

public partial class CardPreviewTests : Node
{
    public override void _Ready()
    {
        try
        {
            string[] arguments = OS.GetCmdlineUserArgs();
            int packArgument = Array.IndexOf(arguments, "--game-pack");
            if (packArgument < 0 || packArgument + 1 >= arguments.Length)
                throw new InvalidOperationException("Supply --game-pack <installed SlayTheSpire2.pck>.");

            var results = new TestResults();
            int modelStates = CardPreviewContracts.Run(results);
            int layoutCases = CardLayoutContracts.Run(this, arguments[packArgument + 1], results);
            GD.Print($"Card previews: {modelStates} model states, {layoutCases} layout cases, {results.Failures} failures.");
            if (results.Failures == 0) GD.Print("CARD_PREVIEW_TESTS_PASSED");
            GetTree().Quit(results.Failures == 0 ? 0 : 1);
        }
        catch (Exception error)
        {
            GD.PushError(error.ToString());
            GetTree().Quit(2);
        }
    }
}

internal sealed class TestResults
{
    public int Failures { get; private set; }

    public void Check(string name, bool passed, string details)
    {
        if (!passed) Failures++;
        GD.Print($"{(passed ? "PASS" : "FAIL")} {name}: {details}");
    }
}
