using System.Runtime.InteropServices;

namespace OcctSharp.Runtime.Tests;

public sealed class BatchECompletionTests
{
    [Fact]
    public void PmiReferenceReplacementDetachRollbackAndRemovalAreComplete()
    {
        using Shape firstShape = ShapeFactory.CreateBox(10, 10, 10);
        using Shape secondShape = ShapeFactory.CreateBox(5, 5, 5);
        using XdeDocument document = XdeDocument.Create();
        XdeLabel first;
        XdeLabel second;
        XdeDatum datum;
        XdeDimension dimension;
        XdeGeomTolerance tolerance;
        XdeSavedView view;

        using (XdeTransaction transaction = document.BeginTransaction())
        {
            first = document.AddShape(firstShape, "First");
            second = document.AddShape(secondShape, "Second");
            datum = document.CreateDatum(new XdeDatumDefinition
            {
                Name = "A",
                Identification = "A",
                SemanticName = "Datum A"
            }, [first]);
            dimension = document.CreateDimension(new XdeDimensionDefinition(
                XCAFDimTolObjectsDimensionType.XCAFDimTolObjects_DimensionType_Location_LinearDistance,
                [10.0]), [first], [second]);
            tolerance = document.CreateGeometricTolerance(new XdeGeomToleranceDefinition
            {
                Type = XCAFDimTolObjectsGeomToleranceType.XCAFDimTolObjects_GeomToleranceType_Flatness,
                Value = 0.1,
                SemanticName = "Flatness"
            }, [first], [datum]);
            view = document.CreateSavedView(new XdeSavedViewDefinition { Name = "Initial" }, [first], [dimension]);
            Assert.True(transaction.Commit());
        }

        using (XdeTransaction transaction = document.BeginTransaction())
        {
            dimension.SetReferences([second]);
            datum.SetReferences([second]);
            tolerance.SetReferences([second], []);
            view.Update(new XdeSavedViewDefinition { Name = "Updated" }, [], []);
            Assert.True(transaction.Commit());
        }

        using (XdeDimensionSnapshot snapshot = dimension.GetSnapshot())
        {
            Assert.Equal([second.Entry], snapshot.FirstShapeEntries);
            Assert.Empty(snapshot.SecondShapeEntries);
        }
        using (XdeDatumSnapshot snapshot = datum.GetSnapshot())
            Assert.Equal([second.Entry], snapshot.ShapeEntries);
        using (XdeGeomToleranceSnapshot snapshot = tolerance.GetSnapshot())
        {
            Assert.Equal([second.Entry], snapshot.ShapeEntries);
            Assert.Empty(snapshot.DatumEntries);
        }
        XdeSavedViewSnapshot updatedView = view.GetSnapshot();
        Assert.Equal("Updated", updatedView.Definition.Name);
        Assert.Empty(updatedView.VisibleShapeEntries);
        Assert.Empty(updatedView.PmiEntries);

        using (XdeTransaction transaction = document.BeginTransaction())
        {
            dimension.Update(new XdeDimensionDefinition(
                XCAFDimTolObjectsDimensionType.XCAFDimTolObjects_DimensionType_Size_Radius,
                [25.0]) { SemanticName = "Rolled back" });
            tolerance.Remove();
            view.Remove();
            transaction.Abort();
        }
        using (XdeDimensionSnapshot snapshot = dimension.GetSnapshot())
            Assert.Equal(10.0, Assert.Single(snapshot.Definition.Values), 8);
        Assert.Single(document.GetGeometricTolerances());
        Assert.Single(document.GetSavedViews());

        using (XdeTransaction transaction = document.BeginTransaction())
        {
            dimension.Remove();
            tolerance.Remove();
            datum.Remove();
            view.Remove();
            Assert.True(transaction.Commit());
        }
        Assert.Empty(document.GetDimensions());
        Assert.Empty(document.GetGeometricTolerances());
        Assert.Empty(document.GetDatums());
        Assert.Empty(document.GetSavedViews());
        Assert.Throws<ArgumentException>(() => dimension.GetSnapshot());
        Assert.Throws<ArgumentException>(() => tolerance.GetSnapshot());
        Assert.Throws<ArgumentException>(() => datum.GetSnapshot());
        Assert.Throws<ArgumentException>(() => view.GetSnapshot());
    }

