using FluentValidation;
using RetailNexus.Api.Contracts;
using RetailNexus.Application.Interfaces;
using RetailNexus.Controllers;

namespace RetailNexus.Api.Validators;

public class AreaRequestValidator<T> : AbstractValidator<T> where T : IAreaRequest
{
    protected AreaRequestValidator()
    {
        RuleFor(x => x.AreaName)
            .NotEmpty().WithMessage("エリア名は必須です。")
            .MaximumLength(20).WithMessage("エリア名は20文字以内で入力してください。");
    }

    protected IRuleBuilderOptions<T, string> AreaCdBaseRules()
    {
        return RuleFor(x => x.AreaCd)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("エリアコードは必須です。")
            .MaximumLength(2).WithMessage("エリアコードは2文字以内で入力してください。");
    }
}

public sealed class CreateAreaRequestValidator : AreaRequestValidator<AreasController.CreateAreaRequest>
{
    public CreateAreaRequestValidator(IAreaRepository repo)
    {
        AreaCdBaseRules()
            .MustAsync(async (code, ct) =>
            {
                var existing = await repo.GetByCodeAsync(code.Trim(), ct);
                return existing is null;
            }).WithMessage("このエリアコードは既に使用されています。");
    }
}

public sealed class UpdateAreaRequestValidator : AreaRequestValidator<AreasController.UpdateAreaRequest>
{
    public UpdateAreaRequestValidator(IAreaRepository repo)
    {
        AreaCdBaseRules()
            .MustAsync(async (request, code, context, ct) =>
            {
                var entityId = (Guid)context.RootContextData["EntityId"];
                var existing = await repo.GetByCodeAsync(code.Trim(), ct);
                return existing is null || existing.AreaId == entityId;
            }).WithMessage("このエリアコードは既に使用されています。");
    }
}

public sealed class ReorderAreasRequestValidator : AbstractValidator<AreasController.ReorderAreasRequest>
{
    public ReorderAreasRequestValidator(IAreaRepository repo)
    {
        RuleFor(x => x.AreaIds)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("エリアIDリストは必須です。")
            .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("エリアIDリストに重複があります。")
            .MustAsync(async (ids, ct) =>
            {
                var distinctIds = ids.Distinct().ToArray();
                var entities = await repo.GetByIdsAsync(distinctIds, ct);
                return entities.Count == distinctIds.Length;
            }).WithMessage("存在しないエリアIDが含まれています。");
    }
}
