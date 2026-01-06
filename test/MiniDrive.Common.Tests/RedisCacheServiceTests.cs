using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Options;
using MiniDrive.Common.Caching;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace MiniDrive.Common.Tests;

public class RedisCacheServiceTests
{
    [Fact]
    public async Task GetAsync_ReturnsDefault_WhenValueMissing()
    {
        var databaseMock = new Mock<IDatabase>();
        databaseMock
            .Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        var cache = CreateService(databaseMock);

        var result = await cache.GetAsync<string>("missing-key");

        result.Should().BeNull();
        databaseMock.Verify(
            db => db.StringGetAsync(
                It.Is<RedisKey>(k => k.ToString() == "minidrive:missing-key"),
                CommandFlags.None),
            Times.Once);
    }

    [Fact]
    public async Task GetAsync_ReturnsRawString_WhenTypeIsString()
    {
        var databaseMock = new Mock<IDatabase>();
        databaseMock
            .Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue("plain-text-value"));

        var cache = CreateService(databaseMock);

        var result = await cache.GetAsync<string>("plain-key");

        result.Should().Be("plain-text-value");
        databaseMock.Verify(
            db => db.StringGetAsync(
                It.Is<RedisKey>(k => k.ToString() == "minidrive:plain-key"),
                CommandFlags.None),
            Times.Once);
    }

    [Fact]
    public async Task SetAsync_UsesDefaultTtlAndPrefix()
    {
        var databaseMock = new Mock<IDatabase>();
        databaseMock
            .Setup(db => db.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        var options = new RedisCacheOptions
        {
            KeyPrefix = "custom:",
            DefaultTtl = TimeSpan.FromMinutes(5)
        };

        var cache = CreateService(databaseMock, options);

        await cache.SetAsync("user:42", new { Name = "Alice" });

        databaseMock.Verify(
            db => db.StringSetAsync(
                It.Is<RedisKey>(k => k.ToString() == "custom:user:42"),
                It.Is<RedisValue>(v =>
                    v.ToString().Contains("\"Alice\"", StringComparison.OrdinalIgnoreCase)),
                options.DefaultTtl,
                false,
                When.Always,
                CommandFlags.None),
            Times.Once);
    }

    [Fact]
    public async Task GetOrCreateAsync_ReturnsCachedValue_WithoutInvokingFactory()
    {
        var databaseMock = new Mock<IDatabase>();
        databaseMock
            .Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue("cached-value"));

        var cache = CreateService(databaseMock);
        var factoryInvoked = false;

        var result = await cache.GetOrCreateAsync(
            "resource",
            _ =>
            {
                factoryInvoked = true;
                return Task.FromResult("fresh-value");
            });

        result.Should().Be("cached-value");
        factoryInvoked.Should().BeFalse();
        databaseMock.Verify(
            db => db.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()),
            Times.Never);
    }

    [Fact]
    public async Task GetOrCreateAsync_CreatesAndCachesValue_WhenMissing()
    {
        var databaseMock = new Mock<IDatabase>();
        databaseMock
            .Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        var cache = CreateService(databaseMock);
        var ttl = TimeSpan.FromSeconds(30);

        var result = await cache.GetOrCreateAsync(
            "resource",
            _ => Task.FromResult("generated"),
            ttl);

        result.Should().Be("generated");
        databaseMock.Verify(
            db => db.StringSetAsync(
                It.Is<RedisKey>(k => k.ToString() == "minidrive:resource"),
                It.Is<RedisValue>(v => v.ToString() == "generated"),
                ttl,
                false,
                When.Always,
                CommandFlags.None),
            Times.Once);
    }

    [Fact]
    public async Task SetAsync_WithWhitespaceKey_ThrowsArgumentException()
    {
        var cache = CreateService(new Mock<IDatabase>());

        var action = async () => await cache.SetAsync("   ", "value");

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Cache key cannot be null or whitespace.*");
    }

    private static RedisCacheService CreateService(
        Mock<IDatabase> databaseMock,
        RedisCacheOptions? options = null,
        JsonSerializerOptions? serializerOptions = null)
    {
        var connectionMock = new Mock<IConnectionMultiplexer>();
        connectionMock
            .Setup(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<object?>()))
            .Returns(databaseMock.Object);

        return new RedisCacheService(
            connectionMock.Object,
            Options.Create(options ?? new RedisCacheOptions()),
            serializerOptions);
    }
}

