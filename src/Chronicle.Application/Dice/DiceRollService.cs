using Chronicle.Contracts;

namespace Chronicle.Application;

public static class DiceRollService
{
    public static DiceRollResult Execute(DiceRollRequest request, IDiceValueGenerator generator)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(generator);

        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            return new DiceRollResult(
                request.RequestId,
                false,
                [],
                DiceRollFailureCode.InvalidQuantity,
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        if (request.Quantity < 0)
        {
            return new DiceRollResult(
                request.RequestId,
                false,
                [],
                DiceRollFailureCode.InvalidQuantity,
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        if (!generator.SupportsSize(request.DiceSize))
        {
            return new DiceRollResult(
                request.RequestId,
                false,
                [],
                DiceRollFailureCode.InvalidFaces,
                new Dictionary<string, string>(StringComparer.Ordinal));
        }

        try
        {
            var diceValues = generator.Generate(request.Quantity, request.DiceSize);
            return new DiceRollResult(
                request.RequestId,
                true,
                diceValues,
                null,
                request.Metadata);
        }
        catch (InvalidOperationException ex)
        {
            return new DiceRollResult(
                request.RequestId,
                false,
                [],
                DiceRollFailureCode.GeneratorUnavailable,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["error"] = ex.Message
                });
        }
    }
}
