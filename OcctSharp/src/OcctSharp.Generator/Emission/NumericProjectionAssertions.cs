using System.Text;

namespace OcctSharp.Generator.Emission;

internal static class NumericProjectionAssertions
{
    internal static void AppendTo(StringBuilder builder)
    {
        builder.AppendLine("#include <climits>");
        builder.AppendLine("#include <cstdint>");
        builder.AppendLine("#include <limits>");
        builder.AppendLine("static_assert(CHAR_BIT == 8 && sizeof(short) == 2 && sizeof(int) == 4);");
        builder.AppendLine("static_assert(sizeof(long) == 4 && sizeof(long long) == 8 && sizeof(size_t) == 8);");
        builder.AppendLine("static_assert(sizeof(float) == 4 && std::numeric_limits<float>::is_iec559);");
        builder.AppendLine("static_assert(sizeof(double) == 8 && std::numeric_limits<double>::is_iec559);");
    }
}
