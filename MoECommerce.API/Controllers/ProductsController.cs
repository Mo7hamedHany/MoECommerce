﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoECommerce.Core.Interfaces.Services;
using MoECommerce.Core.SpecificationParameters;

namespace MoECommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;


        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllProducts([FromQuery]ProductSpecificationParameters parameters)
        {
            return Ok(await _productService.GetAllProductsAsync(parameters));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetProduct(int? id)
        {
            return Ok(await _productService.GetProductsAsync(id.Value));
        }

        [HttpGet("brands")]
        public async Task<ActionResult> GetAllBrands()
        {
            return Ok(await _productService.GetAllBrandsAsync());
        }

        [HttpGet("types")]
        public async Task<ActionResult> GetAllTypes()
        {
            return Ok(await _productService.GetAllTypesAsync());
        }
    }
}