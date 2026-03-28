using FluentAssertions;
using FluentValidation;
using FluentValidation.TestHelper;
using Moq;
using RetailNexus.Api.Validators;
using RetailNexus.Application.Interfaces;
using RetailNexus.Controllers;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Tests.Validators;

public class CreateAreaValidatorTests
{
    private readonly Mock<IAreaRepository> _repoMock = new();
    private readonly CreateAreaRequestValidator _validator;

    public CreateAreaValidatorTests()
    {
        _repoMock
            .Setup(r => r.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Area?)null);

        _validator = new CreateAreaRequestValidator(_repoMock.Object);
    }

    [Fact]
    public async Task ValidRequest_ShouldPass()
    {
        var request = new AreasController.CreateAreaRequest("01", "関東");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task AreaCd_WhenEmpty_ShouldFail(string? code)
    {
        var request = new AreasController.CreateAreaRequest(code!, "関東");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.AreaCd)
            .WithErrorMessage("エリアコードは必須です。");
    }

    [Fact]
    public async Task AreaCd_WhenTooLong_ShouldFail()
    {
        var request = new AreasController.CreateAreaRequest("123", "関東");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.AreaCd)
            .WithErrorMessage("エリアコードは2文字以内で入力してください。");
    }

    [Fact]
    public async Task AreaCd_WhenDuplicate_ShouldFail()
    {
        var existingArea = new Area("01", "既存エリア", 1, true, Guid.NewGuid());
        _repoMock
            .Setup(r => r.GetByCodeAsync("01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingArea);

        var request = new AreasController.CreateAreaRequest("01", "関東");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.AreaCd)
            .WithErrorMessage("このエリアコードは既に使用されています。");
    }

    [Fact]
    public async Task AreaCd_WhenDuplicate_ShouldStopAtFirstError()
    {
        // CascadeMode.Stop により、必須チェックで止まり重複チェックは実行されない
        var request = new AreasController.CreateAreaRequest("", "関東");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.AreaCd);
        result.Errors.Where(e => e.PropertyName == "AreaCd").Should().HaveCount(1);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task AreaName_WhenEmpty_ShouldFail(string? name)
    {
        var request = new AreasController.CreateAreaRequest("01", name!);

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.AreaName)
            .WithErrorMessage("エリア名は必須です。");
    }

    [Fact]
    public async Task AreaName_WhenTooLong_ShouldFail()
    {
        var request = new AreasController.CreateAreaRequest("01", new string('あ', 21));

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.AreaName)
            .WithErrorMessage("エリア名は20文字以内で入力してください。");
    }

    [Fact]
    public async Task AreaName_WhenExactlyMaxLength_ShouldPass()
    {
        var request = new AreasController.CreateAreaRequest("01", new string('あ', 20));

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.AreaName);
    }
}

public class UpdateAreaValidatorTests
{
    private readonly Mock<IAreaRepository> _repoMock = new();
    private readonly UpdateAreaRequestValidator _validator;
    private readonly Guid _entityId = Guid.NewGuid();

    public UpdateAreaValidatorTests()
    {
        _repoMock
            .Setup(r => r.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Area?)null);

        _validator = new UpdateAreaRequestValidator(_repoMock.Object);
    }

    private ValidationContext<AreasController.UpdateAreaRequest> CreateContext(AreasController.UpdateAreaRequest request)
    {
        var ctx = new ValidationContext<AreasController.UpdateAreaRequest>(request);
        ctx.RootContextData["EntityId"] = _entityId;
        return ctx;
    }

    [Fact]
    public async Task ValidRequest_ShouldPass()
    {
        var request = new AreasController.UpdateAreaRequest("01", "関東");

        var result = await _validator.TestValidateAsync(CreateContext(request));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task AreaCd_WhenSameEntityHasSameCode_ShouldPass()
    {
        // 自分自身のコードと同じ場合はOK
        var existingArea = new Area("01", "関東", 1, true, Guid.NewGuid());
        // Areaのprivate setを使っているので、リフレクションでAreaIdを設定
        typeof(Area).GetProperty("AreaId")!.SetValue(existingArea, _entityId);

        _repoMock
            .Setup(r => r.GetByCodeAsync("01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingArea);

        var request = new AreasController.UpdateAreaRequest("01", "関東更新");

        var result = await _validator.TestValidateAsync(CreateContext(request));

        result.ShouldNotHaveValidationErrorFor(x => x.AreaCd);
    }

    [Fact]
    public async Task AreaCd_WhenDifferentEntityHasSameCode_ShouldFail()
    {
        var otherArea = new Area("01", "他エリア", 1, true, Guid.NewGuid());

        _repoMock
            .Setup(r => r.GetByCodeAsync("01", It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherArea);

        var request = new AreasController.UpdateAreaRequest("01", "関東");

        var result = await _validator.TestValidateAsync(CreateContext(request));

        result.ShouldHaveValidationErrorFor(x => x.AreaCd)
            .WithErrorMessage("このエリアコードは既に使用されています。");
    }
}

public class ReorderAreasValidatorTests
{
    private readonly Mock<IAreaRepository> _repoMock = new();
    private readonly ReorderAreasRequestValidator _validator;

    public ReorderAreasValidatorTests()
    {
        _validator = new ReorderAreasRequestValidator(_repoMock.Object);
    }

    [Fact]
    public async Task ValidRequest_ShouldPass()
    {
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        _repoMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ids.Select(id =>
            {
                var area = new Area("01", "テスト", 1, true, Guid.NewGuid());
                typeof(Area).GetProperty("AreaId")!.SetValue(area, id);
                return area;
            }).ToList());

        var request = new AreasController.ReorderAreasRequest(ids);

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task WhenEmpty_ShouldFail()
    {
        var request = new AreasController.ReorderAreasRequest(new List<Guid>());

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.AreaIds)
            .WithErrorMessage("エリアIDリストは必須です。");
    }

    [Fact]
    public async Task WhenDuplicateIds_ShouldFail()
    {
        var id = Guid.NewGuid();
        var request = new AreasController.ReorderAreasRequest(new List<Guid> { id, id });

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.AreaIds)
            .WithErrorMessage("エリアIDリストに重複があります。");
    }

    [Fact]
    public async Task WhenNonExistentIds_ShouldFail()
    {
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        _repoMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Area>());

        var request = new AreasController.ReorderAreasRequest(ids);

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.AreaIds)
            .WithErrorMessage("存在しないエリアIDが含まれています。");
    }
}