    [Fact]
    public void CompletePmiSnapshotsOwnershipAndInvalidTopologyGuardsArePreserved()
    {
        GpAx2Value axis = GpAx2Value.Create(GpXyz.Origin, new GpXyz(0, 0, 1), new GpXyz(1, 0, 0));
        GpPlane affectedPlane = GpPlane.Create(GpXyz.Origin, new GpXyz(0, 1, 0));
        using Shape box = ShapeFactory.CreateBox(10, 20, 30);
        using Shape path = ShapeFactory.CreateEdge(GpPoint.Origin, new GpPoint(10, 0, 0));
        using Shape presentation = ShapeFactory.CreateBox(1, 1, 1);
        using XdeDocument document = XdeDocument.Create();
        XdeLabel part;
        XdeDimension dimension;
        XdeGeomTolerance tolerance;
        XdeDatum datum;
        XdeDatum areaDatum;

        XdeDimensionDefinition dimensionDefinition = new(
            XCAFDimTolObjectsDimensionType.XCAFDimTolObjects_DimensionType_Location_WithPath,
            [10.0, 10.2, 9.8])
        {
            Qualifier = XCAFDimTolObjectsDimensionQualifier.XCAFDimTolObjects_DimensionQualifier_Max,
            AngularQualifier = XCAFDimTolObjectsAngularQualifier.XCAFDimTolObjects_AngularQualifier_Large,
            ClassOfTolerance = new(false,
                XCAFDimTolObjectsDimensionFormVariance.XCAFDimTolObjects_DimensionFormVariance_H,
                XCAFDimTolObjectsDimensionGrade.XCAFDimTolObjects_DimensionGrade_IT7),
            LeftDecimalPlaces = 2,
            RightDecimalPlaces = 4,
            Modifiers = [XCAFDimTolObjectsDimensionModif.XCAFDimTolObjects_DimensionModif_Between],
            Direction = new GpXyz(1, 0, 0),
            AnnotationPlane = axis,
            FirstPoint = GpPoint.Origin,
            SecondPoint = new GpPoint(10, 0, 0),
            TextPosition = new GpPoint(5, -2, 0),
            SemanticName = "Complete dimension",
            PresentationName = "Dimension presentation",
            Description = "Upper/lower dimensional limit",
            DescriptionName = "limits",
            Path = path,
            Presentation = presentation
        };
        XdeGeomToleranceDefinition toleranceDefinition = new()
        {
            Type = XCAFDimTolObjectsGeomToleranceType.XCAFDimTolObjects_GeomToleranceType_Position,
            TypeOfValue = XCAFDimTolObjectsGeomToleranceTypeValue.XCAFDimTolObjects_GeomToleranceTypeValue_Diameter,
            Value = 0.2,
            MaterialRequirement = XCAFDimTolObjectsGeomToleranceMatReqModif.XCAFDimTolObjects_GeomToleranceMatReqModif_M,
            ZoneModifier = XCAFDimTolObjectsGeomToleranceZoneModif.XCAFDimTolObjects_GeomToleranceZoneModif_Projected,
            ZoneModifierValue = 3.0,
            MaximumValueModifier = 0.4,
            Modifiers = [XCAFDimTolObjectsGeomToleranceModif.XCAFDimTolObjects_GeomToleranceModif_All_Around],
            Axis = axis,
            AnnotationPlane = axis,
            Point = new GpPoint(1, 2, 3),
            TextPosition = new GpPoint(4, 5, 6),
            AffectedPlaneType = XCAFDimTolObjectsToleranceZoneAffectedPlane.XCAFDimTolObjects_ToleranceZoneAffectedPlane_Intersection,
            AffectedPlane = affectedPlane,
            SemanticName = "Position tolerance",
            PresentationName = "Tolerance presentation",
            Presentation = presentation
        };
        XdeDatumDefinition datumDefinition = new()
        {
            Name = "A",
            Description = "Primary datum target",
            Identification = "A1",
            SemanticName = "Datum target A1",
            PresentationName = "Datum presentation",
            Modifiers = [XCAFDimTolObjectsDatumSingleModif.XCAFDimTolObjects_DatumSingleModif_Basic],
            ModifierWithValue = XCAFDimTolObjectsDatumModifWithValue.XCAFDimTolObjects_DatumModifWithValue_Distance,
            ModifierValue = 2.5,
            Position = 1,
            IsDatumTarget = true,
            TargetType = XCAFDimTolObjectsDatumTargetType.XCAFDimTolObjects_DatumTargetType_Rectangle,
            TargetAxis = axis,
            TargetLength = 8,
            TargetWidth = 4,
            TargetNumber = 7,
            AnnotationPlane = axis,
            Point = new GpPoint(2, 3, 4),
            TextPosition = new GpPoint(5, 6, 7),
            Presentation = presentation
        };

        using (XdeTransaction transaction = document.BeginTransaction())
        {
            part = document.AddShape(box, "Complete PMI part");
            datum = document.CreateDatum(datumDefinition, [part]);
            areaDatum = document.CreateDatum(new XdeDatumDefinition
            {
                Name = "B",
                Identification = "B1",
                SemanticName = "Area datum target",
                IsDatumTarget = true,
                TargetType = XCAFDimTolObjectsDatumTargetType.XCAFDimTolObjects_DatumTargetType_Area,
                TargetNumber = 8,
                Target = presentation
            }, [part]);
            dimension = document.CreateDimension(dimensionDefinition, [part], [part]);
            tolerance = document.CreateGeometricTolerance(toleranceDefinition, [part], [datum]);
            Assert.True(transaction.Commit());
        }

        path.Dispose();
        presentation.Dispose();
        using (XdeDimensionSnapshot snapshot = dimension.GetSnapshot())
        {
            Assert.Equal(dimensionDefinition.Values, snapshot.Definition.Values);
            Assert.Equal(dimensionDefinition.Qualifier, snapshot.Definition.Qualifier);
            Assert.Equal(dimensionDefinition.AngularQualifier, snapshot.Definition.AngularQualifier);
            Assert.Equal(dimensionDefinition.ClassOfTolerance, snapshot.Definition.ClassOfTolerance);
            Assert.Equal(2, snapshot.Definition.LeftDecimalPlaces);
            Assert.Equal(4, snapshot.Definition.RightDecimalPlaces);
            Assert.Equal(dimensionDefinition.Modifiers, snapshot.Definition.Modifiers);
            Assert.Equal(dimensionDefinition.Direction, snapshot.Definition.Direction);
            Assert.Equal(axis, snapshot.Definition.AnnotationPlane);
            Assert.Equal(dimensionDefinition.FirstPoint, snapshot.Definition.FirstPoint);
            Assert.Equal(dimensionDefinition.SecondPoint, snapshot.Definition.SecondPoint);
            Assert.Equal(dimensionDefinition.TextPosition, snapshot.Definition.TextPosition);
            Assert.Equal("Complete dimension", snapshot.Definition.SemanticName);
            Assert.Equal("Dimension presentation", snapshot.Definition.PresentationName);
            Assert.Equal("Upper/lower dimensional limit", snapshot.Definition.Description);
            Assert.Equal("limits", snapshot.Definition.DescriptionName);
            Assert.NotNull(snapshot.Definition.Path);
            Assert.Equal(ShapeKind.Edge, snapshot.Definition.Path.Kind);
            Assert.NotNull(snapshot.Definition.Presentation);
            Assert.Equal(ShapeKind.Solid, snapshot.Definition.Presentation.Kind);
        }
        using (XdeGeomToleranceSnapshot snapshot = tolerance.GetSnapshot())
        {
            Assert.Equal(toleranceDefinition.Type, snapshot.Definition.Type);
            Assert.Equal(toleranceDefinition.TypeOfValue, snapshot.Definition.TypeOfValue);
            Assert.Equal(toleranceDefinition.Value, snapshot.Definition.Value, 8);
            Assert.Equal(toleranceDefinition.MaterialRequirement, snapshot.Definition.MaterialRequirement);
            Assert.Equal(toleranceDefinition.ZoneModifier, snapshot.Definition.ZoneModifier);
            Assert.Equal(toleranceDefinition.ZoneModifierValue, snapshot.Definition.ZoneModifierValue, 8);
            Assert.Equal(toleranceDefinition.MaximumValueModifier, snapshot.Definition.MaximumValueModifier, 8);
            Assert.Equal(toleranceDefinition.Modifiers, snapshot.Definition.Modifiers);
            Assert.Equal(axis, snapshot.Definition.Axis);
            Assert.Equal(axis, snapshot.Definition.AnnotationPlane);
            Assert.Equal(toleranceDefinition.Point, snapshot.Definition.Point);
            Assert.Equal(toleranceDefinition.TextPosition, snapshot.Definition.TextPosition);
            Assert.Equal(toleranceDefinition.AffectedPlaneType, snapshot.Definition.AffectedPlaneType);
            Assert.Equal(affectedPlane, snapshot.Definition.AffectedPlane);
            Assert.Equal("Position tolerance", snapshot.Definition.SemanticName);
            Assert.NotNull(snapshot.Definition.Presentation);
        }
        using (XdeDatumSnapshot snapshot = datum.GetSnapshot())
        {
            Assert.Equal("A", snapshot.Definition.Name);
            Assert.Equal("Primary datum target", snapshot.Definition.Description);
            Assert.Equal("A1", snapshot.Definition.Identification);
            Assert.Equal(datumDefinition.Modifiers, snapshot.Definition.Modifiers);
            Assert.Equal(datumDefinition.ModifierWithValue, snapshot.Definition.ModifierWithValue);
            Assert.Equal(2.5, snapshot.Definition.ModifierValue, 8);
            Assert.True(snapshot.Definition.IsDatumTarget);
            Assert.Equal(datumDefinition.TargetType, snapshot.Definition.TargetType);
            Assert.Equal(axis, snapshot.Definition.TargetAxis);
            Assert.Equal(8, snapshot.Definition.TargetLength, 8);
            Assert.Equal(4, snapshot.Definition.TargetWidth, 8);
            Assert.Equal(7, snapshot.Definition.TargetNumber);
            Assert.Equal(axis, snapshot.Definition.AnnotationPlane);
            Assert.Equal(datumDefinition.Point, snapshot.Definition.Point);
            Assert.Equal(datumDefinition.TextPosition, snapshot.Definition.TextPosition);
            Assert.Null(snapshot.Definition.Target);
            Assert.NotNull(snapshot.Definition.Presentation);
            Assert.Equal([tolerance.Entry], snapshot.ToleranceEntries);
        }
        using (XdeDatumSnapshot snapshot = areaDatum.GetSnapshot())
        {
            Assert.NotNull(snapshot.Definition.Target);
            Assert.Equal(ShapeKind.Solid, snapshot.Definition.Target.Kind);
        }
        using (XdeTransaction transaction = document.BeginTransaction())
        {
            Assert.Throws<ArgumentException>(() => datum.Update(new XdeDatumDefinition
            {
                IsDatumTarget = true,
                TargetType = XCAFDimTolObjectsDatumTargetType.XCAFDimTolObjects_DatumTargetType_Rectangle,
                Target = box
            }));
            transaction.Abort();
        }

        using Shape line = ShapeFactory.CreateEdge(GpPoint.Origin, new GpPoint(1, 0, 0));
        using Shape circle = ShapeFactory.CreateCircleEdge(GpPoint.Origin, new GpPoint(0, 0, 1), 2);
        Assert.Throws<InvalidCastException>(() => box.InspectProperties(InspectionPropertyKind.Length));
        Assert.Throws<InvalidCastException>(() => line.InspectProperties(InspectionPropertyKind.Area));
        Assert.Throws<InvalidCastException>(() => line.InspectProperties(InspectionPropertyKind.Volume));
        Assert.Throws<InvalidCastException>(() => line.InspectAngleTo(circle));
        Assert.Throws<InvalidCastException>(() => box.InspectRadius());

        using XdeDocument foreignDocument = XdeDocument.Create();
        XdeLabel foreignPart;
        XdeDatum foreignDatum;
        using (XdeTransaction transaction = foreignDocument.BeginTransaction())
        {
            foreignPart = foreignDocument.AddShape(box, "Foreign part");
            foreignDatum = foreignDocument.CreateDatum(new XdeDatumDefinition { Name = "B" }, [foreignPart]);
            Assert.True(transaction.Commit());
        }
        using (XdeTransaction transaction = document.BeginTransaction())
        {
            Assert.Throws<ArgumentException>(() => dimension.SetReferences([foreignPart]));
            Assert.Throws<ArgumentException>(() => tolerance.SetReferences([part], [foreignDatum]));
            Assert.Throws<ArgumentException>(() => document.CreateSavedView(new XdeSavedViewDefinition(), [foreignPart], [dimension]));
            transaction.Abort();
        }
    }

