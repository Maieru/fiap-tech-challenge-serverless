using System.Diagnostics.CodeAnalysis;

namespace FIAP.TechChallenge.Serverless.OrdemServicoAuthorizer.ValueObjects;

public sealed class Cpf
{
    public string Value { get; }

    private Cpf(string value)
    {
        Value = value;
    }

    public static bool TryCreate(string? input, [NotNullWhen(true)] out Cpf? cpf)
    {
        cpf = null;

        if (string.IsNullOrWhiteSpace(input))
            return false;

        var normalizedCpf = string.Concat(input.Where(char.IsDigit));

        if (normalizedCpf.Length != 11)
            return false;

        var firstDigitValue = normalizedCpf[0];

        if (normalizedCpf.All(digit => digit == firstDigitValue))
            return false;

        var firstDigit = CalculateVerificationDigit(normalizedCpf.AsSpan(0, 9), 10);
        var secondDigit = CalculateVerificationDigit(normalizedCpf.AsSpan(0, 10), 11);

        if (normalizedCpf[9] != firstDigit || normalizedCpf[10] != secondDigit)
            return false;

        cpf = new Cpf(normalizedCpf);
        return true;
    }

    private static char CalculateVerificationDigit(ReadOnlySpan<char> source, int initialWeight)
    {
        var sum = 0;
        var weight = initialWeight;

        foreach (var digit in source)
        {
            sum += (digit - '0') * weight;
            weight--;
        }

        var remainder = sum % 11;
        var verificationDigit = remainder < 2 ? 0 : 11 - remainder;

        return (char)('0' + verificationDigit);
    }
}
