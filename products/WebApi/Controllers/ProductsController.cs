﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Exceptions;
using Domain.Extensions;
using Domain.Models;
using Domain.Services;
using Infra.Contexts;
using Infra.Interfaces.Publishers;
using Infra.Rabbit.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Rabbit.Events;
using WebApi.ViewModel;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;
        private readonly ProductsContext _productContext;
        private readonly ProductValidator _productValidator;
        private readonly IProductUpdatedPublisher _productUpdatedPublisher;
        private readonly IProductCreatedPublisher _productCreatedPublisher;

        public ProductsController(
            ILogger<ProductsController> logger
          , ProductsContext productContext
          , ProductValidator productValidator
          , IProductUpdatedPublisher productUpdatedPublisher
          , IProductCreatedPublisher productCreatedPublisher)
        {
            _logger = logger;
            _productContext = productContext;
            _productValidator = productValidator;
            _productUpdatedPublisher = productUpdatedPublisher;
            _productCreatedPublisher = productCreatedPublisher;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var products = _productContext.Products;

            var productModels = products
                .ToList()
                .Select(product => new ProductModel(product));

            return Ok(productModels);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProductModel productModel)
        {
            Product product = productModel;

            var errors = _productValidator.Validate(product);

            if (errors.Any()) return BadRequest(errors);

            product.Id = ObjectId.GenerateNewId().AsGuid();

            await _productContext.AddAsync(product);
            await _productContext.SaveChangesAsync();
            _productCreatedPublisher.Publish(new ProductCreatedEvent
            {
                Id = product.Id.AsObjectId(),
                Title = product.Title,
                Price = product.Price,
                Description = product.Description,
                Version = product.Version
            });
            
            return Ok(new ProductModel(product));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(String id, [FromBody] ProductModel product)
        {
            Guid productId = id.AsObjectId().AsGuid();

            var errors = _productValidator.Validate(product);

            if (errors.Any()) return BadRequest(errors);

            var existentProduct = _productContext.Products.Find(productId);

            if (existentProduct == null)
            {
                return BadRequest(new List<BusinessError>
                {
                    new BusinessError("The product could not be found to be updated", "Product")
                });
            }

            existentProduct.Title = product.Title;
            existentProduct.Price = product.Price;
            existentProduct.Description = product.Description;
            existentProduct.Version++;

            _productContext.Update(existentProduct);
            await _productContext.SaveChangesAsync();

            _productUpdatedPublisher.Publish(new ProductUpdatedEvent
            {
                Id = existentProduct.Id.AsObjectId(),
                Title = existentProduct.Title,
                Price = existentProduct.Price,
                Description = existentProduct.Description,
                Version = existentProduct.Version
            });

            return Ok(new ProductModel(existentProduct));
        }
    }
}