    [Fact]
    public void ExactInspectionPmiTransactionsAndAp242RoundTripRunAsOneClosure()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"OcctSharp.BatchE.{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            InspectionUnits units = new("mm", "in", "rad", "deg", 4);
            using Shape first = ShapeFactory.CreateBox(10, 20, 30);
            using Shape translatedSource = ShapeFactory.CreateBox(10, 20, 30);
            using Shape second = translatedSource.Transformed(
                ShapeTransform.CreateTranslationAndRotationZ(15, 0, 0, 0));
            using ExactDistanceResult distance = first.InspectDistanceTo(second, units);
            Assert.Equal(5, distance.Distance, 8);
            Assert.Equal(5 / 25.4, distance.DisplayDistance.DisplayValue, 8);
            Assert.NotEmpty(distance.Solutions);
            Assert.All(distance.Solutions, solution =>
            {
                Assert.Equal(5, solution.Distance, 8);
                Assert.NotEqual(ShapeKind.Shape, solution.FirstSupport.Kind);
                Assert.NotEqual(ShapeKind.Shape, solution.SecondSupport.Kind);
            });

            using (ShapePairInspection separated = first.InspectPair(second, units: units))
                Assert.Equal(ShapePairClassification.Separated, separated.Classification);
            using Shape touchingSource = ShapeFactory.CreateBox(10, 20, 30);
            using Shape touching = touchingSource.Transformed(
                ShapeTransform.CreateTranslationAndRotationZ(10, 0, 0, 0));
            using (ShapePairInspection contact = first.InspectPair(touching))
                Assert.Equal(ShapePairClassification.Touching, contact.Classification);
            using Shape containedSource = ShapeFactory.CreateBox(2, 2, 2);
            using Shape contained = containedSource.Transformed(
                ShapeTransform.CreateTranslationAndRotationZ(2, 2, 2, 0));
            using (ShapePairInspection containment = first.InspectPair(contained))
                Assert.Equal(ShapePairClassification.Contained, containment.Classification);
            using Shape overlapSource = ShapeFactory.CreateBox(10, 20, 30);
            using Shape overlap = overlapSource.Transformed(
                ShapeTransform.CreateTranslationAndRotationZ(5, 0, 0, 0));
            using (ShapePairInspection interference = first.InspectPair(overlap))
            {
                Assert.Equal(ShapePairClassification.Interfering, interference.Classification);
                Assert.NotNull(interference.Overlap);
                Assert.True(interference.OverlapVolume > 0);
            }

