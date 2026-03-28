using FluentAssertions;
using FluentValidation;
using FluentValidation.TestHelper;
using Moq;
using RetailNexus.Api.Validators;
using RetailNexus.Application.Interfaces;
using RetailNexus.Controllers;
using RetailNexus.Domain.Entities;

namespace RetailNexus.Tests.Validators;

public class CreateStoreValidatorTests
{
    private readonly Mock<IStoreRepository> _storeRepoMock = new();
    private readonly Mock<IAreaRepository> _areaRepoMock = new();
    private readonly Mock<IStoreTypeRepository> _storeTypeRepoMock = new();
    private readonly CreateStoreRequestValidator _validator;
    private readonly Guid _areaId = Guid.NewGuid();
    private readonly Guid _storeTypeId = Guid.NewGuid();

    public CreateStoreValidatorTests()
    {
        _storeRepoMock
            .Setup(r => r.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Store?)null);

        var activeArea = new Area("01", "関東", 1, true, Guid.NewGuid());
        typeof(Area).GetProperty("AreaId")!.SetValue(activeArea, _areaId);
        _areaRepoMock
            .Setup(r => r.GetByIdAsync(_areaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeArea);

        var activeStoreType = new StoreType("01", "直営", 1, true, Guid.NewGuid());
        typeof(StoreType).GetProperty("StoreTypeId")!.SetValue(activeStoreType, _storeTypeId);
        _storeTypeRepoMock
            .Setup(r => r.GetByIdAsync(_storeTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeStoreType);

        _validator = new CreateStoreRequestValidator(_storeRepoMock.Object, _areaRepoMock.Object, _storeTypeRepoMock.Object);
    }

    private StoresController.CreateStoreRequest ValidRequest()
        => new("渋谷店", _areaId, _storeTypeId);

    [Fact]
    public async Task ValidRequest_ShouldPass()
    {
        var result = await _validator.TestValidateAsync(ValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task StoreName_WhenEmpty_ShouldFail(string? name)
    {
        var request = new StoresController.CreateStoreRequest(name!, _areaId, _storeTypeId);

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.StoreName)
            .WithErrorMessage("店舗名は必須です。");
    }

    [Fact]
    public async Task AreaId_WhenAreaNotFound_ShouldFail()
    {
        var nonExistentAreaId = Guid.NewGuid();
        _areaRepoMock
            .Setup(r => r.GetByIdAsync(nonExistentAreaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Area?)null);

        var request = new StoresController.CreateStoreRequest("渋谷店", nonExistentAreaId, _storeTypeId);

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.AreaId)
            .WithErrorMessage("指定されたエリアが存在しないか、無効になっています。");
    }

    [Fact]
    public async Task AreaId_WhenAreaInactive_ShouldFail()
    {
        var inactiveAreaId = Guid.NewGuid();
        var inactiveArea = new Area("99", "無効エリア", 1, false, Guid.NewGuid());
        typeof(Area).GetProperty("AreaId")!.SetValue(inactiveArea, inactiveAreaId);
        _areaRepoMock
            .Setup(r => r.GetByIdAsync(inactiveAreaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inactiveArea);

        var request = new StoresController.CreateStoreRequest("渋谷店", inactiveAreaId, _storeTypeId);

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.AreaId)
            .WithErrorMessage("指定されたエリアが存在しないか、無効になっています。");
    }

    [Fact]
    public async Task StoreTypeId_WhenStoreTypeNotFound_ShouldFail()
    {
        var nonExistentId = Guid.NewGuid();
        _storeTypeRepoMock
            .Setup(r => r.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreType?)null);

        var request = new StoresController.CreateStoreRequest("渋谷店", _areaId, nonExistentId);

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.StoreTypeId)
            .WithErrorMessage("指定された店舗種別が存在しないか、無効になっています。");
    }
}

public class UpdateStoreValidatorTests
{
    private readonly Mock<IStoreRepository> _storeRepoMock = new();
    private readonly Mock<IAreaRepository> _areaRepoMock = new();
    private readonly Mock<IStoreTypeRepository> _storeTypeRepoMock = new();
    private readonly UpdateStoreRequestValidator _validator;
    private readonly Guid _entityId = Guid.NewGuid();
    private readonly Guid _areaId = Guid.NewGuid();
    private readonly Guid _storeTypeId = Guid.NewGuid();

    public UpdateStoreValidatorTests()
    {
        _storeRepoMock
            .Setup(r => r.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Store?)null);

        var area = new Area("01", "関東", 1, true, Guid.NewGuid());
        typeof(Area).GetProperty("AreaId")!.SetValue(area, _areaId);
        _areaRepoMock
            .Setup(r => r.GetByIdAsync(_areaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(area);

        var storeType = new StoreType("01", "直営", 1, true, Guid.NewGuid());
        typeof(StoreType).GetProperty("StoreTypeId")!.SetValue(storeType, _storeTypeId);
        _storeTypeRepoMock
            .Setup(r => r.GetByIdAsync(_storeTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storeType);

        _validator = new UpdateStoreRequestValidator(_storeRepoMock.Object, _areaRepoMock.Object, _storeTypeRepoMock.Object);
    }

    private ValidationContext<StoresController.UpdateStoreRequest> CreateContext(StoresController.UpdateStoreRequest request)
    {
        var ctx = new ValidationContext<StoresController.UpdateStoreRequest>(request);
        ctx.RootContextData["EntityId"] = _entityId;
        return ctx;
    }

    [Fact]
    public async Task ValidRequest_ShouldPass()
    {
        var request = new StoresController.UpdateStoreRequest("渋谷店", _areaId, _storeTypeId);

        var result = await _validator.TestValidateAsync(CreateContext(request));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task AreaId_WhenAreaNotFound_ShouldFail()
    {
        var nonExistentAreaId = Guid.NewGuid();
        _areaRepoMock
            .Setup(r => r.GetByIdAsync(nonExistentAreaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Area?)null);

        var request = new StoresController.UpdateStoreRequest("渋谷店", nonExistentAreaId, _storeTypeId);

        var result = await _validator.TestValidateAsync(CreateContext(request));

        result.ShouldHaveValidationErrorFor(x => x.AreaId)
            .WithErrorMessage("指定されたエリアが存在しません。");
    }
}
