﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Domain.Exceptions;
using Domain.Models;
using Domain.Services;
using Infra.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;
        private readonly ProductsContext _productContext;
        private readonly ProductValidator _productValidator;

        public ProductsController(
            ILogger<ProductsController> logger
          , ProductsContext productContext
          , ProductValidator productValidator)
        {
            _logger = logger;
            _productContext = productContext;
            _productValidator = productValidator;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var products = _productContext.Products;

            return Ok(products);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Product product)
        {
            var errors = _productValidator.Validate(product);

            if (errors.Any()) return BadRequest(errors);

            await _productContext.AddAsync(product);

            await _productContext.SaveChangesAsync();

            var factory = new ConnectionFactory()
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_URL"),
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare("product:created", "fanout", false, false, null);

                string message = JsonSerializer.Serialize(product);

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish("product:created", "", null, body);
            }

            return Ok(product);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Product product)
        {
            var existentProduct = _productContext.Products.Find(product.Id);

            existentProduct.Title = product.Title;
            existentProduct.Price = product.Price;
            existentProduct.Description = product.Description;

            var errors = _productValidator.Validate(existentProduct);

            if (errors.Any()) return BadRequest(errors);

            _productContext.Update(existentProduct);

            await _productContext.SaveChangesAsync();

            var factory = new ConnectionFactory()
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_URL"),
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare("product:updated", "fanout", false, false, null);

                string message = JsonSerializer.Serialize(existentProduct);

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish("product:updated", "", null, body);
            }

            return Ok(product);
        }
    }
}