            using Shape lineX = ShapeFactory.CreateEdge(GpPoint.Origin, new GpPoint(10, 0, 0));
            using Shape lineY = ShapeFactory.CreateEdge(GpPoint.Origin, new GpPoint(0, 10, 0));
            Assert.Equal(10, lineX.InspectProperties(InspectionPropertyKind.Length).Mass, 8);
            Assert.Equal(90, lineX.InspectAngleTo(lineY).DisplayValue, 8);
            ShapeInspectionProperties volume = first.InspectProperties(InspectionPropertyKind.Volume, units);
            Assert.Equal(6000, volume.Mass, 6);
            Assert.Equal(new GpPoint(5, 10, 15), volume.CenterOfMass);
            using Shape circle = ShapeFactory.CreateCircleEdge(GpPoint.Origin, new GpPoint(0, 0, 1), 5);
            ShapeRadialMeasurement radial = circle.InspectRadius(units);
            Assert.Equal(RadialGeometryKind.Circle, radial.GeometryKind);
            Assert.Equal(5, radial.Radius, 8);
            Assert.Equal(10, radial.Diameter, 8);

            string binary = Path.Combine(directory, "batch-e.xbf");
            string step = Path.Combine(directory, "batch-e-ap242.step");
            using (XdeDocument document = XdeDocument.Create())
            {
                XdeLabel part;
                using (XdeTransaction transaction = document.BeginTransaction())
                {
                    part = document.AddShape(first, "Inspection part");
                    XdeDatum datum = document.CreateDatum(new XdeDatumDefinition
                    {
                        Name = "A",
                        Identification = "A",
                        SemanticName = "Primary datum",
                        Position = 1
                    }, [part]);
                    XdeDimension dimension = document.CreateDimension(new XdeDimensionDefinition(
                        XCAFDimTolObjectsDimensionType.XCAFDimTolObjects_DimensionType_Location_LinearDistance,
                        [10.0])
                    {
                        SemanticName = "Overall length",
                        Description = "Nominal overall length",
                        DescriptionName = "inspection",
                        FirstPoint = GpPoint.Origin,
                        SecondPoint = new GpPoint(10, 0, 0),
                        TextPosition = new GpPoint(5, -3, 0)
                    }, [part], [part]);
                    XdeGeomTolerance tolerance = document.CreateGeometricTolerance(new XdeGeomToleranceDefinition
                    {
                        Type = XCAFDimTolObjectsGeomToleranceType.XCAFDimTolObjects_GeomToleranceType_Flatness,
                        TypeOfValue = XCAFDimTolObjectsGeomToleranceTypeValue.XCAFDimTolObjects_GeomToleranceTypeValue_None,
                        Value = 0.1,
                        SemanticName = "Flatness"
                    }, [part], [datum]);
                    _ = document.CreateSavedView(new XdeSavedViewDefinition
                    {
                        Name = "Inspection view",
                        ProjectionType = XCAFViewProjectionType.XCAFView_ProjectionType_Parallel,
                        ProjectionPoint = new GpPoint(30, 30, 30),
                        ViewDirection = new GpXyz(-1, -1, -1),
                        UpDirection = new GpXyz(0, 0, 1),
                        ZoomFactor = 1.25,
                        WindowHorizontalSize = 100,
                        WindowVerticalSize = 80,
                        ClippingExpression = "1",
                        ClippingPlanes = [new ViewerPlaneEquation(1, 0, 0, -8)]
                    }, [part], [dimension, tolerance, datum]);
                    Assert.True(transaction.Commit());
                }

                Assert.Single(document.GetDimensions());
                Assert.Single(document.GetGeometricTolerances());
                Assert.Single(document.GetDatums());
                XdeSavedView savedView = Assert.Single(document.GetSavedViews());
                using (XdeDimensionSnapshot dimension = Assert.Single(document.GetDimensions()).GetSnapshot())
                {
                    Assert.Equal(10, Assert.Single(dimension.Definition.Values), 8);
                    Assert.Contains(part.Entry, dimension.FirstShapeEntries);
                }
                using (XdeGeomToleranceSnapshot tolerance = Assert.Single(document.GetGeometricTolerances()).GetSnapshot())
                {
                    Assert.Equal(0.1, tolerance.Definition.Value, 8);
                    Assert.Single(tolerance.DatumEntries);
                }
                XdeSavedViewSnapshot view = savedView.GetSnapshot();
                Assert.Contains(part.Entry, view.VisibleShapeEntries);
                Assert.Equal(3, view.PmiEntries.Count);
                Assert.Single(view.Definition.ClippingPlanes);

                document.Save(binary);
                document.WriteStep(step, new XdeStepWriteOptions(
                    ModelType: XdeStepModelType.AsIs,
                    WriteGdt: true,
                    Schema: XdeStepSchema.Ap242));
            }

