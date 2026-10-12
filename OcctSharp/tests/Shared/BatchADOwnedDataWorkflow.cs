using OcctSharp.OwnedValues;

namespace OcctSharp.Tests.Shared;

internal static class BatchADOwnedDataWorkflow
{
    internal static void Run()
    {
        using HatchGenPointOnElement element = HatchGenPointOnElement.Create();
        element.SetIndex(7);
        element.SetParameter(0.25);
        using HatchGenPointOnHatching input = HatchGenPointOnHatching.Create();
        input.SetParameter(2);
        input.AddPoint(element, 1e-9);
        using HatchGenDomain domain = HatchGenDomain.Create(input, true);
        element.SetIndex(99);
        input.ClrPoints();
        input.Dispose();
        element.Dispose();
        using HatchGenPointOnHatching endpoint = domain.FirstPoint();
        domain.SetPoints();
        domain.Dispose();
        using HatchGenPointOnElement copy = endpoint.Point(1);
        endpoint.Dispose();
        if (copy.Index() != 7 || copy.Parameter() != 0.25)
            throw new InvalidOperationException("Copied hatch sequence did not survive source mutation and disposal.");

        using IntToolsRange source = IntToolsRange.Create(2, 5);
        using IntToolsCommonPrt common = IntToolsCommonPrt.Create();
        common.SetRange1(source);
        source.SetFirst(20);
        source.Dispose();
        using IntToolsRange range = common.Range1();
        common.SetRange1(-2, -1);
        common.Dispose();
        if (range.First() != 2 || range.Last() != 5)
            throw new InvalidOperationException("Intersection range aliases its source owner.");

        using SelectBasicsPickResult near = SelectBasicsPickResult.Create(2, 1, new Point3d(1, 2, 3));
        using SelectBasicsPickResult far = SelectBasicsPickResult.Create(8, 1, new Point3d(4, 5, 6));
        using SelectBasicsPickResult picked = SelectBasicsPickResult.Min(near, far);
        near.Invalidate();
        near.Dispose();
        far.Dispose();
        if (!picked.IsValid() || picked.Depth() != 2 || picked.PickedPoint().Z != 3)
            throw new InvalidOperationException("Nearest pick result was not independently copied.");
    }
}
