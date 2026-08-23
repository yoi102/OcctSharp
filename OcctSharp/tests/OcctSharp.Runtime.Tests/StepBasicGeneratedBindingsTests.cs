namespace OcctSharp.Runtime.Tests;

public sealed class StepBasicGeneratedBindingsTests
{
    [Fact]
    public void EveryGeneratedStepBasicTypeConstructsClonesAndReleases()
    {
        Type[] generatedTypes = typeof(StepBasicDate).Assembly.GetExportedTypes()
            .Where(static type => type.IsClass
                && type.Name.StartsWith("StepBasic", StringComparison.Ordinal)
                && typeof(IDisposable).IsAssignableFrom(type)
                && type.GetConstructor(Type.EmptyTypes) is not null)
            .OrderBy(static type => type.FullName, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(129, generatedTypes.Length);
        foreach (Type type in generatedTypes)
        {
            IDisposable instance = Assert.IsAssignableFrom<IDisposable>(Activator.CreateInstance(type));
            IDisposable? clone = null;
            try
            {
                Assert.Equal(1, Assert.IsType<int>(type.GetProperty("ReferenceCount")!.GetValue(instance)));
                Assert.False(string.IsNullOrWhiteSpace(Assert.IsType<string>(
                    type.GetProperty("TypeName")!.GetValue(instance))));
                Assert.True(Assert.IsType<bool>(type.GetMethod("IsKind")!.Invoke(
                    instance,
                    ["Standard_Transient"])));
                clone = Assert.IsAssignableFrom<IDisposable>(type.GetMethod("Clone")!.Invoke(instance, null));
                Assert.Equal(2, Assert.IsType<int>(type.GetProperty("ReferenceCount")!.GetValue(instance)));
            }
            finally
            {
                instance.Dispose();
                clone?.Dispose();
            }
        }
    }

    [Fact]
    public void ExpandedStepBasicScalarsAndEnumsRoundTrip()
    {
        using StepBasicApplicationProtocolDefinition protocol = new();
        protocol.SetApplicationProtocolYear(2026);
        Assert.Equal(2026, protocol.ApplicationProtocolYear());

        using StepBasicDerivedUnitElement element = new();
        element.SetExponent(-2.5);
        Assert.Equal(-2.5, element.Exponent(), 12);

        using StepBasicProductDefinitionFormationWithSpecifiedSource source = new();
        source.SetMakeOrBuy(StepBasicSource.StepBasic_sNotKnown);
        Assert.Equal(StepBasicSource.StepBasic_sNotKnown, source.MakeOrBuy());

        using StepBasicAction action = new();
        Assert.False(action.HasDescription());
        using StepBasicProductCategory category = new();
        Assert.False(category.HasDescription());
        category.UnSetDescription();
        using StepBasicOrganization organization = new();
        Assert.False(organization.HasId());
        organization.UnSetId();
    }

    [Fact]
    public void DateEntitiesRoundTripScalarState()
    {
        using StepBasicDate date = new();
        date.Init(2026);
        Assert.Equal(2026, date.YearComponent());
        date.SetYearComponent(2027);
        Assert.Equal(2027, date.YearComponent());

        using StepBasicCalendarDate calendar = new();
        calendar.Init(2026, 23, 8);
        Assert.Equal(23, calendar.DayComponent());
        Assert.Equal(8, calendar.MonthComponent());
        calendar.SetDayComponent(24);
        calendar.SetMonthComponent(9);
        Assert.Equal(24, calendar.DayComponent());
        Assert.Equal(9, calendar.MonthComponent());

        using StepBasicOrdinalDate ordinal = new();
        ordinal.Init(2026, 235);
        Assert.Equal(235, ordinal.DayComponent());
        ordinal.SetDayComponent(236);
        Assert.Equal(236, ordinal.DayComponent());

        using StepBasicWeekOfYearAndDayDate week = new();
        week.Init(2026, 34, true, 1);
        Assert.Equal(34, week.WeekComponent());
        Assert.True(week.HasDayComponent());
        Assert.Equal(1, week.DayComponent());
        week.UnSetDayComponent();
        Assert.False(week.HasDayComponent());
    }

    [Fact]
    public void TimeAndUnitEntitiesRoundTripBooleanAndEnumState()
    {
        using StepBasicCoordinatedUniversalTimeOffset offset = new();
        offset.Init(9, true, 30, StepBasicAheadOrBehind.StepBasic_aobAhead);
        Assert.Equal(9, offset.HourOffset());
        Assert.True(offset.HasMinuteOffset());
        Assert.Equal(30, offset.MinuteOffset());
        Assert.Equal(StepBasicAheadOrBehind.StepBasic_aobAhead, offset.Sense());
        offset.SetSense(StepBasicAheadOrBehind.StepBasic_aobBehind);
        offset.UnSetMinuteOffset();
        Assert.Equal(StepBasicAheadOrBehind.StepBasic_aobBehind, offset.Sense());
        Assert.False(offset.HasMinuteOffset());

        using StepBasicLocalTime time = new();
        time.SetHourComponent(12);
        time.SetMinuteComponent(45);
        time.SetSecondComponent(12.5);
        Assert.Equal(12, time.HourComponent());
        Assert.True(time.HasMinuteComponent());
        Assert.Equal(45, time.MinuteComponent());
        Assert.True(time.HasSecondComponent());
        Assert.Equal(12.5, time.SecondComponent(), 12);
        time.UnSetMinuteComponent();
        time.UnSetSecondComponent();
        Assert.False(time.HasMinuteComponent());
        Assert.False(time.HasSecondComponent());

        using StepBasicSiUnit unit = new();
        unit.Init(true, StepBasicSiPrefix.StepBasic_spMilli, StepBasicSiUnitName.StepBasic_sunMetre);
        Assert.True(unit.HasPrefix());
        Assert.Equal(StepBasicSiPrefix.StepBasic_spMilli, unit.Prefix());
        Assert.Equal(StepBasicSiUnitName.StepBasic_sunMetre, unit.Name());
        unit.SetPrefix(StepBasicSiPrefix.StepBasic_spKilo);
        unit.SetName(StepBasicSiUnitName.StepBasic_sunGram);
        Assert.Equal(StepBasicSiPrefix.StepBasic_spKilo, unit.Prefix());
        Assert.Equal(StepBasicSiUnitName.StepBasic_sunGram, unit.Name());
        unit.UnSetPrefix();
        Assert.False(unit.HasPrefix());
    }

    [Fact]
    public void DimensionalExponentsRoundTripAllScalarsThroughSharedClone()
    {
        using StepBasicDimensionalExponents exponents = new();
        exponents.Init(1, 2, 3, 4, 5, 6, 7);
        using StepBasicDimensionalExponents clone = exponents.Clone();

        Assert.Equal(2, exponents.ReferenceCount);
        Assert.Equal(2, clone.ReferenceCount);
        Assert.Equal(1, clone.LengthExponent(), 12);
        Assert.Equal(2, clone.MassExponent(), 12);
        Assert.Equal(3, clone.TimeExponent(), 12);
        Assert.Equal(4, clone.ElectricCurrentExponent(), 12);
        Assert.Equal(5, clone.ThermodynamicTemperatureExponent(), 12);
        Assert.Equal(6, clone.AmountOfSubstanceExponent(), 12);
        Assert.Equal(7, clone.LuminousIntensityExponent(), 12);

        clone.SetLengthExponent(8);
        Assert.Equal(8, exponents.LengthExponent(), 12);
        Assert.Equal("StepBasic_DimensionalExponents", exponents.TypeName);
        Assert.True(exponents.IsKind("Standard_Transient"));
    }

    [Fact]
    public void OptionalAddressAndPersonFieldsStartUnsetAndCanBeUnsetAgain()
    {
        using StepBasicAddress address = new();
        Assert.False(address.HasCountry());
        Assert.False(address.HasElectronicMailAddress());
        Assert.False(address.HasFacsimileNumber());
        Assert.False(address.HasInternalLocation());
        Assert.False(address.HasPostalBox());
        Assert.False(address.HasPostalCode());
        Assert.False(address.HasRegion());
        Assert.False(address.HasStreet());
        Assert.False(address.HasStreetNumber());
        Assert.False(address.HasTelephoneNumber());
        Assert.False(address.HasTelexNumber());
        Assert.False(address.HasTown());
        address.UnSetCountry();
        address.UnSetStreet();

        using StepBasicPerson person = new();
        Assert.False(person.HasFirstName());
        Assert.False(person.HasLastName());
        Assert.False(person.HasMiddleNames());
        Assert.False(person.HasPrefixTitles());
        Assert.False(person.HasSuffixTitles());
        Assert.Equal(0, person.NbMiddleNames());
        Assert.Equal(0, person.NbPrefixTitles());
        Assert.Equal(0, person.NbSuffixTitles());
        person.UnSetFirstName();
        person.UnSetSuffixTitles();
    }

    [Fact]
    public void SharedEntityDisposeIsIdempotentAndRejectsFurtherUse()
    {
        StepBasicDate date = new();
        date.Init(2026);
        StepBasicDate retained = date.Clone();
        Assert.Equal(2, date.ReferenceCount);

        date.Dispose();
        date.Dispose();
        Assert.Throws<ObjectDisposedException>(() => date.YearComponent());
        Assert.Equal(1, retained.ReferenceCount);
        Assert.Equal(2026, retained.YearComponent());
        retained.Dispose();
        Assert.Throws<ObjectDisposedException>(() => retained.Clone());
    }
}
