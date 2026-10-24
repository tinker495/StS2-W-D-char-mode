using MegaCrit.Sts2.Core.Map;

namespace Sts2WD.Sts2WDCode.Ancients.Aselige;

// Insert one actual floor while retaining the existing graph's branch choices.
// SavedActMap restores this topology directly, without reinserting on load.
internal sealed class AseligeLifeMap : ActMap
{
    public override MapPoint BossMapPoint { get; }
    public override MapPoint StartingMapPoint { get; }
    public override MapPoint? SecondBossMapPoint { get; }
    protected override MapPoint?[,] Grid { get; }

    internal AseligeLifeMap(ActMap source)
    {
        var insertAfter = source.GetAllMapPoints().Where(p => p.PointType == MapPointType.Treasure)
            .Select(p => p.coord.row).DefaultIfEmpty(Math.Max(1, source.GetRowCount() / 2)).Min();
        var originals = source.GetAllMapPoints().Append(source.StartingMapPoint).Append(source.BossMapPoint)
            .Concat(source.SecondBossMapPoint is { } second ? [second] : Array.Empty<MapPoint>()).Distinct().ToArray();
        var copies = originals.ToDictionary(p => p, p => new MapPoint(p.coord.col, p.coord.row > insertAfter ? p.coord.row + 1 : p.coord.row)
        { PointType = p.PointType, CanBeModified = p.CanBeModified });
        Grid = new MapPoint?[source.GetColumnCount(), source.GetRowCount() + 1];
        BossMapPoint = copies[source.BossMapPoint];
        StartingMapPoint = copies[source.StartingMapPoint];
        SecondBossMapPoint = source.SecondBossMapPoint is { } boss ? copies[boss] : null;
        foreach (var original in source.GetAllMapPoints())
        {
            var copy = copies[original];
            Grid[copy.coord.col, copy.coord.row] = copy;
        }
        foreach (var original in originals)
        {
            var copy = copies[original];
            foreach (var quest in original.Quests) copy.AddQuest(quest);
            if (original.coord.row == insertAfter && original.Children.Count > 0)
            {
                var rest = new MapPoint(copy.coord.col, insertAfter + 1)
                { PointType = MapPointType.RestSite, CanBeModified = false };
                Grid[rest.coord.col, rest.coord.row] = rest;
                copy.AddChildPoint(rest);
                foreach (var child in original.Children) rest.AddChildPoint(copies[child]);
            }
            else foreach (var child in original.Children) copy.AddChildPoint(copies[child]);
        }
        foreach (var start in source.startMapPoints) startMapPoints.Add(copies[start]);
    }
}
