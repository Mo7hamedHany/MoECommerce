﻿using AutoMapper;
using MoECommerce.Core.DataTransferObjects;
using MoECommerce.Core.Interfaces.Repositories;
using MoECommerce.Core.Interfaces.Services;
using MoECommerce.Core.Models.Order;
using MoECommerce.Core.Models.Product;
using MoECommerce.Repository.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoECommerce.Services
{
    public class OrderService : IOrderService
    {
        private readonly IBasketService _basketService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderService(IBasketService basketService, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _basketService = basketService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<OrderResultDto> CreateOrderAsync(OrderDto input)
        {
            var basket = await _basketService.GetBasketAsync(input.BasketId);
            if (basket == null) throw new Exception($"There's no Basket with Id: {input.BasketId}");

            var orderItems = new List<OrderItemDto>();
            foreach (var item in basket.BasketItems)
            {
                var product = await _unitOfWork.Repository<Product, int>().GetAsync(item.ProductId);
                if (product == null) continue;

                var productItem = new OrderItemProduct
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    PictureUrl = product.PictureUrl,
                };

                var orderItem = new OrderItem
                {
                    orderItemProduct = productItem,
                    Quantity = item.Quantity,
                    Price = product.Price,
                };

                var mappedItem = _mapper.Map<OrderItemDto>(orderItem);
                orderItems.Add(mappedItem);
            }
            if (!orderItems.Any()) throw new Exception("No Basket Items found");

            if (!input.DeliveryMethodId.HasValue) throw new Exception("No delivery Method selected");
            var delivery = await _unitOfWork.Repository<DeliveryMethods,int>().GetAsync(input.DeliveryMethodId.Value);
            if (delivery == null) throw new Exception("Invalid Delivery Method Id");

            var shippingAddress = _mapper.Map<ShippingAddress>(input.ShippingAddress);

            var subTotal = orderItems.Sum(item => item.Price * item.Quantity);

            var mappedItems = _mapper.Map<IEnumerable<OrderItem>>(orderItems);

            var order = new Order
            {
                BuyerEmail = input.BuyerEmail,
                ShippingAddress = shippingAddress,
                DeliveryMethod = delivery,
                OrderItems = mappedItems,
                SubTotal = subTotal
            };

            await _unitOfWork.Repository<Order,Guid>().AddAsunc(order);

            return _mapper.Map<OrderResultDto>(order);
        }

        public async Task<IEnumerable<OrderResultDto>> GetAllOrdersAsync(string email)
        {
            var spec = new OrderSpecifications(email);

            var orders = await _unitOfWork.Repository<Order, Guid>().GetAllWithSpecsAsync(spec);

            return _mapper.Map<IEnumerable<OrderResultDto>>(orders);
        }

        public async Task<OrderResultDto> GetOrderAsync(Guid id, string email)
        {
            var spec = new OrderSpecifications(id,email);

            var orders = await _unitOfWork.Repository<Order, Guid>().GetWithSpecsAsync(spec);

            return _mapper.Map<OrderResultDto>(orders);
        }

        public async Task<IEnumerable<DeliveryMethods>> GetDeliveryMethodsAsync()
            => await _unitOfWork.Repository<DeliveryMethods,int>().GetAllAsync();

    }
}
