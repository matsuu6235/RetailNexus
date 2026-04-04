using Microsoft.Extensions.Localization;
using Moq;
using RetailNexus.Resources;

namespace RetailNexus.Tests.Helpers;

/// <summary>
/// SharedMessages の resx リソースキーに対応する日本語メッセージを返す
/// IStringLocalizer モックを生成するヘルパー。
/// </summary>
public static class MockLocalizerHelper
{
    private static readonly Dictionary<string, string> Messages = new()
    {
        ["Validation_Required"] = "{0}は必須です。",
        ["Validation_MaxLength"] = "{0}は{1}文字以内で入力してください。",
        ["Validation_MinLength"] = "{0}は{1}文字以上で入力してください。",
        ["Validation_ExactLength"] = "{0}は{1}桁で入力してください。",
        ["Validation_DigitsOnly"] = "{0}は数字のみ入力できます。",
        ["Validation_Duplicate"] = "この{0}は既に使用されています。",
        ["Validation_MinValue"] = "{0}は{1}以上で入力してください。",
        ["Validation_GreaterThan"] = "{0}は{1}以上で入力してください。",
        ["Validation_EmailFormat"] = "メールアドレスの形式が正しくありません。",
        ["Validation_PhoneFormat"] = "{0}は数字とハイフン（-）のみ入力できます。",
        ["Validation_AlphaRange"] = "{0}は英字{1}〜{2}文字で入力してください。",
        ["Validation_EntityNotFound"] = "指定された{0}が存在しません。",
        ["Validation_EntityNotFoundOrInactive"] = "指定された{0}が存在しないか、無効になっています。",
        ["Validation_ListDuplicate"] = "{0}IDリストに重複があります。",
        ["Validation_ListInvalidIds"] = "存在しない{0}IDが含まれています。",
        ["Validation_ListMinCount"] = "{0}を1行以上入力してください。",
        ["PurchaseOrder_DuplicateProduct"] = "同一商品が複数行に含まれています。数量を変更してください。",
        ["StoreRequest_SameStore"] = "依頼元と依頼先は異なる店舗を選択してください。",
        ["StoreRequest_DuplicateProduct"] = "同一商品が複数行に含まれています。数量を変更してください。",
    };

    public static IStringLocalizer<SharedMessages> Create()
    {
        var mock = new Mock<IStringLocalizer<SharedMessages>>();

        // L["key", arg0, arg1, ...] — パラメータ付き呼び出し
        mock.Setup(x => x[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string key, object[] args) =>
            {
                var template = Messages.TryGetValue(key, out var t) ? t : key;
                var formatted = string.Format(template, args);
                return new LocalizedString(key, formatted);
            });

        // L["key"] — パラメータなし呼び出し
        mock.Setup(x => x[It.IsAny<string>()])
            .Returns((string key) =>
            {
                var value = Messages.TryGetValue(key, out var t) ? t : key;
                return new LocalizedString(key, value);
            });

        return mock.Object;
    }
}