            using (XdeDocument reopened = XdeDocument.Open(binary))
            {
                Assert.Single(reopened.GetDimensions());
                Assert.Single(reopened.GetGeometricTolerances());
                Assert.Single(reopened.GetDatums());
                Assert.Single(reopened.GetSavedViews());
            }
            using (XdeDocument imported = XdeDocument.ReadStep(step, new XdeStepReadOptions(
                ReadGdt: true, ReadSavedViews: true)))
            {
                Assert.Single(imported.GetDimensions());
                Assert.Single(imported.GetGeometricTolerances());
                Assert.Single(imported.GetDatums());
            }
            using (XdeDocument withoutGdt = XdeDocument.ReadStep(step, new XdeStepReadOptions(
                ReadGdt: false, ReadSavedViews: false)))
            {
                Assert.Empty(withoutGdt.GetDimensions());
                Assert.Empty(withoutGdt.GetGeometricTolerances());
                Assert.Empty(withoutGdt.GetDatums());
                Assert.Empty(withoutGdt.GetSavedViews());
            }

            using (XdeDocument aborted = XdeDocument.Create())
            using (XdeTransaction transaction = aborted.BeginTransaction())
            {
                XdeLabel part = aborted.AddShape(first, "Aborted part");
                _ = aborted.CreateDimension(new XdeDimensionDefinition(
                    XCAFDimTolObjectsDimensionType.XCAFDimTolObjects_DimensionType_Location_LinearDistance,
                    [1.0]), [part]);
                transaction.Abort();
                Assert.Empty(aborted.GetDimensions());
            }
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void SavedViewAndFourViewerDimensionsRunOnARealHwndWithOwnershipGuardsAndScreenshot()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"OcctSharp.BatchE.Viewer.{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        nint window = CreateTestWindow();
        try
        {
            using OcctViewer viewer = OcctViewer.Create(window);
            using Shape box = ShapeFactory.CreateBox(10, 10, 10);
            using Shape circle = ShapeFactory.CreateCircleEdge(new GpPoint(5, 5, 12), new GpPoint(0, 0, 1), 3);
            ViewerPresentation presentation = viewer.Display(box);
            ViewerDimensionStyle style = new()
            {
                Units = new InspectionUnits("mm", "mm", "rad", "deg", 2),
                Color = new ViewerColor(1, 0.8, 0.1),
                Flyout = 6,
                LineWidth = 2
            };
            using ViewerDimension length = viewer.DisplayLengthDimension(
                GpPoint.Origin, new GpPoint(10, 0, 0), new ViewerPlaneEquation(0, 0, 1, 0), style);
            using ViewerDimension angle = viewer.DisplayAngleDimension(
                new GpPoint(10, 0, 0), GpPoint.Origin, new GpPoint(0, 10, 0), style);
            using ViewerDimension radius = viewer.DisplayRadiusDimension(circle, style);
            using ViewerDimension diameter = viewer.DisplayDiameterDimension(circle, style);
            length.Hide();
            Assert.False(length.IsVisible);
            length.Show();
            Assert.True(length.IsVisible);
            angle.UpdateStyle(style with { Color = new ViewerColor(0.2, 0.8, 1), Flyout = 8 });
            angle.UpdateStyle(style with { CustomValue = 45 });
            angle.UpdateStyle(style with { CustomValue = null });
            radius.SetSelected();
            radius.SetSelected(false);
            viewer.FitAll();
            viewer.Redraw();

            using XdeDocument document = XdeDocument.Create();
            XdeSavedView saved;
            using (XdeTransaction transaction = document.BeginTransaction())
            {
                XdeLabel part = document.AddShape(box, "Viewer part");
                saved = document.CreateSavedView(new XdeSavedViewDefinition
                {
                    Name = "Viewer inspection",
                    ProjectionType = XCAFViewProjectionType.XCAFView_ProjectionType_Parallel,
                    ProjectionPoint = new GpPoint(30, 30, 30),
                    ViewDirection = new GpXyz(-1, -1, -1),
                    UpDirection = new GpXyz(0, 0, 1),
                    ClippingPlanes = [new ViewerPlaneEquation(1, 0, 0, -9)]
                }, [part]);
                Assert.True(transaction.Commit());
            }
            saved.ApplyTo(viewer);
            string screenshot = viewer.SaveScreenshot(Path.Combine(directory, "Batch-E-检测.png"));
            Assert.True(new FileInfo(screenshot).Length > 0);

            Exception? threadError = null;
            Thread worker = new(() =>
            {
                try { length.Hide(); }
                catch (Exception error) { threadError = error; }
            });
            worker.Start();
            worker.Join();
            Assert.IsType<InvalidOperationException>(threadError);

            nint otherWindow = CreateTestWindow();
            try
            {
                using OcctViewer other = OcctViewer.Create(otherWindow);
                Assert.Throws<ArgumentException>(() => other.SetDimensionVisible(length, true));
            }
            finally { Assert.True(NativeWindowMethods.DestroyWindow(otherWindow)); }

            diameter.Dispose();
            Assert.Throws<ObjectDisposedException>(() => diameter.Show());
            presentation.Dispose();
        }
        finally
        {
            Assert.True(NativeWindowMethods.DestroyWindow(window));
            Directory.Delete(directory, recursive: true);
        }
    }

    private static nint CreateTestWindow()
    {
        nint window = NativeWindowMethods.CreateWindowEx(
            0, "STATIC", "OcctSharp Batch E viewer test", 0x80000000u,
            -32000, -32000, 320, 320, 0, 0, 0, 0);
        Assert.NotEqual(0, window);
        _ = NativeWindowMethods.ShowWindow(window, 4);
        _ = NativeWindowMethods.UpdateWindow(window);
        return window;
    }

    private static class NativeWindowMethods
    {
        [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode)]
        internal static extern nint CreateWindowEx(
            uint extendedStyle, string className, string windowName, uint style,
            int x, int y, int width, int height,
            nint parent, nint menu, nint instance, nint parameter);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowWindow(nint window, int command);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UpdateWindow(nint window);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DestroyWindow(nint window);
    }
}
