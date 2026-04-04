namespace RetailNexus.Application.Services;

public static class CodeGenerator
{
    /// <summary>店舗コード: 6桁の連番 (000001, 000002, ...)</summary>
    public static string NextStoreCode(string? currentMax)
    {
        var next = ParseNumeric(currentMax) + 1;
        return $"{next:D6}";
    }

    /// <summary>仕入先コード: 5桁の連番 (00001, 00002, ...)</summary>
    public static string NextSupplierCode(string? currentMax)
    {
        var next = ParseNumeric(currentMax) + 1;
        return $"{next:D5}";
    }

    /// <summary>商品コード: カテゴリ略称-6桁連番 (FD-000001, ...)</summary>
    public static string NextProductCode(string? currentMax, string categoryAbbreviation)
    {
        var next = 1;
        if (currentMax is not null)
        {
            var numericPart = currentMax[(categoryAbbreviation.Length + 1)..];
            next = int.Parse(numericPart) + 1;
        }
        return $"{categoryAbbreviation}-{next:D6}";
    }

    /// <summary>発注番号: PO-6桁連番 (PO-000001, ...)</summary>
    public static string NextOrderNumber(string? currentMax)
    {
        var next = ParsePrefixed(currentMax, "PO-") + 1;
        return $"PO-{next:D6}";
    }

    /// <summary>発送依頼番号: SR-6桁連番 (SR-000001, ...)</summary>
    public static string NextRequestNumber(string? currentMax)
    {
        var next = ParsePrefixed(currentMax, "SR-") + 1;
        return $"SR-{next:D6}";
    }

    private static int ParseNumeric(string? value)
        => value is not null ? int.Parse(value) : 0;

    private static int ParsePrefixed(string? value, string prefix)
        => value is not null ? int.Parse(value[prefix.Length..]) : 0;
}
