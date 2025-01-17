﻿using AutoFixture;
using ECommerce.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ECommerce.Repository.UnitTests.Products;

public partial class ProductTests
{
    [Fact]
    public async Task Add_RequiredNameField_ThrowsException()
    {
        // Arrange
        var product = Fixture.Create<Product>();
        product.Name = null!;

        // Act
        async Task Action()
        {
            _productRepository.Add(product);
            await UnitOfWork.SaveAsync(CancellationToken);
        }

        // Assert
        await Assert.ThrowsAsync<DbUpdateException>(Action);
    }

    [Fact]
    public async Task Add_RequiredUrlField_ThrowsException()
    {
        // Arrange
        var product = Fixture.Create<Product>();
        product.Name = null!;

        // Act
        async Task Action()
        {
            _productRepository.Add(product);
            await UnitOfWork.SaveAsync(CancellationToken);
        }

        // Assert
        await Assert.ThrowsAsync<DbUpdateException>(Action);
    }

    [Fact]
    public async void Add_NullProduct_ThrowsException()
    {
        // Act
        async Task Action()
        {
            _productRepository.Add(null!);
            await UnitOfWork.SaveAsync(CancellationToken);
        }

        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(Action);
    }

    [Fact]
    public async void Add_AddEntity_EntityExistsInRepository()
    {
        // Arrange
        Product expected = Fixture.Create<Product>();

        // Act
        _productRepository.Add(expected);
        await UnitOfWork.SaveAsync(CancellationToken);
        var actual = DbContext.Products.FirstOrDefault(x => x.Id == expected.Id);

        // Assert
        DbContext.Products.Count().Should().Be(1);
        actual.Should().BeEquivalentTo(expected);
    }
}
